using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Service.CodeCompiler{
    public interface ICodeCompilerService{
        Assembly CompileToAssembly(string[] filePaths, string assemblyName = "");
        Assembly CompileToAssemblyFromCode(string[] code, string assemblyName = "");

        Dictionary<string, Assembly> assemblies {
            get;
            set;
        }
    }
}