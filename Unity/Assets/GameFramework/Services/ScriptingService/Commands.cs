
using System.Collections.Generic;
using System.Collections;
using MoonSharp.Interpreter;
using System.Text;
using ECS;

using System;
using Service.Events;
using Zenject;
using UniRx;
using System.Diagnostics;


///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 
namespace Service.Scripting{
    public partial class Commands : CommandsBase {
        IScriptingService _service;

        [Inject]
        void Initialize([Inject] IScriptingService service) {
            _service = service;
            
            this.OnEvent<GetMainScriptCommand>().Subscribe(e => GetMainScriptCommandHandler(e)).AddTo(this);

            this.OnEvent<ExecuteStringOnMainScriptCommand>().Subscribe(e => ExecuteStringOnMainScriptCommandHandler(e)).AddTo(this);

            this.OnEvent<ExecuteFileToMainScriptCommand>().Subscribe(e => ExecuteFileToMainScriptCommandHandler(e)).AddTo(this);

            this.OnEvent<ExecuteStringOnMainScriptRawCommand>().Subscribe(e => ExecuteStringOnMainScriptRawCommandHandler(e)).AddTo(this);

            this.OnEvent<AutocompleteProposalsCommand>().Subscribe(e => AutocompleteProposalsCommandHandler(e)).AddTo(this);

            this.OnEvent<CreateCoroutineCommand>().Subscribe(e => CreateCoroutineCommandHandler(e)).AddTo(this);

            this.OnEvent<CallbackCommand>().Subscribe(e => CallbackCommandHandler(e)).AddTo(this);

            this.OnEvent<RegisterCallbackCommand>().Subscribe(e => RegisterCallbackCommandHandler(e)).AddTo(this);

            this.OnEvent<UnregisterCallbackCommand>().Subscribe(e => UnregisterCallbackCommandHandler(e)).AddTo(this);

            this.OnEvent<RegisterCustomYieldCheckCommand>().Subscribe(e => RegisterCustomYieldCheckCommandHandler(e)).AddTo(this);

            this.OnEvent<RegisterEntityToLuaCommand>().Subscribe(e => RegisterEntityToLuaCommandHandler(e)).AddTo(this);

            this.OnEvent<IsEntityRegisteredCommand>().Subscribe(e => IsEntityRegisteredCommandHandler(e)).AddTo(this);

            this.OnEvent<GetLUAEntityIDCommand>().Subscribe(e => GetLUAEntityIDCommandHandler(e)).AddTo(this);

            this.OnEvent<GetComponentCommand>().Subscribe(e => GetComponentCommandHandler(e)).AddTo(this);

            this.OnEvent<SetupCommand>().Subscribe(e => SetupCommandHandler(e)).AddTo(this);

            this.OnEvent<CleanupCommand>().Subscribe(e => CleanupCommandHandler(e)).AddTo(this);

            this.OnEvent<TickCommand>().Subscribe(e => TickCommandHandler(e)).AddTo(this);

            this.OnEvent<StartLogCommand>().Subscribe(e => StartLogCommandHandler(e)).AddTo(this);

            this.OnEvent<WriteLogCommand>().Subscribe(e => WriteLogCommandHandler(e)).AddTo(this);

            this.OnEvent<ActivateLuaReplayScriptCommand>().Subscribe(e => ActivateLuaReplayScriptCommandHandler(e)).AddTo(this);

            this.OnEvent<LuaScriptActivatedCommand>().Subscribe(e => LuaScriptActivatedCommandHandler(e)).AddTo(this);

            this.OnEvent<SaveCurrentLuaReplayCommand>().Subscribe(e => SaveCurrentLuaReplayCommandHandler(e)).AddTo(this);

            this.OnEvent<GetCurrentLuaReplayCommand>().Subscribe(e => GetCurrentLuaReplayCommandHandler(e)).AddTo(this);

            this.OnEvent<GetLuaReplayStringBuilderCommand>().Subscribe(e => GetLuaReplayStringBuilderCommandHandler(e)).AddTo(this);

            this.OnEvent<SetLuaReplayStringBuilderCommand>().Subscribe(e => SetLuaReplayStringBuilderCommandHandler(e)).AddTo(this);

            this.OnEvent<SetLuaReplayGetGameTimeFuncCommand>().Subscribe(e => SetLuaReplayGetGameTimeFuncCommandHandler(e)).AddTo(this);

            this.OnEvent<ReplayWrite_RegisterEntityCommand>().Subscribe(e => ReplayWrite_RegisterEntityCommandHandler(e)).AddTo(this);

            this.OnEvent<ReplayWrite_CustomLuaCommand>().Subscribe(e => ReplayWrite_CustomLuaCommandHandler(e)).AddTo(this);

            this.OnEvent<ReplayWrite_SetCurrentEntityCommand>().Subscribe(e => ReplayWrite_SetCurrentEntityCommandHandler(e)).AddTo(this);

        }
        

        
        /// <summary>
        /// 
        /// </summary>
        
