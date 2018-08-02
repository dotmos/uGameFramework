using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

using Zenject;
using UniRx;

namespace Service.LoggingService {



    partial class LoggingServiceImpl : LoggingServiceBase
    {
        protected override void InitAPI() {
            /*
            var cmdGetScript = new Service.Scripting.Commands.GetMainScriptCommand();
            Publish(cmdGetScript);
            var api = new API(this);
            Kernel.Instance.Inject(api);
            cmdGetScript.result.Globals["LoggingService"] = api; */
            ActivateDefaultScripting("LoggingService");
        }


        class API
        {
            LoggingServiceImpl instance;

            public API( LoggingServiceImpl instance) {
                this.instance = instance;
            }


            /* add here scripting for this service */
        }
    }

}
