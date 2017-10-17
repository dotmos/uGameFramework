using System;
using Newtonsoft.Json;


namespace Service.Serializer{
    public class JsonNetSerializerService : ServiceBase,ISerializerService {

        JsonSerializerSettings settings;

        protected override void AfterBind() {
            base.AfterBind();

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

        /// <summary>
        /// Serialize the specified obj to a json string
        /// </summary>
        /// <param name="obj">Object.</param>
        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, settings);
        }

        /// <summary>
        /// Deserialize json into existing instance
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="json">Json.</param>
        public void DeserializeToInstance(object instance, string data)
        {
            
            //Create new object
            object _o = JsonConvert.DeserializeObject(data, instance.GetType(), settings);

            //Copy from new object to instance.
            //Reflection.CopyProperties(_o, instance);
            //Reflection.CopyFields(_o, instance);
            if(_o is MVC.IModel) {
                ((MVC.IModel)_o).Bind();
            }
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
            return JsonConvert.DeserializeObject<TObject>(data, settings);
            //return (TObject)Jil.JSON.DeserializeDynamic(data);
        }
        
    }
}