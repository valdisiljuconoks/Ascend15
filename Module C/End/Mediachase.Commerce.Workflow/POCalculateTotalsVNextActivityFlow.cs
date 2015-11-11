using EPiServer.ServiceLocation;
using Mediachase.Commerce.Engine;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Workflow.Activities;
using Mediachase.Commerce.Workflow.Activities.OrderGroupActivities;
using Mediachase.Commerce.WorkflowCompatibility;
using System.Collections.Specialized;

namespace Mediachase.Commerce.Workflow
{
    /// <summary>
    /// NOTE: This is a pre-release API that is UNSTABLE and might not satisfy the compatibility requirements as denoted by its associated normal version.
    /// </summary>
    [ActivityFlowConfiguration(Name = OrderGroupWorkflowManager.OrderCalculateTotalsWorkflowName, AvailableInBetaMode = true)]
    public class POCalculateTotalsVNextActivityFlow : ActivityFlow
    {
        /// <inheritdoc />
        public override ActivityFlowRunner Configure(ActivityFlowRunner activityFlow)
        {
            return activityFlow.Do<RemoveDiscountsVNextActivity>()
                           .Do<CalculateDiscountsVNextActivity>()
                           .Do<UpdateTotalsVNextActivity>();
        }
    }
}
