using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

using Zenject;
using UniRx;
using System.Linq;
using Service.Serializer;

namespace Service.DevUIService {

    partial class DevUIServiceImpl : DevUIServiceBase {

        public ReactiveCollection<DevUIView> rxViews;

        [Inject]
        Service.LoggingService.ILoggingService logging;

        [Inject(Id = SerializerID.JsonNet)]
        Service.Serializer.ISerializerService serializer;

        [Inject]
        Service.FileSystem.IFileSystemService fileSystem;
            

        protected override void AfterInitialize() {
            // this is called right after the Base-Classes Initialize-Method. _eventManager and disposableManager are set
            rxViews = new ReactiveCollection<DevUIView>();

            var win = AddView("ServiceDEVUI");
            win.AddElement(new DevUIButton("kickit", () => {
                UnityEngine.Debug.Log("KICKIT");
            }));

            win.AddElement(new DevUILUAButton("kickit-lua", "print('tom')"));

            logging.Info("Something is good", "org.tt");

            OnEvent<Kernel.Events.OnApplicationQuit>().Subscribe(_ => {
                SaveViews();
            });

            try {
                LoadViews();
            }
            catch (Exception e) {
                logging.Error("Could not load Views!");
                UnityEngine.Debug.LogException(e);
            }
            
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
