using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine {
    public static partial class Vector3Extensions {
        /// <summary>
        /// Rotate the point around a pivot and return the result
        /// </summary>
        /// <param name="point"></param>
        /// <param name="pivot"></param>
        /// <param name="eulerAngles"></param>
        /// <returns></returns>
        public static Vector3 RotateAroundPivot(this Vector3 point, Vector3 pivot, Vector3 eulerAngles) {
            return RotatePointAroundPivot(point, pivot, eulerAngles);
        }

        /// <summary>
        /// Rotate the point around a pivot and return the result
        /// </summary>
        /// <param name="point"></param>
        /// <param name="pivot"></param>
        /// <param name="eulerAngles"></param>
        /// <returns></returns>
        public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 eulerAngles) {
            Vector3 dir = point - pivot;
            dir = Quaternion.Euler(eulerAngles) * dir;
            return dir + pivot;
        }

        /// <summary>
        /// Rotate the point around a pivot and return the result
        /// </summary>
        /// <param name="point"></param>
        /// <param name="pivot"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public static Vector3 RotateAroundPivot(this Vector3 point, Vector3 pivot, Quaternion rotation) {
            return RotatePointAroundPivot(point, pivot, rotation);
        }

        /// <summary>
        /// Rotate the point around a pivot and return the result
        /// </summary>
        /// <param name="point"></param>
        /// <param name="pivot"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation) {
            Vector3 dir = point - pivot;
            dir = rotation * dir;
            return dir + pivot;
        }
    }
}
