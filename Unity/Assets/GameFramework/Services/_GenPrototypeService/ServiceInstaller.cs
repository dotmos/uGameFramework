
using Zenject;


namespace /*name:namespace*/Service.GeneratorPrototype/*endname*/
{
    public class ServiceInstaller : Installer<ServiceInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind</*name:interfaceName*/IPrototypeService/*endname*/>().To</*name:implName*/PrototypeService/*endname*/>().AsSingle();
            CommandsInstaller.Install(Container);
        }
        
    }
}
