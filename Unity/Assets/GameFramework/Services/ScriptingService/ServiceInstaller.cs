
using Zenject;


namespace Service.Scripting
{
    public class ServiceInstaller : Installer<ServiceInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<IScriptingService>().To<ScriptingServiceImpl>().AsSingle();
            CommandsInstaller.Install(Container);
        }
        
    }
}
