using System.Collections.Generic;
using System.Web.Mvc;
using Ascend15.Models.Pages;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Web.Mvc;
using Mediachase.Commerce.Catalog;

namespace Ascend15.Controllers
{
    public class ShopEntryPageController : PageController<ShopEntryPage>
    {
        private readonly ReferenceConverter _converter;
        private readonly IContentLoader _loader;

        public ShopEntryPageController(IContentLoader loader, ReferenceConverter converter)
        {
            _loader = loader;
            _converter = converter;
        }

        public ActionResult Index(ShopEntryPage currentPage)
        {
            var catalogs = _loader.GetChildren<CatalogContent>(_converter.GetRootLink());

            var model = new ShopEntryPageViewModel(catalogs);

            return View(model);
        }
    }

    public class ShopEntryPageViewModel {
        public IEnumerable<CatalogContent> Catalogs { get; set; }

        public ShopEntryPageViewModel(IEnumerable<CatalogContent> catalogs)
        {
            Catalogs = catalogs;
        }
    }
}
