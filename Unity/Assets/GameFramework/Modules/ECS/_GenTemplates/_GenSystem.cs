
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

/*block:systemComponent*/        public /*name:componentName*/GenTemplateComponent/*endname*/ /*name:sysCompName*/templateComponent/*endname*/;
/*endblock:systemComponent*/
/*block:rip*/        public GenTemplateComponent2 comp2;/*endblock:rip*/
    }

    public class /*name:baseName*/GenTemplateSystemBase/*endname*/ : ECS.System</*name:systemComponentsName*/GenTemplateSystemComponents/*endname*/> {

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

        protected override void AfterBind() {
            base.AfterBind();

            //Observable.EveryUpdate().Subscribe(e => ProcessAll()).AddTo(this);
        }

        protected override void Process(/*name:systemComponentsName*/GenTemplateSystemComponents/*endname*/ components) {
            base.Process(components);
            // add the system-logic here
        }
    }
/*endblock:implPart*/
}