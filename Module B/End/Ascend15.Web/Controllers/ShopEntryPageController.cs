using System.Linq;
using System.Web.Mvc;
using Ascend15.Extensions;
using Ascend15.Models.Domain;
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
        private readonly IContentRepository _repository;

        public ShopEntryPageController(IContentRepository repository, ReferenceConverter converter)
        {
            _repository = repository;
            _converter = converter;
        }

        public ActionResult Index(ShopEntryPage currentPage)
        {
            var catalogs = _repository.GetChildren<CatalogContent>(_converter.GetRootLink()).Select(c => new NameAndLinkPair
            {
                Name = c.Name,
                Url = c.GetUrl()
            });

            return View(catalogs);
        }
    }
}
