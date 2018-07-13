
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
namespace Service.Scripting{
    public partial class Commands : CommandsBase {
        IScriptingService _service;

        [Inject]
        void Initialize([Inject] IScriptingService service) {
            _service = service;
            
            this.OnEvent<OpenScriptingConsoleCommand>().Subscribe(e => OpenScriptingConsoleCommandHandler(e)).AddTo(this);

            this.OnEvent<CloseScriptingConsoleCommand>().Subscribe(e => CloseScriptingConsoleCommandHandler(e)).AddTo(this);

            this.OnEvent<ToggleScriptingConsoleCommand>().Subscribe(e => ToggleScriptingConsoleCommandHandler(e)).AddTo(this);

            this.OnEvent<GetMainScriptCommand>().Subscribe(e => GetMainScriptCommandHandler(e)).AddTo(this);

            this.OnEvent<IsScriptingConsoleVisibleCommand>().Subscribe(e => IsScriptingConsoleVisibleCommandHandler(e)).AddTo(this);

            this.OnEvent<ExecuteStringOnMainScriptCommand>().Subscribe(e => ExecuteStringOnMainScriptCommandHandler(e)).AddTo(this);

            this.OnEvent<ExecuteFileToMainScriptCommand>().Subscribe(e => ExecuteFileToMainScriptCommandHandler(e)).AddTo(this);

            this.OnEvent<AutocompleteProposalsCommand>().Subscribe(e => AutocompleteProposalsCommandHandler(e)).AddTo(this);

            this.OnEvent<WriteToScriptingConsoleCommand>().Subscribe(e => WriteToScriptingConsoleCommandHandler(e)).AddTo(this);

        }
        

        
        /// <summary>
        /// 
        /// </summary>
        
        public class OpenScriptingConsoleCommand {

            
        }

		protected void OpenScriptingConsoleCommandHandler(OpenScriptingConsoleCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("OpenScriptingConsoleCommand");
#endif
            _service.OpenScriptingConsole();
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("OpenScriptingConsoleCommand");
#endif
        }
        

        
        /// <summary>
        /// 
        /// </summary>
        
        public class CloseScriptingConsoleCommand {

            
        }

		protected void CloseScriptingConsoleCommandHandler(CloseScriptingConsoleCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("CloseScriptingConsoleCommand");
#endif
            _service.CloseScriptingConsole();
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("CloseScriptingConsoleCommand");
#endif
        }
        

        
        /// <summary>
        /// 
        /// </summary>
        
        public class ToggleScriptingConsoleCommand {

            
        }

		protected void ToggleScriptingConsoleCommandHandler(ToggleScriptingConsoleCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("ToggleScriptingConsoleCommand");
#endif
            _service.ToggleScriptingConsole();
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("ToggleScriptingConsoleCommand");
#endif
        }
        

        
        /// <summary>
        /// 
        /// </summary>
        
        public class GetMainScriptCommand {
            public Script result;
            
            
        }

		protected void GetMainScriptCommandHandler(GetMainScriptCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("GetMainScriptCommand");
#endif
            
            cmd.result = _service.GetMainScript();
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("GetMainScriptCommand");
#endif
        }
        

        
        /// <summary>
        /// 
        /// </summary>
        
        public class IsScriptingConsoleVisibleCommand {
            public bool result;
            
            
        }

		protected void IsScriptingConsoleVisibleCommandHandler(IsScriptingConsoleVisibleCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("IsScriptingConsoleVisibleCommand");
#endif
            
            cmd.result = _service.IsScriptingConsoleVisible();
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("IsScriptingConsoleVisibleCommand");
#endif
        }
        

        
        /// <summary>
        /// Execute a string into the default-lua-context
        /// </summary>
        
        public class ExecuteStringOnMainScriptCommand {
            public string result;
                        public string luaCode;
            
            
        }

		protected void ExecuteStringOnMainScriptCommandHandler(ExecuteStringOnMainScriptCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("ExecuteStringOnMainScriptCommand");
#endif
            
            cmd.result = _service.ExecuteStringOnMainScript(cmd.luaCode);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("ExecuteStringOnMainScriptCommand");
#endif
        }
        

        
        /// <summary>
        /// Load a script into the default lua-context
        /// </summary>
        
        public class ExecuteFileToMainScriptCommand {
            public string result;
                        public string fileName;
            
            
        }

		protected void ExecuteFileToMainScriptCommandHandler(ExecuteFileToMainScriptCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("ExecuteFileToMainScriptCommand");
#endif
            
            cmd.result = _service.ExecuteFileToMainScript(cmd.fileName);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("ExecuteFileToMainScriptCommand");
#endif
        }
        

        
        /// <summary>
        /// Generates a list of possible proposals
        /// </summary>
        
        public class AutocompleteProposalsCommand {
            public Proposal result;
                        public string currentInput;
                        public int cursorPos;
            
            
        }

		protected void AutocompleteProposalsCommandHandler(AutocompleteProposalsCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("AutocompleteProposalsCommand");
#endif
            
            cmd.result = _service.AutocompleteProposals(cmd.currentInput,cmd.cursorPos);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("AutocompleteProposalsCommand");
#endif
        }
        

        
        /// <summary>
        /// Output to console
        /// </summary>
        
        public class WriteToScriptingConsoleCommand {
            public string text;
            
            
        }

		protected void WriteToScriptingConsoleCommandHandler(WriteToScriptingConsoleCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("WriteToScriptingConsoleCommand");
#endif
            _service.WriteToScriptingConsole(cmd.text);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("WriteToScriptingConsoleCommand");
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


