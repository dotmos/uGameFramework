///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 

using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using UniRx;

using static Service.LoggingService.Events;
using System.Runtime.Serialization;
using FlatBuffers;
using Service.Serializer;

namespace Service.LoggingService {
    public interface ILoggingService {



					void AddLog(DebugType debugType,string message,string domain="");


					void Info(string message,string domain="");


					void Warn(string message,string domain="");


					void Error(string message,string domain="");


					void Severe(string message,string domain="");


					ReactiveCollection<LogData> GetRxOutputData();

	}


    public enum DebugType {
        info,warning,error,severe,native
        
    }
    
    
}
///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 
