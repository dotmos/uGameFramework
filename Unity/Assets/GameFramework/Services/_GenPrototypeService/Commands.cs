
/*name:using*/using System.Collections.Generic;/*endname*/
using System;
using Service.Events;
using Zenject;
using UniRx;
using System.Diagnostics;

/*block:commandFile*/
///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 
namespace /*name:namespace*/Service.GeneratorPrototype/*endname*/{
    public partial class Commands : CommandsBase {
        /*name:interfaceName*/IPrototypeService/*endname*/ _service;

        [Inject]
        void Initialize([Inject] /*name:interfaceName*/IPrototypeService/*endname*/ service) {
            _service = service;
            /*block:eventBinding*/
            this.OnEvent</*name:methodCommand*/DoPrototypeCommand/*endname*//*name:genericOnEvent*//*endname*/>().Subscribe(e => /*name:methodCommandHandler*/DoPrototypeCommandHandler/*endname*/(e)).AddTo(this);
/*endblock:eventBinding*/
        }
        /*block:command*/

        /*block:clazz*/
        /// <summary>
        /// /*name:documentation*//*endname*/
        /// </summary>
        
        public class /*name:methodCommand*/DoPrototypeCommand/*endname*/ /*name:genInput*//*endname*/ {
/*block:commandParameter*/            public /*name:type*/string/*endname*/ /*name:name*/settings/*endname*//*name:defaultValue*//*endname*/;
            /*endblock:commandParameter*/
            /*block:rip*/
            public int npcId;
            public string result; 
/*endblock:rip*/
        }
/*endblock:clazz*/
		protected void /*name:methodCommandHandler*/DoPrototypeCommandHandler/*endname*/ /*block:genericDefinition*//*name:genInput*//*endname*//*endblock:genericDefinition*/ (/*name:methodCommand*/DoPrototypeCommand/*endname*//*name:genInput*//*endname*/ cmd)/*block:genericRestriction*//*name:genericRestrictionInput*//*endname*//*endblock:genericRestriction*/ {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start(/*name|dq:methodCommand*/"DoPrototypeCommand"/*endname*/);
#endif
        /*block:result*/
            cmd.result = /*endblock:result*/_service./*name:methodName*/DoPrototype/*endname*//*name:genInput*//*endname*/(/*block:handlerParameter*//*name:comma*//*endname*/cmd./*name:name*/settings/*endname*//*endblock:handlerParameter*/);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop(/*name|dq:methodCommand*/"DoPrototypeCommand"/*endname*/);
#endif
        }
        /*endblock:command*/
    }


    public class CommandsInstaller : Installer<CommandsInstaller>{
        public override void InstallBindings()
        {
            Commands cmds = Container.Instantiate<Commands>();
            // commented out due to zenject update (26.06.18)
            //Container.BindAllInterfaces<Commands>().FromInstance(cmds);
        }
    }
}
///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 
/*endblock:commandFile*/
/*block:customCommandFile*/
namespace /*name:namespace*/Service.GeneratorPrototype/*endname*/{
    public partial class Commands : CommandsBase {
	// use [custom-command:True] on framework methods to enable custom-command for this method
        /*block:command*/
        public class CustomCommandBody {
            //public Vector2 pos = new Vector2(0, 100);
        }
        /*endblock:command*/
    }

}
/*endblock:customCommandFile*/
