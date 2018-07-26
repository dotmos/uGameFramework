using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UserInterface {
    public class UIViewProperty : MonoBehaviour {
        public Text labelOutput;
        public InputField input;

        public void Initialize(string label, string value, InputField.InputType inputType = InputField.InputType.Standard) {
            labelOutput.text = label;
            input.text = value;
            input.inputType = inputType;

            input.onEndEdit.AddListener(SetValue);
        }

        void SetValue(string value) {

        }
	}
}
