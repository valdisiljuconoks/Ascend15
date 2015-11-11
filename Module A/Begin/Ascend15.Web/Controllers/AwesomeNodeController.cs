using System.Collections.Generic;
using System.Web.Mvc;
using Ascend15.Models.Catalog;
using EPiServer;
using EPiServer.Core;
using EPiServer.Web.Mvc;

namespace Ascend15.Controllers
{
    public class AwesomeNodeController : ContentController<AwesomeNode>
    {
        private readonly IContentLoader _loader;

        public AwesomeNodeController(IContentLoader loader)
        {
            _loader = loader;
        }

        public ActionResult Index(PageData currentPage, AwesomeNode currentContent)
        {
            var products = _loader.GetChildren<AwesomeProduct>(currentContent.ContentLink);

            var model = new AwesomeNodeViewModel(products);
            return View(model);
        }
    }

    public class AwesomeNodeViewModel
    {
        public AwesomeNodeViewModel(IEnumerable<AwesomeProduct> products)
        {
            Products = products;
        }

        public IEnumerable<AwesomeProduct> Products { get; set; }
    }
}
