using Service.Serializer;
using System;
using System.Collections.Generic;

namespace ECS {
    public interface IEntityManager : IFBSerializable,IFBSerializable2, IDisposable {
        void Initialize();

        void Tick(float deltaTime,float unscaledTime,float systemScaled);

        bool AutoCallEntityModified { get; set; }

        UID CreateEntity();
        bool EntityExists(UID entity);
        void DestroyEntity(ref UID entity);

        T AddComponent<T>(UID entity) where T : IComponent, new();
        IComponent AddComponent(UID entity, IComponent component);
        IComponent AddComponent(UID entity, Type componentType);
        
        IComponent SetComponent(UID entity, IComponent component);
        IComponent SetComponent<T>(UID entity, T component) where T : IComponent;
        IComponent CloneComponent(IComponent componentToClone);

        void RemoveComponent<T>(UID entity) where T : IComponent;
        void RemoveComponent(UID entity, IComponent component);

        T GetComponent<T>(UID entity) where T : IComponent;
        IComponent GetComponent(UID entity,Type componentType);

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

        int EntityCount();

        /// <summary>
        /// Tries to find an entity for the supplied ID. This funtion is VERY slow!
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        UID? GetEntityForID_SLOW(int id);

        void Clear();

#if ECS_PROFILING
        void ShowLog(bool showOnDevUIConsole = false);
        void ResetLog();
#endif
    }
}