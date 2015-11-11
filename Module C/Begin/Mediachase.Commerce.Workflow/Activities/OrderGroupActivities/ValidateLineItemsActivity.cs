using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Dto;
using Mediachase.Commerce.Catalog.Managers;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.InventoryService;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.WorkflowCompatibility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mediachase.Commerce.Workflow.Activities
{
    public class ValidateLineItemsActivity : OrderGroupActivityBase
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

                // Calculate order discounts
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

        private void ValidateItems()
        {
            CatalogRelationDto relationDto = null;
            CatalogDto.CatalogRow catalogRow = null;

            var marketTester = new ExcludedCatalogEntryMarketsField();
            var orderMarket = ServiceLocator.Current.GetInstance<IMarketService>().GetMarket(OrderGroup.MarketId);
            var lineItems = OrderGroup.OrderForms.SelectMany(x => x.LineItems.ToArray());
            var validLineItems = lineItems.Where(x => x.Code != "0" && !String.IsNullOrEmpty(x.Code) && !x.Code.StartsWith("@"));

            List<LineItem> lineItemsToRemoved = new List<LineItem>();
            foreach (var lineItem in validLineItems)
            {
                var entryRow = GetEntryRowForLineItem(lineItem);

                if (entryRow == null)
                {
                    AddWarningSafe(Warnings, "LineItemCodeRemoved-" + lineItem.Id,
                        String.Format("The catalog entry code that maps to the line item {0} has been removed or changed.  The line item is no longer valid", lineItem.Code));
                    lineItemsToRemoved.Add(lineItem);
                    continue;
                }

                if (!marketTester.IsValidForMarket(entryRow, orderMarket))
                {
                    AddWarningSafe(Warnings, "LineItemRemoved-" + lineItem.LineItemId.ToString(),
                        String.Format("Item \"{0}\" has been removed from the cart because it is not available in your market.",
                        lineItem.DisplayName));
                    lineItemsToRemoved.Add(lineItem);
                    continue;
                }

                var requestDate = FrameworkContext.Current.CurrentDateTime;

                if (catalogRow == null || catalogRow.CatalogId != entryRow.CatalogId)
                {
                    var catalogDto = CatalogContext.Current.GetCatalogDto(entryRow.CatalogId);
                    catalogRow = catalogDto.Catalog.FirstOrDefault();
                }

                //check if the catalog of this entry is not available
                if (catalogRow == null || !catalogRow.IsActive || requestDate < catalogRow.StartDate || requestDate > catalogRow.EndDate)
                {
                    AddWarningSafe(Warnings, "LineItemRemoved-" + lineItem.LineItemId.ToString(),
                                                               String.Format("Item \"{0}\" has been removed from the cart because the catalog of this entry is not available.",
                                                               lineItem.DisplayName));
                    lineItemsToRemoved.Add(lineItem);
                    continue;
                }

                relationDto = CatalogContext.Current.GetCatalogRelationDto(entryRow.CatalogEntryId);
                // populate item
                lineItem.Catalog = catalogRow.Name;
                lineItem.ParentCatalogEntryId = GetParentCatalogEntryId(entryRow.CatalogEntryId, relationDto);

                //Variation Info
                PopulateVariationInfo(entryRow, lineItem);

                if (string.IsNullOrEmpty(lineItem.WarehouseCode))
                {
                    // This case was passed because lineItem.WarehouseCode will be set in next activity - GetFulfillmentWarehouseActivity
                    continue;
                }

                var inventoryRecord = InventoryService.Get(lineItem.Code, lineItem.WarehouseCode);

                if (inventoryRecord == null)
                {
                    AddWarningSafe(Warnings, "LineItemCodeRemoved-" + lineItem.Id,
                        String.Format("The catalog entry code that maps to the line item {0} has been removed or changed.  The line item is no longer valid", lineItem.Code));
                    lineItemsToRemoved.Add(lineItem);
                    continue;
                }

                // Get minimum date that the entry is available. It should be is minimum date of preorder date and start date
                var minAvailableDate = entryRow.StartDate;
                if (inventoryRecord != null && inventoryRecord.PreorderAvailableUtc > SafeBeginningOfTime && minAvailableDate > inventoryRecord.PreorderAvailableUtc)
                {
                    minAvailableDate = inventoryRecord.PreorderAvailableUtc;
                }

                //check if the entry is not available
                if (!entryRow.IsActive || requestDate < minAvailableDate || requestDate > entryRow.EndDate)
                {
                    AddWarningSafe(Warnings, "LineItemRemoved-" + lineItem.LineItemId.ToString(),
                                            String.Format("Item \"{0}\" has been removed from the cart because it is not available.",
                                            lineItem.DisplayName));
                    lineItemsToRemoved.Add(lineItem);
                    continue;
                }

                //Inventory Info
                PopulateInventoryInfo(inventoryRecord, lineItem);
            }

            if (lineItemsToRemoved.Count > 0)
            {
                // remove lineitem from shipment
                foreach (OrderForm form in OrderGroup.OrderForms)
                {
                    foreach (var lineItem in lineItemsToRemoved)
                    {
                        form.RemoveLineItemFromShipments(lineItem);
                    }
                }

                // remove lineitem from order
                foreach (var lineItem in lineItemsToRemoved)
                {
                    lineItem.Delete();
                }
            }
        }

        private void PopulateInventoryInfo(InventoryRecord inventoryRecord, LineItem lineItem)
        {
            lineItem.AllowBackordersAndPreorders = inventoryRecord.BackorderAvailableQuantity > 0 | inventoryRecord.PreorderAvailableUtc > SafeBeginningOfTime;
            lineItem.BackorderQuantity = inventoryRecord.BackorderAvailableQuantity;
            lineItem.InStockQuantity = inventoryRecord.PurchaseAvailableQuantity + inventoryRecord.AdditionalQuantity;
            lineItem.PreorderQuantity = inventoryRecord.PreorderAvailableQuantity;
            lineItem.InventoryStatus = inventoryRecord.IsTracked ? 1 : 0;
        }

        private void PopulateVariationInfo(CatalogEntryDto.CatalogEntryRow entryRow, LineItem lineItem)
        {
            CatalogEntryDto.VariationRow variationRow = entryRow.GetVariationRows().FirstOrDefault();

            if (variationRow != null)
            {
                lineItem.MaxQuantity = variationRow.MaxQuantity;
                lineItem.MinQuantity = variationRow.MinQuantity;
                CustomerContact customerContact = CustomerContext.Current.GetContactById(lineItem.Parent.Parent.CustomerId);

                Money? newListPrice = GetItemPrice(entryRow, lineItem, customerContact);
                if (newListPrice.HasValue)
                {
                    Money oldListPrice = new Money(Math.Round(lineItem.ListPrice, 2), lineItem.Parent.Parent.BillingCurrency);

                    if (oldListPrice != newListPrice.Value)
                    {
                        AddWarningSafe(Warnings,
                            "LineItemPriceChange-" + lineItem.Parent.LineItems.IndexOf(lineItem).ToString(),
                            string.Format("Price for \"{0}\" has been changed from {1} to {2}.", lineItem.DisplayName, oldListPrice.ToString(), newListPrice.ToString()));

                        // Set new price on line item.
                        lineItem.ListPrice = newListPrice.Value.Amount;
                        if (lineItem.Parent.Parent.ProviderId.ToLower().Equals("frontend"))
                        {
                            lineItem.PlacedPrice = newListPrice.Value.Amount;
                        }
                    }
                }
            }
        }

        private static string GetParentCatalogEntryId(int catalogEntryId, CatalogRelationDto relationDto)
        {
            string retVal = null;
            var entryRelationRows = relationDto.CatalogEntryRelation.Select(String.Format("ChildEntryId={0}", catalogEntryId)).Cast<CatalogRelationDto.CatalogEntryRelationRow>();
            if (entryRelationRows.Count() > 0)
            {
                //This method use catalog entry id, not code, so it'll not use the cache we have with CatalogEntryResponseGroup.ResponseGroup.Variations
                CatalogEntryDto parentEntryDto = CatalogContext.Current.GetCatalogEntryDto(entryRelationRows.First().ParentEntryId, new CatalogEntryResponseGroup(CatalogEntryResponseGroup.ResponseGroup.CatalogEntryInfo));
                if (parentEntryDto.CatalogEntry.Count > 0)
                {
                    retVal = parentEntryDto.CatalogEntry[0].Code;
                }
            }
            return retVal;
        }
    }
}