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
using System.Collections;

namespace Service.DevUIService {

    public interface IDevUIVisible { }

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

        private Dictionary<Type, Func<object, object>> typeConverter = new Dictionary<Type, Func<object, object>>();

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

        private List<DataBrowserTopLevel> dataBrowserTopLevelElements = new List<DataBrowserTopLevel>();

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
            }).AddTo(disposables);


            // on startup create some sample data
            ReactivePriorityExecutionList rxStartup = Kernel.Instance.rxStartup;

            rxStartup.Add(UtilsObservable.LoadScene(developmentSceneID),Priorities.PRIORITY_EARLY);
            
            if (TouchScreenKeyboard.isSupported) {
                rxStartup.Add(()=> {
                    var uiView = CreateView("UI-Options");
                    uiView.AddElement(new DevUIToggle("VirtualKeyboard active", (newValue) => {
                        UserInterface.GMInputField.VirtualKeyboardActive = newValue;
                    }, UserInterface.GMInputField.VirtualKeyboardActive));
                }, Priorities.PRIORITY_DEFAULT);
            }

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
                DevUIView view = CreateViewFromEntity(evt.entity);

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


        public override DevUIView CreateView(string viewName,bool dynamicallyCreated=false, bool extensionAllowed=true) {
            DevUIView view = GetView(viewName);
            if (view != null) {
                return view;
            }
            DevUIView newView = new DevUIView(viewName,dynamicallyCreated);
            newView.extensionAllowed = extensionAllowed;
            rxViews.Add(newView);
            return newView;
        }

        public override DevUIView GetOrCreateView(string viewName) {

            DevUIView view = GetView(viewName);
            if (view == null) {

                view = CreateView(viewName);
            }
            return view;
        }



        protected override void OnDispose() {
            // do your IDispose-actions here. It is called right after disposables got disposed
            foreach (DevUIView view in rxViews) {
                view.Dispose();
            }
            rxViews.Clear();
        }

        public override IObservable<float> LoadViews() {
            List<string> viewFiles = fileSystem.GetFilesInDomain(FileSystem.FSDomain.DevUIViews,"", "*.json");

            float progressFactor = 1.0f / viewFiles.Count;
            float progress = 0;
            IObservable<float> result = Observable.Return(progressFactor);

            // check if we already used a viewname
            HashSet<string> usedViewNames = new HashSet<string>();

            List<string> tempCurrentViewFiles = new List<string>(viewPathsLoaded);
            
            foreach (string viewFile in viewFiles) {
                result = result.Concat(Observable.Start(() => {
                    Debug.Log("thread: " + System.Threading.Thread.CurrentThread.Name);
                    // did we already load this view? if yes, skip
                    if (tempCurrentViewFiles.Contains(viewFile)) {
                        // remove file from temp list (the ones that stays in list are the ones to be deleted afterwards)
                        tempCurrentViewFiles.Remove(viewFile);
                        return null;
                    }

                    string viewDataAsString = fileSystem.LoadFileAsString(viewFile);
                    return viewDataAsString;
                }).ObserveOnMainThread().Select(fileData => {
                    try {
                        if (fileData != null) {
                            DevUIView viewData = serializer.DeserializeToObject<DevUIView>(fileData);

                            if (usedViewNames.Contains(viewData.Name)) {
                                logging.Warn("There is already already a view with the name " + viewData.Name + "! This results in merged views");
                            }

                            DevUIView view = GetView(viewData.Name);
                            if (view == null) {
                                // a new view
                                view = CreateView(viewData.Name, viewData.createdDynamically);
                                view.extensionAllowed = viewData.extensionAllowed;
                            }
                            usedViewNames.Add(viewData.Name);
                            view.currentFilename = viewFile;

                            foreach (DevUIElement uiElem in viewData.uiElements) {
                                view.AddElement(uiElem, false);
                            }

                            viewPathsLoaded.Add(viewFile);
                        }
                    }
                    catch (Exception e) {
                        Debug.LogWarning("Could not load viewElement! Ignoring!");
                    }
                    return "";
                }).Select(_ => { progress += progressFactor; return progress; }));
            }

            result = result.Finally(() => {
                // are there any files left, that were loaded before, but now vanished?
                foreach (string oldPath in tempCurrentViewFiles) {
                    DevUIView view = rxViews.Where(v => v.currentFilename == oldPath).FirstOrDefault();
                    if (view != null) {
                        RemoveViewFromModel(view);
                    }
                }
            });

            return result.ObserveOnMainThread();
        }

        public override void SaveViews() {
            foreach (DevUIView view in rxViews) {
                if (!view.extensionAllowed) {
                    continue;
                }
                SaveViewToPath(view);
            }
        }

        private void SaveViewToPath(DevUIView view, bool saveToArchieve=false,bool forceNewFilename=false) {
            string viewAsString = serializer.Serialize(view);

            if (saveToArchieve) {
                string saveAsFilename = DateTime.Now.ToFileTime() +"-" +view.Name + ".json";
                fileSystem.WriteStringToFileAtDomain(FileSystem.FSDomain.DevUIViewsArchieve,saveAsFilename, viewAsString);
                view.currentFilename = fileSystem.GetPath(FileSystem.FSDomain.DevUIViewsArchieve, saveAsFilename);
            } else {
                string saveAsFilename = (view.currentFilename == null || forceNewFilename) ? view.Name + ".json" : Path.GetFileName(view.currentFilename);
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


        private MemoryBrowser Create(DevUIView resultView, object viewObject,bool autoupdate=false, Action<object,object> onValueChanged=null) {
            DevUIButton compButton = new DevUIButton(viewObject.GetType().ToString(), () => { });
            resultView.AddElement(compButton);

            MemoryBrowser mB = new MemoryBrowser(viewObject);

            Dictionary<string, DevUIKeyValue> dict = new Dictionary<string, DevUIKeyValue>();

            foreach (string key in mB.rxCurrentSnapShot.Keys) {
                object obj = mB.rxCurrentSnapShot[key];

                if (obj == null || MemoryBrowser.IsSimple(obj.GetType()) || obj is Vector3 || obj is Vector2 || obj is Vector4) {
                    DevUIKeyValue uiKV = new DevUIKeyValue(key, obj == null ? "null" : obj.ToString());
                    uiKV.OnValueChangeRequested = (newVal) => {
                        mB.SetValue(key, newVal);
                        if (onValueChanged != null) {
                            onValueChanged(key, newVal);
                        }
                    };
                    resultView.AddElement(uiKV);
                    dict[key] = uiKV;
                } else if (obj is IDevUIVisible) {
                    DevUIKeyValue uiKV = new DevUIKeyValue(key, obj == null ? "null" : "'" + obj.ToString() + "'");
                    resultView.AddElement(uiKV);
                    // TODO: Detect changes in custom-type
                } else if (obj is IList && ((IList)obj).Count > 0) {
                    IList thelist = (IList)obj;
                    object firstElement = thelist[0];
                    if (firstElement is IDevUIVisible) {
                        resultView.AddElement(new DevUIButton(key + "(List)", null));

                        for (int i = 0; i < thelist.Count; i++) {
                            object listObj = thelist[i];
                            DevUIKeyValue uiKV = new DevUIKeyValue(key, i + "| " + listObj == null ? "null" : "'" + listObj.ToString() + "'");
                            resultView.AddElement(uiKV);
                            // TODO: detect list changes    
                        }
                    }
                }

            }

            if (autoupdate && dict.Count > 0) {
                mB.rxCurrentSnapShot
                    .ObserveReplace()
                    .Subscribe(evt => {
                        if (evt.OldValue == evt.NewValue) {
                            return;
                        }

                        string key = evt.Key;
                        if (dict.ContainsKey(key)) {
                            DevUIKeyValue uiKV = dict[key];
                            uiKV.Value = evt.NewValue.ToString();
                        }
                    })
                    .AddTo(resultView.disposables); // when the view is disposed, also dispose this subscription
            } else {
                // TODO: no button if we cannot show any values?
                resultView.RemoveElement(compButton);
            }


            // Poll the data at a this interval for every component
            // TODO: Make this more efficient: only the view that is in focus!?
            timeService.CreateGlobalTimer(1.0f, () => {
                mB.UpdateCurrentSnapshot();
                logging.Info("UPDATED SNAPSHOT");
            }, 0);

            return mB;
        }

        public override DevUIView CreateViewFromPOCO(object viewObject, string name) {
            DevUIView resultView = CreateView(name, false, false);
            Create(resultView, viewObject);
            return resultView;
        }

        public override DevUIView CreateViewFromEntity(UID entity,string name="") {
            if (entityManager == null) {
                entityManager = Kernel.Instance.Container.Resolve<IEntityManager>();
            }

            if (entityManager == null) {
                Debug.LogError("Could not locate entity-manager");
            }

            List<IComponent> components = entityManager.GetAllComponents(entity);

            DevUIView resultView = CreateView((name==null||name=="")?"entity-" + entity.ID : name, false,false);
            


            List<MemoryBrowser> mBrowsers = new List<MemoryBrowser>();
            foreach (IComponent comp in components) {
                MemoryBrowser mB = Create(resultView, comp,true, (key, value) => {
                    entityManager.EntityModified(entity);
                });
                mBrowsers.Add(mB);
            }

            return resultView;
        }

        private DataBrowserTopLevel GetTopLevelObjectWithName(string name) {
            for (int i = dataBrowserTopLevelElements.Count - 1; i >= 0; i--) {
                var topLevelElement = dataBrowserTopLevelElements[i];
                if (topLevelElement.topLevelName=="name") {
                    return topLevelElement;
                }
            }
            return null;
        }

        public override void CreateDataBrowserTopLevelElement(string name, IList objectList) {
            var checkForToplevelElement = GetTopLevelObjectWithName(name);
            if (checkForToplevelElement != null) {
                
                return;
            }
            dataBrowserTopLevelElements.Add(new DataBrowserTopLevel() {
                topLevelName = name,
                objectList = objectList
            });
        }

        public override void AddDataBrowserObjectConverter(Type objType, Func<object, object> converter) {
            typeConverter[objType] = converter;
        }

        public override object DataBrowserConvertObject(object inObject) {
            Type inType = inObject.GetType();
            foreach (Type converterType in typeConverter.Keys) {
                if (converterType.IsInstanceOfType(inObject)) {
                    return typeConverter[converterType](inObject);
                }
            }
            return inObject;
        }


        public override List<DataBrowserTopLevel> GetDataBrowserTopLevelElements() {
            return dataBrowserTopLevelElements;
        }

        public override void OutputGameInfo(float systemStartupTime) {
            this.Publish(new Events.GameInfoChanged() { systemStartupTime = systemStartupTime });
        }
    }
}
