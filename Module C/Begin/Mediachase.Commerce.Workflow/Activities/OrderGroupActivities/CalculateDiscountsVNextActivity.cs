using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Marketing;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.WorkflowCompatibility;
using System.Linq;

namespace Mediachase.Commerce.Workflow.Activities
{
    /// <summary>
    /// NOTE: This is a pre-release API that is UNSTABLE and might not satisfy the compatibility requirements as denoted by its associated normal version.
    /// Calculate discounts using the new promotion engine
    /// </summary>
    public class CalculateDiscountsVNextActivity : OrderGroupActivityBase
    {
        /// <summary>
        /// Executes the specified execution context.
        /// </summary>
        /// <param name="executionContext">The execution context.</param>
        /// <returns></returns>
        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            ValidateRuntime();
            ServiceLocator.Current.GetInstance<IPromotionEngine>().Run(OrderGroup);
            var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
            var order = OrderGroup as IOrderGroup;
            foreach (var promotion in order.Promotions)
            {
                var item = contentLoader.Get<VariationContent>(promotion.ContentLink);
                if (item == null)
                {
                    continue;
                }
                var lineItem = order.Forms.SelectMany(x => x.Shipments).SelectMany(x => x.LineItems).FirstOrDefault(x => x.Code.Equals(item.Code));
                if (lineItem == null)
                {
                    continue;
                }
                var metaItem = OrderGroup.OrderForms.SelectMany(x => x.LineItems).FirstOrDefault(x => x.LineItemId == lineItem.LineItemId);
                if (metaItem != null)
                {
                    metaItem.LineItemDiscountAmount = lineItem.LineItemDiscountAmount;
                }
            }
            return ActivityExecutionStatus.Closed;
        }
    }
}
