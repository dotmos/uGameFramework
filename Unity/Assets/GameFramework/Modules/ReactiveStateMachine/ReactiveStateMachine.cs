using UnityEngine;
using System.Collections;
using UniRx;
using System;
using System.Reflection;
using System.Collections.Generic;

public class ReactiveStateMachine<TTrigger, TState> : IDisposable {

    bool isDisposed;
    //For firing triggers
    protected readonly Subject<object> triggerSubject = new Subject<object>();
    //For firing onExit for states
    protected readonly Subject<TState> onExitSubject = new Subject<TState>();
    //For firing onEnter for states
    protected readonly Subject<TState> onEnterSubject = new Subject<TState>();
    //Tick event for states
    //protected Subject<object> tickSubject = new Subject<object>();

    public class TickCommand {
        public TState state;
        public float deltaTime;
    }
    protected ReactiveCommand<TickCommand> tickCommand = new ReactiveCommand<TickCommand>();
    protected TickCommand tick = new TickCommand();
    public string Name { get; set; }

    protected CompositeDisposable disposables = new CompositeDisposable();

    /// <summary>
    /// Tick commands cache. For splitting up tick commands and getting rid of GC allocs.
    /// Unfortunally tickCommands.ContainsKey(...) produces garbage as well, so it should only be used when the state changes
    /// </summary>
    Dictionary<TState, ReactiveCommand<TickCommand>> tickCommands = new Dictionary<TState, ReactiveCommand<TickCommand>>();
   
    ReactiveCommand<TickCommand> GetTickCommand(TState state) {
        ReactiveCommand<TickCommand> _cmd = null;

        if (state != null && tickCommands.ContainsKey(state)) {
            _cmd = tickCommands[state];
        }

        return _cmd;
    }


    /// <summary>
    /// Tick this state machine. Then listen to ticks via OnTick(). Can be used instead of While if you don't want to update every frame
    /// </summary>
    public void Tick(float deltaTime = 1){

        if(tickCommand != null) {
            tick.deltaTime = deltaTime;
            tick.state = CurrentState;
            tickCommand.Execute(tick);
        }

        /*
        //tickSubject.OnNext(CurrentState);
        tick.deltaTime = deltaTime;
        tick.state = CurrentState;
        tickCommand.Execute(tick);
        */
    }

    /// <summary>
    /// Method cache. Cache for OnEnter, OnStay and OnExit reflection and faster function calling
    /// </summary>
    //    public class MethodCache{
    //        MethodInfo onEnter;
    //        MethodInfo onExit;
    //
    //        public MethodCache(Type inType)
    //        {
    //            onEnter = inType.GetMethod("OnEnter");
    //            onExit = inType.GetMethod("OnExit");
    //        }
    //
    //        public enum MethodName{
    //            OnEnter,
    //            OnExit
    //        }
    //
    //        public void CallMethod(MethodName methodName, object instance)
    //        {
    //            if(methodName == MethodName.OnEnter && onEnter != null) onEnter.Invoke(instance, null);
    //            else if(methodName == MethodName.OnExit && onExit != null) onExit.Invoke(instance, null);
    //        }
    //    }
    //    protected Dictionary<Type, MethodCache> methodCache = new Dictionary<Type, MethodCache>();


    /// <summary>
    /// Current state.
    /// </summary>
    /// <value>The current state property.</value>
    public ExtendedReactiveProperty<TState> CurrentStateProperty {get; private set;}
    public TState CurrentState {
        get{
            return CurrentStateProperty.Value;
        }
        protected set{
            CurrentStateProperty.Value = value;
        }
    }

    //Constructor
    public ReactiveStateMachine()
    {
        Setup(default(TState));
    }

    public ReactiveStateMachine(TState startState)
    {
        Setup(startState);
    }

    protected void Setup(TState startState)
    {
        if(startState != null) CurrentStateProperty = new ExtendedReactiveProperty<TState>(startState);
        else CurrentStateProperty = new ExtendedReactiveProperty<TState>();

        disposables.Add(
            //If current state is changed, fire on exit event for last state and onEnter event for current state
            CurrentStateProperty.Subscribe(s => {
                ExecuteState(s);
            })
        );
    }

    /// <summary>
    /// Start the state machine with specified state. Not needed to call this if state was already supplied in constructor.
    /// If Start is not called at all and TState is an enum, the first entry in the enum will automatically be set as first state
    /// </summary>
    /// <param name="startState">Start state.</param>
    public void Start(TState startState)
    {
        SetState(startState);
    }

