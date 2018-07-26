using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UserInterface.Scrollbar;

namespace UserInterface {
    public class AutoCompleteWindow : MonoBehaviour {
        public GMScrollbar scrollbar;

        private bool hasBeenEnabled;

        private void LateUpdate() {
            //Make sure scroll rect is still on bottom when auto complete window opens
            if (hasBeenEnabled) {
                scrollbar.value = 0;
                hasBeenEnabled = false;
            }
        }

        private void OnEnable() {
            hasBeenEnabled = true;
        }
    }
}
