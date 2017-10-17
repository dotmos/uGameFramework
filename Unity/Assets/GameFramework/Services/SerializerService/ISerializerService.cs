using System.Collections;
using System;

namespace Service.Serializer{
    public interface ISerializerService {
        string Serialize(object obj);

        void DeserializeToInstance(object instance, string data);
        TObject DeserializeToObject<TObject>(string data);
    }
}
