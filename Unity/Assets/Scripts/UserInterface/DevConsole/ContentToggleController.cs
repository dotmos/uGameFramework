using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UserInterface {
    public class ContentToggleController : MonoBehaviour {
        public GMToggle leftContentToggle;
        public GameObject leftContent;
        [Space]
        public GMToggle rightContentToggle;
        public GameObject rightContent;

        private void Awake() {
            ToggleLeftContent(PlayerPrefs.GetInt("DevelopmentConsole_IsLeftContentActive") == 0 ? false : true);
            ToggleRightContent(PlayerPrefs.GetInt("DevelopmentConsole_IsRightContentActive") == 0 ? false : true);

            leftContentToggle.isOn = leftContent.activeSelf;
            rightContentToggle.isOn = rightContent.activeSelf;

            //Fallback for initialization of player prefs
            if (!leftContent.activeSelf && !rightContent.activeSelf) {
                ToggleLeftContent(true);
                ToggleRightContent(true);

                leftContentToggle.isOn = leftContentToggle.interactable = true;
                rightContentToggle.isOn = rightContentToggle.interactable = true;
            }

            leftContentToggle.onValueChanged.AddListener(ToggleLeftContent);
            rightContentToggle.onValueChanged.AddListener(ToggleRightContent);
        }

        void ToggleLeftContent(bool activate) {
            leftContent.SetActive(activate);
            rightContentToggle.interactable = activate;

            PlayerPrefs.SetInt("DevelopmentConsole_IsLeftContentActive", activate ? 1 : 0);
        }

        void ToggleRightContent(bool activate) {
            rightContent.SetActive(activate);
            leftContentToggle.interactable = activate;

            PlayerPrefs.SetInt("DevelopmentConsole_IsRightContentActive", activate ? 1 : 0);
        }
    }
}
