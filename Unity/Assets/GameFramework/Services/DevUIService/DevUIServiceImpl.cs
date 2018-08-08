using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

using Zenject;
using UniRx;
using System.Linq;
using Service.Serializer;
using UnityEngine;
using System.IO;
using System.Threading;
using ECS;
using Service.MemoryBrowserService;

namespace Service.DevUIService {

    partial class DevUIServiceImpl : DevUIServiceBase {

        public const string SUBFOLDER_ARCHIEVE = "archive";

        public ReactiveCollection<DevUIView> rxViews;

        [Inject]
        Service.LoggingService.ILoggingService logging;

        [Inject(Id = SerializerID.JsonNet)]
        Service.Serializer.ISerializerService serializer;

        [Inject]
        Service.FileSystem.IFileSystemService fileSystem;

        [Inject]
        Service.Scene.ISceneService sceneService;

        [Inject]
        Service.TimeService.ITimeService timeService;

        ECS.IEntityManager entityManager;

        /// <summary>
        /// A list in which all files-path of the currently loaded views is visible
        /// </summary>
        private List<string> viewPathsLoaded = new List<string>();

        // Scene loading commands
        private const string developmentSceneID = "DevelopmentConsole";
/*        private Scene.Commands.ActivateSceneCommand activateDevelopmentConsole = new Scene.Commands.ActivateSceneCommand() { sceneID = developmentSceneID };
        private Scene.Commands.DeactivateSceneCommand deactivateDevelopmentConsole = new Scene.Commands.DeactivateSceneCommand() { sceneID = developmentSceneID };*/
/*        private Scene.Commands.LoadSceneCommand loadDevelopmentConsole = new Scene.Commands.LoadSceneCommand() {
            sceneID = developmentSceneID,
            additive = true,
            asynchron = false,
            makeActive = false
        };*/
        private bool devConsoleActive = false;

        /// <summary>
        /// Currently in entity pick mode?
        /// </summary>
        bool pickingEntity = false;
        UnityEngine.Camera _mainCamera = null;
        UnityEngine.Camera MainCamera {
            get {
                if (_mainCamera == null) {
                    _mainCamera = UnityEngine.Camera.main;
                }
                return _mainCamera;
            }
        }

        protected override void AfterInitialize() {
            // this is called right after the Base-Classes Initialize-Method. _eventManager and disposableManager are set
            rxViews = new ReactiveCollection<DevUIView>();

            // TODO: get rid of EveryUpdate
            Observable.EveryUpdate().Subscribe(_ => {
                if (UnityEngine.Input.GetKeyDown(KeyCode.F8)) {
                    ToggleScriptingConsole();
                }
            }).AddTo(disposables);

            OnEvent<Events.UIViewRenamed>().Subscribe(evt => {
                if (evt.view.currentFilename != null) {
                    // there is already an representation! delete it and create it new
                    fileSystem.RemoveFile(evt.view.currentFilename);
                    // save to default-location but force a new name
                    SaveViewToPath(evt.view,false,true);
                }
            });


            // on startup create some sample data
            var rxStartup = Kernel.Instance.rxStartup;

            // For testing data that is already present when the DevConsole gets active add test-data at very early stage
            rxStartup.Add(() => {
                var win = CreateView("ServiceDEVUI");
                win.AddElement(new DevUIButton("kickit", () => {
                    UnityEngine.Debug.Log("KICKIT");
                }));

                var luaExpression = new DevUILuaExpression("key", 2.5f);
                win.AddElement(luaExpression);
                luaExpression.valueProperty.Subscribe(val => {
                    logging.Info("val:" + val);
                }).AddTo(disposables);

                win.AddElement(new DevUILUAButton("kickit-lua", "print('tom')"));

                win.AddElement(new DevUIKeyValue("KeyValue", "0"));

                logging.Info("Something is good", "org.tt");

            },Priorities.PRIORITY_VERY_EARLY); // Init Test-Data at an early stage

            rxStartup.Add(UtilsObservable.LoadScene(developmentSceneID),Priorities.PRIORITY_EARLY);

            //TODO: This is a workaround to close the dev console on start. Usually the loadDevelopmentConsole command published above should load the scene deactivated (makeActive set to false) which doesn't seem to work.
            // To Ensure that the 
            rxStartup.Add(() => {
                CloseScriptingConsole();
            }, Priorities.PRIORITY_DEFAULT); // with using PRIORITY_DEFAULT which is called after the PRIORITY_EARLY-Block it is ensured that the scene is fully loaded, before this block is executed


            rxStartup.Add(LoadViews().Do(pr=> {
                logging.Info("progress:" + pr);
            }).Last().Select(_=>true)

            ,Priorities.PRIORITY_DEFAULT); // Default Priority



            // on shutdown persist the current views
            Kernel.Instance.rxShutDown.Add(() => {
                SaveViews();
            });


            OnEvent<Events.PickedEntity>().Subscribe(evt => {
                var view = CreateViewFromEntity(evt.entity);

            }).AddTo(disposables);

            
        }

