using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Service.CodeCompiler.CSharp {
    public class MCS : ICodeCompilerService {

        CSharpCompiler.CodeCompiler compiler;
        System.CodeDom.Compiler.CompilerParameters options;

        public Dictionary<string, Assembly> assemblies {
            get;
            set;
        }

        public MCS()
        {
            compiler = new CSharpCompiler.CodeCompiler();
            options = new System.CodeDom.Compiler.CompilerParameters();
            options.GenerateExecutable = false;
            options.GenerateInMemory = true;
            options.OutputAssembly = "ScriptAssembly.dll";
            //options.ReferencedAssemblies.AddRange(ReferencedAssemblies.assemblies);

            //Reference all assemblies, except System.IO
            string[] assemblyReferences = System.AppDomain.CurrentDomain.GetAssemblies().Where(e => e.FullName.Contains("System.IO") == false ).Select(a => a.Location).ToArray(); 
            options.ReferencedAssemblies.AddRange(assemblyReferences);

            assemblies = new Dictionary<string, Assembly>();
        }

        void SetOutputAssemblyName(string name)
        {
            if(string.IsNullOrEmpty(name) == false)
                options.OutputAssembly = name;
        }

        Assembly CheckCompileResult(System.CodeDom.Compiler.CompilerResults result, string assemblyName)
        {
            if(result != null && result.CompiledAssembly != null)
            {
                if(string.IsNullOrEmpty(assemblyName))
                {
                    assemblyName = options.OutputAssembly;
                }

                if(assemblies.ContainsKey(assemblyName))
                {
                    assemblies[assemblyName] = result.CompiledAssembly;
                }
                else
                {
                    assemblies.Add(assemblyName, result.CompiledAssembly);
                }

                return assemblies[assemblyName];
            }
            Debug.LogWarning("No assembly created!");
            return null;
        }

        /// <summary>
        /// Compiles .cs files to assembly.
        /// </summary>
        /// <returns>The to assembly.</returns>
        /// <param name="filePaths">File paths.</param>
        /// <param name="assemblyName">Assembly name.</param>
        public Assembly CompileToAssembly(string[] filePaths, string assemblyName = "")
        {
            SetOutputAssemblyName(assemblyName);
            var result = compiler.CompileAssemblyFromFileBatch(options, filePaths);

            return CheckCompileResult(result, assemblyName);
        }

        /// <summary>
        /// Compiles (multiple) code to assembly.
        /// </summary>
        /// <returns>The to assembly from code.</returns>
        /// <param name="code">Code.</param>
        /// <param name="assemblyName">Assembly name.</param>
        public Assembly CompileToAssemblyFromCode(string[] code, string assemblyName = "")
        {
            SetOutputAssemblyName(assemblyName);
            var result = compiler.CompileAssemblyFromSourceBatch(options, code);

            return CheckCompileResult(result, assemblyName);
        }
    }
}