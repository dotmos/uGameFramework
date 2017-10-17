using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

/// <summary>
/// Helper class to serialize Color, since DataContractSerializer can not correcty deserialize Color.
/// </summary>
[DataContract]
public class ColorDataContract {
    [DataMember]
    public float r;
    [DataMember]
    public float g;
    [DataMember]
    public float b;
    [DataMember]
    public float a;

    public ColorDataContract() {

    }

    public ColorDataContract(float r, float g, float b, float a) {
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = a;
    }

    public static ColorDataContract ToDataContract(Color c) {
        return new ColorDataContract(c.r, c.g, c.b, c.a);
    }

    public static Color FromDataContract(ColorDataContract v) {
        return new Color(v.r, v.g, v.b, v.a);
    }

}
