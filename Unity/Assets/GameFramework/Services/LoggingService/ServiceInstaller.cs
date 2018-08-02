
using Zenject;


namespace Service.LoggingService
{
    public class ServiceInstaller : Installer<ServiceInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<ILoggingService>().To<LoggingServiceImpl>().AsSingle();
            CommandsInstaller.Install(Container);
        }
        
    }
}
