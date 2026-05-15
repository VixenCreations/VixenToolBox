// ===========================================================================
// VIXEN WEAR — COMBINED EDITOR + DRAWERS + UITK INSPECTOR
// ===========================================================================

using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

// ===========================================================================
// EDITOR DRAWERS (MaterialPropertyDrawers for ShaderLab)
// ===========================================================================

// Natively hooks into ShaderLab's [VectorLabel(X, Y, Z, W)] attribute
public class VectorLabelDrawer : MaterialPropertyDrawer
{
    private readonly string[] labels = new string[4];
    private readonly bool[] show = new bool[4];
    private readonly int visibleCount = 0;

    // ShaderLab passes the strings directly to the constructor
    public VectorLabelDrawer(string x, string y = "", string z = "", string w = "")
    {
        labels[0] = Sanitize(x);
        labels[1] = Sanitize(y);
        labels[2] = Sanitize(z);
        labels[3] = Sanitize(w);

        for (int i = 0; i < 4; i++)
        {
            // Ignore placeholders
            if (!string.IsNullOrEmpty(labels[i]) && labels[i] != "Unused" && labels[i] != "NONE")
            {
                show[i] = true;
                visibleCount++;
            }
        }
    }

    private string Sanitize(string s) => string.IsNullOrWhiteSpace(s) ? "" : s.Trim().Replace("_", " ");

    public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
    {
        if (prop.type != MaterialProperty.PropType.Vector || visibleCount == 0) 
            return EditorGUIUtility.singleLineHeight;
            
        // Enforces height: Main Label (1) + Sub Labels (1) + Input Fields (1) + Padding
        return (EditorGUIUtility.singleLineHeight * 3f) + 6f;
    }

