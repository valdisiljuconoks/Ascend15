using Mediachase.Commerce.Engine;
using Mediachase.Commerce.WorkflowCompatibility;

namespace Mediachase.Commerce.Workflow.Activities.CartActivities
{
    public class MutateContextPropertyActivity : CartActivityBase
    {
        [ActivityFlowContextProperty]
        public string ThisIsMutableProperty { get; set; }

        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {

            ThisIsMutableProperty = "This value is set in MutableContextPropertyActivity";

            return ActivityExecutionStatus.Closed;
        }
    }
}
