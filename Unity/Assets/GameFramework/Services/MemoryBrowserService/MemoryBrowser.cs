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
using System.Reflection;

namespace Service.MemoryBrowserService {

    public class DataBrowser {
        /// <summary>
        /// Attribute to mark a class to be processed with UIDBInclude-Attributes
        /// </summary>
        public class UIDataBrowser : Attribute { }

        /// <summary>
        /// Tag fields and properties to be included into the databrowser
        /// </summary>
        public class UIDBInclude : Attribute {
            public enum Type {
                readAndWrite,
                onlyread,
                isHeader,
                subdata // this obj/list should be visualize along with the root. 
            }

            public string visualName = null;

            public Type type = Type.readAndWrite;

            public int colOrder = 255;

            public float width = 100;
        }

        private static UIDBInclude GetAttribute(FieldInfo fI) {
            UIDBInclude attrib = (UIDBInclude)Attribute.GetCustomAttribute(fI, typeof(UIDBInclude));
            return attrib;
        }

        private static UIDBInclude GetAttribute(PropertyInfo pI) {
            UIDBInclude attrib = (UIDBInclude)Attribute.GetCustomAttribute(pI, typeof(UIDBInclude));
            return attrib;
        }

        private static bool IsManaged(object obj) {
            return Attribute.GetCustomAttribute(obj.GetType(),typeof(UIDataBrowser)) != null;
        }

        public static bool SetValue(object obj,string valueName, string newValueAsString) {
            FieldInfo field = obj.GetType().GetField(valueName);
            if (field != null) {
                if (field.FieldType == typeof(Vector2)) {
                    field.SetValue(obj, UtilsExtensions.StringToVector2(newValueAsString));
                }
                else if (field.FieldType == typeof(Vector3)) {
                    field.SetValue(obj, UtilsExtensions.StringToVector3(newValueAsString));
                } 
                else if (field.FieldType.IsEnum) {
                    object enumVal = Enum.Parse(field.FieldType, newValueAsString);
                    field.SetValue(obj, enumVal);
                }
                else {
                    // simple types
                    field.SetValue(obj, Convert.ChangeType(newValueAsString, field.FieldType));
                }
            } 
            else {
                PropertyInfo prop = obj.GetType().GetProperty(valueName);
                if (prop != null) {
                    if (prop.PropertyType == typeof(Vector2)) {
                        prop.SetValue(obj, UtilsExtensions.StringToVector2(newValueAsString));
                    } else if (prop.PropertyType == typeof(Vector3) ) {
                        prop.SetValue(obj, UtilsExtensions.StringToVector3(newValueAsString));
                    } else {
                        prop.SetValue(obj, Convert.ChangeType(newValueAsString, prop.PropertyType));
                    }
                } else {
                    Debug.LogError("MemoryBrowser: Tried to set unknown value with name " + valueName );
                    return false;
                }

                // try property
            }
            return true;
        }

        public static void Traverse(object obj,Action<string,object,MemoryBrowser.ElementType,UIDBInclude> callback) {

            // process fields
            if (obj is IList) {
                // traverse list
                foreach (object val in ((IList)obj)) {
                    bool isManaged = IsManaged(val);
                    MemoryBrowser.ElementType elemType = MemoryBrowser.GetElementType(val);
                    callback(val.GetType().Name, val, elemType, null);
                    //if (!isManaged) {
                    //    // not managed ==> all elements are part of the table
                    //    callback(obj.GetType().Name, val, elemType, null);
                    //} else {
                    //    var attrib = GetAttribute(val);
                    //    if (attrib != null) {
                    //        callback(field.Name, val, elemType, attrib);
                    //    }
                    //}
                }

            } else {
                bool isManaged = IsManaged(obj);
                // traverse object
                foreach (FieldInfo field in obj.GetType().GetFields()) {
                    object val = field.GetValue(obj);
                    MemoryBrowser.ElementType elemType = MemoryBrowser.GetElementType(val);
                    if (!isManaged) {
                        // not managed ==> all elements are part of the table
                        callback(field.Name, val, elemType, null);
                    } else {
                        UIDBInclude attrib = GetAttribute(field);
                        if (attrib != null) {
                            callback(field.Name, val, elemType, attrib);
                        }
                    }
                }
                // process properties
                foreach (PropertyInfo prop in obj.GetType().GetProperties()) {
                    object val = prop.GetValue(obj);
                    MemoryBrowser.ElementType elemType = MemoryBrowser.GetElementType(obj);
                    if (!isManaged) {
                        // not managed ==> all elements are part of the table
                        callback(prop.Name, val, elemType, null);
                    } else {
                        UIDBInclude attrib = GetAttribute(prop);
                        if (attrib != null) {
                            callback(prop.Name, val, elemType, attrib);
                        }
                    }
                }
            }

        }
                        
    }




