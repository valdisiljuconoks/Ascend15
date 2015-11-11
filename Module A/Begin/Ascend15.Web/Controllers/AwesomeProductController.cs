using System.Collections.Generic;
using System.Web.Mvc;
using Ascend15.Extensions;
using Ascend15.Models.Catalog;
using EPiServer.Core;
using EPiServer.Web.Mvc;

namespace Ascend15.Controllers
{
    public class AwesomeProductController : ContentController<AwesomeProduct>
    {
        public ActionResult Index(PageData currentPage, AwesomeProduct currentContent)
        {
            var variations = currentContent.GetVariations<AwesomeVariation>();

            var model = new AwesomeProductViewModel(variations);
            return View(model);
        }
    }

    public class AwesomeProductViewModel
    {
        public AwesomeProductViewModel(IEnumerable<AwesomeVariation> variations)
        {
            Variations = variations;
        }

        public IEnumerable<AwesomeVariation> Variations { get; set; }
    }
}
