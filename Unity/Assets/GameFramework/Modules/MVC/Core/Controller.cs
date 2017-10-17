using UniRx;
using Zenject;
using System;
using Service.Events;
using UnityEngine;

namespace MVC{
    public class Controller : IController{

        /// <summary>
        /// Fired when controller is disposing
        /// </summary>
        ReactiveCommand _OnDisposing = new ReactiveCommand();
        public ReactiveCommand OnDisposing {
            get {
                return _OnDisposing;
            }
            private set {
                _OnDisposing = value;
            }
        }

        /// <summary>
        /// Auto dispose controller if model is disposed. Default is false.
        /// </summary>
        public bool disposeOnModelDispose = false;
        IDisposable modelDisposeListener;

        protected bool initialized = false;

        /// <summary>
        /// Automatically create a new model when controller is created. Default is false.
        /// </summary>
        public bool createModelOnBind = false;

        /// <summary>
        /// Dispose the model when controller is disposed. Default is true.
        /// </summary>
        public bool disposeModel = true;

        /// <summary>
        /// The Model Property of this controller
        /// </summary>
        /// <value>The model property.</value>
        public ReactiveProperty<IModel> ModelProperty = new ReactiveProperty<IModel>();
        /// <summary>
        /// The model of this controller
        /// </summary>
        /// <value>The model.</value>
        public IModel Model {
            get{
                return ModelProperty.Value;
            }
            private set{
                ModelProperty.Value = value;
            }
        }

        //Disposables
        private ReactiveProperty<CompositeDisposable> DisposablesProperty = new ReactiveProperty<CompositeDisposable>(new CompositeDisposable());
        private CompositeDisposable Disposables{
            get{
                return DisposablesProperty.Value;
            }
        }

        

        protected DiContainer Container;
        private DisposableManager _dManager;
        private IEventsService _eventService;

        /// <summary>
        /// Container for firing view events
        /// </summary>
        Subject<object> viewEventsSubject = new Subject<object>();
        /// <summary>
        /// Container for firing controller events
        /// </summary>
        Subject<object> controllerEventsSubject = new Subject<object>();

//        public Controller(){
//            Bind();
//        }

        /// <summary>
        /// Initialize the controller and inject everything
        /// </summary>
        /// <param name="container">Container.</param>
        /// <param name="eventService">Event service.</param>
        /// <param name="dManager">D manager.</param>
        [Inject]
        private void _Initialize(
            [Inject] DiContainer container,
            [Inject] IEventsService eventService,
            [Inject] DisposableManager dManager
        )
        {
            if(initialized == true) return;
            initialized = true;

            PreBind();

            Container = container;
            _dManager = dManager;
            _eventService = eventService;

            _dManager.Add(this);

            this.ModelProperty.DistinctUntilChanged().Subscribe(e => ListenToModelDispose()).AddTo(this);

            if (createModelOnBind && Model == null) {
                IModel _m = CreateModel();
                _m.Bind();
                SetModel(_m);
            }

            AfterBind();

            //UnityEngine.Debug.Log(this + " init");
        }

        /// <summary>
        /// Bind this instance.
        /// </summary>
        public virtual void Bind(){
            if(!initialized) Kernel.Instance.Inject(this);
        }

        protected virtual void PreBind(){}
        protected virtual void AfterBind(){}


        /// <summary>
        /// Use this to listen to an event from the controller (views will most likely listen to controller events)
        /// </summary>
        /// <typeparam name="TEvent">The 1st type parameter.</typeparam>
        public IObservable<TEvent> OnController<TEvent>()
        {
            return controllerEventsSubject.Where(p =>
            {
                return p is TEvent;
            }).Select(delegate(object p)
            {
                return (TEvent)p;
            });
        }
        /// <summary>
        /// Use this to publish events to the view
        /// </summary>
        /// <param name="evt">Evt.</param>
        /// <typeparam name="TEvent">The 1st type parameter.</typeparam>
        protected void PublishToViews<TEvent>(TEvent evt)
        {
            controllerEventsSubject.OnNext(evt);
        }
            
