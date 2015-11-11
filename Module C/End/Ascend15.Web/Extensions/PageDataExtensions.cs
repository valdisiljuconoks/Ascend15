using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;

namespace Ascend15.Extensions
{
    public static class PageDataExtensions
    {
        public static string GetUrl(this PageData content)
        {
            return ServiceLocator.Current.GetInstance<UrlResolver>().GetUrl(content);
        }
    }
}
