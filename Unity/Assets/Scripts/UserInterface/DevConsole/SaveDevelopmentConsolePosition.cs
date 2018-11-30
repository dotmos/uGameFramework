using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace UserInterface {
    [RequireComponent(typeof(GMDraggableObject))]
    public class SaveDevelopmentConsolePosition : GameComponent {
        private GMDraggableObject dragHandler;

        protected override void AfterBind() {
            base.AfterBind();

            this.OnEvent<Service.DevUIService.Events.ScriptingConsoleOpened>().Subscribe(e => RestorePosition()).AddTo(this);
            this.OnEvent<Service.DevUIService.Events.ScriptingConsoleClosed>().Subscribe(e => SavePosition()).AddTo(this);
        }

        void RestorePosition() {
            if (dragHandler == null) dragHandler = GetComponent<GMDraggableObject>();

            if (dragHandler != null) {
                Vector2 restoredPosition = new Vector2(PlayerPrefs.GetFloat("DevelopmentConsole_PositionX"), PlayerPrefs.GetFloat("DevelopmentConsole_PositionY"));
                dragHandler.target.anchoredPosition = restoredPosition;
            }
        }

        void SavePosition() {
            if (dragHandler != null) {
                PlayerPrefs.SetFloat("DevelopmentConsole_PositionX", dragHandler.target.anchoredPosition.x);
                PlayerPrefs.SetFloat("DevelopmentConsole_PositionY", dragHandler.target.anchoredPosition.y);
            }
        }

        private void OnApplicationQuit() {
            SavePosition();
        }
    }
}
