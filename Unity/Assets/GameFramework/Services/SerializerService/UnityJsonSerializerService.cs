using UnityEngine;

namespace Service.Serializer{
    public class UnityJsonSerializerService : ISerializerService {
        
        //NOTE: UnityEngine.JSONUtility only serializes Fields

        /// <summary>
        /// Serialize the specified obj to a json string
        /// </summary>
        /// <param name="obj">Object.</param>
        public string Serialize(object obj)
        {
            return JsonUtility.ToJson(obj);
        }

        /// <summary>
        /// Deserialize json into existing instance
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="json">Json.</param>
        public void DeserializeToInstance(object instance, string data)
        {
            JsonUtility.FromJsonOverwrite(data, instance);
        }

        /// <summary>
        /// Deserializes json to new object.
        /// </summary>
        /// <returns>The to object.</returns>
        /// <param name="json">Json.</param>
        /// <typeparam name="TObject">The 1st type parameter.</typeparam>
        public TObject DeserializeToObject<TObject>(string data)
        {
            return JsonUtility.FromJson<TObject>(data);
        }
    }
}