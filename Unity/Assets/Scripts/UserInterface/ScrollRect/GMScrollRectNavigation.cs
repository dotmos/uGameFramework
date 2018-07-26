using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UserInterface {
    [RequireComponent(typeof(GMScrollRect))]
    [AddComponentMenu(NamingHelper.ScrollRectNavigation.Name, 34)]
    public class GMScrollRectNavigation : MonoBehaviour {

        public Selectable scrollLeftButton;
        public Selectable scrollRightButton;
        /// <summary>
        /// The time in seconds the controller waits until activating continuous scroll when a button is held down.
        /// </summary>
        public float continuousScrollDelay = 0.25f;

        private GMScrollRect scrollRect;

        private float step = 0.25f;
        private float continuousScrollTimer;

        private int continuousScrollDirection;

        private void Awake() {
            scrollRect = GetComponent<GMScrollRect>();
            SetupScrollButton(scrollLeftButton, -1);
            SetupScrollButton(scrollRightButton, 1);
        }

        private void Update()
        {
            //Only do something if a continuousScroll direction was set by holding down a button
            if (continuousScrollDirection != 0) {
                if (continuousScrollTimer >= continuousScrollDelay) {
                    //Scroll
                    Scroll(continuousScrollDirection);
                } else {
                    //Increase timer
                    continuousScrollTimer += Time.deltaTime;
                }
            }
        }

        /// <summary>
        /// Sets up a selectable to be used as control element for scrolling.
        /// </summary>
        /// <param name="selectable">A selectable that can be clicked to scroll.</param>
        /// <param name="direction">The direction in which this selectable will scroll (-1 left, 1 right).</param>
        void SetupScrollButton(Selectable selectable, int direction)
        {
            EventTrigger eTrigger = selectable.GetComponent<EventTrigger>();
            if (eTrigger == null) eTrigger = selectable.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry pointerDown = new EventTrigger.Entry();
            pointerDown.eventID = EventTriggerType.PointerDown;
            pointerDown.callback.AddListener((eventData) => {
                //Do one scroll on click
                Scroll(direction);
                //Set continuous scroll direction
                continuousScrollDirection = direction;
            });

            EventTrigger.Entry pointerUp = new EventTrigger.Entry();
            pointerUp.eventID = EventTriggerType.PointerUp;
            pointerUp.callback.AddListener((eventData) => {
                //Reset continuous scroll direction
                continuousScrollDirection = 0;
                //Reset timer
                continuousScrollTimer = 0;
            });

            eTrigger.triggers.Add(pointerDown);
            eTrigger.triggers.Add(pointerUp);
        }

        void Scroll(int direction) {
            //Step is based on child count so it scales if there are dynamically spawned elements
            step = 1f / (float)scrollRect.content.transform.childCount;
            scrollRect.horizontalNormalizedPosition = Mathf.Clamp01(scrollRect.horizontalNormalizedPosition + step * direction);
        }
    }
}
