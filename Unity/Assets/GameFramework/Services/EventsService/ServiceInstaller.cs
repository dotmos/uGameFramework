using UnityEngine;
using System.Collections;
using Zenject;

namespace Service.Events{
    public class ServiceInstaller : Installer<ServiceInstaller> {
        public override void InstallBindings()
        {
            Container.Bind<IEventsService>().To<EventsService>().AsSingle();
            Container.BindAllInterfaces<IEventsService>().AsSingle();
        }
    }
}