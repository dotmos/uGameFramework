using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Service.DevUIService;
using UnityEngine.UI;
using UniRx;
using System.Linq;

namespace UserInterface {
    public class GameDataTopLevelNavigation : GameComponent {
        public GMTabbar tabbar;
        public GMTab tabTemplate;

        Commands.GetDataBrowserTopLevelElementsCommand getTopLevels = new Commands.GetDataBrowserTopLevelElementsCommand();

        Dictionary<DataBrowserTopLevel, GMTab> tabs = new Dictionary<DataBrowserTopLevel, GMTab>();

        protected override void AfterBind() {
            base.AfterBind();

            tabbar.AddValueChangedListener(ChangeDataTable);

            this.OnEvent<Service.DevUIService.Events.ScriptingConsoleOpened>().Subscribe(e => { Initialize(); }).AddTo(this);

            tabTemplate.gameObject.SetActive(false);
        }

        void SpawnTopLevelTab(DataBrowserTopLevel topLevel) {
            GMTab tab = Instantiate(tabTemplate) as GMTab;
            tab.GetComponentInChildren<Text>().text = topLevel.topLevelName;
            tab.transform.SetParent(tabbar.transform, false);
            if (!tab.gameObject.activeSelf) tab.gameObject.SetActive(true);
            tabbar.RegisterTab(tab);

            tabs.Add(topLevel, tab);
        }

        void Initialize() {
            //Get top levels
            this.Publish(getTopLevels);

            List<DataBrowserTopLevel> tabsToRemove = new List<DataBrowserTopLevel>();

            //Destroy unneeded toplevels
            foreach(KeyValuePair<DataBrowserTopLevel, GMTab> tab in tabs) {
                if (!getTopLevels.result.Contains(tab.Key)) {
                    tabbar.RemoveTab(tab.Value, true);
                    tabsToRemove.Add(tab.Key);
                }
            }

            //Remove from list
            foreach(DataBrowserTopLevel topLevel in tabsToRemove) {
                tabs.Remove(topLevel);
            }

            //Spawn missing
            foreach (DataBrowserTopLevel topLevel in getTopLevels.result) {
                if (!tabs.ContainsKey(topLevel)) {
                    SpawnTopLevelTab(topLevel);
                }
            }

            tabbar.ActivateFirstTab();

            tabsToRemove.Clear();
        }

        void ChangeDataTable() {
            //Get data by active tab
            DataBrowserTopLevel topLevel = tabs.FirstOrDefault(t => t.Value == tabbar.GetActiveTab()).Key;
            if (topLevel != null) {
                this.Publish(new Service.DevUIService.Events.NewDataTable() {
                    // since this is a single object and the DataBrowser is meant for lists, wrap the object in a list
                    objectList = topLevel.objectList,
                    tableTitle = topLevel.topLevelName,
                    history = new List<HistoryElement>()
                });
            }
        }
    }
}
