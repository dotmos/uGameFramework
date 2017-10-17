using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using UnityEngine;

namespace Service.Serializer {
    public class DataContractSerializerService : ISerializerService {
        public void DeserializeToInstance(object instance, string data) {
            throw new NotImplementedException();
        }

        public TObject DeserializeToObject<TObject>(string data) {
            return (TObject)DeserializeDataContract(data, typeof(TObject));
        }

        public string Serialize(object obj) {
            return SerializeDataContract(obj);
        }

        string SerializeDataContract(object obj) {
            using (MemoryStream memoryStream = new MemoryStream())
            using (StreamReader reader = new StreamReader(memoryStream)) {
                DataContractSerializer serializer = new DataContractSerializer(obj.GetType(), null,
                    0x7FFF, //maxItemsInObjectGraph
                    false, //ignoreExtensionDataObject
                    true, //preserveObjectReferences : this is where the magic happens
                    null //dataContractSurrogate
                );
                serializer.WriteObject(memoryStream, obj);
                memoryStream.Position = 0;
                return reader.ReadToEnd();
            }
        }

        object DeserializeDataContract(string xml, Type toType) {
            using (Stream stream = new MemoryStream()) {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(xml);
                stream.Write(data, 0, data.Length);
                stream.Position = 0;
                DataContractSerializer deserializer = new DataContractSerializer(toType);
                return deserializer.ReadObject(stream);
            }
        }
    }
}