    /// <summary>
    /// Use this to subscribe to a trigger event. Used internally to add transitions to states
    /// </summary>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    protected IObservable<TTrigger> OnTrigger<T>()
    {
        return triggerSubject.Where(p =>
        {
            return p is TTrigger;
        }).Select(delegate(object p)
        {
            return (TTrigger)p;
        });
    }

    /// <summary>
    /// Fire the specified trigger
    /// </summary>
    /// <param name="evt">Evt.</param>
    /// <typeparam name="TTrigger">The 1st type parameter.</typeparam>
    public void Trigger(TTrigger evt)
    {
        triggerSubject.OnNext(evt);
    }

    /// <summary>
    /// Directly set the state of the statemachine
    /// </summary>
    /// <param name="state">State.</param>
    public void SetState(TState newState)
    {
        //if(this.CurrentState!=null && this.CurrentState.Equals(newState)) return;
        this.CurrentState = newState;
    }

    /// <summary>
    /// Executes state logic for the state. Call this when state changes
    /// </summary>
    /// <param name="state"></param>
    void ExecuteState(TState state) {
        //Fire OnExit for last State if current state != lastState
        if (CurrentStateProperty.LastValue != null) { // && !CurrentStateProperty.LastValue.Equals(state)) {
            //                    CallStateChangeMethod(CurrentStateProperty.LastValue, MethodCache.MethodName.OnExit);
            onExitSubject.OnNext(CurrentStateProperty.LastValue); //Fire onExit
        }

        //Fire OnEnter for currentState
        if (state != null) {
            //                    CallStateChangeMethod(s, MethodCache.MethodName.OnEnter);
            onEnterSubject.OnNext(state);
            //Debug.Log("Current State changed to:" + s);
        }

        //Get tick command for current state. Might be null.
        tickCommand = GetTickCommand(state);
    }

    /// <summary>
    /// Retrigger OnEnter/OnExit/OnTrigger/OnTick/While. OnExit might not be called.
    /// </summary>
    public void RetriggerCurrentState() {
        ExecuteState(CurrentState);
    }

    /// <summary>
    /// Adds the transition to the statemachine
    /// </summary>
    /// <param name="currentState">Current state.</param>
    /// <param name="trigger">Trigger.</param>
    /// <param name="newState">New state.</param>
    public virtual IDisposable AddTransition(TState currentState, TTrigger trigger, TState newState){
        IDisposable _disposable = OnTrigger<TTrigger>()
            .Where(x => x.Equals(trigger) && this.CurrentState.Equals(currentState))
            .Subscribe(e => this.SetState(newState));
        disposables.Add(_disposable);
        return _disposable;
    }

    /// <summary>
    /// Adds the transition to the statemachine
    /// </summary>
    /// <returns>The transition.</returns>
    /// <typeparam name="TCurrentState">The 1st type parameter.</typeparam>
    /// <typeparam name="TStateTrigger">The 2nd type parameter.</typeparam>
    /// <typeparam name="TNewState">The 3rd type parameter.</typeparam>
    public virtual IDisposable AddTransition<TCurrentState, TStateTrigger, TNewState>() where TCurrentState : TState where TNewState : TState where TStateTrigger : TTrigger{
        IDisposable _disposable = OnTrigger<TStateTrigger>().Where(x => x.GetType() == typeof(TStateTrigger) && this.CurrentState.GetType() == typeof(TCurrentState) ).Subscribe(e => this.CurrentState = Activator.CreateInstance<TNewState>() );
        disposables.Add( _disposable );
        return _disposable;
    }

    /// <summary>
    /// Adds the transition to the statemachine
    /// </summary>
    /// <returns>The transition.</returns>
    /// <param name="trigger">Trigger.</param>
    /// <typeparam name="TCurrentState">The 1st type parameter.</typeparam>
    /// <typeparam name="TNewState">The 2nd type parameter.</typeparam>
    public virtual IDisposable AddTransition<TCurrentState, TNewState>(TTrigger trigger) where TCurrentState : TState where TNewState : TState{
        IDisposable _disposable = OnTrigger<TTrigger>().Where(x => x.Equals(trigger) && this.CurrentState.GetType() == typeof(TCurrentState) ).Subscribe(e => this.CurrentState = Activator.CreateInstance<TNewState>() );
        disposables.Add( _disposable );
        return _disposable;
    }

