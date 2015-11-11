using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Dto;
using Mediachase.Commerce.Catalog.Managers;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Engine;
using Mediachase.Commerce.Inventory;
using Mediachase.Commerce.InventoryService;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Pricing;
using Mediachase.Commerce.WorkflowCompatibility;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;

namespace Mediachase.Commerce.Workflow.Activities
{
    public abstract class OrderGroupActivityBase : Activity
    {
        [NonSerialized]
        private readonly MapUserKey _mapUserKey = new MapUserKey();

        protected IInventoryService InventoryService
        {
            get
            {
                return ServiceLocator.Current.GetInstance<IInventoryService>();
            }
        }

        protected IWarehouseRepository WarehouseRepository
        {
            get
            {
                return ServiceLocator.Current.GetInstance<IWarehouseRepository>();
            }
        }

        protected DateTime SafeBeginningOfTime
        {
            get
            {
                return new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            }
        }

        /// <summary>
        /// Gets or sets the order group.
        /// </summary>
        /// <value>The order group.</value>
        [ActivityFlowContextProperty]
        public OrderGroup OrderGroup { get; set; }

        /// <summary>
        /// Gets or sets the warnings.
        /// </summary>
        /// <value>The warnings.</value>
        [ActivityFlowContextProperty]
        public StringDictionary Warnings { get; set; }

        /// <summary>
        /// Validates the runtime.
        /// </summary>
        /// <returns></returns>
        protected virtual bool ValidateRuntime()
        {
            // Create a new collection for storing the validation errors
            ValidationErrorCollection validationErrors = new ValidationErrorCollection();

            // Validate the Order Properties
            this.ValidateOrderProperties(validationErrors);

            // Validate properties
            this.ValidateProperties(validationErrors);

            // Raise an exception if we have ValidationErrors
            if (validationErrors.HasErrors)
            {
                string validationErrorsMessage = String.Empty;

                foreach (ValidationError error in validationErrors)
                {
                    validationErrorsMessage +=
                        string.Format("Validation Error:  Number {0} - '{1}' \n",
                        error.ErrorNumber, error.ErrorText);
                }

                // Throw a new exception with the validation errors.
                throw new WorkflowValidationFailedException(validationErrorsMessage, validationErrors);

            }
            // If we made it this far, then the data must be valid.
            return true;
        }

        protected virtual IWarehouse CheckMultiWarehouse()
        {
            var warehouses = WarehouseRepository.List()
                .Where(w => (OrderGroup.ApplicationId == w.ApplicationId) && w.IsActive && w.IsFulfillmentCenter);
            if (warehouses.Count() > 1)
            {
                throw new NotSupportedException("Multiple fulfillment centers without custom fulfillment process.");
            }
            return warehouses.SingleOrDefault();
        }

        /// <summary>
        /// Validates the order properties.
        /// </summary>
        /// <param name="validationErrors">The validation errors.</param>
        protected virtual void ValidateOrderProperties(ValidationErrorCollection validationErrors)
        {
            // Validate the To property
            if (this.OrderGroup == null)
            {
                ValidationError validationError = ValidationError.GetNotSetValidationError("OrderGroup");
                validationErrors.Add(validationError);
            }
        }

        protected Money? GetItemPrice(CatalogEntryDto.CatalogEntryRow entry, LineItem lineItem, CustomerContact customerContact)
        {
            List<CustomerPricing> customerPricing = new List<CustomerPricing>();
            customerPricing.Add(CustomerPricing.AllCustomers);
            if (customerContact != null)
            {
                var userKey = _mapUserKey.ToUserKey(customerContact.UserId);
                if (userKey != null && !string.IsNullOrWhiteSpace(userKey.ToString()))
                {
                    customerPricing.Add(new CustomerPricing(CustomerPricing.PriceType.UserName, userKey.ToString()));
                }

                if (!string.IsNullOrEmpty(customerContact.EffectiveCustomerGroup))
                {
                    customerPricing.Add(new CustomerPricing(CustomerPricing.PriceType.PriceGroup, customerContact.EffectiveCustomerGroup));
                }
            }

            IPriceService priceService = ServiceLocator.Current.GetInstance<IPriceService>();

            PriceFilter priceFilter = new PriceFilter()
            {
                Currencies = new List<Currency>() { new Currency(lineItem.Parent.Parent.BillingCurrency) },
                Quantity = lineItem.Quantity,
                CustomerPricing = customerPricing,
                ReturnCustomerPricing = false // just want one value
            };
            // Get the lowest price among all the prices matching the parameters
            IPriceValue priceValue = priceService
                .GetPrices(lineItem.Parent.Parent.MarketId, FrameworkContext.Current.CurrentDateTime, new CatalogKey(entry), priceFilter)
                .OrderBy(pv => pv.UnitPrice)
                .FirstOrDefault();

            if (priceValue == null)
            {
                return null;
            }
            else
            {
                return priceValue.UnitPrice;
            }
        }