    public override void OnGUI(Rect pos, MaterialProperty prop, GUIContent label, MaterialEditor editor)
    {
        if (prop.type != MaterialProperty.PropType.Vector)
        {
            EditorGUI.LabelField(pos, label.text, "VectorLabel requires a Vector property.");
            return;
        }

        if (visibleCount == 0) return;

        // 1. Draw the primary property label
        Rect mainRect = new Rect(pos.x, pos.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(mainRect, label);

        // 2. Setup the structural grid for the inputs
        float spacing = 4f;
        
        // Start the input grid exactly where standard Unity fields start for perfect alignment
        float startX = pos.x + EditorGUIUtility.labelWidth;
        float availableWidth = pos.width - EditorGUIUtility.labelWidth;
        float slotWidth = (availableWidth - (spacing * (visibleCount - 1))) / visibleCount;

        // Strict styling to prevent text bleed
        GUIStyle miniStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
        {
            normal = { textColor = new Color(0.7f, 0.7f, 0.8f) },
            fontSize = 11,
            clipping = TextClipping.Clip,
            alignment = TextAnchor.LowerCenter
        };

        int drawnCount = 0;
        float currentY = pos.y + EditorGUIUtility.singleLineHeight + 2f;
        Vector4 v = prop.vectorValue;

        for (int i = 0; i < 4; i++)
        {
            if (!show[i]) continue;

            float currentX = startX + (slotWidth + spacing) * drawnCount;

            // Stack the label on top of the float field
            Rect labelRect = new Rect(currentX, currentY, slotWidth, EditorGUIUtility.singleLineHeight);
            Rect fieldRect = new Rect(currentX, currentY + EditorGUIUtility.singleLineHeight + 2f, slotWidth, EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(labelRect, labels[i], miniStyle);

            float oldVal = (i == 0) ? v.x : (i == 1) ? v.y : (i == 2) ? v.z : v.w;
            
            // Clean trick: Use GUIContent.none to force the float field to use 100% of the rect space without hacking labelWidth
            float newVal = EditorGUI.FloatField(fieldRect, GUIContent.none, oldVal);

            if (i == 0) v.x = newVal;
            else if (i == 1) v.y = newVal;
            else if (i == 2) v.z = newVal;
            else v.w = newVal;

            drawnCount++;
        }

        prop.vectorValue = v;
    }
}

public class VixenTooltipDrawer : MaterialPropertyDrawer
{
    private readonly string tip;
    public VixenTooltipDrawer(string t) => tip = t.Replace("_", " ").Replace("DOT", ".");
    public override void OnGUI(Rect r, MaterialProperty p, GUIContent l, MaterialEditor e)
    {
        GUI.Label(r, new GUIContent("", tip));
        e.DefaultShaderProperty(r, p, l.text);
    }
}

public class VixenToggleDrawer : MaterialPropertyDrawer
{
    private readonly string keyword;
    private readonly string tip;

    public VixenToggleDrawer(string kw, string t)
    {
        keyword = kw;
        tip = t.Replace("_", " ").Replace("DOT", ".");
    }

    public override void OnGUI(Rect r, MaterialProperty p, GUIContent l, MaterialEditor e)
    {
        GUIContent final = new GUIContent(l.text, tip);
        bool val = EditorGUI.Toggle(r, final, p.floatValue > 0.5f);

        if (val != (p.floatValue > 0.5f))
        {
            p.floatValue = val ? 1 : 0;

            if (!string.IsNullOrEmpty(keyword) && keyword != "NONE")
            {
                foreach (Material m in p.targets)
                {
                    if (val) m.EnableKeyword(keyword);
                    else m.DisableKeyword(keyword);
                }
            }
        }
    }
}

public class VixenNormalDrawer : MaterialPropertyDrawer
{
    private readonly string tip;
    public VixenNormalDrawer(string t) => tip = t.Replace("_", " ").Replace("DOT", ".");
    public override void OnGUI(Rect r, MaterialProperty p, GUIContent l, MaterialEditor e)
    {
        GUI.Label(r, new GUIContent("", tip));
        e.DefaultShaderProperty(r, p, l.text);
    }
}

public class VixenHDRDrawer : MaterialPropertyDrawer
{
    private readonly string tip;
    public VixenHDRDrawer(string t) => tip = t.Replace("_", " ").Replace("DOT", ".");
    public override void OnGUI(Rect r, MaterialProperty p, GUIContent l, MaterialEditor e)
    {
        GUIContent final = new GUIContent(l.text, tip);
        Color c = EditorGUI.ColorField(r, final, p.colorValue, true, true, true);
        p.colorValue = c;
    }
}

public class VixenALBandDrawer : MaterialPropertyDrawer
{
    private readonly string tip;
    public VixenALBandDrawer(string t) => tip = t.Replace("_", " ").Replace("DOT", ".");
    public override void OnGUI(Rect r, MaterialProperty p, GUIContent l, MaterialEditor e)
    {
        GUIContent final = new GUIContent(l.text, tip);
        GUIContent[] names =
        {
            new GUIContent("Bass (0)"),
            new GUIContent("Low Mid (1)"),
            new GUIContent("High Mid (2)"),
            new GUIContent("Treble (3)")
        };
        int[] vals = { 0, 1, 2, 3 };
        p.floatValue = EditorGUI.IntPopup(r, final, (int)p.floatValue, names, vals);
    }
}

// ===========================================================================
// MAIN SHADER GUI — VixenWearEditor
// ===========================================================================
public class VixenWearEditor : ShaderGUI
{
    // Use EditorPrefs to survive Unity Domain Reloads (shader/script compiles)
    public static int ActiveTab
    {
        get => EditorPrefs.GetInt("VixenWear_ActiveTab", 0);
        set => EditorPrefs.SetInt("VixenWear_ActiveTab", value);
    }

    private static bool showBase
    {
        get => EditorPrefs.GetBool("VixenWear_ShowBase", true);
        set => EditorPrefs.SetBool("VixenWear_ShowBase", value);
    }

    private static bool showSurface
    {
        get => EditorPrefs.GetBool("VixenWear_ShowSurface", true);
        set => EditorPrefs.SetBool("VixenWear_ShowSurface", value);
    }

    private static bool showPolish
    {
        get => EditorPrefs.GetBool("VixenWear_ShowPolish", true);
        set => EditorPrefs.SetBool("VixenWear_ShowPolish", value);
    }

    private static bool showIntegration
    {
        get => EditorPrefs.GetBool("VixenWear_ShowIntegration", true);
        set => EditorPrefs.SetBool("VixenWear_ShowIntegration", value);
    }

    private static bool showAudio
    {
        get => EditorPrefs.GetBool("VixenWear_ShowAudio", true);
        set => EditorPrefs.SetBool("VixenWear_ShowAudio", value);
    }

    private readonly Color cyan = new Color(0f, 0.898f, 1f);
    private readonly Color pink = new Color(1f, 0f, 0.667f);
    private readonly Color orange = new Color(1f, 0.6f, 0f);
    private readonly Color green = new Color(0.2f, 0.8f, 0.2f);
    private readonly Color red = new Color(1f, 0.2f, 0.2f);

    private readonly Color bgPanel = new Color(0.118f, 0.118f, 0.118f);
    private readonly Color bgBanner = new Color(0.047f, 0.024f, 0.071f);

    private bool Header(string title, bool state, Color accent)
    {
        GUILayout.Space(6);
        Rect r = GUILayoutUtility.GetRect(16f, 26f, EditorStyles.boldLabel);

        EditorGUI.DrawRect(r, bgPanel);
        EditorGUI.DrawRect(new Rect(r.x, r.y, 4f, r.height), accent);

        if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition))
        {
            state = !state;
            Event.current.Use();
        }

        GUIStyle s = new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = 12,
            normal = { textColor = Color.white }
        };

