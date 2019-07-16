using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FloatExt
{
    /// <summary>
    /// Maps a value from one range (e.g. 0-1) to another range (e.g. 3-42)
    /// </summary>
    /// <param name="value"></param>
    /// <param name="oldRangeMin"></param>
    /// <param name="oldRangeMax"></param>
    /// <param name="newRangeMin"></param>
    /// <param name="newRangeMax"></param>
    /// <returns></returns>
    public static float Map(this float value, float oldRangeMin, float oldRangeMax, float newRangeMin, float newRangeMax) {
        return newRangeMin + (newRangeMax - newRangeMin) * ((value - oldRangeMin) / (oldRangeMax - oldRangeMin));
    }
}
