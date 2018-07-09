using System;
using System.Collections.Generic;

using Zenject;
using UniRx;
using MoonSharp.Interpreter;
using UnityEngine;
using MoonSharp.Interpreter.Interop;
using System.IO;
using System.Linq;

namespace Service.Scripting {
    partial class ScriptingServiceImpl : ScriptingServiceBase {

        private Script mainScript;
        private ScriptingConsoleComponent scriptingComponent;
        private static readonly HashSet<char> delimiters = new HashSet<char>() { '(',')',',','=',';'};

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
                Debug.LogException(ex);
                return "Error: "+ex.DecoratedMessage;
            }
            catch (SyntaxErrorException sex) {
                Debug.LogException(sex);
                return "Error: " + sex.DecoratedMessage;
            }
            catch (MoonSharp.Interpreter.InterpreterException ie) {
                Debug.LogException(ie);
                return "Error: " + ie.DecoratedMessage;
            }
        }


        private List<string> Proposal(DynValue dynvalue,string currentInput) {
            if (dynvalue.UserData != null) {
                var userDataResult = Proposal(dynvalue.UserData);
                return userDataResult;
            } else {
                Debug.LogWarning("Could not process proposal for Input:" + currentInput);
                return null;
            }
        }

        private List<string> Proposal(UserData userdata) {
            var result = new List<string>();
            if (userdata.Descriptor is StandardUserDataDescriptor) {
                var dscr = (StandardUserDataDescriptor)userdata.Descriptor;

                result.AddRange(dscr.MemberNames);
            } else {
                Debug.LogWarning("Propsal from Userdata cannot process UserData-Descriptor-Type:" + userdata.Descriptor.GetType().ToString());
            }

            return result;
        }

        /// <summary>
        /// Get Keyword under cursor take into account delimiters so you can also detect keyword that are inside of 'something' 
        /// e.g. (lua) print(FileSystem.[autocomplete])
        /// </summary>
        /// <param name="all"></param>
        /// <param name="cursorPos"></param>
        /// <returns></returns>
        private void CurrentWordOnCursor(string all,out string firstPart,out string secondPart,int cursorPos,ref int start,ref int endPos) {
            endPos = cursorPos;
            // TODO: Regular expression?
            string first = "";
            string second = "";
            bool checkFirst = true;
            while (all[cursorPos]!='.' && cursorPos>=0) {
                if (delimiters.Contains(all[cursorPos])) {
                    // found end => there is not firstpart
                    checkFirst = false; // since we already hit a delimiter, we can stop word processing
                    break;
                }
                second = all[cursorPos] + second;
                cursorPos--;
            }
            if (checkFirst) {
                cursorPos--;
                while (!delimiters.Contains(all[cursorPos]) && cursorPos >= 0) {
                    first = all[cursorPos] + first;
                    cursorPos--;
                }
            }
            firstPart = first;
            secondPart = second;
            start = cursorPos + 1;

            // at last find the endposition
            cursorPos = endPos;
            while (cursorPos < all.Length && !delimiters.Contains(all[cursorPos])) {
                cursorPos++;
            }
            endPos = cursorPos;
        }

        public override Proposal AutocompleteProposals(string currentInput,int cursorPos) {
            var result = new Proposal();

            string currentObjectPath = null;
            string lastInputPart = null;
            cursorPos--;
            int from = 0;
            int to = 0;

            // get the current lua-parts corresponding to its cursor-position
            CurrentWordOnCursor(currentInput, out currentObjectPath, out lastInputPart, cursorPos,ref from,ref to);

            List<string> currentProposals = null;
            bool root = currentObjectPath == "";

            if (!root) {
                try {
                    var currentLuaObject = mainScript.Globals.Get(currentObjectPath);
                    currentProposals = Proposal(currentLuaObject,currentInput);
                }
                catch (Exception exCurrentObject) {
                    Debug.LogException(exCurrentObject);
                    return null;
                }
            } else {
                // check root-global
                currentProposals = mainScript.Globals.Keys.Select(val=>val.String).ToList();
            }

            if (currentProposals == null) {
                return null;
            }

            result.proposalElements = currentProposals.FindAll(prop => lastInputPart.Length == 0 || prop.StartsWith(lastInputPart))
                           .Select(elem => new ProposalElement() { simple = elem, full = (currentObjectPath==""?elem:currentObjectPath + "." + elem) })
                           .ToList();

            result.replaceStringStart = from;
            result.replaceStringEnd = to;
            return result;
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

    public class Proposal
    {
        public List<ProposalElement> proposalElements;
        public int replaceStringStart;
        public int replaceStringEnd;
    }

    public class ProposalElement
    {
        public enum ProposalType
        {
            method,constant,variable
        }

        /// <summary>
        /// Type of this proposal
        /// </summary>
        ProposalType type;

        /// <summary>
        /// Type specific info
        /// </summary>
        object info;

        /// <summary>
        /// The Element of the current object. if full qualified it would be a.b.c, the result would be c
        /// </summary>
        public string simple;
        /// <summary>
        /// The Element of hte current object but full qualified (a.b.c)
        /// </summary>
        public string full;
    }

}