        GUI.Label(new Rect(r.x + 10f, r.y, r.width - 16f, r.height), (state ? "▼" : "▶") + "   " + title, s);
        return state;
    }

    private GUIStyle Card => new GUIStyle("HelpBox")
    {
        padding = new RectOffset(10, 10, 10, 10),
        margin = new RectOffset(8, 8, 2, 8)
    };

    public override void OnGUI(MaterialEditor ed, MaterialProperty[] p)
    {
        GUILayout.Space(4);

        Rect banner = GUILayoutUtility.GetRect(100, 40);
        EditorGUI.DrawRect(banner, bgBanner);
        EditorGUI.DrawRect(new Rect(banner.x, banner.y + banner.height - 2, banner.width, 2), pink);

        GUIStyle title = new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 14,
            normal = { textColor = Color.white }
        };
        GUI.Label(banner, "LATEX ULTRA CONFIGURATION", title);

        GUILayout.Space(6);

        var _Color = FindProperty("_Color", p);
        var _CutOff = FindProperty("_CutOff", p);
        var _MainTex = FindProperty("_MainTex", p);
        var _MinBrightness = FindProperty("_MinBrightness", p);

        var _UV = FindProperty("_UVParams", p);
        var _PBR = FindProperty("_PBRParams", p);
        var _Geo = FindProperty("_GeoEmisParams", p);
        var _CC = FindProperty("_ClearcoatParams", p);
        var _Film = FindProperty("_FilmRimParams", p);
        var _SSS = FindProperty("_SSSParams", p);

        var _Metal = FindProperty("_MetallicGlossMap", p);
        var _Bump = FindProperty("_BumpMap", p);

        var _UseDet = FindProperty("_UseDetailNormal", p);
        var _DetMap = FindProperty("_DetailNormalMap", p);
        var _Det = FindProperty("_DetailParams", p);

        var _Emis = FindProperty("_EmissionColor", p);
        var _EmisMap = FindProperty("_EmissionMap", p);

        var _MatCap = FindProperty("_MatCap", p);
        var _MatMask = FindProperty("_MatCapMask", p);
        var _Int = FindProperty("_IntegrationParams", p);

        var _LV = FindProperty("_UseLightVolumes", p);
        var _LTCGI = FindProperty("_UseLTCGI", p);

        var _AL = FindProperty("_UseAudioLink", p);
        var _ALBand = FindProperty("_AL_EmissionBand", p);
        var _ALA = FindProperty("_ALParamsA", p);
        var _ALB = FindProperty("_ALParamsB", p);

        // Only draw the section matching the active tab
        if (ActiveTab == 0)
        {
            showBase = Header("Base Settings", showBase, cyan);
            if (showBase)
            {
                EditorGUILayout.BeginVertical(Card);
                ed.ShaderProperty(_Color, "Base Color");
                ed.ShaderProperty(_MainTex, "Albedo (RGB) Cutout (A)");
                ed.ShaderProperty(_CutOff, "Alpha Cutoff");
                ed.ShaderProperty(_MinBrightness, "Minimum Brightness");
                GUILayout.Space(6);
                ed.ShaderProperty(_UV, "UV Animation");
                EditorGUILayout.EndVertical();
            }
        }
        else if (ActiveTab == 1)
        {
            showSurface = Header("Surface & PBR", showSurface, pink);
            if (showSurface)
            {
                EditorGUILayout.BeginVertical(Card);
                ed.ShaderProperty(_Metal, "Packed PBR Map");
                ed.ShaderProperty(_Bump, "Normal Map");
                GUILayout.Space(6);
                ed.ShaderProperty(_PBR, "PBR Adjustments");
                ed.ShaderProperty(_Geo, "Geometry & Parallax");
                GUILayout.Space(6);
                ed.ShaderProperty(_UseDet, "Enable Micro Detail");
                if (_UseDet.floatValue > 0.5f)
                {
                    ed.ShaderProperty(_DetMap, "Micro Detail Map");
                    ed.ShaderProperty(_Det, "Detail Params");
                }
                EditorGUILayout.EndVertical();
            }
        }
        else if (ActiveTab == 2)
        {
            showPolish = Header("Polish & Translucency", showPolish, orange);
            if (showPolish)
            {
                EditorGUILayout.BeginVertical(Card);
                ed.ShaderProperty(_CC, "Clearcoat Settings");
                ed.ShaderProperty(_Film, "Film & Rim Settings");
                ed.ShaderProperty(_SSS, "Subsurface Settings");
                EditorGUILayout.EndVertical();
            }
        }
        else if (ActiveTab == 3)
        {
            showIntegration = Header("Integration & Emission", showIntegration, green);
            if (showIntegration)
            {
                EditorGUILayout.BeginVertical(Card);
                ed.ShaderProperty(_Emis, "Emission Color");
                ed.ShaderProperty(_EmisMap, "Emission Map");
                GUILayout.Space(6);
                ed.ShaderProperty(_MatCap, "MatCap Texture");
                ed.ShaderProperty(_MatMask, "MatCap Mask");
                ed.ShaderProperty(_Int, "System Intensities");
                GUILayout.Space(6);
                ed.ShaderProperty(_LV, "Enable Light Volumes");
                ed.ShaderProperty(_LTCGI, "Enable LTCGI");
                EditorGUILayout.EndVertical();
            }
        }
        else if (ActiveTab == 4)
        {
            showAudio = Header("AudioLink Reactivity", showAudio, red);
            if (showAudio)
            {
                EditorGUILayout.BeginVertical(Card);
                ed.ShaderProperty(_AL, "Enable AudioLink");
                if (_AL.floatValue > 0.5f)
                {
                    GUILayout.Space(4);
                    ed.ShaderProperty(_ALBand, "Reaction Band");
                    GUILayout.Space(4);
                    ed.ShaderProperty(_ALA, "Modulation Core");
                    ed.ShaderProperty(_ALB, "Modulation FX");
                }
                EditorGUILayout.EndVertical();
            }
        }

        GUILayout.Space(10);
        ed.RenderQueueField();
        ed.EnableInstancingField();
        ed.DoubleSidedGIField();
    }
}

