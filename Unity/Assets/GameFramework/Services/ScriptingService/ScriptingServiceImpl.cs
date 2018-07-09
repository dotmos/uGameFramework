using System;
using System.Collections.Generic;

using Zenject;
using UniRx;
using MoonSharp.Interpreter;
using UnityEngine;
using MoonSharp.Interpreter.Interop;
using System.IO;

namespace Service.Scripting {
    partial class ScriptingServiceImpl : ScriptingServiceBase {

        private Script mainScript;
        private ScriptingConsoleComponent scriptingComponent;

        /// <summary>
        /// Is the gameconsole enabled?
        /// </summary>

        protected override void AfterInitialize() {
            try {
                // make all datatypes available
                UserData.RegistrationPolicy = InteropRegistrationPolicy.Automatic;

                //scriptingComponent = GameObject.Find("/ScriptingConsole").GetComponent<ScriptingConsoleComponent>();
                // this is called right after the Base-Classes Initialize-Method. _eventManager and disposableManager are set
                mainScript = new Script();


                // TODO: get rid of EveryUpdate
                Observable.EveryUpdate().Subscribe(_ => {
                    if (UnityEngine.Input.GetKeyDown(KeyCode.F8)) {
                        ToggleScriptingConsole();
                    }
                }).AddTo(disposables);
                
                // TODO: get rid of nextframe
                Observable.NextFrame().Subscribe(_ => ActivateDefaultScripting("script")).AddTo(disposables);
            }
            catch (Exception e) {
                Debug.LogError("COULD NOT START SCRIPTINGCONSOLE SERVICE!");
                Debug.LogException(e);
            }
        }

        public override string ExecuteStringOnMainScript(string luaCode) {
            try {
                var result = mainScript.DoString(luaCode);
                return result.ToString();
            }
            catch (ScriptRuntimeException ex) {
                return "Error: "+ex.DecoratedMessage;
            }
            catch (SyntaxErrorException sex) {
                return "Error: " + sex.DecoratedMessage;
            }
            catch (MoonSharp.Interpreter.InterpreterException ie) {
                return "Error: " + ie.DecoratedMessage;
            }
        }

        public override string ExecuteFileToMainScript(string fileName) {
            try {
                // TODO: User DoFile with corresponding platform-controller
                //var result = mainScript.DoFile(fileName);
                //return result.ToString();
                var input = File.ReadAllText(fileName);
                return ExecuteStringOnMainScript(input);
            }
            catch (ScriptRuntimeException ex) {
                return "Error: " + ex.DecoratedMessage;
            }
            catch (SyntaxErrorException sex) {
                return "Error: " + sex.DecoratedMessage;
            }
            catch (MoonSharp.Interpreter.InterpreterException ie) {
                return "Error: " + ie.DecoratedMessage;
            }
        }



        public override void OpenScriptingConsole() {
            if (scriptingComponent == null) {
                var prefab = UnityEngine.Resources.Load("ScriptingConsole");
                scriptingComponent = (GameObject.Instantiate(prefab) as GameObject).GetComponent<ScriptingConsoleComponent>();
            } 
            scriptingComponent.ConsoleEnabled = true;
        }

		public override void CloseScriptingConsole() {
            if (scriptingComponent == null) {
                return;
            }
            scriptingComponent.ConsoleEnabled = false;
        }

        public override void ToggleScriptingConsole() {
            if (scriptingComponent==null || !scriptingComponent.ConsoleEnabled)  {
                OpenScriptingConsole();
            } else {
                CloseScriptingConsole();
            }
        }

        public override Script GetMainScript() {
            return mainScript;
        }

        public override bool IsScriptingConsoleVisible() {
            return scriptingComponent.ConsoleEnabled;
        }


        protected override void OnDispose() {
            // do your IDispose-actions here. It is called right after disposables got disposed
        }

    }
}
