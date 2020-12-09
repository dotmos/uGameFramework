using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Service.MemoryBrowserService;
using System.Linq;
using Service.DevUIService;
using System.Text;
using Zenject;

namespace UserInterface {
    public class GameDataScrollView : DataScrollView {
        //The data cells that were actually spawned and will be filled with data cell objects
        private List<List<GameDataCell>> dataRows = new List<List<GameDataCell>>();
        //The virtual rows that we put into the spawned cells on scrolling
        private List<List<DataCellObject>> data = new List<List<DataCellObject>>();

        [Inject]
        private Service.DevUIService.IDevUIService devui;

        [System.Serializable]
        public class DataCellObject {
            public CellType cellType;
            public object value;
            public List<string> dropdownValues;
            public Action<string> callback;

            public enum CellType { Output, EditableOutput, Dropdown, Button, Header, Subdata,Empty };
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
            ClearTable();
            if (dataTable == null) {
                return;
            }
            data = dataTable.rows;
            Setup(dataTable.columnDef, dataTable.rows.Count);
            InitializeTable();
        }

        public static string OutputListAsStrings(IList theList) {
            StringBuilder stb = new StringBuilder();
            for (int i = 0; i < theList.Count; i++) {
                if (i > 0) {
                    stb.Append(",");
                }
                stb.Append(theList[i].ToString());
            }
            return stb.ToString();
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

            DataTable dataTable = new DataTable();

            // root object? init history
            if (history == null) {
                history = new List<HistoryElement>();
            }


            bool addBackButton = history != null && history.Count > 0;

            // fake-headers-row:
            List<DataCellObject> header = new List<DataCellObject>();
            dataTable.rows.Add(header);
            int rowAmount = Mathf.Min(list.Count, 50);
            for (int rowNr=0; rowNr < rowAmount; rowNr++) {
                object rowObject = list[rowNr];
                // try to convert this object if there is an converter registered(e.g. UID)
                rowObject = devui.DataBrowserConvertObject(rowObject);
                List<DataCellObject> rowDataList = new List<DataCellObject>();

                Action<object, List<DataCellObject>> traverseObj = null;
                traverseObj = (rowObj,rowData) => {
                    DataBrowser.Traverse(rowObj, (varName, varObj, type, meta) => {
#if UNITY_EDITOR
                        //Debug.Log(varName + " : " + varObj);
#endif
                        if (rowNr == 0) {
                            // define the columns in the first row
                            ColumnDefinition headerElement = new ColumnDefinition() {
                                width = meta == null ? 100.0f : meta.width,
                                colName = (meta == null || meta.visualName == null) ? varName : meta.visualName
                            };

                            dataTable.columnDef.Add(headerElement);

                            // --------fake headers------------
                            header.Add(new DataCellObject() {
                                value = headerElement.colName,
                                cellType = DataCellObject.CellType.Header,
                                callback = (st) => { Debug.Log("Pressed the header:" + headerElement.colName); }
                            });
                            // --------------------------------
                        }

                        // traverse another list and add the whole data in one DataCellObject
                        /*if (meta!=null && meta.type == DataBrowser.UIDBInclude.Type.subdata) {
                            if (type == MemoryBrowser.ElementType.listType) {
                                var subList = (IList)varObj;
                                List<List<DataCellObject>> subdata = new List<List<DataCellObject>>();
                                for (int i = 0; i < subList.Count; i++) {
                                    var subListElem = subList[i];
                                    List<DataCellObject> subRowData = new List<DataCellObject>();
                                    traverseObj(subListElem, subRowData);
                                    subdata.Add(subRowData);
                                }
                                var rowElem = new DataCellObject() {
                                    cellType = DataCellObject.CellType.Subdata,
                                    value = subdata
                                };
                                rowData.Add(rowElem);
                            }
                        }
                        else */
                        if (varObj == null || MemoryBrowser.IsSimple(varObj.GetType()) || varObj is Vector2 || varObj is Vector3) {
                            // SIMPLE TYPE-HANDLING (int,string,enums,bool,...)
                            DataCellObject rowElem = new DataCellObject() {
                                value = varObj,
                                callback = (newVal) => {
                                    if (newVal == null) {
                                        // selection of readonly-cell
                                        return;
                                    }
                                    // try to set the value of this field/property
                                    bool successful = DataBrowser.SetValue(rowObj, varName, newVal);
#if UNITY_EDITOR
                                   // Debug.Log("Setting var " + varName + " to new Value:" + newVal + " [" + (successful ? "success" : "fail") + "]");
#endif
                                }

                            };

                            bool onlyread = varObj == null || (meta != null && meta.type == DataBrowser.UIDBInclude.Type.onlyread);

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
                                    rowElem.value = ((bool)varObj) ? 1 : 0;
                                } else {
                                    rowElem.cellType = DataCellObject.CellType.EditableOutput;
                                }
                            }
                            rowData.Add(rowElem);
                        } else {
                            // NOT SIMPLE TYPES ( Object-Instances, List, Dict...)
                            if (type == MemoryBrowser.ElementType.objectType || type == MemoryBrowser.ElementType.dictType) {

                                // REFERENCE - LINK : OBJECT
                                string cellTitle = (meta != null && meta.type == DataBrowser.UIDBInclude.Type.subdata)
                                                ? "( "+(varObj.ToString() )+" )"
                                                : "LINK";
                                DataCellObject rowElem = new DataCellObject() {
                                    value = cellTitle,
                                    callback = (newVal) => {
                                        ArrayList objList = new ArrayList() { varObj };
#if UNITY_EDITOR
                                        Debug.Log("Go to REF:" + varObj.ToString());
#endif
                                        history.Add(new HistoryElement() { historyTitle = title, objectList = list });
                                        _eventService.Publish(new Service.DevUIService.Events.NewDataTable() {
                                            // since this is a single object and the DataBrowser is meant for lists, wrap the object in a list
                                            objectList = objList,
                                            history = history,
                                            tableTitle = varName + ":" + varObj.GetType().Name
                                        });
                                        return;
                                    },
                                    cellType = DataCellObject.CellType.Button
                                };
                                rowData.Add(rowElem);
                            } else if (type == MemoryBrowser.ElementType.listType) {

                                // REFERENCE-LINK: LIST
                                IList theList = (IList)varObj;



                                string cellTitle = (meta != null && meta.type == DataBrowser.UIDBInclude.Type.subdata)
                                                ? "( "+( OutputListAsStrings(theList) )+" )"
                                                : "List[" + theList.Count + "]";


                                DataCellObject rowElem = new DataCellObject() {
                                    value = cellTitle,
                                    callback = (newVal) => {
#if UNITY_EDITOR
                                        Debug.Log("Cannot go into empty lists");
#endif
                                        if (theList.Count == 0) {
                                            //
                                            return;
                                        }
                                        history.Add(new HistoryElement() { historyTitle = title, objectList = list });
                                        _eventService.Publish(new Service.DevUIService.Events.NewDataTable() {
                                            // since this is a single object and the DataBrowser is meant for lists, wrap the object in a list
                                            objectList = new List<object>() { (IList)varObj },
                                            history = history,
                                            tableTitle = varName + ":" + varObj.GetType().Name
                                        });
                                        return;
                                    },
                                    cellType = DataCellObject.CellType.Button
                                };
                                rowData.Add(rowElem);
                            }
                        }
                    });

                };
                traverseObj(rowObject,rowDataList);