        public override void WriteToScriptingConsole(string text) {
            this.Publish(new Events.WriteToScriptingConsole() { text = text });
        }

        void _ShowScriptingConsole() {
            sceneService.ActivateScene(developmentSceneID);
        }

        void _HideScriptingConsole() {
            sceneService.DeactivateScene(developmentSceneID);
        }

        public override void OpenScriptingConsole() {
            devConsoleActive = true;
            _ShowScriptingConsole();
            this.Publish(new Events.ScriptingConsoleOpened());
        }

        public override void CloseScriptingConsole() {
            devConsoleActive = false;
            _HideScriptingConsole();
            this.Publish(new Events.ScriptingConsoleClosed());
        }

        public override void ToggleScriptingConsole() {
            if (!devConsoleActive) {
                OpenScriptingConsole();
            } else {
                CloseScriptingConsole();
            }
        }
        public override bool IsScriptingConsoleVisible() {
            return devConsoleActive;
        }


        public override ReactiveCollection<DevUIView> GetRxViews() {
            return rxViews;
        }


        public override DevUIView GetView(string viewName) {
            return rxViews.Where(v => v.Name == viewName).FirstOrDefault();
        }

        public override void RemoveViewFromModel(DevUIView view) {
            rxViews.Remove(view);
            viewPathsLoaded.Remove(view.currentFilename);
            view.Dispose();
        }

        public override void RemoveViewToArchieve(DevUIView view) {
            RemoveViewFromModel(view);
            fileSystem.RemoveFile(view.currentFilename);
            // save view to archieve path (true)
            SaveViewToPath(view, true);
        }


        public override bool ViewNameExists(string viewName) {
            return GetView(viewName) != null;
        }


        public override DevUIView CreateView(string viewName,bool dynamicallyCreated=false) {
            var view = GetView(viewName);
            if (view != null) {
                return view;
            }
            var newView = new DevUIView(viewName,dynamicallyCreated);
            rxViews.Add(newView);
            return newView;
        }





        protected override void OnDispose() {
            // do your IDispose-actions here. It is called right after disposables got disposed
            foreach (var view in rxViews) {
                view.Dispose();
            }
            rxViews.Clear();
        }

        public override IObservable<float> LoadViews() {
            var viewFiles = fileSystem.GetFilesInDomain(FileSystem.FSDomain.DevUIViews, "*.json");

            float progressFactor = 1.0f / viewFiles.Count;
            float progress = 0;
            var result = Observable.Return(progressFactor);

            // check if we already used a viewname
            HashSet<string> usedViewNames = new HashSet<string>();

            List<string> tempCurrentViewFiles = new List<string>(viewPathsLoaded);
            
            foreach (string viewFile in viewFiles) {
                result = result.Concat(Observable.Start(() => {
                    Debug.Log("thread: " + Thread.CurrentThread.Name);
                    // did we already load this view? if yes, skip
                    if (tempCurrentViewFiles.Contains(viewFile)) {
                        // remove file from temp list (the ones that stays in list are the ones to be deleted afterwards)
                        tempCurrentViewFiles.Remove(viewFile);
                        return null;
                    }

                    var viewDataAsString = fileSystem.LoadFileAsString(viewFile);
                    return viewDataAsString;
                }).ObserveOnMainThread().Select(fileData => {
                    if (fileData != null) {
                        var viewData = serializer.DeserializeToObject<DevUIView>(fileData);

                        if (usedViewNames.Contains(viewData.Name)) {
                            logging.Warn("There is already already a view with the name " + viewData.Name + "! This results in merged views");
                        }

                        var view = GetView(viewData.Name);
                        if (view == null) {
                            // a new view
                            view = CreateView(viewData.Name);
                            view.createdDynamically = viewData.createdDynamically;
                            view.extensionAllowed = viewData.extensionAllowed;
                        }
                        usedViewNames.Add(viewData.Name);
                        view.currentFilename = viewFile;

                        foreach (var uiElem in viewData.uiElements) {
                            view.AddElement(uiElem, false);
                        }

                        viewPathsLoaded.Add(viewFile);
                    }
                    return "";
                }).Select(_ => { progress += progressFactor; return progress; }));
            }

            result = result.Finally(() => {
                // are there any files left, that were loaded before, but now vanished?
                foreach (string oldPath in tempCurrentViewFiles) {
                    var view = rxViews.Where(v => v.currentFilename == oldPath).FirstOrDefault();
                    if (view != null) {
                        RemoveViewFromModel(view);
                    }
                }
            });

            return result.ObserveOnMainThread();
        }

