using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Service.DevUIService;
using UniRx;

namespace UserInterface {
    public class UIViewLUAExpression : UIViewEditableElement {

        public Text labelOutput;
        public Text valueOutput;
        [Space]
        public GMInputField nameInput;
        public GMInputField luaCommandInput;
        public Slider intervalSlider;
        [Space]
        public GMButton deleteButton;

        private DevUILuaExpression luaExpression;
        private DevUIView view;

        public virtual void Initialize(DevUILuaExpression luaExpression, DevUIView view) {
            base.Initialize();

            this.view = view;
            this.luaExpression = luaExpression;

            labelOutput.text = luaExpression.name;
            valueOutput.text = luaExpression.Value;

            //Value listener
            luaExpression.valueProperty.Subscribe(e => {
                valueOutput.text = e;
            }).AddTo(this);

            //Setup buttons
            if (luaExpression.createdDynamically) {
                deleteButton.onClick.AddListener(Delete);
            } else {
                IsEditable = false;
            }

            luaCommandInput.text = luaExpression.LuaExpression;
            nameInput.text = luaExpression.name;

            luaExpression.luaExpressionProperty.Subscribe(e => {
                luaCommandInput.text = e;
            }).AddTo(this);

            intervalSlider.value = luaExpression.updateRateInSeconds;
            luaExpression.SetInterval(intervalSlider.value);
        }

        protected override void OnSave() {
            base.OnSave();

            luaExpression.SetInterval(intervalSlider.value);
            luaExpression.SetLuaExpresion(luaCommandInput.text);
            luaExpression.name = labelOutput.text = nameInput.text;
        }

        void Delete() {
            view.RemoveElement(luaExpression);
        }
    }
}
