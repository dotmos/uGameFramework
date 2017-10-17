using UnityEngine;
using System.Collections;
using Zenject;

namespace Service.CloudStorage{
    public class ServiceInstaller : Installer<ServiceInstaller>{
        public override void InstallBindings()
        {
            Container.Bind<ICloudStorageService>().To<DefaultCloudStorage>().AsSingle();
            CommandsInstaller.Install(Container);
        }
    }
}