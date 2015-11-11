using Mediachase.Commerce.Orders;
using Mediachase.Commerce.WorkflowCompatibility;
using System.Collections.Generic;
using System.Linq;

namespace Mediachase.Commerce.Workflow.Activities.OrderGroupActivities
{
    public class UpdateTotalsVNextActivity : OrderGroupActivityBase
    {
        /// <summary>
        /// Gets the totals, and updates the order group with the calculated values.
        /// </summary>
        /// <param name="executionContext"></param>
        /// <returns><see cref="ActivityExecutionStatus.Closed"/></returns>
        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            var calculatedValues = GetTotals();
            UpdateOrderGroupTotals(calculatedValues);

            foreach (OrderForm orderForm in OrderGroup.OrderForms)
            {
                var orderFormTotals = calculatedValues[orderForm];
                var extendedPrices = new Dictionary<int, decimal>();
                foreach (Shipment shipment in orderForm.Shipments)
                {
                    var shipmentTotals = orderFormTotals[shipment];
                    foreach (var lineItem in shipment.LineItems)
                    {
                        if (!extendedPrices.Keys.Contains(lineItem.LineItemId))
                        {
                            extendedPrices.Add(lineItem.LineItemId, shipmentTotals[lineItem].Value.Amount);
                        }
                        else
                        {
                            extendedPrices[lineItem.LineItemId] += shipmentTotals[lineItem].Value.Amount;
                        }
                    }
                    UpdateShipmentTotals(shipment, shipmentTotals);
                }
                foreach (var lineItem in orderForm.LineItems.ToList())
                {
                    lineItem.ExtendedPrice = extendedPrices.ContainsKey(lineItem.LineItemId) ? extendedPrices[lineItem.LineItemId] : (lineItem.Quantity * lineItem.PlacedPrice) - (lineItem.LineItemDiscountAmount + lineItem.OrderLevelDiscountAmount);
                }
                UpdateOrderFormTotals(orderForm, orderFormTotals);
            }

            // Retun the closed status indicating that this activity is complete.
            return ActivityExecutionStatus.Closed;
        }

        /// <summary>
        /// Gets the totals for the order group.
        /// </summary>
        /// <returns>Model containing the calculated total values.</returns>
        protected virtual OrderGroupTotals GetTotals()
        {
            return ((IOrderGroup)OrderGroup).GetTotals();
        }

        /// <summary>
        /// Updates the totals for an order form.
        /// </summary>
        /// <param name="orderForm">The order form</param>
        /// <param name="totals">The model containing calculated totals for the order form.</param>
        protected virtual void UpdateOrderFormTotals(OrderForm orderForm, OrderFormTotals totals)
        {
            orderForm.HandlingTotal = totals.HandlingTotal.Amount;
            orderForm.ShippingTotal = totals.ShippingTotal.Amount;
            orderForm.SubTotal = totals.SubTotal.Amount;
            orderForm.TaxTotal = totals.TaxTotal.Amount;
            orderForm.Total = totals.Total.Amount;
        }

        /// <summary>
        /// Updates the totals for a shipment.
        /// </summary>
        /// <param name="shipment">The shipment</param>
        /// <param name="totals">The model containing calculated totals for the shipment</param>
        protected virtual void UpdateShipmentTotals(Shipment shipment, ShippingTotals totals)
        {
            shipment.ShippingSubTotal = totals.ShippingCost.Amount;
            shipment.SubTotal = totals.ItemsTotal.Amount;
        }

        /// <summary>
        /// Updates the totals for the current order group.
        /// </summary>
        /// <param name="totals">The model containing calculated totals for the order group</param>
        protected virtual void UpdateOrderGroupTotals(OrderGroupTotals totals)
        {
            OrderGroup.SubTotal = totals.SubTotal.Amount;
            OrderGroup.Total = totals.Total.Amount;
            OrderGroup.ShippingTotal = totals.ShippingTotal.Amount;
            OrderGroup.TaxTotal = totals.TaxTotal.Amount;
            OrderGroup.HandlingTotal = totals.HandlingTotal.Amount;
        }
    }
}