// ===========================================================================
// UITK INSPECTOR WRAPPER
// ===========================================================================
[CustomEditor(typeof(Material), true)]
public class VixenWearUITKInspector : MaterialEditor
{
    private const string HubUssPath = "Packages/com.vixencreations.vixens-toolbox/Editor/UiStyles/VixenWearEditor.uss";

    public override VisualElement CreateInspectorGUI()
    {
        var mat = target as Material;
        if (mat == null || mat.shader == null || mat.shader.name != "VixenWear/Latex Ultra")
            return base.CreateInspectorGUI();

        var root = new VisualElement { name = "hub-root" };

        var style = AssetDatabase.LoadAssetAtPath<StyleSheet>(HubUssPath);
        if (style != null) root.styleSheets.Add(style);

        // Header
        var header = new VisualElement { name = "hub-header" };
        header.Add(new Label("LATEX ULTRA CONFIGURATION") { name = "hub-header-title" });
        root.Add(header);

        // Tabs
        var tabContainer = new VisualElement { name = "tab-container" };
        root.Add(tabContainer);

        string[] tabNames =
        {
            "Base",
            "Surface & PBR",
            "Polish & Translucency",
            "Integration & Emission",
            "AudioLink"
        };

        var descContainer = new VisualElement { name = "desc-container" };
        var descLabel = new Label("Configure Latex Ultra’s material response, polish, integration, and AudioLink reactivity.")
        {
            name = "tab-desc-label"
        };
        descContainer.Add(descLabel);
        root.Add(descContainer);

        var scroll = new ScrollView { name = "main-scroll" };
        root.Add(scroll);

        var imgui = new IMGUIContainer(() =>
        {
            if (targets == null || targets.Length == 0)
                return;

            var props = MaterialEditor.GetMaterialProperties(targets);
            var gui = new VixenWearEditor();
            gui.OnGUI(this, props);
        });
        scroll.Add(imgui);

        int activeIndex = Mathf.Clamp(VixenWearEditor.ActiveTab, 0, tabNames.Length - 1);

        void UpdateTabs(int newIndex)
        {
            activeIndex = newIndex;
            VixenWearEditor.ActiveTab = newIndex;

            for (int i = 0; i < tabContainer.childCount; i++)
            {
                if (tabContainer[i] is Button b)
                {
                    b.RemoveFromClassList("tab-btn-active");
                    b.RemoveFromClassList("tab-btn-inactive");
                    b.AddToClassList(i == activeIndex ? "tab-btn-active" : "tab-btn-inactive");
                }
            }

            descLabel.text = activeIndex switch
            {
                0 => "Base color, albedo, alpha cutoff, and UV animation for the latex surface.",
                1 => "PBR maps, normals, and micro detail shaping the primary surface response.",
                2 => "Clearcoat, thin film, rim, and subsurface polish for premium latex sheen.",
                3 => "Emission, MatCap, light volumes, and LTCGI integration controls.",
                4 => "AudioLink‑driven modulation for emission, film, parallax, and glitch effects.",
                _ => descLabel.text
            };

            imgui.MarkDirtyRepaint();
        }

        for (int i = 0; i < tabNames.Length; i++)
        {
            int index = i;
            var btn = new Button(() => UpdateTabs(index))
            {
                text = tabNames[i]
            };

            btn.AddToClassList("tab-btn");
            btn.AddToClassList(i == activeIndex ? "tab-btn-active" : "tab-btn-inactive");

            tabContainer.Add(btn);
        }

        // Initialize once
        UpdateTabs(activeIndex);

        return root;
    }
}
#endif