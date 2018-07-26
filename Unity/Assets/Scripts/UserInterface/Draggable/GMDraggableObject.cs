using UnityEngine;
using UnityEngine.EventSystems;

namespace UserInterface {
    [RequireComponent(typeof(EventTrigger))]
    public class GMDraggableObject : MonoBehaviour {
        public RectTransform target;
        [Space]
        public bool keepOnScreen;
        public bool dragOnX = true;
        public bool dragOnY = true;
        [Space]
        public Vector2 marginMin;
        public Vector2 marginMax;

        private Vector3 initialPosition;
        private Vector2 initialSize;
        private EventTrigger eventTrigger;

        void Start() {
            target.pivot = new Vector2(0, 1);

            initialPosition = target.transform.position;
            initialSize = target.sizeDelta;

            eventTrigger = GetComponent<EventTrigger>();
            EventUtility.CreateEventTriggerEntry(eventTrigger, EventTriggerType.Drag, OnDrag);
            EventUtility.CreateEventTriggerEntry(eventTrigger, EventTriggerType.EndDrag, OnEndDrag);

            RestrictToScreen();
        }

        private void OnRectTransformDimensionsChange() {
            RestrictToScreen();
        }

        void OnDrag(BaseEventData data) {
            PointerEventData ped = (PointerEventData)data;

            Vector2 deltaMovement = ped.delta;

            if (!dragOnX) deltaMovement = deltaMovement * new Vector2(0, 1);
            if (!dragOnY) deltaMovement = deltaMovement * new Vector2(1, 0);

            target.transform.Translate(deltaMovement);

            if (keepOnScreen) {
                RestrictToScreen();
            }
        }

        void OnEndDrag(BaseEventData data) {
            if (keepOnScreen) {
                RestrictToScreen();
            }
        }

        void RestrictToScreen() {
            float minY = Mathf.Round(-Screen.height + target.sizeDelta.y);
            float maxX = Mathf.Round(Screen.width - target.sizeDelta.x);

            float clampedX = Mathf.Clamp(target.anchoredPosition.x, 0f, maxX);
            float clampedY = Mathf.Clamp(target.anchoredPosition.y, minY, 0f);
            target.anchoredPosition = new Vector2(clampedX, clampedY);
        }

        void ResetWindow() {
            target.transform.position = initialPosition;
            target.sizeDelta = initialSize;
        }
    }
}