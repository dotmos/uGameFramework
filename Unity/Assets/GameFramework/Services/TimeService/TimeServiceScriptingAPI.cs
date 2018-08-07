using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using Service.GameStateService;

using Zenject;
using UniRx;

namespace Service.TimeService {



    partial class TimeServiceImpl : TimeServiceBase
    {
        protected override void InitAPI() {
            var cmdGetScript = new Service.Scripting.Commands.GetMainScriptCommand();
            Publish(cmdGetScript);
            var api = new API(this);
            Kernel.Instance.Inject(api);
            cmdGetScript.result.Globals["TimeService"] = api;
        }


        class API
        {
            TimeServiceImpl instance;

            public API( TimeServiceImpl instance) {
                this.instance = instance;
            }

            /* add here scripting for this service */
        }
    }

}
