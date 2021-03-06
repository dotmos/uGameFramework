/*name:using*//*endname*/
using Zenject;
using UniRx;
using FlatBuffers;
using System;
using Service.Scripting;

namespace /*name:namespace*/Service.GeneratorPrototype/*endname*/ {
/*block:impl*/
    partial class /*name:implName*/PrototypeServiceImpl/*endname*/ : /*name:serviceName*/PrototypeService/*endname*/ {
/*block:property*/
/*name:propAsString*/public override int MaxSoundChannels{get;set;}/*endname*/
/*endblock:property*/

        protected override void AfterInitialize() {
			// this is called right after the Base-Classes Initialize-Method. _eventManager and disposableManager are set
        }

/*block:method*/
		/*name:docs*/ //docs come here /*endname*/
		public override /*name:methodHead*/string DoPrototype(string settings="")/*endname*/ {
/*block:return*/            return default(/*name:returnType*/string/*endname*/);
/*endblock:return*/
        }
/*endblock:method*/ 
    }
/*endblock:impl*/

/*block:scripting*/
    partial class /*name:implName*/PrototypeServiceImpl/*endname*/ : /*name:serviceName*/PrototypeService/*endname*/
    {
        protected override void InitAPI() {


            //For manual/custom scripting uncomment following lines and comment DefaultScripting
            //var scriptService = Kernel.Instance.Resolve<IScriptingService>();
            //var api = new API(this);
            //Kernel.Instance.Inject(api);
            //var mainscript = scriptService.GetMainScript();
            //mainscript.Globals[/*name|dq:scriptName*/"GS"/*endname*/] = api;
            ActivateDefaultScripting(/*name|dq:scriptName*/"GS"/*endname*/);
        }

        /*block:rip*/
        public override string Abstract_User_DoPrototype(string settings = "") {
            throw new NotImplementedException();
        }/*endblock:rip*/

        class API
        {
            /*name:implName*/PrototypeServiceImpl/*endname*/ instance;

            public API( /*name:implName*/PrototypeServiceImpl/*endname*/ instance) {
                this.instance = instance;
            }

            /* add here scripting for this service */
        }
    }
/*endblock:scripting*/
}
