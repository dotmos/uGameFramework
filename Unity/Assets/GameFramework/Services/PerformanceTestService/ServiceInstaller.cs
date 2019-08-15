
using Zenject;


namespace Service.PerformanceTest
{
    public class ServiceInstaller : Installer<ServiceInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<IPerformanceTestService>().To<PerformanceTestServiceImpl>().AsSingle();
            CommandsInstaller.Install(Container);
        }
        
    }
}
