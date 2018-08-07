
using Service.Events;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using Service.GameStateService;

using System;
using Zenject;
using UniRx;
using System.Diagnostics;


///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 
namespace Service.TimeService{
    public partial class Commands : CommandsBase {
        ITimeService _service;

        [Inject]
        void Initialize([Inject] ITimeService service) {
            _service = service;
            
            this.OnEvent<CreateGlobalTimerCommand>().Subscribe(e => CreateGlobalTimerCommandHandler(e)).AddTo(this);

            this.OnEvent<RemoveGlobalTimerCommand>().Subscribe(e => RemoveGlobalTimerCommandHandler(e)).AddTo(this);

        }
        

        
        /// <summary>
        /// Adds a timer in the global update-method and calls the callback n-times (or infinite till application end)
        /// </summary>
        
        public class CreateGlobalTimerCommand  {
            public TimerElement result;
                        public float interval;
                        public Action callback;
                        public int repeatTimes;
                        public string info="";
            
            
        }

		protected void CreateGlobalTimerCommandHandler  (CreateGlobalTimerCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("CreateGlobalTimerCommand");
#endif
        
            cmd.result = _service.CreateGlobalTimer(cmd.interval,cmd.callback,cmd.repeatTimes,cmd.info);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("CreateGlobalTimerCommand");
#endif
        }
        

        
        /// <summary>
        /// 
        /// </summary>
        
        public class RemoveGlobalTimerCommand  {
            public TimerElement timer;
            
            
        }

		protected void RemoveGlobalTimerCommandHandler  (RemoveGlobalTimerCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("RemoveGlobalTimerCommand");
#endif
        _service.RemoveGlobalTimer(cmd.timer);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("RemoveGlobalTimerCommand");
#endif
        }
        
    }


    public class CommandsInstaller : Installer<CommandsInstaller>{
        public override void InstallBindings()
        {
            Commands cmds = Container.Instantiate<Commands>();
            // commented out due to zenject update (26.06.18)
            //Container.BindAllInterfaces<Commands>().FromInstance(cmds);
        }
    }
}
///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 


