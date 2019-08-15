using System.Collections.Generic;
using MoonSharp.Interpreter;

using Zenject;
using UniRx;
using FlatBuffers;
using System;

namespace Service.PerformanceTest {



    partial class PerformanceTestServiceImpl : PerformanceTestServiceBase
    {
        protected override void InitAPI() {
            var cmdGetScript = new Service.Scripting.Commands.GetMainScriptCommand();
            Publish(cmdGetScript);
            
            //For manual/custom scripting uncomment following lines and comment DefaultScripting
            
            //var api = new API(this);
            //Kernel.Instance.Inject(api);
            //cmdGetScript.result.Globals["PerformanceTest"] = api;
            ActivateDefaultScripting("PerformanceTest");
        }


        class API
        {
            PerformanceTestServiceImpl instance;

            public API( PerformanceTestServiceImpl instance) {
                this.instance = instance;
            }

            /* add here scripting for this service */
        }
    }

}
