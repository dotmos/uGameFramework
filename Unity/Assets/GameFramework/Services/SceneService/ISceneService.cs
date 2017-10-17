using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Service.Scene{
    public interface ISceneService {
        /// <summary>
        /// Load the specified scene.
        /// </summary>
        /// <param name="sceneID">Scene I.</param>
        /// <param name="additive">If set to <c>true</c> additive.</param>
        /// <param name="makeActive">If set to <c>true</c> make active.</param>
        void Load(string sceneID, bool additive = true, bool makeActive = true);

        /// <summary>
        /// Loads the scene.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="sceneID">Scene I.</param>
        /// <param name="additive">If set to <c>true</c> additive.</param>
        /// <param name="makeActive">If set to <c>true</c> make active.</param>
        AsyncOperation LoadAsync(string sceneID, bool additive = true, bool makeActive = true);

        /// <summary>
        /// Unload the specified scene.
        /// </summary>
        /// <param name="sceneID">Scene I.</param>
        void Unload(string sceneID);

        /// <summary>
        /// Sets the active scene.
        /// </summary>
        /// <param name="sceneID">Scene I.</param>
        void SetActiveScene(string sceneID);

        /// <summary>
        /// Determines whether this instance is scene loaded the specified sceneID.
        /// </summary>
        /// <returns><c>true</c> if this instance is scene loaded the specified sceneID; otherwise, <c>false</c>.</returns>
        /// <param name="sceneID">Scene I.</param>
        bool IsSceneLoaded(string sceneID);

        /// <summary>
        /// Gets the active scene ID
        /// </summary>
        /// <returns>The active scene I.</returns>
        string GetActiveSceneID();

        /// <summary>
        /// Activates the scene, if available.
        /// </summary>
        /// <param name="sceneID">Scene I.</param>
        void ActivateScene(string sceneID);

        /// <summary>
        /// Deactivates the scene, if available.
        /// </summary>
        /// <param name="sceneID">Scene I.</param>
        void DeactivateScene(string sceneID);

        /// <summary>
        /// returns the root objects for the given scene
        /// </summary>
        /// <param name="sceneID"></param>
        /// <returns></returns>
        List<GameObject> GetSceneRootObjects(string sceneID);
    }
}