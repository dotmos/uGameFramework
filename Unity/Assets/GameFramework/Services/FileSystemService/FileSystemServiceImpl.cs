using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

using Zenject;
using UniRx;
using UnityEngine;
using System.IO;
using Service.Scripting;

namespace Service.FileSystem {
    class FileSystemServiceImpl : FileSystemServiceBase {

        public readonly string MISC_PATH = Application.persistentDataPath + "/default";

        protected override void AfterInitialize() {
            // this is called right after the Base-Classes Initialize-Method. _eventManager and disposableManager are set
        }

        public override string GetPath(FSDomain domain) {
            string prefix = Application.streamingAssetsPath;
            string path = MISC_PATH;
            switch (domain) {
                case FSDomain.ScriptingOutput: path = Application.persistentDataPath + "/scripting"; break;
                default: Debug.LogError("UNKNOWN DOMAIN:" + domain.ToString()+" in GetPath! Using MISC-Path"); break;
            }
            return EnsureDirectoryExistsAndReturn(path);
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

        public override string LoadFileAsString(string pathToFile) {
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

    }
}
