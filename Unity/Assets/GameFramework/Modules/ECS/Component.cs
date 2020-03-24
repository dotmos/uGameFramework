using System;
using System.Runtime.Serialization;
using FlatBuffers;
using Service.Serializer;

namespace ECS {
    // Attribute to the a supported component-field(list/dictionary) to get an new instance. e.g. a new List BUT with the same values. This is not a deep copy
    [AttributeUsage(AttributeTargets.Field)]
    public class NewInstance : Attribute {
    }

    public abstract class Component : DefaultSerializable2, IComponent, IDisposable, IFBSerializable {
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

        public abstract void CopyValues(IComponent target, bool initFromPrefab=false);

        public abstract IComponent Clone(bool cloneFromPrefab=false);

        public virtual void Dispose() {
        }

        public virtual int Serialize(FlatBufferBuilder builder) {
            return Serial.FBComponent.CreateFBComponent(builder, new Offset<Serial.FBUID>(ID.Serialize(builder)), new Offset<Serial.FBUID>(Entity.Serialize(builder)), wasConstructed).Value;
        }

        public virtual void Deserialize(object incoming) {
            Serial.FBComponent data = (Serial.FBComponent)incoming;
            ID = FlatBufferSerializer.GetOrCreateDeserialize<UID>(data.Id);
            Entity = FlatBufferSerializer.GetOrCreateDeserialize<UID>(data.Entity);
        }

        public virtual void Deserialize(ByteBuffer buf) {
            throw new NotImplementedException();
        }
    }
}