using System;
using System.Collections.Generic;
using Zenject;
using UniRx;
using MoonSharp.Interpreter;
using UnityEngine;
using MoonSharp.Interpreter.Interop;
using System.IO;
using System.Linq;
using System.Text;

namespace Service.Scripting {

    public partial class LuaCoroutine {
        public DynValue Resume(params object[] objs) {
            if (co==null || co.Coroutine.State == CoroutineState.Dead) {
                Debug.Log("dead lua co routine!");
                co = null;
                return null;
                // TODO discard
            }
            var result = co.Coroutine.Resume(objs);
            if (result.Type == DataType.Tuple) {
                var yieldValues = result.Tuple;
                if (yieldValues.Length >= 1) waitForType = yieldValues[0].String;
                value1 = yieldValues.Length >= 2 ? yieldValues[1].ToObject() : null;
                value2 = yieldValues.Length >= 3 ? yieldValues[2] : null;
            }

            return result;
        }
    }
    partial class ScriptingServiceImpl : ScriptingServiceBase {


        [Inject]
        private Service.LoggingService.ILoggingService logging;
        [Inject]
        private Service.FileSystem.IFileSystemService filesystem;

        private List<LuaCoroutine> coRoutines = new List<LuaCoroutine>();

        private Script mainScript;
        //private UserInterface.DevelopmentConsoleComponent devConsoleComponent;
        private static readonly HashSet<char> delimiters = new HashSet<char>() { '(', ')', ',', '=', ';', ' ', '+' };

        private List<Action<string, object, object>> callbacks = new List<Action<string, object, object>>();
        private List<Func<LuaCoroutine,bool>> customYieldsChecks = new List<Func<LuaCoroutine,bool>>();

        /// <summary>
        /// Is the gameconsole enabled?
        /// </summary>
        protected override void AfterInitialize() {
            try {
                // make all datatypes available
                UserData.RegistrationPolicy = InteropRegistrationPolicy.Automatic;
                //UserData.DefaultAccessMode = InteropAccessMode.Reflection;

                //scriptingComponent = GameObject.Find("/ScriptingConsole").GetComponent<ScriptingConsoleComponent>();
                // this is called right after the Base-Classes Initialize-Method. _eventManager and disposableManager are set
                mainScript = new Script();

                // TODO: get rid of nextframe
                Observable.NextFrame().Subscribe(_ => ActivateDefaultScripting("script")).AddTo(disposables);

                RegisterCallback(LuaCallback); 
                RegisterCallback(LuaCoroutineCallback);
            }
            catch (Exception e) {
                Debug.LogError("COULD NOT START SCRIPTINGCONSOLE SERVICE!");
                Debug.LogException(e);
            }
        }

        public override DynValue ExecuteStringOnMainScriptRaw(string luaCode) {
            var result = mainScript.DoString(luaCode);
            return result;
        }

        public override string ExecuteStringOnMainScript(string luaCode) {
            try {
                var result = ExecuteStringOnMainScriptRaw(luaCode);
                return result.ToString();
            }
            catch (ScriptRuntimeException ex) {
                //Debug.LogException(ex);
                return "Error: "+ex.DecoratedMessage;
            }
            catch (SyntaxErrorException sex) {
               // Debug.LogException(sex);
                return "Error: " + sex.DecoratedMessage;
            }
            catch (MoonSharp.Interpreter.InterpreterException ie) {
               // Debug.LogException(ie);
                return "Error: " + ie.DecoratedMessage;
            }
        }

        private List<string> Proposal(DynValue dynvalue,string currentInput) {
            if (dynvalue.UserData != null) {
                var userDataResult = Proposal(dynvalue.UserData);
                return userDataResult;
            } else {
                //Debug.LogWarning("Could not process proposal for Input:" + currentInput);
                return null;
            }
        }

        private List<string> Proposal(UserData userdata) {
            var result = new List<string>();
            if (userdata.Descriptor is StandardUserDataDescriptor) {
                var dscr = (StandardUserDataDescriptor)userdata.Descriptor;


                if (userdata.Object == null) {
                    var st = dscr.MemberNames.First();
                    if (dscr.Members.Count()>0 && dscr.MemberNames.First() == "__new") {
                        result.Add("__new()");
                    }
                    return result;
                }
                var methods = userdata.Object.GetType().GetMethods();
                foreach (var m in methods) {
                    var memberResult = m.Name;
                    var methodparams = m.GetParameters();

                    memberResult += "(";
                    foreach (var param in methodparams) {
                        if (memberResult[memberResult.Length - 1] != '(') {
                            memberResult += ",";
                        }
                        memberResult += param.Name;
                    }
                    memberResult += ")";

                    result.Add(memberResult);
                }
                var fields = userdata.Object.GetType().GetFields();
                foreach (var f in fields) {
                    result.Add(f.Name);
                }

                /*
                //NOTE: This code results in AmbiguousMatchException
                foreach (var member in dscr.Members) {
                    var memberResult = member.Key;

                    // is Method?
                    if (member.Value is OverloadedMethodMemberDescriptor) {
                        var declaringType = (member.Value as OverloadedMethodMemberDescriptor).DeclaringType;

                        try {
                            var m1 = declaringType.GetMethod(member.Key); //NOTE: Will trigger AmbiguousMatchException if two functions of same name but with different parameters are found
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
                */
            } else if (userdata.Descriptor is StandardEnumUserDataDescriptor) {
                var dscr = (StandardEnumUserDataDescriptor)userdata.Descriptor;
                foreach (var ev in dscr.MemberNames) {
                    result.Add(ev.ToString());
                }
            }
            else {
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
            cursorPos = endPos>0?endPos:0;
            try {
                while (cursorPos < all.Length && !delimiters.Contains(all[cursorPos])) {
                    cursorPos++;
                }
            }
            catch (Exception e) {
                Debug.LogException(e);
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
                    //int a = 0;
                }
            }

            if (currentProposals == null) {
                return null;
            }

            result.proposalElements = currentProposals.FindAll(prop => lastInputPart.Length == 0 || prop.StartsWith(lastInputPart))
                           .Where(elem=>/*!elem.StartsWith("__new") &&*/ !elem.StartsWith("ToString") && !elem.StartsWith("Dispose") && !elem.StartsWith("Equals") && !elem.StartsWith("GetHashCode") && !elem.StartsWith("GetType") )
                           .Select(elem => new ProposalElement() { simple = elem, full = (currentObjectPath==""?elem:currentObjectPath + "." + elem) })
                           .ToList();

            result.replaceStringStart = Math.Max(0,from);
            result.replaceStringEnd = Math.Min(currentInput.Length,to);
            return result;
        }

