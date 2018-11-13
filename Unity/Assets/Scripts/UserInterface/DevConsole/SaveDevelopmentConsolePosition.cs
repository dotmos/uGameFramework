using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UserInterface {
    [RequireComponent(typeof(GMDraggableObject))]
    public class SaveDevelopmentConsolePosition : MonoBehaviour {
        private GMDraggableObject dragHandler;

        void Awake() {
            dragHandler = GetComponent<GMDraggableObject>();
            Vector2 restoredPosition = new Vector2(PlayerPrefs.GetFloat("DevelopmentConsole_PositionX"), PlayerPrefs.GetFloat("DevelopmentConsole_PositionY"));
            dragHandler.target.anchoredPosition = restoredPosition;
        }

        private void OnApplicationQuit() {
            PlayerPrefs.SetFloat("DevelopmentConsole_PositionX", dragHandler.target.anchoredPosition.x);
            PlayerPrefs.SetFloat("DevelopmentConsole_PositionY", dragHandler.target.anchoredPosition.y);
        }
    }
}
