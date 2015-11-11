using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;

namespace Ascend15.Extensions
{
    public static class CatalogContentExtensions
    {
        public static string GetUrl(this CatalogContent content)
        {
            return ServiceLocator.Current.GetInstance<UrlResolver>().GetUrl(content.ContentLink);
        }

        public static string GetUrl(this CatalogContentBase content)
        {
            return ServiceLocator.Current.GetInstance<UrlResolver>().GetUrl(content.ContentLink);
        }
    }
}
