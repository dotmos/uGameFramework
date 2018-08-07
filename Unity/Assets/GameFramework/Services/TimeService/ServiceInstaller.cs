
using Zenject;


namespace Service.TimeService
{
    public class ServiceInstaller : Installer<ServiceInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<ITimeService>().To<TimeServiceImpl>().AsSingle();
            CommandsInstaller.Install(Container);
        }
        
    }
}
