using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UserInterface
{
    [AddComponentMenu(NamingHelper.Button.Name, 0)]
    public class GMButton : Button {
        public List<Graphic> colorizeElements = new List<Graphic>();
        public Color defaultColor;
        public Color highlightColor;
        public Color pressedColor;
        public Color disabledColor;

        // Event delegate triggered on mouse or touch down.
        [SerializeField]
        GMButtonRightClickEvent _onRightClick = new GMButtonRightClickEvent();

        [Serializable]
        public class GMButtonRightClickEvent : UnityEvent { }

        protected GMButton() { }

        public override void OnPointerClick(PointerEventData eventData) {
            if (eventData.button == PointerEventData.InputButton.Left) {
                onClick.Invoke();
            } else if (eventData.button == PointerEventData.InputButton.Right) {
                _onRightClick.Invoke();
            }
        }

        public GMButtonRightClickEvent onRightClick {
            get { return _onRightClick; }
            set { _onRightClick = value; }
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            if (state == SelectionState.Disabled)
            {
                foreach (Graphic colorizeElement in colorizeElements)
                {
                    colorizeElement.color = disabledColor;
                }
            } else if (state == SelectionState.Highlighted)
            {
                foreach (Graphic colorizeElement in colorizeElements)
                {
                    colorizeElement.color = highlightColor;
                }
            } else if (state == SelectionState.Normal)
            {
                foreach (Graphic colorizeElement in colorizeElements)
                {
                    colorizeElement.color = defaultColor;
                }
            } else if (state == SelectionState.Pressed)
            {
                foreach (Graphic colorizeElement in colorizeElements)
                {
                    colorizeElement.color = pressedColor;
                }
            }
        }

        public void Highlight(bool highlight) {
            if (highlight) {
                DoStateTransition(SelectionState.Highlighted, true);
            } else {
                if (interactable) {
                    DoStateTransition(SelectionState.Normal, true);
                } else {
                    DoStateTransition(SelectionState.Disabled, true);
                }
            }
        }
    }
}
