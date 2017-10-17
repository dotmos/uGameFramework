using UnityEngine;
using System;
using UniRx;
using System.Collections.Generic;

namespace Service.Resources{
    public class ResourcesUpdatedData{
        public bool success;
    }

    public interface IResourcesService {
        void PreloadResources(List<ResourcesData> resources);
        void UnloadResources();

        UnityEngine.Object Load(string path);
        T Load<T>(string path) where T : UnityEngine.Object;
    }
}
