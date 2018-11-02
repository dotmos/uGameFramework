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
using Zenject;

namespace Service.DevUIService
{
    public  abstract class DevUIServiceBase : IDevUIService, IDisposable
    {
        protected DisposableManager _dManager;
        protected Service.Events.IEventsService _eventService;
        protected Service.AsyncManager.IAsyncManager _asyncManager;

        protected CompositeDisposable disposables = new CompositeDisposable();


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
                UnityEngine.Debug.LogError("Error activating default scripting for Service.DevUIService with lua-name:" + name);
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
            if (isDisposed || Kernel.applicationQuitting) return;
            isDisposed = true;
            disposables.Dispose();

            OnDispose();

            _dManager.Remove(this);
        }

        protected virtual void OnDispose() { }

        
                                                          
        public abstract ReactiveCollection<DevUIView> GetRxViews();
        
        public abstract DevUIView CreateView(string viewName,bool dynamicallyCreated=false,bool extensionAllowed=true);
        
        public abstract DevUIView GetView(string viewName);
        
        public abstract bool ViewNameExists(string viewName);
        
        public abstract void RemoveViewFromModel(DevUIView view);
        
        public abstract void RemoveViewToArchieve(DevUIView view);
        
        public abstract IObservable<float> LoadViews();
        
        public abstract void SaveViews();
        
        public abstract void WriteToScriptingConsole(string text);
        
        public abstract void OpenScriptingConsole();
        
        public abstract void CloseScriptingConsole();
        
        public abstract void ToggleScriptingConsole();
        
        public abstract bool IsScriptingConsoleVisible();
        
        public abstract void StartPickingEntity();
        
        public abstract DevUIView CreateViewFromEntity(UID entity,string name="");
        
        public abstract DevUIView CreateViewFromPOCO(object entity,string name);
        
    }
}
///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 
