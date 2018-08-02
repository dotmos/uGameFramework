using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using UniRx;
using Service.DevUIService;

namespace UserInterface {
    public class UIViewsManager : GameComponent {

        private int counter = 0;

        public GameObject uiViewTabPrefab;
        public GameObject uiViewPrefab;

        public Transform uiViewsContainer;
        public GMTabbar uiViewTabbar;

        [Inject]
        private Service.DevUIService.IDevUIService _devUiService;

        protected override void AfterBind() {
            base.AfterBind();

            _devUiService.GetRxViews().ObserveAdd().Subscribe(evt => {
                // spawn View
                var name = evt.Key;
                var view = evt.Value;
                SpawnUIView(view);
                
            }).AddTo(this);
        }

        public void CreateDummyViewByUI(string name) {
            if (name == null || _devUiService.ViewNameExists(name) ) {
                counter++;
                name = "view_" + counter;
            }
            _devUiService.AddView(name);
        }

        public void SpawnUIView(DevUIView devuiView) {
            GameObject uiViewTabGO = Instantiate(uiViewTabPrefab) as GameObject;
            uiViewTabGO.transform.SetParent(uiViewTabbar.transform, false);
            uiViewTabGO.name = "tab_" + devuiView.name;
            uiViewTabGO.GetComponentInChildren<Text>().text = devuiView.name.ToUpper();

            GameObject uiViewGO = Instantiate(uiViewPrefab) as GameObject;
            uiViewGO.transform.SetParent(uiViewsContainer, false);

            GMTab uiViewTab = uiViewTabGO.GetComponent<GMTab>();
            uiViewTab.content = uiViewGO;
            uiViewTabbar.RegisterTab(uiViewTab);
        }
	}
}
