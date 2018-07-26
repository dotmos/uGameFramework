using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UserInterface {
    public class ButtonBase : MonoBehaviour {

        protected GMButton button;

        private void Awake() {
            button = gameObject.GetComponent<GMButton>();
            button.onClick.AddListener(OnClick);
        }

        protected virtual void OnClick() {}
    }
}
