using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using InitializationModule = EPiServer.Web.InitializationModule;

namespace Ascend15.Infrastructure.Initialization
{
    [InitializableModule]
    [ModuleDependency(typeof (InitializationModule))]
    public class RegisterActivityFlowEventHandlers : IInitializableModule
    {
        public void Initialize(InitializationEngine context) { }

        public void Uninitialize(InitializationEngine context) { }
    }
}
