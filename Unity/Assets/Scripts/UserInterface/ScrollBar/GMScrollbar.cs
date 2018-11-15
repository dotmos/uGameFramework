using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UserInterface.Scrollbar {
    [AddComponentMenu(NamingHelper.Scrollbar.Name, 34)]
    [RequireComponent(typeof(RectTransform))]
    public class GMScrollbar : Selectable, IBeginDragHandler, IDragHandler, IInitializePotentialDragHandler, ICanvasElement {

        [Tooltip("Left/Up navigation button")]
        [SerializeField]
        private Selectable m_firstButton;
        public Selectable firstButton { get { return m_firstButton; } set { if (SetPropertyUtility.SetClass(ref m_firstButton, value)) {} } }

        [Tooltip("Right/Bottom navigation button")]
        [SerializeField]
        private Selectable m_secondButton;
        public Selectable secondButton { get { return m_secondButton; } set { if (SetPropertyUtility.SetClass(ref m_secondButton, value)) {} } }

        /// <summary>
        /// simple batch processor to iterate on our navigation buttons
        /// </summary>
        private NavigationButtonProcessor navButtonProcessor = new NavigationButtonProcessor();

        [Range(0f, 1f), Tooltip("Impact of the adjustment.")]
        [SerializeField]
        private float m_navButtonStep;
        public float navButtonStep { get { return m_navButtonStep; } set { m_navButtonStep = value; } }

        [Range(0f, 1f), Tooltip("Time until continous scrolling is activated.")]
        [SerializeField]
        public float m_navContinousScrollDelay;
        public float navContinousScrollDelay { get { return m_navContinousScrollDelay; } set { m_navContinousScrollDelay = value; } }

        [SerializeField]
        private RectTransform m_HandleRect;
        public RectTransform handleRect { get { return m_HandleRect; } set { if (SetPropertyUtility.SetClass(ref m_HandleRect, value)) { UpdateCachedReferences(); UpdateVisuals(); } } }

        [Serializable]
        public class ScrollEvent : UnityEvent<float> { }
        // Allow for delegate-based subscriptions for faster events than 'eventReceiver', and allowing for multiple receivers.
        [SerializeField]
        private ScrollEvent m_OnValueChanged = new ScrollEvent();
        public ScrollEvent onValueChanged { get { return m_OnValueChanged; } set { m_OnValueChanged = value; } }

        // Direction of movement.
        [SerializeField]
        private Direction m_Direction = Direction.BottomToTop;
        public Direction direction { get { return m_Direction; } set { if (SetPropertyUtility.SetStruct(ref m_Direction, value)) UpdateVisuals(); } }

        // Behavior bound to the direction of movement
        private DirectionalStrategy behavior { get { return DirectionalStrategy.lookup[m_Direction]; } }

        // Behavior bound to the current movement direction
        private MoveDirection currentMoveDirection;
        private MoveStrategy moveBehavior { get { return MoveStrategy.lookup[currentMoveDirection]; } }

        // Scroll bar's current value in 0 to 1 range.
        [Range(0f, 1f)]
        [SerializeField]
        private float m_Value;
        public float value {
            get {
                float val = m_Value;
                if (m_NumberOfSteps > 1)
                    val = Mathf.Round(val * (m_NumberOfSteps - 1)) / (m_NumberOfSteps - 1);
                return val;
            }
            set {
                Set(value);
            }
        }

        // Scroll bar's current size in 0 to 1 range.
        [Range(0f, 1f)]
        [SerializeField]
        private float m_Size = 0.2f;
        public float size { get { return m_Size; } set { if (SetPropertyUtility.SetStruct(ref m_Size, Mathf.Clamp01(value))) UpdateVisuals(); } }

        // Number of steps the scroll bar should be divided into. For example 5 means possible values of 0, 0.25, 0.5, 0.75, and 1.0.
        [Range(0, 11)]
        [SerializeField]
        private int m_NumberOfSteps = 0;
        public int numberOfSteps { get { return m_NumberOfSteps; } set { if (SetPropertyUtility.SetStruct(ref m_NumberOfSteps, value)) { Set(m_Value); UpdateVisuals(); } } }

        [Space(6)]

        // Private fields
        private RectTransform m_ContainerRect;
        private DrivenRectTransformTracker m_Tracker;
        private Coroutine m_PointerDownRepeat;
        private bool isPointerDownAndNotDragging = false;

        // The offset from handle position to mouse down position
        private Vector2 m_Offset = Vector2.zero;

        // Size of each step.
        private float stepSize { get { return (m_NumberOfSteps > 1) ? 1f / (m_NumberOfSteps - 1) : 0.1f; } }

#if UNITY_EDITOR
        protected override void OnValidate() {
            base.OnValidate();

            m_Size = Mathf.Clamp01(m_Size);

            //This can be invoked before OnEnabled is called. So we shouldn't be accessing other objects, before OnEnable is called.
            if (IsActive()) {
                UpdateCachedReferences();
                Set(m_Value, false);
                // Update rects since other things might affect them even if value didn't change.
                UpdateVisuals();
            }

            var prefabType = UnityEditor.PrefabUtility.GetPrefabType(this);
            if (prefabType != UnityEditor.PrefabType.Prefab && !Application.isPlaying)
                CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
        }

#endif // if UNITY_EDITOR

        public virtual void Rebuild(CanvasUpdate executing) {
#if UNITY_EDITOR
            if (executing == CanvasUpdate.Prelayout)
                onValueChanged.Invoke(value);
#endif
        }

        public virtual void LayoutComplete() { }

        public virtual void GraphicUpdateComplete() { }

        protected override void Awake() {
            base.Awake();
            SetupNavigationButtons();
        }

        private void SetupNavigationButtons() {
            navButtonProcessor.continuousScrollDelay = navContinousScrollDelay;
            navButtonProcessor.buttons.Clear();

            if (firstButton != null) {
                navButtonProcessor.buttons.Add(
                    new NavigationButton(firstButton.gameObject, NavigationButton.Direction.Positive, navButtonStep, OnNavigationMove));
            }

            if (secondButton != null) {
                navButtonProcessor.buttons.Add(
                    new NavigationButton(secondButton.gameObject, NavigationButton.Direction.Negative, navButtonStep, OnNavigationMove));
            }
        }

        /// <summary>
        /// Callback, fired when a navigation button was clicked or is being held
        /// </summary>
        /// <param name="step"></param>
        private void OnNavigationMove(float step) {
            Set(value + step);
        }

        /// <summary>
        /// Update function, needed to manage our button navigation
        /// Ticks are throttled by navContinousScrollDelay.
        /// </summary>
        private void Update() {
            navButtonProcessor.Update();
        }

        protected override void OnEnable() {
            base.OnEnable();
            UpdateCachedReferences();
            Set(m_Value, false);
            // Update rects since they need to be initialized correctly.
            UpdateVisuals();
        }

        protected override void OnDisable() {
            m_Tracker.Clear();
            base.OnDisable();
        }

        void UpdateCachedReferences() {
            if (m_HandleRect && m_HandleRect.parent != null)
                m_ContainerRect = m_HandleRect.parent.GetComponent<RectTransform>();
            else
                m_ContainerRect = null;
        }

        void Set(float input) {
            Set(input, true);
        }

        void Set(float input, bool sendCallback) {
            //If our scrollbar is full size we don't have content to scroll
            if (size == 1) return;

            float currentValue = m_Value;
            // Clamp the input
            m_Value = Mathf.Clamp01(input);

            // If the stepped value doesn't match the last one, it's time to update
            if (currentValue == value)
                return;

            UpdateVisuals();
            if (sendCallback) {
                UISystemProfilerApi.AddMarker("Scrollbar.value", this);
                m_OnValueChanged.Invoke(value);
            }
        }

        protected override void OnRectTransformDimensionsChange() {
            base.OnRectTransformDimensionsChange();

            //This can be invoked before OnEnabled is called. So we shouldn't be accessing other objects, before OnEnable is called.
            if (!IsActive())
                return;

            UpdateVisuals();
        }

        // Force-update the scroll bar. Useful if you've changed the properties and want it to update visually.
        private void UpdateVisuals() {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                UpdateCachedReferences();
#endif
            m_Tracker.Clear();

            if (m_ContainerRect != null) {
                m_Tracker.Add(this, m_HandleRect, DrivenTransformProperties.Anchors);
                Vector2 anchorMin = Vector2.zero;
                Vector2 anchorMax = Vector2.one;

                float movement = value * (1 - size);
                if (behavior.reverse) {
                    anchorMin[behavior.axisValue] = 1 - movement - size;
                    anchorMax[behavior.axisValue] = 1 - movement;
                }
                else {
                    anchorMin[behavior.axisValue] = movement;
                    anchorMax[behavior.axisValue] = movement + size;
                }

                m_HandleRect.anchorMin = anchorMin;
                m_HandleRect.anchorMax = anchorMax;
            }
        }

        // Update the scroll bar's position based on the mouse.
        void UpdateDrag(PointerEventData eventData) {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (m_ContainerRect == null)
                return;

            Vector2 localCursor;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(m_ContainerRect, eventData.position, eventData.pressEventCamera, out localCursor))
                return;

            Vector2 handleCenterRelativeToContainerCorner = localCursor - m_Offset - m_ContainerRect.rect.position;
            Vector2 handleCorner = handleCenterRelativeToContainerCorner - (m_HandleRect.rect.size - m_HandleRect.sizeDelta) * 0.5f;

            float parentSize = behavior.ChooseValueByAxis(m_ContainerRect.rect.width, m_ContainerRect.rect.height);
            float remainingSize = parentSize * (1 - size);
            if (remainingSize <= 0)
                return;

            Set(behavior.updateDragBehavior(handleCorner, remainingSize));
        }

        private bool MayDrag(PointerEventData eventData) {
            return IsActive() && IsInteractable() && eventData.button == PointerEventData.InputButton.Left;
        }

        public virtual void OnBeginDrag(PointerEventData eventData) {
            isPointerDownAndNotDragging = false;

            if (!MayDrag(eventData))
                return;

            if (m_ContainerRect == null)
                return;

            m_Offset = Vector2.zero;
            if (RectTransformUtility.RectangleContainsScreenPoint(m_HandleRect, eventData.position, eventData.enterEventCamera)) {
                Vector2 localMousePos;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_HandleRect, eventData.position, eventData.pressEventCamera, out localMousePos))
                    m_Offset = localMousePos - m_HandleRect.rect.center;
            }
        }

        public virtual void OnDrag(PointerEventData eventData) {
            if (!MayDrag(eventData))
                return;

            if (m_ContainerRect != null)
                UpdateDrag(eventData);
        }

        public override void OnPointerDown(PointerEventData eventData) {
            if (!MayDrag(eventData))
                return;

            base.OnPointerDown(eventData);
            isPointerDownAndNotDragging = true;
            m_PointerDownRepeat = StartCoroutine(ClickRepeat(eventData));
        }

        protected IEnumerator ClickRepeat(PointerEventData eventData) {
            while (isPointerDownAndNotDragging) {
                if (!RectTransformUtility.RectangleContainsScreenPoint(m_HandleRect, eventData.position, eventData.enterEventCamera)) {
                    Vector2 localMousePos;
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_HandleRect, eventData.position, eventData.pressEventCamera, out localMousePos)) {
                        var axisCoordinate = behavior.ChooseValueByAxis(localMousePos.x, localMousePos.y);
                        if (axisCoordinate < 0)
                            value -= size;
                        else
                            value += size;
                    }
                }
                yield return new WaitForEndOfFrame();
            }
            StopCoroutine(m_PointerDownRepeat);
        }

        public override void OnPointerUp(PointerEventData eventData) {
            base.OnPointerUp(eventData);
            isPointerDownAndNotDragging = false;
        }

        public override void OnMove(AxisEventData eventData) {
            if (!IsActive() || !IsInteractable()) {
                base.OnMove(eventData);
                return;
            }

            currentMoveDirection = eventData.moveDir;

            if (moveBehavior.axisValid(behavior.axis) && moveBehavior.findSelectable(this) == null) {
                Set(moveBehavior.movement(behavior, value, stepSize));
            }else {
                base.OnMove(eventData);
            }
        }

        public override Selectable FindSelectableOnLeft() {
            if (MoveStrategy.lookup[MoveDirection.Left].selectableShouldBeNull(navigation.mode, behavior.axis))
                return null;
            return base.FindSelectableOnLeft();
        }

        public override Selectable FindSelectableOnRight() {
            if (MoveStrategy.lookup[MoveDirection.Right].selectableShouldBeNull(navigation.mode, behavior.axis))
                return null;
            return base.FindSelectableOnRight();
        }

        public override Selectable FindSelectableOnUp() {
            if (MoveStrategy.lookup[MoveDirection.Up].selectableShouldBeNull(navigation.mode, behavior.axis))
                return null;
            return base.FindSelectableOnUp();
        }

        public override Selectable FindSelectableOnDown() {
            if (MoveStrategy.lookup[MoveDirection.Down].selectableShouldBeNull(navigation.mode, behavior.axis))
                return null;
            return base.FindSelectableOnDown();
        }

        public virtual void OnInitializePotentialDrag(PointerEventData eventData) {
            eventData.useDragThreshold = false;
        }

        public void SetDirection(Direction direction, bool includeRectLayouts) {
            Axis oldAxis = behavior.axis;
            bool oldReverse = behavior.reverse;
            this.direction = direction;

            if (!includeRectLayouts)
                return;

            if (behavior.axis != oldAxis)
                RectTransformUtility.FlipLayoutAxes(transform as RectTransform, true, true);

            if (behavior.reverse != oldReverse)
                RectTransformUtility.FlipLayoutOnAxis(transform as RectTransform, behavior.axisValue, true, true);
        }
    }
}
