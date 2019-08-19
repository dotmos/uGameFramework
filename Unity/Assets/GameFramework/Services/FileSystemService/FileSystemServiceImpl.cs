using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

using Zenject;
using UniRx;
using UnityEngine;
using System.IO;
using Service.Scripting;
using System.IO.Compression;

namespace Service.FileSystem {
    partial class FileSystemServiceImpl : FileSystemServiceBase {

        public readonly string MISC_PATH = Application.persistentDataPath + "/default";

        /// <summary>
        /// cache the unity-paths here since you cannot use those in a thread
        /// </summary>
        private string streamingAssetsPath = Application.streamingAssetsPath;
        private string persistentDataPath = Application.persistentDataPath;

        protected override void AfterInitialize() {
            // this is called right after the Base-Classes Initialize-Method. _eventManager and disposableManager are set
        }

        public static byte[] Compress(byte[] data) {
            MemoryStream output = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(output, System.IO.Compression.CompressionLevel.Optimal)) {
                dstream.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }

        public static byte[] Decompress(byte[] data) {
            MemoryStream input = new MemoryStream(data);
            MemoryStream output = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress)) {
                dstream.CopyTo(output);
            }
            return output.ToArray();
        }

        public override string GetPath(FSDomain domain,string relativePart="") {
            string path = MISC_PATH;
            switch (domain) {
                case FSDomain.ConfigFolder: path = persistentDataPath+"/config"; break;
                case FSDomain.SaveGames: path = persistentDataPath + "/savegame"; break;
                case FSDomain.ScriptingOutput: path = persistentDataPath + "/scripting"; break;
                case FSDomain.DevUIViews: path = persistentDataPath + "/dev-ui/views"; break;
                case FSDomain.DevUIViewsArchieve: path = GetPath(FSDomain.DevUIViews)+"/archives"; break;
                case FSDomain.RuntimeAssets: path = streamingAssetsPath; break;

                
                default: Debug.LogError("UNKNOWN DOMAIN:" + domain.ToString()+" in GetPath! Using MISC-Path"); break;
            }

            if (relativePart != "") {
                // remove leading slashes
                while (relativePart.StartsWith("/")) {
                    relativePart = relativePart.Substring(1);
                }
            }

            return EnsureDirectoryExistsAndReturn(path) + (relativePart!=""?"/"+relativePart:"");
        }

        public override bool WriteStringToFile(string pathToFile, string data) {
            // TODO: Ensure Directory?
            // TODO: Use the PC3-bulletproof writing version
            try {
                File.WriteAllText(pathToFile, data);
                return true;
            }
            catch (Exception e) {
                Debug.LogError("There was a problem using SaveStringToFile with "+pathToFile+"=>DATA:\n"+data);
                Debug.LogException(e);
                return false;
            }
        }

        public override bool WriteStringToFileAtDomain(FSDomain domain, string relativePathToFile, string data) {
            return WriteStringToFile(GetPath(domain) + "/" + relativePathToFile, data);
        }

        public override bool WriteBytesToFile(string pathToFile, byte[] bytes, bool compress = false) {
            // TODO: Ensure Directory?
            // TODO: Use the PC3-bulletproof writing version
            try {
                if (compress) {
                    File.WriteAllBytes(pathToFile, Compress(bytes));
                } else {
                    File.WriteAllBytes(pathToFile, bytes);
                }
                return true;
            }
            catch (Exception e) {
                Debug.LogError("There was a problem using WriteBytesToFile with " + pathToFile + "=>DATA:\n" + bytes);
                Debug.LogException(e);
                return false;
            }
        }

        public override bool WriteBytesToFileAtDomain(FSDomain domain, string relativePathToFile, byte[] bytes,bool compress=false) {
            return WriteBytesToFile(GetPath(domain) + "/" + relativePathToFile, bytes,compress);
        }

        public override string LoadFileAsString(string pathToFile, bool compressed = false) {
            try {
                var result = File.ReadAllText(pathToFile);
                return result;
            }
            catch (Exception e) {
                Debug.LogError("There was a problem using LoadFileAsString with " + pathToFile);
                Debug.LogException(e);
                return null;
            }

        }

        public override byte[] LoadFileAsBytesAtDomain(FSDomain domain, string relativePathToFile, bool compressed = false) {
            return LoadFileAsBytes(GetPath(domain) + "/" + relativePathToFile,compressed);
        }

        public override byte[] LoadFileAsBytes(string pathToFile, bool compressed = false) {
            try {
                var result = File.ReadAllBytes(pathToFile);
                if (compressed) {
                    result = Decompress(result);
                }
                return result;
            }
            catch (Exception e) {
                Debug.LogError("There was a problem using LoadFileAsString with " + pathToFile);
                Debug.LogException(e);
                return null;
            }
        }

        public override string LoadFileAsStringAtDomain(FSDomain domain, string relativePathToFile) {
            return LoadFileAsString(GetPath(domain) + "/" + relativePathToFile);
        }

        public override bool FileExists(string pathToFile) {
            try {
                return File.Exists(pathToFile);
            }
            catch (Exception e) {
                Debug.LogException(e);
                return false;
            }
        }

        public override bool FileExistsInDomain(FSDomain domain, string relativePath) {
            string absPath = GetPath(domain, relativePath);
            return FileExists(absPath);
        }

        private string EnsureDirectoryExistsAndReturn(string path) {
            try {
                if (!Directory.Exists(path)) {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
            catch (Exception e) {
                Debug.LogError("There was a problem accessing the folder at " + path);
                Debug.LogException(e);
                return "";
            }
        }


        protected override void OnDispose() {
            // do your IDispose-actions here. It is called right after disposables got disposed
        }

        public override List<string> GetFilesInAbsFolder(string absPath, string pattern = "*.*") {
            var result = new List<string>();
            foreach (var path in Directory.GetFiles(absPath, pattern, SearchOption.TopDirectoryOnly)) {
                result.Add(path.Replace('\\', '/'));
            }
            return result;
        }

        public override List<string> GetFilesInDomain(FSDomain domain, string innerDomainPath="",string filter = "*.*") {
            return GetFilesInAbsFolder(GetPath(domain,innerDomainPath), filter);
        }

        public override void RemoveFile(string filePath) {
            try {
                if (FileExists(filePath)) {
                    File.Delete(filePath);
                }
            }
            catch (Exception e) {
                Debug.LogException(e);
            }
        }

        public override void RemoveFileInDomain(FSDomain domain, string relativePath) {
            var path = GetPath(domain,relativePath);
            RemoveFile(path);
        }
    }
}