    public class MemoryBrowser : IDisposable {

        public enum ElementType
        {
            nullType, objectType, listType, dictType
        }

        [Inject]
        Scripting.IScriptingService scripting;
        [Inject]
        Service.DevUIService.IDevUIService devUiService;

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

        public static ElementType GetElementType(object obj) {
            //obj = obj==null?Current:obj;

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
            object obj = Current;

            if (obj is IDictionary) {
                IDictionary dict = (IDictionary)obj;

                Type keyType = dict.GetType().GetGenericArguments()[0];
                Type valueType = dict.GetType().GetGenericArguments()[1];

                if ( IsSimple(keyType) ) {
                    // string keys
                    foreach (string key in dict.Keys) {
                        rxCurrentSnapShot[key] = dict[key];
                    }
                }
                else {
                    int counter = 0;
                    // other keys
                    foreach (object keyObj in dict.Keys) {
                        // object-keys? create counter-[key] counter-[value] entries
                        rxCurrentSnapShot[counter + "_key"] = keyObj;
                        rxCurrentSnapShot[counter + "_value"] = dict[keyObj];
                    }
                }
            }
            else if (obj is IList) {
                IList list = (IList)obj;
                for (int i=0;i<list.Count;i++) {
                    string name = i.ToString(); // TODO: check if use [ idx ] as name?
                    rxCurrentSnapShot[name] = list[i];
                }
            } 
            else if (obj is IDictionary) {
                IDictionary dict = (IDictionary)obj;
                foreach (object key in dict.Keys){
                    string name = (string)key;
                    rxCurrentSnapShot[name] = dict[key];
                }
            } else {
                // properties
                foreach (FieldInfo field in obj.GetType().GetFields()) {
                    object val = field.GetValue(obj);
                    rxCurrentSnapShot[field.Name] = val;
                }
                foreach (PropertyInfo prop in obj.GetType().GetProperties()) {
                    object val = prop.GetValue(obj);
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
        /// Set Value by using the valueName(field or property) and the new Value as string. The value is tried to be converted properly. Return true if every worked fine
        /// </summary>
        /// <param name="valueName"></param>
        /// <param name="newValueAsString"></param>
        /// <returns></returns>
        public bool SetValue(string valueName,string newValueAsString) {
            object obj = Current;
            try {
                if (obj is IDictionary) {
                    IDictionary dict = (IDictionary)obj;

                    Type keyType = dict.GetType().GetGenericArguments()[0];
                    Type valueType = dict.GetType().GetGenericArguments()[1];


                    if ( !rxCurrentSnapShot.ContainsKey(valueName)) {
                        Debug.LogError("Unknown dictionary key:" + valueName);
                        return false;
                    }
                    object objToChange = rxCurrentSnapShot[valueName];

                    if (!IsSimple(objToChange) || !IsSimple(valueType) ) {
                        Debug.LogError("Tried to change non simple-obj");
                        return false;
                    }

                    dict[valueName]= Convert.ChangeType(newValueAsString, valueType);
                }
                else if (obj is IList) {
                    IList list = (IList)obj;

                    int idx = int.Parse(valueName);

                    if (list.GetType().IsGenericType) {
                        Type listType = list.GetType().GetGenericArguments()[0];
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
                    FieldInfo field = obj.GetType().GetField(valueName);
                    if (field != null) {
                        if (field.FieldType == typeof(Vector3)) {
                            field.SetValue(obj, UtilsExtensions.StringToVector3(newValueAsString));
                        } else {
                            // simple types
                            field.SetValue(obj, Convert.ChangeType(newValueAsString, field.FieldType));
                        }
                    } else {
                        PropertyInfo prop = obj.GetType().GetProperty(valueName);
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
                object nextObj = rxCurrentSnapShot[valueName];

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
            Type type = obj.GetType();
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
            foreach (KeyValuePair<string, object> p in rxCurrentSnapShot) {
                if (p.Value == null) {
                    devUiService.WriteToScriptingConsole(p.Key + " = null");
                } else if (IsSimple(p.Value)) {
                    devUiService.WriteToScriptingConsole(p.Key + " = " + p.Value);
                } else {
                    devUiService.WriteToScriptingConsole(p.Key + " = [object|"+p.Value.GetType().Name+"]");
                }
            }
        }

        public void Dispose() {
            compDisp.Dispose();
        }
    }
}
