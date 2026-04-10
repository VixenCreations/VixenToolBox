#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class PreviewEngine
{
    private GameObject target;
    private AnimationClip clip;
    private double playStartEditorTime;
    private float startTime;
    private float speed = 1f;

    public bool IsPreviewing { get; private set; }

    public void SetTarget(GameObject go) => target = go;

    public void SetSpeed(float s) => speed = Mathf.Max(0.01f, s);

    public void StartPreview(AnimationClip c, float from = 0f)
    {
        if (target == null || c == null) return;

        StopPreview(); // reset if needed
        clip = c;
        startTime = from;
        playStartEditorTime = EditorApplication.timeSinceStartup;

        AnimationMode.StartAnimationMode();
        EditorApplication.update += OnUpdate;
        IsPreviewing = true;
    }

    public void StopPreview()
    {
        if (!IsPreviewing) return;

        EditorApplication.update -= OnUpdate;
        AnimationMode.StopAnimationMode();
        IsPreviewing = false;
        SceneView.RepaintAll();
    }

    private void OnUpdate()
    {
        if (target == null || clip == null)
        {
            StopPreview();
            return;
        }

        double elapsed = (EditorApplication.timeSinceStartup - playStartEditorTime) * speed;
        float t = startTime + (float)elapsed;

        if (t > clip.length)
        {
            StopPreview();
            return;
        }

        AnimationMode.BeginSampling();
        AnimationMode.SampleAnimationClip(target, clip, t);
        AnimationMode.EndSampling();

        SceneView.RepaintAll();
    }
}
#endif
