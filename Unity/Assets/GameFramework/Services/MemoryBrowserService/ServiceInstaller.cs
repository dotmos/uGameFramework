
using Zenject;


namespace Service.MemoryBrowserService
{
    public class ServiceInstaller : Installer<ServiceInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<IMemoryBrowserService>().To<MemoryBrowserServiceImpl>().AsSingle();
            CommandsInstaller.Install(Container);
        }
        
    }
}
