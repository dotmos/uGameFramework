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
#if UNITY_PS5
            UnlockConsole();
#else
            if (Application.isEditor || Application.isConsolePlatform) UnlockConsole();
#endif

        }

        void Update() {
            if (Input.GetKeyUp(KeyCode.Return)) {
                CheckPassword();
            }

#if ENABLE_CONSOLE_UI
            CheckKonamiCode();
#endif
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

#if ENABLE_CONSOLE_UI
        private KeyCode[] code = new KeyCode[]
                          {
                                KeyCode.UpArrow,
                                KeyCode.UpArrow,
                                KeyCode.DownArrow,
                                KeyCode.DownArrow,
                                KeyCode.LeftArrow,
                                KeyCode.RightArrow,
                                KeyCode.LeftArrow,
                                KeyCode.RightArrow,
                                KeyCode.B,
                                KeyCode.A
                          };

        int currentIndex = 0;

        void CheckKonamiCode() {
            if (Input.GetJoystickNames().Length > 0) {
                KeyCode? keyCode = HandleJoystick();

                if (keyCode != null) {
                    if (code[currentIndex] == keyCode) {
                        currentIndex += 1;

                        if (currentIndex == code.Length) {
                            UnlockConsole();
                            currentIndex = 0;
                        }
                    } else {
                        //Reset
                        currentIndex = 0;
                    }
                }
            }
        }

        private Vector2 previousPadInput;

        /// <summary>
        /// Translates joystick input into keyboard keys
        /// </summary>
        /// <returns>The detected keyboard key</returns>
        private KeyCode? HandleJoystick() {
            const float deadZone = 0.15f; // A joystick is never fully rested
            KeyCode? currentKey = null;

            // Gamepad A or B
            //---------------------------------------------------------------------------
            bool a = false;
            bool b = false;

#if UNITY_WEBPLAYER || UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
            a = Input.GetKeyDown(KeyCode.Joystick1Button0);
            b = Input.GetKeyDown(KeyCode.Joystick1Button1);
#elif UNITY_STANDALONE_OSX
            a = Input.GetKeyDown(KeyCode.Joystick1Button16);
            b = Input.GetKeyDown(KeyCode.Joystick1Button17);
#endif

            if (a) {
                return KeyCode.A;
            }

            if (b) {
                return KeyCode.B;
            }

            // Axis
            //---------------------------------------------------------------------------
            float x = Input.GetAxis("Horizontal");
            float y = Input.GetAxis("Vertical");

            // Rest?
            if (Mathf.Abs(x) < deadZone && Mathf.Abs(y) < deadZone) {
                x = 0;
                y = 0;
            }

            // Horizontal only
            if (Mathf.Abs(x) > deadZone && Mathf.Abs(y) < deadZone) {
                if (previousPadInput.x == 0) {
                    currentKey = x > 0 ? KeyCode.RightArrow : KeyCode.LeftArrow;
                }
            }
            // Vertical only
            else if (Mathf.Abs(y) > deadZone && Mathf.Abs(x) < deadZone) {
                if (previousPadInput.y == 0) {
                    currentKey = y > 0 ? KeyCode.UpArrow : KeyCode.DownArrow;
                }
            }

            previousPadInput.x = x;
            previousPadInput.y = y;

            return currentKey;
        }
#endif
    }
}
