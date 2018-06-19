using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UniRx;
using Zenject;
using Service.Events;
using System;
using System.Collections.Generic;

namespace Service.Scene{
    public class DefaultSceneService : ISceneService {

        IEventsService _eventService;

        public DefaultSceneService(
            [Inject] IEventsService eventService)
        {
            _eventService = eventService;
        }

        /// <summary>
        /// Load the specified scene.
        /// </summary>
        /// <param name="sceneID">Scene I.</param>
        /// <param name="additive">If set to <c>true</c> additive.</param>
        /// <param name="makeActive">If set to <c>true</c> make active.</param>
        public void Load(string sceneID, bool additive = true, bool makeActive = true){
            if(!IsSceneLoaded(sceneID)){
                SceneManager.LoadScene(sceneID, additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
            }

            //Fire scene loaded signal
            _eventService.Publish(new Events.SceneLoadedEvent() { sceneID = sceneID });

            //activate scene if it is currently disabled
            ActivateScene(sceneID);

            //Set scene to active
            if(!additive || makeActive) Observable.NextFrame().Subscribe(e => SetActiveScene(sceneID));
        }

        /// <summary>
        /// Loads the scene.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="sceneID">Scene I.</param>
        /// <param name="additive">If set to <c>true</c> additive.</param>
        /// <param name="makeActive">If set to <c>true</c> make active.</param>
        public AsyncOperation LoadAsync(string sceneID, bool additive = true, bool makeActive = true){
            AsyncOperation _asyncOp = null;
            if(!IsSceneLoaded(sceneID)){
                _asyncOp = SceneManager.LoadSceneAsync(sceneID, additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
                if(_asyncOp != null){
                    _asyncOp.AsAsyncOperationObservable()
                        .Last()
                        .Subscribe(e => {
                            Debug.Log(sceneID + " loaded");
                            //Fire signal trigger
                            _eventService.Publish(new Events.SceneLoadedEvent { sceneID = sceneID});
                            //Set scene to active
                            if(!additive || makeActive) SetActiveScene(sceneID);
                        });
                }
            } else {
                _asyncOp = new AsyncOperation();

                //Fire signal trigger
                _eventService.Publish(new Events.SceneLoadedEvent { sceneID = sceneID }); ;

                //activate scene if it is currently disabled
                ActivateScene(sceneID);

                //Set scene to active
                if(!additive || makeActive) SetActiveScene(sceneID);
            }
            return _asyncOp;
        }

        /// <summary>
        /// Unload the specified scene.
        /// </summary>
        /// <param name="sceneName">Scene name.</param>
        public void Unload(string sceneID){
            //Do nothing if scene is not loaded
            if(!SceneManager.GetSceneByName(sceneID).isLoaded) return;

            AsyncOperation _asyncUnload = SceneManager.UnloadSceneAsync(sceneID);
            //Do nothing if there is no scene
            if(_asyncUnload == null){
                Debug.Log(sceneID + " not in active hirachy. Could not unload");
                return;
            }
            _asyncUnload.AsAsyncOperationObservable().Subscribe(
                e => {
                    //Do stuff while unloading
                },
                
                error => {
                    Debug.Log("Error unloading scene: "+sceneID);
                    _eventService.Publish(new Events.SceneUnloadedEvent(){hasError = true, sceneID = sceneID});
                },

                () => {
                    //Unload finished
                     _eventService.Publish(new Events.SceneUnloadedEvent(){hasError = false, sceneID = sceneID});
                    Debug.Log(sceneID + " unloaded");
                }
            );
        }

        public bool IsSceneLoaded(string sceneID){
            return SceneManager.GetSceneByName(sceneID).isLoaded;
        }

        /// <summary>
        /// Sets the active scene. When using Load or LoadAsync, newly loaded scene will automatically be set to active if scene was not loaded additive.
        /// </summary>
        /// <param name="sceneName">Scene name.</param>
        public void SetActiveScene(string sceneID){
            UnityEngine.SceneManagement.Scene scene = SceneManager.GetSceneByName(sceneID);
            if(scene.isLoaded == true)
            {
                //Activate scene
                ActivateScene(sceneID);
                //Set as active scene
                SceneManager.SetActiveScene(scene);
                //Fire signal trigger
                //_sceneSetAsActiveSignalTrigger.Fire(sceneName);
                _eventService.Publish(new Events.SceneSetAsActiveEvent { sceneID = sceneID});
                Debug.Log("Setting " + scene.name + " as active scene");
            }
        }

        /// <summary>
        /// Gets the active scene ID
        /// </summary>
        /// <returns>The active scene I.</returns>
        public string GetActiveSceneID(){
            return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        }

        /// <summary>
        /// Activates the scene, if available.
        /// </summary>
        /// <param name="sceneID">Scene I.</param>
        public void ActivateScene(string sceneID) {
            EnableScene(sceneID, true);
        }

        /// <summary>
        /// Deactivates the scene, if available.
        /// </summary>
        /// <param name="sceneID">Scene I.</param>
        public void DeactivateScene(string sceneID) {
            EnableScene(sceneID, false);
        }

        void EnableScene(string sceneID, bool state){
            if(IsSceneLoaded(sceneID)){
                foreach(GameObject go in SceneManager.GetSceneByName(sceneID).GetRootGameObjects()){
                    go.SetActive(state);
                }

                if(state == true){
                    Observable.NextFrame().Subscribe(e => _eventService.Publish(new Events.ActivatedSceneEvent(){sceneID = sceneID}));
                }
                else {
                    Observable.NextFrame().Subscribe(e => _eventService.Publish(new Events.DeactivatedSceneEvent(){sceneID = sceneID}));
                }
            }
        }

        /// <summary>
        /// Returns all root objects of the scene. List is empty if scene is not loaded
        /// </summary>
        /// <param name="sceneID"></param>
        /// <returns></returns>
        public List<GameObject> GetSceneRootObjects(string sceneID) {
            if (IsSceneLoaded(sceneID)) {
                UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneID);
                return new List<GameObject>(scene.GetRootGameObjects());
            } else {
                return new List<GameObject>();
            }
        }
    }
}