using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UserInterface {
    public class GameDataCell : MonoBehaviour, IPointerClickHandler {
        public GMInputField output;
        public Dropdown dropdown;

        private bool isClickable;

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
                default:
                    break;
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

        public void SetOutput(string str, bool editable = false) {
            RemoveCallbacks();

            output.text = str;
            output.targetGraphic.raycastTarget = editable;
            isClickable = !editable;

            output.gameObject.SetActive(true);
            dropdown.gameObject.SetActive(false);

            if (editable) output.onEndEdit.AddListener(OnInputChanged);
        }

        public void SetupDropdown(List<string> dropdownValues,int currentValue) {
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

            output.gameObject.SetActive(false);
            dropdown.gameObject.SetActive(true);

            dropdown.onValueChanged.AddListener(OnDropdownChanged);
        }


        void RemoveCallbacks() {
            output.onEndEdit.RemoveListener(OnInputChanged);
            dropdown.onValueChanged.RemoveListener(OnDropdownChanged);
        }

        public void OnPointerClick(PointerEventData eventData) {
            if (isClickable && eventData.button == PointerEventData.InputButton.Left) {
                OnClick();
            }
        }
    }
}