        protected void PopulateInventoryInfo(IWarehouseInventory inv, LineItem lineItem)
        {
            if (inv != null)
            {
                lineItem.AllowBackordersAndPreorders = inv.AllowBackorder | inv.AllowPreorder;
                // Init quantities once
                lineItem.BackorderQuantity = inv.BackorderQuantity;
                lineItem.InStockQuantity = inv.InStockQuantity - inv.ReservedQuantity;
                lineItem.PreorderQuantity = inv.PreorderQuantity;
                lineItem.InventoryStatus = (int)inv.InventoryStatus;
            }
            else
            {
                var baseEntry = CatalogContext.Current.GetCatalogEntry(lineItem.Code,
                    new CatalogEntryResponseGroup(
                        CatalogEntryResponseGroup.ResponseGroup.CatalogEntryInfo |
                        CatalogEntryResponseGroup.ResponseGroup.Variations));
                lineItem.AllowBackordersAndPreorders = false;
                lineItem.InStockQuantity = 0;
                lineItem.PreorderQuantity = 0;
                lineItem.InventoryStatus = (int)baseEntry.InventoryStatus;
            }
        }

        protected void AddWarningSafe(StringDictionary warnings, string key, string value)
        {
            string uniqueKey, uniqueKeyPrefix = key + '-';

            int counter = 1;
            do
            {
                string suffix = counter.ToString(CultureInfo.InvariantCulture);
                uniqueKey = uniqueKeyPrefix + suffix;
                ++counter;
            }
            while (warnings.ContainsKey(uniqueKey));

            warnings.Add(uniqueKey, value);
        }

        /// <summary>
        /// Gets the type of the resulting payments by transaction.
        /// </summary>
        /// <param name="payments">The payments.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        protected IEnumerable<Payment> GetResultingPaymentsByTransactionType(IEnumerable<Payment> payments, TransactionType type)
        {
            List<Payment> retVal = new List<Payment>();
            var paymentsWithSameTranType = GetPaymentsByTransactionType(payments, new TransactionType[] { type });
            foreach (var payment in paymentsWithSameTranType)
            {
                //Get all related payments only in Processing status
                var allRelatedPayments = GetAllRelatedPayments(payment).Where(x => PaymentStatusManager.GetPaymentStatus(x) == PaymentStatus.Processed);
                //do not return authorization payments which have Void related payments, or it has been Captured all amount.
                if (type == TransactionType.Authorization)
                {
                    var anyVoidTransaction = allRelatedPayments.Any(x => GetPaymentTransactionType(x) == TransactionType.Void);
                    var anyCaptureTransaction = allRelatedPayments.Any(x => GetPaymentTransactionType(x) == TransactionType.Capture);
                    var capturedAmount = allRelatedPayments.Where(x => GetPaymentTransactionType(x) == TransactionType.Capture).Sum(x => x.Amount);
                    if (!anyVoidTransaction)
                    {
                        if (anyCaptureTransaction)
                        {
                            // get total captured amount, then check if it is lower than the authorized amount to return the payment.
                            // in other word, we will not return the payment if all of authorized amount has been captured, which means no need to capture anymore.
                            if (capturedAmount < payment.Amount)
                            {
                                yield return payment;
                            }
                        }
                        else
                        {
                            yield return payment;
                        }
                    }
                }
                else
                {
                    //do not return other payments with haved Void related payments
                    if (!allRelatedPayments.Any(x => GetPaymentTransactionType(x) == TransactionType.Void))
                    {
                        yield return payment;
                    }
                }
            }
        }

        private IEnumerable<Payment> GetPaymentsByTransactionType(IEnumerable<Payment> payments, TransactionType[] types)
        {
            return payments.Where(x => types.Any(type => GetPaymentTransactionType(x) == type));
        }

        /// <summary>
        /// Gets the type of the payment transaction.
        /// </summary>
        /// <param name="payment">The payment.</param>
        /// <returns></returns>
        protected TransactionType GetPaymentTransactionType(Payment payment)
        {
            TransactionType retVal = TransactionType.Other;
            if (!string.IsNullOrEmpty(payment.TransactionType))
            {
                retVal = (TransactionType)Enum.Parse(typeof(TransactionType), payment.TransactionType);
            }
            return retVal;
        }


