using System.Collections.Generic;
using System.Threading.Tasks;
using Ascend15.PriceUpdater.Refit.Models;
using Refit;

namespace Ascend15.PriceUpdater.Refit
{
    public interface ICommerceServiceApi
    {
        [Get("/episerverapi/version")]
        Task<string> GetVersionAsync();

        [Post("/episerverapi/token")]
        Task<AccessTokenResponse> GetTokenAsync(
            [Body(BodySerializationMethod.UrlEncoded)] AccessTokenRequest request);

        [Get("/episerverapi/commerce/entries/{entryCode}/prices")]
        [Headers("Authorization: Bearer")]
        Task<IEnumerable<PriceValueModel>> GetPricesAsync(string entryCode);

        [Post("/episerverapi/commerce/entries/{entryCode}/prices")]
        [Headers("Authorization: Bearer")]
        Task<string> SetPriceAsync(string entryCode, [Body] PriceValueModel request);

        [Put("/episerverapi/commerce/entries/{entryCode}/prices/{priceId}")]
        [Headers("Authorization: Bearer")]
        Task<string> UpdatePriceAsync(string entryCode, long priceId, [Body] PriceValueModel existingPrice);
    }
}
