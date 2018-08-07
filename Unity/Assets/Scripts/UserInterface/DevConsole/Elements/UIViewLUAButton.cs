using Service.DevUIService;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace UserInterface {
    public class UIViewLUAButton : UIViewEditableElement {

        public GMButton executeButton;
        public GMButton deleteButton;
        [Space]
        public GMInputField luaCommandInput;
        public GMInputField nameInput;
        public Text labelOutput;

        private DevUILUAButton luaButton;
        private DevUIView view;

        public virtual void Initialize(string label, Action callback, DevUILUAButton luaButton, DevUIView view) {
            base.Initialize();

            this.luaButton = luaButton;
            this.view = view;

            labelOutput.text = label;

            executeButton.onClick.AddListener(callback.Invoke);

            //Setup buttons
            if (luaButton.createdDynamically) {
                deleteButton.onClick.AddListener(Delete);
            } else {
                IsEditable = false;
            }

            luaCommandInput.text = luaButton.LuaCommand;
            nameInput.text = luaButton.name;

            luaButton.luaCommandProperty.Subscribe(e => {
                luaCommandInput.text = e;
            }).AddTo(this);
        }

        protected override void OnSave() {
            base.OnSave();

            luaButton.SetLuaCall(luaCommandInput.text);
            luaButton.name = labelOutput.text = nameInput.text;
        }

        void Delete() {
            view.RemoveElement(luaButton);
        }
    }
}
