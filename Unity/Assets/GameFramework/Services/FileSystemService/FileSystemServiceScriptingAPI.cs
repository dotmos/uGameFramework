using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

using Zenject;
using UniRx;
using System.Linq;

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

            [Inject]
            Service.DevUIService.IDevUIService devUIService;

            public API( FileSystemServiceImpl instance) {
                this.instance = instance;
            }

            /* add here scripting for this service */
            public const string DEFAULT = "TOM";

            public void OutputFolders() {
                devUIService.WriteToScriptingConsole("FileSystem-Folders:");
                foreach (var fsType in Enum.GetValues(typeof(FSDomain)).Cast<FSDomain>()) {
                    devUIService.WriteToScriptingConsole(fsType.ToString() + " => " + instance.GetPath(fsType));
                }
            }

        }
    }

}
