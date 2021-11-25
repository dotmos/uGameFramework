using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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
        public Color selectedColor;
        public Color disabledColor;
        public bool surpressSubmitEvent = false;

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
            if (targetGraphic == null) targetGraphic = GetComponent<Image>();

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

        public GMTabbar GetTabBar()
        {
            return myTabBar;
        }

        protected override void DoStateTransition(SelectionState state, bool instant) {
            base.DoStateTransition(state, instant);

            if (IsInteractable()) {
                if (state == SelectionState.Pressed) {
                    ColorizeElements(pressedColor);
                } else if (isOn) {
                    ColorizeElements(activeColor);
                } else if (state == SelectionState.Highlighted) {
                    ColorizeElements(highlightColor);
                } else if (state == SelectionState.Selected) {
                    ColorizeElements(selectedColor);
                } else {
                    ColorizeElements(defaultColor);
                }

                if (border != null) border.SetActive(isOn);
            } else {
                ColorizeElements(disabledColor);
                if (border != null) border.SetActive(false);
            }
        }

        protected virtual void OnToggleValueChanged(bool _isOn) {
            DoStateTransition(currentSelectionState, false);
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


        public override bool IsInteractable() {
            //Is Only interactable if it has a parent (thus is part of a canvas).
            //This is used to avoid that it is considered for navigation creation
            //event though it has been returned (to root) by an object pool
            return base.IsInteractable() && transform.parent != null;
        }

        public override void OnSubmit(BaseEventData eventData) {
            if (!surpressSubmitEvent) {
                base.OnSubmit(eventData);
            }
        }
    }
}
