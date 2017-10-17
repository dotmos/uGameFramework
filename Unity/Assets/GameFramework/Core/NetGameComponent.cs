using UnityEngine;
using Zenject;
using Service.Events;
using System;
using UniRx;
using UnityEngine.Networking;

public class NetGameComponent : NetworkBehaviour, ITickable, IDisposable, Service.GlobalNetwork.INetworkTickableObject {

    /// <summary>
    /// Automatic dispose on object destruction
    /// </summary>
    public bool disposeOnDestroy = true;
    /// <summary>
    /// Destroy the object when Dispose is called
    /// </summary>
    public bool destroyOnDispose = true;
    /// <summary>
    /// If set to true, GameBehaviour will have it's dependencies injected and is initialized on startup. This can only be set from the editor or in PreBind().
    /// </summary>
    public bool bindOnAwake = true;

    bool isInitialized = false;

    protected DiContainer Container {
        get;
        private set;
    }
    private Service.Events.IEventsService _eventService;
    private DisposableManager _dManager;
    private TickableManager _tManager;

    [Inject]
    void Initialize(
        [Inject] DiContainer diContainer,
        [Inject] DisposableManager dManager,
        [Inject] TickableManager tManager,
        [Inject] IEventsService eventService)
    {
        if(isInitialized) return;
        isInitialized = true;


        Container = diContainer;
        _dManager = dManager;
        _tManager = tManager;
        _eventService = eventService;

        //To make IDisposable work, we have to add this instance to the disposable manager
        _dManager.Add(this);

        //Make ITickable and INetworkTickable work
        AddTickable(this);


        //Bind all interfaces for this instance
        Container.Bind<ITickable>().FromInstance(this);
        Container.Bind<IDisposable>().FromInstance(this);
        //Container.BindAllInterfacesFromInstance(this);

        AfterBind();
    }

    void Awake()
    {
        PreBind();
        //If this view is part of the scenefile, make sure it gets initialized/injected
        if(bindOnAwake && !isInitialized)
        {
            if(Kernel.Instance != null) Kernel.Instance.Inject(this);
        }
    }

    protected virtual void PreBind(){}
    protected virtual void AfterBind(){}

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

    void OnDestroy()
    {
        if(disposeOnDestroy) Dispose();
    }

    bool wasDisposed;
    public virtual void Dispose()
    {
        if(this == null || wasDisposed) return;

        wasDisposed = true;

        OnDispose();

        if(this.gameObject != null && destroyOnDispose)
        {
            disposeOnDestroy = false;
            Destroy(this.gameObject);
        }

        if(isInitialized){
            //If we get a null error here, object was not injected to Container!
            RemoveTickable(this);
            _dManager.Remove(this);
        }

        //Debug.Log(this + " disposed");
    }

    protected virtual void OnDispose(){}

    public virtual void Tick(){}

    /// <summary>
    /// The time in seconds between each network tick. Do not manually change this!
    /// </summary>
    /// <value>The network tick delta.</value>
    public float NetworkTickDelta{get;set;}
    /// <summary>
    /// The current network tick. Do not manually change this!
    /// </summary>
    /// <value>The current network tick.</value>
    public uint CurrentNetworkTick{get;set;}
    public virtual void NetworkTick(uint tick){}
}