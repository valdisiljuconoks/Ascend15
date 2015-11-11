using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Globalization;
using EPiServer.ServiceLocation;

namespace Ascend15.Extensions
{
    public static class ProductContentExtensions
    {
        public static IEnumerable<T> GetVariations<T>(this ProductContent product) where T : VariationContent
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            var loader = ServiceLocator.Current.GetInstance<IContentLoader>();
            return loader.GetItems(product.GetVariants(), ContentLanguage.PreferredCulture)
                         .Cast<T>();
        }
    }
}
