using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UserInterface
{
    [AddComponentMenu(NamingHelper.Tab.Name, 2)]
    public class GMTab : Toggle
    {
        public GameObject content;
        public GameObject border;
        public List<Graphic> colorizeElements = new List<Graphic>();
        public Color defaultColor;
        public Color activeColor;
        public Color highlightColor;
        public Color pressedColor;
        public Color disabledColor;

        private GMTabbar myTabBar;
        bool isBeingDestroyed;

        public bool InteractableStatus
        {
            get
            {
                return interactable;
            }
            set
            {
                if (value != interactable)
                {
                    interactable = value;
                    OnToggleValueChanged(isOn);
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();

            this.onValueChanged.AddListener(OnToggleValueChanged);
            this.toggleTransition = ToggleTransition.None;
            OnToggleValueChanged(isOn);
        }

        public void Initialize(GMTabbar _tabbar)
        {
            group = _tabbar;
            myTabBar = _tabbar;
            this.onValueChanged.AddListener(ToggleTab);
            targetGraphic = GetComponent<Image>();

            //Toggle tab correctly
            ToggleTab(isOn);
        }

        /// <summary>
        /// Toggles the tab
        /// </summary>
        /// <param name="activate">If set to <c>true</c> activate.</param>
        public void ToggleTab(bool activate)
        {
            //If gameobject has been deactivated make sure everything is toggled to false
            if (!gameObject.activeSelf) activate = false;

            if (activate)
            {
                if (myTabBar != null)
                    myTabBar.SetAsActiveTab(this);
            }
            else 
            {
                if (myTabBar != null)
                    myTabBar.DeactivateTab(this);
            }

            ActivateContent(activate);
        }

        /// <summary>
        /// Activates the tab's content.
        /// </summary>
        void ActivateContent(bool activate)
        {
            if (content == null || isBeingDestroyed) {
                return;
            }

            if (activate && isOn)
                content.SetActive(true);
            else
                content.SetActive(false);
        }

        public override void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);

            if (interactable && !isOn)
            {
                ColorizeElements(highlightColor);
            }
        }

        public override void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

            if (interactable && !isOn)
            {
                ColorizeElements(defaultColor);
            }
        }

        public GMTabbar GetTabBar()
        {
            return myTabBar;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            OnToggleValueChanged(isOn);
        }
#endif

        protected override void DoStateTransition(SelectionState state, bool instant) {
            base.DoStateTransition(state, instant);

            if (IsInteractable()) {
                if (state == SelectionState.Pressed) {
                    ColorizeElements(pressedColor);
                } else if (state == SelectionState.Highlighted) {
                    ColorizeElements(highlightColor);
                } else {
                    Color color = isOn ? activeColor : defaultColor;
                    ColorizeElements(color);
                }
            } else {
                ColorizeElements(disabledColor);
            }
        }

        protected virtual void OnToggleValueChanged(bool _isOn) {
            DoStateTransition(currentSelectionState, false);

            if (border != null) {
                border.SetActive(_isOn);
            }
        }

        void ColorizeElements(Color color) {
            foreach (Graphic colorizeElement in colorizeElements) {
                colorizeElement.color = color;
            }
        }

        public void UpdateColors() {
            DoStateTransition(currentSelectionState, true);
        }

        protected override void OnDestroy() {
            isBeingDestroyed = true;
        }
    }
}
