///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 

using System.Collections.Generic;
using MoonSharp.Interpreter;

using System;
using static Service.GameStateService.Events;
using System.Runtime.Serialization;
using FlatBuffers;
using Service.Serializer;
using System.Linq;

namespace Service.GameStateService {
    public interface IGameStateService : IFBSerializable,IService {



		/// <summary>
        /// Register gamestate with its name. Optionally you can pass an overriden GameState-Classtype of your own 
        /// <param name="name"></param>
        /// <param name="gamestate"></param>
 /// </summary>
        

					GameState RegisterGameState(string name,GameState gamestate=null);


		/// <summary>
        /// Get the current gamestate. Alternatively use "[Inject] GameState current;" 
 /// </summary>
        

					GameState GetCurrentGameState();


		/// <summary>
        /// Check if the current gamestate is the specified one 
        /// <param name="gs"></param>
 /// </summary>
        

					bool IsInState(GameState gs);


		/// <summary>
        /// Start a new gamestate after stopping the current one (if present). Optionally pass a context in which you can e.g. set gamestate-flags 
        /// <param name="gamestate"></param>
        /// <param name="ctx"></param>
 /// </summary>
        

					IObservable<bool> StartGameState(GameState gamestate,GSContext ctx=null);


					IObservable<bool> StopGameState(GameState gamestate);


		/// <summary>
        /// Get gamestate by name 
        /// <param name="name"></param>
 /// </summary>
        

					GameState GetGameState(string name);


		/// <summary>
        /// Tick the current gamestate. 
        /// <param name="deltaTime"></param>
        /// <param name="unscaledDeltaTime"></param>
 /// </summary>
        

					void Tick(float deltaTime,float unscaledDeltaTime);

	}


    public enum GSStatus {
        noneStatus,starting,running,closing
        
    }
    
    
    [System.Serializable]
    public  class GSContext {
        public GSContext() { }
        
        
        

        
    }

    
}
///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 
