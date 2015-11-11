using EPiServer.Logging;
using Mediachase.BusinessFoundation.Data;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Managers;
using Mediachase.Commerce.Engine;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.WorkflowCompatibility;
using Mediachase.MetaDataPlus.Configurator;
using System;
using System.Linq;
using System.Threading;

namespace Mediachase.Commerce.Workflow.Activities
{
    /// <summary>
    /// This activity is responsible for calculating the shipping prices for Payments defined for order group.
    /// It calls the appropriate interface defined by the shipping option table and passes the method id and Payment object.
    /// </summary>
    public class CapturePaymentActivity : OrderGroupActivityBase
    {
        private const string EventsCategory = "Handlers";
        [NonSerialized]
        private readonly ILogger Logger;

        /// <summary>
        /// Gets or sets the shipment.
        /// </summary>
        /// <value>The shipment.</value>
        [ActivityFlowContextProperty]
        public Shipment Shipment { get; set; }

        #region Public Events

        /// <summary>
        /// Occurs when [processing payment].
        /// </summary>
        public static string ProcessingPaymentEvent = "ProcessingPayment";

        /// <summary>
        /// Occurs when [processed payment].
        /// </summary>
        public static string ProcessedPaymentEvent = "ProcessedPayment";
        
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="CapturePaymentActivity"/> class.
        /// </summary>
        public CapturePaymentActivity()
        {
            Logger = LogManager.GetLogger(GetType());
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
                // Raise the ProcessingPaymentEvent event to the parent workflow
                RaiseEvent(ProcessingPaymentEvent, EventArgs.Empty);
                var orderForm = this.OrderGroup.OrderForms[0];
                if (orderForm.CapturedPaymentTotal < orderForm.Total)
                {
                    // Validate the properties at runtime
                    this.ValidateRuntime();

                    // Process payment now
                    this.ProcessPayment();
                }

                // Raise the ProcessedPaymentEvent event to the parent workflow
                RaiseEvent(ProcessedPaymentEvent, EventArgs.Empty);

                // Retun the closed status indicating that this activity is complete.
                return ActivityExecutionStatus.Closed;
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to process payment.", ex);
                // An unhandled exception occured.  Throw it back to the WorkflowRuntime.
                throw;
            }
        }

        /// <summary>
        /// Validates the order properties.
        /// </summary>
        /// <param name="validationErrors">The validation errors.</param>
        protected override void ValidateOrderProperties(ValidationErrorCollection validationErrors)
        {
            // Validate the To property
            if (this.OrderGroup == null)
            {
                ValidationError validationError = ValidationError.GetNotSetValidationError("OrderGroup");
                validationErrors.Add(validationError);
            }

            var orderForm = this.OrderGroup.OrderForms[0];
            decimal shipmentTotal = CalculateShipmentTotal();
            var totalPaid = orderForm.AuthorizedPaymentTotal + orderForm.CapturedPaymentTotal;
            if (totalPaid < shipmentTotal)
            {
                Logger.Error(String.Format("Defective authorization total."));
                ValidationError validationError = new ValidationError("Defective authorization total", 205, false);
                validationErrors.Add(validationError);
            }
        }

        /// <summary>
        /// Processes the payment.
        /// </summary>
        private void ProcessPayment()
        {
            OrderGroup order = OrderGroup;
            decimal shipmentTotal = CalculateShipmentTotal();

            //Calculate payment total
            var formPayments = order.OrderForms[0].Payments.ToArray();
            var resultingAuthorizedPayments = base.GetResultingPaymentsByTransactionType(formPayments, TransactionType.Authorization);
            var authorizedPayments = resultingAuthorizedPayments.Where(x => PaymentStatusManager.GetPaymentStatus(x) == PaymentStatus.Processed);

            //find intire authorization
            var intirePayment = authorizedPayments.OrderBy(x => x.Amount).FirstOrDefault(x => x.Amount >= shipmentTotal);
            if (intirePayment == null)
            {
                var payments = authorizedPayments.OrderByDescending(x => x.Amount);
                foreach (Payment partialPayment in payments)
                {
                    if (partialPayment.Amount < shipmentTotal)
                    {
                        DoCapture(partialPayment, partialPayment.Amount);
                        shipmentTotal -= partialPayment.Amount;
                    }
                    else
                    {
                        DoCapture(partialPayment, shipmentTotal);
                        break;
                    }
                }
            }
            else
            {
                DoCapture(intirePayment, shipmentTotal);
            }
        }

