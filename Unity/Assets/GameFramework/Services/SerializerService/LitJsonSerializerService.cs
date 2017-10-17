using LitJson;
//using System.Collections;
//using System.Collections.Generic;
//using System;
using System.Reflection;
//using Zenject;
using System;
using Zenject;

namespace Service.Serializer{
    public class LitJsonSerializerService : ISerializerService {

        //NOTE: This is now using the Reflection.CopyFields and Reflection.CopyProperties custom extensions. The original code is still kept, in case there are any serialization problems in the future
        //NOTE2: LitJson -> JsonMapper was changed: New function ToObject(string json, Type objectType). The original implementation only supported a generic Type ToObject
        //NOTE3: UnityEngine.JSONUtility does not serialize everything. Meh.
        [Inject]
        void Initialize() {
            JsonMapper.RegisterExporter<float>((obj, writer) => writer.Write(Convert.ToDouble(obj)));
            JsonMapper.RegisterImporter<double, float>(input => Convert.ToSingle(input));
        }

        /// <summary>
        /// Serialize the specified obj to a json string
        /// </summary>
        /// <param name="obj">Object.</param>
        public string Serialize(object obj)
        {
            return JsonMapper.ToJson(obj);
        }

        /// <summary>
        /// Deserialize json into existing instance
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="json">Json.</param>
        public void DeserializeToInstance(object instance, string data)
        {
            //Create new object
            object _o = JsonMapper.ToObject(data, instance.GetType());

            //Copy from new object to instance.
            //Reflection.CopyProperties(_o, instance);
            //Reflection.CopyFields(_o, instance);
            _o.CopyProperties(instance);
            //_o.CopyFields(instance);

            //Dispose copy if it is disposable
            if(_o is IDisposable){
                ((IDisposable)_o).Dispose();
            }
        }

        /// <summary>
        /// Deserializes json to new object.
        /// </summary>
        /// <returns>The to object.</returns>
        /// <param name="json">Json.</param>
        /// <typeparam name="TObject">The 1st type parameter.</typeparam>
        public TObject DeserializeToObject<TObject>(string data)
        {
            return JsonMapper.ToObject<TObject>(data);// UnityEngine.JsonUtility.FromJson<TObject>(json);
        }


