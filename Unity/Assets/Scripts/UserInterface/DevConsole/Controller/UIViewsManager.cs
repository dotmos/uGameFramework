using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UniRx;
using Service.DevUIService;
using UnityEditor;

namespace UserInterface {
    public class UIViewsManager : GameComponent {

        public GameObject uiViewTabPrefab;
        public GameObject uiViewPrefab;
        [Space]
        public Transform uiViewsContainer;
        public GMTabbar uiViewTabbar;
        [Space]
        public GMButton pickObjectFromSceneButton;
        public GMButton browseViewsButton;
        public GMButton refreshViewsButton;

        [Inject]
        private IDevUIService _devUiService;

        private Dictionary<DevUIView, UIViewController> uiViews = new Dictionary<DevUIView, UIViewController>();

        protected override void AfterBind() {
            base.AfterBind();

            //Load existing
            ReactiveCollection<DevUIView> devUIViews = _devUiService.GetRxViews();
            
            foreach(DevUIView uiView in devUIViews) {
                SpawnUIView(uiView);
            }

            //listen to new
            _devUiService.GetRxViews().ObserveAdd().Subscribe(evt => {
                // spawn View
                DevUIView view = evt.Value;
                SpawnUIView(view);
            }).AddTo(this);

            //listen to remove
            _devUiService.GetRxViews().ObserveRemove().Subscribe(evt => {
                RemoveView(evt.Value);
            }).AddTo(this);

            //Button
            browseViewsButton.onClick.AddListener(Browse);
            //Refresh
            refreshViewsButton.onClick.AddListener(Refresh);
            //Pick from Scene
            pickObjectFromSceneButton.onClick.AddListener(PickObjectFromScene);
        }

        /// <summary>
        /// Creates a default UI view (triggered from ui button)
        /// </summary>
        /// <param name="name">(optional) A name for the ui View.</param>
        public void CreateDefaultUIView(string name) {
            if (name == null || _devUiService.ViewNameExists(name)) {
                name = "view_" + uiViews.Count;
            }
            _devUiService.CreateView(name,true);
        }

        void PickObjectFromScene() {
            _devUiService.StartPickingEntity();
        }

        /// <summary>
        /// Sets up a new ui view and adds ui elements if there are any
        /// </summary>
        /// <param name="devUIView">The dev UI view data</param>
        public void SpawnUIView(DevUIView devUIView) {
            //Spawn  the tab
            GameObject uiViewTabGO = Instantiate(uiViewTabPrefab) as GameObject;
            uiViewTabGO.transform.SetParent(uiViewTabbar.transform, false);
            uiViewTabGO.name = "tab_" + devUIView.Name;

            //Spawn the view
            GameObject uiViewGO = Instantiate(uiViewPrefab) as GameObject;
            uiViewGO.transform.SetParent(uiViewsContainer, false);

            //Connect tab and view
            GMTab uiViewTab = uiViewTabGO.GetComponent<GMTab>();
            uiViewTab.content = uiViewGO;
            uiViewTabbar.RegisterTab(uiViewTab);

            UIViewController uiViewController = uiViewGO.GetComponent<UIViewController>();
            uiViewController.Initialize(devUIView, uiViewTab);

            uiViews.Add(devUIView, uiViewController);
        }

        void RemoveView(DevUIView view) {
            if (uiViews.ContainsKey(view)) {
                //Remove tab
                uiViewTabbar.RemoveTab(uiViews[view].myTab, true);
                //Destroy uiView
                Destroy(uiViews[view].gameObject);
                uiViews.Remove(view);
            }
        }

        void Browse() {
            var filesystem = Kernel.Instance.Container.Resolve<Service.FileSystem.IFileSystemService>();
            var path = filesystem.GetPath(Service.FileSystem.FSDomain.DevUIViewsArchieve);

            //TODO: This will not work in build! Try to use something like System.Diagnostics.Process.Start("explorer.exe","/select,"+path); instead
#if UNITY_EDITOR
            EditorUtility.RevealInFinder(path);
#endif
        }

#if ENABLE_CONSOLE_UI
        bool hasChangedTab = false;
        GMScrollRect scrollRect;

        private void Update() {
            float hAxis = Input.GetAxis("HorizontalGamepad");

            if (!hasChangedTab && hAxis > 0f) {
                GMTab currentTab = uiViewTabbar.GetActiveTab();
                int nextTabIndex = (uiViewTabbar.GetIndexOfTab(currentTab) + 1) % uiViewTabbar.Tabs.Count;
                uiViewTabbar.ActivateTabByIndex(nextTabIndex);
                hasChangedTab = true;
            } else if (!hasChangedTab && hAxis < 0f) {
                GMTab currentTab = uiViewTabbar.GetActiveTab();
                int nextTabIndex = (uiViewTabbar.GetIndexOfTab(currentTab) - 1);
                if (nextTabIndex < 0) nextTabIndex = uiViewTabbar.Tabs.Count - 1;
                uiViewTabbar.ActivateTabByIndex(nextTabIndex);
                hasChangedTab = true;
            } else {
                hasChangedTab = false;
            }

            if (scrollRect == null) scrollRect = uiViewTabbar.GetComponentInParent<GMScrollRect>();
            else {
                scrollRect.content.localPosition = UIViewController.GetSnapToPositionToBringChildIntoView(scrollRect, uiViewTabbar.GetActiveTab().transform as RectTransform);
                scrollRect.content.offsetMax = scrollRect.content.offsetMin = Vector2.zero;
            }
        }
#endif

        void Refresh() {
            // TODO show progress-thingy
            _devUiService.LoadViews()
                .Finally(()=> {
                    // finally is called when the observable finished
                    // disable progress-thingy
                })
                .Subscribe(progress=> {
                    // set progress-thingy value
                    Debug.Log("Progress:" + progress);
                }).AddTo(this);
        }
    }
}
