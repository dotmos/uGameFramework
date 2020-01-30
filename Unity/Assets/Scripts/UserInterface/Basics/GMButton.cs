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

        public bool isPressed {
            get; private set;
        }

        // Event delegate triggered on mouse or touch down.
        [SerializeField]
        GMButtonRightClickEvent _onRightClick = new GMButtonRightClickEvent();

        [Serializable]
        public class GMButtonRightClickEvent : UnityEvent { }

        [SerializeField]
        GMButtonPressedEvent _onPressed = new GMButtonPressedEvent();

        [Serializable]
        public class GMButtonPressedEvent : UnityEvent { }

        [SerializeField]
        GMButtonReleaseEvent _onRelease = new GMButtonReleaseEvent();

        [Serializable]
        public class GMButtonReleaseEvent : UnityEvent { }

        protected GMButton() { }

        public override void OnPointerClick(PointerEventData eventData) {
            if (eventData.button == PointerEventData.InputButton.Left) {
                if (interactable && !isPressed) onClick.Invoke();
            } else if (eventData.button == PointerEventData.InputButton.Right) {
                if (interactable) onRightClick.Invoke();
            }
        }

        public GMButtonRightClickEvent onRightClick {
            get { return _onRightClick; }
            set { _onRightClick = value; }
        }

        public GMButtonPressedEvent onPressed {
            get { return _onPressed; }
            set { _onPressed = value; }
        }

        public GMButtonReleaseEvent onRelease {
            get { return _onRelease; }
            set { _onRelease = value; }
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
                isPressed = true;

                foreach (Graphic colorizeElement in colorizeElements)
                {
                    colorizeElement.color = pressedColor;
                }
            }
        }

        public override void OnPointerExit(PointerEventData eventData) {
            base.OnPointerExit(eventData);

            if (isPressed) {
                onRelease.Invoke();
                isPressed = false;
            }
        }

        public override void OnPointerUp(PointerEventData eventData) {
            base.OnPointerUp(eventData);

            if (isPressed && interactable) {
                onRelease.Invoke();
                isPressed = false;
            }
        }

        private void Update() {
            if (isPressed && interactable) {
                onPressed.Invoke();
            }
        }

        public void UpdateColors() {
            DoStateTransition(currentSelectionState, true);
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
