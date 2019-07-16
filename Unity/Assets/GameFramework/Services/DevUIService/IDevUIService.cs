///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 

using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using ECS;
using UniRx;

using static Service.DevUIService.Events;
using System.Runtime.Serialization;
using FlatBuffers;
using Service.Serializer;

namespace Service.DevUIService {
    public interface IDevUIService : IFBSerializable {



		/// <summary>
        /// Get ReaciveDictionar of all views 
 /// </summary>
        

					ReactiveCollection<DevUIView> GetRxViews();


		/// <summary>
        /// Add/Create view with name 
        /// <param name="viewName"></param>
        /// <param name="dynamicallyCreated"></param>
        /// <param name="extensionAllowed"></param>
 /// </summary>
        

					DevUIView CreateView(string viewName,bool dynamicallyCreated=false,bool extensionAllowed=true);


		/// <summary>
        /// Get a view by name 
        /// <param name="viewName"></param>
 /// </summary>
        

					DevUIView GetView(string viewName);


		/// <summary>
        /// Check if view already exists 
        /// <param name="viewName"></param>
 /// </summary>
        

					bool ViewNameExists(string viewName);


		/// <summary>
        /// Remove View from data model 
        /// <param name="view"></param>
 /// </summary>
        

					void RemoveViewFromModel(DevUIView view);


		/// <summary>
        /// Remove View from views-folder and put it to the archieve-folder 
        /// <param name="view"></param>
 /// </summary>
        

					void RemoveViewToArchieve(DevUIView view);


		/// <summary>
        /// Load views from views-folder 
 /// </summary>
        

					IObservable<float> LoadViews();


		/// <summary>
        /// Save views and its dynamically created elements 
 /// </summary>
        

					void SaveViews();


		/// <summary>
        /// Output to console 
        /// <param name="text"></param>
 /// </summary>
        

					void WriteToScriptingConsole(string text);


		/// <summary>
        /// Open the console 
 /// </summary>
        

					void OpenScriptingConsole();


		/// <summary>
        /// Close the console 
 /// </summary>
        

					void CloseScriptingConsole();


		/// <summary>
        /// Toggle the console visibility 
 /// </summary>
        

					void ToggleScriptingConsole();


		/// <summary>
        /// Check if console is visible at the moment 
 /// </summary>
        

					bool IsScriptingConsoleVisible();


		/// <summary>
        /// Start entity picking mode 
 /// </summary>
        

					void StartPickingEntity();


					DevUIView CreateViewFromEntity(UID entity,string name="");


					DevUIView CreateViewFromPOCO(object entity,string name);


					void CreateDataBrowserTopLevelElement(string name,System.Collections.IList objectList);


					List<DataBrowserTopLevel> GetDataBrowserTopLevelElements();


		/// <summary>
        /// map one object type to another 
        /// <param name="objType"></param>
        /// <param name="converter"></param>
 /// </summary>
        

					void AddDataBrowserObjectConverter(Type objType,Func<object,object> converter);


		/// <summary>
        /// try to convert the inObject, if no conversion is possible return the inObject 
        /// <param name="inObject"></param>
 /// </summary>
        

					object DataBrowserConvertObject(object inObject);


					void OutputGameInfo(float systemStartupTime);

	}


    
    [System.Serializable]
    public partial class DataBrowserTopLevel {
        
        public string topLevelName ;
        
        public System.Collections.IList objectList ;
        
        
        
        
    }

    
    [System.Serializable]
    public partial class HistoryElement {
        
        public System.Collections.IList objectList ;
        
        public string historyTitle ;
        
        
        
        
    }

    
}
///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 
