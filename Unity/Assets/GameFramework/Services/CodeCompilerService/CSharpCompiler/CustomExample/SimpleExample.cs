using UnityEngine;
using System.Collections;
using System;
using System.Linq;

public class SimpleExample : MonoBehaviour {

	// Use this for initialization
	void Start () {

        //Setup compiler
        CSharpCompiler.CodeCompiler compiler = new CSharpCompiler.CodeCompiler();
        System.CodeDom.Compiler.CompilerParameters options = new System.CodeDom.Compiler.CompilerParameters();
        options.GenerateExecutable = false;
        options.GenerateInMemory = true;
        options.OutputAssembly = "MyAssembly.dll";
        //-- Add ALL assemblies to compiler. This is a security issue as user would have access to System.IO.File to delete data...
        //var domain = System.AppDomain.CurrentDomain;
        //string[] assemblyReferences = domain.GetAssemblies().Select(a => a.Location).ToArray(); 
        //options.ReferencedAssemblies.AddRange(assemblyReferences);
        //-- Add only some specific assemblies
        options.ReferencedAssemblies.Add("UnityEngine"); //Add UnityEngine assembly
        options.ReferencedAssemblies.Add("Assembly-CSharp"); //Add Assembly which holds all our scripts after build (THIS script is also located in this assembly)

        //Compile
        var result = compiler.CompileAssemblyFromFileBatch(options, new[]{
            Application.streamingAssetsPath + "/BasicExampleScript.cs"
            //Add other scripts here, separated by commas
        } );

        //Create instances for all classes we just compiled
        //foreach (var type in result.CompiledAssembly.GetTypes())
        //{
        //    if (typeof(Component).IsAssignableFrom(type)) this.gameObject.AddComponent(type); //If type is a MonoBehaviour, add it to the gameobject
        //    else System.Activator.CreateInstance(type); //Otherwise create a new instance of the class, using the default class constructor
        //}

        //Add specific MonoBehaviour from our compiled scripts
        Type _mb = result.CompiledAssembly.GetType("MyMonoBehaviour");
        this.gameObject.AddComponent(_mb);
        Debug.Log(result.CompiledAssembly.GetName());

        //Create an instance of a specific class
        Type _classType = result.CompiledAssembly.GetType("SomePublicClass");
        var classInstance = Activator.CreateInstance(_classType);
        //Since SomePublicClass uses IMyClass interface (see below), we can cast to it :)
        IMyClass myClassInstance = (IMyClass)classInstance;
        myClassInstance.DoSomething(); //...and call a function defined in IMyClass
        Debug.Log("Sum:"+ myClassInstance.Sum(40, 2)); //Another function in SomePublicClass which returns an int

	}
}

//Interface for SomePublicClass in BasicExampleScript.cs
public interface IMyClass
{
    void DoSomething();
    int Sum(int a, int b);
}
