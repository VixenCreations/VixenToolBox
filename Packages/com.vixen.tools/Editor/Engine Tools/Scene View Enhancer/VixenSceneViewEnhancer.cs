using System.Reflection;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UIElements;

namespace VixenTools.Editor
{
    [InitializeOnLoad]
    public static class VixenSceneViewEnhancer
    {
        public const string MenuPath = "VixenTools/Unity Engine/Scene View Enhancer";

        const string EnabledKey = "VixenTools_SceneEnhancerEnabled";
        const string FpsKey = "VixenTools_TargetFPS";
        const string SavedModeKey = "VixenTools_SceneEnhancer_SavedInteractionMode";
        const string SavedIdleKey = "VixenTools_SceneEnhancer_SavedIdleTime";
        const string InteractionModeKey = "InteractionMode";
        const string IdleTimeKey = "ApplicationIdleTime";
        const int CustomInteractionMode = 3;
        const int NotSet = -1;

        public const int MinFps = 30;
        public const int MaxFps = 240;

        static readonly MethodInfo s_updateInteractionMode = typeof(EditorApplication).GetMethod(
            "UpdateInteractionModeSettings", BindingFlags.Static | BindingFlags.NonPublic);

        static VixenSceneViewEnhancer()
        {
            EditorApplication.delayCall += () =>
            {
                if (IsEnabled) ApplyFrameCap();
            };
        }

        public static bool IsEnabled => EditorPrefs.GetBool(EnabledKey, false);

        public static int TargetFps
        {
            get { return Mathf.Clamp(EditorPrefs.GetInt(FpsKey, 60), MinFps, MaxFps); }
            set
            {
                EditorPrefs.SetInt(FpsKey, Mathf.Clamp(value, MinFps, MaxFps));
                if (IsEnabled) ApplyFrameCap();
            }
        }

        [MenuItem(MenuPath, false, 30)]
        static void Toggle()
        {
            SetEnabled(!IsEnabled);
        }

        [MenuItem(MenuPath, true)]
        static bool ToggleValidate()
        {
            Menu.SetChecked(MenuPath, IsEnabled);
            return true;
        }

        public static void SetEnabled(bool on)
        {
            if (on == IsEnabled) return;

            EditorPrefs.SetBool(EnabledKey, on);
            if (on) ApplyFrameCap();
            else RestoreFrameCap();

            Menu.SetChecked(MenuPath, on);
            foreach (SceneView view in SceneView.sceneViews)
            {
                if (view == null) continue;
                Overlay overlay;
                if (view.TryGetOverlay(VixenSceneStatsOverlay.OverlayId, out overlay)) overlay.displayed = on;
                view.Repaint();
            }
        }

        static int IdleTimeFor(int fps)
        {
            return Mathf.Clamp(Mathf.RoundToInt(1000f / fps), 1, 33);
        }

        static void ApplyFrameCap()
        {
            if (!EditorPrefs.HasKey(SavedModeKey))
            {
                EditorPrefs.SetInt(SavedModeKey, EditorPrefs.HasKey(InteractionModeKey) ? EditorPrefs.GetInt(InteractionModeKey) : NotSet);
                EditorPrefs.SetInt(SavedIdleKey, EditorPrefs.HasKey(IdleTimeKey) ? EditorPrefs.GetInt(IdleTimeKey) : NotSet);
            }

            EditorPrefs.SetInt(InteractionModeKey, CustomInteractionMode);
            EditorPrefs.SetInt(IdleTimeKey, IdleTimeFor(TargetFps));
            UpdateInteractionMode();
        }

        static void RestoreFrameCap()
        {
            if (!EditorPrefs.HasKey(SavedModeKey)) return;

            bool stillOurs = EditorPrefs.GetInt(InteractionModeKey, NotSet) == CustomInteractionMode
                             && EditorPrefs.GetInt(IdleTimeKey, NotSet) == IdleTimeFor(TargetFps);
            if (stillOurs)
            {
                RestoreKey(InteractionModeKey, EditorPrefs.GetInt(SavedModeKey, NotSet));
                RestoreKey(IdleTimeKey, EditorPrefs.GetInt(SavedIdleKey, NotSet));
                UpdateInteractionMode();
            }

            EditorPrefs.DeleteKey(SavedModeKey);
            EditorPrefs.DeleteKey(SavedIdleKey);
        }

        static void RestoreKey(string key, int value)
        {
            if (value == NotSet) EditorPrefs.DeleteKey(key);
            else EditorPrefs.SetInt(key, value);
        }

        static void UpdateInteractionMode()
        {
            if (s_updateInteractionMode != null) s_updateInteractionMode.Invoke(null, null);
        }

        public static string FormatBytes(long bytes)
        {
            string[] suffix = { "B", "KB", "MB", "GB" };
            int index = 0;
            double value = bytes;
            while (value >= 1024 && index < suffix.Length - 1)
            {
                index++;
                value /= 1024;
            }
            return value.ToString(index == 0 ? "0" : "0.0") + " " + suffix[index];
        }
    }

    [Overlay(typeof(SceneView), OverlayId, "Vixen Scene Stats")]
    public class VixenSceneStatsOverlay : Overlay
    {
        public const string OverlayId = "vixen-scene-stats";

