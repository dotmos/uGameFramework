using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MoonSharp.Interpreter;
using UniRx;
using Service.FileSystem;
using Zenject;
using Service.Scripting;
using System.IO;
using System;
using System.Linq;

namespace UserInterface {
    public class DevelopmentConsole : GameComponent {

        public Text consoleText;
        public GMInputField consoleInput;
        public AutoCompletionWindow autoCompleteWindow;
        public GMButton closeButton;

        private int historyID = -1;
        private List<string> history = new List<string>();
        private string currentViewText = "";
        private Service.Scripting.Commands.ExecuteStringOnMainScriptCommand executeLuaCmd = new Service.Scripting.Commands.ExecuteStringOnMainScriptCommand();

        private string scriptingPath = null;
        private float keyHeldTimer;

        [Inject]
        IFileSystemService filesystem;
        [Inject]
        IScriptingService scripting;

        protected override void AfterBind() {
            scriptingPath = filesystem.GetPath(FSDomain.Scripting);

            consoleInput.onEndEdit.AddListener(OnEndEdit);
            closeButton.onClick.AddListener(CloseConsole);

            this.OnEvent<Service.DevUIService.Events.WriteToScriptingConsole>().Subscribe(e => {
                AddToText(e.text);
            }).AddTo(this);
             
            this.OnEvent<Service.DevUIService.Events.ScriptingConsoleOpened>().Subscribe(e => {
                consoleInput.ActivateInputField();
            }).AddTo(this);

            Observable.EveryUpdate().Subscribe(_ => {
                if (Input.GetKeyDown(KeyCode.LeftControl)) {
                    consoleInput.readOnly = true;
                }

                if (Input.GetKeyUp(KeyCode.LeftControl)) {
                    consoleInput.readOnly = false;
                }

                //Auto completion switch
                if (Input.GetKey(KeyCode.UpArrow)) {
                    if (historyID < 0) {
                        //if keyHeldTimer == 0 we do one step, else if button is held more than 0.5 seconds we do continuous scroll
                        if (keyHeldTimer == 0 || keyHeldTimer > 0.5f) {
                            autoCompleteWindow.SwitchElement(-1);
                        }
                    }
                } else if (Input.GetKey(KeyCode.DownArrow)) {
                    if (historyID < 0) {
                        //if keyHeldTimer == 0 we do one step, else if button is held more than 0.5 seconds we do continuous scroll
                        if (keyHeldTimer == 0 || keyHeldTimer > 0.5f) {
                            autoCompleteWindow.SwitchElement(1);
                        }
                    }
                } else if (Input.GetKeyDown(KeyCode.Tab) && consoleInput.isFocused) {
                    autoCompleteWindow.ApplyCurrentProposal();
                }

                if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.UpArrow)) {
                    keyHeldTimer += Time.deltaTime;
                }

                if (Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.UpArrow)) {
                    keyHeldTimer = 0;
                }

                //History switch
                if (Input.GetKeyDown(KeyCode.UpArrow) && !autoCompleteWindow.HasContent) {
                    if (consoleInput.text == "" || historyID >= 0) {
                        if (history.Count == 0) {
                            return;
                        }

                        historyID = Mathf.Clamp(++historyID, -1, history.Count - 1);

                        consoleInput.text = history[historyID];
                    }
                }
                    
                if (Input.GetKeyDown(KeyCode.DownArrow) && !autoCompleteWindow.HasContent) {
                    if (historyID >= 0) {
                        if (history.Count == 0) {
                            return;
                        }

                        historyID = Mathf.Clamp(--historyID, -1, history.Count - 1);

                        if (historyID < 0) {
                            consoleInput.text = "";
                        } else {
                            consoleInput.text = history[historyID];
                        }
                    }
                }

                //Exclude buttons from value changed
                if (Input.anyKeyDown && !(Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.UpArrow)) || Input.GetKeyDown(KeyCode.Tab)) {
                    OnValueChanged();
                }