        /// <summary>
        /// Use this to listen to an event from the view which is meant for the controller
        /// </summary>
        /// <typeparam name="TEvent">The 1st type parameter.</typeparam>
        protected IObservable<TEvent> OnView<TEvent>()
        {
            return viewEventsSubject.Where(p =>
            {
                return p is TEvent;
            }).Select(delegate(object p)
            {
                return (TEvent)p;
            });
        }
        /// <summary>
        /// Use this to publish an event to the controller
        /// </summary>
        /// <param name="evt">Evt.</param>
        /// <typeparam name="TEvent">The 1st type parameter.</typeparam>
        public void PublishToController<TEvent>(TEvent evt)
        {
            viewEventsSubject.OnNext(evt);
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
        /// Publish this event async. Choose the behaviour via the async-type and be informed for the result via the onFinished-callback.
        /// These are the call-options:
        /// Publish_Worker_Result_Worker - Do the publish call on the worker-thread and call the callback immediately with the result inside this thread
        /// Publish_Worker_Result_Main   - Do the publish call on the worker-thread and add the callback with the result inside to the mainthread-queue
        /// Publish_Main_Result_Main     - Queue the publish call in the MAIN-thread queue and call the callback immediately within the MAIN-thread
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="evt"></param>
        /// <param name="onFinished"></param>
        /// <param name="asyncType"></param>
        protected void PublishAsync<TEvent>(TEvent evt,Action<TEvent> onFinished=null,AsyncType asyncType=AsyncType.Publish_Main_Result_Main) {
            _eventService.PublishAsync(evt, onFinished, asyncType);
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

        /// <summary>
        /// Creates a model which is suitable for the controller. Does NOT assign the model to the controller
        /// </summary>
        /// <returns>The model.</returns>
        public virtual IModel CreateModel(){
            throw new Exception("Not implemented! You are trying to create a default model, but have not implemented it. Please override the CreateModel() function and add your implementation!");
        }

        /// <summary>
        /// Creates a model. Does NOT assign the model to the controller
        /// </summary>
        /// <returns>The model.</returns>
        /// <typeparam name="TModel">The 1st type parameter.</typeparam>
		public virtual TModel CreateModel<TModel>() where TModel : IModel, new(){
			return new TModel();
        }

        /// <summary>
        /// Creates a model. Does NOT assign the model to the controller
        /// </summary>
        /// <returns>The model.</returns>
        /// <param name="modelType">Model type.</param>
        public virtual IModel CreateModel(Type modelType){
			return (IModel)Container.Instantiate(modelType);
        }

        /// <summary>
        /// Sets the model for this controller
        /// </summary>
        /// <param name="model">Model.</param>
        public virtual void SetModel(IModel model){
            Model = model;
        }

        /// <summary>
        /// Sets the model.
        /// </summary>
        /// <typeparam name="TModel">The 1st type parameter.</typeparam>
		public virtual void SetModel<TModel>() where TModel : IModel, new(){
            SetModel( CreateModel<TModel>());
        }

        /// <summary>
        /// Sets the model.
        /// </summary>
        /// <param name="model">Model.</param>
        /// <param name="modelType">Model type.</param>
        public virtual void SetModel(Type modelType){
            SetModel( CreateModel(modelType));
        }

       /// <summary>
       /// Copies values from the supplied model and puts them in the controller model
       /// </summary>
       /// <param name="model">Model.</param>
        public virtual void CopyModelValues(IModel model){
            if(Model == null){
                SetModel(CreateModel());
            }
            Model.CopyValues(model);
        }

        /// <summary>
        /// Helper function for getting Model in (Mono)View
        /// </summary>
        /// <returns>The model.</returns>
        public IModel GetModel()
        {
            return Model;
        }

        public void AddDisposable(IDisposable disposable)
        {
            Disposables.Add(disposable);
        }

        /// <summary>
        /// Used internally to listen to model dispose. No need to call this manually.
        /// </summary>
        protected void ListenToModelDispose() {
            if (modelDisposeListener != null) modelDisposeListener.Dispose();
            if(Model != null) {
                modelDisposeListener = Model.OnDisposing.Subscribe(e => OnModelDispose());
                modelDisposeListener.AddTo(this);
            }
        }

        void OnModelDispose() {
            //Dispose controller on model dispose?
            if (disposeOnModelDispose) Dispose();
        }

        bool wasDisposed;
        public void Dispose(){
            if(wasDisposed || Kernel.applicationQuitting) return;

            //Tell subscribers that controller is disposing
            OnDisposing.Execute();
            OnDisposing.Dispose();
            OnDisposing = null;


            wasDisposed = true;

            Disposables.Dispose();
            DisposablesProperty.Dispose();
            DisposablesProperty = null;

            OnDispose();

            controllerEventsSubject.Dispose();
            controllerEventsSubject = null;
            viewEventsSubject.Dispose();
            viewEventsSubject = null;

            if(disposeModel && Model != null) Model.Dispose();
            Model = null;
            Container = null;
            _eventService = null;
            _dManager = null;

            ModelProperty.Dispose();
            ModelProperty = null;
        }

        public virtual void OnDispose(){}
    }

    public class Controller<TModel> : Controller where TModel : IModel,new(){
        /// <summary>
        /// The Model Property of this controller
        /// </summary>
        /// <value>The model property.</value>
        new public ReactiveProperty<TModel> ModelProperty = new ReactiveProperty<TModel>();
        /// <summary>
        /// The model of this controller
        /// </summary>
        /// <value>The model.</value>
        new public TModel Model {
            get{
                return ModelProperty.Value;
            }
            private set{
                ModelProperty.Value = value;
                base.ModelProperty.Value = value;
            }
        }

        /// <summary>
        /// Creates a model which is suitable for the controller
        /// </summary>
        /// <returns>The model.</returns>
        public override IModel CreateModel(){
			return new TModel ();
        }

        /// <summary>
        /// Sets the model for this controller
        /// </summary>
        /// <param name="model">Model.</param>
        public override void SetModel(IModel model){
            Model = (TModel)model;
        }

        protected override void AfterBind() {
            base.AfterBind();

            this.ModelProperty.DistinctUntilChanged().Subscribe(e => ListenToModelDispose()).AddTo(this);
        }

        public override void OnDispose() {
            base.OnDispose();

            ModelProperty.Dispose();
            ModelProperty = null;
        }
    }
}