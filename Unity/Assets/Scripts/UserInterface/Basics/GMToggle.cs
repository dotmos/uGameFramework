using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UserInterface {
    [AddComponentMenu(NamingHelper.Toggle.Name, 1)]
    public class GMToggle : Toggle {
        public List<Graphic> colorizeElements = new List<Graphic>();
        public Color defaultColor;
        public Color highlightColor;
        public Color pressedColor;
        public Color disabledColor;

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            if (state == SelectionState.Disabled) {
                foreach (Graphic colorizeElement in colorizeElements) {
                    colorizeElement.color = disabledColor;
                }
            } else if (state == SelectionState.Highlighted) {
                foreach (Graphic colorizeElement in colorizeElements) {
                    colorizeElement.color = highlightColor;
                }
            } else if (state == SelectionState.Normal) {
                foreach (Graphic colorizeElement in colorizeElements) {
                    colorizeElement.color = defaultColor;
                }
            } else if (state == SelectionState.Pressed) {
                foreach (Graphic colorizeElement in colorizeElements) {
                    colorizeElement.color = pressedColor;
                }
            }
        }
    }
}
