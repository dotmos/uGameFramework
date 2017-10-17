using UnityEngine;
using System.Collections;
using Zenject;

namespace Service.LocalStorage{
    public class ServiceInstaller : Installer<ServiceInstaller>{
        public override void InstallBindings()
        {
            Container.Bind<ILocalStorageService>().To<DefaultLocalStorage>().AsSingle();
            CommandsInstaller.Install(Container);
        }
    }
}