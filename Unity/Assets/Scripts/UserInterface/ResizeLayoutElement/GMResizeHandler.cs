using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UserInterface {
    [AddComponentMenu(NamingHelper.ResizeHandler.Name, 34)]
    public class GMResizeHandler : MonoBehaviour {

        public GMResizableLayoutElement target;
        public HandlerType handlerType;

        public enum HandlerType {
            BottomRight,
        }

        /// <summary>
        /// Extend this if we need more handlers! Keep in mind, that scaling from BottomLeft will need a moveStrategy to counter position the main window.
        /// </summary>
        private Dictionary<HandlerType, Vector2> handlerScaleStrategy = new Dictionary<HandlerType, Vector2>() {
            { HandlerType.BottomRight, new Vector2(1, -1) },
        };

        private EventTrigger eventTrigger;

        void Start() {
            eventTrigger = GetComponent<EventTrigger>();
            EventUtility.CreateEventTriggerEntry(eventTrigger, EventTriggerType.Drag, OnDrag);
        }

        void OnDrag(BaseEventData data) {
            PointerEventData ped = (PointerEventData)data;

            target.ResizeLayoutElement(Mathf.Round(ped.delta.x * handlerScaleStrategy[handlerType].x), Mathf.Round(ped.delta.y * handlerScaleStrategy[handlerType].y));
        }
    }
}
