using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.DataAnnotations;

namespace Ascend15.Models.Catalog
{
    [CatalogContentType(DisplayName = "AwesomeProduct", GUID = "FF1665ED-1B2F-4927-8166-8B272CAB2CA7", Description = "")]
    public class AwesomeProduct : ProductContent
    {
        public virtual string Brand { get; set; }
    }
}
