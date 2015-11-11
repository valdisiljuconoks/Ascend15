using System;
using System.Linq;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Mediachase.Commerce.Catalog;

namespace Ascend15.Extensions
{
    public static class VariationContentExtensions
    {
        public static VariationContent GetVariationByCode(this string code)
        {
            if (code == null)
            {
                throw new ArgumentNullException(nameof(code));
            }

            var converter = ServiceLocator.Current.GetInstance<ReferenceConverter>();
            var loader = ServiceLocator.Current.GetInstance<IContentLoader>();
            return loader.Get<VariationContent>(converter.GetContentLink(code));
        }

        public static VariationContent GetVariationByCode<TVariation>(this string code) where TVariation : VariationContent
        {
            var result = GetVariationByCode(code);
            return result as TVariation;
        }

        public static ProductContent GetProduct(this VariationContent variation)
        {
            if (variation == null)
            {
                throw new ArgumentNullException(nameof(variation));
            }

            var relations = variation.GetParentProducts().ToList();
            if (!relations.Any())
            {
                throw new ArgumentException($"No products defined for variation '{variation.Code}'");
            }

            var loader = ServiceLocator.Current.GetInstance<IContentLoader>();
            return loader.Get<ProductContent>(relations.First());
        }

        public static string GetVariationIconUrl(this VariationContent variation, string groupName)
        {
            var urlResolver = ServiceLocator.Current.GetInstance<UrlResolver>();
            var collection = ((IAssetContainer) variation).CommerceMediaCollection;

            return collection.Any(m => m.GroupName.Equals(groupName, StringComparison.InvariantCultureIgnoreCase))
                       ? urlResolver.GetUrl(collection.First(m => m.GroupName.Equals(groupName, StringComparison.InvariantCultureIgnoreCase)).AssetLink)
                       : null;
        }
    }
}
