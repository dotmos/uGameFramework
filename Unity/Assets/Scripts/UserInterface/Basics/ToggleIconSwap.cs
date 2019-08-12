using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UserInterface {
    public class ToggleIconSwap : MonoBehaviour
    {
        public Toggle toggle;
        public Image icon;
        [Header("Icons")]
        public Sprite activeIcon;
        public Sprite inactiveIcon;

        private void Awake() {
            toggle.onValueChanged.AddListener(OnValueChanged);
            OnValueChanged(toggle.isOn);
        }

        void OnValueChanged(bool isOn) {
            icon.sprite = isOn ? activeIcon : inactiveIcon;
        }
    }
}