        /// <summary>
        /// Gets all related payments. On Order.
        /// </summary>
        /// <param name="payment">The payment.</param>
        /// <returns></returns>
        protected IEnumerable<Payment> GetAllRelatedPayments(Payment payment)
        {
            if (payment == null)
            {
                throw new ArgumentNullException("payment");
            }
            if (payment.Parent == null)
            {
                throw new NullReferenceException("payment.Parent");
            }

            var retVal = payment.Parent.Payments.ToArray().Where(x => x.TransactionID == payment.TransactionID);
            return retVal;
        }

        /// <summary>
        /// Adjust item stock.
        /// </summary>
        /// <param name="lineItem">The line item.</param>
        /// <returns></returns>
        protected void AdjustStockItemQuantity(LineItem lineItem)
        {
            AdjustStockItemQuantity(null, lineItem);
        }

        /// <summary>
        /// Adjust item stock.
        /// </summary>
        /// <param name="shipment">The shipment.</param>
        /// <param name="lineItem">The line item.</param>
        /// <returns></returns>
        protected InventoryRequest AdjustStockItemQuantity(Shipment shipment, LineItem lineItem)
        {
            var entryDto = CatalogContext.Current.GetCatalogEntryDto(lineItem.Code,
                                                                    new CatalogEntryResponseGroup(CatalogEntryResponseGroup.ResponseGroup.Variations));
            if (entryDto == null)
            {
                return null;
            }

            var catalogEntry = entryDto.CatalogEntry.FirstOrDefault();
            if (catalogEntry != null && InventoryTrackingEnabled(catalogEntry))
            {
                decimal delta = GetLineItemAdjustedQuantity(shipment, lineItem);

                string warehouseCode = shipment != null && !string.IsNullOrEmpty(shipment.WarehouseCode) ? shipment.WarehouseCode : lineItem.WarehouseCode;
                var inventoryRecord = InventoryService.Get(catalogEntry.Code, warehouseCode);
                if (inventoryRecord == null)
                {
                    return null;
                }

                var requestItems = GetInventoryRequestItems(lineItem, inventoryRecord, delta);
                if (requestItems.Any())
                {
                    return new InventoryRequest(DateTime.UtcNow, requestItems, null);
                }
            }

            return null;
        }

