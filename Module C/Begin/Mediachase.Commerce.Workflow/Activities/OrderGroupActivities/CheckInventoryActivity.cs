using Mediachase.Commerce.Orders;
using Mediachase.Commerce.WorkflowCompatibility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mediachase.Commerce.Workflow.Activities
{
    public class CheckInventoryActivity : OrderGroupActivityBase
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
            try
            {
                // Validate the properties at runtime
                this.ValidateRuntime();

                this.ValidateItems();

                // Retun the closed status indicating that this activity is complete.
                return ActivityExecutionStatus.Closed;
            }
            catch
            {
                // An unhandled exception occured.  Throw it back to the WorkflowRuntime.
                throw;
            }
        }

        /// <summary>
        /// Validate inventory in the order group.
        /// </summary>
        /// <remarks>We don't need to validate quantity in the wishlist.</remarks>
        private void ValidateItems()
        {
            if (string.Equals(OrderGroup.Name, Mediachase.Commerce.Orders.Cart.WishListName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var orderForms = OrderGroup.OrderForms.ToArray();
            var lineItems = orderForms.SelectMany(x => x.LineItems.ToArray());
            var validLineItems = lineItems.Where(x => x.Code != "0" && !String.IsNullOrEmpty(x.Code) && !x.Code.StartsWith("@"));
            foreach (LineItem lineItem in validLineItems)
            {
                List<string> changeQtyReason = new List<string>();

                decimal newQty;
                if (lineItem.IsInventoryAllocated)
                {
                    newQty = lineItem.Quantity;
                }
                else
                {
                    newQty = GetNewLineItemQty(lineItem, changeQtyReason, null);
                }

                var changeQtyReasonDisplay = String.Join(" and ", changeQtyReason.ToArray());
                if (newQty == 0)
                {
                    // Remove item if it reached this stage
                    Warnings.Add("LineItemRemoved-" + lineItem.LineItemId.ToString(), String.Format("Item \"{0}\" has been removed from the cart because it is no longer available or there is not enough available quantity.", lineItem.DisplayName));
                    DeleteLineItemFromShipments(lineItem);
                    // Delete item
                    lineItem.Delete();
                }
                else
                {
                    var delta = lineItem.Quantity - newQty;
                    if (delta != 0)
                    {
                        lineItem.Quantity -= delta;
                        ChangeShipmentsLineItemQty(lineItem, delta);
                        Warnings.Add("LineItemQtyChanged-" + lineItem.LineItemId.ToString(),
                                     String.Format("Item \"{0}\" quantity has been changed {1}", lineItem.DisplayName, changeQtyReasonDisplay));
                    }
                }
            }

            //delete shipment if it has no item.
            var shipments = orderForms.SelectMany(of => of.Shipments.ToArray());
            foreach (Shipment shipment in shipments)
            {
                if (shipment.LineItemIndexes.Length == 0)
                {
                    CancelOperationKeys(shipment);
                    shipment.Delete();
                }
            }
        }

        private void DeleteLineItemFromShipments(LineItem lineItem)
        {
            var orderForm = OrderGroup.OrderForms.ToArray().FirstOrDefault();
            if (orderForm != null)
            {
                int lineItemIndex = orderForm.LineItems.IndexOf(lineItem);
                var allShipmentContainsLineItem = orderForm.Shipments.ToArray().Where(x => Shipment.GetShipmentLineItems(x).Contains(lineItem));
                foreach (var shipment in allShipmentContainsLineItem)
                {
                    shipment.RemoveLineItemIndex(lineItemIndex);
                }

                ReorderIndexes(orderForm, lineItem);
            }
        }
        private void ChangeShipmentsLineItemQty(LineItem lineItem, decimal delta)
        {
            var orderForm = OrderGroup.OrderForms.ToArray().FirstOrDefault();
            if (orderForm != null)
            {
                var lineItemIndex = orderForm.LineItems.IndexOf(lineItem);
                var allShipmentContainsLineItem = orderForm.Shipments.ToArray().Where(x => Shipment.GetShipmentLineItems(x).Contains(lineItem));
                foreach (var shipment in allShipmentContainsLineItem)
                {
                    //Decrease qty in all shipment contains line item
                    var shipmentQty = Shipment.GetLineItemQuantity(shipment, lineItem.LineItemId);
                    var newShipmentQty = shipmentQty - delta;
                    newShipmentQty = newShipmentQty > 0 ? newShipmentQty : 0;
                    //Set new line item qty in shipment
                    shipment.SetLineItemQuantity(lineItemIndex, newShipmentQty);
                    delta -= Math.Min(delta, shipmentQty);

                    if (delta == 0)
                        break;
                }
            }
        }
    }
}