using System;
using System.Collections.Generic;

using Zenject;
using UniRx;
using MoonSharp.Interpreter;
using UnityEngine;
using MoonSharp.Interpreter.Interop;
using System.IO;
using System.Linq;
using System.Collections;

namespace Service.MemoryBrowserService {

    public class MemoryBrowser : IDisposable {

        public enum ElementType
        {
            nullType, objectType, listType, dictType
        }

        [Inject]
        Scripting.IScriptingService scripting;

        public ReactiveCollection<object> rxBreadcrumbs = new ReactiveCollection<object>();
        public ReactiveProperty<object> currentProperty = new ReactiveProperty<object>();
        public ReactiveDictionary<string, object> rxCurrentSnapShot = new ReactiveDictionary<string, object>();

        private CompositeDisposable compDisp = new CompositeDisposable();

        public object Current {
            get { return currentProperty.Value; }
            private set { currentProperty.Value = value; }
        }

        public MemoryBrowser(object root) {
            Kernel.Instance.Inject(this);

            // TODO prevent simple-values here
            rxBreadcrumbs.Add(root);

            Current = root; // this will trigger the UpdateCurrentSnapshot

            currentProperty.DistinctUntilChanged().Subscribe(_ => {
                rxCurrentSnapShot.Clear();
                UpdateCurrentSnapshot();
            }).AddTo(compDisp);

        }

        public ElementType GetElementType(object obj) {
            obj = obj==null?Current:obj;

            if (obj == null) {
                return ElementType.nullType;
            }
            else if (obj is IList) {
                return ElementType.listType;
            } 
            else if (obj is IDictionary) {
                return ElementType.dictType;
            }
            else {
                return ElementType.objectType;
            }
        }

        /// <summary>
        /// Update the data for the current values visibile in the memory browser
        /// </summary>
        /// <returns></returns>
        public ReactiveDictionary<string,object> UpdateCurrentSnapshot() {
            var obj = Current;

            if (obj is IDictionary) {
                var dict = (IDictionary)obj;

                var keyType = dict.GetType().GetGenericArguments()[0];
                var valueType = dict.GetType().GetGenericArguments()[1];

                if ( IsSimple(keyType) ) {
                    // string keys
                    foreach (string key in dict.Keys) {
                        rxCurrentSnapShot[key] = dict[key];
                    }
                }
                else {
                    int counter = 0;
                    // other keys
                    foreach (var keyObj in dict.Keys) {
                        // object-keys? create counter-[key] counter-[value] entries
                        rxCurrentSnapShot[counter + "_key"] = keyObj;
                        rxCurrentSnapShot[counter + "_value"] = dict[keyObj];
                    }
                }
            }
            else if (obj is IList) {
                var list = (IList)obj;
                for (int i=0;i<list.Count;i++) {
                    var name = i.ToString(); // TODO: check if use [ idx ] as name?
                    rxCurrentSnapShot[name] = list[i];
                }
            } 
            else if (obj is IDictionary) {
                var dict = (IDictionary)obj;
                foreach (var key in dict.Keys){
                    var name = (string)key;
                    rxCurrentSnapShot[name] = dict[key];
                }
            } else {
                // properties
                foreach (var field in obj.GetType().GetFields()) {
                    var val = field.GetValue(obj);
                    rxCurrentSnapShot[field.Name] = val;
                }
                foreach (var prop in obj.GetType().GetProperties()) {
                    var val = prop.GetValue(obj);
                    rxCurrentSnapShot[prop.Name] = val;
                }
            }

            return rxCurrentSnapShot;
        }

        public object GetValue(string valueName) {
            if (rxCurrentSnapShot.ContainsKey(valueName)) {
                return rxCurrentSnapShot[valueName];
            } else {
                Debug.LogError("WARNING: you are trying to browse to a unknown value-name:" + valueName + "! Possible values :" + rxCurrentSnapShot.Keys.Aggregate((i, j) => i + " " + j));
                return null;
            }
        }

