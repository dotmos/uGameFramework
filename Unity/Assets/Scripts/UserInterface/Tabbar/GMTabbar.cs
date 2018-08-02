using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UserInterface {
    [AddComponentMenu(NamingHelper.Tabbar.Name, 3)]
    public class GMTabbar : ToggleGroup {
        /// <summary>
        /// If activated the tabbar checks if more than one tab is currently open and then automatically opens the 
        /// first or - if set - default tab. This should be deactivated if the behaviour is handled from outside.
        /// </summary>
        public bool deactivateDefaultOnEnableBehaviour;
        public GMTab defaultTab;

        private List<GMTab> tabs = new List<GMTab>();

        private GMTab activeTab;

        protected override void Start() {
            base.Start();

            //Register all tabs that are children of this tabbar
            foreach (GMTab tab in GetComponentsInChildren<GMTab>(true))
            {
                RegisterTab(tab);
            }

            ActivateCustomDefaultTab();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (!deactivateDefaultOnEnableBehaviour)
            {
                //activate custom or first tab
                ActivateCustomDefaultTab();
            }
        }

        public void ActivateFirstTab() {
            if (tabs.Count > 0) {
                GMTab firstTab = tabs.FirstOrDefault();

                if (firstTab != null)
                {
                    ActivateTab(firstTab);
                }
                else
                {
                    ActivateTab(tabs[0]);
                }
            }
        }

        public void ActivateCustomDefaultTab() {
            if (defaultTab != null) {
                if (tabs.Contains(defaultTab)) {
                    ActivateTab(defaultTab);
                } else {
                    ActivateFirstTab();
                }
            } else {
                ActivateFirstTab();
            }
        }

        public void ActivateLastTab() {
            if (tabs.Count > 0) {
                ActivateTab(tabs[tabs.Count - 1]);
            }
        }

        /// <summary>
        /// Activates the index of the tab by. Used by management view
        /// </summary>
        /// <param name="index">Index.</param>
        public void ActivateTabByIndex(int index) {
            GMTab tab = tabs[index];

            if (tab != null) {
                ActivateTab(tab);
            } else {
                ActivateFirstTab();
            }
        }

        public void ActivateTab(GMTab tab) {
            //Deactivate all tabs and activate new tab
            foreach (GMTab _tab in tabs) {
                if (_tab == tab) {
                    _tab.isOn = true;
                } else {
                    _tab.isOn = false;
                }
            }
        }

        /// <summary>
        /// Adds a new tab to the tabbar.
        /// </summary>
        /// <param name="tab">Tab.</param>
        public void RegisterTab(GMTab tab)
        {
            tabs.Add(tab);
            tab.Initialize(this);
            RegisterToggle(tab);
        }

        public void RemoveTab(GMTab tab)
        {
            tabs.Remove(tab);
        }

        public GMTab GetActiveTab()
        {
            return activeTab;
        }

        /// <summary>
        /// This method should only be used by a Tab to register itself as active.
        /// </summary>
        public void SetAsActiveTab(GMTab tab)
        {
            activeTab = tab;
        }
    }
}
