
using Service.Events;
using System.Collections.Generic;
using MoonSharp.Interpreter;

using System;
using Zenject;
using UniRx;
using System.Diagnostics;


namespace Service.DevUIService{

    /// <summary>
    /// Data model for DevUIVies
    /// </summary>
    public class DevUIView
    {
        public string name;
        public ReactiveCollection<DevUIElement> uiElements = new ReactiveCollection<DevUIElement>();

        public DevUIView(string name) {
            this.name = name;
        }

        public void AddElement(DevUIElement elem) {
            if (!uiElements.Contains(elem)) {
                uiElements.Add(elem);
            }
        }

        public void RemoveElement(DevUIElement elem) {
            uiElements.Remove(elem);
        }

    }

    /// <summary>
    /// Base type for devui-elements
    /// </summary>
    public class DevUIElement
    {
        public string name;

        public DevUIElement(string name) {
            this.name = name;
            Kernel.Instance.Inject(this);
        }

    }

    /// <summary>
    /// DevUI-Button
    /// </summary>
    public class DevUIButton : DevUIElement
    {
        /// <summary>
        /// The Action to be called when this button is pressed
        /// </summary>
        protected Action callback;

        public DevUIButton(string name,Action action) : base(name) {
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
    public class DevUILUAButton : DevUIButton
    {
        Service.Scripting.IScriptingService _scriptingService;

        public ReactiveProperty<string> luaCommandProperty = new ReactiveProperty<string>();

        public string LuaCommand {
            get { return luaCommandProperty.Value; }
            set { luaCommandProperty.Value = value; }
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

}