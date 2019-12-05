using System;

using Zenject;
using UniRx;

namespace Service.AsyncManager {



    partial class AsyncManagerImpl : AsyncManagerBase
    {
        protected override void InitAPI() {
            Scripting.Commands.GetMainScriptCommand cmdGetScript = new Service.Scripting.Commands.GetMainScriptCommand();
            Publish(cmdGetScript);
            API api = new API(this);
            Kernel.Instance.Inject(api);
            cmdGetScript.result.Globals["AsyncManager"] = api;
        }


        class API
        {
            AsyncManagerImpl instance;

            public API( AsyncManagerImpl instance) {
                this.instance = instance;
            }

            /* add here scripting for this service */
        }
    }

}
