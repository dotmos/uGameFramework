using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

using Zenject;
using UniRx;

namespace Service.DevUIService {



    partial class DevUIServiceImpl : DevUIServiceBase
    {
        protected override void InitAPI() {
            /*var cmdGetScript = new Service.Scripting.Commands.GetMainScriptCommand();
            Publish(cmdGetScript);
            var api = new API(this);
            Kernel.Instance.Inject(api);
            cmdGetScript.result.Globals["DevUIService"] = api;*/

            ActivateDefaultScripting("DevUIService");
        }


        class API
        {
            DevUIServiceImpl instance;

            public API( DevUIServiceImpl instance) {
                this.instance = instance;
            }

            /* add here scripting for this service */
        }
    }

}
