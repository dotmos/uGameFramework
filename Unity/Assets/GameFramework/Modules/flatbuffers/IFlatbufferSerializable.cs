using System;
/*
public interface IFBSerializable<T> where T : struct, FlatBuffers.IFlatbufferObject {
    FlatBuffers.Offset<T> Serialize(FlatBuffers.FlatBufferBuilder builder);
    void Deserialize(T incoming);
    void Deserialize(FlatBuffers.ByteBuffer buf);
}
*/

public interface IFBDontCache { }

public interface IFBSerializable {
    int Serialize(FlatBuffers.FlatBufferBuilder builder);
    void Deserialize(object incoming); 
    void Deserialize(FlatBuffers.ByteBuffer buf);
}

public interface IFBSerializable2
{
    int Serialize2(FlatBuffers.FlatBufferBuilder builder);
    void Deserialize2(int tblOffset, FlatBuffers.ByteBuffer builder);
    void Deserialize2(FlatBuffers.ByteBuffer buf);

    void CreateTable(FlatBuffers.FlatBufferBuilder builder);
    void Update(FlatBuffers.FlatBufferBuilder builder);

    bool IsDirty { get; set; }
}

public interface IFBSerializeOnMainThread { }

public interface IFBPostDeserialization {
    void OnPostDeserialization(ECS.IEntityManager entityManager,object userobject,int savedDataFormat, int currentDataFormat);
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
