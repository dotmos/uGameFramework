using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UserInterface {
    public class UIViewsManager : MonoBehaviour {
        public GameObject uiViewTabPrefab;
        public GameObject uiViewPrefab;

        public Transform uiViewsContainer;
        public GMTabbar uiViewTabbar;

        public void SpawnUIView(string name = "new") {
            GameObject uiViewTabGO = Instantiate(uiViewTabPrefab) as GameObject;
            uiViewTabGO.transform.SetParent(uiViewTabbar.transform, false);
            uiViewTabGO.name = "tab_" + name;
            uiViewTabGO.GetComponentInChildren<Text>().text = name.ToUpper();

            GameObject uiViewGO = Instantiate(uiViewPrefab) as GameObject;
            uiViewGO.transform.SetParent(uiViewsContainer, false);

            GMTab uiViewTab = uiViewTabGO.GetComponent<GMTab>();
            uiViewTab.content = uiViewGO;
            uiViewTabbar.RegisterTab(uiViewTab);
        }
	}
}
