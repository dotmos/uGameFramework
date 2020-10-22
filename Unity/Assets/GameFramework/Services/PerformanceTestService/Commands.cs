
using System.Collections.Generic;
using MoonSharp.Interpreter;
using System.Collections.Concurrent;

using System;
using Service.Events;
using Zenject;
using UniRx;
using System.Diagnostics;


///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 
namespace Service.PerformanceTest{
    public partial class Commands : CommandsBase {
        IPerformanceTestService _service;

        [Inject]
        void Initialize([Inject] IPerformanceTestService service) {
//            _service = service;
//            
//            this.OnEvent<StartWatchCommand>().Subscribe(e => StartWatchCommandHandler(e)).AddTo(this);
//
//            this.OnEvent<StopWatchCommand>().Subscribe(e => StopWatchCommandHandler(e)).AddTo(this);
//
//            this.OnEvent<PerfTestOutputToConsoleCommand>().Subscribe(e => PerfTestOutputToConsoleCommandHandler(e)).AddTo(this);
//
//            this.OnEvent<PerfTestOutputAsStringCommand>().Subscribe(e => PerfTestOutputAsStringCommandHandler(e)).AddTo(this);
//
//            this.OnEvent<ClearCommand>().Subscribe(e => ClearCommandHandler(e)).AddTo(this);
//
//            this.OnEvent<AddInstanceCommand>().Subscribe(e => AddInstanceCommandHandler(e)).AddTo(this);
//
//            this.OnEvent<RemoveInstanceCommand>().Subscribe(e => RemoveInstanceCommandHandler(e)).AddTo(this);
//
//            this.OnEvent<GetInstanceViewCommand>().Subscribe(e => GetInstanceViewCommandHandler(e)).AddTo(this);
//
//            this.OnEvent<GetInstanceCountCommand>().Subscribe(e => GetInstanceCountCommandHandler(e)).AddTo(this);
//
//            this.OnEvent<OutputInstanceViewToLogCommand>().Subscribe(e => OutputInstanceViewToLogCommandHandler(e)).AddTo(this);
//
        }
        

        
        /// <summary>
        /// 
        /// </summary>
        
//        public class StartWatchCommand  {
//            public string t;
//            
//            
//        }

//		protected void StartWatchCommandHandler  (StartWatchCommand cmd) {
//#if PERFORMANCE_TEST
//            var ptest=Service.Performance.PerformanceTest.Get();
//            ptest.Start("StartWatchCommand");
//#endif
//        _service.StartWatch(cmd.t);
//#if PERFORMANCE_TEST
//            // now stop the watches
//            ptest.Stop("StartWatchCommand");
//#endif
//        }
        

        
        /// <summary>
        /// 
        /// </summary>
        
//        public class StopWatchCommand  {
//            public string t;
//            
//            
//        }

//		protected void StopWatchCommandHandler  (StopWatchCommand cmd) {
//#if PERFORMANCE_TEST
//            var ptest=Service.Performance.PerformanceTest.Get();
//            ptest.Start("StopWatchCommand");
//#endif
//        _service.StopWatch(cmd.t);
//#if PERFORMANCE_TEST
//            // now stop the watches
//            ptest.Stop("StopWatchCommand");
//#endif
//        }
        

        
        /// <summary>
        /// 
        /// </summary>
        
//        public class PerfTestOutputToConsoleCommand  {
//
//            
//        }

//		protected void PerfTestOutputToConsoleCommandHandler  (PerfTestOutputToConsoleCommand cmd) {
//#if PERFORMANCE_TEST
//            var ptest=Service.Performance.PerformanceTest.Get();
//            ptest.Start("PerfTestOutputToConsoleCommand");
//#endif
//        _service.PerfTestOutputToConsole();
//#if PERFORMANCE_TEST
//            // now stop the watches
//            ptest.Stop("PerfTestOutputToConsoleCommand");
//#endif
//        }
        

        
        /// <summary>
        /// 
        /// </summary>
        
//        public class PerfTestOutputAsStringCommand  {
//            public string result;
//            
//            
//        }

//		protected void PerfTestOutputAsStringCommandHandler  (PerfTestOutputAsStringCommand cmd) {
//#if PERFORMANCE_TEST
//            var ptest=Service.Performance.PerformanceTest.Get();
//            ptest.Start("PerfTestOutputAsStringCommand");
//#endif
//        
//            cmd.result = _service.PerfTestOutputAsString();
//#if PERFORMANCE_TEST
//            // now stop the watches
//            ptest.Stop("PerfTestOutputAsStringCommand");
//#endif
//        }
        

        
        /// <summary>
        /// 
        /// </summary>
        
//        public class ClearCommand  {
//
//            
//        }

//		protected void ClearCommandHandler  (ClearCommand cmd) {
//#if PERFORMANCE_TEST
//            var ptest=Service.Performance.PerformanceTest.Get();
//            ptest.Start("ClearCommand");
//#endif
//        _service.Clear();
//#if PERFORMANCE_TEST
//            // now stop the watches
//            ptest.Stop("ClearCommand");
//#endif
//        }
        

        
        /// <summary>
        /// Add instance to 'leak-system' (must be removed in destructor)
        /// </summary>
        
//        public class AddInstanceCommand  {
//            public object o;
//            
//            
//        }

//		protected void AddInstanceCommandHandler  (AddInstanceCommand cmd) {
//#if PERFORMANCE_TEST
//            var ptest=Service.Performance.PerformanceTest.Get();
//            ptest.Start("AddInstanceCommand");
//#endif
//        _service.AddInstance(cmd.o);
//#if PERFORMANCE_TEST
//            // now stop the watches
//            ptest.Stop("AddInstanceCommand");
//#endif
//        }
        

        
        /// <summary>
        /// Remove instance from 'leak-system'
        /// </summary>
        
//        public class RemoveInstanceCommand  {
//            public object o;
//            
//            
//        }

//		protected void RemoveInstanceCommandHandler  (RemoveInstanceCommand cmd) {
//#if PERFORMANCE_TEST
//            var ptest=Service.Performance.PerformanceTest.Get();
//            ptest.Start("RemoveInstanceCommand");
//#endif
//        _service.RemoveInstance(cmd.o);
//#if PERFORMANCE_TEST
//            // now stop the watches
//            ptest.Stop("RemoveInstanceCommand");
//#endif
//        }
        

        
        /// <summary>
        /// Get the current amount of captured instance-counts
        /// </summary>
        
//        public class GetInstanceViewCommand  {
//            public ConcurrentDictionary<System.Type,int> result;
//            
//            
//        }

//		protected void GetInstanceViewCommandHandler  (GetInstanceViewCommand cmd) {
//#if PERFORMANCE_TEST
//            var ptest=Service.Performance.PerformanceTest.Get();
//            ptest.Start("GetInstanceViewCommand");
//#endif
//        
//            cmd.result = _service.GetInstanceView();
//#if PERFORMANCE_TEST
//            // now stop the watches
//            ptest.Stop("GetInstanceViewCommand");
//#endif
//        }
        

        
        /// <summary>
        /// Get the instance-count for a specific type
        /// </summary>
        
//        public class GetInstanceCountCommand  {
//            public int result;
//                        public System.Type instanceType;
//            
//            
//        }

//		protected void GetInstanceCountCommandHandler  (GetInstanceCountCommand cmd) {
//#if PERFORMANCE_TEST
//            var ptest=Service.Performance.PerformanceTest.Get();
//            ptest.Start("GetInstanceCountCommand");
//#endif
//        
//            cmd.result = _service.GetInstanceCount(cmd.instanceType);
//#if PERFORMANCE_TEST
//            // now stop the watches
//            ptest.Stop("GetInstanceCountCommand");
//#endif
//        }
        

        
        /// <summary>
        /// Output instance-view to log
        /// </summary>
        
//        public class OutputInstanceViewToLogCommand  {
//            public bool gccollect=true;
//            
//            
//        }

//		protected void OutputInstanceViewToLogCommandHandler  (OutputInstanceViewToLogCommand cmd) {
//#if PERFORMANCE_TEST
//            var ptest=Service.Performance.PerformanceTest.Get();
//            ptest.Start("OutputInstanceViewToLogCommand");
//#endif
//        _service.OutputInstanceViewToLog(cmd.gccollect);
//#if PERFORMANCE_TEST
//            // now stop the watches
//            ptest.Stop("OutputInstanceViewToLogCommand");
//#endif
//        }
        
    }


    public class CommandsInstaller : Installer<CommandsInstaller>{
        public override void InstallBindings()
        {
//            Commands cmds = Container.Instantiate<Commands>();
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


