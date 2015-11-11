using EPiServer.Commerce.Catalog;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.ServiceLocation;

namespace Ascend15.Extensions
{
    public static class AssetContainerExtensions
    {
        public static string GetDefaultAsset(this IAssetContainer target)
        {
            return ServiceLocator.Current.GetInstance<AssetUrlResolver>().GetAssetUrl(target);
        }
    }
}