        private decimal CalculateShipmentTotal()
        {
            var retVal = Shipment.SubTotal + Shipment.ShippingSubTotal + Shipment.ShippingTax - Shipment.ShippingDiscountAmount;
            // need to calculate tax fee
            var shippingAddress = Shipment.Parent.Parent.OrderAddresses.Cast<OrderAddress>().FirstOrDefault(a => string.Equals(a.Name, Shipment.ShippingAddressId, StringComparison.Ordinal));
            foreach (var lineItem in Shipment.GetShipmentLineItems(Shipment))
            {
                // Calculate the tax for a line item
                var taxRate = GetItemSaleTax(lineItem, shippingAddress);
                var quantity = Shipment.GetLineItemQuantity(Shipment, lineItem.LineItemId);
                var itemsPricesExcTax = lineItem.PlacedPrice * quantity - (lineItem.OrderLevelDiscountAmount + lineItem.LineItemDiscountAmount);
                decimal itemTax = taxRate > 0 ? (itemsPricesExcTax * taxRate / 100) : 0;
                retVal += itemTax;
            }
            return retVal;
        }

        /// <summary>
        /// Gets the item tax in percent.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="addressByName">Name of the address by.</param>
        /// <returns>Return the tax in percent</returns>
        private decimal GetItemSaleTax(LineItem item, OrderAddress addressByName)
        {
            var itemTax = 0M;
            // Get the catalogEntryDto
            var catalogEntryDto = CatalogContext.Current.GetCatalogEntryDto(item.Code, new CatalogEntryResponseGroup(CatalogEntryResponseGroup.ResponseGroup.Variations));
            if (catalogEntryDto.CatalogEntry.Count > 0)
            {
                // Get the variationrows
                var variationRows = catalogEntryDto.CatalogEntry[0].GetVariationRows();
                if (variationRows.Length > 0)
                {
                    // Get TaxCategory
                    var taxCategoryNameById = CatalogTaxManager.GetTaxCategoryNameById(variationRows[0].TaxCategoryId);
                    var valueArray = OrderContext.Current.GetTaxes(Guid.Empty, taxCategoryNameById, Thread.CurrentThread.CurrentCulture.Name, addressByName.CountryCode, addressByName.State, addressByName.PostalCode, addressByName.RegionCode, string.Empty, addressByName.City);
                    if (valueArray.Length > 0)
                    {
                        itemTax = (decimal)valueArray.Where(v => v.TaxType == TaxType.SalesTax).Sum(v => v.Percentage);
                    }
                }
            }
            return itemTax;
        }

        private void DoCapture(Payment authorizePayment, decimal amount)
        {
            if (String.IsNullOrEmpty(authorizePayment.TransactionID))
                authorizePayment.TransactionID = Guid.NewGuid().ToString();

            Type paymentType = null;

            PaymentMethodDto paymentMethodDto = PaymentManager.GetPaymentMethod(authorizePayment.PaymentMethodId, true);
            string className = paymentMethodDto.PaymentMethod[0].PaymentImplementationClassName;

            paymentType = AssemblyUtil.LoadType(className);

            Payment payment = this.OrderGroup.OrderForms[0].Payments.AddNew(paymentType);

            foreach (MetaField field in authorizePayment.MetaClass.MetaFields)
            {
                if (!field.Name.Equals("PaymentId", StringComparison.InvariantCultureIgnoreCase))
                    payment[field.Name] = authorizePayment[field.Name];
            }

            payment.Amount = amount;
            payment.TransactionType = TransactionType.Capture.ToString();
            payment.Status = PaymentStatus.Pending.ToString();
        }
    }
}
