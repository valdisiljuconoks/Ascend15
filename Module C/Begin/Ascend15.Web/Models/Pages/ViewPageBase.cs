using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;

namespace Ascend15.Models.Pages
{
    public class ViewPageBase : ViewPage { }

    public class ViewPageBase<TModel> : WebViewPage<TModel>
    {
        private Injected<IContentLoader> _loader;

        public StartPage StartPage => _loader.Service.Get<StartPage>(ContentReference.StartPage);
        public PageData CurrentPage => ServiceLocator.Current.GetInstance<PageRouteHelper>().Page;
        public CartPage CartPage => _loader.Service.Get<CartPage>(StartPage.CartPage);
        public CheckoutPage CheckoutPage => _loader.Service.Get<CheckoutPage>(CartPage.CheckoutPage);
        public ShopEntryPage ShopEntryPage => _loader.Service.Get<ShopEntryPage>(StartPage.ShopEntryPage);

        public override void Execute() { }
    }
}
