using Ascend15.Models.Pages;
using EPiServer;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web.Mvc;
using EPiServer.Web.Routing;

namespace Ascend15.Controllers
{
    public abstract class BasePageController<TPage> : PageController<TPage> where TPage : PageData
    {
        private Injected<IContentLoader> _loader;

        public StartPage StartPage => _loader.Service.Get<StartPage>(ContentReference.StartPage);
        public PageData CurrentPage => ServiceLocator.Current.GetInstance<PageRouteHelper>().Page;
    }
}
