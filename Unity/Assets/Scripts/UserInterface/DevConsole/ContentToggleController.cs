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
            leftContent.SetActive(PlayerPrefs.GetInt("DevelopmentConsole_IsLeftContentActive") == 0 ? false : true);
            leftContent.SetActive(PlayerPrefs.GetInt("DevelopmentConsole_IsRightContentActive") == 0 ? false : true);

            //Fallback for initialization of player prefs
            if (!leftContent.activeSelf && !rightContent.activeSelf) {
                ToggleLeftContent(true);
                ToggleRightContent(true);
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
            rightContentToggle.interactable = activate;

            PlayerPrefs.SetInt("DevelopmentConsole_IsRightContentActive", activate ? 1 : 0);
        }
    }
}
