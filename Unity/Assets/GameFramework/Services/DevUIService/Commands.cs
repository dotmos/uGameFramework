
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
namespace Service.DevUIService{
    public partial class Commands : CommandsBase {
        IDevUIService _service;

        [Inject]
        void Initialize([Inject] IDevUIService service) {
            _service = service;
            
            this.OnEvent<GetRxViewsCommand>().Subscribe(e => GetRxViewsCommandHandler(e)).AddTo(this);

            this.OnEvent<AddViewCommand>().Subscribe(e => AddViewCommandHandler(e)).AddTo(this);

            this.OnEvent<GetViewCommand>().Subscribe(e => GetViewCommandHandler(e)).AddTo(this);

            this.OnEvent<ViewNameExistsCommand>().Subscribe(e => ViewNameExistsCommandHandler(e)).AddTo(this);

            this.OnEvent<RemoveViewCommand>().Subscribe(e => RemoveViewCommandHandler(e)).AddTo(this);

            this.OnEvent<LoadViewsCommand>().Subscribe(e => LoadViewsCommandHandler(e)).AddTo(this);

            this.OnEvent<SaveViewsCommand>().Subscribe(e => SaveViewsCommandHandler(e)).AddTo(this);

            this.OnEvent<WriteToScriptingConsoleCommand>().Subscribe(e => WriteToScriptingConsoleCommandHandler(e)).AddTo(this);

            this.OnEvent<OpenScriptingConsoleCommand>().Subscribe(e => OpenScriptingConsoleCommandHandler(e)).AddTo(this);

            this.OnEvent<CloseScriptingConsoleCommand>().Subscribe(e => CloseScriptingConsoleCommandHandler(e)).AddTo(this);

            this.OnEvent<ToggleScriptingConsoleCommand>().Subscribe(e => ToggleScriptingConsoleCommandHandler(e)).AddTo(this);

            this.OnEvent<IsScriptingConsoleVisibleCommand>().Subscribe(e => IsScriptingConsoleVisibleCommandHandler(e)).AddTo(this);

        }
        

        
        /// <summary>
        /// Get ReaciveDictionar of all views
        /// </summary>
        
        public class GetRxViewsCommand  {
            public ReactiveCollection<DevUIView> result;
            
            
        }

		protected void GetRxViewsCommandHandler  (GetRxViewsCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("GetRxViewsCommand");
#endif
        
            cmd.result = _service.GetRxViews();
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("GetRxViewsCommand");
#endif
        }
        

        
        /// <summary>
        /// Add/Create view with name
        /// </summary>
        
        public class AddViewCommand  {
            public DevUIView result;
                        public string viewName;
            
            
        }

		protected void AddViewCommandHandler  (AddViewCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("AddViewCommand");
#endif
        
            cmd.result = _service.AddView(cmd.viewName);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("AddViewCommand");
#endif
        }
        

        
        /// <summary>
        /// Get a view by name
        /// </summary>
        
        public class GetViewCommand  {
            public DevUIView result;
                        public string viewName;
            
            
        }

		protected void GetViewCommandHandler  (GetViewCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("GetViewCommand");
#endif
        
            cmd.result = _service.GetView(cmd.viewName);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("GetViewCommand");
#endif
        }
        

        
        /// <summary>
        /// Check if view already exists
        /// </summary>
        
        public class ViewNameExistsCommand  {
            public bool result;
                        public string viewName;
            
            
        }

		protected void ViewNameExistsCommandHandler  (ViewNameExistsCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("ViewNameExistsCommand");
#endif
        
            cmd.result = _service.ViewNameExists(cmd.viewName);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("ViewNameExistsCommand");
#endif
        }
        

        
        /// <summary>
        /// Remove View
        /// </summary>
        
        public class RemoveViewCommand  {
            public string viewName;
            
            
        }

		protected void RemoveViewCommandHandler  (RemoveViewCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("RemoveViewCommand");
#endif
        _service.RemoveView(cmd.viewName);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("RemoveViewCommand");
#endif
        }
        

        
        /// <summary>
        /// Load views from views-folder
        /// </summary>
        
        public class LoadViewsCommand  {

            
        }

		protected void LoadViewsCommandHandler  (LoadViewsCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("LoadViewsCommand");
#endif
        _service.LoadViews();
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("LoadViewsCommand");
#endif
        }
        

        
        /// <summary>
        /// Save views and its dynamically created elements
        /// </summary>
        
        public class SaveViewsCommand  {

            
        }

		protected void SaveViewsCommandHandler  (SaveViewsCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("SaveViewsCommand");
#endif
        _service.SaveViews();
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("SaveViewsCommand");
#endif
        }
        

        
        /// <summary>
        /// Output to console
        /// </summary>
        
        public class WriteToScriptingConsoleCommand  {
            public string text;
            
            
        }

		protected void WriteToScriptingConsoleCommandHandler  (WriteToScriptingConsoleCommand cmd) {
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
        

        
        /// <summary>
        /// Open the console
        /// </summary>
        
        public class OpenScriptingConsoleCommand  {

            
        }

		protected void OpenScriptingConsoleCommandHandler  (OpenScriptingConsoleCommand cmd) {
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
        /// Close the console
        /// </summary>
        
        public class CloseScriptingConsoleCommand  {

            
        }

		protected void CloseScriptingConsoleCommandHandler  (CloseScriptingConsoleCommand cmd) {
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
        /// Toggle the console visibility
        /// </summary>
        
        public class ToggleScriptingConsoleCommand  {

            
        }

		protected void ToggleScriptingConsoleCommandHandler  (ToggleScriptingConsoleCommand cmd) {
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
        /// Check if console is visible at the moment
        /// </summary>
        
        public class IsScriptingConsoleVisibleCommand  {
            public bool result;
            
            
        }

		protected void IsScriptingConsoleVisibleCommandHandler  (IsScriptingConsoleVisibleCommand cmd) {
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