    //
    //    public IDisposable AddTransitionWithTriggerValue<TCurrentState, TStateTrigger, TNewState>() where TCurrentState : ReactiveStateMachineStateWithTriggerValue<TTrigger>,new() where TNewState : ReactiveStateMachineStateWithTriggerValue<TTrigger>,new() where TStateTrigger : TTrigger{
    //        IDisposable _disposable = OnTrigger<TStateTrigger>().Where(x => x.GetType() == typeof(TStateTrigger) && this.CurrentState.GetType() == typeof(TCurrentState) ).Subscribe(e => {
    //            //this.CurrentState = new ReactiveStateMachineStateWithTriggerValue<TStateTrigger>((TStateTrigger)e)
    //            this.cur
    //        });
    //        disposables.Add( _disposable );
    //        return _disposable;
    //        return null;
    //    } 

    //    public IDisposable AddTransition<TTrigger>(TState currentState, TState newState){
    //        return null;
    //    }
    //    public IDisposable AddTransition<TCurrentState, TNewState>(TTrigger trigger){
    //        return null;
    //    }

    /// <summary>
    /// Subscribe to the OnEnter event of a state
    /// </summary>
    /// <param name="state">State.</param>
    public virtual IObservable<TState> OnEnter(TState state)
    {

        return onEnterSubject.Where(itm => itm.Equals(state));
        
        /*
        return onEnterSubject.Where(p =>
        {
            return p.Equals(state);
        }).Select(delegate(object p)
        {
            return (TState)p;
        });
        */
    }

    /// <summary>
    /// Subscribe to the OnEnter event of a state
    /// </summary>
    /// <typeparam name="TState">The 1st type parameter.</typeparam>
    public virtual IObservable<TNewState> OnEnter<TNewState>() where TNewState : TState{
        return onEnterSubject.Where(p => p.GetType() == typeof(TNewState)).Select(delegate(TState p)
        {
            return (TNewState)p;
        });
    }

    /// <summary>
    /// Subscribe while state is active
    /// </summary>
    /// <param name="state">State.</param>
    public virtual IObservable<long> While(TState state)
    {
        return Observable.EveryUpdate().Where(e => CurrentState.Equals(state));
    }

    /// <summary>
    /// Subscribe while state is active
    /// </summary>
    /// <typeparam name="TState">The 1st type parameter.</typeparam>
    public virtual IObservable<long> While<TCurrentState>() where TCurrentState : TState
    {
        return Observable.EveryUpdate().Where(e => CurrentState.GetType() == typeof(TCurrentState));
    }

    /// <summary>
    /// Subscribe to the OnExit event of a state
    /// </summary>
    /// <param name="state">State.</param>
    public virtual IObservable<TState> OnExit(TState state)
    {
        return onExitSubject.Where(itm => itm.Equals(state));

        /*
        return onExitSubject.Where(p =>
        {
            return p.Equals(state);
        }).Select(delegate(object p)
        {
            return (TState)p;
        });
        */
    }

    /// <summary>
    /// Subscribe to the OnExit event of a state
    /// </summary>
    /// <param name="state">State.</param>
    public virtual IObservable<TCurrentState> OnExit<TCurrentState>() where TCurrentState : TState
    {
        return onExitSubject.Where(p => p.GetType() == typeof(TCurrentState)).Select(delegate(TState p)
        {
            return (TCurrentState)p;
        });
    }


    /// <summary>
    /// Subscribe to the Tick event of a state. You have to manually call Tick() to make this work
    /// </summary>
    /// <param name="state">State.</param>
    public virtual IObservable<TickCommand> OnTick(TState state){
        ReactiveCommand<TickCommand> _cmd = GetTickCommand(state);
        if(_cmd == null) {
            _cmd = new ReactiveCommand<TickCommand>();
            tickCommands.Add(state, _cmd);
        }

        return _cmd;
        //return tickCommand.Where(itm => itm.state.Equals(state));
        /*
        return tickSubject.Where(p =>
        {
            return p.Equals(state);
        }).Select(delegate(object p)
        {
            return (TState)p;
        });
        */
    }

    /*
    /// <summary>
    /// Subscribe to the Tick event of a state. You have to manually call Tick() to make this work
    /// </summary>
    /// <typeparam name="TState">The 1st type parameter.</typeparam>
    public virtual IObservable<TickCommand> OnTick<TCurrentState>() where TCurrentState : TState{
        return tickCommand.Where(itm => itm.state.GetType() == typeof(TCurrentState));
    }
    */


