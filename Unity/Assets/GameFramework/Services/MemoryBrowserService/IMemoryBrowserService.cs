///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 

using System.Collections.Generic;
using MoonSharp.Interpreter;
using UniRx;

using System;
using static Service.MemoryBrowserService.Events;
using System.Runtime.Serialization;
using FlatBuffers;
using Service.Serializer;
using System.Linq;

namespace Service.MemoryBrowserService {
    public interface IMemoryBrowserService : IFBSerializable,IService {



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
