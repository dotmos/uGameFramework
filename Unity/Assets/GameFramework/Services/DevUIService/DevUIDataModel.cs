
using Service.Events;
using System.Collections.Generic;
using MoonSharp.Interpreter;

using System;
using Zenject;
using UniRx;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Linq;

namespace Service.DevUIService {

    /// <summary>
    /// Data model for DevUIViews
    /// </summary>
    [DataContract]
    public class DevUIView {
        private ReactiveProperty<string> nameProperty=new ReactiveProperty<string>("");

        [DataMember]
        public string Name {
            get { return nameProperty.Value; }
            set {
                if (nameProperty.Value == value) {
                    return;
                }
                // tell the world that the uiname has been changed
                var evt = new Events.UIViewRenamed() {
                    from = nameProperty.Value,
                    to = value,
                    view = this
                };
                nameProperty.Value = value;
                _eventsService.Publish(evt);
            }
        }

        public ReactiveCollection<DevUIElement> uiElements = new ReactiveCollection<DevUIElement>();

        /// <summary>
        /// Do not serialize. This is just meta-data
        /// </summary>
        public string currentFilename = null;

        [Inject]
        private Service.Events.IEventsService _eventsService;

        private Service.DevUIService.IDevUIService _devUIService;

        [DataMember]
        private List<DevUIElement> DATA_persistedUiElements {
            get {
                var dynamicals = uiElements.Where(elem => elem.createdDynamically).ToList();
                return dynamicals;
            }
            set {
                foreach (var elem in value) { uiElements.Add(elem); };
            }
        }

        public DevUIView(string name) {
            Kernel.Instance.Inject(this);
            this.Name = name;
        }

        /// <summary>
        /// Add an element to view and immediately go in editview (or specifiy not to do so)
        /// </summary>
        /// <param name="elem"></param>
        /// <param name="addInEditMode"></param>
        public void AddElement(DevUIElement elem,bool addInEditMode=true) {
            if (!uiElements.Contains(elem)) {
                uiElements.Add(elem);

                _eventsService.Publish(new Events.NewUIElement() {
                    view = this,
                    elem = elem,
                    inEditMode = addInEditMode
                });
            }
        }

        public void RemoveElement(DevUIElement elem) {
            uiElements.Remove(elem);
        }

    }

    /// <summary>
    /// Base type for devui-elements
    /// </summary>
    /// 
    [DataContract]
    public class DevUIElement {
        /// <summary>
        /// The name of this DevUI-Element
        /// </summary>
        [DataMember]
        public string name;

        /// <summary>
        /// if this element is created dynamically it gets persisted
        /// </summary>
        [DataMember]
        public bool createdDynamically = false;

        public DevUIElement(string name) {
            this.name = name;
            Kernel.Instance.Inject(this);
        }

    }

    /// <summary>
    /// DevUI-Button
    /// </summary>
    [DataContract]
    public class DevUIButton : DevUIElement {
        /// <summary>
        /// The Action to be called when this button is pressed
        /// </summary>
        protected Action callback;

        public DevUIButton(string name, Action action) : base(name) {
            callback = action;
        }

        /// <summary>
        /// Execute the button
        /// </summary>
        public void Execute() {
            if (callback != null) {
                callback();
            }
        }
    }

    /// <summary>
    /// Special Button to execute lua calls 
    /// </summary>
    [DataContract]
    public class DevUILUAButton : DevUIButton {
        Service.Scripting.IScriptingService _scriptingService;


        public ReactiveProperty<string> luaCommandProperty = new ReactiveProperty<string>();

        [DataMember]
        private string DATA_LuaCommand {
            get {return LuaCommand;}
            set { SetLuaCall(value); }
        }
    

        public string LuaCommand {
            get { return luaCommandProperty.Value; }
            private set { luaCommandProperty.Value = value; }
        }

        /// <summary>
        /// The Action to be called when this button is pressed
        /// </summary>
        public DevUILUAButton(string name, string action) : base(name,null) {
            // get the scripting-service
            _scriptingService = Kernel.Instance.Container.Resolve<Service.Scripting.IScriptingService>();
            SetLuaCall(action);
        }

        public void SetLuaCall(string luacall) {
            luaCommandProperty.Value = luacall;

            callback = () => {
                // execute the current command with the scripting service
                _scriptingService.ExecuteStringOnMainScript(LuaCommand);
            };
        }

        
    }

    /// <summary>
    /// DevUI-Element that let you watch a specific lua-expression at a given rate
    /// </summary>
    [DataContract]
    public class DevUILuaExpression : DevUIElement
    {
        public float updateRateInSeconds = 2;

        Service.Scripting.IScriptingService _scriptingService;
        Func<List<KeyValuePair<string,string>>> luaFunc = null;

        public ReactiveProperty<string> luaExpressionProperty = new ReactiveProperty<string>();
        public ReactiveProperty<List<KeyValuePair<string,string>>> currentValue = new ReactiveProperty<List<KeyValuePair<string, string>>>();

        [DataMember]
        private string DATA_LuaExpression {
            get { return LuaExpression; }
            set { SetLuaExpresion(value); }
        }


        public string LuaExpression {
            get { return luaExpressionProperty.Value; }
            private set { luaExpressionProperty.Value = value; }
        }

        /// <summary>
        /// The Action to be called when this button is pressed
        /// </summary>
        public DevUILuaExpression(string name) : base(name) {
            // get the scripting-service
            _scriptingService = Kernel.Instance.Container.Resolve<Service.Scripting.IScriptingService>();
            
            SetLuaExpresion("testValue"); // <-- this "testValue" is specified in DevUIServiceScriptingAPI and is accessable in lua)
        }

        public void SetLuaExpresion(string luaExpression) {
            luaExpressionProperty.Value = luaExpression;

            luaFunc = () => {
                // execute the current command with the scripting service
                var result = _scriptingService.ExecuteStringOnMainScriptRaw("return "+LuaExpression);
                var output = new List<KeyValuePair<string, string>>();
                output.Add(new KeyValuePair<string, string>("result", result.ToString()));
                return output;
            };
        }

        public void UpdateExpression() {

        }
    }

}