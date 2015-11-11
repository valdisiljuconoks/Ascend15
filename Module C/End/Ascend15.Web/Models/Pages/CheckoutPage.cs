using EPiServer.Core;
using EPiServer.DataAnnotations;

namespace Ascend15.Models.Pages
{
    [ContentType(DisplayName = "Checkout Page", GUID = "CB8D7AA5-184A-4CC6-9602-FE373F10845A", Description = "")]
    public class CheckoutPage : PageData
    {
        [AllowedTypes(typeof (ConfirmationPage))]
        public virtual ContentReference ConfirmationPage { get; set; }
    }
}
