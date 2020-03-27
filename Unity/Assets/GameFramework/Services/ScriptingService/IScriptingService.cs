///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 

using System.Collections.Generic;
using System.Collections;
using MoonSharp.Interpreter;
using System.Text;
using ECS;

using System;
using static Service.Scripting.Events;
using System.Runtime.Serialization;
using FlatBuffers;
using Service.Serializer;
using System.Linq;

namespace Service.Scripting {
    public interface IScriptingService : IFBSerializable, IService {




					Script GetMainScript();
    


		/// <summary>
        /// Execute a string into the default-lua-context 
        /// <param name="luaCode"></param>
 /// </summary>
        

					string ExecuteStringOnMainScript(string luaCode);
    


		/// <summary>
        /// Load a script into the default lua-context 
        /// <param name="fileName"></param>
        /// <param name="useScriptDomain"></param>
 /// </summary>
        

					string ExecuteFileToMainScript(string fileName,bool useScriptDomain=false);
    


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
    


		/// <summary>
        /// Creates a lua coroutine 
        /// <param name="funcName"></param>
 /// </summary>
        

					LuaCoroutine CreateCoroutine(DynValue funcName);
    


					void Callback(string cbtype,object o2=null,object o3=null,object o4=null,object o5=null);
    


					void RegisterCallback(Action<string,object,object,object,object> cbCallbackFunc);
    


					void UnregisterCallback(Action<string,object,object,object,object> cbCallbackFunc);
    


		/// <summary>
        /// custom coroutine-yield functions. return false if the coRoutine should be removed from the system 
        /// <param name="coRoutines"></param>
 /// </summary>
        

					void RegisterCustomYieldCheck(Func<LuaCoroutine,bool> coRoutines);
    


		/// <summary>
        /// Gives this uid a unique id which makes accessing this entity entity-id independed 
        /// <param name="persistedId"></param>
        /// <param name="entity"></param>
 /// </summary>
        

					void RegisterEntityToLua(int persistedId,UID entity);
    


					bool IsEntityRegistered(UID entity);
    


					int GetLUAEntityID(UID entity);
    


					IComponent GetComponent(UID entity,string componentName);
    


					void Setup(bool isNewGame);
    


					void Cleanup();
    


					void Tick(float dt);
    


					void StartLog(string filename);
    


					void WriteLog(string outputString,bool alsoToConsole=true);
    


					void ActivateLuaReplayScript(bool activate);
    


					bool LuaScriptActivated();
    


		/// <summary>
        /// Save to this filename in the scripting-folder 
        /// <param name="fileName"></param>
 /// </summary>
        

					void SaveCurrentLuaReplay(string fileName);
    


		/// <summary>
        /// Get the current lua-replay as script 
 /// </summary>
        

					string GetCurrentLuaReplay();
    


					System.Text.StringBuilder GetLuaReplayStringBuilder();
    


					void SetLuaReplayStringBuilder(StringBuilder replayScript);
    


		/// <summary>
        /// Sets a func to get the current gametime that is used for ReplayWrite 
        /// <param name="getCurrentGameTime"></param>
 /// </summary>
        

					void SetLuaReplayGetGameTimeFunc(Func<float> getCurrentGameTime);
    


					void ReplayWrite_RegisterEntity(string entityVarName="entity");
    


					void ReplayWrite_CustomLua(string luaScript,bool waitForGameTime=true);
    


					void ReplayWrite_SetCurrentEntity(ECS.UID uid);
    


	}


    
    [System.Serializable]
    public  class ScriptingServiceData: DefaultSerializable2
    {

        private ExtendedTable ser2table = ExtendedTable.NULL;

        public new ExtendedTable Ser2Table => ser2table;

        public new bool Ser2IsDirty { get; set; } // TODO. Is dirty should be some kind of virtual

        public new bool Ser2HasOffset => !ser2table.IsNULL();

        public new int Ser2Offset => ser2table.offset;

        public virtual void Ser2Deserialize(DeserializationContext ctx) {
            int offset = ctx.bb.Length - ctx.bb.GetInt(ctx.bb.Position) + ctx.bb.Position;
            Ser2Deserialize(offset, ctx);
        }

        public virtual int Ser2Serialize(SerializationContext ctx) {
            if (!Ser2HasOffset) {
                Ser2CreateTable(ctx, ctx.builder);
            } else {
                Ser2UpdateTable(ctx, ctx.builder);
            }
            return base.ser2table.offset;
        }

        public ScriptingServiceData() { }
        
        /// <summary>
        /// 
        /// </summary>
        
        public StringBuilder replayScript = new StringBuilder();
        
        /// <summary>
        /// 
        /// </summary>
        
        public bool saveReplayScript = true;
        
        /// <summary>
        /// 
        /// </summary>
        
        public Dictionary<UID,int> uid2persistedId = new Dictionary<UID, int>();
        
        
        
        /// <summary>
        /// Default constructor
        /// </summary>
        
    }

    
    [System.Serializable]
    public partial class LuaCoroutine: DefaultSerializable2
    {

        private ExtendedTable ser2table = ExtendedTable.NULL;

        public new ExtendedTable Ser2Table => ser2table;

        public new bool Ser2IsDirty { get; set; } // TODO. Is dirty should be some kind of virtual

        public new bool Ser2HasOffset => !ser2table.IsNULL();

        public new int Ser2Offset => ser2table.offset;

        public virtual void Ser2Deserialize(DeserializationContext ctx) {
            int offset = ctx.bb.Length - ctx.bb.GetInt(ctx.bb.Position) + ctx.bb.Position;
            Ser2Deserialize(offset, ctx);
        }

        public virtual int Ser2Serialize(SerializationContext ctx) {
            if (!Ser2HasOffset) {
                Ser2CreateTable(ctx, ctx.builder);
            } else {
                Ser2UpdateTable(ctx, ctx.builder);
            }
            return base.ser2table.offset;
        }

        public LuaCoroutine() { }
        
        /// <summary>
        /// 
        /// </summary>
        
        public MoonSharp.Interpreter.DynValue co ;
        
        /// <summary>
        /// 
        /// </summary>
        
        public string waitForType ;
        
        /// <summary>
        /// 
        /// </summary>
        
        public object value1 ;
        
        /// <summary>
        /// 
        /// </summary>
        
        public DynValue value2 ;
        
        /// <summary>
        /// 
        /// </summary>
        
        public Dictionary<string,object> context ;
        
        
        
        /// <summary>
        /// Default constructor
        /// </summary>
        
    }

    
}
///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 