                Func<int,List<DataCellObject>> CreateEmptyLine = (amount)=>{
                    List<DataCellObject> result = new List<DataCellObject>(amount);
                    for (int i = 0; i < amount; i++) {
                        result.Add(new DataCellObject() {
                            cellType = DataCellObject.CellType.Empty
                        });
                    }
                    return result;
                };

                if (rowDataList.Count == 0) {
                    dataTable.rows.Add(CreateEmptyLine(10));
                } else {
                    dataTable.rows.Add(rowDataList);
                }
            }
            return dataTable;
        }

        

        protected override void AfterBind() {
            base.AfterBind();

            // list for events to show new data
            _eventService.OnEvent<Service.DevUIService.Events.NewDataTable>().Subscribe(data => {
                // TODO: Clear the table?
                DataTable dataTable = CreateDataTableFromList(data.tableTitle,data.objectList);
                SetupData(dataTable);
            }).AddTo(this);

        }

        protected override void OnEnable() {
            base.OnEnable();

            if (isReady) InitializeTable();
        }

        protected override void OnDisable() {
            base.OnDisable();

            ClearTable();
        }

        void ClearTable() {
            for (int i = 0; i < dataRows.Count; ++i) {
                for (int c = 0; c < dataRows[i].Count; ++c) {
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
