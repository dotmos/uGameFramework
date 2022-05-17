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
    }


}
