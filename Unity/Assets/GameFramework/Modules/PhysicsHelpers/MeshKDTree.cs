using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PhysicsHelper{
    public class MeshKDTree : MonoBehaviour
    {
        KDTree kd;

        void Awake()
        {
            Mesh mesh = null;
            MeshCollider mc = GetComponent<MeshCollider>();
            if(mc != null) mesh = mc.sharedMesh;
            else {
                MeshFilter mf = GetComponent<MeshFilter>();
                if(mf != null) mesh = mf.mesh;
            }

            if(mesh != null) kd = KDTree.CreateFromMesh(mesh);
            else Destroy(this);
        }

        public Vector3 ClosestPointOnSurface(Vector3 position)
        {
            return kd.FindNearestPointOnMesh(position, this.transform);
//            position = transform.InverseTransformPoint(position);
//            return transform.TransformPoint(NearestPointOnMesh(position, verts, kd, tris, vt));
        }
    }
}