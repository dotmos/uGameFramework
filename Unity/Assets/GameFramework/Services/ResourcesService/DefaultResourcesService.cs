﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using System;
using Service.Events;
using Zenject;
using UnityEngine.Profiling;

namespace Service.Resources{
    public class DefaultResourcesService : IResourcesService{
        Dictionary<string, AssetBundle> bundles = new Dictionary<string, AssetBundle>();
        List<string> bundleNames = new List<string>();

        IEventsService _eventService;
        Events.ResourcesPreloadedEvent resourcesUpdatedEvent = new Events.ResourcesPreloadedEvent();

        Dictionary<string, UnityEngine.Object> resourcesCache = new Dictionary<string, UnityEngine.Object>();

        /// <summary>
        /// Create a new instance of ResourcesService.
        /// </summary>
        /// <param name="inCoroutine"></param>
        public DefaultResourcesService([Inject] IEventsService eventService)
        {
            _eventService = eventService;
        }

        public void UnloadResources()
        {
            //Unload everything from all current asset bundles
            Debug.Log("Unloading assetbundles");
            for(int i=0; i<bundleNames.Count; ++i)
            {
                bundles[bundleNames[i]].Unload(true);
                bundles[bundleNames[i]] = null;
            }
            bundles.Clear();
            bundleNames.Clear();
        }

        public void PreloadResources(List<ResourcesData> inBundleData)
        {
            if(inBundleData == null) return;

            //Unload all current bundles of this resources service
            UnloadResources();

            //Get AssetBundles
            MainThreadDispatcher.StartCoroutine(GetAssetBundles(inBundleData));
        }

        IEnumerator GetAssetBundles(List<ResourcesData> inBundleData)
        {
            Debug.Log("-------------- (Down)loading Asset Bundles --------------");

            //Load assetbundles
            WWW www;
            for (int i = 0; i < inBundleData.Count; ++i )
            {
                www = new WWW(inBundleData[i].url);
                while(!www.isDone){
                    _eventService.Publish(new Events.LoadingBundleEvent(){bundleID = inBundleData[i].bundleID, progress = www.progress});
                    yield return null;
                }
                //yield return www;

                if(string.IsNullOrEmpty(www.error) && www.assetBundle != null)
                {
                    bundles.Add(inBundleData[i].bundleID, www.assetBundle);
                    Debug.Log(inBundleData[i].bundleID);
                    bundleNames.Add(inBundleData[i].bundleID);

                    _eventService.Publish(new Events.BundleLoadedEvent(){bundleID = inBundleData[i].bundleID});
                }
                else
                {
                    Debug.LogError(www.error);
                }
            }

            Debug.Log("-------------- Finished (down)loading Asset Bundles --------------");
            _eventService.Publish(resourcesUpdatedEvent);

            yield break;
        }

        public T Get<T>(string path) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(path))
                return default(T);

            //Access resources cache
            if (resourcesCache.ContainsKey(path)) {
                return (T)resourcesCache[path];
            }

            T result = null;

            //Retrive bundle key
            int _cutIndex = path.LastIndexOf("/");
            //This is definitely not inside an assetbundle, try to load from resources
            if (_cutIndex < 0) {
                //try to load from resources folder
                result = UnityEngine.Resources.Load<T>(path);
            }

            if(result == null) {
                string _key = path.Remove(path.LastIndexOf("/"));
                string _assetName = path.Remove(0, path.LastIndexOf("/") + 1);
                //Try to load asset from bundle
                if (bundles.ContainsKey(_key)) {
                    result = bundles[_key].LoadAsset<T>(_assetName);
                }
            }
            
            //Last chance: Try to load asset from Resources
            if(result == null) result = UnityEngine.Resources.Load<T>(path);
            resourcesCache.Add(path, result);
            return result;
        }

        public UnityEngine.Object Get(string path)
        {
            return Get<UnityEngine.Object>(path);
        }
    }
}