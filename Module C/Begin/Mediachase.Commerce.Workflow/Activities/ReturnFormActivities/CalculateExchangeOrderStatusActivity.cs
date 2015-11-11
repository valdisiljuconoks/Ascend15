using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.WorkflowCompatibility;

namespace Mediachase.Commerce.Workflow.Activities.ReturnForm
{
    public class CalculateExchangeOrderStatusActivity : ReturnFormBaseActivity
    {
        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            try
            {
                if (base.ReturnFormStatus == ReturnFormStatus.Complete)
                {
                    //Need change ExchangeOrder from AvaitingCompletition to InProgress
                    var exchangeOrder = ReturnExchangeManager.GetExchangeOrderForReturnForm(base.ReturnOrderForm);
                    if (exchangeOrder != null && OrderStatusManager.GetOrderGroupStatus(exchangeOrder) == OrderStatus.AwaitingExchange)
                    {
                        OrderStatusManager.ProcessOrder(exchangeOrder);
                        exchangeOrder.AcceptChanges();
                    }
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
    }
}
