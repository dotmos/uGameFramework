namespace Service.Scene{
    public class Events{
        public class SceneLoadedEvent
        {
            public string sceneID;
        }

        public class SceneUnloadedEvent
        {
            public string sceneID;
            public bool hasError = false;
        }

        public class SceneSetAsActiveEvent
        {
            public string sceneID;
        } 

        public class ActivatedSceneEvent
        {
            public string sceneID;
        }

        public class DeactivatedSceneEvent
        {
            public string sceneID;
        }
    }
}