        /// <summary>
        /// Set Value by using the valueName(field or property) and the new Value as string. The value is tried to be converted properly.
        /// </summary>
        /// <param name="valueName"></param>
        /// <param name="newValueAsString"></param>
        /// <returns></returns>
        public bool SetValue(string valueName,string newValueAsString) {
            var obj = Current;
            try {
                if (obj is IDictionary) {
                    var dict = (IDictionary)obj;

                    var keyType = dict.GetType().GetGenericArguments()[0];
                    var valueType = dict.GetType().GetGenericArguments()[1];

                    Debug.LogWarning("SETTING DICTIONARIES NOT IMPLEMENTED YET!");
                    // TODO
                }
                else if (obj is IList) {
                    var list = (IList)obj;

                    int idx = int.Parse(valueName);

                    if (list.GetType().IsGenericType) {
                        var listType = list.GetType().GetGenericArguments()[0];
                        if (IsSimple(listType)) {
                            list[idx] = Convert.ChangeType(newValueAsString, listType);
                            UpdateCurrentSnapshot();
                            return true;
                        } else {
                            Debug.LogWarning("You cannot change list-elements that are not simple!! IGNORING");
                            return false;
                        }
                    } else {
                        Debug.LogWarning("You cannot change non-generic lists! IGNORING");
                        return false;
                    }
                    
                } else {
                    var field = obj.GetType().GetField(valueName);
                    if (field != null) {
                        field.SetValue(obj, Convert.ChangeType(newValueAsString, field.FieldType));
                    } else {
                        var prop = obj.GetType().GetProperty(valueName);
                        if (prop != null) {
                            prop.SetValue(obj, Convert.ChangeType(newValueAsString, prop.PropertyType));
                        } else {
                            Debug.LogError("MemoryBrowser: Tried to set unknown value with name " + valueName + "!  Possible values :" + rxCurrentSnapShot.Keys.Aggregate((i, j) => i + " " + j));
                            return false;
                        }

                        // try property
                    }
                    UpdateCurrentSnapshot();
                }

                return true;

            }
            catch (Exception e) {
                return false;
            }
        }

        /// <summary>
        /// Browse deeper into the data-structure (non simple-types)
        /// </summary>
        /// <param name="valueName"></param>
        public void Browse(string valueName) {
            if (rxCurrentSnapShot.ContainsKey(valueName)) {
                var nextObj = rxCurrentSnapShot[valueName];

                if (IsSimple(nextObj)) {
                    Debug.LogWarning("Cannot browse into a simpletype:" + valueName+"!! IGNORING!");
                    return;
                }
                rxBreadcrumbs.Add(nextObj);
                Current = nextObj; // triggers update
            } else {
                Debug.LogError("WARNING: you are trying to browse to a unknown value-name:" + valueName + "! Possible values :" + rxCurrentSnapShot.Keys.Aggregate((i, j) => i + " " + j));
            }
        }

        /// <summary>
        /// Go back on hierarchy
        /// </summary>
        public void Back() {
            if (rxBreadcrumbs.Count > 1) {
                // remove the last one
                rxBreadcrumbs.RemoveAt(rxBreadcrumbs.Count - 1);
                Current = rxBreadcrumbs[rxBreadcrumbs.Count - 1];
            } else {
                Debug.LogWarning("MemoryBrowser.Back():Tyring to move higher than root");
            }
        }

        /// <summary>
        /// Check if the specified object is simple
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static bool IsSimple(object obj) {
            var type = obj.GetType();
            return IsSimple(type);
        }

        /// <summary>
        /// Check if the specified type is simple
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsSimple(Type type) {
            return type.IsPrimitive
                || type.IsEnum
                || type.Equals(typeof(string))
                || type.Equals(typeof(decimal));
        }
        /// <summary>
        /// Convenience Method to output the current data to the console (mainly for testing)
        /// </summary>
        public void OutputToConsole() {
            foreach (var p in rxCurrentSnapShot) {
                if (p.Value == null) {
                    scripting.WriteToScriptingConsole(p.Key + " = null");
                } else if (IsSimple(p.Value)) {
                    scripting.WriteToScriptingConsole(p.Key + " = " + p.Value);
                } else {
                    scripting.WriteToScriptingConsole(p.Key + " = [object|"+p.Value.GetType().Name+"]");
                }
            }
        }

        public void Dispose() {
            compDisp.Dispose();
        }
    }
}
