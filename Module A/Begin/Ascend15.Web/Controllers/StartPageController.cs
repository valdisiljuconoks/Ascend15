using System.Web.Mvc;
using Ascend15.Models.Pages;
using EPiServer.Web.Mvc;

namespace Ascend15.Controllers
{
    public class StartPageController : PageController<StartPage>
    {
        public ActionResult Index(StartPage currentPage)
        {
            return View(currentPage);
        }
    }
}
