using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UserInterface {
    public class GMPositionTargetOnDeactivate : MonoBehaviour {
        public GMDraggableObject draggableObject;

        private RectTransform rectTransform;
        private Vector2 cachedSize;

        private void Awake() {
            rectTransform = GetComponent<RectTransform>();
        }

        private void OnDisable() {
            cachedSize = rectTransform.sizeDelta;
            draggableObject.SetTargetPosition(new Vector2(draggableObject.target.anchoredPosition.x + rectTransform.sizeDelta.x, draggableObject.target.anchoredPosition.y));
        }

        private void OnEnable() {
            draggableObject.SetTargetPosition(new Vector2(draggableObject.target.anchoredPosition.x - cachedSize.x, draggableObject.target.anchoredPosition.y));
        }
    }
}
