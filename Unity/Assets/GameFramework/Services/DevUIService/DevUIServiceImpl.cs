using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

using Zenject;
using UniRx;
using System.Linq;
using Service.Serializer;
using UnityEngine;
using System.IO;

namespace Service.DevUIService {

    partial class DevUIServiceImpl : DevUIServiceBase {

        public const string SUBFOLDER_ARCHIEVE = "archieve";

        public ReactiveCollection<DevUIView> rxViews;

        [Inject]
        Service.LoggingService.ILoggingService logging;

        [Inject(Id = SerializerID.JsonNet)]
        Service.Serializer.ISerializerService serializer;

        [Inject]
        Service.FileSystem.IFileSystemService fileSystem;

        [Inject]
        Service.Scene.ISceneService sceneService;

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
        }

        public override void RemoveViewToArchieve(DevUIView view) {
            RemoveViewFromModel(view);
            // save view to archieve path (true)
            SaveViewToPath(view, true);
            fileSystem.RemoveFile(view.currentFilename);
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

            List<string> tempCurrentViewFiles = new List<string>(viewPathsLoaded);

            foreach (string viewFile in viewFiles) {
                // did we already load this view? if yes, skip
                if (tempCurrentViewFiles.Contains(viewFile)) {
                    // remove file from temp list (the ones that stays in list are the ones to be deleted afterwards)
                    tempCurrentViewFiles.Remove(viewFile);
                    continue;
                }

                var viewDataAsString = fileSystem.LoadFileAsString(viewFile);
                var viewData = serializer.DeserializeToObject<DevUIView>(viewDataAsString);

                if (usedViewNames.Contains(viewData.Name)) {
                    logging.Warn("There is already already a view with the name " + viewData.Name + "! This results in merged views");
                }

                var view = GetView(viewData.Name);
                if (view == null) {
                    // a new view
                    view = AddView(viewData.Name);
                }
                usedViewNames.Add(viewData.Name);
                view.currentFilename = viewFile;
                
                foreach (var uiElem in viewData.uiElements) {
                    view.AddElement(uiElem,false);
                }

                viewPathsLoaded.Add(viewFile);
            }
            // are there any files left, that were loaded before, but now vanished?
            foreach (string oldPath in tempCurrentViewFiles) {
                var view = rxViews.Where(v => v.currentFilename == oldPath).FirstOrDefault();
                if (view != null) {
                    RemoveViewFromModel(view);
                }
            }
        }

        public override void SaveViews() {
            foreach (var view in rxViews) {
                SaveViewToPath(view);
            }
        }

        private void SaveViewToPath(DevUIView view, bool saveToArchieve=false,bool forceNewFilename=false) {
            var viewAsString = serializer.Serialize(view);

            if (saveToArchieve) {
                var saveAsFilename = DateTime.Now.ToFileTime().ToString() + view.Name + ".json";
                fileSystem.WriteStringToFileAtDomain(FileSystem.FSDomain.DevUIViewsArchieve,saveAsFilename, viewAsString);
            } else {
                var saveAsFilename = (view.currentFilename == null || forceNewFilename) ? view.Name + ".json" : Path.GetFileName(view.currentFilename);
                fileSystem.WriteStringToFileAtDomain(FileSystem.FSDomain.DevUIViews, saveAsFilename, viewAsString);
            }

        }
    }



}
