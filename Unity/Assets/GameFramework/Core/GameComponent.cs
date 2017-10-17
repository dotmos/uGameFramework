using UnityEngine;
using Zenject;
using Service.Events;
using System;
using UniRx;
using UnityEngine.Profiling;

public class GameComponent : MonoBehaviour, ITickable, IDisposable {

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
    protected Service.Events.IEventsService _eventService {get; private set;}
    private DisposableManager _dManager;
    private TickableManager _tManager;

    [Inject]
    void Initialize(
        [Inject] DiContainer diContainer,
        [Inject] DisposableManager dManager,
        [Inject] TickableManager tManager,
        [Inject] IEventsService eventService
        )
    {
        if (isInitialized) return;
        isInitialized = true;

		PreBind();

        Container = diContainer;
        _dManager = dManager;
        _tManager = tManager;
        _eventService = eventService;

        Profiler.BeginSample("dmanager add");

        //To make IDisposable work, we have to add this instance to the disposable manager
        _dManager.Add(this);
        Profiler.EndSample();

        //Make ITickable work
        Profiler.BeginSample("Add tickable");
        // this takes most of the time
        AddTickable(this);
        Profiler.EndSample();

        //Bind all interfaces for this instance
        Profiler.BeginSample("ITickable fromInstance");
        Container.Bind<ITickable>().FromInstance(this);
        Profiler.EndSample();
        Profiler.BeginSample("IDisposable fromInstance");
        Container.Bind<IDisposable>().FromInstance(this);
        //Container.BindAllInterfacesFromInstance(this);
        Profiler.EndSample();

        Profiler.BeginSample("AfterBind");
        AfterBind();
        Profiler.EndSample();
        
    }

    void Awake()
    {
        //If this view is part of the scenefile, make sure it gets initialized/injected
        if(bindOnAwake)
        {
			Bind();
        }
    }

	protected virtual void Bind(){
		Profiler.BeginSample("Bind");
		if(!isInitialized && Kernel.Instance != null) Kernel.Instance.Inject(this);
		Profiler.EndSample();
	}

    protected virtual void PreBind(){}
    protected virtual void AfterBind(){}

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
        if(disposeOnDestroy && !Kernel.applicationQuitting) Dispose();
    }

    bool wasDisposed;
    public virtual void Dispose()
    {
        if(this == null || wasDisposed || Kernel.applicationQuitting ) return;

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
}