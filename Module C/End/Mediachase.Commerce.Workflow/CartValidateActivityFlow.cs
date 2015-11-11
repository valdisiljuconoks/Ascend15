using System;
using Mediachase.Commerce.Engine;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Workflow.Activities;
using Mediachase.Commerce.Workflow.Activities.Cart;
using Mediachase.Commerce.Workflow.Activities.CartActivities;
using Mediachase.Commerce.Workflow.Activities.PurchaseOrderActivities;

namespace Mediachase.Commerce.Workflow
{
    /// <summary>
    ///     This class represents the Cart Validate workflow
    /// </summary>
    [ActivityFlowConfiguration(Name = OrderGroupWorkflowManager.CartValidateWorkflowName)]
    public class CartValidateActivityFlow : ActivityFlow
    {
        /// <inheritdoc />
        public override ActivityFlowRunner Configure(ActivityFlowRunner activityFlow)
        {
            return activityFlow.Do<ValidateLineItemsActivity>()
                               .Do<GetFulfillmentWarehouseActivity>()
                               .Do<SampleEventCanonActivity>()
                               .Do<MutateContextPropertyActivity>()
                               .On("SampleEvent", HandleSampleEvent)
                               .If(ShouldCheckInstoreInventory)
                               .Do<CheckInstoreInventoryActivity>()
                               .Else()
                               .Do<CheckInventoryActivity>()
                               .EndIf()
                               .Do<CalculateTotalsActivity>()
                               .Do<RemoveDiscountsActivity>()
                               .Do<CalculateTotalsActivity>()
                               .Do<CalculateDiscountsActivity>()
                               .Do<CalculateTotalsActivity>()
                               .Do<RecordPromotionUsageActivity>()
                               .Do<WatchMutablePropertyActivity>();
        }

        private void HandleSampleEvent(object sender, EventArgs e)
        {
        }
    }
}
