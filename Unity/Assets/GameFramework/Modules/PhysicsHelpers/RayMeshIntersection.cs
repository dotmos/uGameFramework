using UnityEngine;

namespace PhysicsHelper {
    public class RayMeshIntersection {
        /// <summary>
        /// Check if a ray intersects with a mesh and writes the closest worldspace hit position to hitPoint if the mesh was hit
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="mesh"></param>
        /// <param name="hitPoint"></param>
        /// <returns></returns>
        public static bool Intersect(Ray ray, Mesh mesh, Vector3 meshPosition, Vector3 meshScale, Quaternion meshRotation, ref Vector3 hitPoint) {
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            float closestHitDistance = float.MaxValue;
            float tempHitDistance = 0;
            Vector3 closestHitPoint = Vector3.zero;
            Vector3 tempHitPoint = Vector3.zero;
            Vector3 v0, v1, v2 = Vector3.zero;
            bool hitSomething = false;

            for(int i=0; i<triangles.Length; i += 3) {
                //Get triangle vertices
                v0 = Vector3.Scale(vertices[triangles[i]], meshScale).RotateAroundPivot(Vector3.zero, meshRotation) + meshPosition;
                v1 = Vector3.Scale(vertices[triangles[i+1]], meshScale).RotateAroundPivot(Vector3.zero, meshRotation) + meshPosition;
                v2 = Vector3.Scale(vertices[triangles[i+2]], meshScale).RotateAroundPivot(Vector3.zero, meshRotation) + meshPosition;

                //Check if ray hit triangle
                if (RayTriangleIntersection.Intersect(v0, v1, v2, ray, ref tempHitPoint)) {
                    hitSomething = true;
                    tempHitDistance = Vector3.SqrMagnitude(ray.origin - tempHitPoint);
                    //Check if hit is nearer to the ray origin than other hits
                    if (tempHitDistance < closestHitDistance) {
                        closestHitPoint = tempHitPoint;
                        closestHitDistance = tempHitDistance;
                    }
                }
            }

            return hitSomething;
        }
    }
}