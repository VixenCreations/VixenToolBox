#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class TimelineRibbon : VisualElement
{
    public Action<float, float> OnRangeChanged;
    private AnimationClip clip;
    private float start = 0f;
    private float end = 1f;

    public TimelineRibbon()
    {
        var container = new IMGUIContainer(OnGUI);
        Add(container);
        style.height = 40;
        tooltip = "Time range control. Adjusts the visible window and the range used for key generation.";
    }

    public void SetClip(AnimationClip c)
    {
        clip = c;
        if (clip != null)
        {
            start = 0f;
            end = clip.length;
        }
    }

    public void SetRange(float s, float e)
    {
        start = s;
        end = e;
    }

    private void OnGUI()
    {
        Rect r = EditorGUILayout.GetControlRect(false, 28);
        EditorGUI.DrawRect(r, new Color(0.09f, 0.09f, 0.1f));

        if (clip != null)
        {
            // Fix: Allows expanding bounds past the current clip length and prevents 0-length slider collapse.
            float maxLen = Mathf.Max(1f, clip.length, start, end);

            EditorGUI.BeginChangeCheck();
            float tStart = EditorGUILayout.Slider(
                new GUIContent("Start", "Start of the visible and generation range."),
                start, 0f, maxLen);
            float tEnd = EditorGUILayout.Slider(
                new GUIContent("End", "End of the visible and generation range."),
                end, 0f, maxLen);

            if (EditorGUI.EndChangeCheck())
            {
                start = Mathf.Min(tStart, tEnd);
                end = Mathf.Max(tStart, tEnd);
                OnRangeChanged?.Invoke(start, end);
            }

            EditorGUILayout.LabelField(
                $"Clip length: {clip.length:0.000}s  Range: {start:0.000} - {end:0.000}");
        }
        else
        {
            EditorGUILayout.LabelField("No clip loaded");
        }
    }
}
#endif