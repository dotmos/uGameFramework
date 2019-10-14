using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UserInterface {
    public class ToggleIcon : MonoBehaviour {
        public Toggle toggle;

        public float rotationOff = -180f;
        public float rotationOn = 0f;

        private void Awake() {
            toggle.onValueChanged.AddListener(OnValueChanged);
        }

        void OnValueChanged(bool isOn) {
            float rotationValue = isOn ? rotationOn : rotationOff;
            transform.localRotation = Quaternion.Euler(new Vector3(0, 0, rotationValue));
        }
    }
}
