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
    public interface IScriptingService : IFBSerializable2, IFBSerializable, IService {


        
        
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
    public partial class ScriptingServiceData: DefaultSerializable2
    {
        

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
        /// Merges data into your object. (no deep copy)
        /// </summary>
        /// <param name="incoming"></param>
        /// <param name="onlyCopyPersistedData"></param>
        public void MergeDataFrom(ScriptingServiceData incoming, bool onlyCopyPersistedData = false) {
            // base.MergeDataFrom(incoming, onlyCopyPersistedData);

            if (!onlyCopyPersistedData) this.replayScript = incoming.replayScript;
            if (!onlyCopyPersistedData) this.saveReplayScript = incoming.saveReplayScript;
            if (!onlyCopyPersistedData) this.uid2persistedId = incoming.uid2persistedId;
            
        }

        
    }

    public partial class ScriptingServiceData : IFBSerializeAsTypedObject, IMergeableData<ScriptingServiceData> {
    }
        
    [System.Serializable]
    public partial class LuaCoroutine: DefaultSerializable2
    {
        

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
        /// Merges data into your object. (no deep copy)
        /// </summary>
        /// <param name="incoming"></param>
        /// <param name="onlyCopyPersistedData"></param>
        public void MergeDataFrom(LuaCoroutine incoming, bool onlyCopyPersistedData = false) {
            // base.MergeDataFrom(incoming, onlyCopyPersistedData);

            if (!onlyCopyPersistedData) this.co = incoming.co;
            if (!onlyCopyPersistedData) this.waitForType = incoming.waitForType;
            if (!onlyCopyPersistedData) this.value1 = incoming.value1;
            if (!onlyCopyPersistedData) this.value2 = incoming.value2;
            if (!onlyCopyPersistedData) this.context = incoming.context;
            
        }

        
    }

    public partial class LuaCoroutine : IFBSerializeAsTypedObject, IMergeableData<LuaCoroutine> {
    }
        
}
///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 
