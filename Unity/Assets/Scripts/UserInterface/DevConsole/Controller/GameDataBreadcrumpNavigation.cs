using Service.DevUIService;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace UserInterface {
    public class GameDataBreadcrumpNavigation : GameComponent {
        public GMButton breadcrumpTemplate;
        public Text currentTitleOutput;

        private Dictionary<GMButton, HistoryElement> breadcrumps = new Dictionary<GMButton, HistoryElement>();
        private HistoryElement historyElement;

        protected override void AfterBind() {
            base.AfterBind();

            breadcrumpTemplate.gameObject.SetActive(false);
            this.OnEvent<Events.NewDataTable>().Subscribe(e => CreateBreadcrumps(e)).AddTo(this);
        }

        void CreateBreadcrumps(Events.NewDataTable dataTable) {
            //Clear old
            ClearBreadcrumps();

            //Set title of current sheet
            SetCurrentTableName(dataTable.tableTitle);

            //Create breadcrumps
            if (dataTable.history == null) return;

            foreach (HistoryElement history in dataTable.history) {
                SpawnBreadcrump(history);
            }

            //Move current title to end of breadcrumps
            currentTitleOutput.transform.parent.SetAsLastSibling();
        }

        void SpawnBreadcrump(HistoryElement historyElement) {
            GMButton breadcrump = Instantiate(breadcrumpTemplate);
            breadcrump.transform.SetParent(transform, false);
            if (!breadcrump.gameObject.activeSelf) breadcrump.gameObject.SetActive(true);
            breadcrump.GetComponentInChildren<Text>().text = historyElement.historyTitle;
            breadcrump.onClick.AddListener(
                () => OpenBreadcrump(historyElement)
            );

            breadcrumps.Add(breadcrump, historyElement);
        }

        void SetCurrentTableName(string name) {
            currentTitleOutput.text = name;
        }
        
        void ClearBreadcrumps() {
            foreach(GMButton breadcrump in breadcrumps.Keys) {
                Destroy(breadcrump.gameObject);
            }

            breadcrumps.Clear();
        }

        void OpenBreadcrump(HistoryElement history) {
            this.Publish(new Service.DevUIService.Events.NewDataTable() {
                // since this is a single object and the DataBrowser is meant for lists, wrap the object in a list
                objectList = history.objectList,
                tableTitle = history.historyTitle,
            });
        }
	}
}
