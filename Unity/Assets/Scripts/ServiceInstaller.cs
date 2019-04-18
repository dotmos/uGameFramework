using UnityEngine;
using Zenject;

public class ServiceInstaller : MonoInstaller {

	public override void InstallBindings()
    {
        Install(Container);
    }

    public static void Install(DiContainer Container) {
        // register execution wrappers, that can wrap logic around of the wrapped logic. e.g. for performance tests, exception handling and more?
        // register different wrappers e.g. for debugging and release
        Container.Bind<IReactiveExecutionWrapper>().To<DefaultReactiveExecutionWrapper>().AsSingle();
        Container.Bind<IExecutionWrapper>().To<DefaultExecutionWrapper>().AsSingle();

        Debug.Log("Installing Services ...");

        //EventService
        Service.Events.ServiceInstaller.Install(Container);

        //Serializer 
        Service.Serializer.ServiceInstaller.Install(Container);

        //ThreadManager
        Service.AsyncManager.ServiceInstaller.Install(Container);

        // Logging
        Service.LoggingService.ServiceInstaller.Install(Container);

        // Time-Service
        Service.TimeService.ServiceInstaller.Install(Container);

        //optional services (see "Services" folder for more, or write your own)
        //Input
        Service.Input.ServiceInstaller.Install(Container);

        // FileSystem
        Service.FileSystem.ServiceInstaller.Install(Container);

        //Scene Service
        Service.Scene.ServiceInstaller.Install(Container);

        //Console 
        Service.Scripting.ServiceInstaller.Install(Container);

        //GameStateService
        Service.GameStateService.ServiceInstaller.Install(Container);

        //DevUI Service
        Service.DevUIService.ServiceInstaller.Install(Container);

        //MemoryBrowser
        Service.MemoryBrowserService.ServiceInstaller.Install(Container);

        //Local Storage
        //Service.LocalStorage.ServiceInstaller.Install(Container);

        //Resources Service
        //Service.Resources.ServiceInstaller.Install(Container);

        //Camera Service
        //Service.Camera.ServiceInstaller.Install(Container);

        Debug.Log("Finished installing services");
    }


}
