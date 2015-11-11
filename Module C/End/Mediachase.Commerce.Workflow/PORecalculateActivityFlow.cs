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
    /// This class represents the Purchase Order Recalculate workflow
    /// </summary>
    [ActivityFlowConfiguration(Name = OrderGroupWorkflowManager.OrderRecalculateWorkflowName)]
    public class PORecalculateActivityFlow : ActivityFlow
    {
        /// <inheritdoc />
        public override ActivityFlowRunner Configure(ActivityFlowRunner activityFlow)
        {
            return activityFlow.Do<ValidateLineItemsActivity>()
                            .Do<GetFulfillmentWarehouseActivity>()
                            .If(() => ShouldAdjustInventory())
                                .If(() => ShouldCheckInstoreInventory())
                                    .Do<CheckInstoreInventoryActivity>()
                                .Else()
                                    .Do<CheckInventoryActivity>()
                                .EndIf()
                            .EndIf()
                            .If(() => ShouldAdjustInventory())
                                .Do<AdjustInventoryActivity>()
                            .EndIf()
                            .If(() => ShouldRecalculateOrder())
                                .Do<ProcessShipmentsActivity>()
                                .Do<RemoveDiscountsActivity>()
                                .Do<CalculateTotalsActivity>()
                                .Do<CalculateDiscountsActivity>()
                                .Do<CalculateTotalsActivity>()
                                .Do<CalculateTaxActivity>()
                                .Do<CalculateTotalsActivity>()
                            .EndIf()
                            .Do<CalculatePurchaseOrderStatusActivity>();
        }
    }
}
