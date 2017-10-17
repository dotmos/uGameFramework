
using Zenject;


namespace Service.AsyncManager
{
    public class ServiceInstaller : Installer<ServiceInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<IAsyncManager>().To<AsyncManagerImpl>().AsSingle();
            CommandsInstaller.Install(Container);
        }
        
    }
}
