
using Zenject;


namespace Service.DevUIService
{
    public class ServiceInstaller : Installer<ServiceInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<IDevUIService>().To<DevUIServiceImpl>().AsSingle();
            CommandsInstaller.Install(Container);
        }
        
    }
}
