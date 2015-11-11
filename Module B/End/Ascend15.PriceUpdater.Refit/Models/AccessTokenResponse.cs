using Newtonsoft.Json;

namespace Ascend15.PriceUpdater.Refit.Models
{
    public class AccessTokenResponse
    {
        [JsonProperty("access_token")]
        public string Token { get; set; }

        [JsonProperty("token_type")]
        public string Type { get; set; }

        [JsonProperty("expires_in")]
        public int Expires { get; set; }
    }
}