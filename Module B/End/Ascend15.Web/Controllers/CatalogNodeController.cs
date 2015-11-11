using System.Web.Mvc;
using Ascend15.Models.Catalog;
using Ascend15.Models.ViewModels;
using EPiServer;
using EPiServer.Commerce.Catalog;
using EPiServer.Core;

namespace Ascend15.Controllers
{
    public class CatalogNodeController : CatalogContentBaseController<AwesomeNode>
    {
        public CatalogNodeController(IContentLoader contentLoader,
                                     AssetUrlResolver assetUrlResolver,
                                     ThumbnailUrlResolver thumbnailUrlResolver)
            : base(contentLoader, assetUrlResolver, thumbnailUrlResolver) { }

        public ActionResult Index(PageData currentPage, AwesomeNode currentContent)
        {
            var model = new CatalogNodeViewModel
            {
                Nodes = GetNodes(currentContent.ContentLink),
                Products = GetProducts(currentContent.ContentLink)
            };

            return View(model);
        }
    }
}
