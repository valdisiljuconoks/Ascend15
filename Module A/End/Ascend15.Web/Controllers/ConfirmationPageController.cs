using System.Web.Mvc;
using Ascend15.Models.Pages;
using EPiServer.Web.Mvc;

namespace Ascend15.Controllers
{
    public class ConfirmationPageController : PageController<ConfirmationPage>
    {
        public ActionResult Index(ConfirmationPage currentPage)
        {
            return View(currentPage);
        }
    }
}
