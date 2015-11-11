using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Dto;
using Mediachase.Commerce.Catalog.Managers;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.WorkflowCompatibility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mediachase.Commerce.Workflow.Activities
{
    public class CalculateTaxActivity : OrderGroupActivityBase
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

                // Calculate taxes
                this.CalculateTaxes();

                // Retun the closed status indicating that this activity is complete.
                return ActivityExecutionStatus.Closed;
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(this.GetType()).Error(ex.Message, ex);
                // An unhandled exception occured.  Throw it back to the WorkflowRuntime.
                throw;
            }
        }

        /// <summary>
        /// Calculates the sale and shipping taxes.
        /// </summary>
        private void CalculateTaxes()
        {
            // Get the property, since it is expensive process, make sure to get it once
            OrderGroup order = OrderGroup;

            foreach (OrderForm form in order.OrderForms)
            {

                decimal totalTaxes = 0;
                foreach (Shipment shipment in form.Shipments)
                {
                    var shippingTaxes = new List<ITaxValue>();
                    decimal shippingCost = shipment.ShippingSubTotal - shipment.ShippingDiscountAmount;
                    
                    var lineItems = Shipment.GetShipmentLineItems(shipment);
                    // Calculate sales and shipping taxes per items
                    foreach (LineItem item in lineItems)
                    {
                        // Try getting an address
                        OrderAddress address = GetAddressByName(form, shipment.ShippingAddressId);
                        if (address != null) // no taxes if there is no address
                        {
                            // Try getting an entry
                            CatalogEntryDto entryDto = CatalogContext.Current.GetCatalogEntryDto(item.Code, new CatalogEntryResponseGroup(CatalogEntryResponseGroup.ResponseGroup.Variations));
                            if (entryDto.CatalogEntry.Count > 0) // no entry, no tax category, no tax
                            {
                                CatalogEntryDto.VariationRow[] variationRows = entryDto.CatalogEntry[0].GetVariationRows();
                                if (variationRows.Length > 0)
                                {
                                    string taxCategory = CatalogTaxManager.GetTaxCategoryNameById(variationRows[0].TaxCategoryId);
                                    IMarket market = ServiceLocator.Current.GetInstance<IMarketService>().GetMarket(order.MarketId);
                                    TaxValue[] taxes = OrderContext.Current.GetTaxes(Guid.Empty, taxCategory, market.DefaultLanguage.Name, address.CountryCode, address.State, address.PostalCode, address.RegionCode, String.Empty, address.City);

                                    if (taxes.Length > 0)
                                    {
                                        var quantity = Shipment.GetLineItemQuantity(shipment, item.LineItemId);

                                        // price exclude tax for 1 line item
                                        var lineItemPricesExcTax = item.PlacedPrice - (item.OrderLevelDiscountAmount + item.LineItemDiscountAmount) / item.Quantity;
                                        // price exclude tax for item in shipment
                                        var shipmentItemsPricesExcTax = lineItemPricesExcTax * quantity;

                                        totalTaxes += CalculateSalesTax(taxes, shipmentItemsPricesExcTax);
                                        shippingTaxes.AddRange(GetShippingTax(taxes, shippingTaxes));
                                    }
                                }
                            }
                        }
                    }
                    shipment.ShippingTax = CalculateShippingTaxes(shippingTaxes, shippingCost);
                    totalTaxes += shipment.ShippingTax;
                }

                form.TaxTotal = Math.Round(totalTaxes, 2);
            }
        }

        /// <summary>
        /// gets the shipping taxes not in shippingTaxes  
        /// </summary>
        /// <param name="taxes">the taxes for the item</param>
        /// <param name="shippingTaxes">the shippingtaxes for the order</param>
        /// <returns></returns>
        private static IEnumerable<ITaxValue> GetShippingTax(IEnumerable<ITaxValue> taxes, IEnumerable<ITaxValue> shippingTaxes)
        {
            return taxes.Where(x => x.TaxType == TaxType.ShippingTax && shippingTaxes.FirstOrDefault(y => y.Name.Equals(x.Name)) == null);
        }
        /// <summary>
        /// Calculate the sales tax 
        /// </summary>
        /// <param name="taxes">the taxes</param>
        /// <param name="shipmentItemPriceWithoutTax">the item price excludin taxes</param>
        /// <returns>the sales tax value</returns>
        private static decimal CalculateSalesTax(IEnumerable<ITaxValue> taxes, decimal shipmentItemPriceWithoutTax)
        {
            return taxes.Where(x => x.TaxType == TaxType.SalesTax).Sum(x => shipmentItemPriceWithoutTax * ((decimal)x.Percentage / 100));
        }
        /// <summary>
        /// Calculate the shipping taxes
        /// </summary>
        /// <param name="taxes">the taxes</param>
        /// <param name="shippingCost">the shipping cost</param>
        /// <returns>the shipping tax value</returns>
        private decimal CalculateShippingTaxes(IEnumerable<ITaxValue> taxes, decimal shippingCost)
        {
            var shippingTax = 0m;
            taxes.Where(x => x.TaxType == TaxType.ShippingTax).ToList().ForEach(x => shippingTax += (shippingCost * ((decimal)x.Percentage / 100)));

            return Math.Round(shippingTax, 2);
        }
        /// <summary>
        /// Gets the name of the address by name.
        /// </summary>
        /// <param name="form">The form.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        private OrderAddress GetAddressByName(OrderForm form, string name)
        {
            foreach (OrderAddress address in form.Parent.OrderAddresses)
            {
                if (address.Name.Equals(name))
                    return address;
            }

            return null;
        }
    }
}