        public class GetMainScriptCommand  {
            public Script result;
            
            
        }

		protected void GetMainScriptCommandHandler  (GetMainScriptCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("GetMainScriptCommand");
#endif
        
            cmd.result = _service.GetMainScript();
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("GetMainScriptCommand");
#endif
        }
        

        
        /// <summary>
        /// Execute a string into the default-lua-context
        /// </summary>
        
        public class ExecuteStringOnMainScriptCommand  {
            public string result;
                        public string luaCode;
            
            
        }

		protected void ExecuteStringOnMainScriptCommandHandler  (ExecuteStringOnMainScriptCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("ExecuteStringOnMainScriptCommand");
#endif
        
            cmd.result = _service.ExecuteStringOnMainScript(cmd.luaCode);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("ExecuteStringOnMainScriptCommand");
#endif
        }
        

        
        /// <summary>
        /// Load a script into the default lua-context
        /// </summary>
        
        public class ExecuteFileToMainScriptCommand  {
            public string result;
                        public string fileName;
                        public bool useScriptDomain=false;
            
            
        }

		protected void ExecuteFileToMainScriptCommandHandler  (ExecuteFileToMainScriptCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("ExecuteFileToMainScriptCommand");
#endif
        
            cmd.result = _service.ExecuteFileToMainScript(cmd.fileName,cmd.useScriptDomain);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("ExecuteFileToMainScriptCommand");
#endif
        }
        

        
        /// <summary>
        /// Load a script into the default lua-context
        /// </summary>
        
        public class ExecuteStringOnMainScriptRawCommand  {
            public DynValue result;
                        public string fileName;
            
            
        }

		protected void ExecuteStringOnMainScriptRawCommandHandler  (ExecuteStringOnMainScriptRawCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("ExecuteStringOnMainScriptRawCommand");
#endif
        
            cmd.result = _service.ExecuteStringOnMainScriptRaw(cmd.fileName);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("ExecuteStringOnMainScriptRawCommand");
#endif
        }
        

        
        /// <summary>
        /// Generates a list of possible proposals
        /// </summary>
        
        public class AutocompleteProposalsCommand  {
            public Proposal result;
                        public string currentInput;
                        public int cursorPos;
            
            
        }

		protected void AutocompleteProposalsCommandHandler  (AutocompleteProposalsCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("AutocompleteProposalsCommand");
#endif
        
            cmd.result = _service.AutocompleteProposals(cmd.currentInput,cmd.cursorPos);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("AutocompleteProposalsCommand");
#endif
        }
        

        
        /// <summary>
        /// Creates a lua coroutine
        /// </summary>
        
        public class CreateCoroutineCommand  {
            public LuaCoroutine result;
                        public DynValue funcName;
            
            
        }

		protected void CreateCoroutineCommandHandler  (CreateCoroutineCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("CreateCoroutineCommand");
#endif
        
            cmd.result = _service.CreateCoroutine(cmd.funcName);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("CreateCoroutineCommand");
#endif
        }
        

        
        /// <summary>
        /// 
        /// </summary>
        
        public class CallbackCommand  {
            public string cbtype;
                        public object o2=null;
                        public object o3=null;
                        public object o4=null;
                        public object o5=null;
            
            
        }

		protected void CallbackCommandHandler  (CallbackCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("CallbackCommand");
#endif
        _service.Callback(cmd.cbtype,cmd.o2,cmd.o3,cmd.o4,cmd.o5);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("CallbackCommand");
#endif
        }
        

        
        /// <summary>
        /// 
        /// </summary>
        
        public class RegisterCallbackCommand  {
            public Action<string,object,object,object,object> cbCallbackFunc;
            
            
        }

		protected void RegisterCallbackCommandHandler  (RegisterCallbackCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("RegisterCallbackCommand");
#endif
        _service.RegisterCallback(cmd.cbCallbackFunc);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("RegisterCallbackCommand");
#endif
        }
        

        
        /// <summary>
        /// 
        /// </summary>
        
        public class UnregisterCallbackCommand  {
            public Action<string,object,object,object,object> cbCallbackFunc;
            
            
        }

		protected void UnregisterCallbackCommandHandler  (UnregisterCallbackCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("UnregisterCallbackCommand");
#endif
        _service.UnregisterCallback(cmd.cbCallbackFunc);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("UnregisterCallbackCommand");
#endif
        }
        

        
        /// <summary>
        /// custom coroutine-yield functions. return false if the coRoutine should be removed from the system
        /// </summary>
        
