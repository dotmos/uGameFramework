namespace ECS {
    public interface IComponent : System.IDisposable{
        UID ID { get; set; }
        UID Entity { get; set; }

        void CopyValues(IComponent otherComponent);
    }
}