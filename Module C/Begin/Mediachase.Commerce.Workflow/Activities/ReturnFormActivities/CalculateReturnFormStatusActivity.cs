using Mediachase.Commerce.Orders;
using Mediachase.Commerce.WorkflowCompatibility;

namespace Mediachase.Commerce.Workflow.Activities.ReturnForm
{
    public class CalculateReturnFormStatusActivity : ReturnFormBaseActivity
    {
        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            try
            {
                var newStatus = CalculateReturnFormStatus();
                if (newStatus != base.ReturnFormStatus)
                {
                    ChangeReturnFormStatus(newStatus);
                }

                // Retun the closed status indicating that this activity is complete.
                return ActivityExecutionStatus.Closed;
            }
            catch
            {
                // An unhandled exception occured.  Throw it back to the WorkflowRuntime.
                throw;
            }
        }

        private ReturnFormStatus CalculateReturnFormStatus()
        {
            ReturnFormStatus retVal = base.ReturnFormStatus;

            return retVal;
        }
    }
}
