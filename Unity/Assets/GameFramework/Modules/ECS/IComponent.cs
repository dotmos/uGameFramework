using Service.Serializer;

namespace ECS {
    public interface IComponent : System.IDisposable, IFBSerializable, IFBSerializable2, IFBSerializeAsTypedObject {
        UID ID { get; set; }
        UID Entity { get; set; }

        void CopyValues(IComponent otherComponent, bool initFromPrefab = false);
        IComponent Clone(bool cloneFromPrefab=false);
    }
}