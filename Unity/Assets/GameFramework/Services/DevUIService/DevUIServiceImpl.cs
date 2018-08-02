using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

using Zenject;
using UniRx;

namespace Service.DevUIService {

    partial class DevUIServiceImpl : DevUIServiceBase {

        public ReactiveDictionary<string,DevUIView> rxWindows;
        

        protected override void AfterInitialize() {
            // this is called right after the Base-Classes Initialize-Method. _eventManager and disposableManager are set
            rxWindows = new ReactiveDictionary<string, DevUIView>();

            var win = AddView("Default");
            win.AddElement(new DevUIButton("kickit", () => {
                UnityEngine.Debug.Log("KICKIT");
            }));
        }


        public override ReactiveDictionary<string, DevUIView> GetRxViews() {
            return rxWindows;
        }


        public override DevUIView GetView(string viewName) {
            if (ViewNameExists(viewName)) {
                return rxWindows[viewName];
            }
            return null;
        }

        public override bool ViewNameExists(string viewName) {
            return rxWindows.ContainsKey(viewName);
            
        }


        public override DevUIView AddView(string viewName) {
            if (ViewNameExists(viewName)) {
                return rxWindows[viewName];
            }
            var newWindow = new DevUIView(viewName);
            rxWindows[viewName] =newWindow;
            return newWindow;
        }





        protected override void OnDispose() {
            // do your IDispose-actions here. It is called right after disposables got disposed
        }

    }



}
