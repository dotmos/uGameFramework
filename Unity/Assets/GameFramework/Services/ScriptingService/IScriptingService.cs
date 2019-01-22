///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 

using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

using static Service.Scripting.Events;
using System.Runtime.Serialization;
using FlatBuffers;
using Service.Serializer;

namespace Service.Scripting {
    public interface IScriptingService {



					Script GetMainScript();


		/// <summary>
        /// Execute a string into the default-lua-context 
        /// <param name="luaCode"></param>
 /// </summary>
        

					string ExecuteStringOnMainScript(string luaCode);


		/// <summary>
        /// Load a script into the default lua-context 
        /// <param name="fileName"></param>
 /// </summary>
        

					string ExecuteFileToMainScript(string fileName);


		/// <summary>
        /// Load a script into the default lua-context 
        /// <param name="fileName"></param>
 /// </summary>
        

					DynValue ExecuteStringOnMainScriptRaw(string fileName);


		/// <summary>
        /// Generates a list of possible proposals 
        /// <param name="currentInput"></param>
        /// <param name="cursorPos"></param>
 /// </summary>
        

					Proposal AutocompleteProposals(string currentInput,int cursorPos);

	}



}
///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 
