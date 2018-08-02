
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
        string name;

        public DevUIElement(string name) {
            this.name = name;
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
        public Action callback;

        public DevUIButton(string name,Action action) : base(name) {
            callback = action;
        }
    }

}