        public override void SaveViews() {
            foreach (var view in rxViews) {
                if (!view.extensionAllowed) {
                    continue;
                }
                SaveViewToPath(view);
            }
        }

        private void SaveViewToPath(DevUIView view, bool saveToArchieve=false,bool forceNewFilename=false) {
            var viewAsString = serializer.Serialize(view);

            if (saveToArchieve) {
                var saveAsFilename = DateTime.Now.ToFileTime() +"-" +view.Name + ".json";
                fileSystem.WriteStringToFileAtDomain(FileSystem.FSDomain.DevUIViewsArchieve,saveAsFilename, viewAsString);
                view.currentFilename = fileSystem.GetPath(FileSystem.FSDomain.DevUIViewsArchieve, saveAsFilename);
            } else {
                var saveAsFilename = (view.currentFilename == null || forceNewFilename) ? view.Name + ".json" : Path.GetFileName(view.currentFilename);
                fileSystem.WriteStringToFileAtDomain(FileSystem.FSDomain.DevUIViews, saveAsFilename, viewAsString);
                view.currentFilename = fileSystem.GetPath(FileSystem.FSDomain.DevUIViews,saveAsFilename);
            }
            
        }

        public override void StartPickingEntity() {
            _HideScriptingConsole();
            pickingEntity = true;
            IDisposable pickingEntityDisposable = null;
            pickingEntityDisposable = Observable.EveryUpdate().Subscribe(e => {
                if (UnityEngine.Input.GetMouseButtonDown(0)) {
                    RaycastHit hitInfo;
                    if(Physics.Raycast(MainCamera.ScreenPointToRay(UnityEngine.Input.mousePosition), out hitInfo, 100)){
                        ECS.MonoEntity entity = hitInfo.collider.GetComponent<ECS.MonoEntity>();
                        if(entity != null) {
                            //Clicked on an entity
                            Debug.Log("Picked entity:" + entity.Entity.ID);
                            this.Publish(new Events.PickedEntity() { entity = entity.Entity });
                        }
                        
                    }

                    _ShowScriptingConsole();
                    pickingEntity = false;
                    pickingEntityDisposable.Dispose();
                }
                else if (UnityEngine.Input.GetKeyDown(KeyCode.Escape)) {
                    _ShowScriptingConsole();
                    pickingEntity = false;
                    pickingEntityDisposable.Dispose();
                }
            });
            AddDisposable(pickingEntityDisposable);
        }

        public override DevUIView CreateViewFromEntity(UID entity) {
            if (entityManager == null) {
                entityManager = Kernel.Instance.Container.Resolve<IEntityManager>();
            }

            if (entityManager == null) {
                Debug.LogError("Could not locate entity-manager");
            }

            var components = entityManager.GetAllComponents(entity);

            var resultView = CreateView("entity-" + entity.ID, false);
            resultView.extensionAllowed = false;

            List<MemoryBrowser> mBrowsers = new List<MemoryBrowser>();
            foreach (var comp in components) {
                var mB = new MemoryBrowser(comp);

                var dict = new Dictionary<string, DevUIKeyValue>();

                foreach (string key in mB.rxCurrentSnapShot.Keys) {
                    object obj = mB.rxCurrentSnapShot[key];

                    if (obj == null || MemoryBrowser.IsSimple(obj.GetType()) || obj is Vector3 || obj is Vector2 || obj is Vector4) {
                        var uiKV = new DevUIKeyValue(key, obj==null?"null":obj.ToString());
                        uiKV.OnValueChangeRequested = (newVal) => {
                            mB.SetValue(key, newVal);
                            entityManager.EntityModified(entity);
                        };
                        resultView.AddElement(uiKV);
                        dict[key] = uiKV;
                    }
                }

                if (dict.Count > 0) {
                    mB.rxCurrentSnapShot
                        .ObserveReplace()
                        .Subscribe(evt => {
                            if (evt.OldValue == evt.NewValue) {
                                return;
                            }

                            string key = evt.Key;
                            if (dict.ContainsKey(key)) {
                                var uiKV = dict[key];
                                uiKV.Value = evt.NewValue.ToString();
                            }
                        })
                        .AddTo(resultView.disposables); // when the view is disposed, also dispose this subscription
                }

                mBrowsers.Add(mB);

                // Poll the data at a this interval for every component
                // TODO: Make this more efficient: only the view that is in focus!?
                timeService.CreateGlobalTimer(1.0f, () => {
                    mB.UpdateCurrentSnapshot();
                    logging.Info("UPDATED SNAPSHOT");
                }, 0);
            }

            return resultView;
        }
    }



}
