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
    void OnPostDeserialization();
}


public interface IFBConvertible {
    void convert(object incoming);
}

/*public interface IFBSerializableManual {
    int Serialize(FlatBuffers.FlatBufferBuilder builder);
    void Deserialize(FlatBuffers.FlatBufferBuilder builder);
}*/
