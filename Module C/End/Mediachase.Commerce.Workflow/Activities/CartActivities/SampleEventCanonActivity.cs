using System;
using Mediachase.Commerce.WorkflowCompatibility;

namespace Mediachase.Commerce.Workflow.Activities.CartActivities
{
    public class SampleEventCanonActivity : CartActivityBase
    {
        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            RaiseEvent("SampleEvent", new EventArgs());

            return ActivityExecutionStatus.Closed;
        }
    }
}
