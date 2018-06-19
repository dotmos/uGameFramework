using System;
using Newtonsoft.Json;


namespace Service.Serializer{
    public class JsonNetSerializerService : ServiceBase, ISerializerService {

        /// <summary>
        /// Static wrapper so other scripts that need this serializer can work without having to instance it.
        /// This is needed for GlobalSettings or editor scripts as an example.
        /// It avoids code redundancy in these cases.
        /// </summary>
        public static class Static {

            /// <summary>
            /// Settings file. Made static so other scripts can use it.
            /// </summary>
            private static JsonSerializerSettings settings = null;
            public static JsonSerializerSettings Settings {
                get {
                    if (settings == null) {
                        settings = new JsonSerializerSettings();
                        //Handle references and reference loops
                        settings.PreserveReferencesHandling = PreserveReferencesHandling.All;
                        //Make sure that derived classes know from which class they derived from.
                        //e.g. Person : PersonBase.
                        //Person can not be deserialized without this setting if Person and PersonBase instances are stored in the same list of type List<PersonBase>
                        settings.TypeNameHandling = TypeNameHandling.Auto;
                        //When serializing lists, create new lists instead of using old ones. We need that for ReactiveCollections. If this is set to Replace, get setter of the property will be used, instead of the getter!
                        settings.ObjectCreationHandling = ObjectCreationHandling.Replace;
                        //settings.ObjectCreationHandling = ObjectCreationHandling.Replace;
                        //settings.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
                        settings.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
                        //settings.ReferenceResolver
                    }
                    return settings;
                }
            }

            /// <summary>
            /// Serialize the specified obj to a json string
            /// </summary>
            /// <param name="obj">Object.</param>
            public static string Serialize(object obj, JsonSerializerSettings settings) {
                return JsonConvert.SerializeObject(obj, settings);
            }
            public static string Serialize(object obj) {
                return Serialize(obj, Settings);
            }

            /// <summary>
            /// Serialize the specified obj to a json string
            /// </summary>
            /// <param name="obj">Object to serialize</param>
            /// <param name="formatting">Way to format the json (should it be indented or not)</param>
            /// <returns></returns>
            public static string Serialize(object obj, Formatting formatting, JsonSerializerSettings settings) {
                return JsonConvert.SerializeObject(obj, formatting, settings);
            }
            public static string Serialize(object obj, Formatting formatting) {
                return Serialize(obj, formatting, Settings);
            }

            /// <summary>
            /// Deserializes json to new object.
            /// </summary>
            /// <param name="obj">Object.</param>
            /// <param name="json">Json.</param>
            public static TObject DeserializeToObject<TObject>(string data, JsonSerializerSettings settings) {
                return JsonConvert.DeserializeObject<TObject>(data, settings);
            }
            public static TObject DeserializeToObject<TObject>(string data) {
                return DeserializeToObject<TObject>(data, Settings);
            }

            /// <summary>
            /// Deserialize json into existing instance
            /// </summary>
            /// <returns>The to object.</returns>
            /// <param name="json">Json.</param>
            /// <typeparam name="TObject">The 1st type parameter.</typeparam>
            public static void DeserializeToInstance(object instance, string data, JsonSerializerSettings settings) {
                //Create new object
                object _o = JsonConvert.DeserializeObject(data, instance.GetType(), settings);

                //Copy from new object to instance.
                //Reflection.CopyProperties(_o, instance);
                //Reflection.CopyFields(_o, instance);
                if (_o is MVC.IModel) {
                    ((MVC.IModel)_o).Bind();
                }
                _o.CopyProperties(instance);
                //_o.CopyFields(instance);

                //Dispose copy if it is disposable
                if (_o is IDisposable) {
                    ((IDisposable)_o).Dispose();
                }
            }
            public static void DeserializeToInstance(object instance, string data) {
                DeserializeToInstance(instance, data, Settings);
            }

        }

        /// <summary>
        /// Serialize the specified obj to a json string
        /// </summary>
        /// <param name="obj">Object.</param>
        public string Serialize(object obj)
        {
            return Static.Serialize(obj);
        }

        /// <summary>
        /// Deserialize json into existing instance
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="json">Json.</param>
        public void DeserializeToInstance(object instance, string data)
        {
            Static.DeserializeToInstance(instance, data);
        }

        /// <summary>
        /// Deserializes json to new object.
        /// </summary>
        /// <returns>The to object.</returns>
        /// <param name="json">Json.</param>
        /// <typeparam name="TObject">The 1st type parameter.</typeparam>
        public TObject DeserializeToObject<TObject>(string data)
        {
            return Static.DeserializeToObject<TObject>(data);
        }
        
    }
}