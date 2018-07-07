using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

using Zenject;
using UniRx;

namespace Service.FileSystem {



    partial class FileSystemServiceImpl : FileSystemServiceBase
    {
        protected override void InitAPI() {
            var cmdGetScript = new Service.Scripting.Commands.GetMainScriptCommand();
            Publish(cmdGetScript);
            var api = new API(this);
            Kernel.Instance.Inject(api);
            cmdGetScript.result.Globals["FileSystem"] = api;
        }


        class API
        {
            FileSystemServiceImpl instance;

            public API( FileSystemServiceImpl instance) {
                this.instance = instance;
            }

            /* add here scripting for this service */
        }
    }

}
