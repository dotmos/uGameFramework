using Service.DevUIService;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace UserInterface {
    public class UIViewLUAButton : GameComponent {

        public GameObject outputMode;
        public GameObject editMode;
        [Space]
        public GMButton executeButton;
        public GMButton editButton;
        public GMButton saveButton;
        public GMButton deleteButton;
        [Space]
        public GMInputField luaCommandInput;
        public GMInputField nameInput;
        public Text labelOutput;

        private DevUILUAButton luaButton;

        public void Initialize(string label, Action callback, DevUILUAButton luaButton) {
            this.luaButton = luaButton;
            labelOutput.text = label;

            executeButton.onClick.AddListener(callback.Invoke);

            editButton.onClick.AddListener(
                () => ActivateEditMode(true)
            );

            saveButton.onClick.AddListener(
                () => ActivateEditMode(false)
            );

            saveButton.onClick.AddListener(SaveLUACommand);

            luaCommandInput.text = luaButton.LuaCommand;
            nameInput.text = luaButton.name;

            luaButton.luaCommandProperty.Subscribe(e => {
                luaCommandInput.text = e;
            }).AddTo(this);
        }

        public void ActivateEditMode(bool activate) {
            editMode.SetActive(activate);
            outputMode.SetActive(!activate);
        }

        void SaveLUACommand() {
            luaButton.SetLuaCall(luaCommandInput.text);
            luaButton.name = labelOutput.text = nameInput.text;
        }
    }
}
