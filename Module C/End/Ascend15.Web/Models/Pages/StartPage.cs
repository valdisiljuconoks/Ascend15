using EPiServer.Core;
using EPiServer.DataAnnotations;

namespace Ascend15.Models.Pages
{
    [ContentType(DisplayName = "Start Page", GUID = "a707575f-d905-45c7-99bc-a81bcd81cb9c", Description = "")]
    public class StartPage : PageData
    {
        public virtual ContentReference ShopEntryPage { get; set; }

        [AllowedTypes(typeof(CartPage))]
        public virtual ContentReference CartPage { get; set; }
    }
}
