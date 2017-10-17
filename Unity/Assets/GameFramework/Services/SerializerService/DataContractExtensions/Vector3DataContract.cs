using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

/// <summary>
/// Helper class to serialize vector3, since DataContractSerializer can not correcty (de)serialize Vector3. Also, DataContractSerializer will serialize kEpsilon (which is static and always the same) and therefor not needed.
/// </summary>
[DataContract]
public class Vector3DataContract {
    [DataMember]
    public float x;
    [DataMember]
    public float y;
    [DataMember]
    public float z;

    public Vector3DataContract() {

    }

    public Vector3DataContract(float x, float y, float z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static Vector3DataContract ToDataContract(Vector3 v) {
        return new Vector3DataContract(v.x, v.y, v.z);
    }

    public static Vector3 ToVector3(Vector3DataContract v) {
        return new Vector3(v.x, v.y, v.z);
    }

}
