using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Zenject;

namespace Service.Backend{
    public class ServiceInstaller : Installer<ServiceInstaller> {

    	public override void InstallBindings()
        {
            /*
            #if !SERVER
            PlayfabService _serviceInstance = Container.Instantiate<PlayfabService>(new List<object>(){"XXXX"});
            #else
            //NEVER EVERY expose the secret key to the client!
            PlayfabService _serviceInstance = Container.Instantiate<PlayfabService>(new List<object>(){"XXXX", "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX"});
            #endif

            Container.Bind<IBackendService>().FromInstance(_serviceInstance);
            */
            CommandsInstaller.Install(Container);
        }
    }
}