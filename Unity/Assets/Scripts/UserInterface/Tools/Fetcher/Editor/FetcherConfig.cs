using UnityEngine;
using UnityEditor;
using System.IO;

namespace UserInterface.Fetcher {
    public class FetcherConfig : ScriptableObject {
        public string pathToPrefabsFolder = "";

        /// <summary>
        /// Self initialize the config file if needed.
        /// We use the singleton pattern here, as the config file should only exist once in a project.
        /// </summary>
        public static FetcherConfig Instance {
            get {
                if (_Instance == null) {
                    if (File.Exists(ConfigFilePath)) {
                        _Instance = AssetDatabase.LoadAssetAtPath<FetcherConfig>(ConfigFilePath);
                    }
                    else {
                        string empty = "";
                        _Instance = CreateInstance<FetcherConfig>();
                        _Instance.pathToPrefabsFolder = EditorUtility.OpenFolderPanel(nameof(FetcherConfig), empty, empty);
                        _Instance.pathToPrefabsFolder = _Instance.pathToPrefabsFolder ?? empty;
                        if (_Instance.pathToPrefabsFolder.StartsWith(Application.dataPath)) {
                            _Instance.pathToPrefabsFolder = "Assets" + _Instance.pathToPrefabsFolder.Substring(Application.dataPath.Length);
                        }
                        AssetDatabase.CreateAsset(_Instance, ConfigFilePath);
                    }
                }
                return _Instance;
            }
        }
        private static FetcherConfig _Instance = null;

        /// <summary>
        /// Path to where we expect the config file to be.
        /// </summary>
        private static string ConfigFilePath {
            get {
                if (_ConfigFilePath == null) {
                    MonoScript ms = MonoScript.FromScriptableObject(CreateInstance<FetcherConfig>());
                    string containingFolder = Path.GetDirectoryName(AssetDatabase.GetAssetPath(ms));
                    _ConfigFilePath = Path.Combine(containingFolder, nameof(FetcherConfig) + ".asset");
                }
                return _ConfigFilePath;
            }
        }
        private static string _ConfigFilePath = null;
    }
}

