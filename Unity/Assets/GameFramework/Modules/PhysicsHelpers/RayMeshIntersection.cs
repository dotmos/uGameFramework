using System.Collections.Generic;
using UnityEngine;

namespace PhysicsHelper {
    public class RayMeshIntersection {
        /// <summary>
        /// Check if a ray intersects with a mesh and writes the closest worldspace hit position to hitPoint if the mesh was hit. NOTE: Produces garbage due to Unity's mesh handling.
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="mesh"></param>
        /// <param name="hitPoint"></param>
        /// <returns></returns>
        public static bool Intersect(Ray ray, Mesh mesh, Vector3 meshPosition, Vector3 meshScale, Quaternion meshRotation, ref Vector3 hitPoint) {
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            return Intersect(ray, vertices, triangles, meshPosition, meshScale, meshRotation, ref hitPoint);
        }

        /// <summary>
        /// Check if a ray intersects with a mesh and writes the closest worldspace hit position to hitPoint if the mesh was hit.
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="vertices"></param>
        /// <param name="triangles"></param>
        /// <param name="meshPosition"></param>
        /// <param name="meshScale"></param>
        /// <param name="meshRotation"></param>
        /// <param name="hitPoint"></param>
        /// <returns></returns>
        public static bool Intersect(Ray ray, Vector3[] vertices, int[] triangles, Vector3 meshPosition, Vector3 meshScale, Quaternion meshRotation, ref Vector3 hitPoint) {
            float closestHitDistance = float.MaxValue;
            float tempHitDistance = 0;
            Vector3 closestHitPoint = Vector3.zero;
            Vector3 tempHitPoint = Vector3.zero;
            Vector3 v0, v1, v2 = Vector3.zero;
            bool hitSomething = false;

            for (int i = 0; i < triangles.Length; i += 3) {
                //Get triangle vertices
                v0 = Vector3.Scale(vertices[triangles[i]], meshScale).RotateAroundPivot(Vector3.zero, meshRotation) + meshPosition;
                v1 = Vector3.Scale(vertices[triangles[i + 1]], meshScale).RotateAroundPivot(Vector3.zero, meshRotation) + meshPosition;
                v2 = Vector3.Scale(vertices[triangles[i + 2]], meshScale).RotateAroundPivot(Vector3.zero, meshRotation) + meshPosition;

                //Check if ray hit triangle
                if (RayTriangleIntersection.Intersect(ref v0, ref v1, ref v2, ref ray, ref tempHitPoint)) {
                    hitSomething = true;
                    tempHitDistance = Vector3.SqrMagnitude(ray.origin - tempHitPoint);
                    //Check if hit is nearer to the ray origin than other hits
                    if (tempHitDistance < closestHitDistance) {
                        closestHitPoint = tempHitPoint;
                        closestHitDistance = tempHitDistance;
                    }
                }
            }

            if (hitSomething) hitPoint = closestHitPoint;

            return hitSomething;
        }

        /// <summary>
        /// Check if a ray intersects with a mesh and writes the closest worldspace hit position to hitPoint if the mesh was hit.
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="vertices"></param>
        /// <param name="triangles"></param>
        /// <param name="meshPosition"></param>
        /// <param name="meshScale"></param>
        /// <param name="meshRotation"></param>
        /// <param name="hitPoint"></param>
        /// <returns></returns>
        public static bool Intersect(Ray ray, List<Vector3> vertices, List<int> triangles, Vector3 meshPosition, Vector3 meshScale, Quaternion meshRotation, ref Vector3 hitPoint) {
            float closestHitDistance = float.MaxValue;
            float tempHitDistance = 0;
            Vector3 closestHitPoint = Vector3.zero;
            Vector3 tempHitPoint = Vector3.zero;
            Vector3 v0, v1, v2 = Vector3.zero;
            bool hitSomething = false;

            for (int i = 0; i < triangles.Count; i += 3) {
                //Get triangle vertices
                v0 = Vector3.Scale(vertices[triangles[i]], meshScale).RotateAroundPivot(Vector3.zero, meshRotation) + meshPosition;
                v1 = Vector3.Scale(vertices[triangles[i + 1]], meshScale).RotateAroundPivot(Vector3.zero, meshRotation) + meshPosition;
                v2 = Vector3.Scale(vertices[triangles[i + 2]], meshScale).RotateAroundPivot(Vector3.zero, meshRotation) + meshPosition;

                //Check if ray hit triangle
                if (RayTriangleIntersection.Intersect(ref v0, ref v1, ref v2, ref ray, ref tempHitPoint)) {
                    hitSomething = true;
                    tempHitDistance = Vector3.SqrMagnitude(ray.origin - tempHitPoint);
                    //Check if hit is nearer to the ray origin than other hits
                    if (tempHitDistance < closestHitDistance) {
                        closestHitPoint = tempHitPoint;
                        closestHitDistance = tempHitDistance;
                    }
                }
            }

            if (hitSomething) hitPoint = closestHitPoint;

            return hitSomething;
        }

    }
}