using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UserInterface {
    [AddComponentMenu(NamingHelper.ResizeHandler.Name, 34)]
    public class GMResizeHandler : MonoBehaviour {

        public List<GMResizableLayoutElement> targets = new List<GMResizableLayoutElement>();
        public HandlerType handlerType;

        private List<IResizeListener> resizeListeners = new List<IResizeListener>();

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

        void Awake() {
            eventTrigger = GetComponent<EventTrigger>();
            EventUtility.CreateEventTriggerEntry(eventTrigger, EventTriggerType.Drag, OnDrag);
            EventUtility.CreateEventTriggerEntry(eventTrigger, EventTriggerType.EndDrag, OnEndDrag);

            IEnumerable<IResizeListener> listeners = FindObjectsOfType<MonoBehaviour>().OfType<IResizeListener>();
            foreach (IResizeListener listener in listeners) {
                resizeListeners.Add(listener);
            }
        }

        void OnDrag(BaseEventData data) {
            PointerEventData ped = (PointerEventData)data;

            foreach (GMResizableLayoutElement target in targets) {
                target.ResizeLayoutElement(Mathf.Round(ped.delta.x * handlerScaleStrategy[handlerType].x), Mathf.Round(ped.delta.y * handlerScaleStrategy[handlerType].y));
            }
        }

        void OnEndDrag(BaseEventData data) {
            foreach(IResizeListener listener in resizeListeners) {
                listener.OnResize();
            }
        }
    }
}
