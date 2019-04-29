using UnityEngine;

namespace PhysicsHelper {

    public class RayTriangleIntersection {

        /// <summary>
        /// Checks if the specified ray hits the triangle descibed by p1, p2 and p3.
        /// Möller–Trumbore ray-triangle intersection algorithm implementation.
        /// Thanks: https://forum.unity.com/threads/hit-point-for-moller-trumbore-triangle-intersection.457106/
        /// </summary>
        /// <param name="p1">Vertex 1 of the triangle.</param>
        /// <param name="p2">Vertex 2 of the triangle.</param>
        /// <param name="p3">Vertex 3 of the triangle.</param>
        /// <param name="ray">The ray to test hit for.</param>
        /// <param name="hitPoint">The resulting hit point, if the triangle was hit.</param>
        /// <returns><c>true</c> when the ray hits the triangle, otherwise <c>false</c></returns>
        public static bool Intersect(ref Vector3 p1, ref Vector3 p2, ref Vector3 p3, ref Ray ray, ref Vector3 hitPoint) {
            // Vectors from p1 to p2/p3 (edges)
            Vector3 e1, e2;

            Vector3 p, q, t;
            float det, invDet, u, v;


            //Find vectors for two edges sharing vertex/point p1
            e1 = p2 - p1;
            e2 = p3 - p1;

            // calculating determinant 
            p = Vector3.Cross(ray.direction, e2);

            //Calculate determinat
            det = Vector3.Dot(e1, p);

            //if determinant is near zero, ray lies in plane of triangle otherwise not
            if (det > -Mathf.Epsilon && det < Mathf.Epsilon) { return false; }
            invDet = 1.0f / det;

            //calculate distance from p1 to ray origin
            t = ray.origin - p1;

            //Calculate u parameter
            u = Vector3.Dot(t, p) * invDet;

            //Check for ray hit
            if (u < 0 || u > 1) { return false; }

            //Prepare to test v parameter
            q = Vector3.Cross(t, e1);

            //Calculate v parameter
            v = Vector3.Dot(ray.direction, q) * invDet;

            //Check for ray hit
            if (v < 0 || u + v > 1) { return false; }

            //Calculate hitpoint
            hitPoint = p1 + u * e1 + v * e2;

            if ((Vector3.Dot(e2, q) * invDet) > Mathf.Epsilon) {
                //ray does intersect
                return true;
            }

            // No hit at all
            return false;
        }

        /// <summary>
        /// Checks if the specified ray hits the triangle descibed by p1, p2 and p3.
        /// Möller–Trumbore ray-triangle intersection algorithm implementation.
        /// Thanks: https://forum.unity.com/threads/hit-point-for-moller-trumbore-triangle-intersection.457106/
        /// </summary>
        /// <param name="p1">Vertex 1 of the triangle.</param>
        /// <param name="p2">Vertex 2 of the triangle.</param>
        /// <param name="p3">Vertex 3 of the triangle.</param>
        /// <param name="ray">The ray to test hit for.</param>
        /// <returns><c>true</c> when the ray hits the triangle, otherwise <c>false</c></returns>
        public static bool Intersect(Vector3 p1, Vector3 p2, Vector3 p3, Ray ray) {
            // Vectors from p1 to p2/p3 (edges)
            Vector3 e1, e2;

            Vector3 p, q, t;
            float det, invDet, u, v;


            //Find vectors for two edges sharing vertex/point p1
            e1 = p2 - p1;
            e2 = p3 - p1;

            // calculating determinant 
            p = Vector3.Cross(ray.direction, e2);

            //Calculate determinat
            det = Vector3.Dot(e1, p);

            //if determinant is near zero, ray lies in plane of triangle otherwise not
            if (det > -Mathf.Epsilon && det < Mathf.Epsilon) { return false; }
            invDet = 1.0f / det;

            //calculate distance from p1 to ray origin
            t = ray.origin - p1;

            //Calculate u parameter
            u = Vector3.Dot(t, p) * invDet;

            //Check for ray hit
            if (u < 0 || u > 1) { return false; }

            //Prepare to test v parameter
            q = Vector3.Cross(t, e1);

            //Calculate v parameter
            v = Vector3.Dot(ray.direction, q) * invDet;

            //Check for ray hit
            if (v < 0 || u + v > 1) { return false; }

            if ((Vector3.Dot(e2, q) * invDet) > Mathf.Epsilon) {
                //ray does intersect
                return true;
            }

            // No hit at all
            return false;
        }
    }
}