///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 

using System.Collections.Generic;
using MoonSharp.Interpreter;

using System;
using static Service.PerformanceTest.Events;
using System.Runtime.Serialization;
using FlatBuffers;
using Service.Serializer;
using System.Linq;

namespace Service.PerformanceTest {
    public interface IPerformanceTestService : IFBSerializable, IService {
        

        
        
                    void StartWatch(string t);
        
                           
        
                    void StopWatch(string t);
        
                           
        
                    void PrintPerfTests();
        
                           
        
                    void Clear();
        
                           

    }

    
    
}
///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 
