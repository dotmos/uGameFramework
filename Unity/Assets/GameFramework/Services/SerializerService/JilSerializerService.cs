using System;


namespace Service.Serializer{
    public class JilSerializerService : ISerializerService {

        

        /// <summary>
        /// Serialize the specified obj to a json string
        /// </summary>
        /// <param name="obj">Object.</param>
        public string Serialize(object obj)
        {
            //return JsonMapper.ToJson(obj);
            return Jil.JSON.Serialize(obj);
        }

        /// <summary>
        /// Deserialize json into existing instance
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="json">Json.</param>
        public void DeserializeToInstance(object instance, string data)
        {
            
            //Create new object
            object _o = Jil.JSON.Deserialize(data, instance.GetType());

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
            return Jil.JSON.Deserialize<TObject>(data);
            //return (TObject)Jil.JSON.DeserializeDynamic(data);
        }
        
    }
}