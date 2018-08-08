
using Service.Events;
using System.Collections.Generic;
using MoonSharp.Interpreter;

using System;
using Zenject;
using UniRx;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Linq;
using Service.TimeService;

namespace Service.DevUIService {



    /// <summary>
    /// Data model for DevUIViews
    /// </summary>
    [DataContract]
    public class DevUIView : IDisposable {
        private ReactiveProperty<string> nameProperty=new ReactiveProperty<string>("");

        public CompositeDisposable disposables = new CompositeDisposable();

        [DataMember]
        public bool createdDynamically;

        [DataMember]
        public bool extensionAllowed=true;

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

        public DevUIView(string name,bool dynamicallyCreated=false) {
            Kernel.Instance.Inject(this);
            this.Name = name;
            this.createdDynamically = dynamicallyCreated;
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

        public virtual void Dispose() {
            foreach (var elem in uiElements) {
                elem.Dispose();
            }
            uiElements.Clear();
            disposables.Clear();
        }

    }

    /// <summary>
    /// Base type for devui-elements
    /// </summary>
    /// 
    [DataContract]
    public class DevUIElement : IDisposable {
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
        public virtual void Dispose() { }

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

        public override void Dispose() {
            luaCommandProperty.Dispose();
        }
    }

    [DataContract]
    public class DevUIKeyValue : DevUIElement
    {
        public Action<string> OnValueChangeRequested {
            get; set;
        }

        /// <summary>
        /// The reactive property to keep track of the current value
        /// </summary>
        public ReactiveProperty<string> valueProperty = new ReactiveProperty<string>("");
        public string Value {
            get { return valueProperty.Value; }
            set {
                valueProperty.Value = value;
            }
        }

        public DevUIKeyValue(string name,string value="") : base(name) {
            Value = value;
        }

        public override void Dispose() {
            valueProperty.Dispose();
        }

        public void RequestValueChange(string newValue) {
            if (OnValueChangeRequested == null) {
                // no special handling? just set the value
                Value = newValue;
            } else {
                // there is a custom handling for this (e.g. set the value in the memory-browser which might trigger setting the Value....)
                OnValueChangeRequested(newValue);
            }
        }

    }

    /// <summary>
    /// DevUI-Element that let you watch a specific lua-expression at a given rate
    /// </summary>
    [DataContract]
    public class DevUILuaExpression : DevUIKeyValue
    {
        [Inject]
        private Service.TimeService.ITimeService timeService;
        [Inject]
        Service.Scripting.IScriptingService _scriptingService;

        [DataMember]
        public float updateRateInSeconds = 2;

        private TimerElement timer;

        Func<string> luaFunc = null;

        public ReactiveProperty<string> luaExpressionProperty = new ReactiveProperty<string>();

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
        public DevUILuaExpression(string name, float interval) : base(name) {
            SetInterval(interval);            
            SetLuaExpresion("testValue..'-'..math.random()"); // <-- this "testValue" is specified in DevUIServiceScriptingAPI and is accessable in lua)
        }

        public void SetInterval(float f) {
            if (timer == null) {
                timer = timeService.CreateGlobalTimer(f, () => {
                    UpdateExpression();
                },0);
            }
            timer.interval = f;
            timer.timeLeft = f;
        }

        public void SetLuaExpresion(string luaExpression) {
            luaExpressionProperty.Value = luaExpression;

            luaFunc = () => {
                // execute the current command with the scripting service
                var result = _scriptingService.ExecuteStringOnMainScript("return "+LuaExpression);
                return result;
            };
        }

        /// <summary>
        /// Call the lua-func at set the result as value
        /// </summary>
        public void UpdateExpression() {
            if (luaFunc == null) {
                return;
            }
            var result = luaFunc();
            Value = result;
        }

        public override void Dispose() {
            base.Dispose();
            luaExpressionProperty.Dispose();
            if (timer != null) {
                timeService.RemoveGlobalTimer(timer);
                timer = null;
            }
        }
    }

}