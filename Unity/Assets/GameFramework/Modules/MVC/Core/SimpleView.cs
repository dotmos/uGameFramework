using System.Collections;
using Zenject;
using MVC;
using UniRx;
using System;

namespace MVC{
    public class SimpleView : ITickable, IView {
        /// <summary>
        /// Dispose the controller when Dispose is called
        /// </summary>
        public bool disposeControllerOnDispose = true;
        /// <summary>
        /// If set to true, a controller and model will be created when view is bound. This can only be set from PreBind()
        /// </summary>
        public bool createController = false;

        public ReactiveProperty<bool> ViewInitializedProperty = new ReactiveProperty<bool>(false);
        /// <summary>
        /// Set to true, once view is ready for action. This will be set to true when AfterBind() is fired
        /// </summary>
        /// <value><c>true</c> if view ready; otherwise, <c>false</c>.</value>
        public bool ViewInitialized {
            get{
                return ViewInitializedProperty.Value;
            }
            private set{
                ViewInitializedProperty.Value = value;
            }
        }

        public ReactiveProperty<IController> ControllerProperty = new ReactiveProperty<IController>();
        public IController Controller{
            get {
                return ControllerProperty.Value;
            }
            protected set{
                ControllerProperty.Value = value;
            }
        }
        public IModel Model{
            get{
                return Controller.GetModel();
            }
        }

        public DiContainer Container {
            get;
            private set;
        }
        private Service.Events.IEventsService _eventService;
        private DisposableManager _dManager;
        private TickableManager _tManager;

        [Inject]
        public void Initialize(
            [Inject] DiContainer diContainer,
            [Inject] DisposableManager dManager,
            [Inject] TickableManager tManager,
            [Inject] Service.Events.IEventsService eventService)
        {
            if(ViewInitialized) return;
            ViewInitialized = true;

			PreBind();

            Container = diContainer;
            _dManager = dManager;
            _tManager = tManager;
            _eventService = eventService;

            //Create controller if this view is created through editor/sceneload
			if (createController && Controller == null) {
				IController _c = CreateController ();
				_c.Bind ();
				SetController (_c);
			}

            //To make IDisposable work, we have to add this instance to the disposable manager
            _dManager.Add(this);

            //Make ITickable work
            AddTickable(this);

            //Bind all interfaces for this instance
            //Container.BindAllInterfacesFromInstance(this);
            Container.Bind<IView>().FromInstance(this);
            Container.Bind<ITickable>().FromInstance(this);

            //Debug.Log(this + " init");

            AfterBind();
        }

        public void Bind(){
            //If this view is part of the scenefile, make sure it gets initialized/injected
            if(!ViewInitialized)
            {
                if(Kernel.Instance != null) Kernel.Instance.Inject(this);
            }
        }

        /// <summary>
        /// Called before this instance is injected to Kernel.Container. Nothing has been injected yet.
        /// </summary>
        protected virtual void PreBind(){
        }

        /// <summary>
        /// Called after this intance is injected to Kernel.Container. Everything for this instance is injected by now
        /// </summary>
        protected virtual void AfterBind(){
        }

        /// <summary>
        /// Makes this ITickable work. Override to implement your own tickable manager (don't forget to also override RemoveTickable())
        /// </summary>
        /// <param name="tickable">Tickable.</param>
        protected virtual void AddTickable(ITickable tickable){
            _tManager.Add(tickable);
        }

        /// <summary>
        /// Removes this ITickable from TickableManager
        /// </summary>
        /// <param name="tickable">Tickable.</param>
        protected virtual void RemoveTickable(ITickable tickable){
            _tManager.Remove(tickable);
        }

        /// <summary>
        /// Publish the specified global event.
        /// </summary>
        /// <param name="evt">Evt.</param>
        public void Publish(object evt)
        {
            _eventService.Publish(evt);
        }
        public void Publish(object evt, Subject<object> eventStream) {
            _eventService.Publish(evt, eventStream);
        }

