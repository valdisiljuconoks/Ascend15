using System.Collections.Generic;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Web.Mvc;

namespace Ascend15.Controllers
{
    public class CatalogContentController : ContentController<CatalogContent>
    {
        private readonly IContentLoader _loader;

        public CatalogContentController(IContentLoader loader)
        {
            _loader = loader;
        }

        public ActionResult Index(PageData currentPage, CatalogContent currentContent)
        {
            var categories = _loader.GetChildren<NodeContent>(currentContent.ContentLink);
            var model = new CatalogContentViewModel(categories);
            return View(model);
        }
    }

    public class CatalogContentViewModel
    {
        public CatalogContentViewModel(IEnumerable<NodeContent> categories)
        {
            Categories = categories;
        }

        public IEnumerable<NodeContent> Categories { get; set; }
    }
}
