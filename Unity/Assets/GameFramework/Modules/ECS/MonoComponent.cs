using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace ECS {
    /// <summary>
    /// Helper class for setting up data / adding components through Unity's inspector
    /// </summary>
    /// <typeparam name="TComponent"></typeparam>
    public class MonoComponent<TComponent> : MonoBehaviour where TComponent : IComponent, new(){
        //public UID Entity {get; set;}
        private MonoEntity monoEntity;

        public TComponent component;
        
        protected virtual void Awake() {
            monoEntity = GetComponent<MonoEntity>();
            if(monoEntity == null) {
                monoEntity = gameObject.AddComponent<MonoEntity>();
            }
            if (component == null) component = new TComponent();
            monoEntity.GetEntityManager().AddComponent(monoEntity.Entity, component);
        }   
        
        public IEntityManager EntityManager() {
            return monoEntity.GetEntityManager();
        }

        public void ComponentModified() {
            EntityManager().EntityModified(monoEntity.Entity);
        }
    }
}
