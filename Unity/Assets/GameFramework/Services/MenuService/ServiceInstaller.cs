using UnityEngine;
using System.Collections;
using Zenject;

namespace Service.Menu{
    public class ServiceInstaller : Installer<ServiceInstaller>{
        public override void InstallBindings()
        {
            //Install service
            Container.Bind<IMenuService>().To<DefaultMenuService>().AsSingle();
            //Install commands
            CommandsInstaller.Install(Container);
        }
    }
}