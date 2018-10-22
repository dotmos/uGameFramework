namespace ECS {
    public interface ISystemComponents {
        UID Entity { get; set; }
        IEntityManager EntityManager { get; set; }
    }
}
