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
    public class DevelopmentConsoleComponent : GameComponent {

        public Text consoleText;
        public InputField consoleInput;
        public AutoCompletionWindow autoCompleteWindow;

        private int historyID = -1;
        private List<string> history = new List<string>();
        private string currentViewText = "";
        private Service.Scripting.Commands.ExecuteStringOnMainScriptCommand executeLuaCmd = new Service.Scripting.Commands.ExecuteStringOnMainScriptCommand();

        private string scriptingPath = null;

        public ReactiveProperty<bool> consoleEnabledPropery = new ReactiveProperty<bool>(false);
        /// <summary>
        /// Is the gameconsole enabled?
        /// </summary>
        public bool ConsoleEnabled {
            get { return consoleEnabledPropery.Value; }
            set { consoleEnabledPropery.Value = value; }
        }

        [Inject]
        IFileSystemService filesystem;
        [Inject]
        IScriptingService scripting;

        protected override void AfterBind() {
            scriptingPath = filesystem.GetPath(FSDomain.ScriptingOutput);

            consoleInput.onValueChanged.AddListener(OnValueChanged);
            consoleInput.onEndEdit.AddListener(OnEndEdit);

            autoCompleteWindow.currentSelectedElementID = -1;

            Observable.EveryUpdate().Subscribe(_ => {
                if (historyID < 0 && autoCompleteWindow.currentSelectedElementID < 0) {
                    consoleInput.readOnly = false;
                } else {
                    consoleInput.readOnly = true;
                }

                if (Input.GetKeyDown(KeyCode.UpArrow)) {
                    if (autoCompleteWindow.currentSelectedElementID < 0) {
                        if (history.Count == 0) {
                            return;
                        }

                        if (historyID + 1 < history.Count) {
                            historyID++;
                        }
                        consoleInput.text = history[historyID];
                    } else {
                        autoCompleteWindow.SwitchElement(-1);
                    }
                } else if (Input.GetKeyDown(KeyCode.DownArrow)) {
                    if (historyID < 0) {
                        autoCompleteWindow.SwitchElement(1);
                    } else {
                        if (history.Count == 0) {
                            return;
                        }
                        if (historyID > 0) {
                            historyID--;
                            consoleInput.text = history[historyID];
                        } else if (historyID < 0) {
                            historyID = 0;
                            consoleInput.text = "";
                        }
                    }
                }

                if (Input.GetKeyDown(KeyCode.Space) && Input.GetKey(KeyCode.LeftControl)) {
                    GetAutocompleteProposal();
                }
            }).AddTo(this);

            consoleEnabledPropery
                .DistinctUntilChanged()
                .Subscribe(enabled => {
                    if (enabled) {
                        gameObject.SetActive(true);
                        consoleInput.ActivateInputField();
                    } else {
                        gameObject.SetActive(false);
                    }
                })
                .AddTo(this);

            var cmdGetMainScript = new Service.Scripting.Commands.GetMainScriptCommand();
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

        void OnValueChanged(string input) {
            //Reset history ID if user typed
            historyID = -1;

            if (consoleInput.text.Length > 0) {
                GetAutocompleteProposal();
            } else {
                autoCompleteWindow.ClearItems();
            }
        }

        void GetAutocompleteProposal() {
            var proposal = scripting.AutocompleteProposals(GetCurrentText(), consoleInput.caretPosition);

            if (proposal == null || proposal.proposalElements == null) {
                // no proposals
                autoCompleteWindow.ClearItems();
                return;
            }
            var proposals = proposal.proposalElements;

            if (proposals.Count > 0) {
                autoCompleteWindow.UpdateList(proposal);
            }
        }

        public void AddToText(string txt) {
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
                var splits = input.Split(' ');
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
                var splits = input.Split(' ');
                if (splits.Length < 2) {
                    AddToText("'load' needs a path (relative to the scripting folder)\ne.g.: load test.lua");
                } else {
                    // save the history
                    string path = splits[1];
                    if (path.StartsWith("/") || path.StartsWith("\\")) {
                        path = path.Substring(1);
                    }
                    var result = scripting.ExecuteFileToMainScript(scriptingPath + "/" + path);
                    if (result != "void") {
                        AddToText(result);
                    } else {
                        AddToText("\nloaded");
                    }
                }
            } else if (input.ToLower().StartsWith("dir")) {
                // save current history to file
                var splits = input.Split(' ');

                string path = splits.Length < 2 ? "" : (splits[1].StartsWith("/") ? splits[1] : splits[1].Substring(1));
                string filePath = scriptingPath + path;

                try {
                    if (!Directory.Exists(filePath)) {
                        AddToText("Directory " + path + " does not exist");
                    } else {
                        var dir = Directory.GetFiles(filePath);

                        AddToText("Directory:" + (path == "" ? "/" : path));
                        if (dir.Length == 0) {
                            AddToText("NO FILES (in folder" + filePath + ")");
                        } else {
                            foreach (var entry in dir) {
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
            consoleInput.ActivateInputField();
            // always be at the end of the history after execution
            historyID = -1;
        }
    }
}
