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
                    SetOutput(dataCellObject.value.ToString());
                    break;
                case GameDataScrollView.DataCellObject.CellType.EditableOutput:
                    SetOutput(dataCellObject.value.ToString(), true);
                    break;
                case GameDataScrollView.DataCellObject.CellType.Dropdown:
                    SetupDropdown(dataCellObject.value as List<string>);
                    break;
                default:
                    break;
            }
        }

        void OnDropdownChanged(int index) {
            dataCellObject.callback();
        }

        void OnClick() {
            dataCellObject.callback();
        }

        void OnInputChanged(string newValue) {
            dataCellObject.callback();
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

        public void SetupDropdown(List<string> dropdownValues) {
            RemoveCallbacks();

            dropdown.ClearOptions();
            List<Dropdown.OptionData> dataList = new List<Dropdown.OptionData>();

            for (int i = 0; i < dropdownValues.Count; ++i) {
                Dropdown.OptionData data = new Dropdown.OptionData();
                data.text = dropdownValues[i];
                dataList.Add(data);
            }

            dropdown.options = dataList;

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
