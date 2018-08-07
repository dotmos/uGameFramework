using System;
using System.Runtime.Serialization;

namespace ECS {
    public class Component : IComponent, IDisposable {
        /// <summary>
        /// The unique ID of this component. Will be set by EntityManager when component is first added to an entity or by manually calling EntityManager.SetComponentID
        /// </summary>
        public UID ID { get; set; }

        /// <summary>
        /// The entity this component is attached to
        /// </summary>
        public UID Entity { get; set; }

        private bool wasConstructed = false;
        public Component() {
            TryOnConstruct();
        }

        /// <summary>
        /// Hook to make sure that "OnConstruct" is called when component is deserialized
        /// </summary>
        /// <param name="context"></param>
        [OnDeserializing]
        void _Deserializing(StreamingContext context) {
            TryOnConstruct();
        }

        void TryOnConstruct() {
            if (!wasConstructed) OnConstruct();
        }
    
        //TODO: Implement an IObservable that triggers a system recheck for this entity when a component value is changed.

        /// <summary>
        /// Executed when constructor is called, OnDeserializingAttribute is fired or Bind() is called. Will not be executed again, if already executed in the past.
        /// </summary>
        protected virtual void OnConstruct() {
            wasConstructed = true;
        }

        public virtual void Dispose() {
        }
    }
}