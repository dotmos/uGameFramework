using Service.DevUIService;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;

namespace UserInterface {
    public class UIViewKeyValueOutput : UIViewEditableElement {
        [Space]
        public Text keyOutput;
        public Text valueOutput;
        public GMInputField valueInput;

        private DevUIKeyValue keyValue;

        public virtual void Initialize(DevUIKeyValue keyValue) {
            base.Initialize();

            this.keyValue = keyValue;

            //Initial setup
            keyOutput.text = keyValue.name;
            valueOutput.text = keyValue.Value;

            //Add listener
            keyValue.valueProperty.Subscribe(e => {
                valueOutput.text = e;
            }).AddTo(this);

            editButton.gameObject.SetActive(keyValue.showEditButton);
        }

        protected override void OnEdit() {
            base.OnEdit();

            valueInput.text = keyValue.Value;
        }

        protected override void OnSave() {
            base.OnSave();

            valueOutput.text = keyValue.Value;

            // request value-change, the model will know what to do
            keyValue.RequestValueChange(valueInput.text);
        }
    }
}
