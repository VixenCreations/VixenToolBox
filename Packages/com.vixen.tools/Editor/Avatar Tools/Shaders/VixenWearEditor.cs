// VIXEN WEAR — NATIVE SHADERGUI INSPECTOR

using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

// EDITOR DRAWERS (MaterialPropertyDrawers for ShaderLab)

public class VectorLabelDrawer : MaterialPropertyDrawer
{
    private readonly string[] labels = new string[4];
    private readonly bool[] show = new bool[4];
    private readonly int visibleCount = 0;

    public VectorLabelDrawer(string x, string y = "", string z = "", string w = "")
    {
        labels[0] = Sanitize(x);
        labels[1] = Sanitize(y);
        labels[2] = Sanitize(z);
        labels[3] = Sanitize(w);

        for (int i = 0; i < 4; i++)
        {
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

        Rect mainRect = new Rect(pos.x, pos.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(mainRect, label);

        float spacing = 4f;
        float startX = pos.x + EditorGUIUtility.labelWidth;
        float availableWidth = pos.width - EditorGUIUtility.labelWidth;
        float slotWidth = (availableWidth - (spacing * (visibleCount - 1))) / visibleCount;

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

            Rect labelRect = new Rect(currentX, currentY, slotWidth, EditorGUIUtility.singleLineHeight);
            Rect fieldRect = new Rect(currentX, currentY + EditorGUIUtility.singleLineHeight + 2f, slotWidth, EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(labelRect, labels[i], miniStyle);

            float oldVal = (i == 0) ? v.x : (i == 1) ? v.y : (i == 2) ? v.z : v.w;
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

// MAIN SHADER GUI — NATIVE IMGUI IMPLEMENTATION

public class VixenWearEditor : ShaderGUI
{
    public static int ActiveTab
    {
        get => EditorPrefs.GetInt("VixenWear_ActiveTab", 0);
        set => EditorPrefs.SetInt("VixenWear_ActiveTab", value);
    }

    private readonly Color cyan = new Color(0f, 0.898f, 1f);
    private readonly Color pink = new Color(1f, 0f, 0.667f);
    private readonly Color bgBanner = new Color(0.047f, 0.024f, 0.071f);
    private readonly Color bgTabIdle = new Color(0.12f, 0.12f, 0.12f);
    private readonly Color bgTabActive = new Color(0.18f, 0.18f, 0.18f);

    private readonly string[] tabNames = { "BASE", "SURFACE", "POLISH", "INTEGRATION", "AUDIOLINK" };
    private readonly string[] tabDesc = {
        "Base color, albedo, alpha cutoff, and UV animation.",
        "PBR maps, normals, and micro detail shaping.",
        "Clearcoat, thin film, rim, and subsurface translucency.",
        "Emission, MatCap, light volumes, and LTCGI integration.",
        "AudioLink‑driven modulation for reactive effects."
    };

    private GUIStyle Card => new GUIStyle("HelpBox")
    {
        padding = new RectOffset(10, 10, 10, 10),
        margin = new RectOffset(4, 4, 8, 8)
    };

    public override void OnGUI(MaterialEditor ed, MaterialProperty[] p)
    {
        ed.SetDefaultGUIWidths();
        GUILayout.Space(4);

        // --- CUSTOM HEADER BANNER ---
        Rect banner = GUILayoutUtility.GetRect(100, 36);
        EditorGUI.DrawRect(banner, bgBanner);
        EditorGUI.DrawRect(new Rect(banner.x, banner.y + banner.height - 2, banner.width, 2), pink);

        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 13,
            normal = { textColor = Color.white }
        };
        GUI.Label(banner, "LATEX ULTRA CONFIGURATION", titleStyle);
        GUILayout.Space(4);

        // --- NATIVE IMGUI TAB BAR ---
        Rect tabGroupRect = GUILayoutUtility.GetRect(10f, 26f, GUILayout.ExpandWidth(true));
        float tabWidth = tabGroupRect.width / tabNames.Length;

        for (int i = 0; i < tabNames.Length; i++)
        {
            Rect btnRect = new Rect(tabGroupRect.x + (i * tabWidth), tabGroupRect.y, tabWidth, tabGroupRect.height);
            bool isActive = (ActiveTab == i);

            EditorGUI.DrawRect(btnRect, isActive ? bgTabActive : bgTabIdle);
            if (isActive) EditorGUI.DrawRect(new Rect(btnRect.x, btnRect.y + btnRect.height - 2, btnRect.width, 2), cyan);

            if (Event.current.type == EventType.MouseDown && btnRect.Contains(Event.current.mousePosition))
            {
                ActiveTab = i;
                Event.current.Use();
            }

            GUIStyle labelStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel) {
                normal = { textColor = isActive ? cyan : new Color(0.6f, 0.6f, 0.6f) },
                fontStyle = FontStyle.Bold,
                fontSize = 10
            };
            GUI.Label(btnRect, tabNames[i], labelStyle);
        }

        // --- DESCRIPTION LABEL ---
        GUILayout.Space(6);
        GUIStyle descStyle = new GUIStyle(EditorStyles.wordWrappedLabel) {
            fontSize = 11,
            normal = { textColor = new Color(0.7f, 0.7f, 0.7f) },
            alignment = TextAnchor.MiddleCenter
        };
        GUILayout.Label(tabDesc[ActiveTab], descStyle);
        GUILayout.Space(4);

        // --- PROPERTY FETCHING ---
        var _Color = FindProperty("_Color", p);
        var _CutOff = FindProperty("_CutOff", p);
        var _MainTex = FindProperty("_MainTex", p);
        var _MinBrightness = FindProperty("_MinBrightness", p);
        var _UV_Rot = FindProperty("_UV_Rot", p);
        var _SpeedX = FindProperty("_SpeedX", p);
        var _SpeedY = FindProperty("_SpeedY", p);
        var _MatCap_Rot = FindProperty("_MatCap_Rot", p);

        var _Metal = FindProperty("_MetallicGlossMap", p);
        var _Bump = FindProperty("_BumpMap", p);
        var _AO_Str = FindProperty("_AO_Str", p);
        var _Spec_Occ = FindProperty("_Spec_Occ", p);
        var _Shad_Hard = FindProperty("_Shad_Hard", p);
        var _Norm_Str = FindProperty("_Norm_Str", p);

        var _Parallax = FindProperty("_Parallax", p);
        var _Disp_Str = FindProperty("_Disp_Str", p);
        var _Tess_Edge = FindProperty("_Tess_Edge", p);
        var _Emis_Exp = FindProperty("_Emis_Exp", p);

        var _CC_Strength = FindProperty("_CC_Strength", p);
        var _CC_Smoothness = FindProperty("_CC_Smoothness", p);
        var _CC_Spec_AA = FindProperty("_CC_Spec_AA", p);
        var _CC_Flat = FindProperty("_CC_Flat", p);

        var _Film_Str = FindProperty("_Film_Str", p);
        var _Film_Thick = FindProperty("_Film_Thick", p);
        var _Rim_Str = FindProperty("_Rim_Str", p);
        var _Rim_Power = FindProperty("_Rim_Power", p);

        var _SSS_Str = FindProperty("_SSS_Str", p);
        var _SSS_Dist = FindProperty("_SSS_Dist", p);
        var _SSS_Power = FindProperty("_SSS_Power", p);

        var _UseDet = FindProperty("_UseDetailNormal", p);
        var _DetMap = FindProperty("_DetailNormalMap", p);
        var _Det_Strength = FindProperty("_Det_Strength", p);
        var _Det_UV_Tiling = FindProperty("_Det_UV_Tiling", p);

        var _Emis = FindProperty("_EmissionColor", p);
        var _EmisMap = FindProperty("_EmissionMap", p);
        var _MatCap = FindProperty("_MatCap", p);
        var _MatMask = FindProperty("_MatCapMask", p);

        var _MatCap_Int = FindProperty("_MatCap_Int", p);
        var _MatCap_Lit = FindProperty("_MatCap_Lit", p);
        var _LV_Int = FindProperty("_LV_Int", p);
        var _LTCGI_Int = FindProperty("_LTCGI_Int", p);
        var _LV = FindProperty("_UseLightVolumes", p);
        var _LTCGI = FindProperty("_UseLTCGI", p);

        var _AL = FindProperty("_UseAudioLink", p);
        var _ALBand = FindProperty("_AL_EmissionBand", p);
        var _AL_Emis_Mod = FindProperty("_AL_Emis_Mod", p);
        var _AL_Col_Blend = FindProperty("_AL_Col_Blend", p);
        var _AL_Scanlines = FindProperty("_AL_Scanlines", p);
        var _AL_Scan_Speed = FindProperty("_AL_Scan_Speed", p);

        var _AL_Film_Mod = FindProperty("_AL_Film_Mod", p);
        var _AL_Paralx_Mod = FindProperty("_AL_Paralx_Mod", p);
        var _AL_CC_Shatter = FindProperty("_AL_CC_Shatter", p);
        var _AL_Glitch_Mod = FindProperty("_AL_Glitch_Mod", p);

        // --- RENDER ACTIVE TAB ---
        EditorGUILayout.BeginVertical(Card);
        
        if (ActiveTab == 0)
        {
            ed.ShaderProperty(_Color, "Base Color");
            ed.ShaderProperty(_MainTex, "Albedo (RGB) Cutout (A)");
            ed.ShaderProperty(_CutOff, "Alpha Cutoff");
            ed.ShaderProperty(_MinBrightness, "Minimum Brightness");
            GUILayout.Space(10);
            ed.ShaderProperty(_UV_Rot, "UV Rotation");
            ed.ShaderProperty(_SpeedX, "UV Speed X");
            ed.ShaderProperty(_SpeedY, "UV Speed Y");
        }
        else if (ActiveTab == 1)
        {
            ed.ShaderProperty(_Metal, "Packed PBR Map");
            ed.ShaderProperty(_Bump, "Normal Map");
            GUILayout.Space(10);
            ed.ShaderProperty(_AO_Str, "AO Strength");
            ed.ShaderProperty(_Spec_Occ, "Specular Occlusion");
            ed.ShaderProperty(_Shad_Hard, "Shadow Hardness");
            ed.ShaderProperty(_Norm_Str, "Normal Strength");
            GUILayout.Space(10);
            ed.ShaderProperty(_Parallax, "Parallax Depth");
            ed.ShaderProperty(_Disp_Str, "Displacement Strength");
            ed.ShaderProperty(_Tess_Edge, "Tessellation Edge");
            GUILayout.Space(10);
            ed.ShaderProperty(_UseDet, "Enable Micro Detail");
            if (_UseDet.floatValue > 0.5f)
            {
                ed.ShaderProperty(_DetMap, "Micro Detail Map");
                ed.ShaderProperty(_Det_Strength, "Detail Strength");
                ed.ShaderProperty(_Det_UV_Tiling, "Detail UV Tiling");
            }
        }
        else if (ActiveTab == 2)
        {
            ed.ShaderProperty(_CC_Strength, "Clearcoat Strength");
            ed.ShaderProperty(_CC_Smoothness, "Clearcoat Smoothness");
            ed.ShaderProperty(_CC_Spec_AA, "Specular Anti-Aliasing");
            ed.ShaderProperty(_CC_Flat, "Clearcoat Flattening");
            GUILayout.Space(10);
            ed.ShaderProperty(_Film_Str, "Thin Film Strength");
            ed.ShaderProperty(_Film_Thick, "Thin Film Thickness");
            ed.ShaderProperty(_Rim_Str, "Rim Light Strength");
            ed.ShaderProperty(_Rim_Power, "Rim Light Power");
            GUILayout.Space(10);
            ed.ShaderProperty(_SSS_Str, "Subsurface Strength");
            ed.ShaderProperty(_SSS_Dist, "Subsurface Distance");
            ed.ShaderProperty(_SSS_Power, "Subsurface Power");
        }
        else if (ActiveTab == 3)
        {
            ed.ShaderProperty(_Emis, "Emission Color");
            ed.ShaderProperty(_EmisMap, "Emission Map");
            ed.ShaderProperty(_Emis_Exp, "Emission Exposure");
            GUILayout.Space(10);
            ed.ShaderProperty(_MatCap, "MatCap Texture");
            ed.ShaderProperty(_MatMask, "MatCap Mask");
            ed.ShaderProperty(_MatCap_Rot, "MatCap Rotation");
            ed.ShaderProperty(_MatCap_Int, "MatCap Intensity");
            ed.ShaderProperty(_MatCap_Lit, "MatCap Lighting Mix");
            GUILayout.Space(10);
            ed.ShaderProperty(_LV, "Enable Light Volumes");
            if (_LV.floatValue > 0.5f) ed.ShaderProperty(_LV_Int, "Light Volumes Intensity");
            ed.ShaderProperty(_LTCGI, "Enable LTCGI");
            if (_LTCGI.floatValue > 0.5f) ed.ShaderProperty(_LTCGI_Int, "LTCGI Intensity");
        }
        else if (ActiveTab == 4)
        {
            ed.ShaderProperty(_AL, "Enable AudioLink");
            if (_AL.floatValue > 0.5f)
            {
                GUILayout.Space(6);
                ed.ShaderProperty(_ALBand, "Reaction Band");
                GUILayout.Space(10);
                ed.ShaderProperty(_AL_Emis_Mod, "Emission Modulation");
                ed.ShaderProperty(_AL_Col_Blend, "Color Blend Modulation");
                ed.ShaderProperty(_AL_Scanlines, "Scanline Modulation");
                ed.ShaderProperty(_AL_Scan_Speed, "Scanline Speed");
                GUILayout.Space(10);
                ed.ShaderProperty(_AL_Film_Mod, "Thin Film Modulation");
                ed.ShaderProperty(_AL_Paralx_Mod, "Parallax Modulation");
                ed.ShaderProperty(_AL_CC_Shatter, "Clearcoat Shatter");
                ed.ShaderProperty(_AL_Glitch_Mod, "Glitch Modulation");
            }
        }

        EditorGUILayout.EndVertical();

        GUILayout.Space(10);
        ed.RenderQueueField();
        ed.EnableInstancingField();
        ed.DoubleSidedGIField();
    }
}
#endif