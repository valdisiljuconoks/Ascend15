using Mediachase.Commerce.Engine;
using Mediachase.Commerce.WorkflowCompatibility;

namespace Mediachase.Commerce.Workflow.Activities.CartActivities
{
    public class WatchMutablePropertyActivity : CartActivityBase
    {
        [ActivityFlowContextProperty]
        public string ThisIsMutableProperty { get; set; }

        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            return ActivityExecutionStatus.Closed;
        }
    }
}
