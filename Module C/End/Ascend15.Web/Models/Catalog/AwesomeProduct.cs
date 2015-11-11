using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAnnotations;

namespace Ascend15.Models.Catalog
{
    [CatalogContentType(DisplayName = "AwesomeProduct", GUID = "6ba40290-6755-456f-a42b-655782caf6f8", MetaClassName = "Awesome_Product")]
    public class AwesomeProduct : ProductContent
    {
        [CultureSpecific]
        [IncludeInDefaultSearch]
        [Searchable]
        [Tokenize]
        public virtual XhtmlString MainBody { get; set; }

        public virtual string Brand { get; set; }
    }
}
