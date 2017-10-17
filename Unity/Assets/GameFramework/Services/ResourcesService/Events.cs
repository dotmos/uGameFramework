namespace Service.Resources{
    public class Events{
        public class ResourcesPreloadedEvent
        {
        }

        public class LoadingBundleEvent{
            public string bundleID;
            public float progress;
        }

        public class BundleLoadedEvent{
            public string bundleID;
        }
    }
}