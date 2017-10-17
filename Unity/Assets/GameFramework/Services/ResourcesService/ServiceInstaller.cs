using Zenject;
using UnityEngine;
using System.Collections;

namespace Service.Resources {
    public class ServiceInstaller : Installer<ServiceInstaller>{
        public override void InstallBindings()
        {
            Container.Bind<IResourcesService>().To<DefaultResourcesService>().AsSingle();
            CommandsInstaller.Install(Container);
        }
    }
}