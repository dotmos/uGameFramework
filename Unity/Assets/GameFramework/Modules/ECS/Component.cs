using System;
using System.Runtime.Serialization;
using FlatBuffers;
using Service.Serializer;

namespace ECS {
    // Attribute to the a supported component-field(list/dictionary) to get an new instance. e.g. a new List BUT with the same values. This is not a deep copy
    [AttributeUsage(AttributeTargets.Field)]
    public class NewInstance : Attribute {
    }

    public abstract class Component : IComponent, IDisposable {
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

        public abstract void CopyValues(IComponent target);

        public abstract IComponent Clone();

        public virtual void Dispose() {
        }

        public int Serialize(FlatBufferBuilder builder) {
            return Serial.FBComponent.CreateFBComponent(builder, ID.Serialize(builder), Entity.Serialize(builder), wasConstructed).Value;
        }

        public void Deserialize(object incoming) {
            var data = (Serial.FBComponent)incoming;
            ID = FlatbufferSerializer.GetOrCreateDeserialize<UID>(data.Id);
            Entity = FlatbufferSerializer.GetOrCreateDeserialize<UID>(data.Entity);
        }

        public void Deserialize(ByteBuffer buf) {
            throw new NotImplementedException();
        }
    }
}