///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 

using System.Collections.Generic;
using MoonSharp.Interpreter;

using System;
using static Service.FileSystem.Events;
using System.Runtime.Serialization;
using FlatBuffers;
using Service.Serializer;
using System.Linq;
using Service.PerformanceTest;

namespace Service.FileSystem {
    public interface IFileSystemService : IFBSerializable2, IFBSerializable, IService {


        
        
        /// <summary>
        /// Get path as string for given domain 
                /// <param name="domain"></param>
                /// <param name="realtivePart"></param>
         /// </summary>
        
        
                    string GetPath(FSDomain domain,string realtivePart="");
        
                           
        
        /// <summary>
        /// Write bytes to file. (if maxFileSize is small than the file, multiple files will be created postfixed with '.1','.2'....) 
                /// <param name="pathToFile"></param>
                /// <param name="bytes"></param>
                /// <param name="compress"></param>
                /// <param name="maxFileSize"></param>
         /// </summary>
        
        
                    bool WriteBytesToFile(string pathToFile,byte[] bytes,bool compress=false,int maxFileSize=int.MaxValue);
        
                           
        
        /// <summary>
        /// Write bytes to file at domain (if maxFileSize is small than the file, multiple files will be created postfixed with '.1','.2'....) 
                /// <param name="domain"></param>
                /// <param name="relativePathToFile"></param>
                /// <param name="bytes"></param>
                /// <param name="compress"></param>
                /// <param name="maxFileSize"></param>
         /// </summary>
        
        
                    bool WriteBytesToFileAtDomain(FSDomain domain,string relativePathToFile,byte[] bytes,bool compress=false,int maxFileSize=int.MaxValue);
        
                           
        
        /// <summary>
        /// Write string to file 
                /// <param name="pathToFile"></param>
                /// <param name="thedata"></param>
                /// <param name="append"></param>
         /// </summary>
        
        
                    bool WriteStringToFile(string pathToFile,string thedata,bool append=false);
        
                           
        
        /// <summary>
        /// Write string to file at domain 
                /// <param name="domain"></param>
                /// <param name="relativePathToFile"></param>
                /// <param name="thedata"></param>
                /// <param name="append"></param>
         /// </summary>
        
        
                    bool WriteStringToFileAtDomain(FSDomain domain,string relativePathToFile,string thedata,bool append=false);
        
                           
        
        /// <summary>
        /// Load file as string 
                /// <param name="pathToFile"></param>
                /// <param name="compressed"></param>
         /// </summary>
        
        
                    string LoadFileAsString(string pathToFile,bool compressed=false);
        
                           
        
        /// <summary>
        /// Load file as string from domain 
                /// <param name="domain"></param>
                /// <param name="relativePathToFile"></param>
         /// </summary>
        
        
                    string LoadFileAsStringAtDomain(FSDomain domain,string relativePathToFile);
        
                           
        
        /// <summary>
        /// Load file as bytes 
                /// <param name="pathToFile"></param>
                /// <param name="compressed"></param>
                /// <param name="estimatedUncompressedSize"></param>
         /// </summary>
        
        
                    byte[] LoadFileAsBytes(string pathToFile,bool compressed=false,int estimatedUncompressedSize=0);
        
                           
        
        /// <summary>
        /// Load file as string from domain 
                /// <param name="domain"></param>
                /// <param name="relativePathToFile"></param>
                /// <param name="compressed"></param>
                /// <param name="estimatedUncompressedSize"></param>
         /// </summary>
        
        
                    byte[] LoadFileAsBytesAtDomain(FSDomain domain,string relativePathToFile,bool compressed=false,int estimatedUncompressedSize=0);
        
                           
        
        /// <summary>
        /// Get all absolute file-paths in specified path with optional filter (see https://msdn.microsoft.com/en-us/library/wz42302f(v=vs.110).aspx#Remarks ) 
                /// <param name="absPath"></param>
                /// <param name="pattern"></param>
                /// <param name="recursive"></param>
         /// </summary>
        
        
                    List<string> GetFilesInAbsFolder(string absPath,string pattern="*.*",bool recursive=false);
        
                           
        
        /// <summary>
        /// Get all absolute file-paths in specified domain with optional filter (see https://msdn.microsoft.com/en-us/library/wz42302f(v=vs.110).aspx#Remarks ) 
                /// <param name="domain"></param>
                /// <param name="innerDomainPath"></param>
                /// <param name="filter"></param>
                /// <param name="recursive"></param>
         /// </summary>
        
        
                    List<string> GetFilesInDomain(FSDomain domain,string innerDomainPath="",string filter="*.*",bool recursive=false);
        
                           
        
        /// <summary>
        /// Remove file from filesystem 
                /// <param name="filePath"></param>
         /// </summary>
        
        
                    void RemoveFile(string filePath);
        
                           
        
        /// <summary>
        /// Remove file in domain 
                /// <param name="domain"></param>
                /// <param name="relativePath"></param>
         /// </summary>
        
        
                    void RemoveFileInDomain(FSDomain domain,string relativePath);
        
                           
        
        /// <summary>
        /// Remove file in background-thread. 
                /// <param name="filePath"></param>
         /// </summary>
        
        
                    void RemoveFileAsync(string filePath);
        
                           
        
        /// <summary>
        /// Remove file from domain in background-thread 
                /// <param name="domain"></param>
                /// <param name="relativePath"></param>
         /// </summary>
        
        
                    void RemoveFileInDomainAsync(FSDomain domain,string relativePath);
        
                           
        
        /// <summary>
        /// Check if a file exists(absolute) 
                /// <param name="pathToFile"></param>
         /// </summary>
        
        
                    bool FileExists(string pathToFile);
        
                           
        
        /// <summary>
        /// Check if a file exists in a domain(relative to domain-root) 
                /// <param name="domain"></param>
                /// <param name="relativePath"></param>
         /// </summary>
        
        
                    bool FileExistsInDomain(FSDomain domain,string relativePath);
        
                           
        
        /// <summary>
        /// Set the persistent root-path 
                /// <param name="root"></param>
         /// </summary>
        
        
                    void SetPersistentRoot(string root);
        
                           
        
        /// <summary>
        /// Get max available storage 
         /// </summary>
        
        
                    long GetMaxAvailableSavegameStorage();
        
                           
        
        /// <summary>
        /// Get max available storage 
         /// </summary>
        
        
                    long GetCurrentlyUsedSavegameStorage();
        
                           
        
        /// <summary>
        /// Get free available storage 
         /// </summary>
        
        
                    long GetFreeSavegameStorage();
        
                           
        
        /// <summary>
        /// Get filesize with absolute path 
                /// <param name="pathToFile"></param>
         /// </summary>
        
        
                    long GetFileSize(string pathToFile);
        
                           
        
        /// <summary>
        /// Get filesize in domain 
                /// <param name="domain"></param>
                /// <param name="relativePathInDomain"></param>
         /// </summary>
        
        
                    long GetFileSize(FSDomain domain,string relativePathInDomain);
        
                           

    }

    
    public enum FSDomain {
        Scripting,RuntimeAssets,DevUIViews,DevUIViewsArchieve,ConfigFolder,SaveGames,Localizations,Debugging,Modding,SteamingAssets,Addressables
        
    }
    
    
}
///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 
