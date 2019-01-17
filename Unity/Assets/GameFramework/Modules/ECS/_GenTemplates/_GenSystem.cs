
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

        protected override void ProcessAtIndex(int index, float deltaTime) {
            /*name:systemComponentsName*/ GenTemplateSystemComponents/*endname*/ components = componentsToProcess[index];

            // add the system-logic here
        }
    }
/*endblock:implPart*/
}