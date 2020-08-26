using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ParallelProcessing {
    /// <summary>
    /// Threadsafe alternative for Unity's AnimationCurve. NOTE: Use with care as it bakes the given animation curve into a LUT and then uses the LUT for fetching the data, thus eating up memory.
    /// </summary>
    public class ParallelAnimationCurve {
        float[] lut;
        readonly int res;

        public ParallelAnimationCurve(AnimationCurve ac, int resolution = 256) {
            res = resolution;
            lut = new float[res+1];
            for(int i=0; i<=res; ++i) {
                lut[i] = ac.Evaluate(i / (float)res);
            }
        }

        public float Evaluate(float t) {
            return lut[Mathf.Clamp(Mathf.RoundToInt(t * res), 0, res)];
        }
    }

    /*
    public class ParallelLinearCurve_BROKEN {
        Keyframe[] keyframes;
        float[] distanceToHighFrame;
        int keyFrameCount;

        public ParallelLinearCurve_BROKEN(AnimationCurve ac) {
            List<Keyframe> sortedKeyframes = new List<Keyframe>(ac.keys);
            sortedKeyframes.Sort((a, b) => a.time.CompareTo(b.time));
            //Copy keyframes from animation curve
            keyframes = new Keyframe[sortedKeyframes.Count];
            distanceToHighFrame = new float[sortedKeyframes.Count - 1];
            for (int i = 0; i < sortedKeyframes.Count; ++i) {
                keyframes[i] = sortedKeyframes[i];
                if (i < sortedKeyframes.Count - 1) {
                    distanceToHighFrame[i] = ac.keys[i + 1].time - sortedKeyframes[i].time;
                }
            }
            keyFrameCount = keyframes.Length;
        }

        public float Evaluate(float x) {
            if (x <= keyframes[0].time) {
                return keyframes[0].value;
            }
            else if (x >= keyframes[keyFrameCount - 1].time) {
                return keyframes[keyFrameCount - 1].value;
            }

            for (int i = 0; i < keyFrameCount - 1; ++i) {
                Keyframe lowFrame = keyframes[i];
                if (x >= lowFrame.time) {
                    //Found the correct lower frame. Now lerp between lower and higher frame
                    Keyframe highFrame = keyframes[i + 1];
                    float normalizedPosBetweenFrames = (x - lowFrame.time) / distanceToHighFrame[i];
                    //return normalizedPosBetweenFrames.Map(0, 1, lowFrame.value, highFrame.value);
                    //return CubicHermiteInterpolation(x, lowFrame.value, highFrame.value, lowFrame.outTangent, highFrame.inTangent);
                    //return CubicHermiteInterpolation(Mathf.InverseLerp(lowFrame.time, highFrame.time, x), lowFrame.value, highFrame.value, lowFrame.outTangent, highFrame.inTangent);
                    return CubicHermiteInterpolation(normalizedPosBetweenFrames, lowFrame.value, highFrame.value, lowFrame.outTangent, highFrame.inTangent);

                    
                    return EvaluateInternal(
                        x,
                        lowFrame.time,
                        lowFrame.value,
                        lowFrame.outTangent,
                        highFrame.time,
                        highFrame.value,
                        highFrame.inTangent);
                        
                }
            }

            return 0;
        }

        float CubicHermiteInterpolation(float t, float point0, float point1, float tan0, float tan1) {
           return ((2.0f * t * t * t) - (3.0f * t * t) + 1.0f) * point0
                    + ((t * t * t) - (2.0f * t * t) + t) * tan0
                    + ((-2.0f * t * t * t) + (3.0f * t * t)) * point1
                    + ((t * t * t) - (t * t)) * tan1;
        }

        float unlerp(float a, float b, float x) { return (x - a) / (b - a); }
        private float EvaluateInternal(float iInterp,
        float iLeft, float vLeft, float tLeft,
        float iRight, float vRight, float tRight) {
            float t = unlerp(iLeft, iRight, iInterp); //math.unlerp(iLeft, iRight, iInterp);

            //TODO: This could be precalculated from when we are creating this struct for each interval? Micro optimization?
            float scale = iRight - iLeft;

            //TODO: Maybe we could create an approximation of each hermite basis that blends into an answer?
            float h00(float x) => (2 * x * x * x) - (3 * x * x) + 1;
            float h10(float x) => (x * x * x) - (2 * x * x) + x;
            float h01(float x) => -(2 * x * x * x) + (3 * x * x);
            float h11(float x) => (x * x * x) - (x * x);

            //TODO: Scaled tangents could also be precalculated. Micro optimization?
            return (h00(t) * vLeft) + (h10(t) * scale * tLeft) + (h01(t) * vRight) + (h11(t) * scale * tRight);
        }
    }
    */
}