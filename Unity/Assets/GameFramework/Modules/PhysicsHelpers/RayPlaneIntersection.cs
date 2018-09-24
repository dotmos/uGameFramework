using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsHelper {
    public class RayPlaneIntersection {
        public static Vector3 CalculateIntersection(Vector3 planeOrigin, Vector3 planeNormal, Vector3 rayOrigin, Vector3 rayDirection) {
            //Ray-> groundplane intersection
            float distance = Vector3.Dot(planeNormal, planeOrigin - rayOrigin) / Vector3.Dot(planeNormal, rayDirection);
            Vector3 intersection = rayOrigin + (rayDirection * distance);
            return intersection;
        }

        public static Vector3 CalculateIntersection(Vector3 planeOrigin, Vector3 planeNormal, Ray ray) {
            return CalculateIntersection(planeOrigin, planeNormal, ray.origin, ray.direction);
        }
    }
}