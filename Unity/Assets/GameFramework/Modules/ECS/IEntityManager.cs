using System;
using System.Collections.Generic;

namespace ECS {
    public interface IEntityManager : IFBSerializable {
        void Initialize();

        void Tick(float deltaTime);

        bool AutoCallEntityModified { get; set; }

        UID CreateEntity();
        bool EntityExists(UID entity);
        void DestroyEntity(ref UID entity);
        void DestroyAllEntities();

        T AddComponent<T>(UID entity) where T : IComponent, new();
        IComponent AddComponent(UID entity, IComponent component);
        IComponent AddComponent(UID entity, Type componentType);

        IComponent CloneComponent(IComponent componentToClone);

        void RemoveComponent<T>(UID entity) where T : IComponent;
        void RemoveComponent(UID entity, IComponent component);

        T GetComponent<T>(UID entity) where T : IComponent;

        List<IComponent> GetAllComponents(UID entity);

        bool HasComponent<T>(UID entity) where T : IComponent;
        bool HasComponent(UID entity, IComponent component);
        bool HasComponent(UID entity, Type componentType);

        void SetupComponentID(IComponent component);

        void DisposeComponent(IComponent component);

        void RegisterSystem(ISystem system);
        void UnregisterSystem(ISystem system);

        void EntityModified(UID entity);
        void EntitiesModified(List<UID> entity);

        void ResetIDs();
    }
}