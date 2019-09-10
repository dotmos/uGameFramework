using UnityEngine;
using System.Collections;
using Zenject;
using UniRx;
using System;

//Simple, non-invasive Kernel class based on Zenject.SceneCompositionRoot
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using ModestTree.Util;
using System.Collections.Concurrent;
using Service.Serializer;
using ECS;

public partial class Kernel : SceneContext {

    Service.Events.IEventsService eventService;
    protected DisposableManager dManager;

    static bool loadingKernelScene = false;
    public static string overrideSceneName = null;

    protected bool KernelReady { get; private set; }
    public bool KernelCallUpdate { get; set; } = true;

    public static bool applicationQuitting = false;

    public ReactivePriorityExecutionList rxStartup = new ReactivePriorityExecutionList();
    public ReactivePriorityExecutionList rxShutDown = new ReactivePriorityExecutionList();

    public static ReactiveProperty<Kernel> InstanceProperty = new ReactiveProperty<Kernel>();
    public static Kernel Instance {
        get{

            // Added !applicationQuitting into the if check to make sure that the kernel scene isn't loaded OnApplicationQuit, when it became null
            // Seems like OnApplicationQuit is sometimes wrongly called, causing a reload of the Kernel Scene.
            // Loading the Kernel Scene should only happen from inside the editor, if the kernel scene was not initially loaded
            if (InstanceProperty.Value == null && !loadingKernelScene && SceneManager.GetActiveScene().name != "Kernel" && !applicationQuitting){

#if UNITY_EDITOR
                overrideSceneName = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().name;
#endif
                System.Diagnostics.StackFrame frame = new System.Diagnostics.StackFrame(1);
                var method = frame.GetMethod();
                var type = method.DeclaringType;
                var name = method.Name;
                Debug.Log("kernel is null! Calling script:"+ frame.GetFileName() +" in scene: "+ SceneManager.GetActiveScene().name + " calling method name: "+ name);

                loadingKernelScene = true;
                SceneManager.LoadScene("Kernel");
            }

            return InstanceProperty.Value;
        }
        private set{
            InstanceProperty.Value = value;
        }
    }

    private static int MAINTHREAD_ID;

    public static bool IsMainThread() {
        return System.Threading.Thread.CurrentThread.ManagedThreadId == MAINTHREAD_ID;
    }

    /// <summary>
    /// Inject the specified injectable to Kernel Container, thus triggering DependencyInjection
    /// </summary>
    /// <param name="injectable">Injectable.</param>
    public void Inject(object injectable)
    {
        Container.Inject(injectable);
    }

    public T Resolve<T>() {
        return Container.Resolve<T>();
    }

    new protected virtual void Awake(){
        if(System.Threading.Thread.CurrentThread.Name != "MainThread") {
            System.Threading.Thread.CurrentThread.Name = "MainThread";
        }
        MAINTHREAD_ID = System.Threading.Thread.CurrentThread.ManagedThreadId;

        Instance = this;
 
        base.Awake();
    }

    protected virtual void Update() {
        if (!Kernel.IsMainThread()) {
            Debug.Log("MainThreadID-mismatch:" + System.Threading.Thread.CurrentThread.ManagedThreadId + " != mainThread:" + MAINTHREAD_ID);
        }
        FutureProcessor.Instance.ProcessMainThreadActions();

        if (KernelReady && KernelCallUpdate) Tick(Time.deltaTime);
    }

    protected virtual void Tick(float deltaTime) {
    }

    protected virtual void Start(){
        //Resolve disposableManager
        dManager = Container.Resolve<DisposableManager>();
        //Let the program know, that Kernel has finished loading


        // first start rxStartup-queue
        // if this is finished, start initial gamestate
        rxStartup
            .RxExecute()
            .SelectMany(_ => {
                var initialGameStateName = GetInitialGamestateName();
                var initialGameStateCtx = GetInitialGamestateContext();
                if (initialGameStateName == null) {
                    Debug.LogWarning("No inital gamestate specified!");
                    return Observable.Return(true);
                } else {
                    var gameStateService = Container.Resolve<Service.GameStateService.IGameStateService>();
                    var initialGameState = gameStateService.GetGameState(initialGameStateName);
                    if (initialGameState == null) {
                        Debug.LogWarning("Could not find initial gamestate with name:" + initialGameStateName);
                        return Observable.Return(true);
                    }
                    return gameStateService.StartGameState(initialGameState, initialGameStateCtx).Do(__ => { Debug.Log("Started initial gamestate:" + initialGameStateName); });
                }
            })
            .Last()
            .Take(1)
            .Subscribe(_ => {
                KernelReady = true;
                Debug.Log("Startup done!");
                OnKernelReady();
            });
    }

    public virtual String GetInitialGamestateName() {
        return null;
    }

    public virtual Service.GameStateService.GSContext GetInitialGamestateContext() {
        return null;
    }

    protected virtual void OnKernelReady() {

    }

    void OnApplicationFocus(bool hasFocus) {
        if(eventService != null) {
            if (hasFocus) {
                eventService.Publish(new Events.OnApplicationFocus());
            } else {
                eventService.Publish(new Events.OnApplicationLostFocus());
            }
            
        }
    }

    private void OnApplicationQuit() {
        
        SetOnApplicationQuitSettings();
        
        rxShutDown
            .RxExecute()
            .Take(1)
            .Subscribe(_ => {
                Debug.Log("Shutdown complete!");
            });

        /*if (eventService != null) {
            eventService.Publish(new Events.OnApplicationQuit());
        }  */
    }

    // since there seems to be no defined way to hit the very first OnApplicationQuit
    // set settings to discard dispose must be called from somewhere else as well 
    // (e.g. gameObject's OnApplicationQuit) That is ugly....
    //Edit: This is now called by the GameService right before the application is told to quit
    public void SetOnApplicationQuitSettings() {
        if (!applicationQuitting) {
            applicationQuitting = true;
           // dManager.SkipDispose(true);
        }
    }

    /*
    void OnApplicationPause(bool pauseStatus) {
        if(eventService != null) {
        }
    }
    */
}