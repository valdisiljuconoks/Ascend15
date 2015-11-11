using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Ascend15.PriceUpdater.Refit.Infrastructure;
using Refit;

namespace Ascend15.PriceUpdater.Refit
{
    public class PriceUpdateService
    {
        public async Task ImportPricesAsync(Dictionary<string, decimal> newPrices)
        {
            var serviceFacade = GetServiceFacade();
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
