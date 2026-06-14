#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class CurveOperations
{
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

        if (original != null && original.keys != null)
        {
            foreach (var k in original.keys)
            {
                if (k.time < sTime)
                    buffer.Add(new Keyframe(k.time, k.value, k.inTangent, k.outTangent));
            }
        }

        buffer.Add(new Keyframe(sTime, sVal));

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

        buffer.Add(new Keyframe(eTime, eVal));

        buffer.Sort((a, b) => a.time.CompareTo(b.time));

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
                dedup[dedup.Count - 1] = k;
            }
        }

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