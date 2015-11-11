using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Ascend15.PriceUpdater.Refit.Infrastructure;
using Ascend15.PriceUpdater.Refit.Models;
using Refit;

namespace Ascend15.PriceUpdater.Refit
{
    public class PriceUpdateService
    {
        public async Task ImportPricesAsync(Dictionary<string, decimal> newPrices)
        {
            var serviceFacade = GetServiceFacade();

            foreach (var newPrice in newPrices)
            {
                var existingPricesResult = await serviceFacade.GetPricesAsync(newPrice.Key);
                var existingPrices = existingPricesResult.ToList();

                if (existingPrices.Any())
                {
                    var existingPrice = existingPrices.First();
                    existingPrice.UnitPrice = newPrice.Value;
                    await serviceFacade.UpdatePriceAsync(newPrice.Key,
                                                         existingPrice.PriceValueId.Value,
                                                         existingPrice);
                }
                else
                {
                    await serviceFacade.SetPriceAsync(newPrice.Key,
                                                      new PriceValueModel
                                                      {
                                                          CatalogEntryCode = newPrice.Key,
                                                          MarketId = "DEFAULT",
                                                          CurrencyCode = "USD",
                                                          UnitPrice = newPrice.Value,
                                                          ValidFrom = DateTime.UtcNow,
                                                      });
                }
            }
        }

        private ICommerceServiceApi GetServiceFacade()
        {
            return RestService.For<ICommerceServiceApi>(new HttpClient(new AuthenticatedHttpClientHandler())
            {
                BaseAddress = new Uri("https://localhost:44300")
            });
        }

        
    }
}
