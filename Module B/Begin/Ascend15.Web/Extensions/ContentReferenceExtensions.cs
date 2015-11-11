using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;

namespace Ascend15.Extensions
{
    public static class ContentReferenceExtensions
    {
        public static string GetUrl(this ContentReference content)
        {
            return ServiceLocator.Current.GetInstance<UrlResolver>().GetUrl(content);
        }
    }
}
