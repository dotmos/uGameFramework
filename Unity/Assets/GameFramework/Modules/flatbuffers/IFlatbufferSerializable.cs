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
