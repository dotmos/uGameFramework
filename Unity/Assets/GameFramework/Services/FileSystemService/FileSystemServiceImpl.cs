using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

using Zenject;
using UniRx;
using UnityEngine;
using System.IO;

namespace Service.FileSystem {
    class FileSystemServiceImpl : FileSystemServiceBase {

        public readonly string MISC_PATH = Application.persistentDataPath + "/default";

        protected override void AfterInitialize() {
            // this is called right after the Base-Classes Initialize-Method. _eventManager and disposableManager are set

            string path = GetPath(FSDomain.ScriptingOutput);
            File.WriteAllText(path+"/script.lua", "a=10;print(a);");
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
