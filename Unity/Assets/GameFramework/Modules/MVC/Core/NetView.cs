using UnityEngine;
using System.Collections;
using Zenject;
using System;
using UniRx;
using UnityEngine.Networking;

//TODO: NetView can NOT use a generic parameter as there is a bug/restriction in NetworkBehaviour/UNetWeaver. As soon as this is fixed by Unity, reimplement generic parameter for easier controller handling
namespace MVC{
    public class NetView : NetworkBehaviour, ITickable, IView, Service.GlobalNetwork.INetworkTickableObject{

        /// <summary>
        /// Automatic dispose on object destruction
        /// </summary>
        public bool disposeOnDestroy = true;
        /// <summary>
        /// Destroy the object when Dispose is called
        /// </summary>
        public bool destroyOnDispose = true;
        /// <summary>
        /// Dispose the controller when Dispose is called
        /// </summary>
        public bool disposeControllerOnDispose = true;
        /// <summary>
        /// If set to true, a controller and model will be created when view is initialized. This can only be set from the editor or in PreBind()
        /// Otherwise it will do nothing. When creating a view from code, you should use MVCService.CreateView() instead
        /// </summary>
        public bool createController = false;
        /// <summary>
        /// If set to true, view will have it's dependencies injected and is initialized on startup. This can only be set from the editor or in PreBind().
        /// Do NOT set this to true when creating the view through MVCService.CreateView() !
        /// </summary>
        public bool bindOnAwake = true;

        protected ReactiveProperty<bool> ViewInitializedProperty = new ReactiveProperty<bool>(false);
        /// <summary>
        /// Set to true, once view is ready for action. This will be set to true when AfterBind() is fired
        /// </summary>
        /// <value><c>true</c> if view ready; otherwise, <c>false</c>.</value>
        protected bool ViewInitialized {
            get{
                return ViewInitializedProperty.Value;
            }
            set{
                ViewInitializedProperty.Value = value;
            }
        }

        public ReactiveProperty<IController> ControllerProperty = new ReactiveProperty<IController>();
        public IController Controller{
            get {
                return ControllerProperty.Value;
            }
            private set{
                ControllerProperty.Value = value;
            }
        }
        public IModel Model{
            get{
                return Controller.GetModel();
            }
        }

        protected DiContainer Container {
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

        void Awake()
        {
            //If this view is part of the scenefile, make sure it gets initialized/injected
            if(bindOnAwake)
            {
                Bind();
            }
        }

        public void Bind(){
            if(!ViewInitialized){
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
            Publish(new Service.GlobalNetwork.Commands.AddTickableCommand(){tickable = this});
        }

        /// <summary>
        /// Removes this ITickable from TickableManager
        /// </summary>
        /// <param name="tickable">Tickable.</param>
        protected virtual void RemoveTickable(ITickable tickable){
            _tManager.Remove(tickable);
            Publish(new Service.GlobalNetwork.Commands.RemoveTickableCommand(){tickable = this});
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

        /// <summary>
        /// Creates a default controller for this view. This is used when createController is set to true. Controller will NOT be set for this view. You have to call SetController() manually.
        /// </summary>
        /// <returns>The controller.</returns>
        public virtual IController CreateController(){
            throw new Exception("Not implemented! You are trying to create a default controller, but have not implemented it. Please override the CreateController() function and add your implementation!");
        }

        /// <summary>
        /// Creates a controller. Controller will NOT be set for this view. You have to call SetController() manually
        /// </summary>
        /// <returns>The controller.</returns>
		public IController CreateController<TController>() where TController : IController,new()
        {
			return new TController();
        }

        /// <summary>
        /// Creates a controller of type TController. Controller will NOT be set for this view. You have to call SetController() manually
        /// </summary>
        /// <returns>The controller.</returns>
        /// <param name="controllerType">Controller type.</param>
        public IController CreateController(Type controllerType)
        {
			return (IController)Container.Instantiate(controllerType);
        }

        /// <summary>
        /// Sets the controller for this view
        /// </summary>
        /// <param name="controller">Controller.</param>
        public void SetController(IController controller)
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
            Debug.LogWarning("No controller set! Can not listen to controller event!");
            return null;
        }

        /// <summary>
        /// Publishs an event from the view to the controller.
        /// </summary>
        /// <param name="evt">Evt.</param>
        public void PublishToController(object evt)
        {
            if(Controller != null) Controller.PublishToController(evt);
        }

        void OnDestroy()
        {
            if(disposeOnDestroy) Dispose();
        }

        bool wasDisposed;
        public void Dispose()
        {
            if(this == null || wasDisposed || !ViewInitialized) return;

            wasDisposed = true;

            OnDispose();

            if(this.gameObject != null && destroyOnDispose)
            {
                disposeOnDestroy = false;
                Destroy(this.gameObject);
            }

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

        /// <summary>
        /// The time in seconds since the last network tick
        /// </summary>
        /// <value>The network tick delta.</value>
        public float NetworkTickDelta{get;set;}
        /// <summary>
        /// Gets or sets the current network tick.
        /// </summary>
        /// <value>The current network tick.</value>
        public uint CurrentNetworkTick{get;set;}
        public virtual void NetworkTick(uint tick){}
    }
}