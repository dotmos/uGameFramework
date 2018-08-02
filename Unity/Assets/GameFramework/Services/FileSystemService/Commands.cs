
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
namespace Service.FileSystem{
    public partial class Commands : CommandsBase {
        IFileSystemService _service;

        [Inject]
        void Initialize([Inject] IFileSystemService service) {
            _service = service;
            
            this.OnEvent<GetPathCommand>().Subscribe(e => GetPathCommandHandler(e)).AddTo(this);

            this.OnEvent<WriteStringToFileCommand>().Subscribe(e => WriteStringToFileCommandHandler(e)).AddTo(this);

            this.OnEvent<WriteStringToFileAtDomainCommand>().Subscribe(e => WriteStringToFileAtDomainCommandHandler(e)).AddTo(this);

            this.OnEvent<LoadFileAsStringCommand>().Subscribe(e => LoadFileAsStringCommandHandler(e)).AddTo(this);

            this.OnEvent<LoadFileAsStringAtDomainCommand>().Subscribe(e => LoadFileAsStringAtDomainCommandHandler(e)).AddTo(this);

        }
        

        
        /// <summary>
        /// Get path as string for given domain
        /// </summary>
        
        public class GetPathCommand  {
            public string result;
                        public FSDomain domain;
            
            
        }

		protected void GetPathCommandHandler  (GetPathCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("GetPathCommand");
#endif
        
            cmd.result = _service.GetPath(cmd.domain);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("GetPathCommand");
#endif
        }
        

        
        /// <summary>
        /// Write string to file
        /// </summary>
        
        public class WriteStringToFileCommand  {
            public bool result;
                        public string pathToFile;
                        public string data;
            
            
        }

		protected void WriteStringToFileCommandHandler  (WriteStringToFileCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("WriteStringToFileCommand");
#endif
        
            cmd.result = _service.WriteStringToFile(cmd.pathToFile,cmd.data);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("WriteStringToFileCommand");
#endif
        }
        

        
        /// <summary>
        /// Write string to file at domain
        /// </summary>
        
        public class WriteStringToFileAtDomainCommand  {
            public bool result;
                        public FSDomain domain;
                        public string relativePathToFile;
                        public string data;
            
            
        }

		protected void WriteStringToFileAtDomainCommandHandler  (WriteStringToFileAtDomainCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("WriteStringToFileAtDomainCommand");
#endif
        
            cmd.result = _service.WriteStringToFileAtDomain(cmd.domain,cmd.relativePathToFile,cmd.data);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("WriteStringToFileAtDomainCommand");
#endif
        }
        

        
        /// <summary>
        /// Load file as string
        /// </summary>
        
        public class LoadFileAsStringCommand  {
            public string result;
                        public string pathToFile;
            
            
        }

		protected void LoadFileAsStringCommandHandler  (LoadFileAsStringCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("LoadFileAsStringCommand");
#endif
        
            cmd.result = _service.LoadFileAsString(cmd.pathToFile);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("LoadFileAsStringCommand");
#endif
        }
        

        
        /// <summary>
        /// Load file as string from domain
        /// </summary>
        
        public class LoadFileAsStringAtDomainCommand  {
            public string result;
                        public FSDomain domain;
                        public string relativePathToFile;
            
            
        }

		protected void LoadFileAsStringAtDomainCommandHandler  (LoadFileAsStringAtDomainCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("LoadFileAsStringAtDomainCommand");
#endif
        
            cmd.result = _service.LoadFileAsStringAtDomain(cmd.domain,cmd.relativePathToFile);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("LoadFileAsStringAtDomainCommand");
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


