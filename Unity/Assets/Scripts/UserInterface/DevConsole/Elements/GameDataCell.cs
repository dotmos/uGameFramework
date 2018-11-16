using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UserInterface {
    public class GameDataCell : MonoBehaviour {
        public GMInputField output;
        public GMButton button;
        public Dropdown dropdown;
        public Image cellBackground;

        public List<GameDataCellConfig> cellConfigs = new List<GameDataCellConfig>();

        [System.Serializable]
        public class GameDataCellConfig {
            public GameDataScrollView.DataCellObject.CellType type;
            public Color cellBackground;
        }

        GameDataScrollView.DataCellObject dataCellObject;

        public void Initialize(GameDataScrollView.DataCellObject dataCellObject) {
            this.dataCellObject = dataCellObject;

            switch (dataCellObject.cellType) {
                case GameDataScrollView.DataCellObject.CellType.Output:
                    SetOutput(dataCellObject.value==null?"":dataCellObject.value.ToString());
                    break;
                case GameDataScrollView.DataCellObject.CellType.EditableOutput:
                    SetOutput(dataCellObject.value == null ? "null" : dataCellObject.value.ToString(), true);
                    break;
                case GameDataScrollView.DataCellObject.CellType.Dropdown:
                    SetupDropdown(dataCellObject.dropdownValues as List<string>, dataCellObject.value==null?0:(int)dataCellObject.value);
                    break;
                case GameDataScrollView.DataCellObject.CellType.Header:
                    SetOutput(dataCellObject.value == null ? "" : dataCellObject.value.ToString().ToUpper());
                    break;
                case GameDataScrollView.DataCellObject.CellType.Button:
                    SetButton(dataCellObject.value == null ? "" : dataCellObject.value.ToString());
                    break;
                default:
                    break;
            }

            UpdateVisualsByType(dataCellObject.cellType);
        }

        void UpdateVisualsByType(GameDataScrollView.DataCellObject.CellType type) {
            GameDataCellConfig config = cellConfigs.Find(c => c.type == type);
            if (config != null) {
                cellBackground.color = config.cellBackground;
            }
        }

        void OnDropdownChanged(int index) {
            if (dataCellObject.callback == null) {
                return;
            }
            var val = dataCellObject.dropdownValues[index];
            dataCellObject.callback(val);
            dataCellObject.value = val;
        }

        void OnClick() {
            if (dataCellObject.callback == null) {
                return;
            }
            dataCellObject.callback(null);
        }

        void OnInputChanged(string newValue) {
            if (dataCellObject.callback == null) {
                return;
            }
            dataCellObject.callback(newValue);
            dataCellObject.value = newValue;
        }

        void SetButton(string str) {
            RemoveCallbacks();

            button.gameObject.SetActive(true);
            output.gameObject.SetActive(false);
            dropdown.gameObject.SetActive(false);

            button.GetComponentInChildren<Text>().text = str;
            button.onClick.AddListener(OnClick);
        }

        public void SetOutput(string str, bool editable = false) {
            RemoveCallbacks();

            output.text = str;
            output.targetGraphic.raycastTarget = editable;

            button.gameObject.SetActive(false);
            output.gameObject.SetActive(true);
            dropdown.gameObject.SetActive(false);

            if (editable) output.onEndEdit.AddListener(OnInputChanged);
        }

        public void SetupDropdown(List<string> dropdownValues, int currentValue) {
            RemoveCallbacks();

            dropdown.ClearOptions();
            List<Dropdown.OptionData> dataList = new List<Dropdown.OptionData>();

            for (int i = 0; i < dropdownValues.Count; ++i) {
                Dropdown.OptionData data = new Dropdown.OptionData();
                data.text = dropdownValues[i];
                dataList.Add(data);
            }

            dropdown.options = dataList;
            dropdown.value = currentValue;

            button.gameObject.SetActive(false);
            output.gameObject.SetActive(false);
            dropdown.gameObject.SetActive(true);

            dropdown.onValueChanged.AddListener(OnDropdownChanged);
        }


        void RemoveCallbacks() {
            button.onClick.RemoveListener(OnClick);
            output.onEndEdit.RemoveListener(OnInputChanged);
            dropdown.onValueChanged.RemoveListener(OnDropdownChanged);
        }
    }
}
