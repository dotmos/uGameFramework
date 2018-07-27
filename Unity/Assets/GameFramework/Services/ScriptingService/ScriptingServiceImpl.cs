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
        //private UserInterface.DevelopmentConsoleComponent devConsoleComponent;
        private static readonly HashSet<char> delimiters = new HashSet<char>() { '(',')',',','=',';',' '};

        // Scene loading commands
        private const string developmentSceneID = "DevelopmentConsole";
        private Scene.Commands.ActivateSceneCommand activateDevelopmentConsole = new Scene.Commands.ActivateSceneCommand() { sceneID = developmentSceneID };
        private Scene.Commands.DeactivateSceneCommand deactivateDevelopmentConsole = new Scene.Commands.DeactivateSceneCommand() { sceneID = developmentSceneID };
        private Scene.Commands.LoadSceneCommand loadDevelopmentConsole = new Scene.Commands.LoadSceneCommand() {
            sceneID = developmentSceneID,
            additive = true,
            asynchron = false,
        };
        private bool devConsoleActive = false;

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

                // Load our development console scene
                Publish(loadDevelopmentConsole);

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

                foreach (var member in dscr.Members) {
                    var memberResult = member.Key;

                    // is Method?
                    if (member.Value is OverloadedMethodMemberDescriptor) {
                        var declaringType = (member.Value as OverloadedMethodMemberDescriptor).DeclaringType;

                        try {
                            var m1 = declaringType.GetMethod(member.Key, new[] { typeof(string) });
                            //var m1 = declaringType.GetMethod(member.Key);

                            if (m1 == null) {
                                result.Add(memberResult);
                                continue;
                            }
                            var methodparams = m1.GetParameters();

                            memberResult += "(";
                            foreach (var param in methodparams) {
                                if (memberResult[memberResult.Length - 1] != '(') {
                                    memberResult += ",";
                                }
                                memberResult += param.Name;
                            }
                            memberResult += ")";
                        }
                        catch (Exception e) {
                            Debug.LogWarning("Problem is processing userdata/members with memberName:" + member.Key);
                            Debug.LogException(e);
                        }
                    }
                    result.Add(memberResult);
                }
            } else {
                Debug.LogWarning("Proposal from Userdata cannot process UserData-Descriptor-Type:" + userdata.Descriptor.GetType().ToString());
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
            // TODO: Make all this process in Regular expression? Not sure how this special case could be mapped to a regex

            if (all.Length == 0) {
                // nothing to do
                start = 0;
                endPos = 0;
                firstPart = "";
                secondPart = "";
                return;
            }


            string first = "";
            string second = "";

            bool checkFirst = true;
            while (cursorPos >= 0 && all[cursorPos]!='.') {
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
                while (cursorPos >= 0 && !delimiters.Contains(all[cursorPos])) {
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
                foreach (var key in mainScript.Globals.Keys) {
                    var val = mainScript.Globals[key];
                    int a = 0;
                }
            }

            if (currentProposals == null) {
                return null;
            }

            result.proposalElements = currentProposals.FindAll(prop => lastInputPart.Length == 0 || prop.StartsWith(lastInputPart))
                           .Where(elem=>!elem.StartsWith("__new") && !elem.StartsWith("ToString") && !elem.StartsWith("Dispose") && !elem.StartsWith("Equals") && !elem.StartsWith("GetHashCode") && !elem.StartsWith("GetType") )
                           .Select(elem => new ProposalElement() { simple = elem, full = (currentObjectPath==""?elem:currentObjectPath + "." + elem) })
                           .ToList();

            result.replaceStringStart = Math.Max(0,from);
            result.replaceStringEnd = Math.Min(currentInput.Length,to);
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

        public override void WriteToScriptingConsole(string text) {}

        public override void OpenScriptingConsole() {
            devConsoleActive = true;
            Publish(activateDevelopmentConsole);
        }

		public override void CloseScriptingConsole() {
            devConsoleActive = false;
            Publish(deactivateDevelopmentConsole);
        }

        public override void ToggleScriptingConsole() {
            if (!devConsoleActive)  {
                OpenScriptingConsole();
            } else {
                CloseScriptingConsole();
            }
        }

        public override Script GetMainScript() {
            return mainScript;
        }

        public override bool IsScriptingConsoleVisible() {
            return devConsoleActive;
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
