using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

using Zenject;
using UniRx;
using System.Linq;
using Service.Serializer;
using UnityEngine;

namespace Service.DevUIService {

    partial class DevUIServiceImpl : DevUIServiceBase {

        public ReactiveCollection<DevUIView> rxViews;

        [Inject]
        Service.LoggingService.ILoggingService logging;

        [Inject(Id = SerializerID.JsonNet)]
        Service.Serializer.ISerializerService serializer;

        [Inject]
        Service.FileSystem.IFileSystemService fileSystem;

        [Inject]
        Service.Scene.ISceneService sceneService;


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


        protected override void AfterInitialize() {
            // this is called right after the Base-Classes Initialize-Method. _eventManager and disposableManager are set
            rxViews = new ReactiveCollection<DevUIView>();

            // TODO: get rid of EveryUpdate
            Observable.EveryUpdate().Subscribe(_ => {
                if (UnityEngine.Input.GetKeyDown(KeyCode.F8)) {
                    ToggleScriptingConsole();
                }
            }).AddTo(disposables);

            // on startup create some sample data
            var rxStartup = Kernel.Instance.rxStartup;

            // For testing data that is already present when the DevConsole gets active add test-data at very early stage
            rxStartup.Add(() => {
                var win = AddView("ServiceDEVUI");
                win.AddElement(new DevUIButton("kickit", () => {
                    UnityEngine.Debug.Log("KICKIT");
                }));

                win.AddElement(new DevUILUAButton("kickit-lua", "print('tom')"));

                logging.Info("Something is good", "org.tt");
            },Priorities.PRIORITY_VERY_EARLY); // Init Test-Data at an early stage

            rxStartup.Add(UtilsObservable.LoadScene(developmentSceneID),Priorities.PRIORITY_EARLY);

            //TODO: This is a workaround to close the dev console on start. Usually the loadDevelopmentConsole command published above should load the scene deactivated (makeActive set to false) which doesn't seem to work.
            // To Ensure that the 
            rxStartup.Add(() => {
                CloseScriptingConsole();
            }, Priorities.PRIORITY_DEFAULT); // with using PRIORITY_DEFAULT which is called after the PRIORITY_EARLY-Block it is ensured that the scene is fully loaded, before this block is executed


            rxStartup.Add(() => {
                try {
                    LoadViews();
                }
                catch (Exception e) {
                    logging.Error("Could not load Views!");
                    UnityEngine.Debug.LogException(e);
                }
            },Priorities.PRIORITY_DEFAULT); // Default Priority



            // on shutdown persist the current views
            Kernel.Instance.rxShutDown.Add(() => {
                SaveViews();
            });


            
        }

        public override void WriteToScriptingConsole(string text) {
            this.Publish(new Events.WriteToScriptingConsole() { text = text });
        }

        public override void OpenScriptingConsole() {
            devConsoleActive = true;
            sceneService.ActivateScene(developmentSceneID);
            this.Publish(new Events.ScriptingConsoleOpened());
        }

        public override void CloseScriptingConsole() {
            devConsoleActive = false;
            sceneService.DeactivateScene(developmentSceneID);
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
            return rxViews.Where(v => v.name == viewName).FirstOrDefault();
        }

        public override void RemoveView(string viewName) {
            var view = GetView(viewName);
            if (view != null) {
                rxViews.Remove(view);
            }
        }

        public override bool ViewNameExists(string viewName) {
            return GetView(viewName) != null;
        }


        public override DevUIView AddView(string viewName) {
            var view = GetView(viewName);
            if (view != null) {
                return view;
            }
            var newView = new DevUIView(viewName);
            rxViews.Add(newView);
            return newView;
        }





        protected override void OnDispose() {
            // do your IDispose-actions here. It is called right after disposables got disposed
        }

        public override void LoadViews() {
            var viewFiles = fileSystem.GetFilesInDomain(FileSystem.FSDomain.DevUIViews,"*.json");

            // check if we already used a viewname
            HashSet<string> usedViewNames = new HashSet<string>();

            foreach (string viewFile in viewFiles) {
                var viewDataAsString = fileSystem.LoadFileAsString(viewFile);
                var viewData = serializer.DeserializeToObject<DevUIView>(viewDataAsString);

                if (usedViewNames.Contains(viewData.name)) {
                    logging.Warn("There is already already a view with the name " + viewData.name + "! This results in merged views");
                }

                var view = GetView(viewData.name);
                if (view == null) {
                    // a new view
                    view = AddView(viewData.name);
                }
                usedViewNames.Add(viewData.name);
                
                foreach (var uiElem in viewData.uiElements) {
                    view.AddElement(uiElem,false);
                }
            }
        }

        public override void SaveViews() {
            foreach (var view in rxViews) {
                var viewAsString = serializer.Serialize(view);
                fileSystem.WriteStringToFileAtDomain(FileSystem.FSDomain.DevUIViews, view.name + ".json", viewAsString);
            }
        }
    }



}
