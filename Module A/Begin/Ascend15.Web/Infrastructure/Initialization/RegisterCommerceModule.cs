using System.Web.Routing;
using Ascend15.Models.Pages;
using EPiServer;
using EPiServer.Commerce.Routing;
using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using InitializationModule = EPiServer.Commerce.Initialization.InitializationModule;

namespace Ascend15.Infrastructure.Initialization
{
    [InitializableModule]
    [ModuleDependency(typeof (InitializationModule))]
    public class RegisterCommerceModule : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            //CatalogRouteHelper.MapDefaultHierarchialRouter(RouteTable.Routes, GetShopStartingPoint, false);
        }

        public void Uninitialize(InitializationEngine context) { }

        private static ContentReference GetShopStartingPoint()
        {
            var repo = ServiceLocator.Current.GetInstance<IContentRepository>();
            var startPage = repo.Get<StartPage>(ContentReference.StartPage);

            return startPage.ShopEntryPage == ContentReference.EmptyReference
                       ? ContentReference.StartPage
                       : startPage.ShopEntryPage;
        }
    }
}
