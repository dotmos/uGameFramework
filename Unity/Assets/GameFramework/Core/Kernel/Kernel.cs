﻿using UnityEngine;
using System.Collections;
using Zenject;
using UniRx;
using System;

//Simple, non-invasive Kernel class based on Zenject.SceneCompositionRoot
using UnityEngine.SceneManagement;
using ECS;
using System.Threading;

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

    private Service.Scripting.IScriptingService scriptingService;

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
                System.Reflection.MethodBase method = frame.GetMethod();
                Type type = method.DeclaringType;
                string name = method.Name;
                Debug.Log("kernel is null! Calling script:"+ frame.GetFileName() +" in scene: "+ SceneManager.GetActiveScene().name + " calling method name: "+ name);

                loadingKernelScene = true;
#if ADDRESSABLES
                LoadSceneFromAddressables("Kernel");
#else
                SceneManager.LoadScene("Kernel");
#endif
            }

            return InstanceProperty.Value;
        }
        private set{
            InstanceProperty.Value = value;
        }
    }

#if ADDRESSABLES
    async static void LoadSceneFromAddressables(string scene) {

        UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance> asyncOp = UnityEngine.AddressableAssets.Addressables.LoadSceneAsync(scene);
        await asyncOp.Task;
    }
#endif

    private static int MAINTHREAD_ID;
    private static Thread mainThread;

    public static bool IsMainThread() {
        return System.Threading.Thread.CurrentThread.ManagedThreadId == MAINTHREAD_ID;
    }

    public static Thread MainThread {
        get { return mainThread; }
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
        mainThread = System.Threading.Thread.CurrentThread;
        MAINTHREAD_ID = mainThread.ManagedThreadId;

        Instance = this;
 
        base.Awake();
    }

    protected virtual void Update() {
        if (!Kernel.IsMainThread()) {
            Debug.Log("MainThreadID-mismatch:" + System.Threading.Thread.CurrentThread.ManagedThreadId + " != mainThread:" + MAINTHREAD_ID);
        }
        FutureProcessor.Instance.ProcessMainThreadActions();

#if !NO_LUA_TESTING
        if (scriptingService == null) scriptingService = Container.Resolve<Service.Scripting.IScriptingService>();
        scriptingService.Tick(Time.deltaTime);
#endif

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
                string initialGameStateName = GetInitialGamestateName();
                Service.GameStateService.GSContext initialGameStateCtx = GetInitialGamestateContext();
                if (initialGameStateName == null) {
                    Debug.LogWarning("No inital gamestate specified!");
                    return Observable.Return(true);
                } else {
                    Service.GameStateService.IGameStateService gameStateService = Container.Resolve<Service.GameStateService.IGameStateService>();
                    Service.GameStateService.GameState initialGameState = gameStateService.GetGameState(initialGameStateName);
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
                Debug.LogWarning("Startup done!");
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