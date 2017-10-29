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
        /// Autobind on creation? You have to set this when calling the constructor. Default is false.
        /// </summary>
        public bool autoBind = false;

        /// <summary>
        /// If set to true, will delay AfterBind() by one frame. Might be needed for deserialization
        /// </summary>
        bool delayAfterBind = false;

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

        ReactiveCommand _OnAfterBind;
        /// <summary>
        /// Fired when AfterBind is called
        /// </summary>
        public ReactiveCommand OnAfterBind {
            get { return _OnAfterBind; }
            private set { _OnAfterBind = value; }
        }

        private ReactiveProperty<CompositeDisposable> DisposablesProperty;
        private CompositeDisposable Disposables{
            get{
                return DisposablesProperty.Value;
            }
        }

        /// <summary>
        /// Every model listens to this and will bind itself when this is executed
        /// </summary>
        private static ReactiveCommand<bool> bindAllCommand = new ReactiveCommand<bool>();
        /// <summary>
        /// This will bind ALL models that were ever created. If a model was already bound, it will not be bound again.
        /// </summary>
        /// <param name="delayAfterBind">If set to true, will delay AfterBind execution 1 frame. This might be needed when deserializing. Default is false.</param>
        public static void BindAll(bool delayAfterBind = false) {
            bindAllCommand.Execute(delayAfterBind);
        }
        

        protected DiContainer Container;
        DisposableManager _dManager;
        ISerializerService _serializerService;
        IEventsService _eventService;

        protected bool initialized = false;
        private bool wasPreBound = false;
        private bool wasConstructed = false;
        public Model(){
            TryOnConstruct();
            if(autoBind) Bind();
        }

        [OnDeserializing]
        void _Deserializing(StreamingContext context) {
            TryOnConstruct();
        }

        void TryOnConstruct() {
            if (!wasConstructed) OnConstruct();
        }

        IDisposable bindAllListener;

        /// <summary>
        /// Executed when constructor is called, OnDeserializingAttribute is fired or Bind() is called. Will not be executed again, if already executed in the past.
        /// </summary>
        protected virtual void OnConstruct() {
            wasConstructed = true;
            if (DisposablesProperty == null) DisposablesProperty = new ReactiveProperty<CompositeDisposable>(new CompositeDisposable());
            OnDisposing = new ReactiveCommand();
            OnAfterBind = new ReactiveCommand();
            bindAllListener = bindAllCommand.Subscribe(e => Bind(e));
            bindAllListener.AddTo(this);
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

            Bind(delayAfterBind);

            if (bindAllListener != null) bindAllListener.Dispose();
            bindAllListener = null;

            Container = _container;
            _dManager = dManager;
            _eventService = eventService;
            _serializerService = serializerService;

            _dManager.Add(this);

            OnAfterBind.Execute();
            if (!delayAfterBind) {
                AfterBind();
            } else {
                Observable.NextFrame().Subscribe(e => AfterBind()).AddTo(this);
            }
        }

        /// <summary>
        /// Bind this instance.
        /// </summary>
        /// <param name="delayAfterBind">If set to true, will delay AfterBind execution 1 frame. This might be needed when deserializing. Default is false.</param>
        public virtual void Bind(bool delayAfterBind = false)
        {
            this.delayAfterBind = delayAfterBind;
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
        /// Copies the values from the supplied model to this model
        /// </summary>
        /// <param name="model">Model.</param>
        public virtual void CopyValues(IModel model){
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
            DisposablesProperty.Dispose();
            DisposablesProperty = null;

            OnDispose();

            OnDisposing.Execute();

            OnDisposing.Dispose();
            OnDisposing = null;

            OnAfterBind.Dispose();
            OnAfterBind = null;
            
            _dManager.Remove(this);
        }

        public virtual void OnDispose(){
        }
    }
}