        /// <summary>
        /// Gets list of InventoryRequestItem for adjusting inventory of specific line item .
        /// </summary>
        /// <param name="lineItem">Line item object in cart.</param>
        /// <param name="inventoryRecord">Inventory record associated with the line item's catalog entry.</param>
        /// <param name="delta">The change in inventory.</param>
        private IList<InventoryRequestItem> GetInventoryRequestItems(LineItem lineItem, InventoryRecord inventoryRecord, decimal delta)
        {
            var requestItems = new List<InventoryRequestItem>();

            var entryCode = lineItem.Code;
            var warehouseCode = lineItem.WarehouseCode;

            //arrival
            if (delta > 0)
            {
                // TODO: that is impossible to request a negative quantity with new inventory API, so need to find another way in this case.
                // need distribute delta between InStock, Backorder, Preorder.
                if (lineItem.InStockQuantity > 0)
                {
                    var backorderDelta = Math.Min(delta, lineItem.BackorderQuantity - inventoryRecord.BackorderAvailableQuantity - inventoryRecord.BackorderRequestedQuantity);
                    var preorderdelta = Math.Min(delta, lineItem.PreorderQuantity - inventoryRecord.PreorderAvailableQuantity - inventoryRecord.PreorderRequestedQuantity);

                    // In this case, need to add more backorder or preorder quantity
                    requestItems.Add(CreateRequestItem(requestItems.Count, InventoryRequestType.Backorder, entryCode, warehouseCode, -backorderDelta));
                    requestItems.Add(CreateRequestItem(requestItems.Count, InventoryRequestType.Preorder, entryCode, warehouseCode, -preorderdelta));
                    requestItems.Add(CreateRequestItem(requestItems.Count, InventoryRequestType.Purchase, entryCode, warehouseCode, -(delta - backorderDelta - preorderdelta)));

                } //need distribute delta between Preorder and Backorder
                else if (lineItem.InStockQuantity == 0)
                {
                    if (lineItem.PreorderQuantity > 0)
                    {
                        requestItems.Add(CreateRequestItem(requestItems.Count, InventoryRequestType.Preorder, entryCode, warehouseCode, delta));
                    }
                    else if (lineItem.BackorderQuantity > 0)
                    {
                        requestItems.Add(CreateRequestItem(requestItems.Count, InventoryRequestType.Backorder, entryCode, warehouseCode, delta));
                    }
                }
            }//consumption
            else
            {
                delta = Math.Abs(delta);
                var requestDate = FrameworkContext.Current.CurrentDateTime;
                var allowPreorder = inventoryRecord.PreorderAvailableUtc > SafeBeginningOfTime && requestDate >= inventoryRecord.PreorderAvailableUtc && delta <= inventoryRecord.PreorderAvailableQuantity;
                var allowBackOrder = inventoryRecord.BackorderAvailableQuantity > 0 && inventoryRecord.BackorderAvailableUtc <= requestDate;

                if (requestDate >= inventoryRecord.PurchaseAvailableUtc)
                {
                    // In case inventory status is Disable or enough purchase quantity, always do Purchase request.
                    if (!inventoryRecord.IsTracked || inventoryRecord.PurchaseAvailableQuantity >= delta)
                    {
                        requestItems.Add(CreateRequestItem(requestItems.Count, InventoryRequestType.Purchase, entryCode, warehouseCode, delta));
                    }
                    else
                    {
                        if (inventoryRecord.PurchaseAvailableQuantity > 0)
                        {
                            var backOrderDelta = delta - inventoryRecord.PurchaseAvailableQuantity;
                            if (allowBackOrder && backOrderDelta <= inventoryRecord.BackorderAvailableQuantity)
                            {
                                // purchase remain items and backorder other
                                requestItems.Add(CreateRequestItem(requestItems.Count, InventoryRequestType.Purchase, entryCode, warehouseCode, inventoryRecord.PurchaseAvailableQuantity));
                                requestItems.Add(CreateRequestItem(requestItems.Count, InventoryRequestType.Backorder, entryCode, warehouseCode, backOrderDelta));
                            }
                        }
                        else if (allowBackOrder && delta <= inventoryRecord.BackorderAvailableQuantity)
                        {
                            // Backorder request
                            requestItems.Add(CreateRequestItem(requestItems.Count, InventoryRequestType.Backorder, entryCode, warehouseCode, delta));
                        }
                    }
                }
                else if (allowPreorder)
                {
                    // Preorder request
                    requestItems.Add(CreateRequestItem(requestItems.Count, InventoryRequestType.Preorder, entryCode, warehouseCode, delta));
                }
            }
            return requestItems;
        }

        private InventoryRequestItem CreateRequestItem(int index, InventoryRequestType requestType, string entryCode, string warehouseCode, decimal delta)
        {
            //We currently do not use operation key and context
            return new InventoryRequestItem(index, requestType, entryCode, warehouseCode, delta, string.Empty, null);
        }

        private decimal GetLineItemAdjustedQuantity(Shipment shipment, LineItem lineItem)
        {
            return -(shipment != null ? Shipment.GetLineItemQuantity(shipment, lineItem.LineItemId) : lineItem.Quantity);
        }

        /// <summary>
        /// Get entry row from a line item
        /// </summary>
        /// <param name="lineItem">line item</param>
        protected static CatalogEntryDto.CatalogEntryRow GetEntryRowForLineItem(LineItem lineItem)
        {
            var responseGroup = new CatalogEntryResponseGroup(CatalogEntryResponseGroup.ResponseGroup.Variations);

            CatalogEntryDto entryDto = CatalogContext.Current.GetCatalogEntryDto(lineItem.Code, responseGroup);
            return entryDto.CatalogEntry.FirstOrDefault();
        }

        /// <summary>
        /// Check catalog entry's tracking inventory was enable or not.
        /// </summary>
        /// <param name="catalogEntry">Catalog entry.</param>
        private bool InventoryTrackingEnabled(CatalogEntryDto.CatalogEntryRow catalogEntry)
        {
            var entryVariations = catalogEntry.GetVariationRows();
            var variation = entryVariations.FirstOrDefault();
            return variation != null && variation.TrackInventory;
        }

