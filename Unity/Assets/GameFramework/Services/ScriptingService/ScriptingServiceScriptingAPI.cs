using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

using Zenject;
using UniRx;

namespace Service.Scripting {



    partial class ScriptingServiceImpl : ScriptingServiceBase
    {
        protected override void InitAPI() {
            var cmdGetScript = new Service.Scripting.Commands.GetMainScriptCommand();
            Publish(cmdGetScript);
            var api = new API(this);
            Kernel.Instance.Inject(api);
            cmdGetScript.result.Globals["Scripting"] = api;
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
