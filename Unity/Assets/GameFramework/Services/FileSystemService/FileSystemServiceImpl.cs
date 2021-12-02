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
    public partial class FileSystemServiceImpl : FileSystemServiceBase {


        /// <summary>
        /// cache the unity-paths here since you cannot use those in a thread
        /// </summary>
        private readonly string streamingAssetsPath = Application.streamingAssetsPath;
        private string persistentDataPath = Application.persistentDataPath;
        private readonly string gamePath =  Application.dataPath.Remove(Application.dataPath.LastIndexOf("/"));

        private string configPath;
        private string savegamePath;
        private string scriptingPath;
        private string devUIViewsPath;
        private string localizationPath;
        private string debuggingPath;
        private string moddingPath;
        private string defaultDataPath;

        private long totalSpace;
        private long usedSpace;

        protected override void AfterInitialize() {
            RefreshDataPath();

            //Check if there are locas in streaming assets. If not, use game/Localization folder
            localizationPath = streamingAssetsPath + "/Localizations"; 

            if (!Directory.Exists(localizationPath) || Directory.GetFiles(localizationPath, "*.json").Length == 0) {
                localizationPath = gamePath;
#if !UNITY_EDITOR && UNITY_STANDALONE_OSX
                //on mac we need to go one folder up
                localizationPath += "/..";         
#endif
                localizationPath += "/Localizations";
            }

            Debug.Log($"Diskspace: savegame-used:{GetCurrentlyUsedSavegameStorage()} space available:{GetMaxAvailableSavegameStorage()}");
            UpdateSavegameStorage();
        }
        private void RefreshDataPath() {
            configPath = persistentDataPath + "/config";
            savegamePath = persistentDataPath + "/savegame";
            scriptingPath = persistentDataPath + "/scripting";
            devUIViewsPath = persistentDataPath + "/dev-ui/views";
            debuggingPath = persistentDataPath + "/debugging";
            moddingPath = persistentDataPath + "/modding";
            defaultDataPath = persistentDataPath + "/default";
        }

        public static byte[] Compress(byte[] data, System.IO.Compression.CompressionLevel compressionLevel = System.IO.Compression.CompressionLevel.Optimal) {
            try {
                UnityEngine.Profiling.Profiler.BeginSample("Compress");
                MemoryStream output = new MemoryStream();
                using (DeflateStream dstream = new DeflateStream(output, compressionLevel)) {
                    dstream.Write(data, 0, data.Length);
                }
                return output.ToArray();
            }
            finally {
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        public static byte[] Decompress(byte[] data,int estimatedSize=0) {
            try {
                UnityEngine.Profiling.Profiler.BeginSample("Decompress");

                UnityEngine.Profiling.Profiler.BeginSample("MemoryStreamInput");
                MemoryStream input = new MemoryStream(data);
                UnityEngine.Profiling.Profiler.EndSample();
                UnityEngine.Profiling.Profiler.BeginSample("MemoryStreamOutput");
                MemoryStream output = new MemoryStream(estimatedSize);
                UnityEngine.Profiling.Profiler.EndSample();
                UnityEngine.Profiling.Profiler.BeginSample("dstream");
                using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress)) {
                    UnityEngine.Profiling.Profiler.EndSample();
                    UnityEngine.Profiling.Profiler.BeginSample("copy");
                    dstream.CopyTo(output);
                    UnityEngine.Profiling.Profiler.EndSample();
                }
                UnityEngine.Profiling.Profiler.BeginSample("Decompress");
                return output.ToArray();
            }
            finally {
                UnityEngine.Profiling.Profiler.EndSample();
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        public override string GetPath(FSDomain domain,string relativePart="") {
            string path = defaultDataPath;
            switch (domain) {
                case FSDomain.ConfigFolder: path = configPath; break;
                case FSDomain.SaveGames: path = savegamePath; break;
                case FSDomain.Scripting: path = scriptingPath; break;
                case FSDomain.DevUIViews: path = devUIViewsPath; break;
                case FSDomain.DevUIViewsArchieve: path = GetPath(FSDomain.DevUIViews)+"/archives"; break;
                case FSDomain.RuntimeAssets: path = streamingAssetsPath; break;
                case FSDomain.Localizations: path = localizationPath; break;
                case FSDomain.Debugging: path = debuggingPath; break;
                case FSDomain.Modding: path = moddingPath; break;
                case FSDomain.SteamingAssets: path = streamingAssetsPath; break;
                case FSDomain.Addressables: path = "Assets"; break;

                default: Debug.LogError("UNKNOWN DOMAIN:" + domain.ToString()+" in GetPath! Using MISC-Path"); break;
            }

            if (!string.IsNullOrEmpty(relativePart)) {
                // remove leading slashes
                while (relativePart.StartsWith("/")) {
                    relativePart = relativePart.Substring(1);
                }
            }

            return EnsureDirectoryExistsAndReturn(path) + (!string.IsNullOrEmpty(relativePart) ? "/"+relativePart:"");
        }

        public override bool WriteStringToFile(string pathToFile, string data, bool append=false) {
            // TODO: Ensure Directory?
            // TODO: Use the PC3-bulletproof writing version
            string tempPath = pathToFile + ".tmp";

            try {
                if (File.Exists(tempPath)) {
                    File.Delete(tempPath);
                }

                if (!append) {
                    if (File.Exists(pathToFile)) {
                        File.Delete(pathToFile);
                    }
                    File.WriteAllText(tempPath, data);
                    File.Move(tempPath, pathToFile);
                } else {
                    // TODO: This is not bulletproof as !append (not sure about a good way!? move twice?)
                    File.AppendAllText(pathToFile, data);
                }
                UpdateSavegameStorage();

                return true;
            }
            catch (Exception e) {
                Debug.LogError("There was a problem using WriteStringToFile with "+pathToFile+"=>DATA:\n"+data);
                Debug.LogException(e);
                return false;
            }
        }

        public override bool WriteStringToFileAtDomain(FSDomain domain, string relativePathToFile, string data,bool append=false) {
            if (domain == FSDomain.Addressables) return false;
            relativePathToFile = Utils.CreateValidFilename(relativePathToFile.TrimStart('/'));
            return WriteStringToFile(GetPath(domain, relativePathToFile), data,append);
        }

        //old File-Class-Based 
        //public override bool WriteBytesToFile(string pathToFile, byte[] bytes, bool compress = false) {
        //    try {
        //        UnityEngine.Profiling.Profiler.BeginSample("WriteBytesToFile");

        //        // TODO: Ensure Directory?
        //        // TODO: Use the PC3-bulletproof writing version
        //        try {
        //            string tempName = pathToFile + ".tmp";

        //            if (File.Exists(tempName)) {
        //                File.Delete(tempName);
        //            }
        //            if (File.Exists(pathToFile)) {
        //                File.Delete(pathToFile);
        //            }

        //            if (compress) {
        //                File.WriteAllBytes(tempName, Compress(bytes));
        //            } else {
        //                File.WriteAllBytes(tempName, bytes);
        //            }
        //            File.Move(tempName, pathToFile);
        //            return true;
        //        }
        //        catch (Exception e) {
        //            Debug.LogError("There was a problem using WriteBytesToFile with " + pathToFile + "=>DATA:\n" + bytes);
        //            Debug.LogException(e);
        //            return false;
        //        }
        //    }
        //    finally {
        //        UnityEngine.Profiling.Profiler.EndSample();
        //    }
        //}

        public override bool WriteBytesToFile(string pathToFile, byte[] bytes, bool compress = false) {
            try {
                UnityEngine.Profiling.Profiler.BeginSample("WriteBytesToFile");

                // TODO: Ensure Directory?
                // TODO: Use the PC3-bulletproof writing version
                try {
                    if (File.Exists(pathToFile)) {
                        File.Delete(pathToFile);
                    }

                    if (compress) {
                        bytes = Compress(bytes);
                    }

                    int size = bytes.Length;
                    int chunkSize = size / 20;
                    int totalWritten = 0;
                    using (FileStream fs = new FileStream(pathToFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 1024 * 1024 * 5)) {
                        // Add some information to the file.
                        while (totalWritten < bytes.Length) {
                            int writeSize = Math.Min(size - totalWritten, chunkSize);
                            fs.Write(bytes, totalWritten, writeSize);
                            totalWritten += writeSize;
                        }
                    }
                    UpdateSavegameStorage();

                    return true;
                }
                catch (Exception e) {
                    Debug.LogError("There was a problem using WriteBytesToFile with " + pathToFile + "=>DATA:\n" + bytes);
                    Debug.LogException(e);
                    return false;
                }
            }
            finally {
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        public override bool WriteBytesToFileAtDomain(FSDomain domain, string relativePathToFile, byte[] bytes,bool compress=false) {
            if (domain == FSDomain.Addressables) return false;
            relativePathToFile = Utils.CreateValidFilename(relativePathToFile.TrimStart('/'));
            return WriteBytesToFile(GetPath(domain) + "/" + relativePathToFile, bytes,compress);
        }

        public override string LoadFileAsString(string pathToFile, bool compressed = false) {
            try {
                if (System.IO.File.Exists(pathToFile)) {
                    string result = File.ReadAllText(pathToFile);
                    return result;
                } else {
                    Debug.LogWarning("File " + pathToFile + " does not exist");
                    return null;
                }
            }
            catch (Exception e) {
                Debug.LogError("There was a problem using LoadFileAsString with " + pathToFile);
                Debug.LogException(e);
                return null;
            }

        }

        public override byte[] LoadFileAsBytesAtDomain(FSDomain domain, string relativePathToFile, bool compressed = false, int estimatedUncompressedSize = 0) {            

            if (domain != FSDomain.Addressables) {
                return LoadFileAsBytes(GetPath(domain) + "/" + relativePathToFile,compressed,estimatedUncompressedSize);
            }
            else {

                string path = GetPath(domain, relativePathToFile);
                UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<TextAsset> h = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<TextAsset>(path);
                h.WaitForCompletion();
                byte[] bytes = h.Result.bytes;
                if (compressed) {

                    return Decompress(bytes);
                }
                return bytes;
            }
        }

        public override byte[] LoadFileAsBytes(string pathToFile, bool compressed = false, int estimatedUncompressedSize=0) {
            try {
                UnityEngine.Profiling.Profiler.BeginSample("LoadFileAsBytes");

                try {
                    if (System.IO.File.Exists(pathToFile)) {
                        byte[] result = File.ReadAllBytes(pathToFile);
                        if (compressed) {
                            result = Decompress(result, estimatedUncompressedSize);
                        }
                        return result;
                    } else {
                        
                        return null;
                    }
                        
                }
                catch (Exception e) {
                    Debug.LogError("There was a problem using LoadFileAsString with " + pathToFile);
                    Debug.LogException(e);
                    return null;
                }
            }
            finally {
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        public override string LoadFileAsStringAtDomain(FSDomain domain, string relativePathToFile) {

            if (domain != FSDomain.Addressables) {
                return LoadFileAsString(GetPath(domain) + "/" + relativePathToFile);
            }
            else {

                string path = GetPath(domain, relativePathToFile);
                UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<TextAsset> h = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<TextAsset>(path);
                h.WaitForCompletion();
                return h.Result.text;
            }
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
            if (domain != FSDomain.Addressables) {

                return FileExists(absPath);
            }
            else {

                UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<IList<UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation>> h = UnityEngine.AddressableAssets.Addressables.LoadResourceLocationsAsync(absPath);
                h.WaitForCompletion();
                return h.Result.Count > 0;
            }
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


        public override void SetPersistentRoot(string root) {
            persistentDataPath = root;
            RefreshDataPath();

        }

        protected override void OnDispose() {
            // do your IDispose-actions here. It is called right after disposables got disposed
        }

        public override List<string> GetFilesInAbsFolder(string absPath, string pattern = "*.*",bool recursive=false) {
            List<string> result = new List<string>();
            foreach (string path in Directory.GetFiles(absPath, pattern, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)) {
                result.Add(path.Replace('\\', '/'));
            }
            return result;
        }

        public override List<string> GetFilesInDomain(FSDomain domain, string innerDomainPath="",string filter = "*.*",bool recursive=false) {
            if (domain != FSDomain.Addressables) {

                return GetFilesInAbsFolder(GetPath(domain,innerDomainPath), filter, recursive);
            }
            else {
                Debug.LogError(typeof(FileSystemServiceImpl).Name + "GetFilesInDomain() is not implemented for domain " + FSDomain.Addressables.ToString());
                return null;
            }
        }

        public override void RemoveFile(string filePath) {
            try {
                if (FileExists(filePath)) {
                    File.Delete(filePath);
                }
                UpdateSavegameStorage();
            }
            catch (Exception e) {
                Debug.LogException(e);
            }
        }

        public override void RemoveFileInDomain(FSDomain domain, string relativePath) {
            if (domain == FSDomain.Addressables) return;
            string path = GetPath(domain,relativePath);
            RemoveFile(path);
        }

        public override long GetCurrentlyUsedSavegameStorage() {
#if ENABLE_CONSOLE_UI
            return usedSpace;
#else
            return 10485760; //10 MB in bytes
#endif
        }



        public override long GetMaxAvailableSavegameStorage() {
#if ENABLE_CONSOLE_UI
            return (long)(GetCurrentlyUsedSavegameStorage()*1.25f);
#else
            //FileInfo file = new FileInfo(configPath);
            //DriveInfo drive = new DriveInfo(file.Directory.Root.FullName);
            //return drive.AvailableFreeSpace;
            return 1073741824; //1 GB in bytes
#endif
        }

        private void UpdateSavegameStorage() {
#if ENABLE_CONSOLE_UI && !BUILD_CONSOLE
            usedSpace = DirSize(new DirectoryInfo(savegamePath));
#endif
        }

        public override long GetFreeSavegameStorage() {
            return GetMaxAvailableSavegameStorage() - GetCurrentlyUsedSavegameStorage();
        }
        public static long DirSize(DirectoryInfo d) {
            long size = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis) {
                size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis) {
                size += DirSize(di);
            }
            return size;
        }
    }
}
