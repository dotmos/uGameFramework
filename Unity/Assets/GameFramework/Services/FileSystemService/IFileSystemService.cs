///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 

using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

using static Service.FileSystem.Events;
using UniRx;
using Zenject;
using System.Runtime.Serialization;

namespace Service.FileSystem {
    public interface IFileSystemService {



		/// <summary>
        /// Get path as string for given domain 
        /// <param name="domain"></param>

        /// </summary>
        

					string GetPath(FSDomain domain);


		/// <summary>
        /// Write string to file 
        /// <param name="pathToFile"></param>
        /// <param name="data"></param>

        /// </summary>
        

					bool WriteStringToFile(string pathToFile,string data);


		/// <summary>
        /// Load file as string 
        /// <param name="pathToFile"></param>

        /// </summary>
        

					string LoadFileAsString(string pathToFile);

	}


    public enum FSDomain {
        ScriptingOutput
        
    }


}
///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 
