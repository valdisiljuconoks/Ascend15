using System.Web.Mvc;
using Ascend15.Models.ViewModels;
using EPiServer;
using EPiServer.Commerce.Catalog;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;

namespace Ascend15.Controllers
{
    public class CatalogController : CatalogContentBaseController<CatalogContent>
    {
        public CatalogController(IContentLoader contentLoader,
                                 AssetUrlResolver assetUrlResolver,
                                 ThumbnailUrlResolver thumbnailUrlResolver)
            : base(contentLoader, assetUrlResolver, thumbnailUrlResolver) { }

        public ActionResult Index(PageData currentPage, CatalogContent currentContent)
        {
            var model = new CatalogViewModel
            {
                Nodes = GetNodes(currentContent.ContentLink)
            };

            return View(model);
        }
    }
}
