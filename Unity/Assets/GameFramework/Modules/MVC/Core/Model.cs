using System;
using Zenject;
using UniRx;
using Service.Serializer;
using Service.Events;
using System.Runtime.Serialization;

namespace MVC{
    [DataContract(IsReference = true)]
    public class Model : IModel {

        /// <summary>
        /// Skip Debug.LogError(...)/Exception-output if the Model is disposed but not bound
        /// </summary>
        protected bool skipWarning = false;

        ReactiveCommand _OnDisposing;
        /// <summary>
        /// Fired when model is disposing
        /// </summary>
        public ReactiveCommand OnDisposing {
            get {
                return _OnDisposing;
            }
            private set {
                _OnDisposing = value;
            }
        }

        private CompositeDisposable Disposables;

        protected DiContainer Container;
        DisposableManager _dManager;
        ISerializerService _serializerService;
        protected IEventsService _eventService;

        protected bool initialized = false;
        private bool wasPreBound = false;
        private bool wasConstructed = false;
        public Model(){
            TryOnConstruct();
        }

        [OnDeserializing]
        void _Deserializing(StreamingContext context) {
            TryOnConstruct();
        }

        void TryOnConstruct() {
            if (!wasConstructed) OnConstruct();
        }

        /// <summary>
        /// Executed when constructor is called, OnDeserializingAttribute is fired or Bind() is called. Will not be executed again, if already executed in the past.
        /// </summary>
        protected virtual void OnConstruct() {
            wasConstructed = true;
            if (Disposables == null) Disposables = new CompositeDisposable();
            OnDisposing = new ReactiveCommand();
        }

        

        [Inject]
        void _Initialize(
            [Inject] DiContainer _container,
            [Inject] DisposableManager dManager,
            [Inject] ISerializerService serializerService,
            [Inject] IEventsService eventService)
        {
            if(initialized) return;
            initialized = true;

            Bind();

            Container = _container;
            _dManager = dManager;
            _eventService = eventService;
            _serializerService = serializerService;

            _dManager.Add(this);

            AfterBind();
        }

        /// <summary>
        /// Bind this instance.
        /// </summary>
        /// <param name="delayAfterBind">If set to true, will delay AfterBind execution 1 frame. This might be needed when deserializing. Default is false.</param>
        public virtual void Bind()
        {
            if (!wasPreBound) PreBind();
            if (!initialized) Kernel.Instance.Inject(this);
        }

        protected virtual void PreBind() {
            wasPreBound = true;
        }

        /// <summary>
        /// Called after the Model has been bound and initialized
        /// </summary>
        protected virtual void AfterBind(){
        }

        /// <summary>
        /// Serialize this instance to a string
        /// </summary>
        public virtual string Serialize()
        {
            return _serializerService.Serialize(this);//t
        }

        /// <summary>
        /// Deserialize the supplied data to this instance
        /// </summary>
        /// <param name="json">Json.</param>
        public virtual void Deserialize(string data)
        {
            _serializerService.DeserializeToInstance(this, data);
        }

        /// <summary>
        /// Copies the values from the supplied model to this model. Both properties and fields are copied.
        /// </summary>
        /// <param name="model">Model.</param>
        public virtual void CopyValuesFromOtherModel(IModel model){
            model.CopyProperties(this);
            model.CopyFields(this);
        }

        /// <summary>
        /// Publish the specified global event.
        /// </summary>
        /// <param name="evt">Evt.</param>
        protected void Publish(object evt)
        {
            _eventService.Publish(evt);
        }
        protected void Publish(object evt, Subject<object> eventStream) {
            _eventService.Publish(evt, eventStream);
        }

        /// <summary>
        /// Subscribe to a global event of type TEvent
        /// </summary>
        /// <typeparam name="TEvent">The 1st type parameter.</typeparam>
        protected IObservable<TEvent> OnEvent<TEvent>(){
            return _eventService.OnEvent<TEvent>();
        }
        protected IObservable<TEvent> OnEvent<TEvent>(Subject<object> eventStream) {
            return _eventService.OnEvent<TEvent>(eventStream);
        }

        public void AddDisposable(IDisposable disposable)
        {
            Disposables.Add(disposable);
        }

        public bool wasDisposed { get; private set; }
        public void Dispose()
        {
            if(wasDisposed || Kernel.applicationQuitting || this == null) return;
            wasDisposed = true;


            Disposables.Dispose();

            OnDispose();

            OnDisposing.Execute();
            OnDisposing.Dispose();
            OnDisposing = null;

            //If an error is thrown here, you are trying to dispose a model that was not bound. This should never happen/you should always bind your models and this comment is just here so you know whats wrong :)
            //_dManager.Remove(this);
            //If an error is thrown here, you are trying to dispose a model that was not bound. This should never happen/you should always bind your models and this comment is just here so you know whats wrong :)
            try {
                _dManager.Remove(this);
            }
            catch (Exception e) {
                if (!skipWarning) { // sry, but I have some cases(in my NPC-Nodes) where I have a Model but unbound. I shouldn't have used MVC-Model in the first place....
                    UnityEngine.Debug.LogError("Tried to dispose unbound MVC-Model:" + GetType().ToString());
                    UnityEngine.Debug.LogException(e);
                }
            }
        }

        public virtual void OnDispose(){
        }
    }
}