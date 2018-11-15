using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UserInterface {
    public class GameDataScrollView : DataScrollView {
        //The data cells that were actually spawned and will be filled with data cell objects
        private List<List<GameDataCell>> dataRows = new List<List<GameDataCell>>();
        //The virtual rows that we put into the spawned cells on scrolling
        private List<List<DataCellObject>> data = new List<List<DataCellObject>>();

        [System.Serializable]
        public class DataCellObject {
            public CellType cellType;
            public object value;
            public object dropdownValue;
            public Action callback;

            public enum CellType { Output, EditableOutput, Dropdown };
        }

        protected override void Awake() {
            base.Awake();

            //Demo Data
            List<float> demoColumns = new List<float>() { 200, 200, 300, 500, 500 };
            List<List<DataCellObject>> demoData = new List<List<DataCellObject>>();

            for (int i = 0; i < 50; ++i) {
                List<DataCellObject> rowData = new List<DataCellObject>();

                for (int k = 0; k < demoColumns.Count; ++k) {
                    DataCellObject dataCellObject = new DataCellObject();
                    if (k == 0) {
                        dataCellObject.cellType = DataCellObject.CellType.Output;
                        dataCellObject.value = "Row " + i;
                        dataCellObject.callback = () => Debug.Log("Clicked column 0 in row " + i.ToString());
                    }

                    if (k == 1) {
                        dataCellObject.cellType = DataCellObject.CellType.EditableOutput;
                        dataCellObject.value = "Cell " + k;
                        dataCellObject.callback = () => Debug.Log("Edited column 1 in row " + i.ToString());
                    }

                    if (k == 2) {
                        dataCellObject.cellType = DataCellObject.CellType.Dropdown;
                        dataCellObject.value = new List<string>() { DataCellObject.CellType.Dropdown.ToString(), DataCellObject.CellType.EditableOutput.ToString(), DataCellObject.CellType.Output.ToString() };
                        dataCellObject.callback = () => Debug.Log("Changed dropdown value of column 2 in row " + i.ToString());
                    }

                    if (k == 3) {
                        dataCellObject.cellType = DataCellObject.CellType.Output;
                        dataCellObject.value = "Row " + i;
                        dataCellObject.callback = () => Debug.Log("Clicked column 0 in row " + i.ToString());
                    }

                    if (k == 4) {
                        dataCellObject.cellType = DataCellObject.CellType.Output;
                        dataCellObject.value = "Row " + i;
                        dataCellObject.callback = () => Debug.Log("Clicked column 0 in row " + i.ToString());
                    }

                    rowData.Add(dataCellObject);
                }

                demoData.Add(rowData);
            }

            data = demoData;

            Setup(demoColumns, demoData.Count);
            InitializeTable();
        }

        private void OnEnable() {
            InitializeTable();
        }

        protected override void InitializeTable() {
            base.InitializeTable();

            dataRows.Clear();
            dataRows = new List<List<GameDataCell>>();

            //Cache data rows with data cell component
            foreach (List<GameObject> dataRowObject in dataRowObjects) {
                List<GameDataCell> dataRow = new List<GameDataCell>();

                foreach (GameObject dataCellObject in dataRowObject) {
                    dataRow.Add(dataCellObject.GetComponent<GameDataCell>());
                }

                dataRows.Add(dataRow);
            }

            //Update scroll value
            OnScrollVertical(verticalScrollbar.value);
        }

        protected override void UpdateRowOutput() {
            base.UpdateRowOutput();

            for (int i = 0; i < dataRows.Count; ++i) {
                if (topDataIndex + i >= dataCount) break;

                for (int k = 0; k < data[topDataIndex + i].Count; ++k) {
                    dataRows[i][k].Initialize(data[topDataIndex + i][k]);
                }
            }
        }
    }
}
