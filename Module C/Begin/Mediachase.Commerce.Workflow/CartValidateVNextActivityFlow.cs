using EPiServer.ServiceLocation;
using Mediachase.Commerce.Engine;
using Mediachase.Commerce.Inventory;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Workflow.Activities;
using Mediachase.Commerce.Workflow.Activities.Cart;
using Mediachase.Commerce.Workflow.Activities.OrderGroupActivities;
using Mediachase.Commerce.Workflow.Activities.PurchaseOrderActivities;
using Mediachase.Commerce.WorkflowCompatibility;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Mediachase.Commerce.Workflow
{
    /// <summary>
    /// NOTE: This is a pre-release API that is UNSTABLE and might not satisfy the compatibility requirements as denoted by its associated normal version.
    /// </summary>
    [ActivityFlowConfiguration(Name = OrderGroupWorkflowManager.CartValidateWorkflowName, AvailableInBetaMode = true)]
    public class CartValidateVNextActivityFlow : ActivityFlow
    {
        /// <inheritdoc />
        public override ActivityFlowRunner Configure(ActivityFlowRunner activityFlow)
        {
            return activityFlow.Do<ValidateLineItemsActivity>()
                           .Do<GetFulfillmentWarehouseActivity>()
                           .If(() => ShouldCheckInstoreInventory())
                                .Do<CheckInstoreInventoryActivity>()
                            .Else()
                                .Do<CheckInventoryActivity>()
                            .EndIf()
                            .Do<RemoveDiscountsVNextActivity>()
                            .Do<CalculateDiscountsVNextActivity>()
                            .Do<UpdateTotalsVNextActivity>();
        }
    }
}
