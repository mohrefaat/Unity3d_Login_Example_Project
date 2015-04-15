using System;
using UnityEngine;

public static class Utils {

  

    public static void Log(string format, params object[] objects) {
        Debug.Log(string.Format(format, objects));
    }

    internal static void LogError(string format, params object[] objects) {
        Debug.LogError(string.Format(format, objects));
    }

    internal static void LogWarning(string format, params object[] objects) {
        Debug.LogWarning(string.Format(format, objects));
    }

    public static float Spring(float start, float end, float t) {
        t = Mathf.Clamp01(t);
        t = (Mathf.Sin(t * Mathf.PI * (.2f + 2.5f * t * t * t)) * Mathf.Pow(1f - t, 2.2f) + t) * (1f + (1.2f * (1f - t)));
        return start + (end - start) * t;
    }
}