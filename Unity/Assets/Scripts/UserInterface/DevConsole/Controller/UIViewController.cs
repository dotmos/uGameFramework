using Service.DevUIService;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Zenject;

namespace UserInterface {
    public class UIViewController : GameComponent {
        public Text nameOutput;
        public GameObject defaultMode;
        public GameObject extensionButtonGroup;
        [Space]
        public GMInputField renameInputField;
        public GMButton saveNameButton;
        public GameObject renameMode;
        [Space]
        public GMButton renameButton;
        public GMButton archiveButton;
        public GMButton addLuaCommandButton;
        public GMButton addLuaExpressionButton;
        public GMButton applyAllButton;
        public GMButton applyAllAndCloseButton;
        [Space]
        public Transform contentContainer;
        public GameObject uiButtonPrefab;
        public GameObject uiTogglePrefab;
        public GameObject uiLUAButtonPrefab;
        public GameObject uiKeyValueOutputPrefab;
        public GameObject uiLUAExpressionPrefab;

        private DevUIView myView;
        [HideInInspector]
        public GMTab myTab;

        private Dictionary<DevUIElement, GameObject> uiElements = new Dictionary<DevUIElement, GameObject>();

        [Inject]
        IDevUIService _devUIService;

        public void Initialize(DevUIView uiView, GMTab uiViewTab) {
            myView = uiView;
            myTab = uiViewTab;

            //Set name
            UpdateName(myView.Name);

            //Check if we want to have extension buttons
            extensionButtonGroup.SetActive(uiView.extensionAllowed);

            //Button events
            if (uiView.extensionAllowed) {
                addLuaCommandButton.onClick.AddListener(AddLuaButton);
                addLuaExpressionButton.onClick.AddListener(AddLuaExpression);
            }

            applyAllButton.onClick.AddListener(ApplyAll);
            applyAllAndCloseButton.onClick.AddListener(ApplyAllAndClose);

            //Setup buttons if this UI Views is created dynamically from script. If this is not the case deactivate the buttons to rename or archive the ui view.
            if (uiView.createdDynamically) {
                saveNameButton.onClick.AddListener(
                    () => {
                        UpdateName(renameInputField.text);
                        ToggleNamingMode(false);
                    }
                );
                renameButton.onClick.AddListener(
                    () => ToggleNamingMode(true)
                );
                renameInputField.onValueChanged.AddListener(ValidateName);
                archiveButton.onClick.AddListener(ArchiveView);
            } else {
                renameButton.gameObject.SetActive(false);
                archiveButton.gameObject.SetActive(false);
            }

            //Setup ui elements
            foreach (DevUIElement uiElement in uiView.uiElements) {
                SpawnUIElement(uiElement);
            }

            //Add listener
            _eventService.OnEvent<Service.DevUIService.Events.NewUIElement>().Where(u => u.view == myView).Subscribe(evt => {
                if (evt.elem is DevUILuaExpression) {
                    SpawnLuaExpression((DevUILuaExpression)evt.elem, evt.inEditMode);
                } else if (evt.elem is DevUILUAButton) {
                    SpawnLuaButton((DevUILUAButton)evt.elem, evt.inEditMode);
                } else {
                    SpawnUIElement(evt.elem);
                }
            }).AddTo(this);

            uiView.uiElements.ObserveRemove().Subscribe(e => {
                RemoveUIElement(e.Value);
            }).AddTo(this);

            ToggleNamingMode(false);
        }

        void ApplyAll() {
            UIViewEditableElement editableElement = null;

            //Iterate through all ui elements. If it is an editable UI element save it!
            foreach (GameObject uiGO in uiElements.Values) {
                editableElement = uiGO.GetComponent<UIViewEditableElement>();
                if (editableElement != null && editableElement.isInEditMode) {
                    editableElement.Save();
                }
                editableElement = null;
            }
        }

        void ApplyAllAndClose() {
            ApplyAll();

            Kernel.Instance.Container.Resolve<Service.DevUIService.IDevUIService>().CloseScriptingConsole();
        }

        void AddLuaButton() {
            DevUILUAButton newButton = new DevUILUAButton("No Name", "print('Dummy Command')") { createdDynamically = true };
            myView.AddElement(newButton);
        }

        void AddLuaExpression() {
            DevUILuaExpression newButton = new DevUILuaExpression("No Name", 5f) { createdDynamically = true };
            myView.AddElement(newButton);
        }

        void SpawnUIElement(DevUIElement devUIElement) {
            //Spawn and initialize by type
            if (devUIElement is DevUILUAButton) {
                SpawnLuaButton(devUIElement as DevUILUAButton, false);
            } else if (devUIElement is DevUIButton) {
                SpawnButton(devUIElement as DevUIButton);
            } else if (devUIElement is DevUIToggle) {
                SpawnToggle(devUIElement as DevUIToggle);
            } else if (devUIElement is DevUILuaExpression) {
                SpawnLuaExpression(devUIElement as DevUILuaExpression, false);
            } else if (devUIElement is DevUIKeyValue) {
                SpawnKeyValueOutput(devUIElement as DevUIKeyValue);
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

        void SpawnToggle(DevUIToggle devToggle) {
            GameObject devUIElementGO = Instantiate(uiTogglePrefab) as GameObject;
            UIViewToggle toggle = devUIElementGO.GetComponent<UIViewToggle>();
            toggle.Initialize(devToggle.name, devToggle.value, devToggle.Execute);
            devUIElementGO.transform.SetParent(contentContainer, false);
            uiElements.Add(devToggle, devUIElementGO);
        }

        void SpawnKeyValueOutput(DevUIKeyValue devKeyValue) {
            GameObject devUIElementGO = Instantiate(uiKeyValueOutputPrefab) as GameObject;
            UIViewKeyValueOutput keyValueOutput = devUIElementGO.GetComponent<UIViewKeyValueOutput>();
            keyValueOutput.Initialize(devKeyValue);
            devUIElementGO.transform.SetParent(contentContainer, false);
            uiElements.Add(devKeyValue, devUIElementGO);
        }

        void SpawnLuaExpression(DevUILuaExpression luaExpression, bool activateInEditMode) {
            GameObject devUIElementGO = Instantiate(uiLUAExpressionPrefab) as GameObject;
            UIViewLUAExpression luaExpressionUI = devUIElementGO.GetComponent<UIViewLUAExpression>();
            luaExpressionUI.Initialize(luaExpression, myView);
            devUIElementGO.transform.SetParent(contentContainer, false);
            uiElements.Add(luaExpression, devUIElementGO);

            luaExpressionUI.ActivateEditMode(activateInEditMode);
        }

        void RemoveUIElement(DevUIElement element) {
            if (uiElements.ContainsKey(element)) {
                Destroy(uiElements[element].gameObject);
                uiElements.Remove(element);
            }
        }

        //********** Archive ***************//
        void ArchiveView() {
            _devUIService.RemoveViewToArchieve(myView);
        }

        //********** Naming **************//
        void UpdateName(string _name) {
            myView.Name = _name;
            myTab.GetComponentInChildren<Text>().text = myView.Name;
            nameOutput.text = myView.Name;
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
                renameInputField.text = myView.Name;
                renameInputField.ActivateInputField();
            }
        }
        //********************************//
    }
}
