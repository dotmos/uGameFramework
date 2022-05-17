using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UserInterface {
    public class UIViewToggle : MonoBehaviour
    {
        public Text labelOutput;
        public Action callback;
        public GMToggle toggle;
        
        public void Initialize(string label, bool initialValue, Action callback) {
            labelOutput.text = label;
            this.callback = callback;

            toggle.isOn = initialValue;
            toggle.onValueChanged.AddListener(OnValueChanged);

#if ENABLE_CONSOLE_UI
            toggle.navigation = Navigation.defaultNavigation;
#endif
        }

        void OnValueChanged(bool isOn) {
            if (callback != null) callback();
        }
    }
}
