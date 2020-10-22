///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 

using System.Collections.Generic;
using MoonSharp.Interpreter;
using System.Collections.Concurrent;

using System;
using static Service.PerformanceTest.Events;
using System.Runtime.Serialization;
using FlatBuffers;
using Service.Serializer;
using System.Linq;
using Service.PerformanceTest;

namespace Service.PerformanceTest {
    public interface IPerformanceTestService : IFBSerializable2, IFBSerializable, IService {


        
        
                    void StartWatch(string t);
        
                           
        
                    void StopWatch(string t);
        
                           
        
                    void PerfTestOutputToConsole();
        
                           
        
                    string PerfTestOutputAsString();
        
                           
        
                    void Clear();
        
                           
        
        /// <summary>
        /// Add instance to 'leak-system' (must be removed in destructor) 
                /// <param name="o"></param>
         /// </summary>
        
        
                    void AddInstance(object o);
        
                           
        
        /// <summary>
        /// Remove instance from 'leak-system' 
                /// <param name="o"></param>
         /// </summary>
        
        
                    void RemoveInstance(object o);
        
                           
        
        /// <summary>
        /// Get the current amount of captured instance-counts 
         /// </summary>
        
        
                    ConcurrentDictionary<System.Type,int> GetInstanceView();
        
                           
        
        /// <summary>
        /// Get the instance-count for a specific type 
                /// <param name="instanceType"></param>
         /// </summary>
        
        
                    int GetInstanceCount(System.Type instanceType);
        
                           
        
        /// <summary>
        /// Output instance-view to log 
                /// <param name="gccollect"></param>
                /// <param name="compare"></param>
         /// </summary>
        
        
                    void OutputInstanceViewToLog(bool gccollect=true,bool compare=true);
        
                           
        
        /// <summary>
        /// Stores the current view-data for later comparison 
         /// </summary>
        
        
                    void StoreCurrentView();
        
                           

    }

    
    
}
///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 
