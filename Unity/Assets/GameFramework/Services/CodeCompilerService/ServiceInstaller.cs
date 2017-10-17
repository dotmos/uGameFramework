using UnityEngine;
using System.Collections;
using Zenject;

namespace Service.CodeCompiler{
    public class ServiceInstaller : Installer{
        public override void InstallBindings()
        {
            Container.Bind<Service.CodeCompiler.ICodeCompilerService>().To<Service.CodeCompiler.CSharp.MCS>().AsSingle();
        }
    }
}