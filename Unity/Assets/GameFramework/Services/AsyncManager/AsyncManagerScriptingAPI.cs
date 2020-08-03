using System;

using Zenject;
using UniRx;

namespace Service.AsyncManager {



    partial class AsyncManagerImpl : AsyncManagerBase
    {
        protected override void InitAPI() {
            ActivateDefaultScripting("AsyncManager");
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
