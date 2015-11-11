using Refit;

namespace Ascend15.PriceUpdater.Refit.Models
{
    public class AccessTokenRequest
    {
        public AccessTokenRequest(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public string Username { get; set; }
        public string Password { get; set; }

        [AliasAs("grant_type")]
        public string GrantType => "password";
    }
}