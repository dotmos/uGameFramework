
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

            this.OnEvent<WriteBytesToFileCommand>().Subscribe(e => WriteBytesToFileCommandHandler(e)).AddTo(this);

            this.OnEvent<WriteBytesToFileAtDomainCommand>().Subscribe(e => WriteBytesToFileAtDomainCommandHandler(e)).AddTo(this);

            this.OnEvent<WriteStringToFileCommand>().Subscribe(e => WriteStringToFileCommandHandler(e)).AddTo(this);

            this.OnEvent<WriteStringToFileAtDomainCommand>().Subscribe(e => WriteStringToFileAtDomainCommandHandler(e)).AddTo(this);

            this.OnEvent<LoadFileAsStringCommand>().Subscribe(e => LoadFileAsStringCommandHandler(e)).AddTo(this);

            this.OnEvent<LoadFileAsStringAtDomainCommand>().Subscribe(e => LoadFileAsStringAtDomainCommandHandler(e)).AddTo(this);

            this.OnEvent<LoadFileAsBytesCommand>().Subscribe(e => LoadFileAsBytesCommandHandler(e)).AddTo(this);

            this.OnEvent<LoadFileAsBytesAtDomainCommand>().Subscribe(e => LoadFileAsBytesAtDomainCommandHandler(e)).AddTo(this);

            this.OnEvent<GetFilesInAbsFolderCommand>().Subscribe(e => GetFilesInAbsFolderCommandHandler(e)).AddTo(this);

            this.OnEvent<GetFilesInDomainCommand>().Subscribe(e => GetFilesInDomainCommandHandler(e)).AddTo(this);

            this.OnEvent<RemoveFileCommand>().Subscribe(e => RemoveFileCommandHandler(e)).AddTo(this);

            this.OnEvent<RemoveFileInDomainCommand>().Subscribe(e => RemoveFileInDomainCommandHandler(e)).AddTo(this);

            this.OnEvent<FileExistsCommand>().Subscribe(e => FileExistsCommandHandler(e)).AddTo(this);

            this.OnEvent<FileExistsInDomainCommand>().Subscribe(e => FileExistsInDomainCommandHandler(e)).AddTo(this);

        }
        

        
        /// <summary>
        /// Get path as string for given domain
        /// </summary>
        
        public class GetPathCommand  {
            public string result;
                        public FSDomain domain;
                        public string realtivePart="";
            
            
        }

		protected void GetPathCommandHandler  (GetPathCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("GetPathCommand");
#endif
        
            cmd.result = _service.GetPath(cmd.domain,cmd.realtivePart);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("GetPathCommand");
#endif
        }
        

        
        /// <summary>
        /// Write bytes to file
        /// </summary>
        
        public class WriteBytesToFileCommand  {
            public bool result;
                        public string pathToFile;
                        public byte[] bytes;
                        public bool compress=false;
            
            
        }

		protected void WriteBytesToFileCommandHandler  (WriteBytesToFileCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("WriteBytesToFileCommand");
#endif
        
            cmd.result = _service.WriteBytesToFile(cmd.pathToFile,cmd.bytes,cmd.compress);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("WriteBytesToFileCommand");
#endif
        }
        

        
        /// <summary>
        /// Write bytes to file at domain
        /// </summary>
        
        public class WriteBytesToFileAtDomainCommand  {
            public bool result;
                        public FSDomain domain;
                        public string relativePathToFile;
                        public byte[] bytes;
                        public bool compress=false;
            
            
        }

		protected void WriteBytesToFileAtDomainCommandHandler  (WriteBytesToFileAtDomainCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("WriteBytesToFileAtDomainCommand");
#endif
        
            cmd.result = _service.WriteBytesToFileAtDomain(cmd.domain,cmd.relativePathToFile,cmd.bytes,cmd.compress);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("WriteBytesToFileAtDomainCommand");
#endif
        }
        

        
        /// <summary>
        /// Write string to file
        /// </summary>
        
        public class WriteStringToFileCommand  {
            public bool result;
                        public string pathToFile;
                        public string thedata;
            
            
        }

		protected void WriteStringToFileCommandHandler  (WriteStringToFileCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("WriteStringToFileCommand");
#endif
        
            cmd.result = _service.WriteStringToFile(cmd.pathToFile,cmd.thedata);
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
                        public string thedata;
            
            
        }

		protected void WriteStringToFileAtDomainCommandHandler  (WriteStringToFileAtDomainCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("WriteStringToFileAtDomainCommand");
#endif
        
            cmd.result = _service.WriteStringToFileAtDomain(cmd.domain,cmd.relativePathToFile,cmd.thedata);
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
                        public bool compressed=false;
            
            
        }

		protected void LoadFileAsStringCommandHandler  (LoadFileAsStringCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("LoadFileAsStringCommand");
#endif
        
            cmd.result = _service.LoadFileAsString(cmd.pathToFile,cmd.compressed);
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
        

        
        /// <summary>
        /// Load file as bytes
        /// </summary>
        
        public class LoadFileAsBytesCommand  {
            public byte[] result;
                        public string pathToFile;
                        public bool compressed=false;
            
            
        }

		protected void LoadFileAsBytesCommandHandler  (LoadFileAsBytesCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("LoadFileAsBytesCommand");
#endif
        
            cmd.result = _service.LoadFileAsBytes(cmd.pathToFile,cmd.compressed);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("LoadFileAsBytesCommand");
#endif
        }
        

        
        /// <summary>
        /// Load file as string from domain
        /// </summary>
        
        public class LoadFileAsBytesAtDomainCommand  {
            public byte[] result;
                        public FSDomain domain;
                        public string relativePathToFile;
                        public bool compressed=false;
            
            
        }

		protected void LoadFileAsBytesAtDomainCommandHandler  (LoadFileAsBytesAtDomainCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("LoadFileAsBytesAtDomainCommand");
#endif
        
            cmd.result = _service.LoadFileAsBytesAtDomain(cmd.domain,cmd.relativePathToFile,cmd.compressed);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("LoadFileAsBytesAtDomainCommand");
#endif
        }
        

        
        /// <summary>
        /// Get all absolute file-paths in specified path with optional filter (see https://msdn.microsoft.com/en-us/library/wz42302f(v=vs.110).aspx#Remarks )
        /// </summary>
        
        public class GetFilesInAbsFolderCommand  {
            public List<string> result;
                        public string absPath;
                        public string pattern="*.*";
            
            
        }

		protected void GetFilesInAbsFolderCommandHandler  (GetFilesInAbsFolderCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("GetFilesInAbsFolderCommand");
#endif
        
            cmd.result = _service.GetFilesInAbsFolder(cmd.absPath,cmd.pattern);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("GetFilesInAbsFolderCommand");
#endif
        }
        

        
        /// <summary>
        /// Get all absolute file-paths in specified domain with optional filter (see https://msdn.microsoft.com/en-us/library/wz42302f(v=vs.110).aspx#Remarks )
        /// </summary>
        
        public class GetFilesInDomainCommand  {
            public List<string> result;
                        public FSDomain domain;
                        public string innerDomainPath="";
                        public string filter="*.*";
            
            
        }

		protected void GetFilesInDomainCommandHandler  (GetFilesInDomainCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("GetFilesInDomainCommand");
#endif
        
            cmd.result = _service.GetFilesInDomain(cmd.domain,cmd.innerDomainPath,cmd.filter);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("GetFilesInDomainCommand");
#endif
        }
        

        
        /// <summary>
        /// Remove file from filesystem
        /// </summary>
        
        public class RemoveFileCommand  {
            public string filePath;
            
            
        }

		protected void RemoveFileCommandHandler  (RemoveFileCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("RemoveFileCommand");
#endif
        _service.RemoveFile(cmd.filePath);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("RemoveFileCommand");
#endif
        }
        

        
        /// <summary>
        /// Remove file in domain
        /// </summary>
        
        public class RemoveFileInDomainCommand  {
            public FSDomain domain;
                        public string relativePath;
            
            
        }

		protected void RemoveFileInDomainCommandHandler  (RemoveFileInDomainCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("RemoveFileInDomainCommand");
#endif
        _service.RemoveFileInDomain(cmd.domain,cmd.relativePath);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("RemoveFileInDomainCommand");
#endif
        }
        

        
        /// <summary>
        /// Check if a file exists(absolute)
        /// </summary>
        
        public class FileExistsCommand  {
            public bool result;
                        public string pathToFile;
            
            
        }

		protected void FileExistsCommandHandler  (FileExistsCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("FileExistsCommand");
#endif
        
            cmd.result = _service.FileExists(cmd.pathToFile);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("FileExistsCommand");
#endif
        }
        

        
        /// <summary>
        /// Check if a file exists in a domain(relative to domain-root)
        /// </summary>
        
        public class FileExistsInDomainCommand  {
            public bool result;
                        public FSDomain domain;
                        public string relativePath;
            
            
        }

		protected void FileExistsInDomainCommandHandler  (FileExistsInDomainCommand cmd) {
#if PERFORMANCE_TEST
            var ptest=Service.Performance.PerformanceTest.Get();
            ptest.Start("FileExistsInDomainCommand");
#endif
        
            cmd.result = _service.FileExistsInDomain(cmd.domain,cmd.relativePath);
#if PERFORMANCE_TEST
            // now stop the watches
            ptest.Stop("FileExistsInDomainCommand");
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


