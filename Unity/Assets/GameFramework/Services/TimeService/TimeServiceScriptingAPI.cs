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
            ActivateDefaultScripting("TimeService");
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