    /// <summary>
    /// Adds the state to the method cache for faster onEnter/Exit/Stay calling
    /// </summary>
    /// <param name="state">State.</param>
//    protected void AddToMethodCache(TState state)
//    {
//        if(state == null) return;
//
//        Type thisType = state.GetType();
//
//        if(!methodCache.ContainsKey(thisType))
//        {
//            MethodCache _mCache = new MethodCache(thisType);
//            methodCache.Add(thisType, _mCache);
//        }
//    }

    /// <summary>
    /// Calls the state change method using the cached, reflected method
    /// </summary>
    /// <param name="state">State.</param>
    /// <param name="method">Method.</param>
//    protected void CallStateChangeMethod(TState state, MethodCache.MethodName method)
//    {
//        if(state == null) return;
//        Type stateType = state.GetType();
//        if(methodCache.ContainsKey(stateType))
//        {
//            methodCache[stateType].CallMethod(method, state);
//        }
//    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <filterpriority>2</filterpriority>
    /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="ReactiveStateMachine`2"/>. The
    /// <see cref="Dispose"/> method leaves the <see cref="ReactiveStateMachine`2"/> in an unusable state. After calling
    /// <see cref="Dispose"/>, you must release all references to the <see cref="ReactiveStateMachine`2"/> so the
    /// garbage collector can reclaim the memory that the <see cref="ReactiveStateMachine`2"/> was occupying.</remarks>
    public void Dispose()
    {
        if (isDisposed) return;
        //Dispose subjects for this instance
        triggerSubject.Dispose();
        onExitSubject.Dispose();
        onEnterSubject.Dispose();
        //dispose IDisposables for this instance
        disposables.Dispose();
        isDisposed = true;
    }

}


/// <summary>
/// Reactive state machine state with trigger value.
/// </summary>
public class ReactiveStateMachineStateWithTriggerValue<TTrigger>{
    /// <summary>
    /// The trigger used to fire this state
    /// </summary>
    public TTrigger Trigger{get;set;}

    public ReactiveStateMachineStateWithTriggerValue(){
        Debug.LogWarning("You have to add a constructor to your derived class that supplies the class with an object of type TTrigger. Use \"public DerivedClassName(TTrigger trigger) : base(trigger){}\" to do this.");
    }

    public ReactiveStateMachineStateWithTriggerValue( TTrigger trigger){
        this.Trigger = trigger;
    }
}

public class ReactiveStateMachineWithValue<TTrigger, TState> : ReactiveStateMachine<TTrigger, TState> where TState : ReactiveStateMachineStateWithTriggerValue<TTrigger>{

    //Constructor
    public ReactiveStateMachineWithValue()
    {
        Debug.LogWarning("WARNING: You have to supply an initial state when using ReactiveStateMachineWithValue! Now trying to create a default state. This will most likely NOT work ...");
        //        Setup(default(TState));
        Setup( (TState) Activator.CreateInstance(typeof(TState), new object[]{null}) );
    }

    public ReactiveStateMachineWithValue(TState startState)
    {
        Setup(startState);
    }

    /// <summary>
    /// Adds the transition to the statemachine
    /// </summary>
    /// <returns>The transition.</returns>
    /// <typeparam name="TCurrentState">The 1st type parameter.</typeparam>
    /// <typeparam name="TStateTrigger">The 2nd type parameter.</typeparam>
    /// <typeparam name="TNewState">The 3rd type parameter.</typeparam>
    public override IDisposable AddTransition<TCurrentState, TStateTrigger, TNewState>()
    {
        IDisposable _disposable = OnTrigger<TStateTrigger>().Where(x => x.GetType() == typeof(TStateTrigger) && this.CurrentState.GetType() == typeof(TCurrentState) ).Subscribe(e => {
            this.CurrentState = (TState)Activator.CreateInstance(typeof(TNewState), new object[]{(TTrigger)e});
        });
        disposables.Add( _disposable );
        return _disposable;
    }

    /// <summary>
    /// Adds the transition to the statemachine
    /// </summary>
    /// <returns>The transition.</returns>
    /// <param name="trigger">Trigger.</param>
    /// <typeparam name="TCurrentState">The 1st type parameter.</typeparam>
    /// <typeparam name="TNewState">The 2nd type parameter.</typeparam>
    public override IDisposable AddTransition<TCurrentState, TNewState>(TTrigger trigger)
    {
        IDisposable _disposable = OnTrigger<TTrigger>().Where(x => x.Equals(trigger) && this.CurrentState.GetType() == typeof(TCurrentState) ).Subscribe(e => this.CurrentState = (TState)Activator.CreateInstance(typeof(TNewState), new object[]{(TTrigger)e}) );
        disposables.Add( _disposable );
        return _disposable;
    }
}