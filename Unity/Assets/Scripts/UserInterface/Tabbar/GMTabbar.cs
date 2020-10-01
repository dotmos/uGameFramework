using System;
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
        public bool activateNewTabsOnAdd = true;
        public GMTab defaultTab;

        private List<GMTab> tabs = new List<GMTab>();
        public List<GMTab> Tabs => tabs;

        private GMTab activeTab;

        private List<Action> valueChangedListeners = new List<Action>();

        protected override void Start() {
            base.Start();

            //Register all tabs that are children of this tabbar
            foreach (GMTab tab in GetComponentsInChildren<GMTab>(true))
            {
                RegisterTab(tab);
            }

            if (!allowSwitchOff && !deactivateDefaultOnEnableBehaviour) ActivateCustomDefaultTab();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (!deactivateDefaultOnEnableBehaviour && !allowSwitchOff)
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

        public void DeactivateTab(GMTab tab) {
            if (activeTab == tab) {
                tab.isOn = false;
                activeTab = null;

                OnValueChanged();
            }
        }

        /// <summary>
        /// Returns 0 if tab is not registered
        /// </summary>
        /// <param name="tab"></param>
        /// <returns></returns>
        public int GetIndexOfTab(GMTab tab) {
            if (tabs.Contains(tab)) {
                return tabs.IndexOf(tab);
            } else {
                return 0;
            }
        }

        /// <summary>
        /// Adds a new tab to the tabbar.
        /// </summary>
        /// <param name="tab">Tab.</param>
        public void RegisterTab(GMTab tab)
        {
            if (!tabs.Contains(tab)) tabs.Add(tab);
            tab.Initialize(this);
            RegisterToggle(tab);

            if (activateNewTabsOnAdd) {
                ActivateTab(tab);
            }
        }

        public void RemoveTab(GMTab tab, bool destroy = false)
        {
            //activate tab in front
            int index = tabs.IndexOf(tab);

            if (index > 0 && tabs.Count > 0) {
                ActivateTabByIndex(index - 1);
            }

            //Remove tab
            tabs.Remove(tab);
            UnregisterToggle(tab);
            if (destroy) Destroy(tab.gameObject);
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
            OnValueChanged();
        }

        void OnValueChanged() {
            foreach(Action listener in valueChangedListeners) {
                listener();
            }
        }

        public void AddValueChangedListener(Action callback) {
            if (!valueChangedListeners.Contains(callback)) {
                valueChangedListeners.Add(callback);
            }
        }

        public void RemoveValueChangedListener(Action callback) {
            if (valueChangedListeners.Contains(callback)) {
                valueChangedListeners.Remove(callback);
            }
        }

        public void SetOrderForTab(GMTab tab, int index) {
            tab.transform.SetSiblingIndex(index);
            tabs.Remove(tab);
            tabs.Insert(Mathf.Clamp(index, 0, tabs.Count - 2), tab);
        }
    }
}