        /// <summary>
        /// Subscribe to a global event of type TEvent
        /// </summary>
        /// <typeparam name="TEvent">The 1st type parameter.</typeparam>
        public IObservable<TEvent> OnEvent<TEvent>(){
            return _eventService.OnEvent<TEvent>();
        }
        public IObservable<TEvent> OnEvent<TEvent>(Subject<object> eventStream) {
            return _eventService.OnEvent<TEvent>(eventStream);
        }

        /// <summary>
        /// Creates a default controller for this view. This is used when createController is set to true. Controller will NOT be set for this view. You have to call SetController() manually.
        /// </summary>
        /// <returns>The controller.</returns>
        public virtual IController CreateController(){
            throw new Exception("Not implemented! You are trying to create a default controller, but have not implemented it. Please override the CreateController() function and add your implementation!");
        }

        /// <summary>
        /// Creates a controller of type TController. Controller will NOT be set for this view. You have to call SetController() manually
        /// </summary>
        /// <returns>The controller.</returns>
		public virtual IController CreateController<TController>() where TController : IController,new()
        {
			return new TController();
        }

        /// <summary>
        /// Creates a controller of type TController. Controller will NOT be set for this view. You have to call SetController() manually
        /// </summary>
        /// <returns>The controller.</returns>
        public virtual IController CreateController(Type controllerType)
        {
			return (IController)Container.Instantiate(controllerType);
        }

        /// <summary>
        /// Sets the controller for this view
        /// </summary>
        /// <param name="controller">Controller.</param>
        public virtual void SetController(IController controller)
        {
            Controller = controller;
//            if(!ViewInitialized){
//                Kernel.Instance.Inject(this);
//            }
        }

        /// <summary>
        /// Creates and sets the controller for this view
        /// </summary>
        /// <param name="controller">Controller.</param>
		public virtual void SetController<TController>() where TController : IController, new(){
            SetController( CreateController<TController>() );
        }

        /// <summary>
        /// Creates and sets the controller for this view
        /// </summary>
        /// <param name="controller">Controller.</param>
        public virtual void SetController(Type controllerType){
            SetController( CreateController(controllerType));
        }

        /// <summary>
        /// Listen to an event the controller publishes to the view
        /// </summary>
        /// <typeparam name="TEvent">The 1st type parameter.</typeparam>
        protected IObservable<TEvent> OnController<TEvent>(){
            if(Controller != null) return Controller.OnController<TEvent>();

            throw new Exception("No controller set! Can not listen to controller event!");
        }

        /// <summary>
        /// Publishs an event from the view to the controller.
        /// </summary>
        /// <param name="evt">Evt.</param>
        protected void PublishToController(object evt)
        {
            if(Controller != null) Controller.PublishToController(evt);
        }

        bool wasDisposed;
        public void Dispose()
        {
            if(this == null || wasDisposed || !ViewInitialized) return;

            wasDisposed = true;

            OnDispose();

            RemoveTickable(this);
            _dManager.Remove(this);

            //Debug.Log(this + " disposed");

            if(disposeControllerOnDispose && Controller != null)
            {
                Controller.Dispose();
            }
            Controller = null;

            _eventService = null;
            _dManager = null;
            _tManager = null;
        }

        protected virtual void OnDispose(){}

        public virtual void Tick(){}
    }

	public class SimpleView<TController> : SimpleView, ITickable, IView where TController : IController, new(){
        new public ReactiveProperty<TController> ControllerProperty = new ReactiveProperty<TController>();
        new public TController Controller{
            get {
                return ControllerProperty.Value;
            }
            protected set{
                ControllerProperty.Value = value;
                base.ControllerProperty.Value = value;
            }
        }

        /// <summary>
        /// Sets the controller for this view
        /// </summary>
        /// <param name="controller">Controller.</param>
        public override void SetController(IController controller)
        {
            Controller = (TController)controller;
//            if(!ViewInitialized){
//                Kernel.Instance.Inject(this);
//            }
        }

        /// <summary>
        /// Creates a default controller for this view. This is used when createController is set to true. Controller will NOT be set for this view. You have to call SetController() manually.
        /// </summary>
        /// <returns>The controller.</returns>
        public override IController CreateController()
        {
			return new TController();
        }
    }
}