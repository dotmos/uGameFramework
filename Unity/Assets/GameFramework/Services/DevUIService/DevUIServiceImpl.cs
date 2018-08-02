using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

using Zenject;
using UniRx;

namespace Service.DevUIService {

    partial class DevUIServiceImpl : DevUIServiceBase {

        public ReactiveDictionary<string,DevUIView> rxViews;
        

        protected override void AfterInitialize() {
            // this is called right after the Base-Classes Initialize-Method. _eventManager and disposableManager are set
            rxViews = new ReactiveDictionary<string, DevUIView>();

            var win = AddView("ServiceDEVUI");
            win.AddElement(new DevUIButton("kickit", () => {
                UnityEngine.Debug.Log("KICKIT");
            }));

            win.AddElement(new DevUILUAButton("kickit-lua", "print('tom')"));
        }


        public override ReactiveDictionary<string, DevUIView> GetRxViews() {
            return rxViews;
        }


        public override DevUIView GetView(string viewName) {
            if (ViewNameExists(viewName)) {
                return rxViews[viewName];
            }
            return null;
        }

        public override void RemoveView(string viewName) {
            rxViews.Remove(viewName);
        }

        public override bool ViewNameExists(string viewName) {
            return rxViews.ContainsKey(viewName);
            
        }


        public override DevUIView AddView(string viewName) {
            if (ViewNameExists(viewName)) {
                return rxViews[viewName];
            }
            var newWindow = new DevUIView(viewName);
            rxViews[viewName] =newWindow;
            return newWindow;
        }





        protected override void OnDispose() {
            // do your IDispose-actions here. It is called right after disposables got disposed
        }

    }



}
