using System.Web.Mvc;
using Ascend15.Infrastructure.DependencyInjection;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;

namespace Ascend15.Infrastructure.Initialization
{
    [InitializableModule]
    [ModuleDependency(typeof (EPiServer.Commerce.Initialization.InitializationModule))]
    public class ConfigurableModule : IConfigurableModule
    {
        public void Initialize(InitializationEngine context) {}

        public void Uninitialize(InitializationEngine context) {}

        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            DependencyResolver.SetResolver(new StructureMapDependencyResolver(context.Container));
        }
    }
}
