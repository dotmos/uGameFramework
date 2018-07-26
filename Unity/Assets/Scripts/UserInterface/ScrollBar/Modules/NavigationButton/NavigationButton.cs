using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UserInterface.Scrollbar {
    /// <summary>
    /// Class to manage the logic of the button based navigation of a scroll rect.
    /// </summary>
    internal class NavigationButton {
        // Public properties
        public enum Direction { Positive = 1, Negative = -1 }
        public Direction direction = Direction.Positive;
        public bool isPointerDown = false;
        public delegate void OnMove(float delta);
        public float step;

        // Private properties
        private GameObject go;
        private OnMove onMove;
        private int directionValue;
        public float continuousScrollTimer = 0;
        private float MovementDelta { get { return step * directionValue; } }

        public NavigationButton(GameObject go, Direction direction, float step, OnMove onMove) {
            this.go = go;
            this.direction = direction;
            this.directionValue = (int)direction;
            this.step = step;
            this.onMove = onMove;

            // prevent that initialize is called when we are only in edit mode
            // this technique has the benefit of preventing any if condition check (Application.isPlaying) when in a build
            bool shouldRun = true;
#if UNITY_EDITOR
            shouldRun = Application.isPlaying ? shouldRun : false;
#endif
            if (shouldRun) {
                Initialize();
            }
        }

        private void Initialize() {
            EventTrigger trigger = go.GetComponent<EventTrigger>();
            // Initialize our event triggers
            EventUtility.CreateEventTriggerEntry<PointerEventData>(trigger, EventTriggerType.PointerDown, OnPointerDown);
            EventUtility.CreateEventTriggerEntry<PointerEventData>(trigger, EventTriggerType.PointerUp, Reset);
            EventUtility.CreateEventTriggerEntry<PointerEventData>(trigger, EventTriggerType.PointerExit, Reset);
        }

        /// <summary>
        /// Called when the pointing device is released.
        /// Resets the isPointerDown state.
        /// </summary>
        /// <param name="data">info about the current pointer event</param>
        private void Reset(PointerEventData data) {
            isPointerDown = false;
            continuousScrollTimer = 0;
        }

        /// <summary>
        /// Called when the pointing device is held down.
        /// Sets the isPointerDown state.
        /// </summary>
        /// <param name="data">info about the pointer event</param>
        private void OnPointerDown(PointerEventData data) {
            isPointerDown = true;
            Move();
        }

        /// <summary>
        /// Our Move function.
        /// This will call the given delegate function.
        /// </summary>
        public void Move() {
            onMove(MovementDelta);
        }
    }
}
