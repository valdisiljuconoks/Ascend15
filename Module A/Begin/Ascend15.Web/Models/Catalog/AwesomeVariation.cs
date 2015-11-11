using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.DataAnnotations;

namespace Ascend15.Models.Catalog
{
    [CatalogContentType(DisplayName = "AwesomeVariation", GUID = "0B23A7A0-1B9C-495D-9454-CBC3E69D6DFE", Description = "")]
    public class AwesomeVariation : VariationContent
    {
        public virtual string Model { get; set; }
        public virtual string Color { get; set; }
        public virtual string Size { get; set; }
    }
}
