
using Zenject;


namespace Service.FileSystem
{
    public class ServiceInstaller : Installer<ServiceInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<IFileSystemService>().To<FileSystemServiceImpl>().AsSingle();
            CommandsInstaller.Install(Container);
        }
        
    }
}
