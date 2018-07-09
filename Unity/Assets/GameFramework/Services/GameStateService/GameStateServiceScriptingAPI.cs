using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

using Zenject;
using UniRx;

namespace Service.GameStateService {



    partial class GameStateServiceImpl : GameStateServiceBase
    {
        protected override void InitAPI() {
            var cmdGetScript = new Service.Scripting.Commands.GetMainScriptCommand();
            Publish(cmdGetScript);
            var api = new API(this);
            Kernel.Instance.Inject(api);
            cmdGetScript.result.Globals["GameStateService"] = api;
        }


        class API
        {
            GameStateServiceImpl instance;

            public API( GameStateServiceImpl instance) {
                this.instance = instance;
            }

            /* add here scripting for this service */
        }
    }

}