        public class RegisterCustomYieldCheckCommand  {
            public Func<LuaCoroutine,bool> coRoutines;
            
            
        }

		protected void RegisterCustomYieldCheckCommandHandler  (RegisterCustomYieldCheckCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("RegisterCustomYieldCheckCommand");
#endif
        _service.RegisterCustomYieldCheck(cmd.coRoutines);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("RegisterCustomYieldCheckCommand");
#endif
        }
        

        
        /// <summary>
        /// Gives this uid a unique id which makes accessing this entity entity-id independed
        /// </summary>
        
        public class RegisterEntityToLuaCommand  {
            public int persistedId;
                        public UID entity;
            
            
        }

		protected void RegisterEntityToLuaCommandHandler  (RegisterEntityToLuaCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("RegisterEntityToLuaCommand");
#endif
        _service.RegisterEntityToLua(cmd.persistedId,cmd.entity);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("RegisterEntityToLuaCommand");
#endif
        }
        

        
        /// <summary>
        /// 
        /// </summary>
        
        public class IsEntityRegisteredCommand  {
            public bool result;
                        public UID entity;
            
            
        }

		protected void IsEntityRegisteredCommandHandler  (IsEntityRegisteredCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("IsEntityRegisteredCommand");
#endif
        
            cmd.result = _service.IsEntityRegistered(cmd.entity);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("IsEntityRegisteredCommand");
#endif
        }
        

        
        /// <summary>
        /// 
        /// </summary>
        
        public class GetLUAEntityIDCommand  {
            public int result;
                        public UID entity;
            
            
        }

		protected void GetLUAEntityIDCommandHandler  (GetLUAEntityIDCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("GetLUAEntityIDCommand");
#endif
        
            cmd.result = _service.GetLUAEntityID(cmd.entity);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("GetLUAEntityIDCommand");
#endif
        }
        

        
        /// <summary>
        /// 
        /// </summary>
        
        public class GetComponentCommand  {
            public IComponent result;
                        public UID entity;
                        public string componentName;
            
            
        }

		protected void GetComponentCommandHandler  (GetComponentCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("GetComponentCommand");
#endif
        
            cmd.result = _service.GetComponent(cmd.entity,cmd.componentName);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("GetComponentCommand");
#endif
        }
        

        
        /// <summary>
        /// 
        /// </summary>
        
        public class SetupCommand  {
            public bool isNewGame;
            
            
        }

		protected void SetupCommandHandler  (SetupCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("SetupCommand");
#endif
        _service.Setup(cmd.isNewGame);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("SetupCommand");
#endif
        }
        

        
        /// <summary>
        /// 
        /// </summary>
        
        public class CleanupCommand  {

            
        }

		protected void CleanupCommandHandler  (CleanupCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("CleanupCommand");
#endif
        _service.Cleanup();
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("CleanupCommand");
#endif
        }
        

        
        /// <summary>
        /// 
        /// </summary>
        
        public class TickCommand  {
            public float dt;
            
            
        }

		protected void TickCommandHandler  (TickCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("TickCommand");
#endif
        _service.Tick(cmd.dt);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("TickCommand");
#endif
        }
        

        
        /// <summary>
        /// 
        /// </summary>
        
        public class StartLogCommand  {
            public string filename;
            
            
        }

		protected void StartLogCommandHandler  (StartLogCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("StartLogCommand");
#endif
        _service.StartLog(cmd.filename);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("StartLogCommand");
#endif
        }
        

        
        /// <summary>
        /// 
        /// </summary>
        
        public class WriteLogCommand  {
            public string outputString;
                        public bool alsoToConsole=true;
            
            
        }

		protected void WriteLogCommandHandler  (WriteLogCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("WriteLogCommand");
#endif
        _service.WriteLog(cmd.outputString,cmd.alsoToConsole);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("WriteLogCommand");
#endif
        }
        

        
        /// <summary>
        /// 
        /// </summary>
        
        public class ActivateLuaReplayScriptCommand  {
            public bool activate;
            
            
        }

		protected void ActivateLuaReplayScriptCommandHandler  (ActivateLuaReplayScriptCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("ActivateLuaReplayScriptCommand");
#endif
        _service.ActivateLuaReplayScript(cmd.activate);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("ActivateLuaReplayScriptCommand");
#endif
        }
        

        
        /// <summary>
        /// 
        /// </summary>
        
        public class LuaScriptActivatedCommand  {
            public bool result;
            
            
        }

