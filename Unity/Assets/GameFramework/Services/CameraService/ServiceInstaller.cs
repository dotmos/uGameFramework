using UnityEngine;
using System.Collections;
using Zenject;

namespace Service.Camera{
    public class ServiceInstaller : Installer<ServiceInstaller> {

    	public override void InstallBindings()
        {
//            CameraService _cameraService = Container.InstantiateComponentOnNewGameObject<CameraService>("CameraService");
            CameraService _cameraService = Container.InstantiateComponent<CameraService>( Container.CreateEmptyGameObject("CameraService") );
            Container.Bind<ICameraService>().To<CameraService>().FromInstance(_cameraService);
            CommandsInstaller.Install(Container);
        }
    }
}