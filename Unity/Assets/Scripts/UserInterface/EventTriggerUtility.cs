using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UserInterface {
    public class EventUtility{

        /// <summary>
        /// Creates an event trigger entry using UnityActions.
        /// </summary>
        /// <param name="trigger">The event trigger we want to add an entry to.</param>
        /// <param name="type">The trigger type</param>
        /// <param name="method">The method that should get called when the event is fired.</param>
        public static void CreateEventTriggerEntry(EventTrigger trigger, EventTriggerType type, UnityAction<BaseEventData> method) {
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = type;
            entry.callback.AddListener(method);
            trigger.triggers.Add(entry);
        }

        /// <summary>
        /// Creates an event trigger entry using System.Action
        /// </summary>
        /// <typeparam name="T">The event data type we want to use</typeparam>
        /// <param name="trigger">The event trigger we want to add an entry to.</param>
        /// <param name="type">The trigger type</param>
        /// <param name="method">The method that should get called when the event is fired.</param>
        public static void CreateEventTriggerEntry<T>(EventTrigger trigger, EventTriggerType type, Action<T> method) where T: BaseEventData {
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = type;
            entry.callback.AddListener((eventData) => method((T)eventData));
            trigger.triggers.Add(entry);
        }

    }
}

