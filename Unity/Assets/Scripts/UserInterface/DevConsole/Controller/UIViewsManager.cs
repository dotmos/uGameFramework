using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using UniRx;
using Service.DevUIService;
using System.Linq;

namespace UserInterface {
    public class UIViewsManager : GameComponent {

        public GameObject uiViewTabPrefab;
        public GameObject uiViewPrefab;
        public GameObject uiButtonPrefab;
        public GameObject uiLUAButtonPrefab;
        [Space]
        public Transform uiViewsContainer;
        public GMTabbar uiViewTabbar;
        [Space]
        public GMButton addLuaButton;

        [Inject]
        private Service.DevUIService.IDevUIService _devUiService;

        private Dictionary<DevUIView, GMTab> uiViews = new Dictionary<DevUIView, GMTab>();

        protected override void AfterBind() {
            base.AfterBind();

            //Load existing
            ReactiveDictionary<string, DevUIView> devUIViews = _devUiService.GetRxViews();
            
            foreach(KeyValuePair<string, DevUIView> uiView in devUIViews) {
                SpawnUIView(uiView.Value);
            }

            //listen to new
            _devUiService.GetRxViews().ObserveAdd().Subscribe(evt => {
                // spawn View
                var name = evt.Key;
                var view = evt.Value;
                SpawnUIView(view);

                if (uiViews.Count > 0) addLuaButton.interactable = true;
                else addLuaButton.interactable = false;
            }).AddTo(this);

            addLuaButton.onClick.AddListener(AddLuaButton);
        }

        /// <summary>
        /// Creates a default UI view (triggered from ui button)
        /// </summary>
        /// <param name="name">(optional) A name for the ui View.</param>
        public void CreateDefaultUIView(string name) {
            if (name == null || _devUiService.ViewNameExists(name)) {
                name = "view_" + uiViews.Count;
            }
            _devUiService.AddView(name);
        }

        void AddLuaButton() {
            DevUILUAButton newButton = new DevUILUAButton("New Command", "print('Empty Command')");
            GMTab activeTab = uiViewTabbar.GetActiveTab();
            DevUIView uiView = uiViews.FirstOrDefault(u => u.Value == activeTab).Key;

            if (uiView != null) {
                SpawnLuaButton(newButton, uiViews[uiView].content.GetComponent<GMScrollRect>().content, true);
            } else {
                Debug.LogWarning("Couldn't create lua button because no active uiView could be found.");
            }
        }

        /// <summary>
        /// Sets up a new ui view and adds ui elements if there are any
        /// </summary>
        /// <param name="devUIView">The dev UI view data</param>
        public void SpawnUIView(DevUIView devUIView) {
            //Spawn 
            GameObject uiViewTabGO = Instantiate(uiViewTabPrefab) as GameObject;
            uiViewTabGO.transform.SetParent(uiViewTabbar.transform, false);
            uiViewTabGO.name = "tab_" + devUIView.name;
            uiViewTabGO.GetComponentInChildren<Text>().text = devUIView.name.ToUpper();

            GameObject uiViewGO = Instantiate(uiViewPrefab) as GameObject;
            uiViewGO.transform.SetParent(uiViewsContainer, false);

            GMTab uiViewTab = uiViewTabGO.GetComponent<GMTab>();
            uiViewTab.content = uiViewGO;
            uiViewTabbar.RegisterTab(uiViewTab);

            uiViews.Add(devUIView, uiViewTab);

            //Setup ui elements
            foreach(DevUIElement uiElement in devUIView.uiElements) {
                SpawnUIElement(uiElement, devUIView);
            }
        }

        void SpawnUIElement(DevUIElement devUIElement, DevUIView view) {
            Transform container = uiViews[view].content.GetComponent<GMScrollRect>().content;

            //Spawn and initialize by type
            if (devUIElement is DevUILUAButton) {
                SpawnLuaButton(devUIElement as DevUILUAButton, container, false);
            } else if (devUIElement is DevUIButton) {
                SpawnButton(devUIElement as DevUIButton, container);
            } else {
                Debug.LogWarning("Tried to spawn a UI element that has no prefab for view '" + view.name + "'.");
            }
        }

        void SpawnLuaButton(DevUILUAButton luaButton, Transform container, bool activateInEditMode) {
            GameObject devUIElementGO = Instantiate(uiLUAButtonPrefab) as GameObject;
            UIViewLUAButton button = devUIElementGO.GetComponent<UIViewLUAButton>();
            button.Initialize(luaButton.name, luaButton.Execute, luaButton);
            devUIElementGO.transform.SetParent(container, false);

            button.ActivateEditMode(activateInEditMode);
        }

        void SpawnButton(DevUIButton devButton, Transform container) {
            GameObject devUIElementGO = Instantiate(uiButtonPrefab) as GameObject;
            UIViewButton button = devUIElementGO.GetComponent<UIViewButton>();
            button.Initialize(devButton.name, devButton.Execute);
            devUIElementGO.transform.SetParent(container, false);
        }

    }
}
