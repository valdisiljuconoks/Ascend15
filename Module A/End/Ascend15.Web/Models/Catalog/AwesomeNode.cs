using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAnnotations;

namespace Ascend15.Models.Catalog
{
    [CatalogContentType(DisplayName = "AwesomeNode", GUID = "91f873b1-63d6-41fe-a0e3-b572029d966a", MetaClassName = "Awesome_Node")]
    public class AwesomeNode : NodeContent
    {
        [CultureSpecific]
        [IncludeInDefaultSearch]
        [Searchable]
        [Tokenize]
        public virtual XhtmlString MainBody { get; set; }
    }
}
