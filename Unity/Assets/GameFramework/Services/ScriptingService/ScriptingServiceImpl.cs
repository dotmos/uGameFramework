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
using ECS;
using FlatBuffers;
using Service.Serializer;

namespace Service.Scripting {

    public partial class LuaCoroutine {
        public DynValue Resume(params object[] objs) {
            if (co==null || co.Coroutine.State == CoroutineState.Dead) {
                Debug.Log("dead lua co routine!");
                co = null;
                return null;
                // TODO discard
            }
            DynValue result = co.Coroutine.Resume(objs);
            if (result.Type == DataType.Tuple) {
                DynValue[] yieldValues = result.Tuple;
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



        Service.DevUIService.IDevUIService devUIService;
        Service.DevUIService.IDevUIService DevUI {
            get {
                if (devUIService == null) devUIService = Kernel.Instance.Resolve<Service.DevUIService.IDevUIService>();
                return devUIService;
            }
        }

        private ECS.IEntityManager entitymanager;
        ECS.IEntityManager EntityManager {
            get {
                if (entitymanager == null) entitymanager = Kernel.Instance.Resolve<ECS.IEntityManager>();
                return entitymanager;
            }
        }

        private Func<float> getCurrentGameTime = null;

        private List<LuaCoroutine> coRoutines = new List<LuaCoroutine>();

        private Script mainScript;
        //private UserInterface.DevelopmentConsoleComponent devConsoleComponent;
        private static readonly HashSet<char> delimiters = new HashSet<char>() { '(', ')', ',', '=', ';', ' ', '+' };

        private List<Action<string, object, object>> callbacks = new List<Action<string, object, object>>();
        private List<Func<LuaCoroutine,bool>> customYieldsChecks = new List<Func<LuaCoroutine,bool>>();
        private ScriptingServiceData data = new ScriptingServiceData();
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
            DynValue result = mainScript.DoString(luaCode);
            return result;
        }

        public override string ExecuteStringOnMainScript(string luaCode) {
            try {
                DynValue result = ExecuteStringOnMainScriptRaw(luaCode);
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
                List<string> userDataResult = Proposal(dynvalue.UserData);
                return userDataResult;
            } else {
                //Debug.LogWarning("Could not process proposal for Input:" + currentInput);
                return null;
            }
        }

        private List<string> Proposal(UserData userdata) {
            List<string> result = new List<string>();
            if (userdata.Descriptor is StandardUserDataDescriptor) {
                StandardUserDataDescriptor dscr = (StandardUserDataDescriptor)userdata.Descriptor;


                if (userdata.Object == null) {
                    string st = dscr.MemberNames.First();
                    if (dscr.Members.Count()>0 && dscr.MemberNames.First() == "__new") {
                        result.Add("__new()");
                    }
                    return result;
                }
                System.Reflection.MethodInfo[] methods = userdata.Object.GetType().GetMethods();
                foreach (System.Reflection.MethodInfo m in methods) {
                    string memberResult = m.Name;
                    System.Reflection.ParameterInfo[] methodparams = m.GetParameters();

                    memberResult += "(";
                    foreach (System.Reflection.ParameterInfo param in methodparams) {
                        if (memberResult[memberResult.Length - 1] != '(') {
                            memberResult += ",";
                        }
                        memberResult += param.Name;
                    }
                    memberResult += ")";

                    result.Add(memberResult);
                }
                System.Reflection.FieldInfo[] fields = userdata.Object.GetType().GetFields();
                foreach (System.Reflection.FieldInfo f in fields) {
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
                StandardEnumUserDataDescriptor dscr = (StandardEnumUserDataDescriptor)userdata.Descriptor;
                foreach (string ev in dscr.MemberNames) {
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
            Proposal result = new Proposal();

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
                    DynValue currentLuaObject = mainScript.Globals.Get(currentObjectPath);
                    currentProposals = Proposal(currentLuaObject,currentInput);
                }
                catch (Exception exCurrentObject) {
                    Debug.LogException(exCurrentObject);
                    return null;
                }
            } else {
                // check root-global
                currentProposals = mainScript.Globals.Keys.Select(val=>val.String).ToList();
                foreach (DynValue key in mainScript.Globals.Keys) {
                    object val = mainScript.Globals[key];
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

                string input = useScriptDomain
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
            foreach (LuaCoroutine lCo in coRoutines) {
                if (lCo.waitForType == "callback" && (string)lCo.value1 == cbType) {
                    DynValue result = lCo.Resume(cbType, o2,o3);
                    if (result == null) {
                        removeCoRoutine.Add(lCo);
                    }
                }
            }
            if (removeCoRoutine.Count > 0) {
                foreach (LuaCoroutine removeCo in removeCoRoutine) {
                    coRoutines.Remove(removeCo);
                }
                removeCoRoutine.Clear();
            }
        }

        public override void Callback(string cbType, object o2 = null, object o3 = null) {
            foreach (Action<string, object, object> cb in callbacks) {
                cb(cbType, o2, o3);
            }
        }

        List<LuaCoroutine> removeCoRoutine = new List<LuaCoroutine>();


        public override void Setup(bool isNewGame) {
            if (isNewGame) {
                if (data == null) data = new ScriptingServiceData();
                data.replayScript.Clear();
            }
        }

        public override void Tick(float dt) {
#if !NO_LUATESTING
            object tickFunc = mainScript.Globals["tick"];
            if (tickFunc != null) {
                mainScript.Call(tickFunc, dt);
            }
            
            foreach (LuaCoroutine coRoutine in coRoutines) {
                if (coRoutine.waitForType == "waitFrames") {
                    if ( (double)coRoutine.value1>0) {
                        coRoutine.value1 = (double)coRoutine.value1 - 1;
                    } else {
                        DynValue result = coRoutine.Resume();
                        if (result == null) {
                            removeCoRoutine.Add(coRoutine);
                        }
                    }
                }
                else if (coRoutine.waitForType == "waitSecs") {
                    if ( (double)coRoutine.value1 > 0.0f) {
                        coRoutine.value1 = (double)coRoutine.value1 - dt;
                    } else {
                        DynValue result = coRoutine.Resume();
                        if (result == null) {
                            removeCoRoutine.Add(coRoutine);
                        }
                    }
                }
                else {
                    foreach (Func<LuaCoroutine, bool> customYield in customYieldsChecks) {
                        bool finished = customYield(coRoutine);
                        if (finished) {
                            // finished
                            DynValue result = coRoutine.Resume();
                            if (result == null) {
                                removeCoRoutine.Add(coRoutine);
                            }
                            break;
                        }
                    }
                }
            }
            if (removeCoRoutine.Count > 0) {
                foreach (LuaCoroutine removeCo in removeCoRoutine) {
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

            LuaCoroutine luaCo = new LuaCoroutine() {
                co = coroutine
            };
            DynValue result = luaCo.Resume(); // first step
            if (result != null) {
                coRoutines.Add(luaCo);
            }
            return luaCo;
        }

        public override IComponent GetComponent(UID entity, string componentName) {
            List<IComponent> comps = EntityManager.GetAllComponents(entity);
            foreach (IComponent comp in comps) {
                var compName = comp.GetType().Name;
                if (compName == componentName) {
                    return comp;
                }
            }
            return null;
        }
        public override void RegisterCustomYieldCheck(Func<LuaCoroutine,bool> yieldCheck) {
            customYieldsChecks.Add(yieldCheck);
        }

        public override Script GetMainScript() {
            return mainScript;
        }

        public override void StartLog(string filename) {

        }



        public override void WriteLog(string outputString, bool alsoToConsole = true) {
            DevUI.WriteToScriptingConsole(outputString);
        }


        public override void ActivateLuaReplayScript(bool activate) {
            data.saveReplayScript = activate;
        }

        public override bool LuaScriptActivated() {
            return data.saveReplayScript;
        }

        public override StringBuilder GetLuaReplayStringBuilder() {
            return data.replayScript;
        }

        public override string GetCurrentLuaReplay() {
            string finalScript = "uID={} function getUID(id) return uID[id] end\n\nfunction executeLogic()\n" + data.replayScript.ToString() + "end \n\n Scripting.CreateCoroutine(executeLogic)-- start the logic - function";
            return finalScript;
        }

        public override void SetLuaReplayStringBuilder(StringBuilder replayScript) {
            data.replayScript = replayScript;
        }

        private void ReplayWrite_finalize(String filename) {
            string finalScript = GetCurrentLuaReplay();
            filesystem.WriteStringToFileAtDomain(FileSystem.FSDomain.Scripting, filename, finalScript);
        }

        public override void SaveCurrentLuaReplay(string fileName) {
            if (!data.saveReplayScript) return;
            ReplayWrite_finalize(fileName);
        }

        private void ReplayWrite_WaitForCurrentGameTime() {
            if (getCurrentGameTime == null) {
                Debug.LogError("Tried to write currentGameTime to lua-replay, but there is no getCurrentGametime-func");
                return;
            }
            float gametime = getCurrentGameTime();
            data.replayScript.Append($"coroutine.yield('waitForGameTime',{gametime})\n");
        }
        public override void ReplayWrite_CustomLua(string luaScript, bool waitForGameTime = true) {
            if (waitForGameTime) ReplayWrite_WaitForCurrentGameTime();
            data.replayScript.Append(luaScript);
            if (!luaScript.EndsWith("\n")) {
                data.replayScript.Append('\n');
            }
        }

        public override void RegisterEntity(UID uid) {
            if (uid.IsNull()) {
                devUIService.WriteToScriptingConsole("Tried to register null-value");
                return;
            }
            data.uidCounter++;
            data.uid2creationId[uid] = data.uidCounter;
            DynValue mapper = mainScript.Globals.Get("uID");
            var tbl = mapper.Table;
            tbl[data.uidCounter] = uid;
        }

        public override int GetLUAEntityID(UID entity) {
            return data.uid2creationId[entity];
        }

        public override bool IsEntityRegistered(UID entity) {
            return data.uid2creationId.ContainsKey(entity);
        }

        public override void ReplayWrite_RegisterEntity(string entityVarName="entity") {
            ReplayWrite_WaitForCurrentGameTime();
            // todo: this is done automatically
            // data.replayScript.Append($"Scripting.RegisterEntity({entityVarName})");
        }

        public override void ReplayWrite_SetCurrentEntity(UID uid) {
            int bid = data.uid2creationId[uid];
            data.replayScript.Append($"entity=uID[{bid}]\n");
        }

        public override void SetLuaReplayGetGameTimeFunc(Func<float> getCurrentGameTime) {
            this.getCurrentGameTime = getCurrentGameTime;
        }

        public override int Serialize(FlatBufferBuilder builder) {
            return 0;
            // TODO
        }

        public override void Deserialize(object incoming) {
            base.Deserialize(incoming);
            // TODO
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
