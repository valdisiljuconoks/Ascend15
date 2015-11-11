using System;
using Mediachase.Commerce.Engine;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Workflow.Activities;
using Mediachase.Commerce.Workflow.Activities.Cart;
using Mediachase.Commerce.WorkflowCompatibility;

namespace Mediachase.Commerce.Workflow
{
    /// <summary>
    ///     This class represents the Cart Checkout workflow
    /// </summary>
    [ActivityFlowConfiguration(Name = OrderGroupWorkflowManager.CartCheckOutWorkflowName)]
    public class CartCheckoutActivityFlow : ActivityFlow
    {
        /// <inheritdoc />
        public override ActivityFlowRunner Configure(ActivityFlowRunner activityFlow)
        {
            return activityFlow
                .Do<SampleFireCanonActivity>()
                .On("SampleActivityEvent", Notify)
                .If(ShouldProcessPayment)
                    .Do<ProcessPaymentActivity>()
                .EndIf()
                .Do<CalculateTotalsActivity>()
                .Do<AdjustInventoryActivity>()
                .Do<RecordPromotionUsageActivity>();
        }

        private void Notify(object sender, EventArgs e) { }
    }

    public class SampleFireCanonActivity : CartActivityBase
    {
        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            RaiseEvent("SampleActivityEvent", new EventArgs());
            return ActivityExecutionStatus.Closed;
        }
    }
}
