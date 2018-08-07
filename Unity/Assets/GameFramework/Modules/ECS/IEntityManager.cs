using System.Collections.Generic;

namespace ECS {
    public interface IEntityManager {
        UID CreateEntity();
        bool EntityExists(UID entity);
        void DestroyEntity(ref UID entity);

        T AddComponent<T>(UID entity) where T : IComponent, new();
        IComponent AddComponent(UID entity, IComponent component);

        void RemoveComponent<T>(UID entity) where T : IComponent;
        void RemoveComponent(UID entity, IComponent component);

        T GetComponent<T>(UID entity) where T : IComponent;

        List<IComponent> GetAllComponents(UID entity);

        bool HasComponent<T>(UID entity) where T : IComponent;
        bool HasComponent(UID entity, IComponent component);

        void SetupComponentID(IComponent component);

        void DisposeComponent(ref IComponent component);

        void RegisterSystem(ISystem system);
        void UnregisterSystem(ISystem system);

        void EntityModified(UID entity);
    }
}