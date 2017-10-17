using UnityEngine;
using System.Collections;
using Zenject;

namespace Service.Scene{
    public class ServiceInstaller : Installer<ServiceInstaller>{
        public override void InstallBindings()
        {
            Container.Bind<ISceneService>().To<DefaultSceneService>().AsSingle();
//            CommandsInstaller.Install(Container);
            CommandsInstaller.Install(Container);
        }
    }
}