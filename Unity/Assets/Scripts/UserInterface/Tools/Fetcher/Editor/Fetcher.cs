using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using UserInterface.Scrollbar;
using Names = UserInterface.NamingHelper;

namespace UserInterface.Fetcher {

    /// <summary>
    /// This class fetches UI resources to the hierachy context menu.
    /// It uses the path of the FetcherConfig file to determine where to look for prefabs.
    /// We specifically do not use the "Resources" folder here to prevent these items to slip into a build, as they are for editor purposes only.
    /// </summary>
    [InitializeOnLoad]
    internal static class Fetcher {

        // --- GMScrollbar
        [MenuItem(Names.Scrollbar.HierachyName, false, 0)]
        static void AddScrollbarPrefabToScene() {
            InstantiateFromPath(nameof(GMScrollbar));
        }

        // --- Buttons
        // Button Icon
        [MenuItem(Names.Button.HierachyName + Names.DisplaySpace + Names.Button.Variants.Icon, false, 0)]
        static void AddButtonIconPrefabToScene() {
            InstantiateFromPath(nameof(GMButton) + Names.TypeSeparator + Names.Button.Variants.Icon);
        }
        // Button Label
        [MenuItem(Names.Button.HierachyName + Names.DisplaySpace + Names.Button.Variants.Label, false, 0)]
        static void AddButtonLabelPrefabToScene() {
            InstantiateFromPath(nameof(GMButton) + Names.TypeSeparator + Names.Button.Variants.Label);
        }
        // Button IconLabel
        [MenuItem(Names.Button.HierachyName + Names.DisplaySpace + Names.Button.Variants.IconLabel, false, 0)]
        private static void AddButtonIconLabelPrefabToScene() {
            InstantiateFromPath(nameof(GMButton) + Names.TypeSeparator + Names.Button.Variants.IconLabel);
        }

        // --- GMToggle
        [MenuItem(Names.Toggle.HierachyName, false, 0)]
        private static void AddTogglePrefabToScene() {
            InstantiateFromPath(nameof(GMToggle));
        }

        // --- GMResizableLayout
        [MenuItem(Names.ResizableLayout.HierachyName, false, 0)]
        private static void AddResizableLayoutPrefabToScene() {
            // TODO: @Stephan rename class so we can use nameof here
            InstantiateFromPath(Names.ResizableLayout.PrefabName);
        }

        // --- GMTab
        [MenuItem(Names.Tab.HierachyName, false, 0)]
        private static void AddTabPrefabToScene() {
            InstantiateFromPath(nameof(GMTab));
        }

        // --- GMTabbar
        [MenuItem(Names.Tabbar.HierachyName, false, 0)]
        private static void AddTabbarPrefabToScene() {
            InstantiateFromPath(nameof(GMTabbar));
        }

        // --- GMScrollRect
        // Note: Prefabs are named "ScrollView" here, as we follow a naming convention imposed by Unity.
        // ScrollView Vertical
        [MenuItem(Names.ScrollRect.HierachyName + Names.DisplaySpace + Names.ScrollRect.Variants.Vertical, false, 0)]
        private static void AddScrollViewVerticalPrefabToScene() {
            string name = Names.BasePrefix + Names.ScrollRect.PrefabName + Names.TypeSeparator + Names.ScrollRect.Variants.Vertical;
            InstantiateFromPath(name);
        }

        // --- GMScrollRectNavigation
        [MenuItem(Names.ScrollRectNavigation.HierachyName, false, 0)]
        private static void AddScrollRectNavigationPrefabToScene() {
            InstantiateFromPath(nameof(GMScrollRectNavigation));
        }

        #region HelperLogic
        private static Dictionary<string, GameObject> prefabCache = new Dictionary<string, GameObject>();

        static Fetcher() {
            prefabCache.Clear();
        }

        private static void InstantiateFromPath(string relativeFilePathWithoutExtension) {
            string path = Path.Combine(FetcherConfig.Instance.pathToPrefabsFolder, relativeFilePathWithoutExtension + ".prefab");
            
            // check if filepath is valid and we have a file...
            if (File.Exists(path)) {
                string fileName = Path.GetFileNameWithoutExtension(path);

                // add object to cache if it's contained yet.
                if (!prefabCache.ContainsKey(path)) {
                    prefabCache.Add(path, AssetDatabase.LoadAssetAtPath<GameObject>(path));
                }
                // instantiate item in the hierachy, based on the currently active transform
                GameObject gameObject;
                if (Selection.activeTransform != null) {
                    gameObject = GameObject.Instantiate(prefabCache[path], Vector3.zero, Quaternion.identity, Selection.activeTransform);
                }
                else {
                    gameObject = GameObject.Instantiate(prefabCache[path], Vector3.zero, Quaternion.identity);
                }
                gameObject.name = fileName;
            }
            else {
                Debug.LogError("File does not exist @"+path);
            }
        }
        #endregion
    }
}
