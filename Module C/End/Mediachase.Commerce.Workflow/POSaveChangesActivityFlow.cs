using EPiServer.ServiceLocation;
using Mediachase.Commerce.Engine;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Workflow.Activities;
using Mediachase.Commerce.Workflow.Activities.Cart;
using Mediachase.Commerce.WorkflowCompatibility;
using System.Collections.Specialized;

namespace Mediachase.Commerce.Workflow
{
    /// <summary>
    /// This class represents the Purchase Order Save Changes workflow
    /// </summary>
    [ActivityFlowConfiguration(Name = OrderGroupWorkflowManager.OrderSaveChangesWorkflowName)]
    public class POSaveChangesActivityFlow : ActivityFlow
    {
        /// <inheritdoc />
        public override ActivityFlowRunner Configure(ActivityFlowRunner activityFlow)
        {
            return activityFlow.Do<AdjustInventoryActivity>()
                           .Do<ProcessPaymentActivity>()
                           .Do<CalculateTotalsActivity>();
        }
    }
}
