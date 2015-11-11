using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.InventoryService;
using Mediachase.Commerce.InventoryService.BusinessLogic;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.WorkflowCompatibility;
using Mediachase.MetaDataPlus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mediachase.Commerce.Workflow.Activities.Cart
{
    public class AdjustInventoryActivity : CartActivityBase
    {
        [NonSerialized]
        private readonly ILogger _logger = LogManager.GetLogger(typeof(AdjustInventoryActivity));

        private Injected<IInventoryService> _inventoryService;
        private Injected<OperationKeySerializer> _operationKeySerializer;

        private readonly static object _lockObject = new object();

        /// <summary>
        /// Get inventory requests for shipment
        /// </summary>
        /// <param name="shipment">The shipment</param>
        /// <param name="itemIndexStart">The start index</param>
        /// <param name="type">The inventory request type</param>
        /// <returns>List inventory request item of a shipment</returns>
        private IEnumerable<InventoryRequestItem> GetRequestInventory(Shipment shipment, int itemIndexStart, InventoryRequestType type)
        {
            return shipment.OperationKeysMap.SelectMany(c => c.Value).Select(key =>
                    new InventoryRequestItem()
                    {
                        ItemIndex = itemIndexStart++,
                        OperationKey = key,
                        RequestType = type
                    });
        }

        /// <summary>
        /// Get inventory request for the line item.
        /// </summary>
        /// <param name="shipment">The shipment</param>
        /// <param name="lineItemIndex">The line item index</param>
        /// <param name="itemIndexStart">The start index for request item</param>
        /// <param name="type">The inventory request type</param>
        /// <returns>List inventory request item for a line item</returns>
        private IEnumerable<InventoryRequestItem> GetLineItemRequestInventory(Shipment shipment, int lineItemIndex, int itemIndexStart, InventoryRequestType type)
        {
            return shipment.OperationKeysMap.Where(c => c.Key == lineItemIndex).SelectMany(c => c.Value).Select(key =>
                    new InventoryRequestItem()
                    {
                        ItemIndex = itemIndexStart++,
                        OperationKey = key,
                        RequestType = type
                    });
        }

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
                // Check for multiple warehouses. In the default, we simply reject processing an order if the application has
                //  multiple warehouses. Any corresponding fulfillment process is the responsibility of the client.
                this.CheckMultiWarehouse();

                // Validate the properties at runtime
                this.ValidateRuntime();

                // Return close status if order group is Payment Plan
                if (OrderGroup is PaymentPlan)
                {
                    return ActivityExecutionStatus.Closed;
                }

                var orderGroupStatus = OrderStatusManager.GetOrderGroupStatus(OrderGroup);
                var orderForms = OrderGroup.OrderForms.Where(o => !OrderForm.IsReturnOrderForm(o));
                var inventoryRequests = new List<InventoryRequestItem>();

                foreach (OrderForm orderForm in orderForms)
                {
                    foreach (Shipment shipment in orderForm.Shipments)
                    {
                        var shipmentStatus = OrderStatusManager.GetOrderShipmentStatus(shipment);
                        bool completingOrder = orderGroupStatus == OrderStatus.Completed || shipmentStatus == OrderShipmentStatus.Shipped;
                        bool cancellingOrder = orderGroupStatus == OrderStatus.Cancelled || shipmentStatus == OrderShipmentStatus.Cancelled;
                        _logger.Debug(string.Format("Adjusting inventory, got orderGroupStatus as {0} and shipmentStatus as {1}. completingOrder as {2} and cancellingOrder as {3}.", orderGroupStatus, shipmentStatus, completingOrder, cancellingOrder));

                        // When completing/cancelling an order or a shipment
                        if (completingOrder || cancellingOrder)
                        {
                            var requestType = completingOrder ? InventoryRequestType.Complete : InventoryRequestType.Cancel;
                            inventoryRequests.AddRange(GetRequestInventory(shipment, inventoryRequests.Count, requestType));
                            // When processed request, need to clear all operation keys from the shipment
                            shipment.ClearOperationKeys();
                        }
                        // When release a shipment, check if shipment contain a BackOrder then need to complete that BackOrder.
                        else if (shipmentStatus == OrderShipmentStatus.Released)
                        {
                            foreach (LineItem lineItem in Shipment.GetShipmentLineItems(shipment))
                            {
                                var lineItemIndex = orderForm.LineItems.IndexOf(lineItem);
                                var completeBackOrderRequest = new List<InventoryRequestItem>();
                                var lineItemRequest = GetLineItemRequestInventory(shipment, lineItemIndex, 0, InventoryRequestType.Complete);

                                // Only need to process complete BackOrder request type
                                foreach (var request in lineItemRequest)
                                {
                                    InventoryRequestType requestType;
                                    InventoryChange change;
                                    _operationKeySerializer.Service.TryDeserialize(request.OperationKey, out requestType, out change);
                                    if (requestType == InventoryRequestType.Backorder)
                                    {
                                        // Add BackOrder request to request list
                                        completeBackOrderRequest.Add(request);

                                        // Then remove BackOrder request operation key from shipment's operation key map
                                        shipment.RemoveOperationKey(lineItemIndex, request.OperationKey);
                                    }
                                }

                                // Storage the response operation keys from complete BackOrder mapping with line item index
                                if (completeBackOrderRequest.Count > 0)
                                {
                                    InventoryResponse response = _inventoryService.Service.Request(new InventoryRequest(DateTime.UtcNow, completeBackOrderRequest, null));
                                    if (response != null && response.IsSuccess)
                                    {
                                        shipment.InsertOperationKeys(lineItemIndex, response.Items.Select(c => c.OperationKey));
                                    }
                                }
                            }
                        }
                        else if (orderGroupStatus == OrderStatus.InProgress || orderGroupStatus == OrderStatus.AwaitingExchange)
                        {
                            // When placing an order or creating an exchange order
                            bool placingOrder = shipmentStatus == OrderShipmentStatus.AwaitingInventory || shipmentStatus == OrderShipmentStatus.InventoryAssigned;
                            if (placingOrder)
                            {
                                var lineItems = Shipment.GetShipmentLineItems(shipment);
                                if (lineItems.Any(c => c.ObjectState == MetaObjectState.Modified) || HasDeletedLineItem(shipment, lineItems))
                                {
                                    CancelOperationKeys(shipment);
                                    foreach (LineItem lineItem in lineItems)
                                    {
                                        RequestInventory(orderForm, shipment, lineItem);
                                    }
                                }
                                else if (lineItems.Any(c => c.ObjectState == MetaObjectState.Added))
                                {
                                    foreach (LineItem lineItem in lineItems.Where(c => c.ObjectState == MetaObjectState.Added))
                                    {
                                        RequestInventory(orderForm, shipment, lineItem);
                                    }
                                }

                            }
                        }
                        if (shipment.ObjectState == MetaObjectState.Modified)
                        {
                            shipment.AcceptChanges();
                        }
                    }
                }

                if (inventoryRequests.Any())
                {
                    _inventoryService.Service.Request(new InventoryRequest(DateTime.UtcNow, inventoryRequests, null));
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

        private bool HasDeletedLineItem(Shipment shipment, IEnumerable<LineItem> lineItems)
        {
            //There is deleted line item if, except the newly added line items, the number count is less than the operation keys count;
            return lineItems.Count(c => c.ObjectState != MetaObjectState.Added) < shipment.OperationKeysMap.Count;
        }

        private void RequestInventory(OrderForm orderForm, Shipment shipment, LineItem lineItem)
        {
            var lineItemIndex = orderForm.LineItems.IndexOf(lineItem);
            InventoryRequest request;
            var outOfStock = false;
            InventoryResponse response = null;

            lock (_lockObject)
            {
                // Check quantity of order again to make sure there is enough quantity.
                outOfStock = this.GetNewLineItemQty(lineItem, new List<string>(), shipment) <= 0;

                if (!outOfStock)
                {
                    request = AdjustStockItemQuantity(shipment, lineItem);
                    if (request != null)
                    {
                        response = _inventoryService.Service.Request(request);
                    }
                }
            }

            // if out of stock, delete line item and remove line item from the shipment.
            if (outOfStock)
            {
                Warnings.Add("LineItemRemoved-" + lineItem.LineItemId.ToString(), String.Format("Item \"{0}\" has been removed from the cart because there is not enough available quantity.", lineItem.DisplayName));
                lineItem.Delete();
                shipment.RemoveLineItemIndex(lineItemIndex);

                // Delete the shipment and cancel operation keys if it has no more line item.
                if (shipment.LineItemIndexes.Length == 0)
                {
                    CancelOperationKeys(shipment);
                    shipment.Delete();
                    shipment.AcceptChanges();
                }
                return;
            }

            if (response != null && response.IsSuccess)
            {
                lineItem.IsInventoryAllocated = true;

                // Store operation keys to Shipment for each line item, to use later for complete request
                var existedIndex = shipment.OperationKeysMap.ContainsKey(lineItemIndex);
                var operationKeys = response.Items.Select(c => c.OperationKey);
                if (!existedIndex)
                {
                    shipment.AddInventoryOperationKey(lineItemIndex, operationKeys);
                }
                else
                {
                    shipment.InsertOperationKeys(lineItemIndex, operationKeys);
                }
            }
        }
    }
}