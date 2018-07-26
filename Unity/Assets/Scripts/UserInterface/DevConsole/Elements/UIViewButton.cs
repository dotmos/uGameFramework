using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UserInterface {
    public class UIViewButton : ButtonBase {
        public Text labelOutput;
        public Action callback;

        public void Initialize(string label, Action callback) {
            labelOutput.text = label;
            this.callback = callback;
        }

        protected override void OnClick() {
            base.OnClick();

            if (callback != null) callback();
        }
    }
}
