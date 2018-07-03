
using Zenject;


namespace Service.GameStateService
{
    public class ServiceInstaller : Installer<ServiceInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<IGameStateService>().To<GameStateServiceImpl>().AsSingle();
            CommandsInstaller.Install(Container);
        }

    }
}
