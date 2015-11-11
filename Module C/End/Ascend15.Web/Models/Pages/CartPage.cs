using EPiServer.Core;
using EPiServer.DataAnnotations;

namespace Ascend15.Models.Pages
{
    [ContentType(DisplayName = "Cart Overview Page", GUID = "3b809abc-7d1a-4e64-ad92-43801f2a9c27", Description = "")]
    public class CartPage : PageData
    {
        [AllowedTypes(typeof (CheckoutPage))]
        public virtual ContentReference CheckoutPage { get; set; }
    }
}
