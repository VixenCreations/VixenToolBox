#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class CurveOperations
{
    /// <summary>
    /// Rebuilds a curve so that between [sTime, eTime] it transitions from sVal to eVal
    /// with the given easing and intermediate key count.
    ///
    /// Keys BEFORE sTime are preserved.
    /// Keys AFTER eTime are intentionally dropped, so the generated region defines the tail.
    /// </summary>
    public static AnimationCurve BuildStretchedCurve(
        AnimationCurve original,
        float sTime,
        float eTime,
        float sVal,
        float eVal,
        int intermediates,
        EasingFunctions.EaseType ease)
    {
        AnimationCurve result = new AnimationCurve();
        var buffer = new List<Keyframe>();

        // 1. Preserve keys strictly before the edit region
        if (original != null && original.keys != null)
        {
            foreach (var k in original.keys)
            {
                if (k.time < sTime)
                    buffer.Add(new Keyframe(k.time, k.value, k.inTangent, k.outTangent));
            }
        }

        // 2. Insert explicit start key
        buffer.Add(new Keyframe(sTime, sVal));

        // 3. Insert intermediate easing keys
        if (intermediates > 0)
        {
            for (int i = 1; i <= intermediates; ++i)
            {
                float tNorm = i / (float)(intermediates + 1);
                float time = Mathf.Lerp(sTime, eTime, tNorm);
                float value = EasingFunctions.EvaluateEasing(sVal, eVal, tNorm, ease);
                buffer.Add(new Keyframe(time, value));
            }
        }

        // 4. Insert explicit end key
        buffer.Add(new Keyframe(eTime, eVal));

        // 5. Sort keys by time
        buffer.Sort((a, b) => a.time.CompareTo(b.time));

        // 6. Deduplicate same-time keys (Unity hates identical times)
        var dedup = new List<Keyframe>();
        float lastTime = float.NaN;

        foreach (var k in buffer)
        {
            if (float.IsNaN(lastTime) || Mathf.Abs(k.time - lastTime) > 0.000001f)
            {
                dedup.Add(k);
                lastTime = k.time;
            }
            else
            {
                // Prefer the later key (usually the generated easing one)
                dedup[dedup.Count - 1] = k;
            }
        }

        // 7. Build final curve and smooth tangents
        result.keys = dedup.ToArray();

        for (int i = 0; i < result.keys.Length; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode(result, i, AnimationUtility.TangentMode.Auto);
            AnimationUtility.SetKeyRightTangentMode(result, i, AnimationUtility.TangentMode.Auto);
        }

        return result;
    }
}
#endif