		protected void LuaScriptActivatedCommandHandler  (LuaScriptActivatedCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("LuaScriptActivatedCommand");
#endif
        
            cmd.result = _service.LuaScriptActivated();
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("LuaScriptActivatedCommand");
#endif
        }
        

        
        /// <summary>
        /// Save to this filename in the scripting-folder
        /// </summary>
        
        public class SaveCurrentLuaReplayCommand  {
            public string fileName;
            
            
        }

		protected void SaveCurrentLuaReplayCommandHandler  (SaveCurrentLuaReplayCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("SaveCurrentLuaReplayCommand");
#endif
        _service.SaveCurrentLuaReplay(cmd.fileName);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("SaveCurrentLuaReplayCommand");
#endif
        }
        

        
        /// <summary>
        /// Get the current lua-replay as script
        /// </summary>
        
        public class GetCurrentLuaReplayCommand  {
            public string result;
            
            
        }

		protected void GetCurrentLuaReplayCommandHandler  (GetCurrentLuaReplayCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("GetCurrentLuaReplayCommand");
#endif
        
            cmd.result = _service.GetCurrentLuaReplay();
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("GetCurrentLuaReplayCommand");
#endif
        }
        

        
        /// <summary>
        /// 
        /// </summary>
        
        public class GetLuaReplayStringBuilderCommand  {
            public System.Text.StringBuilder result;
            
            
        }

		protected void GetLuaReplayStringBuilderCommandHandler  (GetLuaReplayStringBuilderCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("GetLuaReplayStringBuilderCommand");
#endif
        
            cmd.result = _service.GetLuaReplayStringBuilder();
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("GetLuaReplayStringBuilderCommand");
#endif
        }
        

        
        /// <summary>
        /// 
        /// </summary>
        
        public class SetLuaReplayStringBuilderCommand  {
            public StringBuilder replayScript;
            
            
        }

		protected void SetLuaReplayStringBuilderCommandHandler  (SetLuaReplayStringBuilderCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("SetLuaReplayStringBuilderCommand");
#endif
        _service.SetLuaReplayStringBuilder(cmd.replayScript);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("SetLuaReplayStringBuilderCommand");
#endif
        }
        

        
        /// <summary>
        /// Sets a func to get the current gametime that is used for ReplayWrite
        /// </summary>
        
        public class SetLuaReplayGetGameTimeFuncCommand  {
            public Func<float> getCurrentGameTime;
            
            
        }

		protected void SetLuaReplayGetGameTimeFuncCommandHandler  (SetLuaReplayGetGameTimeFuncCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("SetLuaReplayGetGameTimeFuncCommand");
#endif
        _service.SetLuaReplayGetGameTimeFunc(cmd.getCurrentGameTime);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("SetLuaReplayGetGameTimeFuncCommand");
#endif
        }
        

        
        /// <summary>
        /// 
        /// </summary>
        
        public class ReplayWrite_RegisterEntityCommand  {
            public string entityVarName="entity";
            
            
        }

		protected void ReplayWrite_RegisterEntityCommandHandler  (ReplayWrite_RegisterEntityCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("ReplayWrite_RegisterEntityCommand");
#endif
        _service.ReplayWrite_RegisterEntity(cmd.entityVarName);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("ReplayWrite_RegisterEntityCommand");
#endif
        }
        

        
        /// <summary>
        /// 
        /// </summary>
        
        public class ReplayWrite_CustomLuaCommand  {
            public string luaScript;
                        public bool waitForGameTime=true;
            
            
        }

		protected void ReplayWrite_CustomLuaCommandHandler  (ReplayWrite_CustomLuaCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("ReplayWrite_CustomLuaCommand");
#endif
        _service.ReplayWrite_CustomLua(cmd.luaScript,cmd.waitForGameTime);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("ReplayWrite_CustomLuaCommand");
#endif
        }
        

        
        /// <summary>
        /// 
        /// </summary>
        
        public class ReplayWrite_SetCurrentEntityCommand  {
            public ECS.UID uid;
            
            
        }

		protected void ReplayWrite_SetCurrentEntityCommandHandler  (ReplayWrite_SetCurrentEntityCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("ReplayWrite_SetCurrentEntityCommand");
#endif
        _service.ReplayWrite_SetCurrentEntity(cmd.uid);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("ReplayWrite_SetCurrentEntityCommand");
#endif
        }
        
    }


    public class CommandsInstaller : Installer<CommandsInstaller>{
        public override void InstallBindings()
        {
            Commands cmds = Container.Instantiate<Commands>();
            // commented out due to zenject update (26.06.18)
            //Container.BindAllInterfaces<Commands>().FromInstance(cmds);
        }
    }
}
///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 


