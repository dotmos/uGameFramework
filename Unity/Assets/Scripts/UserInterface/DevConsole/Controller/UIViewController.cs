using Service.DevUIService;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace UserInterface {
    public class UIViewController : GameComponent {
        public Text nameOutput;
        public GameObject defaultMode;
        [Space]
        public GMInputField renameInputField;
        public GMButton saveNameButton;
        public GameObject renameMode;
        [Space]
        public GMButton renameButton;
        public GMButton archiveButton;
        public GMButton addLuaCommandButton;
        public GMButton addTickingExpression;
        [Space]
        public Transform contentContainer;
        public GameObject uiButtonPrefab;
        public GameObject uiLUAButtonPrefab;

        private DevUIView myView;
        private GMTab myTab;

        private Dictionary<DevUIElement, GameObject> uiElements = new Dictionary<DevUIElement, GameObject>();

        public void Initialize(DevUIView uiView, GMTab uiViewTab) {
            myView = uiView;
            myTab = uiViewTab;

            //Set name
            UpdateName(myView.name);

            //Button events
            addLuaCommandButton.onClick.AddListener(AddLuaButton);
            saveNameButton.onClick.AddListener(
                () => {
                    UpdateName(renameInputField.text);
                    ToggleNamingMode(false);
                }
            );
            renameButton.onClick.AddListener(
                ()=> ToggleNamingMode(true)
            );
            renameInputField.onValueChanged.AddListener(ValidateName);

            //Setup ui elements
            foreach (DevUIElement uiElement in uiView.uiElements) {
                SpawnUIElement(uiElement);
            }

            //Add listener
            _eventService.OnEvent<Service.DevUIService.Events.NewUIElement>().Where(u => u.view == myView).Subscribe(evt => {
                if (evt.elem is DevUILUAButton) {
                    SpawnLuaButton((DevUILUAButton)evt.elem, evt.inEditMode);
                }
            }).AddTo(this);

            uiView.uiElements.ObserveRemove().Subscribe(e => {
                RemoveUIElement(e.Value);
            }).AddTo(this);

            ToggleNamingMode(false);
        }

        void AddLuaButton() {
            DevUILUAButton newButton = new DevUILUAButton("No Name", "print('Dummy Command')") { createdDynamically = true };
            myView.AddElement(newButton);
        }

        void SpawnUIElement(DevUIElement devUIElement) {
            //Spawn and initialize by type
            if (devUIElement is DevUILUAButton) {
                SpawnLuaButton(devUIElement as DevUILUAButton, false);
            } else if (devUIElement is DevUIButton) {
                SpawnButton(devUIElement as DevUIButton);
            } else {
                Debug.LogWarning("Tried to spawn a UI element that has no prefab for view!");
            }
        }

        void SpawnLuaButton(DevUILUAButton luaButton, bool activateInEditMode) {
            GameObject devUIElementGO = Instantiate(uiLUAButtonPrefab) as GameObject;
            UIViewLUAButton button = devUIElementGO.GetComponent<UIViewLUAButton>();
            button.Initialize(luaButton.name, luaButton.Execute, luaButton, myView);
            devUIElementGO.transform.SetParent(contentContainer, false);
            uiElements.Add(luaButton, devUIElementGO);

            button.ActivateEditMode(activateInEditMode);
        }

        void SpawnButton(DevUIButton devButton) {
            GameObject devUIElementGO = Instantiate(uiButtonPrefab) as GameObject;
            UIViewButton button = devUIElementGO.GetComponent<UIViewButton>();
            button.Initialize(devButton.name, devButton.Execute);
            devUIElementGO.transform.SetParent(contentContainer, false);
            uiElements.Add(devButton, devUIElementGO);
        }

        void RemoveUIElement(DevUIElement element) {
            if (uiElements.ContainsKey(element)) {
                Destroy(uiElements[element].gameObject);
                uiElements.Remove(element);
            }
        }

        //********** Naming **************//
        void UpdateName(string _name) {
            myView.name = _name;
            myTab.GetComponentInChildren<Text>().text = myView.name.ToUpper();
            nameOutput.text = myView.name.ToUpper();
        }

        void ValidateName(string _name) {
            if (string.IsNullOrEmpty(_name)) {
                saveNameButton.interactable = false;
            } else {
                saveNameButton.interactable = true;
            }
        }

        void ToggleNamingMode(bool activate) {
            renameMode.SetActive(activate);
            defaultMode.SetActive(!activate);
            ValidateName(renameInputField.text);

            if (activate) {
                renameInputField.text = myView.name;
                renameInputField.ActivateInputField();
            }
        }
        //********************************//
    }
}
