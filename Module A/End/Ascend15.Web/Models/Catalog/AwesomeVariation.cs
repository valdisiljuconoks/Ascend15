using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAnnotations;

namespace Ascend15.Models.Catalog
{
    [CatalogContentType(DisplayName = "AwesomeVariation", GUID = "a7ea6ae0-1c2d-4a7d-8abf-e1b9cb0b7eaf", MetaClassName = "Awesome_Variation")]
    public class AwesomeVariation : VariationContent
    {
        [CultureSpecific]
        [IncludeInDefaultSearch]
        [Searchable]
        [Tokenize]
        public virtual XhtmlString MainBody { get; set; }

        [IncludeInDefaultSearch]
        [CultureSpecific]
        public virtual string Size { get; set; }

        [IncludeInDefaultSearch]
        [CultureSpecific]
        public virtual string Color { get; set; }
    }
}