        Label m_redraws;
        Label m_cap;
        Label m_managed;
        Label m_engine;
        Label m_graphics;
        SliderInt m_capSlider;
        int m_repaints;
        double m_countSince;

        public override void OnCreated()
        {
            if (VixenSceneViewEnhancer.IsEnabled) displayed = true;
            m_countSince = EditorApplication.timeSinceStartup;
            SceneView.duringSceneGui += CountRepaint;
        }

        public override void OnWillBeDestroyed()
        {
            SceneView.duringSceneGui -= CountRepaint;
        }

        void CountRepaint(SceneView view)
        {
            if (view == containerWindow && Event.current.type == EventType.Repaint) m_repaints++;
        }

        public override VisualElement CreatePanelContent()
        {
            VisualElement root = new VisualElement();
            root.style.minWidth = 240;
            root.style.paddingLeft = 4;
            root.style.paddingRight = 4;
            root.style.paddingBottom = 4;

            m_redraws = AddRow(root, "Scene redraws",
                "How many times this Scene view drew itself each second. Close to 0 while nothing moves is normal. " +
                "A high number while you are not touching anything means something keeps redrawing it. This panel adds up to one a second.");
            m_cap = AddRow(root, "Editor frame cap",
                "The frame rate the editor holds itself to while idle, set through Unity's Interaction Mode preference. " +
                "Turn on the Scene View Enhancer to use it. Turning it off puts your own setting back.");

            m_capSlider = new SliderInt("Cap", VixenSceneViewEnhancer.MinFps, VixenSceneViewEnhancer.MaxFps);
            m_capSlider.showInputField = true;
            m_capSlider.value = VixenSceneViewEnhancer.TargetFps;
            m_capSlider.tooltip = "The editor frame cap in frames per second. Unity allows 30 to 240.";
            m_capSlider.RegisterValueChangedCallback(e => VixenSceneViewEnhancer.TargetFps = e.newValue);
            root.Add(m_capSlider);

            m_managed = AddRow(root, "Script memory",
                "Memory used by C# scripts, out of what the script heap has reserved. It rises and falls as scripts run and the garbage collector clears up.");
            m_engine = AddRow(root, "Engine memory",
                "Memory Unity itself has in use, out of what it has reserved from Windows. A number that only ever climbs while you repeat the same task points to a leak.");
            m_graphics = AddRow(root, "Graphics driver",
                "Memory the graphics driver holds for textures, meshes and render targets.");

            VisualElement buttons = new VisualElement();
            buttons.style.flexDirection = FlexDirection.Row;
            buttons.style.marginTop = 4;
            Button diagnostics = new Button(VixenMemoryDiagnostics.Open) { text = "Memory Diagnostics" };
            diagnostics.tooltip = "Opens the leak check and the largest textures list.";
            Button free = new Button(VixenMemoryDiagnostics.FreeUnusedMemory) { text = "Free Unused" };
            free.tooltip = "Runs the garbage collector and unloads assets nothing uses any more. It can take a few seconds in a big project.";
            buttons.Add(diagnostics);
            buttons.Add(free);
            root.Add(buttons);

            Refresh();
            root.schedule.Execute(Refresh).Every(1000);
            return root;
        }

        static Label AddRow(VisualElement parent, string title, string tooltip)
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.tooltip = tooltip;

            Label name = new Label(title);
            name.style.flexGrow = 1;
            name.style.minWidth = 120;

            Label value = new Label();
            value.style.unityTextAlign = TextAnchor.MiddleRight;
            value.style.unityFontStyleAndWeight = FontStyle.Bold;

            row.Add(name);
            row.Add(value);
            parent.Add(row);
            return value;
        }

        void Refresh()
        {
            if (m_redraws == null || !InternalEditorUtility.isApplicationActive) return;

            double now = EditorApplication.timeSinceStartup;
            double elapsed = now - m_countSince;
            if (elapsed > 0.1)
            {
                m_redraws.text = (m_repaints / elapsed).ToString("0.0") + " /s";
                m_repaints = 0;
                m_countSince = now;
            }

            bool on = VixenSceneViewEnhancer.IsEnabled;
            m_cap.text = on ? VixenSceneViewEnhancer.TargetFps + " fps" : "Off";
            m_capSlider.SetEnabled(on);
            if (m_capSlider.value != VixenSceneViewEnhancer.TargetFps) m_capSlider.SetValueWithoutNotify(VixenSceneViewEnhancer.TargetFps);

            m_managed.text = VixenSceneViewEnhancer.FormatBytes(Profiler.GetMonoUsedSizeLong()) + " / "
                             + VixenSceneViewEnhancer.FormatBytes(Profiler.GetMonoHeapSizeLong());
            m_engine.text = VixenSceneViewEnhancer.FormatBytes(Profiler.GetTotalAllocatedMemoryLong()) + " / "
                            + VixenSceneViewEnhancer.FormatBytes(Profiler.GetTotalReservedMemoryLong());
            m_graphics.text = VixenSceneViewEnhancer.FormatBytes(Profiler.GetAllocatedMemoryForGraphicsDriver());
        }
    }
}
