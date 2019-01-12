namespace ECS {
    public interface IComponent : System.IDisposable, IFBSerializable{
        UID ID { get; set; }
        UID Entity { get; set; }

        void CopyValues(IComponent otherComponent);
        IComponent Clone();
    }
}