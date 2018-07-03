
using Service.Events;
using System.Collections.Generic;
using MoonSharp.Interpreter;

using System;
using Zenject;
using UniRx;
using System.Diagnostics;


///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 
namespace Service.GameStateService{
    public partial class Commands : CommandsBase {
        IGameStateService _service;

        [Inject]
        void Initialize([Inject] IGameStateService service) {
            _service = service;
            
            this.OnEvent<RegisterGameStateCommand>().Subscribe(e => RegisterGameStateCommandHandler(e)).AddTo(this);

            this.OnEvent<GetCurrentGameStateCommand>().Subscribe(e => GetCurrentGameStateCommandHandler(e)).AddTo(this);

            this.OnEvent<StartGameStateCommand>().Subscribe(e => StartGameStateCommandHandler(e)).AddTo(this);

        }
        

        
        /// <summary>
        /// Register gamestate with its name. Optionally you can pass an overriden GameState-Classtype of your own
        /// </summary>
        
        public class RegisterGameStateCommand {
            public GameState result;
                        public string name;
                        public GameState gamestate=null;
            
            
        }

		protected void RegisterGameStateCommandHandler(RegisterGameStateCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("RegisterGameStateCommand");
#endif
            
            cmd.result = _service.RegisterGameState(cmd.name,cmd.gamestate);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("RegisterGameStateCommand");
#endif
        }
        

        
        /// <summary>
        /// Get the current gamestate. Alternatively use "[Inject] GameState current;"
        /// </summary>
        
        public class GetCurrentGameStateCommand {
            public GameState result;
            
            
        }

		protected void GetCurrentGameStateCommandHandler(GetCurrentGameStateCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("GetCurrentGameStateCommand");
#endif
            
            cmd.result = _service.GetCurrentGameState();
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("GetCurrentGameStateCommand");
#endif
        }
        

        
        /// <summary>
        /// Start a new gamestate after stopping the current one (if present). Optionally pass a context in which you can e.g. set gamestate-flags
        /// </summary>
        
        public class StartGameStateCommand {
            public IObservable<bool> result;
                        public GameState gamestate;
                        public GSContext ctx=null;
            
            
        }

		protected void StartGameStateCommandHandler(StartGameStateCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("StartGameStateCommand");
#endif
            
            cmd.result = _service.StartGameState(cmd.gamestate,cmd.ctx);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("StartGameStateCommand");
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


