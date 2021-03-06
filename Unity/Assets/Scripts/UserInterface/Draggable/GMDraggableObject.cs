﻿using UnityEngine;
using UnityEngine.EventSystems;

namespace UserInterface {
    [RequireComponent(typeof(EventTrigger))]
    public class GMDraggableObject : MonoBehaviour {
        public RectTransform target;
        [Space]
        public bool keepOnScreen;
        public bool dragOnX = true;
        public bool dragOnY = true;

        private EventTrigger eventTrigger;

        private Canvas canvas;
        private Canvas Canvas {
            get {
                if (canvas == null) canvas = GetComponentInParent<Canvas>();
                return canvas;
            } set {
                canvas = value;
            }
        }

        void Start() {
            target.pivot = new Vector2(0, 1);

            eventTrigger = GetComponent<EventTrigger>();
            EventUtility.CreateEventTriggerEntry(eventTrigger, EventTriggerType.Drag, OnDrag);
            EventUtility.CreateEventTriggerEntry(eventTrigger, EventTriggerType.EndDrag, OnEndDrag);

            RestrictToScreen();
        }

        void OnDrag(BaseEventData data) {
            PointerEventData ped = (PointerEventData)data;

            Vector2 deltaMovement = ped.delta;

            if (!dragOnX) deltaMovement = deltaMovement * new Vector2(0, 1);
            if (!dragOnY) deltaMovement = deltaMovement * new Vector2(1, 0);

            target.Translate(deltaMovement);

            if (keepOnScreen) {
                RestrictToScreen();
            }
        }

        void OnEndDrag(BaseEventData data) {
            if (keepOnScreen) {
                RestrictToScreen();
            }
        }

        private void OnRectTransformDimensionsChange() {
            RestrictToScreen();
        }

        void RestrictToScreen() {
            if (!target.gameObject.activeInHierarchy) return;

            Vector2 devConsoleSize = target.sizeDelta;

            if (devConsoleSize == Vector2.zero) {
                devConsoleSize = new Vector2(target.sizeDelta.x, target.sizeDelta.y);
            }

            float minY = -Screen.height / Canvas.scaleFactor + devConsoleSize.y;
            float maxX = Screen.width / Canvas.scaleFactor - devConsoleSize.x;

            float clampedX = Mathf.Clamp(target.anchoredPosition.x, 0f, maxX);
            float clampedY = Mathf.Clamp(target.anchoredPosition.y, minY, 0f);
            Vector2 newPosition = new Vector2(clampedX, clampedY);
            target.anchoredPosition = newPosition;
        }

        public void AlignTop() {
            SetTargetPosition(Vector2.zero);
        }

        public void AlignLeft() {
            SetTargetPosition(Vector2.zero);
        }

        public void SetTargetPosition(Vector2 position) {
            target.anchoredPosition = position;
            RestrictToScreen();
        }

        Vector2 GetDevConsoleSize() {
            Vector3[] v = new Vector3[4];
            target.GetWorldCorners(v);

            float height = Mathf.Abs(v[0].y - v[1].y);
            float width = v[2].x - v[1].x;

            return new Vector2(width, height);
        }
    }
}