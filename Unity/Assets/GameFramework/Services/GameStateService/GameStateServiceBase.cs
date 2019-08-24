///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 
using System.Collections.Generic;
using MoonSharp.Interpreter;


using FlatBuffers;
using UniRx;
using Zenject;
using System;
using System.Threading;

namespace Service.GameStateService
{
    public  abstract class GameStateServiceBase : IGameStateService, IDisposable
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

        protected bool mainThreadRegisteredForExecution = false;
        protected bool mainThreadExecuted = false;
        protected Semaphore mainThreadSemaphore = new Semaphore(0, 1);

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
                UnityEngine.Debug.LogError("Error activating default scripting for Service.GameStateService with lua-name:" + name);
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

        
                                                          
        public abstract GameState RegisterGameState(string name,GameState gamestate=null);

        
        public abstract GameState GetCurrentGameState();

        
        public abstract IObservable<bool> StartGameState(GameState gamestate,GSContext ctx=null);

        
        public abstract IObservable<bool> StopGameState(GameState gamestate);

        
        public abstract GameState GetGameState(string name);

        
        public abstract void Tick(float deltaTime,float unscaledDeltaTime);

        

        public virtual int Serialize(FlatBufferBuilder builder) {
            UnityEngine.Debug.LogError("No serializer for GameStateServiceBase implemented");
            return 0;
        }

        public virtual void Deserialize(object incoming) {
            UnityEngine.Debug.LogError("No deserializer for GameStateServiceBase implemented");
        }

        public virtual void Deserialize(ByteBuffer buf) {
            throw new NotImplementedException();
        }

        public void WaitForWorkOnMainThreadFinished() {
            if (!Kernel.Instance.IsMainThread()) {
                mainThreadSemaphore.WaitOne();
            } else {
                UnityEngine.Debug.LogError("Waiting for mainthread on mainthread,...");
            }
        }

        public void _RunOnMainThreadLogic() { mainThreadExecuted = true; mainThreadSemaphore.Release(); }
        /// <summary>
        /// Flag is already finished with execution. Override if you implement real logic
        /// </summary>
        /// <returns></returns>
        public virtual bool IsRunOnMainFinished() { return mainThreadExecuted; }
        /// <summary>
        /// Is this object already added to be executed on main-thread?
        /// </summary>
        /// <returns></returns>
        public bool IsRunOnMainRegistered() { return mainThreadRegisteredForExecution; }
        /// <summary>
        /// Register the actions to the main thread here
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public virtual Action RegisterRunOnMainThread(object ctx) { return _RunOnMainThreadLogic; }
        /// <summary>
        /// Reset values. Default sets values to prevent execution. Override to use this mechanism
        /// </summary>
        public virtual void ResetRunOnMainThread() {
            mainThreadExecuted = true;
            mainThreadRegisteredForExecution = true;
        }

    }
}
///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 
