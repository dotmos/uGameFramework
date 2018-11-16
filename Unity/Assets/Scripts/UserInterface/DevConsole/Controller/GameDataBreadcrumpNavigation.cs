using Service.DevUIService;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.Linq;

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
            //Set title of current sheet
            SetCurrentTableName(dataTable.tableTitle);

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
        
        public void ClearBreadcrumps() {
            foreach(GMButton breadcrump in breadcrumps.Keys) {
                Destroy(breadcrump.gameObject);
            }

            breadcrumps.Clear();
        }

        void OpenBreadcrump(HistoryElement history) {
            //Destroy all breadcrumps after the one clicked
            GMButton button = breadcrumps.FirstOrDefault(b => b.Value == history).Key;
            if (button != null) {
                int childIndex = button.transform.GetSiblingIndex();

                for (int i = transform.childCount - 1; i >= childIndex; --i) {
                    Transform breadcrump = transform.GetChild(i);
                    GMButton _button = breadcrump.GetComponent<GMButton>();

                    if (_button != null) {
                        breadcrumps.Remove(_button);
                        Destroy(_button.gameObject);
                    }
                }
            }

            this.Publish(new Service.DevUIService.Events.NewDataTable() {
                // since this is a single object and the DataBrowser is meant for lists, wrap the object in a list
                objectList = history.objectList,
                tableTitle = history.historyTitle,
            });
        }
	}
}
