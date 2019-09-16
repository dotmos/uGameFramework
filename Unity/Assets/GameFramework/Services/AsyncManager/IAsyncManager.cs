///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 


using System;
using static Service.AsyncManager.Events;
using System.Runtime.Serialization;
using FlatBuffers;
using Service.Serializer;
using System.Linq;

namespace Service.AsyncManager {
    public interface IAsyncManager : IFBSerializable,IService {



					AsyncFuture AddToMainThread(Action act,bool global=false);


					AsyncFuture AddToWorkerThread(Action act,Action onFinished,bool global=false);


					AsyncFuture Call(Action act,bool usingCoroutine,bool global=false);


					void DisposeThreads(bool onlyNonGlobals=false);

	}


    
}
///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 
