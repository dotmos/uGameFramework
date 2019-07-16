///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 

using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

using static Service.FileSystem.Events;
using System.Runtime.Serialization;
using FlatBuffers;
using Service.Serializer;

namespace Service.FileSystem {
    public interface IFileSystemService : IFBSerializable {



		/// <summary>
        /// Get path as string for given domain 
        /// <param name="domain"></param>
        /// <param name="realtivePart"></param>
 /// </summary>
        

					string GetPath(FSDomain domain,string realtivePart="");


		/// <summary>
        /// Write bytes to file 
        /// <param name="pathToFile"></param>
        /// <param name="bytes"></param>
        /// <param name="compress"></param>
 /// </summary>
        

					bool WriteBytesToFile(string pathToFile,byte[] bytes,bool compress=false);


		/// <summary>
        /// Write bytes to file at domain 
        /// <param name="domain"></param>
        /// <param name="relativePathToFile"></param>
        /// <param name="bytes"></param>
        /// <param name="compress"></param>
 /// </summary>
        

					bool WriteBytesToFileAtDomain(FSDomain domain,string relativePathToFile,byte[] bytes,bool compress=false);


		/// <summary>
        /// Write string to file 
        /// <param name="pathToFile"></param>
        /// <param name="thedata"></param>
 /// </summary>
        

					bool WriteStringToFile(string pathToFile,string thedata);


		/// <summary>
        /// Write string to file at domain 
        /// <param name="domain"></param>
        /// <param name="relativePathToFile"></param>
        /// <param name="thedata"></param>
 /// </summary>
        

					bool WriteStringToFileAtDomain(FSDomain domain,string relativePathToFile,string thedata);


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
 /// </summary>
        

					byte[] LoadFileAsBytes(string pathToFile,bool compressed=false);


		/// <summary>
        /// Load file as string from domain 
        /// <param name="domain"></param>
        /// <param name="relativePathToFile"></param>
        /// <param name="compressed"></param>
 /// </summary>
        

					byte[] LoadFileAsBytesAtDomain(FSDomain domain,string relativePathToFile,bool compressed=false);


		/// <summary>
        /// Get all absolute file-paths in specified path with optional filter (see https://msdn.microsoft.com/en-us/library/wz42302f(v=vs.110).aspx#Remarks ) 
        /// <param name="absPath"></param>
        /// <param name="pattern"></param>
 /// </summary>
        

					List<string> GetFilesInAbsFolder(string absPath,string pattern="*.*");


		/// <summary>
        /// Get all absolute file-paths in specified domain with optional filter (see https://msdn.microsoft.com/en-us/library/wz42302f(v=vs.110).aspx#Remarks ) 
        /// <param name="domain"></param>
        /// <param name="innerDomainPath"></param>
        /// <param name="filter"></param>
 /// </summary>
        

					List<string> GetFilesInDomain(FSDomain domain,string innerDomainPath="",string filter="*.*");


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

	}


    public enum FSDomain {
        ScriptingOutput,RuntimeAssets,DevUIViews,DevUIViewsArchieve,ConfigFolder,SaveGames
        
    }
    
    
}
///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 
