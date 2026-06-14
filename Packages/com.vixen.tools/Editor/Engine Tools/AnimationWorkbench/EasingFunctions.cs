#if UNITY_EDITOR
using UnityEngine;

public static class EasingFunctions
{
    public enum EaseType
    {
        Linear,
        SmoothStep,
        EaseInOutCubic,
        EaseOutQuad,
        EaseInQuad,
        EaseOutCubic
    }

    public static float EvaluateEasing(float a, float b, float t, EaseType ease)
    {
        float u;
        switch (ease)
        {
            case EaseType.Linear: u = t; break;
            case EaseType.SmoothStep: u = Mathf.SmoothStep(0f, 1f, t); break;
            case EaseType.EaseInOutCubic: u = EaseInOutCubic(t); break;
            case EaseType.EaseOutQuad: u = 1 - (1 - t) * (1 - t); break;
            case EaseType.EaseInQuad: u = t * t; break;
            case EaseType.EaseOutCubic: u = 1 - Mathf.Pow(1 - t, 3); break;
            default: u = t; break;
        }

        return Mathf.Lerp(a, b, u);
    }

    private static float EaseInOutCubic(float x) =>
        x < 0.5f ? 4 * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 3) / 2;
}
#endif