        /// <summary>
        /// Reorder line item indexes in all shipments after delete an item
        /// </summary>
        /// <param name="orderForm">order form</param>
        /// <param name="lineItem">removed line item</param>
        protected void ReorderIndexes(OrderForm orderForm, LineItem lineItem)
        {
            int lineItemIndex = orderForm.LineItems.IndexOf(lineItem);
            foreach (var ship in orderForm.Shipments.ToArray())
            {
                IEnumerable<int> listIdx = ship.LineItemIndexes.Select(c => Convert.ToInt32(c)).Where(i => i > lineItemIndex);
                foreach (int idx in listIdx)
                {
                    ship.RemoveLineItemIndex(idx);
                    ship.AddLineItemIndex(idx - 1);
                }
            }
        }

        /// <summary>
        /// Calculate new line item quantity from inventory/in-store inventory
        /// </summary>
        /// <param name="lineItem">line item</param>
        /// <param name="changeQtyReason">messages explain to clients why item's quantity is changed</param>
        /// <param name="shipment">shipment</param>
        /// <returns>new line item quantity</returns>
        protected decimal GetNewLineItemQty(LineItem lineItem, List<string> changeQtyReason, Shipment shipment)
        {
            var newLineItemQty = shipment != null ? Shipment.GetLineItemQuantity(shipment, lineItem.LineItemId) : lineItem.Quantity;

            if (newLineItemQty < lineItem.MinQuantity)
            {
                newLineItemQty = lineItem.MinQuantity;
                changeQtyReason.Add("by Min Quantity setting");
            }
            else if (newLineItemQty > lineItem.MaxQuantity)
            {
                newLineItemQty = lineItem.MaxQuantity;
                changeQtyReason.Add("by Max Quantity setting");
            }

            var entryRow = GetEntryRowForLineItem(lineItem);
            if (!InventoryTrackingEnabled(entryRow))
            {
                return newLineItemQty;
            }

            string warehouseCode = shipment != null && !string.IsNullOrEmpty(shipment.WarehouseCode) ? shipment.WarehouseCode : lineItem.WarehouseCode;

            IWarehouse warehouse = WarehouseRepository.Get(warehouseCode);
            if (warehouse == null || !warehouse.IsActive)
            {
                changeQtyReason.Add("by inactive warehouse");
                return 0;
            }

            var inventoryRecord = InventoryService.Get(lineItem.Code, warehouseCode);
            if (inventoryRecord == null)
            {
                changeQtyReason.Add("by inactive warehouse");
                return 0;
            }

            // In case inventory status is Disable, always using newLineItemQty.
            if (!inventoryRecord.IsTracked)
            {
                return newLineItemQty;
            }

            var requestDate = FrameworkContext.Current.CurrentDateTime;
            var allowPreorder = inventoryRecord.PreorderAvailableUtc > SafeBeginningOfTime && requestDate >= inventoryRecord.PreorderAvailableUtc;
            var allowBackOrder = inventoryRecord.BackorderAvailableQuantity > 0 && inventoryRecord.BackorderAvailableUtc <= requestDate;

            if (requestDate >= inventoryRecord.PurchaseAvailableUtc)
            {
                var availableQuantity = inventoryRecord.PurchaseAvailableQuantity > 0 ? inventoryRecord.PurchaseAvailableQuantity : 0;
                if (newLineItemQty > availableQuantity)
                {
                    if (allowBackOrder)
                    {
                        availableQuantity = inventoryRecord.BackorderAvailableQuantity + availableQuantity;
                        if (newLineItemQty > availableQuantity)
                        {
                            newLineItemQty = availableQuantity;
                            changeQtyReason.Add("by BackOrder quantity");
                        }
                    }
                    else
                    {
                        newLineItemQty = availableQuantity;
                        changeQtyReason.Add("by current available quantity");
                    }
                }
            }
            else if (allowPreorder)
            {
                if (newLineItemQty > inventoryRecord.PreorderAvailableQuantity)
                {
                    newLineItemQty = inventoryRecord.PreorderAvailableQuantity;
                    changeQtyReason.Add("by Preorder quantity");
                }
            }
            else
            {
                changeQtyReason.Add("by Entry is not available");
                return 0;
            }

            return newLineItemQty;
        }

        protected bool CancelOperationKeys(Shipment shipment)
        {
            var itemIndexStart = 0;
            var requestItems = shipment.OperationKeysMap.SelectMany(c => c.Value).Select(key =>
                    new InventoryRequestItem()
                    {
                        ItemIndex = itemIndexStart++,
                        OperationKey = key,
                        RequestType = InventoryRequestType.Cancel
                    }).ToList();

            if (requestItems.Any())
            {
                InventoryService.Request(new InventoryRequest(DateTime.UtcNow, requestItems, null));
                shipment.ClearOperationKeys();
            }
            return true;
        }
    }
}