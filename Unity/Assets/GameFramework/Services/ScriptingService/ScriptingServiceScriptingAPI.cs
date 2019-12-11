using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

using Zenject;
using UniRx;

namespace Service.Scripting {



    partial class ScriptingServiceImpl : ScriptingServiceBase
    {
        protected override void InitAPI() {
            Commands.GetMainScriptCommand cmdGetScript = new Service.Scripting.Commands.GetMainScriptCommand();
            Publish(cmdGetScript);
            API api = new API(this);
            Kernel.Instance.Inject(api);
            Script script = cmdGetScript.result;

            string scriptCode = @"
                print('lua-lib start')
                uID = {}

                _luacallbacks = {}

                function registerCallback(cb)
                  table.insert(_luacallbacks,cb)
                end

		        function __callback(a,b,c)
                    for i, cb in ipairs(_luacallbacks) do
                        cb(a,b,c)
                    end
                end

                function __defaultCallback(a,b,c)
                    print('callback: '..(a or ''))
                end

                registerCallback(__defaultCallback)

                print('lua-lib initialized')
	        ";

            script.DoString(scriptCode);

            //script.Globals["Scripting"] = api;
            ActivateDefaultScripting("Scripting");


        } 


        class API
        {
            ScriptingServiceImpl instance;

            public API( ScriptingServiceImpl instance) {
                this.instance = instance;
            }

            /* add here scripting for this service */
        }
    }

}