        /*
        private IDictionary<Type, IDictionary<string,PropertyMetadata>> typeProperties;
        private static readonly object type_properties_lock = new Object ();

        public JsonSerializerService(){
            typeProperties = new Dictionary<Type, IDictionary<string,PropertyMetadata>> ();
        }
            
        //helper class to cache properties and fields for given type
        private void AddTypeProperties (Type type)
        {
            if (typeProperties.ContainsKey (type))
                return;
            
            IDictionary<string,PropertyMetadata> props = new Dictionary<string, PropertyMetadata> ();

            foreach (PropertyInfo pInfo in type.GetProperties()) {
                if (pInfo.Name == "Item")
                    continue;
                
                PropertyMetadata pData = new PropertyMetadata ();
                pData.Info = pInfo;
                pData.IsField = false;
                pData.Type = pInfo.PropertyType;
                props.Add (pInfo.Name, pData);
            }

            foreach (FieldInfo fInfo in type.GetFields()) {
                PropertyMetadata pData = new PropertyMetadata ();
                pData.Info = fInfo;
                pData.IsField = true;

                props.Add (fInfo.Name, pData);
            }

            //typeProperties.Add (type, props);
            lock (type_properties_lock) {
                try {
                    typeProperties.Add (type, props);
                } catch (ArgumentException) {
                    return;
                }
            }

        }

        /// <summary>
        /// Serialize the specified obj to a json string
        /// </summary>
        /// <param name="obj">Object.</param>
        public string Serialize(object obj)
        {
            return JsonMapper.ToJson(obj);
        }

        void DeserializeFromInstance(ref object instance, JsonData jData)
        {
            //If jData is a value type, just set instance to it and return
            if( !(jData.IsArray || jData.IsObject) )
            {
                if(jData.IsBoolean)
                {
                    instance = (bool)jData;
                    return;
                }
                if(jData.IsDouble)
                {
                    instance = (double)jData;
                    return;
                }
                if(jData.IsInt)
                {
                    instance = (int)jData;
                    return;
                }
                if(jData.IsLong)
                {
                    instance = (long)jData;
                    return;
                }
                if(jData.IsString)
                {
                    instance = (string)jData;
                    return;
                }
            }
            //----------------------------------------------------------------------------
            //Otherwise try to deconstruct jData

            //Get type of instance
            Type instanceType = instance.GetType();

            //Cache instance type Properties
            AddTypeProperties(instanceType);
            IDictionary<string, PropertyMetadata> props = typeProperties[instanceType];

            //jData cache
            JsonData jDataCache;

            foreach(string s in jData.Keys)
            {
                //cache jData
                jDataCache = jData[s];
                //stop here if there is no data and set instance value to null/default value
                if(jDataCache == null)
                {
                    if(props.ContainsKey(s))
                    {
                        if(props[s].IsField)
                        {
                            FieldInfo fInfo = instanceType.GetField(s);
                            //fInfo.SetValue(instance, fInfo.FieldType.
                            if (fInfo.FieldType.IsValueType) fInfo.SetValue(instance, Activator.CreateInstance(fInfo.FieldType));
                            else fInfo.SetValue(instance, null);
                        }
                        else
                        {
                            PropertyInfo pInfo = instanceType.GetProperty(s);
                            if (pInfo.PropertyType.IsValueType) pInfo.SetValue(instance, Activator.CreateInstance(pInfo.PropertyType), null);
                            else pInfo.SetValue(instance, null, null);
                        }
                    }
                    //Stop
                    continue;
                }


                //Boolean
                if(jDataCache.IsBoolean)
                {
                    if(props.ContainsKey(s))
                    {
                        if(props[s].IsField) instanceType.GetField(s).SetValue(instance, (bool)jDataCache);
                        else instanceType.GetProperty(s).SetValue(instance, (bool)jDataCache, null);
                    }
                }
                //Double
                else if(jDataCache.IsDouble)
                {
                    if(props.ContainsKey(s))
                    {
                        if(props[s].IsField) instanceType.GetField(s).SetValue(instance, (double)jDataCache);
                        else instanceType.GetProperty(s).SetValue(instance, (double)jDataCache, null);
                    }
                }
                //Integer
                else if(jDataCache.IsInt)
                {
                    if(props.ContainsKey(s))
                    {
                        if(props[s].IsField) instanceType.GetField(s).SetValue(instance, (int)jDataCache);
                        else instanceType.GetProperty(s).SetValue(instance, (int)jDataCache, null);
                    }
                }
                //Long
                else if(jDataCache.IsLong)
                {
                    if(props.ContainsKey(s))
                    {
                        if(props[s].IsField) instanceType.GetField(s).SetValue(instance, (long)jDataCache);
                        else instanceType.GetProperty(s).SetValue(instance, (long)jDataCache, null);
                    }
                }
                //String
                else if(jDataCache.IsString)
                {
                    if(props.ContainsKey(s))
                    {
                        if(props[s].IsField) instanceType.GetField(s).SetValue(instance, (string)jDataCache);
                        else instanceType.GetProperty(s).SetValue(instance, (string)jDataCache, null);
                    }
                }
                //Array/List/Dict
                else if(jDataCache.IsArray)
                {
                    if(props.ContainsKey(s))
                    {
                        // ------------------------ Field ------------------------------------------------------
                        if(props[s].IsField)
                        {
                            //Get current value of list/array/dict
                            FieldInfo fInfo = instanceType.GetField(s);
                            object _obj = fInfo.GetValue(instance);
                            //create list if current value is null
                            if(_obj == null && HasDefaultConstructor(fInfo.FieldType)) _obj = Activator.CreateInstance(fInfo.FieldType);

                            //Reflect _obj to list object so we can add stuff to it (either List or ArrayList)
                            IList list = null;
                            //Clear all current list entries if there is data
                            if(_obj != null)
                            {
                                ((IList) _obj).Clear();
                                list = (IList) _obj;
                            }

                            //By now we either have an instance if it is a list (list != null), or we have a partially filled array (_obj is Array), or an empty array (list == null)
                            bool isArray = (_obj is Array || list == null);

                            //Get type of array/list content
                            Type listItemType;
                            if(isArray)
                                listItemType = fInfo.FieldType.GetElementType(); //Type of array content
                            else
                                listItemType = fInfo.FieldType.GetProperty("Item").PropertyType; //Type of List content

                            //Use an arraylist if this is an array so we can add content to it using .Add(..) instead of manually resizing the original array all the time
                            if(isArray) list = new ArrayList();

                            //Check if list content has a constructor. If this is the case, it is an object instead of a valueType
                            bool listItemHasDefaultConstructor = HasDefaultConstructor(listItemType);

                            for(int i=0; i<jDataCache.Count; ++i)
                            {
                                object listItem = null;
                                //If list item has a constructor, create a new instance
                                if(listItemHasDefaultConstructor) listItem = Activator.CreateInstance(listItemType);
                                //Fill item with data
                                DeserializeFromInstance(ref listItem, jDataCache[i]);
                                //Add item to list
                                list.Add( listItem );
                            }

                            //Set data (finally!)
                            if(isArray) //Create array from list if target field is an array
                            {
                                int n = list.Count;
                                object array = Array.CreateInstance (listItemType, n);

                                for (int i = 0; i < n; i++)
                                    ((Array) array).SetValue (list[i], i);

                                //Set instance value
                                fInfo.SetValue(instance, array);
                            }
                            else fInfo.SetValue(instance, list); //if target field is a list, just set instance value
                        }
                        // ------------------------ Property ------------------------------------------------------
                        else
                        {
                            //Get current value of list/array/dict
                            PropertyInfo pInfo = instanceType.GetProperty(s);
                            object _obj = pInfo.GetValue(instance, null);
                            //create list if current value is null
                            if(_obj == null && HasDefaultConstructor(pInfo.PropertyType)) _obj = Activator.CreateInstance(pInfo.PropertyType);

                            //Reflect _obj to list object so we can add stuff to it (either List or ArrayList)
                            IList list = null;
                            //Clear all current list entries if there is data
                            if(_obj != null)
                            {
                                ((IList) _obj).Clear();
                                list = (IList) _obj;
                            }

                            //By now we either have an instance if it is a list (list != null), or we have a partially filled array (_obj is Array), or an empty array (list == null)
                            bool isArray = (_obj is Array || list == null);

                            //Get type of array/list content
                            Type listItemType;
                            if(isArray)
                                listItemType = pInfo.PropertyType.GetElementType(); //Type of array content
                            else
                                listItemType = pInfo.PropertyType.GetProperty("Item").PropertyType; //Type of List content

                            //Use an arraylist if this is an array so we can add content to it using .Add(..) instead of manually resizing the original array all the time
                            if(isArray) list = new ArrayList();

                            //Check if list content has a constructor. If this is the case, it is an object instead of a valueType
                            bool listItemHasDefaultConstructor = HasDefaultConstructor(listItemType);

                            for(int i=0; i<jDataCache.Count; ++i)
                            {
                                object listItem = null;
                                //If list item has a constructor, create a new instance
                                if(listItemHasDefaultConstructor) listItem = Activator.CreateInstance(listItemType);
                                //Fill item with data
                                DeserializeFromInstance(ref listItem, jDataCache[i]);
                                //Add item to list
                                list.Add( listItem );
                            }

                            //Set data (finally!)
                            if(isArray) //Create array from list if target field is an array
                            {
                                int n = list.Count;
                                object array = Array.CreateInstance (listItemType, n);

                                for (int i = 0; i < n; i++)
                                    ((Array) array).SetValue (list[i], i);

                                //Set instance value
                                pInfo.SetValue(instance, array, null);
                            }
                            else pInfo.SetValue(instance, list, null); //if target field is a list, just set instance value
                        }
                    }
                }
                //Object
                else if(jDataCache.IsObject)
                {
                    if(props.ContainsKey(s))
                    {
                        // ------------------------ Field --------------------------------
                        if(props[s].IsField)
                        {
                            FieldInfo fInfo = instanceType.GetField(s);
                            object _obj = fInfo.GetValue(instance);
                            if(_obj == null)
                            {
                                //UnityEngine.Debug.LogWarning(fieldInfo.FieldType + " is null");
                                _obj = Activator.CreateInstance(fInfo.FieldType);
                                //UnityEngine.Debug.LogWarning("Trying to fix that. New value:"+_obj.ToString());
                            }
                            if(_obj != null)
                            {
                                //TODO: To deserialize Dictionaries, we can do it here (check if _obj is IDictionary, then do deserialization somehow

                                //Deserialize object
                                DeserializeFromInstance(ref _obj, jDataCache);
                                //Set instance value
                                fInfo.SetValue(instance, _obj);
                            }
                            else
                            {
                                UnityEngine.Debug.LogError("Could not deserialize: "+ s + " : " + fInfo.FieldType);
                            }
                        }
                        // ------------------------ Property --------------------------------
                        else
                        {
                            PropertyInfo pInfo = instanceType.GetProperty(s);
                            object _obj = pInfo.GetValue(instance, null);
                            if(_obj == null)
                            {
                                //UnityEngine.Debug.LogWarning(fieldInfo.FieldType + " is null");
                                _obj = Activator.CreateInstance(pInfo.PropertyType);
                                //UnityEngine.Debug.LogWarning("Trying to fix that. New value:"+_obj.ToString());
                            }
                            if(_obj != null)
                            {
                                //TODO: To deserialize Dictionaries, we can do it here (check if _obj is IDictionary, then do deserialization somehow

                                //Deserialize object
                                DeserializeFromInstance(ref _obj, jDataCache);
                                //Set instance value
                                pInfo.SetValue(instance, _obj, null);
                            }
                            else
                            {
                                UnityEngine.Debug.LogError("Could not deserialize: "+ s + " : " + pInfo.PropertyType);
                            }
                        }
                    }
                }
            }
        }

        bool HasDefaultConstructor(Type t)
        {
            return t.IsValueType || t.GetConstructor(Type.EmptyTypes) != null;
        }

        /// <summary>
        /// Deserialize json into existing instance
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="json">Json.</param>
        public void DeserializeFromInstance(object instance, string json)
        {
            //Get data for object
            JsonData jsonData = JsonMapper.ToObject(json);

            DeserializeFromInstance(ref instance, jsonData);
        }

        /// <summary>
        /// Deserializes json to new object.
        /// </summary>
        /// <returns>The to object.</returns>
        /// <param name="json">Json.</param>
        /// <typeparam name="TObject">The 1st type parameter.</typeparam>
        public TObject DeserializeToObject<TObject>(string json)
        {
            return JsonMapper.ToObject<TObject>(json);
        }
        */
    }
}