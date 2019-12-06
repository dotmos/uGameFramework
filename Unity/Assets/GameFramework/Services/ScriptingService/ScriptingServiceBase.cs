///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 
using System.Collections.Generic;
using MoonSharp.Interpreter;
using System.Text;
using ECS;


using FlatBuffers;
using UniRx;
using Zenject;
using System;
using System.Threading;
using Service.Serializer;

namespace Service.Scripting
{
    public  abstract class ScriptingServiceBase : IScriptingService, IDisposable
    {
        protected DisposableManager _dManager;
        protected Service.Events.IEventsService _eventService;
        protected Service.AsyncManager.IAsyncManager _asyncManager;


        protected CompositeDisposable disposables = new CompositeDisposable();

        protected ReactivePriorityExecutionList rxOnStartup {
            get { return Kernel.Instance.rxStartup; }
        }
        protected ReactivePriorityExecutionList rxOnShutdown {
            get { return Kernel.Instance.rxShutDown; }
        }
        protected bool DeSerializationFinished = false;
        protected Semaphore deSerializationFinishedSempahore;

        protected Service.Scripting.IScriptingService _scriptingService;
        protected Service.Scripting.IScriptingService ScriptingService {
            get {
                if (_scriptingService == null) _scriptingService = Kernel.Instance.Container.Resolve<Service.Scripting.IScriptingService>();
                return _scriptingService;
            }
        }

        [Inject]
        void Initialize(
          [Inject] DisposableManager dManager,
          [Inject] Service.Events.IEventsService eventService,
          [Inject] Service.AsyncManager.IAsyncManager asyncManager
        ) {
            _dManager = dManager;
            _eventService = eventService;
            _asyncManager = asyncManager;
            
            // register as disposable
            _dManager.Add(this);

            try {
                AfterInitialize();
                Observable.NextFrame().Subscribe(_ => {
                    InitAPI();
                });
            }
            catch (Exception e) {
                UnityEngine.Debug.LogError("Catched exception in Service-AfterInitialize() from service:" + GetType());
                UnityEngine.Debug.LogException(e);
            }
        }

        protected abstract void InitAPI();

        protected void ActivateDefaultScripting(string name) {
            try {
                var cmdGetScript = new Service.Scripting.Commands.GetMainScriptCommand();
                Publish(cmdGetScript);
                cmdGetScript.result.Globals[name] = this;
            }
            catch (Exception e) {
                UnityEngine.Debug.LogError("Error activating default scripting for Service.Scripting with lua-name:" + name);
                UnityEngine.Debug.LogException(e);
            }
        }

        /// <summary>
        /// Publish the specified global event.
        /// </summary>
        /// <param name="evt">Evt.</param>
        protected void Publish(object evt) {
            _eventService.Publish(evt);
        }
        protected void Publish(object evt, Subject<object> eventStream) {
            _eventService.Publish(evt, eventStream);
        }

        /// <summary>
        /// Subscribe to a global event of type TEvent
        /// </summary>
        /// <typeparam name="TEvent">The 1st type parameter.</typeparam>
        protected IObservable<TEvent> OnEvent<TEvent>() {
            return _eventService.OnEvent<TEvent>();
        }
        protected IObservable<TEvent> OnEvent<TEvent>(Subject<object> eventStream) {
            return _eventService.OnEvent<TEvent>(eventStream);
        }

        public void AddDisposable(IDisposable disposable) {
            disposables.Add(disposable);
        }

        // overwrite this method to be called right after eventmanger and dManager got initalized
        protected virtual void AfterInitialize() {

        }

        bool isDisposed = false;

        public virtual void Dispose() {
            //if (isDisposed || Kernel.applicationQuitting) return;
            if (isDisposed) return;
            isDisposed = true;
            disposables.Dispose();

            OnDispose();

            _dManager.Remove(this);
        }

        protected virtual void OnDispose() { }

        
                                                          
        public abstract Script GetMainScript();

        
        public abstract string ExecuteStringOnMainScript(string luaCode);

        
        public abstract string ExecuteFileToMainScript(string fileName,bool useScriptDomain=false);

        
        public abstract DynValue ExecuteStringOnMainScriptRaw(string fileName);

        
        public abstract Proposal AutocompleteProposals(string currentInput,int cursorPos);

        
        public abstract LuaCoroutine CreateCoroutine(DynValue funcName);

        
        public abstract void Callback(string cbtype,object o2=null,object o3=null);

        
        public abstract void RegisterCallback(Action<string,object,object> cbCallbackFunc);

        
        public abstract void RegisterCustomYieldCheck(Func<LuaCoroutine,bool> coRoutines);

        
        public abstract void RegisterEntity(UID entity);

        
        public abstract bool IsEntityRegistered(UID entity);

        
        public abstract int GetLUAEntityID(UID entity);

        
        public abstract IComponent GetComponent(UID entity,string componentName);

        
        public abstract void Setup(bool isNewGame);

        
        public abstract void Tick(float dt);

        
        public abstract void StartLog(string filename);

        
        public abstract void WriteLog(string outputString,bool alsoToConsole=true);

        
        public abstract void ActivateLuaReplayScript(bool activate);

        
        public abstract bool LuaScriptActivated();

        
        public abstract void SaveCurrentLuaReplay(string fileName);

        
        public abstract string GetCurrentLuaReplay();

        
        public abstract System.Text.StringBuilder GetLuaReplayStringBuilder();

        
        public abstract void SetLuaReplayStringBuilder(StringBuilder replayScript);

        
        public abstract void SetLuaReplayGetGameTimeFunc(Func<float> getCurrentGameTime);

        
        public abstract void ReplayWrite_RegisterEntity(string entityVarName="entity");

        
        public abstract void ReplayWrite_CustomLua(string luaScript,bool waitForGameTime=true);

        
        public abstract void ReplayWrite_SetCurrentEntity(ECS.UID uid);

        


        
        public virtual int Serialize(FlatBufferBuilder builder) {
            UnityEngine.Debug.LogError("No serializer for ScriptingServiceBase implemented");
            return 0;
        }

        public virtual void Deserialize(object incoming) {
            UnityEngine.Debug.LogError("No deserializer for ScriptingServiceBase implemented");
        }

        public virtual void Deserialize(ByteBuffer buf) {
            throw new NotImplementedException();
        }




    }
}
///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 
