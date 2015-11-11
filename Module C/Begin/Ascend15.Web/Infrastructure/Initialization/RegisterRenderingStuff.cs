using System.Web.Optimization;
using Ascend15.Infrastructure.Bundling;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using InitializationModule = EPiServer.Commerce.Initialization.InitializationModule;

namespace Ascend15.Infrastructure.Initialization
{
    [InitializableModule]
    [ModuleDependency(typeof (InitializationModule))]
    public class RegisterRenderingStuff : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        public void Uninitialize(InitializationEngine context) { }
    }
}
