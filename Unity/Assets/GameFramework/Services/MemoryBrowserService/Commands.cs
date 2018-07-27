
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
namespace Service.MemoryBrowserService{
    public partial class Commands : CommandsBase {
        IMemoryBrowserService _service;

        [Inject]
        void Initialize([Inject] IMemoryBrowserService service) {
            _service = service;
            
            this.OnEvent<IsSimpleTypeCommand>().Subscribe(e => IsSimpleTypeCommandHandler(e)).AddTo(this);

            this.OnEvent<CreateMemoryBrowserCommand>().Subscribe(e => CreateMemoryBrowserCommandHandler(e)).AddTo(this);

            this.OnEvent<GetBrowserCommand>().Subscribe(e => GetBrowserCommandHandler(e)).AddTo(this);

            this.OnEvent<rxGetAllBrowsersCommand>().Subscribe(e => rxGetAllBrowsersCommandHandler(e)).AddTo(this);

        }
        

        
        /// <summary>
        /// Is this obj a simple type? (int,float,bool,string). 
        /// </summary>
        
        public class IsSimpleTypeCommand  {
            public bool result;
                        public object obj;
            
            
        }

		protected void IsSimpleTypeCommandHandler  (IsSimpleTypeCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("IsSimpleTypeCommand");
#endif
        
            cmd.result = _service.IsSimpleType(cmd.obj);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("IsSimpleTypeCommand");
#endif
        }
        

        
        /// <summary>
        /// 
        /// </summary>
        
        public class CreateMemoryBrowserCommand  {
            public MemoryBrowser result;
                        public string id;
                        public object root;
            
            
        }

		protected void CreateMemoryBrowserCommandHandler  (CreateMemoryBrowserCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("CreateMemoryBrowserCommand");
#endif
        
            cmd.result = _service.CreateMemoryBrowser(cmd.id,cmd.root);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("CreateMemoryBrowserCommand");
#endif
        }
        

        
        /// <summary>
        /// Get Browser by name
        /// </summary>
        
        public class GetBrowserCommand  {
            public MemoryBrowser result;
                        public string id;
            
            
        }

		protected void GetBrowserCommandHandler  (GetBrowserCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("GetBrowserCommand");
#endif
        
            cmd.result = _service.GetBrowser(cmd.id);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("GetBrowserCommand");
#endif
        }
        

        
        /// <summary>
        /// Get Reactive Dictionary with a memory-browsers(key=name value=MemoryBrowser)
        /// </summary>
        
        public class rxGetAllBrowsersCommand  {
            public ReactiveDictionary<string, MemoryBrowser> result;
            
            
        }

		protected void rxGetAllBrowsersCommandHandler  (rxGetAllBrowsersCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("rxGetAllBrowsersCommand");
#endif
        
            cmd.result = _service.rxGetAllBrowsers();
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("rxGetAllBrowsersCommand");
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

