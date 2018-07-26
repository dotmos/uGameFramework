using System.Collections.Generic;
using UnityEngine;

namespace UserInterface.Scrollbar {
    internal class NavigationButtonProcessor {
        public List<NavigationButton> buttons = new List<NavigationButton>();
        public float continuousScrollDelay;

        public NavigationButtonProcessor() { }

        public NavigationButtonProcessor(float continuousScrollDelay) {
            this.continuousScrollDelay = continuousScrollDelay;
        }

        public void OnWhilePointerDown(NavigationButton btn) {
            if (btn.continuousScrollTimer > continuousScrollDelay) {
                btn.Move();
            }
            else {
                btn.continuousScrollTimer += Time.deltaTime;
            }
        }

        public void Update() {
            foreach (var btn in buttons) {
                if (btn.isPointerDown) {
                    OnWhilePointerDown(btn);
                }
            }
        }
    }
}
