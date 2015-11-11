using System.Collections.Generic;
using System.Linq;
using Ascend15.Extensions;
using Ascend15.Models.Domain;
using EPiServer;
using EPiServer.Commerce.Catalog;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Filters;
using EPiServer.ServiceLocation;
using EPiServer.Web.Mvc;

namespace Ascend15.Controllers
{
    public class CatalogContentBaseController<T> : ContentController<T> where T : IContent
    {
        protected readonly AssetUrlResolver assetUrlResolver;
        protected readonly IContentLoader contentLoader;
        protected readonly ThumbnailUrlResolver thumbnailUrlResolver;

        public CatalogContentBaseController(IContentLoader contentLoader, AssetUrlResolver assetUrlResolver,
                                            ThumbnailUrlResolver thumbnailUrlResolver)
        {
            this.contentLoader = contentLoader;
            this.assetUrlResolver = assetUrlResolver;
            this.thumbnailUrlResolver = thumbnailUrlResolver;
        }

        protected string GetDefaultAsset(IAssetContainer target)
        {
            return assetUrlResolver.GetAssetUrl(target);
        }

        protected string GetNamedAsset(IAssetContainer target, string groupName)
        {
            return thumbnailUrlResolver.GetThumbnailUrl(target, groupName);
        }

        protected IEnumerable<NameAndLinkPair> GetNodes(ContentReference currentContent)
        {
            var nodes = FilterForVisitor.Filter(contentLoader.GetChildren<NodeContent>(currentContent));

            return nodes.Select(node => new NameAndLinkPair
            {
                Name = node.Name,
                Url = node.ContentLink.GetUrl(),
                ImageUrl = GetDefaultAsset(node as IAssetContainer),
                ImageThumbnailUrl = GetNamedAsset(node as IAssetContainer, "Thumbnail")
            }).ToList();
        }

        protected IEnumerable<NameAndLinkPair> GetProducts(ContentReference currentContent)
        {
            var products = contentLoader.GetChildren<ProductContent>(currentContent);
            var contents = (IList<IContent>)products.ToList<IContent>();

            new FilterPublished(ServiceLocator.Current.GetInstance<IPublishedStateAssessor>()).Filter(contents);
            new FilterAccess().Filter(contents);

            products = contents.OfType<ProductContent>();

            return products.Select(product =>
                                   {
                                       var productVariation = product.GetVariations().FirstOrDefault();
                                       return new NameAndLinkPair
                                       {
                                           Name = product.Name,
                                           Url = (productVariation != null ? productVariation.ContentLink : product.ContentLink).GetUrl(),
                                           ImageUrl = GetDefaultAsset(product),
                                           ImageThumbnailUrl = GetNamedAsset(product, "Thumbnail")
                                       };
                                   }).ToList();
        }
    }
}
