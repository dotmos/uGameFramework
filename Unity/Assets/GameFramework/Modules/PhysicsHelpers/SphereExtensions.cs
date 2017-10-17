using UnityEngine;
using System.Collections;

namespace PhysicsHelper{
    public static class SphereColliderExtensions{
        /// <summary>
        /// Returns the closest point on a box
        /// </summary>
        /// <returns>The point on box.</returns>
        /// <param name="sphere">Sphere.</param>
        /// <param name="box">Box.</param>
        public static Vector3 ClosestPointOnBox(this SphereCollider sphere, BoxCollider box){
            return ClosestPoint.ClosestPointOnBox(box, sphere.transform.position);
        }

        /// <summary>
        /// Returns the closest point on a sphere
        /// </summary>
        /// <returns>The point on sphere.</returns>
        /// <param name="sphere">Sphere.</param>
        /// <param name="otherSphere">Other sphere.</param>
        public static Vector3 ClosestPointOnSphere(this SphereCollider sphere, SphereCollider otherSphere){
            return ClosestPoint.ClosestPointOnSphere(otherSphere, sphere.transform.position);
        }

        /// <summary>
        /// Returns the closest point on a capsule
        /// </summary>
        /// <returns>The point on capsule.</returns>
        /// <param name="sphere">Sphere.</param>
        /// <param name="capsule">Capsule.</param>
        public static Vector3 ClosestPointOnCapsule(this SphereCollider sphere, CapsuleCollider capsule){
            return ClosestPoint.ClosestPointOnCapsule(capsule, sphere.transform.position);
        }

        /// <summary>
        /// Returns the closest point on a terrain
        /// </summary>
        /// <returns>The point on terrain.</returns>
        /// <param name="sphere">Sphere.</param>
        /// <param name="terrain">Terrain.</param>
        public static Vector3 ClosestPointOnTerrain(this SphereCollider sphere, TerrainCollider terrain){
            return ClosestPoint.ClosestPointOnTerrain(terrain, sphere.transform.position);
        }

        /// <summary>
        /// Returns the closest point on a mesh
        /// </summary>
        /// <returns>The point on mesh.</returns>
        /// <param name="sphere">Sphere.</param>
        /// <param name="mesh">Mesh.</param>
        public static Vector3 ClosestPointOnMesh(this SphereCollider sphere, MeshCollider mesh){
            return ClosestPoint.ClosestPointOnMesh(mesh, sphere.transform.position);
        }

        /// <summary>
        /// Returns the closest point on a collider. If you know the type of the collider, use the other ClosestPointOn functions instead, as they are a TINY bit faster
        /// </summary>
        /// <returns>The point on surface.</returns>
        /// <param name="sphere">Sphere.</param>
        /// <param name="collider">Collider.</param>
        public static Vector3 ClosestPointOnSurface(this SphereCollider sphere, Collider collider){
            if(collider is BoxCollider){
                return sphere.ClosestPointOnBox((BoxCollider)collider);
            } 
            else if(collider is SphereCollider){
                return sphere.ClosestPointOnSphere((SphereCollider)collider);
            }
            else if(collider is CapsuleCollider){
                return sphere.ClosestPointOnCapsule((CapsuleCollider)collider);
            }
            else if(collider is TerrainCollider){
                return sphere.ClosestPointOnTerrain((TerrainCollider)collider);
            }
            else if(collider is MeshCollider){
                return sphere.ClosestPointOnMesh((MeshCollider)collider);
            }
            else{
                return collider.transform.position;
            }
        }
    }
}