
using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;
using UniRx;
/*name:using*//*endname*/

namespace Systems {
/*block:basePart*/

/// <summary>
/*name:comment*//*endname*/
/// </summary>


    public partial class /*name:systemComponentsName*/GenTemplateSystemComponents/*endname*/ : ECS.ISystemComponents {
        public UID Entity { get; set; }
        public IEntityManager EntityManager { get; set; }

/*block:systemComponent*/
/*block:comment*//// <summary>
/*name:comment*/ /*endname*/
/// </summary>
/*endblock:comment*/
        public /*name:componentName*/GenTemplateComponent/*endname*/ /*name:sysCompName*/templateComponent/*endname*/;
/*endblock:systemComponent*/
/*block:rip*/        public GenTemplateComponent2 comp2;/*endblock:rip*/
    }

    public abstract class /*name:baseName*/GenTemplateSystemBase/*endname*/ : ECS.System</*name:systemComponentsName*/GenTemplateSystemComponents/*endname*/> {

        protected override /*name:systemComponentsName*/GenTemplateSystemComponents/*endname*/ GetEntityComponents(/*name:systemComponentsName*/GenTemplateSystemComponents/*endname*/ components, UID entity) {
/*block:getComponent*/            components./*name:sysCompName*/templateComponent/*endname*/ = GetComponent</*name:componentName*/GenTemplateComponent/*endname*/>(entity);
/*endblock:getComponent*/
/*block:rip*/            components.comp2 = GetComponent<GenTemplateComponent2>(entity);/*endblock:rip*/
            return components;
        }

        protected override bool IsEntityValid(UID entity) {
            if (/*block:rip*/HasComponent<GenTemplateComponent>(entity) /*endblock:rip*/
                /*block:validComponent*//*name:OPERATION*/&&/*endname*/ HasComponent</*name:componentName*/GenTemplateComponent2/*endname*/>(entity) 
/*endblock:validComponent*/
             ) {
                return true;
            }
            else {
                return false;
            }
        }
    }
/*endblock:basePart*/
/*block:implPart*/

/// <summary>
/*name:comment*//*endname*/
/// </summary>
    public partial class /*name:implName*/GenTemplateSystem/*endname*/ : /*name:baseName*/GenTemplateSystemBase/*endname*/
    {

        protected override bool UseParallelSystemComponentsProcessing() {
            //Set to true if you want to process your components in parallel / use threads
            //If set to true, make sure that code in ProcessAtIndex(..) is threadsafe
            return false;
        }

        /// <summary>
        /// Sets an update rate for this system. 0 = every frame
        /// </summary>
        /// <returns></returns>
        protected override float SystemUpdateRate() {
            return 0;
        }

        /// <summary>
        /// Called for a systemComponent that got registered and valid
        /// </summary>
        /// <param name="newRegisteredComponents"></param>
        protected override void OnRegistered(List</*name:systemComponentsName*/GenTemplateSystemComponents/*endname*/> newRegisteredComponents) {
            base.OnRegistered(newRegisteredComponents);
        }

        /// <summary>
        /// Called for every valid entity that was updated or just registered with the system.
        /// </summary>
        /// <param name="components"></param>
        protected override void EntityUpdated(ref /*name:systemComponentsName*/ GenTemplateSystemComponents/*endname*/ components) {
            base.EntityUpdated(ref components);
        }

        /// <summary>
        /// Called for systemComponent that is destroyed or invalid
        /// </summary>
        /// <param name="unregisteredEntities"></param>
        protected override void OnUnregistered(List</*name:systemComponentsName*/GenTemplateSystemComponents/*endname*/> unregisteredEntities) {
            base.OnUnregistered(unregisteredEntities);
        }

        /// <summary>
        /// Called for every single valid element by base.ProcessAll(dt)
        /// </summary>
        /// <param name="index"></param>
        /// <param name="deltaTime"></param>
        /// <param name="workerID"></param>
        protected override void ProcessAtIndex(int index, float deltaTime, int workerID) {
            /*name:systemComponentsName*/ GenTemplateSystemComponents/*endname*/ components = componentsToProcess[index];

            // add the system-logic here
        }

        /// <summary>
        /// Entrypoint. Called once per updateRate-interval and triggers ProcessAtIndex via base.ProcessAll(dt)
        /// </summary>
        /// <param name="deltaTime"></param>
        protected override void ProcessAll(float deltaTime) {
            base.ProcessAll(deltaTime);
        }



    }
/*endblock:implPart*/
}