using UnityEngine;
using System.Collections;
using Zenject;

namespace Service.Input{
    public class ServiceInstaller : Installer<ServiceInstaller>{

    	public override void InstallBindings()
        {
            Container.Bind<IInputService>().To<InputService>().AsSingle();
            CommandsInstaller.Install(Container);
        }
    }
}