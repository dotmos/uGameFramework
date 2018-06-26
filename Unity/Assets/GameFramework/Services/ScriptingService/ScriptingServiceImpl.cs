using System;
using System.Collections.Generic;

using Zenject;
using UniRx;
using MoonSharp.Interpreter;
using UnityEngine;
using MoonSharp.Interpreter.Interop;

namespace Service.Scripting {
    class ScriptingServiceImpl : ScriptingServiceBase {

        private Script mainScript;
        private ScriptingConsoleComponent scriptingComponent;

        protected override void AfterInitialize() {
            try {
                // make all datatypes available
                UserData.RegistrationPolicy = InteropRegistrationPolicy.Automatic;

                scriptingComponent = GameObject.Find("/ScriptingConsole").GetComponent<ScriptingConsoleComponent>();
                // this is called right after the Base-Classes Initialize-Method. _eventManager and disposableManager are set
                mainScript = new Script();
                
                Observable.NextFrame().Subscribe(_ => ActivateDefaultScripting("script"));
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

        public override void OpenScriptingConsole() {
            if (scriptingComponent == null) {
                return;
            }
            scriptingComponent.gameObject.SetActive(true);
        }

		public override void CloseScriptingConsole() {
            if (scriptingComponent == null) {
                return;
            }
            scriptingComponent.gameObject.SetActive(false);
        }

        public override void ToggleScriptingConsole() {
            if (scriptingComponent == null) {
                return;
            }
            scriptingComponent.gameObject.SetActive(!scriptingComponent.gameObject.activeSelf);
        }

        public override Script GetMainScript() {
            return mainScript;
        }

        public override bool IsScriptingConsoleVisible() {
            return scriptingComponent.gameObject.activeSelf;
        }


        protected override void OnDispose() {
            // do your IDispose-actions here. It is called right after disposables got disposed
        }

        public override void LoadStringToMainScript(string fileName) {
            throw new NotImplementedException();
        }
    }
}