        public override string ExecuteFileToMainScript(string fileName, bool useScriptDomain=false) {
            try {
                // TODO: User DoFile with corresponding platform-controller
                //var result = mainScript.DoFile(fileName);
                //return result.ToString();


                //var input = File.ReadAllText(fileName);

                var input = useScriptDomain
                                ? filesystem.LoadFileAsStringAtDomain(FileSystem.FSDomain.Scripting, fileName)
                                : filesystem.LoadFileAsString(fileName);

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

        public override void RegisterCallback(Action<string, object, object> cbCallbackAction) {
            callbacks.Add(cbCallbackAction);
        }

        /// <summary>
        /// Default callback to forward callbacks to the lua side
        /// </summary>
        /// <param name="cbType"></param>
        /// <param name="o2"></param>
        /// <param name="o3"></param>
        private void LuaCallback(string cbType, object o2 = null, object o3 = null) {
            // give it to the lua side (callback-lua func in definied 
            mainScript.Call(mainScript.Globals["__callback"], cbType, o2, o3);
        }

        
        private void LuaCoroutineCallback(string cbType, object o2 = null, object o3 = null) {
            removeCoRoutine.Clear();
            // give it to the lua side (callback-lua func in definied
            foreach (var lCo in coRoutines) {
                if (lCo.waitForType == "callback" && (string)lCo.value1 == cbType) {
                    var result = lCo.Resume(cbType, o2,o3);
                    if (result == null) {
                        removeCoRoutine.Add(lCo);
                    }
                }
            }
            if (removeCoRoutine.Count > 0) {
                foreach (var removeCo in removeCoRoutine) {
                    coRoutines.Remove(removeCo);
                }
                removeCoRoutine.Clear();
            }
        }

        public override void Callback(string cbType, object o2 = null, object o3 = null) {
            foreach (var cb in callbacks) {
                cb(cbType, o2, o3);
            }
        }

        List<LuaCoroutine> removeCoRoutine = new List<LuaCoroutine>();

        public override void Tick(float dt) {
#if !NO_LUATESTING
            var tickFunc = mainScript.Globals["tick"];
            if (tickFunc != null) {
                mainScript.Call(tickFunc, dt);
            }
            
            foreach (var coRoutine in coRoutines) {
                if (coRoutine.waitForType == "waitFrames") {
                    if ( (double)coRoutine.value1>0) {
                        coRoutine.value1 = (double)coRoutine.value1 - 1;
                    } else {
                        var result = coRoutine.Resume();
                        if (result == null) {
                            removeCoRoutine.Add(coRoutine);
                        }
                    }
                }
                else if (coRoutine.waitForType == "waitSecs") {
                    if ( (double)coRoutine.value1 > 0.0f) {
                        coRoutine.value1 = (double)coRoutine.value1 - dt;
                    } else {
                        var result = coRoutine.Resume();
                        if (result == null) {
                            removeCoRoutine.Add(coRoutine);
                        }
                    }
                }
                else {
                    foreach (var customYield in customYieldsChecks) {
                        var finished = customYield(coRoutine);
                        if (finished) {
                            // finished
                            var result = coRoutine.Resume();
                            if (result == null) {
                                removeCoRoutine.Add(coRoutine);
                            }
                            break;
                        }
                    }
                }
            }
            if (removeCoRoutine.Count > 0) {
                foreach (var removeCo in removeCoRoutine) {
                    coRoutines.Remove(removeCo);
                }
                removeCoRoutine.Clear();
            }
#endif
        }

        public override LuaCoroutine CreateCoroutine(DynValue coFunc) {
            //var coFunc = mainScript.Globals.Get(funcName);

            if (coFunc == null || coFunc.Type!=DataType.Function) return null;

            DynValue coroutine = mainScript.CreateCoroutine(coFunc);

            var luaCo = new LuaCoroutine() {
                co = coroutine
            };
            var result = luaCo.Resume(); // first step
            if (result != null) {
                coRoutines.Add(luaCo);
            }
            return luaCo;
        }

        public override void RegisterCustomYieldCheck(Func<LuaCoroutine,bool> yieldCheck) {
            customYieldsChecks.Add(yieldCheck);
        }

        public override Script GetMainScript() {
            return mainScript;
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
