using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Service.MemoryBrowserService;
using System.Linq;
using Service.DevUIService;

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
            public List<string> dropdownValues;
            public Action<string> callback;

            public enum CellType { Output, EditableOutput, Dropdown };
        }

        [Serializable]
        public class ColumnDefinition {
            public float width = 100;
            public string colName = null;
        }

        [Serializable]
        public class DataTable {
            public List<object> history = null;
            public List<ColumnDefinition> columnDef=new List<ColumnDefinition>();
            public List<List<DataCellObject>> rows = new List<List<DataCellObject>>();
        }


        private void SetupData(DataTable dataTable) {
            // TODO:
            // ClearTable();
            data = dataTable.rows;
            Setup(dataTable.columnDef, dataTable.rows.Count);
            InitializeTable();
        }

        /// <summary>
        /// Method to create a DataTable to be visualized
        /// </summary>
        /// <param name="list"></param>
        /// <param name="history">history as breadcrumbs to jump back. root objects don't need one</param>
        /// <returns></returns>
        private DataTable CreateDataTableFromList(String title,System.Collections.IList list, List<HistoryElement> history=null) {
            if (list.Count == 0) {
                return null;
            }

            var dataTable = new DataTable();

            // root object? init history
            if (history == null) {
                history = new List<HistoryElement>();
            }

            // create one flag that can be checked in the rows-loop
            List<DataCellObject> titleRow = new List<DataCellObject>();
            dataTable.rows.Add(titleRow);

            bool addBackButton = history != null && history.Count > 0;
            bool addTitle = true;

            

            // fake-headers-row:
            List<DataCellObject> header = new List<DataCellObject>();
            dataTable.rows.Add(header);
            for (int rowNr=0; rowNr < list.Count; rowNr++) {
                var rowObj = list[rowNr];

                List<DataCellObject> rowData = new List<DataCellObject>();

                DataBrowser.Traverse(rowObj, (varName, varObj, type, meta) => {
                    if (rowNr == 0) {
                        // define the columns in the first row
                        var headerElement = new ColumnDefinition() {
                            width = meta == null ? 100.0f : meta.width,
                            colName = (meta == null || meta.visualName == null) ? varName : meta.visualName
                        };
                        dataTable.columnDef.Add(headerElement);

                        // --------fake headers------------
                        header.Add(new DataCellObject() {
                            value = headerElement.colName,
                            cellType = DataCellObject.CellType.Output,
                            callback = (st) => { Debug.Log("Pressed the header:" + headerElement.colName); }
                        });
                        // --------------------------------

                        if (addBackButton) {
                            // --------- back-button-column ------------------------
                            addBackButton = false;
                            titleRow.Add(new DataCellObject() {
                                value = "BACK",
                                cellType = DataCellObject.CellType.Output,
                                callback = (st) => {
                                    var backObject = history[0];
                                    history.RemoveAt(0);
                                    _eventService.Publish(new Service.DevUIService.Events.NewDataTable {
                                        objectList = backObject.objectList,
                                        tableTitle = backObject.historyTitle,
                                        history = history
                                    });
                                }
                            });
                        } else if (addTitle) {
                            // ------------ title - column -----------------------------
                            addTitle = false;
                            titleRow.Add(new DataCellObject() {
                                value = title==null?"":title,
                                cellType = DataCellObject.CellType.Output,
                                callback = (st) => {
                                    Debug.Log("You clicked the title");
                                }
                            });
                        } else {
                            titleRow.Add(new DataCellObject() {
                                value="",cellType=DataCellObject.CellType.Output
                            });
                        }
                    }

                    if ( varObj==null || MemoryBrowser.IsSimple(varObj.GetType())||varObj is Vector2||varObj is Vector3) {
                        // SIMPLE TYPE-HANDLING (int,string,enums,bool,...)
                        var rowElem = new DataCellObject() {
                            value = varObj,
                            callback = (newVal) => {
                                if (newVal == null) {
                                    // selection of readonly-cell
                                    return;
                                }
                                // try to set the value of this field/property
                                bool successful = DataBrowser.SetValue(rowObj, varName, newVal);
                                Debug.Log("Setting var " + varName + " to new Value:" + newVal + " [" + (successful ? "success" : "fail") + "]");
                            }

                        };

                        var onlyread = varObj==null || (meta != null && meta.type == DataBrowser.UIDBInclude.Type.onlyread);

                        if (onlyread) {
                            rowElem.cellType = DataCellObject.CellType.Output;
                        } else {
                            if (varObj is Enum) {
                                rowElem.cellType = DataCellObject.CellType.Dropdown;
                                rowElem.dropdownValues = new List<string>(Enum.GetNames(varObj.GetType()));
                                rowElem.value = (int)varObj;
                            } else if (varObj.GetType() == typeof(bool)) {
                                rowElem.cellType = DataCellObject.CellType.Dropdown;
                                rowElem.dropdownValues = new List<string> { "false", "true" };
                                rowElem.value = ((bool)varObj) ? 0 : 1;
                            } else {
                                rowElem.cellType = DataCellObject.CellType.EditableOutput;
                            }
                        }
                        rowData.Add(rowElem);
                    } else {
                        // NOT SIMPLE TYPES ( Object-Instances, List, Dict...)
                        if (type == MemoryBrowser.ElementType.objectType) {

                            // REFERENCE - LINK : OBJECT

                            var rowElem = new DataCellObject() {
                                value = "LINK",
                                callback = (newVal) => {
                                    Debug.Log("Go to REF:"+varObj.ToString());
                                    history.Add(new HistoryElement() { historyTitle = title, objectList = list });
                                    _eventService.Publish(new Service.DevUIService.Events.NewDataTable() {
                                        // since this is a single object and the DataBrowser is meant for lists, wrap the object in a list
                                        objectList = new ArrayList() { varObj },
                                        history = history,
                                        tableTitle = varName+":"+varObj.GetType().Name
                                    });
                                    return;
                                },
                                cellType = DataCellObject.CellType.Output
                            };
                            rowData.Add(rowElem);
                        }
                        else if (type == MemoryBrowser.ElementType.listType) {

                            // REFERENCE-LINK: LIST

                            var theList = (IList)varObj;
                            var rowElem = new DataCellObject() {
                                value = "List[" + theList.Count + "]",
                                callback = (newVal) => {
                                    Debug.Log("Cannot go into empty lists");

                                    if (theList.Count == 0) {
                                        //
                                        return;
                                    }
                                    history.Add(new HistoryElement() { historyTitle = title, objectList = list });
                                    _eventService.Publish(new Service.DevUIService.Events.NewDataTable() {
                                        // since this is a single object and the DataBrowser is meant for lists, wrap the object in a list
                                        objectList =  (IList)varObj,
                                        history = history,
                                        tableTitle = varName + ":" + varObj.GetType().Name
                                    });
                                    return;
                                },
                                cellType = DataCellObject.CellType.Output
                            };
                            rowData.Add(rowElem);
                        }
                    }

                });
                dataTable.rows.Add(rowData);
            }
            return dataTable;
        }

        protected override void AfterBind() {
            base.AfterBind();

            // list for events to show new data
            _eventService.OnEvent<Service.DevUIService.Events.NewDataTable>().Subscribe(data => {
                // TODO: Clear the table?
                var dataTable = CreateDataTableFromList(data.tableTitle,data.objectList);
                SetupData(dataTable);
            }).AddTo(this);

            //var dataTable = new DataTable();

            ////Demo Data
            //dataTable.columnDef = new List<float>() { 200, 200, 300 };
            

            //for (int i = 0; i < 25; ++i) {
            //    List<DataCellObject> rowData = new List<DataCellObject>();

            //    for (int k = 0; k < dataTable.columnDef.Count; ++k) {
            //        DataCellObject dataCellObject = new DataCellObject();
            //        if (k == 0) {
            //            dataCellObject.cellType = DataCellObject.CellType.Output;
            //            dataCellObject.value = "Row " + i;
            //            dataCellObject.callback = () => Debug.Log("Clicked column 0 in row " + i.ToString());
            //        }

            //        if (k == 1) {
            //            dataCellObject.cellType = DataCellObject.CellType.EditableOutput;
            //            dataCellObject.value = "Cell " + k;
            //            dataCellObject.callback = () => Debug.Log("Edited column 1 in row " + i.ToString());
            //        }

            //        if (k == 2) {
            //            dataCellObject.cellType = DataCellObject.CellType.Dropdown;
            //            dataCellObject.value = new List<string>() { DataCellObject.CellType.Dropdown.ToString(), DataCellObject.CellType.EditableOutput.ToString(), DataCellObject.CellType.Output.ToString() };
            //            dataCellObject.callback = () => Debug.Log("Changed dropdown value of column 2 in row " + i.ToString());
            //        }

            //        if (k == 3) {
            //            dataCellObject.cellType = DataCellObject.CellType.Output;
            //            dataCellObject.value = "Row " + i;
            //            dataCellObject.callback = () => Debug.Log("Clicked column 0 in row " + i.ToString());
            //        }

            //        if (k == 4) {
            //            dataCellObject.cellType = DataCellObject.CellType.Output;
            //            dataCellObject.value = "Row " + i;
            //            dataCellObject.callback = () => Debug.Log("Clicked column 0 in row " + i.ToString());
            //        }

            //        rowData.Add(dataCellObject);
            //    }

            //    dataTable.rows.Add(rowData);
            //}

      //      SetupData(dataTable);
        }

        protected override void OnEnable() {
            base.OnEnable();

            if (isReady) InitializeTable();
        }

        protected override void OnDisable() {
            base.OnDisable();

            for (int i = 0; i < dataRows.Count; ++i) {
                for(int c = 0; c < dataRows[i].Count; ++c) {
                    Destroy(dataRows[i][c].gameObject);
                    Destroy(rowObjects[i][c].gameObject);
                }
            }

            dataRows.Clear();
            rowObjects.Clear();
        }


        //Cell data output happens here, when a  row is added
        protected override void OnRowAdded(List<GameObject> row, int rowIndex) {
            base.OnRowAdded(row, rowIndex);

            List<GameDataCell> dataRow = new List<GameDataCell>();

            for (int i = 0; i < row.Count; ++i) {
                GameDataCell cell = row[i].GetComponent<GameDataCell>();
                dataRow.Add(cell);

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
            try {
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
            catch (Exception e) {
                Debug.LogException(e);
            }
        }
    }
}
