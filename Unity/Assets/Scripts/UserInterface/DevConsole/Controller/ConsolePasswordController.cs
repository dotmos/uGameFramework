using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UserInterface {
    public class ConsolePasswordController : MonoBehaviour
    {
        public GMInputField passwordInput;
        public GMButton applyButton;
        /// <summary>
        /// Define the current password here
        /// </summary>
        public string currentPassword;

        private void Awake() {
            applyButton.onClick.AddListener(CheckPassword);
        }

        private void OnEnable() {
            if (Application.isEditor) UnlockConsole();
        }

        void Update() {
            if (Input.GetKeyUp(KeyCode.Return)) {
                CheckPassword();
            }
        }

        void CheckPassword() {
            if (passwordInput.text == currentPassword) {
                UnlockConsole();
            }
        }

        public void LockConsole() {
            passwordInput.text = "";
            gameObject.SetActive(true);
        }

        public void UnlockConsole() {
            gameObject.SetActive(false);
        }
    }
}
