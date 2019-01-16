///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 

using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

using static Service.MemoryBrowserService.Events;
using UniRx;
using Zenject;
using System.Runtime.Serialization;
using System.Collections.Generic;
using FlatBuffers;
using Service.Serializer;

namespace Service.MemoryBrowserService {
    public interface IMemoryBrowserService {



		/// <summary>
        /// Is this obj a simple type? (int,float,bool,string).  
        /// <param name="obj"></param>
 /// </summary>
        

					bool IsSimpleType(object obj);


					MemoryBrowser CreateMemoryBrowser(string id,object root);


		/// <summary>
        /// Get Browser by name 
        /// <param name="id"></param>
 /// </summary>
        

					MemoryBrowser GetBrowser(string id);


		/// <summary>
        /// Get Reactive Dictionary with a memory-browsers(key=name value=MemoryBrowser) 
 /// </summary>
        

					ReactiveDictionary<string, MemoryBrowser> rxGetAllBrowsers();

	}



}
///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 
