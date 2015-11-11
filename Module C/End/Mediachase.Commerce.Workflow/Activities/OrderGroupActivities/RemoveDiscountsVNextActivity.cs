using Castle.Core.Internal;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.WorkflowCompatibility;
using System.Linq;

namespace Mediachase.Commerce.Workflow.Activities
{
    /// <summary>
    /// NOTE: This is a pre-release API that is UNSTABLE and might not satisfy the compatibility requirements as denoted by its associated normal version.
    /// </summary>
    public class RemoveDiscountsVNextActivity : OrderGroupActivityBase
	{
        /// <summary>
        /// Called by the workflow runtime to execute an activity.
        /// </summary>
        /// <param name="executionContext">The <see cref="T:Mediachase.Commerce.WorkflowCompatibility.ActivityExecutionContext"/> to associate with this <see cref="T:Mediachase.Commerce.WorkflowCompatibility.Activity"/> and execution.</param>
        /// <returns>
        /// The <see cref="T:Mediachase.Commerce.WorkflowCompatibility.ActivityExecutionStatus"/> of the run task, which determines whether the activity remains in the executing state, or transitions to the closed state.
        /// </returns>
        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            ValidateRuntime();
            RemoveDiscounts();
            return ActivityExecutionStatus.Closed;
        }

        /// <summary>
        /// Removes the discounts.
        /// </summary>
        private void RemoveDiscounts()
        {
            var order = OrderGroup as IOrderGroup;
            if (order == null)
            {
                return;
            }

            var items = order.Forms.SelectMany(x => x.Shipments).SelectMany(x => x.LineItems);
            items.ForEach(x => x.LineItemDiscountAmount = 0);

            foreach(var promotion in order.Promotions)
            {
                promotion.SavedAmount = 0;
            }
        }
    }
}
