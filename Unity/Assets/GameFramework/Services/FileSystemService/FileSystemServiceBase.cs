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

namespace Service.FileSystem
{
    public  abstract class FileSystemServiceBase : DefaultSerializable2,IFileSystemService, IDisposable
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
                System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
                watch.Start();
                AfterInitialize();
                watch.Stop();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogWarning($"Service FileSystemServiceBase AfterInitialize() took {watch.Elapsed.TotalSeconds}s");
#endif
                Observable.NextFrame().Subscribe(_ => {
                    InitAPI();
                });
            }
            catch (Exception e) {
                UnityEngine.Debug.LogError("Catched exception in AfterInitialize() from service:" + GetType());
                UnityEngine.Debug.LogException(e);
            }
        }

        protected abstract void InitAPI();

        protected void ActivateDefaultScripting(string name) {
            try {
                var scriptService = Kernel.Instance.Resolve<Service.Scripting.IScriptingService>();
                var mainscript = scriptService.GetMainScript();
                mainscript.Globals[name] = this;
            }
            catch (Exception e) {
                UnityEngine.Debug.LogError("Error activating default scripting for Service.FileSystem with lua-name:" + name);
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

        
                                                          
        public abstract             string GetPath(FSDomain domain,string realtivePart="");

        
        public abstract             bool WriteBytesToFile(string pathToFile,byte[] bytes,bool compress=false);

        
        public abstract             bool WriteBytesToFileAtDomain(FSDomain domain,string relativePathToFile,byte[] bytes,bool compress=false);

        
        public abstract             bool WriteStringToFile(string pathToFile,string thedata,bool append=false);

        
        public abstract             bool WriteStringToFileAtDomain(FSDomain domain,string relativePathToFile,string thedata,bool append=false);

        
        public abstract             string LoadFileAsString(string pathToFile,bool compressed=false);

        
        public abstract             string LoadFileAsStringAtDomain(FSDomain domain,string relativePathToFile);

        
        public abstract             byte[] LoadFileAsBytes(string pathToFile,bool compressed=false,int estimatedUncompressedSize=0);

        
        public abstract             byte[] LoadFileAsBytesAtDomain(FSDomain domain,string relativePathToFile,bool compressed=false,int estimatedUncompressedSize=0);

        
        public abstract             List<string> GetFilesInAbsFolder(string absPath,string pattern="*.*",bool recursive=false);

        
        public abstract             List<string> GetFilesInDomain(FSDomain domain,string innerDomainPath="",string filter="*.*",bool recursive=false);

        
        public abstract             void RemoveFile(string filePath);

        
        public abstract             void RemoveFileInDomain(FSDomain domain,string relativePath);

        
        public abstract             bool FileExists(string pathToFile);

        
        public abstract             bool FileExistsInDomain(FSDomain domain,string relativePath);

        
        public abstract             void SetPersistentRoot(string root);

        
        public abstract             long GetMaxAvailableSavegameStorage();

        
        public abstract             long GetCurrentlyUsedSavegameStorage();

        


        
        public virtual int Serialize(FlatBufferBuilder builder) {
            UnityEngine.Debug.LogError("No serializer for FileSystemServiceBase implemented");
            return 0;
        }

        public virtual void Deserialize(object incoming) {
            UnityEngine.Debug.LogError("No deserializer for FileSystemServiceBase implemented");
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
