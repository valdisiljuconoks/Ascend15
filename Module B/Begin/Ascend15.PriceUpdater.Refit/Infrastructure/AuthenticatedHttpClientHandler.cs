using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Ascend15.PriceUpdater.Refit.Models;
using Refit;

namespace Ascend15.PriceUpdater.Refit.Infrastructure
{
    internal class AuthenticatedHttpClientHandler : HttpClientHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // See if the request has an authorize header
            var auth = request.Headers.Authorization;
            if (auth != null)
            {
                var token = await GetTokenAsync().ConfigureAwait(false);
                request.Headers.Authorization = new AuthenticationHeaderValue(auth.Scheme, token);
            }

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        private async Task<string> GetTokenAsync()
        {
            var client = RestService.For<ICommerceServiceApi>("https://localhost:44300");
            var token = await client.GetTokenAsync(new AccessTokenRequest("admin", "store"));
            return token.Token;
        }
    }
}
