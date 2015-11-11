using System.Collections.Generic;
using System.Linq;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;

namespace Ascend15.Extensions
{
    public static class CartExtensions
    {
        public static string Validate(this Cart cart)
        {
            var workflowResult = OrderGroupWorkflowManager.RunWorkflow(cart, OrderGroupWorkflowManager.CartValidateWorkflowName);
            var warnings = OrderGroupWorkflowManager.GetWarningsFromWorkflowResult(workflowResult).ToArray();
            return warnings.Any() ? string.Join(" ", warnings) : null;
        }

        public static string Prepare(this Cart cart)
        {
            var workflowResult = OrderGroupWorkflowManager.RunWorkflow(cart, OrderGroupWorkflowManager.CartPrepareWorkflowName);
            var warnings = OrderGroupWorkflowManager.GetWarningsFromWorkflowResult(workflowResult).ToArray();
            return warnings.Any() ? string.Join(" ", warnings) : null;
        }

        public static string Checkout(this Cart cart)
        {
            var workflowResult = OrderGroupWorkflowManager.RunWorkflow(cart, OrderGroupWorkflowManager.CartCheckOutWorkflowName);
            var warnings = OrderGroupWorkflowManager.GetWarningsFromWorkflowResult(workflowResult).ToArray();
            return warnings.Any() ? string.Join(" ", warnings) : null;
        }

        public static IReadOnlyCollection<LineItem> GetAllLineItems(this Cart cart)
        {
            return cart.OrderForms.Any() ? cart.OrderForms.First().LineItems.ToList() : new List<LineItem>();
        }

        public static LineItem GetLineItem(this Cart cart, string code)
        {
            return cart.GetAllLineItems().FirstOrDefault(x => x.Code == code);
        }
    }
}
