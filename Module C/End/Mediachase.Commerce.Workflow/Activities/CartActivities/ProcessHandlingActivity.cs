using Mediachase.Commerce.WorkflowCompatibility;

namespace Mediachase.Commerce.Workflow.Activities.Cart
{
	public class ProcessHandlingActivity : CartActivityBase
	{
        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            return ActivityExecutionStatus.Closed;
        }
    }
}
