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
using Service.Serializer;

namespace Service.PerformanceTest
{
    public  abstract class PerformanceTestServiceBase : IPerformanceTestService, IDisposable
    {
        protected DisposableManager _dManager;
        protected Service.Events.IEventsService _eventService;
        protected Service.AsyncManager.IAsyncManager _asyncManager;
        protected Service.Scripting.IScriptingService _scripting;

        protected CompositeDisposable disposables = new CompositeDisposable();

        protected ReactivePriorityExecutionList rxOnStartup {
            get { return Kernel.Instance.rxStartup; }
        }
        protected ReactivePriorityExecutionList rxOnShutdown {
            get { return Kernel.Instance.rxShutDown; }
        }
        protected bool DeSerializationFinished = false;
        protected Semaphore deSerializationFinishedSempahore;

        [Inject]
        void Initialize(
          [Inject] DisposableManager dManager,
          [Inject] Service.Events.IEventsService eventService,
          [Inject] Service.AsyncManager.IAsyncManager asyncManager,
          [Inject] Service.Scripting.IScriptingService scripting
        ) {
            _dManager = dManager;
            _eventService = eventService;
            _asyncManager = asyncManager;
            _scripting = scripting;
            
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
                UnityEngine.Debug.LogError("Error activating default scripting for Service.PerformanceTest with lua-name:" + name);
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

        
                                                          
        public abstract void StartWatch(string t);

        
        public abstract void StopWatch(string t);

        
        public abstract void PrintPerfTests();

        
        public abstract void Clear();

        


        
        public virtual int Serialize(FlatBufferBuilder builder) {
            UnityEngine.Debug.LogError("No serializer for PerformanceTestServiceBase implemented");
            return 0;
        }

        public virtual void Deserialize(object incoming) {
            UnityEngine.Debug.LogError("No deserializer for PerformanceTestServiceBase implemented");
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
