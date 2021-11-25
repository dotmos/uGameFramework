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

        public override bool IsInteractable() {
            //Is Only interactable if it has a parent (thus is part of a canvas).
            //This is used to avoid that it is considered for navigation creation
            //event though it has been returned (to root) by an object pool
            return base.IsInteractable() && transform.parent != null;
        }
    }
}
