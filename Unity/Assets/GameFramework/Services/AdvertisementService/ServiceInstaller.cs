using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Zenject;

namespace Service.Advertisement{
    public class ServiceInstaller : Installer<ServiceInstaller> {

    	public override void InstallBindings()
        {
            UnityAdsService adService = Container.Instantiate<UnityAdsService>(new List<object>(){"1061929", "1061930", true});
            Container.Bind<IAdvertisementService>().FromInstance(adService);

            CommandsInstaller.Install(Container);
        }
    }
}