                if (Input.GetKeyDown(KeyCode.Space) && Input.GetKey(KeyCode.LeftControl)) {
                    GetAutocompleteProposal();
                }
            }).AddTo(this);

            Service.Scripting.Commands.GetMainScriptCommand cmdGetMainScript = new Service.Scripting.Commands.GetMainScriptCommand();
            Publish(cmdGetMainScript);
            cmdGetMainScript.result.Options.DebugPrint = (inputString) => {
                AddToText(inputString);
            };
        }

        void OnEndEdit(string input) {
            if (Input.GetKey(KeyCode.Return)) {
                ProcessInput(consoleInput.text);
            }
        }

        void OnValueChanged() {
            //Reset history ID if user typed
            historyID = -1;

            if (consoleInput.text.Length > 0) {
                GetAutocompleteProposal();
            } else {
                autoCompleteWindow.ClearItems();
            }
        }

        void GetAutocompleteProposal() {
            Proposal proposal = scripting.AutocompleteProposals(GetCurrentText(), consoleInput.caretPosition);

            if (proposal == null || proposal.proposalElements == null) {
                // no proposals
                autoCompleteWindow.ClearItems();
                return;
            }
            List<ProposalElement> proposals = proposal.proposalElements;

            if (proposals.Count > 0) {
                autoCompleteWindow.UpdateList(proposal);
            }
        }

        void AddToText(string txt) {
            if (txt == "") return;

            currentViewText += txt + "\n";
            consoleText.text = currentViewText;
        }

        public string GetCurrentText() {
            return consoleInput.text;
        }

        protected override void OnDispose() {
            base.OnDispose();
            consoleInput.onEndEdit.RemoveAllListeners();
            consoleInput.onValueChanged.RemoveAllListeners();
        }

        /// <summary>
        /// This is called when hitting the return-key on the keyboard while the input field is focused and also by the submit button.
        /// </summary>
        public void ProcessInput() {
            ProcessInput(consoleInput.text);
        }

        public void ProcessInput(string input) {
            consoleInput.DeactivateInputField();

            if (input.ToLower() == "help") {
                AddToText("help\ncommands: close,save [relative filename],... plus service-commands");
                // TODO: Make this better
            } else if (input.ToLower() == "clear") {
                currentViewText = "";
                consoleText.text = currentViewText;
            } else if (input.ToLower() == "close") {
                gameObject.SetActive(false);
            } else if (input.ToLower().StartsWith("save")) {
                // save current history to file
                string[] splits = input.Split(' ');
                if (splits.Length < 2) {
                    AddToText("'save' needs a path (relative to the scripting folder)\ne.g.: save test.lua");
                } else {
                    // save the history
                    string path = splits[1];
                    if (path.StartsWith("/") || path.StartsWith("\\")) {
                        AddToText("Warning: Using absolute path not allowed!! files are saved relative to scripting folder:" + scriptingPath);
                        path = path.Substring(1);
                    }

                    filesystem.WriteStringToFile(scriptingPath + "/" + path, history.Aggregate((i, j) => j + " \n" + i));
                    AddToText("saved\n");
                }
            } else if (input.ToLower().StartsWith("load")) {
                // save current history to file
                string[] splits = input.Split(' ');
                if (splits.Length < 2) {
                    AddToText("'load' needs a path (relative to the scripting folder)\ne.g.: load test.lua");
                } else {
                    // save the history
                    string path = splits[1];
                    if (path.StartsWith("/") || path.StartsWith("\\")) {
                        path = path.Substring(1);
                    }
                    string result = scripting.ExecuteFileToMainScript(scriptingPath + "/" + path);
                    if (result != "void") {
                        AddToText(result);
                    } else {
                        AddToText("\nloaded");
                    }
                }
            } else if (input.ToLower().StartsWith("dir")) {
                // save current history to file
                string[] splits = input.Split(' ');

                string path = splits.Length < 2 ? "" : (splits[1].StartsWith("/") ? splits[1] : splits[1].Substring(1));
                string filePath = scriptingPath + path;

                try {
                    if (!Directory.Exists(filePath)) {
                        AddToText("Directory " + path + " does not exist");
                    } else {
                        string[] dir = Directory.GetFiles(filePath);

                        AddToText("Directory:" + (path == "" ? "/" : path));
                        if (dir.Length == 0) {
                            AddToText("NO FILES (in folder" + filePath + ")");
                        } else {
                            foreach (string entry in dir) {
                                AddToText(entry.Replace(scriptingPath, "").Substring(1));
                            }
                        }
                    }
                } catch (Exception e) {
                    Debug.LogError("Something went wrong using 'dir'-command");
                    Debug.LogException(e);
                }
            } else {
                currentViewText += input + "\n";
                executeLuaCmd.luaCode = input;
                Publish(executeLuaCmd);
                if (executeLuaCmd.result != "void") {
                    currentViewText += executeLuaCmd.result + "\n";
                }
                consoleText.text = currentViewText;
                consoleInput.ActivateInputField();
            }

            history.Insert(0, input);

            consoleInput.text = "";
            autoCompleteWindow.ClearItems();
            consoleInput.ActivateInputField();
            // always be at the end of the history after execution
            historyID = -1;
        }

        public void CloseConsole() {
            this.Publish(new Service.DevUIService.Commands.CloseScriptingConsoleCommand());
        }
    }
}
