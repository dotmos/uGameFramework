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
            List<float> demoColumns = new List<float>() { 200, 200, 300 };
            List<List<DataCellObject>> demoData = new List<List<DataCellObject>>();

            for (int i = 0; i < 25; ++i) {
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
            if (isReady) InitializeTable();
        }

        //Cell data output happens here, when a  row is added
        protected override void OnRowAdded(List<GameObject> row, int rowIndex) {
            base.OnRowAdded(row, rowIndex);

            List<GameDataCell> dataRow = new List<GameDataCell>();

            for (int i = 0; i < row.Count; ++i) {
                GameDataCell cell = row[i].GetComponent<GameDataCell>();
                dataRow.Add(cell);

                Debug.Log("Topdataindex: " + topDataIndex + ", rowIndex" + rowIndex);

                //Fill row with data if we have data for this data index
                if (topDataIndex + rowIndex < dataCount) {
                    cell.Initialize(data[topDataIndex + rowIndex][i]);
                    if (!cell.gameObject.activeSelf) cell.gameObject.SetActive(true);
                } else {
                    cell.gameObject.SetActive(false);
                }
            }

            dataRows.Add(dataRow);
        }

        protected override void OnRowRemoved(int rowIndex) {
            base.OnRowRemoved(rowIndex);

            dataRows.RemoveAt(rowIndex);
        }

        protected override void OnTopDataIndexChanged(int newIndex) {
            base.OnTopDataIndexChanged(newIndex);

            for (int i = 0; i < rowCount; ++i) {
                if (topDataIndex + i < dataCount) {
                    for (int k = 0; k < data[topDataIndex + i].Count; ++k) {
                        GameDataCell dataCell = dataRows[i][k];
                        dataCell.Initialize(data[topDataIndex + i][k]);
                        if (!dataCell.gameObject.activeSelf) dataCell.gameObject.SetActive(true);
                    }
                } else {
                    foreach (GameObject dataCell in rowObjects[i]) {
                        dataCell.SetActive(false);
                    }
                }
            }
        }
    }
}
