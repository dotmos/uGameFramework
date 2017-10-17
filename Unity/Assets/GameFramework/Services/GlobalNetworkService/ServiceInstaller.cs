using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Zenject;

namespace Service.GlobalNetwork{
    public class ServiceInstaller : Installer<ServiceInstaller>{

    	public override void InstallBindings()
        {
            #if SERVER
//            Container.Bind<IServer>().ToSingleGameObject<ServerBase>("NetworkServerService");
            Container.Bind<IServer>().To<ServerBase>().FromGameObject().WithGameObjectName("NetworkServerService").AsSingle();
            #else
//            Container.Bind<IClient>().ToSingleGameObject<ClientBase>("NetworkClientService");
            Container.Bind<IClient>().To<ClientBase>().FromGameObject().WithGameObjectName("NetworkClientService").AsSingle();
            #endif

            CommandsInstaller.Install(Container);
        }
    }
}