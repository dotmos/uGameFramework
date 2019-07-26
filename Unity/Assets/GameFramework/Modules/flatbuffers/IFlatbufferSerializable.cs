using System;
/*
public interface IFBSerializable<T> where T : struct, FlatBuffers.IFlatbufferObject {
    FlatBuffers.Offset<T> Serialize(FlatBuffers.FlatBufferBuilder builder);
    void Deserialize(T incoming);
    void Deserialize(FlatBuffers.ByteBuffer buf);
}
*/

public interface IFBSerializable {
    int Serialize(FlatBuffers.FlatBufferBuilder builder);
    void Deserialize(object incoming); 
    void Deserialize(FlatBuffers.ByteBuffer buf);
}

public interface IFBPostDeserialization {
    void OnPostDeserialization(ECS.IEntityManager entityManager,object userobject);
}


/// <summary>
/// If an object (e.g.) has a version mismatch during deserialization it calls the upgrade-function to make it valid (if needed)
/// </summary>
public interface IFBUpgradeable {
    void Upgrade(int serializedFormatNr, int currentNr, object incoming);
}

/*public interface IFBSerializableManual {
    int Serialize(FlatBuffers.FlatBufferBuilder builder);
    void Deserialize(FlatBuffers.FlatBufferBuilder builder);
}*/
