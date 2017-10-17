using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

/// <summary>
/// Helper class to serialize vector3, since DataContractSerializer can not correcty (de)serialize Vector3. Also, DataContractSerializer will serialize kEpsilon (which is static and always the same) and therefor not needed.
/// </summary>
[DataContract]
public class Vector2DataContract {
    [DataMember]
    public float x;
    [DataMember]
    public float y;

    public Vector2DataContract() {

    }

    public Vector2DataContract(float x, float y) {
        this.x = x;
        this.y = y;
    }

    public static Vector2DataContract ToDataContract(Vector2 v) {
        return new Vector2DataContract(v.x, v.y);
    }

    public static Vector2 ToVector2(Vector2DataContract v) {
        return new Vector2(v.x, v.y);
    }

}
