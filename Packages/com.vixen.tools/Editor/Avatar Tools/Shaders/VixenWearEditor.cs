// VIXEN WEAR - NATIVE SHADERGUI INSPECTOR (LATEX ULTRA - SYNCED). Place in Editor folder. Matches shader properties and updates shader keywords.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Rendering;

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

    // Cache of short fallbacks for common labels (shown when slot is too narrow even at minimum font).
    private static readonly Dictionary<string, string> ShortLabel = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "X Offset", "X" },
        { "Y Offset", "Y" },
        { "Z Offset", "Z" },
        { "W Offset", "W" },
        { "Scale",    "Scl" },
        { "Rotation", "Rot" },
        { "Speed",    "Spd" },
        { "Tiling",   "Til" }
    };

    private string FitLabel(string text, float slotWidth, GUIStyle style, out int finalFontSize)
    {
        const int MAX_FONT = 11;
        const int MIN_FONT = 8;

        finalFontSize = MAX_FONT;
        style.fontSize = MAX_FONT;

        GUIContent content = new GUIContent(text);
        float w = style.CalcSize(content).x;
        if (w <= slotWidth) return text;

        // Shrink font down to MIN_FONT while still fitting
        for (int size = MAX_FONT - 1; size >= MIN_FONT; size--)
        {
            style.fontSize = size;
            w = style.CalcSize(content).x;
            if (w <= slotWidth)
            {
                finalFontSize = size;
                return text;
            }
        }

        // Still too wide at min font: substitute short form if known
        string shortForm;
        if (ShortLabel.TryGetValue(text.Trim(), out shortForm))
        {
            finalFontSize = MIN_FONT;
            style.fontSize = MIN_FONT;
            return shortForm;
        }

        // Last resort: truncate with ellipsis
        finalFontSize = MIN_FONT;
        style.fontSize = MIN_FONT;
        string truncated = text;
        while (truncated.Length > 2 && style.CalcSize(new GUIContent(truncated + "…")).x > slotWidth)
        {
            truncated = truncated.Substring(0, truncated.Length - 1);
        }
        return truncated.Length < text.Length ? truncated + "…" : truncated;
    }

    public override void OnGUI(Rect pos, MaterialProperty prop, GUIContent label, MaterialEditor editor)
    {
        if (prop.type != MaterialProperty.PropType.Vector || visibleCount == 0)
            return;

        Rect mainRect = new Rect(pos.x, pos.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(mainRect, label);

        float spacing = 4f;
        float startX = pos.x + EditorGUIUtility.labelWidth;
        float availableWidth = pos.width - EditorGUIUtility.labelWidth;
        float slotWidth = (availableWidth - (spacing * (visibleCount - 1))) / visibleCount;

        // Build a per-slot style so font scaling stays consistent within this row.
        GUIStyle miniStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            normal = { textColor = new Color(0.7f, 0.7f, 0.8f) },
            fontSize = 11,
            clipping = TextClipping.Overflow,
            alignment = TextAnchor.LowerCenter,
            wordWrap = false,
            padding = new RectOffset(0, 0, 0, 0)
        };

        // Pre-compute the fitted label + font size for every visible slot.
        string[] fittedText = new string[4];
        int rowFontSize = 11;
        for (int i = 0; i < 4; i++)
        {
            if (!show[i]) continue;
            int slotFont;
            fittedText[i] = FitLabel(labels[i], slotWidth - 2f, miniStyle, out slotFont);
            if (slotFont < rowFontSize) rowFontSize = slotFont;
        }
        miniStyle.fontSize = rowFontSize;

        int drawnCount = 0;
        float currentY = pos.y + EditorGUIUtility.singleLineHeight + 2f;
        Vector4 v = prop.vectorValue;

        // Per-component mixed-value detection - prop.hasMixedValue only signals any-component diff; users expect each X/Y/Z/W to independently show "-" like Unity Vector4Field.
        bool[] mixed = new bool[4];
        UnityEngine.Object[] targets = (editor != null) ? editor.targets : null;
        if (targets != null && targets.Length > 1)
        {
            Material first = null;
            for (int t = 0; t < targets.Length && first == null; t++)
                if (targets[t] is Material mFirst && mFirst.HasProperty(prop.name)) first = mFirst;

            if (first != null)
            {
                Vector4 firstVec = first.GetVector(prop.name);
                for (int t = 0; t < targets.Length; t++)
                {
                    if (!(targets[t] is Material m) || !m.HasProperty(prop.name)) continue;
                    Vector4 vt = m.GetVector(prop.name);
                    for (int c = 0; c < 4; c++)
                        if (!mixed[c] && Mathf.Abs(vt[c] - firstVec[c]) > 1e-5f) mixed[c] = true;
                }
            }
        }

        for (int i = 0; i < 4; i++)
        {
            if (!show[i]) continue;

            float currentX = startX + (slotWidth + spacing) * drawnCount;
            Rect labelRect = new Rect(currentX, currentY, slotWidth, EditorGUIUtility.singleLineHeight);
            Rect fieldRect = new Rect(currentX, currentY + EditorGUIUtility.singleLineHeight + 2f, slotWidth, EditorGUIUtility.singleLineHeight);

            // Tooltip shows full label even when abbreviated.
            GUIContent labelContent = new GUIContent(fittedText[i], labels[i]);
            EditorGUI.LabelField(labelRect, labelContent, miniStyle);

            float oldVal = (i == 0) ? v.x : (i == 1) ? v.y : (i == 2) ? v.z : v.w;

            EditorGUI.BeginChangeCheck();
            bool prevMixed = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = mixed[i];
            float newVal = EditorGUI.FloatField(fieldRect, GUIContent.none, oldVal);
            EditorGUI.showMixedValue = prevMixed;

            if (EditorGUI.EndChangeCheck())
            {
                // Per-component, per-material write - preserves each target's other components (prop.vectorValue would propagate the first material's full vector to all selected materials, which was the original bug).
                if (targets != null && targets.Length > 0)
                {
                    Undo.RecordObjects(targets, "Edit " + prop.displayName);
                    foreach (var t in targets)
                    {
                        if (!(t is Material m) || !m.HasProperty(prop.name)) continue;
                        Vector4 cur = m.GetVector(prop.name);
                        cur[i] = newVal;
                        m.SetVector(prop.name, cur);
                    }
                }
                else
                {
                    // Fallback (no targets metadata): write through the property normally.
                    if (i == 0) v.x = newVal;
                    else if (i == 1) v.y = newVal;
                    else if (i == 2) v.z = newVal;
                    else v.w = newVal;
                    prop.vectorValue = v;
                }
            }

            drawnCount++;
        }
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

        // Change-gate the write - unconditional p.floatValue = ... overwrites every selected material with the first material's value on every repaint, breaking multi-edit.
        EditorGUI.BeginChangeCheck();
        bool prevMixed = EditorGUI.showMixedValue;
        EditorGUI.showMixedValue = p.hasMixedValue;
        int newVal = EditorGUI.IntPopup(r, final, (int)p.floatValue, names, vals);
        EditorGUI.showMixedValue = prevMixed;
        if (EditorGUI.EndChangeCheck())
            p.floatValue = newVal;
    }
}

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

    private readonly string[] tabNames =
    {
        "BASE",
        "SURFACE",
        "POLISH",
        "INTEGRATION",
        "AUDIOLINK",
        "STAGE"
    };

    private readonly string[] tabDesc =
    {
        "Base color, albedo, alpha cutoff, and UV animation.",
        "PBR maps, normals, parallax, and micro detail shaping.",
        "Clearcoat, thin film, rim, and volumetric SSS thickness.",
        "Emission, MatCap, light volumes, and LTCGI illumination.",
        "AudioLink bands driving emission, UV/vertex engines, and shards.",
        "VRSL DMX routing, intensity override, and kinetic geo-warping."
    };

    // Tab → property names (must match shader Properties)
    private readonly string[][] tabProps = new string[][]
    {
        // BASE
        new[]
        {
            "_Mode",
            "_Color",
            "_CutOff",
            "_MainTex",
            "_MinBrightness",
            "_UV_Rot",
            "_SpeedX",
            "_SpeedY"
        },
        // SURFACE
        new[]
        {
            "_MetallicGlossMap",
            "_PBR_Met_Ch",
            "_PBR_Met_Inv",
            "_PBR_Smooth_Ch",
            "_PBR_Smooth_Inv",
            "_PBR_AO_Ch",
            "_PBR_Height_Ch",
            "_UsePackedMasks",
            "_ReflMask_Ch",
            "_ReflMask_Inv",
            "_ReflMask_Str",
            "_SpecMask_Ch",
            "_SpecMask_Inv",
            "_SpecMask_Str",
            "_BumpMap",
            "_AO_Str",
            "_Spec_Occ",
            "_Shad_Hard",
            "_Norm_Str",
            "_Parallax",
            "_Disp_Str",
            "_Tess_Edge",
            "_UseDetailNormal",
            "_DetailNormalMap",
            "_Det_Strength",
            "_Det_UV_Tiling"
        },
        // POLISH
        new[]
        {
            "_CC_Strength",
            "_CC_Smoothness",
            "_CC_Spec_AA",
            "_CC_Flat",
            "_CC_Tint",
            "_CC_F0",
            "_Film_Str",
            "_Film_Thick",
            "_Rim_Str",
            "_Rim_Power",
            "_SSS_Str",
            "_SSS_Dist",
            "_SSS_Power",
            "_Aniso",
            "_AnisoRot",
            "_Trans_Str",
            "_Trans_Dist",
            "_Trans_Power",
            "_UseMultiScatter",
            "_UsePolish",
            "_PolishMask",
            "_PolishMaskCh",
            "_UseDrip",
            "_DripMask",
            "_DripMaskCh",
            "_Drip_Density",
            "_Drip_Width",
            "_Drip_Coverage",
            "_Drip_Speed",
            "_Drip_Strength",
            "_Drip_Normal",
            "_Drip3D_Strength",
            "_Drip3D_Scale",
            "_Drip3D_Sheen",
            "_Drip3D_Fall",
            "_Drip_Sway",
            "_Drip_BodyFollow",
            "_Drip_FloorCollide",
            "_Wet_Amount",
            "_Wet_Darken",
            "_Wet_Smoothness",
            "_Wet_Sheen",
            "_Wet_Flatten",
            "_UseGoo",
            "_GooMask",
            "_GooMaskCh",
            "_Goo_Strength",
            "_Goo_Noise",
            "_Goo_Speed",
            "_Goo_Droop",
            "_Goo_Reach",
            "_Goo_Variation",
            "_Goo_ToGround",
            "_Goo_GroundY",
            "_Goo_Sway",
            "_Goo_SwaySpeed",
            "_Goo_BodyFollow",
            "_Goo_FloorCollide",
            "_Goo_Pool",
            "_UseOutline",
            "_OutlineColor",
            "_OutlineEmis",
            "_OutlineWidth",
            "_MaxOutlineWidth",
            "_OutlineViewFudge",
            "_OutlineMask",
            "_OutlineMaskCh",
            "_AL_Band_Outline",
            "_AL_Outline_Mod"
        },
        // INTEGRATION
        new[]
        {
            "_EmissionColor",
            "_EmissionMap",
            "_Emis_Exp",
            "_UseEmission2",
            "_EmissionColor2",
            "_EmissionMap2",
            "_Emis2_MaskCh",
            "_AL_Band_Emis2",
            "_AL_Emis2_Mod",
            "_UseRegionMask",
            "_RegionMask",
            "_Region_R_Tint",
            "_Region_R_Emis",
            "_Region_G_Tint",
            "_Region_G_Emis",
            "_Region_B_Tint",
            "_Region_B_Emis",
            "_MatCap",
            "_MatCapMask",
            "_MatCap_MaskCh",
            "_MatCap_Tint",
            "_MatCap_Rot",
            "_MatCap_Int",
            "_MatCap_Lit",
            "_UseMatCap2",
            "_MatCap2",
            "_MatCap2_Mask",
            "_MatCap2_MaskCh",
            "_MatCap2_Tint",
            "_MatCap2_Rot",
            "_MatCap2_Int",
            "_MatCap2_Blend",
            "_LV_Int",
            "_LV_Spec_Mix",
            "_LV_Spec_Dominant",
            "_LV_CC_Spec_Mix",
            "_LV_Bias",
            "_LV_PosOffset",
            "_LV_AdditiveOnly",
            "_LV_ProbeDering",
            "_LTCGI_Int",
            "_LTCGI_Spec_Mix",
            "_LTCGI_Diff_Mix"
        },
        // AUDIOLINK / KINETIC
        new[]
        {
            "_UseAudioLink",
            "_AL_ColorMode",
            "_AL_Strip_Pos",
            "_UseMediaState",

            "_AL_Chrono_Idx",
            "_UseChronoFX",

            "_UseCyber",
            "_CyberMask",
            "_Cyber_Hover",
            "_Cyber_Hover_Bob",
            "_UseCyberVU",
            "_Cyber_VU_Style",
            "_Cyber_VU_Str",
            "_Cyber_VU_Transform",
            "_UseCyberCC",
            "_Cyber_CC_Band",
            "_Cyber_CC_Str",
            "_Cyber_CC_Density",
            "_Cyber_CC_Transform",
            "_UseCyberWave",
            "_Cyber_Wave_Band",
            "_Cyber_Wave_Str",
            "_Cyber_Wave_Transform",
            "_UseCyberDMX",
            "_Cyber_DMX_Band",
            "_Cyber_DMX_Str",
            "_Cyber_DMX_Transform",
            "_UseCyberAuto",
            "_Cyber_Auto_Band",
            "_Cyber_AutoCorr_Str",
            "_Cyber_Auto_Transform",
            "_Cyber_Auto_Shimmer",
            "_Cyber_Auto_Shimmer_Band",
            "_Cyber_Auto_Pop",
            "_Cyber_Auto_Pop_Band",
            "_Cyber_Auto_Sizzle",
            "_Cyber_Auto_Sizzle_Band",
            "_Cyber_Auto_Electrify",
            "_Cyber_Auto_Electrify_Band",

            "_UseVtxKinetic",
            "_Vtx_Pump_Band",
            "_Vtx_Pump_Str",
            "_Vtx_Fracture_Band",
            "_Vtx_Fracture_Str",
            "_Vtx_Fracture_Amount",
            "_Vtx_Fracture_Dist",
            "_Vtx_Fracture_Spin",
            "_Vtx_Fracture_Spiral",
            "_Vtx_Fracture_Lift",
            "_Vtx_Fracture_Float",
            "_Vtx_Fracture_Trail",
            "_Shard_ColorMod",
            "_Shard_ColorMod_Speed",
            "_UseShardCC",
            "_Shard_CC_Str",
            "_Vtx_AutoCorr_Str",

            "_UseALVortex",
            "_AL_Vortex_Band",
            "_AL_Vortex_Str",
            "_AL_Vortex_UV",
            "_UseALPump",
            "_AL_Pump_Band",
            "_AL_Pump_Str",
            "_AL_Pump_UV",
            "_UseALFracture",
            "_AL_Fracture_Band",
            "_AL_Fracture_Str",
            "_AL_Fracture_UV",

            "_AL_Band_Emission",
            "_AL_Emis_Mod",
            "_AL_Col_Blend",
            "_AL_Waveform_Mod",
            "_AL_AutoCorr_Mod",
            "_AL_DFT_Note",
            "_AL_DFT_Mod",

            "_AL_Band_Scanlines",
            "_AL_Scanlines",
            "_AL_Scan_Density",
            "_AL_Scan_Speed",
            "_AL_Scan_React",

            "_AL_Band_Film",
            "_AL_Film_Mod",
            "_AL_Band_Parallax",
            "_AL_Paralx_Mod",
            "_AL_Band_Shatter",
            "_AL_CC_Shatter",
            "_AL_Band_Glitch",
            "_AL_Glitch_Mod"
        },
        // STAGE / VRSL
        new[]
        {
            "_UseVRSL",
            "_DMX_Channel",
            "_VRSL_Intensity",
            "_VRSL_Geo_Warp",
            "_VRSL_Color_Hijack"
        }
    };

    public class TabClipboard
    {
        public int TabIndex;
        public Dictionary<string, float> Floats = new Dictionary<string, float>();
        public Dictionary<string, Color> Colors = new Dictionary<string, Color>();
        public Dictionary<string, Vector4> Vectors = new Dictionary<string, Vector4>();
        public Dictionary<string, Texture> Textures = new Dictionary<string, Texture>();
        public Dictionary<string, Vector2> TexOffsets = new Dictionary<string, Vector2>();
        public Dictionary<string, Vector2> TexScales = new Dictionary<string, Vector2>();
    }

    private static TabClipboard _clipboard = null;

    private GUIStyle Card => new GUIStyle("HelpBox")
    {
        padding = new RectOffset(10, 10, 10, 10),
        margin = new RectOffset(4, 4, 8, 8)
    };

    private void DrawProp(MaterialEditor ed, MaterialProperty prop, string label)
    {
        if (prop == null) return;

        float height = ed.GetPropertyHeight(prop, label);
        Rect r = EditorGUILayout.GetControlRect(true, height);
        Rect labelRect = new Rect(r.x, r.y, EditorGUIUtility.labelWidth, r.height);

        if (Event.current != null &&
            Event.current.type == EventType.ContextClick &&
            labelRect.Contains(Event.current.mousePosition))
        {
            GenericMenu menu = new GenericMenu();
            string animPath = prop.name;
            menu.AddItem(new GUIContent($"Copy Property Name ({prop.name})"), false, () =>
            {
                EditorGUIUtility.systemCopyBuffer = animPath;
                Debug.Log($"[Vixen Wear] Copied to clipboard: {animPath}");
            });
            menu.ShowAsContext();
            Event.current.Use();
        }

        EditorGUI.BeginChangeCheck();
        ed.ShaderProperty(r, prop, label);
        if (EditorGUI.EndChangeCheck())
        {
            ed.PropertiesChanged();
            UpdateKeywordsForTargets(ed.targets);
        }
    }

    // Sets a float/range/enum property on all targets if it exists (used by one-click setup helpers). Null-safe so it no-ops on shader variants missing the property.
    private void SetF(MaterialProperty[] p, string name, float value)
    {
        MaterialProperty prop = FindProperty(name, p, false);
        if (prop != null) prop.floatValue = value;
    }

    private void PerformPaste(MaterialEditor ed, MaterialProperty[] p, int tabIndex, bool includeTextures)
    {
        Undo.RecordObjects(ed.targets, "Paste Tab Settings");

        foreach (MaterialProperty prop in p)
        {
            if (Array.IndexOf(tabProps[tabIndex], prop.name) < 0) continue;

            switch (prop.type)
            {
                case MaterialProperty.PropType.Float:
                case MaterialProperty.PropType.Range:
                    if (_clipboard.Floats.TryGetValue(prop.name, out float fVal))
                        prop.floatValue = fVal;
                    break;

                case MaterialProperty.PropType.Color:
                    if (_clipboard.Colors.TryGetValue(prop.name, out Color cVal))
                        prop.colorValue = cVal;
                    break;

                case MaterialProperty.PropType.Vector:
                    if (_clipboard.Vectors.TryGetValue(prop.name, out Vector4 vVal))
                        prop.vectorValue = vVal;
                    break;

                case MaterialProperty.PropType.Texture:
                    if (!includeTextures) break;
                    if (_clipboard.Textures.TryGetValue(prop.name, out Texture tVal))
                        prop.textureValue = tVal;

                    if (_clipboard.TexOffsets.TryGetValue(prop.name, out Vector2 offset) &&
                        _clipboard.TexScales.TryGetValue(prop.name, out Vector2 scale))
                    {
                        foreach (var tgt in ed.targets)
                        {
                            Material m = (Material)tgt;
                            m.SetTextureOffset(prop.name, offset);
                            m.SetTextureScale(prop.name, scale);
                        }
                    }
                    break;
            }
        }

        ed.PropertiesChanged();
        UpdateKeywordsForTargets(ed.targets);

        // BASE tab carries _Mode - re-run full blend/queue/tag setup so the destination material's blend state matches the pasted mode rather than the previous mode's leftover state.
        if (tabIndex == 0 && _clipboard.Floats.ContainsKey("_Mode"))
        {
            foreach (var t in ed.targets)
                if (t is Material m && m.HasProperty("_Mode"))
                    SetupMaterialWithBlendMode(m, (int)m.GetFloat("_Mode"));
        }

        Debug.Log($"[Vixen Wear] Pasted {tabNames[tabIndex]} tab settings {(includeTextures ? "with" : "without")} textures. Applied to {ed.targets.Length} materials.");
    }

    private void PerformReset(MaterialEditor ed, MaterialProperty[] p, int tabIndex)
    {
        Material sourceMat = (Material)ed.target;
        if (sourceMat == null || sourceMat.shader == null) return;

        // A fresh material built from the same shader carries all shader-declared defaults (floats, colors, vectors, and Unity's built-in white/black/bump/gray textures).
        Material defaults = new Material(sourceMat.shader) { hideFlags = HideFlags.HideAndDontSave };

        try
        {
            Undo.RecordObjects(ed.targets, $"Reset {tabNames[tabIndex]} Tab");

            foreach (string propName in tabProps[tabIndex])
            {
                MaterialProperty prop = FindProperty(propName, p, false);
                if (prop == null || !defaults.HasProperty(propName)) continue;

                switch (prop.type)
                {
                    case MaterialProperty.PropType.Float:
                    case MaterialProperty.PropType.Range:
                        prop.floatValue = defaults.GetFloat(propName);
                        break;
                    case MaterialProperty.PropType.Color:
                        prop.colorValue = defaults.GetColor(propName);
                        break;
                    case MaterialProperty.PropType.Vector:
                        prop.vectorValue = defaults.GetVector(propName);
                        break;
                    case MaterialProperty.PropType.Texture:
                        prop.textureValue = defaults.GetTexture(propName);
                        Vector2 defOffset = defaults.GetTextureOffset(propName);
                        Vector2 defScale  = defaults.GetTextureScale(propName);
                        foreach (var tgt in ed.targets)
                        {
                            Material m = (Material)tgt;
                            m.SetTextureOffset(propName, defOffset);
                            m.SetTextureScale(propName, defScale);
                        }
                        break;
                }
            }

            ed.PropertiesChanged();
            UpdateKeywordsForTargets(ed.targets);

            // BASE tab carries _Mode - re-apply full blend/queue/tag state so the reset value of _Mode actually takes visual effect (otherwise blend state would lag behind the property).
            if (tabIndex == 0)
            {
                foreach (var t in ed.targets)
                    if (t is Material m && m.HasProperty("_Mode"))
                        SetupMaterialWithBlendMode(m, (int)m.GetFloat("_Mode"));
            }

            Debug.Log($"[Vixen Wear] Reset {tabNames[tabIndex]} tab to shader defaults. Applied to {ed.targets.Length} material(s).");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(defaults);
        }
    }

    // Helper: convert targets to Material[] safely
    private Material[] GetMaterialsFromTargets(UnityEngine.Object[] targets)
    {
        List<Material> mats = new List<Material>();
        foreach (var t in targets)
        {
            if (t is Material m) mats.Add(m);
            else if (t is Renderer r && r.sharedMaterial != null) mats.Add(r.sharedMaterial);
        }
        return mats.ToArray();
    }

    // Update shader keywords for all selected materials
    private void UpdateKeywordsForTargets(UnityEngine.Object[] targets)
    {
        Material[] mats = GetMaterialsFromTargets(targets);
        foreach (var mat in mats) UpdateKeywords(mat);
    }

    // Sync shader keywords to material toggle properties. Public/static so the build preprocessor can call it.
    public static void SyncKeywords(Material mat)
    {
        if (mat == null) return;

        bool vrsl  = mat.HasProperty("_UseVRSL")        && mat.GetFloat("_UseVRSL")        > 0.5f;
        bool ltcgi = mat.HasProperty("_LTCGI_Int")      && mat.GetFloat("_LTCGI_Int")      > 0.001f;
        bool lv    = mat.HasProperty("_LV_Int")         && mat.GetFloat("_LV_Int")         > 0.001f;
        bool det   = mat.HasProperty("_UseDetailNormal")&& mat.GetFloat("_UseDetailNormal")> 0.5f;

        SetKeyword(mat, "VRSL_ENABLE",         vrsl);
        SetKeyword(mat, "LTCGI_ENABLE",        ltcgi);
        SetKeyword(mat, "LIGHTVOLUMES_ENABLE", lv);
        SetKeyword(mat, "_DETAIL_NORMAL",      det);
        // AudioLink is runtime-gated by _UseAudioLink (no build-time keyword) so VRCFury material-toggle animations can flip it without a compiled variant - strip the stale keyword.
        mat.DisableKeyword("AL_ENABLE");
        // Force-disable CYBER_ENABLE - shader never #if-gates on it, so the 2x variant set is dead.
        mat.DisableKeyword("CYBER_ENABLE");

        // Clear EmissiveIsBlack so Unity's build pipeline doesn't strip _EmissionColor/_EmissionMap/_EmissionColor2 from materials whose flag was never updated (default on freshly cloned mats, e.g. VRCFury swap targets).
        mat.globalIlluminationFlags &= ~MaterialGlobalIlluminationFlags.EmissiveIsBlack;

        // Alpha workflow keywords mirror _Mode - done here (not just in SetupMaterialWithBlendMode) so upgraded materials pick up the right keyword on the next build/play-mode transition without an inspector visit.
        if (mat.HasProperty("_Mode"))
        {
            int mode = (int)mat.GetFloat("_Mode");
            SetKeyword(mat, "_ALPHATEST_ON",        mode == 1);
            SetKeyword(mat, "_ALPHABLEND_ON",       mode == 2);
            SetKeyword(mat, "_ALPHAPREMULTIPLY_ON", mode == 3);
        }
    }

    // Full alpha-workflow setup (blend state, ZWrite, render queue, RenderType + VRCFallback tags, keywords) - called on _Mode change or shader assignment; SyncKeywords handles the lighter keyword-only case.
    public static void SetupMaterialWithBlendMode(Material material, int blendMode)
    {
        if (material == null) return;

        switch (blendMode)
        {
            case 0: // Opaque
                material.SetOverrideTag("RenderType",  "Opaque");
                material.SetOverrideTag("VRCFallback", "ToonDoubleSided");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = -1;
                break;
            case 1: // Cutout
                material.SetOverrideTag("RenderType",  "TransparentCutout");
                material.SetOverrideTag("VRCFallback", "ToonCutoutDoubleSided");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.EnableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                break;
            case 2: // Fade - straight alpha, everything (including specular) fades out together.
                material.SetOverrideTag("RenderType",  "Transparent");
                material.SetOverrideTag("VRCFallback", "ToonTransparentDoubleSided");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                break;
            case 3: // Transparent - premultiplied alpha; specular highlights survive at low opacity (glass/latex).
                material.SetOverrideTag("RenderType",  "Transparent");
                material.SetOverrideTag("VRCFallback", "ToonTransparentDoubleSided");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                break;
        }
    }

    // Initialize blend/queue/tag state when the shader is first applied so newly-created materials don't render with stale queue/blend from whatever shader was previously assigned.
    public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
    {
        base.AssignNewShaderToMaterial(material, oldShader, newShader);
        if (material != null && material.HasProperty("_Mode"))
            SetupMaterialWithBlendMode(material, (int)material.GetFloat("_Mode"));
        // Clear EmissiveIsBlack on first shader assignment so Unity's build pipeline can't strip emission properties from this material later.
        if (material != null)
            material.globalIlluminationFlags &= ~MaterialGlobalIlluminationFlags.EmissiveIsBlack;
    }

    private void UpdateKeywords(Material mat) => SyncKeywords(mat);

    // Small helper to set keywords safely
    private static void SetKeyword(Material mat, string keyword, bool enabled)
    {
        if (enabled) mat.EnableKeyword(keyword);
        else mat.DisableKeyword(keyword);
    }

    public override void OnGUI(MaterialEditor ed, MaterialProperty[] p)
    {
        ed.SetDefaultGUIWidths();
        EditorGUIUtility.labelWidth = Mathf.Min(220f, EditorGUIUtility.currentViewWidth * 0.55f);
        GUILayout.Space(4);

        // Banner
        Rect banner = GUILayoutUtility.GetRect(100, 36);
        EditorGUI.DrawRect(banner, bgBanner);
        EditorGUI.DrawRect(new Rect(banner.x, banner.y + banner.height - 2, banner.width, 2), pink);

        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 13,
            normal = { textColor = Color.white }
        };
        GUI.Label(banner, "VIXENWEAR EDITOR - LATEX ULTRA", titleStyle);
        GUILayout.Space(4);

        // Tabs
        Rect tabGroupRect = GUILayoutUtility.GetRect(10f, 26f, GUILayout.ExpandWidth(true));
        float tabWidth = tabGroupRect.width / tabNames.Length;

        for (int i = 0; i < tabNames.Length; i++)
        {
            Rect btnRect = new Rect(tabGroupRect.x + (i * tabWidth), tabGroupRect.y, tabWidth, tabGroupRect.height);
            bool isActive = (ActiveTab == i);

            EditorGUI.DrawRect(btnRect, isActive ? bgTabActive : bgTabIdle);
            if (isActive)
                EditorGUI.DrawRect(new Rect(btnRect.x, btnRect.y + btnRect.height - 2, btnRect.width, 2), cyan);

            // Context menu for copy/paste tab
            if (Event.current.type == EventType.ContextClick && btnRect.Contains(Event.current.mousePosition))
            {
                GenericMenu menu = new GenericMenu();
                int tabIndex = i;

                menu.AddItem(new GUIContent($"Copy {tabNames[tabIndex]} Settings"), false, () =>
                {
                    _clipboard = new TabClipboard { TabIndex = tabIndex };
                    Material sourceMat = (Material)ed.target;

                    foreach (string propName in tabProps[tabIndex])
                    {
                        MaterialProperty prop = FindProperty(propName, p, false);
                        if (prop == null) continue;

                        switch (prop.type)
                        {
                            case MaterialProperty.PropType.Float:
                            case MaterialProperty.PropType.Range:
                                _clipboard.Floats[propName] = prop.floatValue;
                                break;
                            case MaterialProperty.PropType.Color:
                                _clipboard.Colors[propName] = prop.colorValue;
                                break;
                            case MaterialProperty.PropType.Vector:
                                _clipboard.Vectors[propName] = prop.vectorValue;
                                break;
                            case MaterialProperty.PropType.Texture:
                                _clipboard.Textures[propName] = prop.textureValue;
                                if (sourceMat.HasProperty(propName))
                                {
                                    _clipboard.TexOffsets[propName] = sourceMat.GetTextureOffset(propName);
                                    _clipboard.TexScales[propName] = sourceMat.GetTextureScale(propName);
                                }
                                break;
                        }
                    }

                    Debug.Log($"[Vixen Wear] Copied {tabNames[tabIndex]} tab settings.");
                });

                if (_clipboard != null && _clipboard.TabIndex == tabIndex)
                {
                    menu.AddItem(new GUIContent($"Paste {tabNames[tabIndex]} Settings (Values Only)"), false,
                        () => PerformPaste(ed, p, tabIndex, false));
                    menu.AddItem(new GUIContent($"Paste {tabNames[tabIndex]} Settings (With Textures)"), false,
                        () => PerformPaste(ed, p, tabIndex, true));
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent($"Paste {tabNames[tabIndex]} Settings (Values Only)"));
                    menu.AddDisabledItem(new GUIContent($"Paste {tabNames[tabIndex]} Settings (With Textures)"));
                }

                menu.AddSeparator("");
                menu.AddItem(new GUIContent($"Reset {tabNames[tabIndex]} to Defaults"), false, () =>
                {
                    if (EditorUtility.DisplayDialog(
                        "Reset Tab to Defaults",
                        $"Reset all {tabNames[tabIndex]} properties to shader defaults?\n\nThis affects {ed.targets.Length} material(s). Use Undo (Ctrl+Z) to revert.",
                        "Reset", "Cancel"))
                    {
                        PerformReset(ed, p, tabIndex);
                    }
                });

                menu.ShowAsContext();
                Event.current.Use();
            }

            if (Event.current.type == EventType.MouseDown && btnRect.Contains(Event.current.mousePosition))
            {
                ActiveTab = i;
                Event.current.Use();
            }

            GUIStyle labelStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                normal = { textColor = isActive ? cyan : new Color(0.6f, 0.6f, 0.6f) },
                fontStyle = FontStyle.Bold,
                fontSize = 10
            };
            GUI.Label(btnRect, tabNames[i], labelStyle);
        }

        GUILayout.Space(6);

        GUIStyle descStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
        {
            fontSize = 11,
            normal = { textColor = new Color(0.7f, 0.7f, 0.7f) },
            alignment = TextAnchor.MiddleCenter
        };
        GUILayout.Label(tabDesc[ActiveTab], descStyle);
        GUILayout.Space(4);

        EditorGUILayout.BeginVertical(Card);

        // BASE
        if (ActiveTab == 0)
        {
            EditorGUILayout.LabelField("Rendering Mode", EditorStyles.boldLabel);
            var _ModeProp = FindProperty("_Mode", p, false);
            if (_ModeProp != null)
            {
                // Render the dropdown ourselves so we can fire SetupMaterialWithBlendMode on change - DrawProp's inner change-check still fires SyncKeywords, and the outer check here applies the full blend/queue/tag state.
                EditorGUI.BeginChangeCheck();
                DrawProp(ed, _ModeProp, "Rendering Mode");
                if (EditorGUI.EndChangeCheck())
                {
                    int mode = (int)_ModeProp.floatValue;
                    foreach (var t in ed.targets)
                        if (t is Material m) SetupMaterialWithBlendMode(m, mode);
                }

                int currentMode = (int)_ModeProp.floatValue;
                if (currentMode == 1)
                {
                    // Cutout is the only mode that uses _CutOff - fade/transparent ignore it.
                    DrawProp(ed, FindProperty("_CutOff", p, false), "Alpha Cutoff");
                }
                else if (currentMode >= 2)
                {
                    EditorGUILayout.HelpBox("Fade/Transparent read alpha from Albedo (A). Fully transparent fragments are still discarded so they don't cast shadows. Disable shadow casting on the renderer if soft-edge fragments produce hard shadows.", MessageType.Info);
                }
            }
            GUILayout.Space(8);

            DrawProp(ed, FindProperty("_Color", p, false), "Base Color");
            DrawProp(ed, FindProperty("_MainTex", p, false), "Albedo (RGB) Cutout (A)");
            DrawProp(ed, FindProperty("_MinBrightness", p, false), "Minimum Brightness");
            GUILayout.Space(10);

            DrawProp(ed, FindProperty("_UV_Rot", p, false), "UV Rotation");
            DrawProp(ed, FindProperty("_SpeedX", p, false), "UV Speed X");
            DrawProp(ed, FindProperty("_SpeedY", p, false), "UV Speed Y");
        }
        // SURFACE
        else if (ActiveTab == 1)
        {
            DrawProp(ed, FindProperty("_MetallicGlossMap", p, false), "Packed PBR Map");
            EditorGUILayout.HelpBox("Poiyomi/Substance/Marmoset compatibility - pick which channel of the packed map drives each PBR property. Defaults are VixenWear native (R:Met G:AO B:Disp A:Smooth). For a Poiyomi/Mochie 'Metallic Maps' texture (R:Met G:Smooth B:Reflection A:Specular), use the one-click setup button below.", MessageType.None);
            EditorGUI.indentLevel++;
            DrawProp(ed, FindProperty("_PBR_Met_Ch", p, false), "Metallic Channel");
            DrawProp(ed, FindProperty("_PBR_Met_Inv", p, false), "Invert Metallic");
            DrawProp(ed, FindProperty("_PBR_Smooth_Ch", p, false), "Smoothness Channel");
            DrawProp(ed, FindProperty("_PBR_Smooth_Inv", p, false), "Channel Stores Roughness (Invert)");
            DrawProp(ed, FindProperty("_PBR_AO_Ch", p, false), "AO Channel");
            DrawProp(ed, FindProperty("_PBR_Height_Ch", p, false), "Height Channel");
            EditorGUI.indentLevel--;
            GUILayout.Space(6);

            // Poiyomi/Mochie reflection + specular masks, sampled from the packed PBR map above.
            var _UsePM = FindProperty("_UsePackedMasks", p, false);
            DrawProp(ed, _UsePM, "Reflection / Specular Masks");
            if (_UsePM != null && _UsePM.floatValue > 0.5f)
            {
                EditorGUI.indentLevel++;
                DrawProp(ed, FindProperty("_ReflMask_Ch", p, false), "Reflection Mask Channel");
                DrawProp(ed, FindProperty("_ReflMask_Inv", p, false), "Invert Reflection Mask");
                DrawProp(ed, FindProperty("_ReflMask_Str", p, false), "Reflection Mask Strength");
                DrawProp(ed, FindProperty("_SpecMask_Ch", p, false), "Specular Mask Channel");
                DrawProp(ed, FindProperty("_SpecMask_Inv", p, false), "Invert Specular Mask");
                DrawProp(ed, FindProperty("_SpecMask_Str", p, false), "Specular Mask Strength");
                EditorGUILayout.HelpBox("Reflection Mask dims environment / reflection-probe specular (including clearcoat env, Light Volume, and LTCGI reflections). Specular Mask dims direct light highlights. Channel defaults match Mochie packing (B = reflection, A = specular). Matcaps keep their own masks and are not affected.", MessageType.None);
                EditorGUI.indentLevel--;
            }
            GUILayout.Space(6);

            if (GUILayout.Button("Set Up for Poiyomi / Mochie Metallic Map"))
            {
                Undo.RecordObjects(ed.targets, "Mochie Metallic Map Setup");
                SetF(p, "_PBR_Met_Ch", 0f); SetF(p, "_PBR_Met_Inv", 0f);
                SetF(p, "_PBR_Smooth_Ch", 1f); SetF(p, "_PBR_Smooth_Inv", 0f);
                SetF(p, "_PBR_AO_Ch", 4f);
                SetF(p, "_UsePackedMasks", 1f);
                SetF(p, "_ReflMask_Ch", 2f); SetF(p, "_ReflMask_Inv", 0f); SetF(p, "_ReflMask_Str", 1f);
                SetF(p, "_SpecMask_Ch", 3f); SetF(p, "_SpecMask_Inv", 0f); SetF(p, "_SpecMask_Str", 1f);
                ed.PropertiesChanged();
                UpdateKeywordsForTargets(ed.targets);
            }
            EditorGUILayout.HelpBox("One-click translation: maps a Mochie/Poiyomi 'Metallic Maps' texture (R:Metallic G:Smoothness B:Reflection Mask A:Specular Mask) onto our packed PBR - sets the channels, disables AO (None), and enables the masks. Drop the Mochie map into Packed PBR Map above first.", MessageType.None);
            GUILayout.Space(6);

            DrawProp(ed, FindProperty("_BumpMap", p, false), "Normal Map");
            GUILayout.Space(10);

            DrawProp(ed, FindProperty("_AO_Str", p, false), "Multi-Bounce AO Strength");
            DrawProp(ed, FindProperty("_Spec_Occ", p, false), "Physical Specular Occlusion");
            DrawProp(ed, FindProperty("_Shad_Hard", p, false), "Parallax Shadow Hardness");
            DrawProp(ed, FindProperty("_Norm_Str", p, false), "Normal Strength");
            GUILayout.Space(10);

            DrawProp(ed, FindProperty("_Parallax", p, false), "Parallax Depth");
            DrawProp(ed, FindProperty("_Disp_Str", p, false), "Displacement Strength");
            DrawProp(ed, FindProperty("_Tess_Edge", p, false), "Tessellation Edge Length");
            GUILayout.Space(10);

            var _UseDet = FindProperty("_UseDetailNormal", p, false);
            DrawProp(ed, _UseDet, "Enable Micro Detail");
            if (_UseDet != null && _UseDet.floatValue > 0.5f)
            {
                DrawProp(ed, FindProperty("_DetailNormalMap", p, false), "Micro Detail Map");
                DrawProp(ed, FindProperty("_Det_Strength", p, false), "Detail Strength");
                DrawProp(ed, FindProperty("_Det_UV_Tiling", p, false), "Detail UV Tiling");
            }
        }
        // POLISH
        else if (ActiveTab == 2)
        {
            var _UsePol = FindProperty("_UsePolish", p, false);
            DrawProp(ed, _UsePol, "Enable Polish Layer");
            EditorGUILayout.HelpBox("Master gate for the polish lighting layer (clearcoat, thin film, SSS, transmission, anisotropy, rim, multi-scatter). Off collapses the material to a flat GGX base. Drip, Goo, and Outline below have their own toggles.", MessageType.None);
            if (_UsePol != null && _UsePol.floatValue > 0.5f)
            {
                EditorGUI.indentLevel++;
                DrawProp(ed, FindProperty("_PolishMask", p, false), "Polish Mask (B&W)");
                DrawProp(ed, FindProperty("_PolishMaskCh", p, false), "Polish Mask Channel");
                GUILayout.Space(8);

                DrawProp(ed, FindProperty("_CC_Strength", p, false), "Clearcoat Strength");
                DrawProp(ed, FindProperty("_CC_Smoothness", p, false), "Clearcoat Smoothness");
                DrawProp(ed, FindProperty("_CC_Spec_AA", p, false), "Specular Anti-Aliasing");
                DrawProp(ed, FindProperty("_CC_Flat", p, false), "Clearcoat Flattening");
                DrawProp(ed, FindProperty("_CC_Tint", p, false), "Clearcoat Tint");
                DrawProp(ed, FindProperty("_CC_F0", p, false), "Clearcoat F0 (0.04 = dielectric)");
                GUILayout.Space(10);

                DrawProp(ed, FindProperty("_Film_Str", p, false), "Thin Film Iridescence");
                DrawProp(ed, FindProperty("_Film_Thick", p, false), "Film Thickness (100-2000nm)");
                DrawProp(ed, FindProperty("_Rim_Str", p, false), "Rim Light Strength");
                DrawProp(ed, FindProperty("_Rim_Power", p, false), "Rim Light Power");
                GUILayout.Space(10);

                DrawProp(ed, FindProperty("_SSS_Str", p, false), "Volumetric SSS Strength");
                DrawProp(ed, FindProperty("_SSS_Dist", p, false), "Terminator Bleed (Wrap)");
                DrawProp(ed, FindProperty("_SSS_Power", p, false), "Backscatter Focus");
                GUILayout.Space(10);

                EditorGUILayout.LabelField("Anisotropic Specular (Latex Stretch)", EditorStyles.boldLabel);
                DrawProp(ed, FindProperty("_Aniso", p, false), "Anisotropy (-1 = vertical, +1 = horizontal)");
                DrawProp(ed, FindProperty("_AnisoRot", p, false), "Anisotropy Rotation (deg)");
                GUILayout.Space(10);

                EditorGUILayout.LabelField("Transmission (Thin-Part Back-Light)", EditorStyles.boldLabel);
                DrawProp(ed, FindProperty("_Trans_Str", p, false), "Transmission Strength");
                DrawProp(ed, FindProperty("_Trans_Dist", p, false), "Absorption Distance");
                DrawProp(ed, FindProperty("_Trans_Power", p, false), "Back-Light Falloff");
                GUILayout.Space(10);

                EditorGUILayout.LabelField("Energy Conservation", EditorStyles.boldLabel);
                DrawProp(ed, FindProperty("_UseMultiScatter", p, false), "Multi-Scatter Compensation");
                EditorGUI.indentLevel--;
            }
            GUILayout.Space(10);

            // Wet - full soaked look plus run-off rivulets.
            EditorGUILayout.LabelField("Wet & Run-Off", EditorStyles.boldLabel);
            var _UseDr = FindProperty("_UseDrip", p, false);
            DrawProp(ed, _UseDr, "Enable Wet");
            if (_UseDr != null && _UseDr.floatValue > 0.5f)
            {
                EditorGUI.indentLevel++;
                DrawProp(ed, FindProperty("_DripMask", p, false), "Wet Mask (B&W)");
                DrawProp(ed, FindProperty("_DripMaskCh", p, false), "Wet Mask Channel");
                GUILayout.Space(4);
                EditorGUILayout.LabelField("Soaked Look (Just Out of the Pool)", EditorStyles.miniBoldLabel);
                DrawProp(ed, FindProperty("_Wet_Amount", p, false), "Wetness");
                DrawProp(ed, FindProperty("_Wet_Darken", p, false), "Darkening");
                DrawProp(ed, FindProperty("_Wet_Smoothness", p, false), "Wet Smoothness");
                DrawProp(ed, FindProperty("_Wet_Sheen", p, false), "Film Sheen");
                DrawProp(ed, FindProperty("_Wet_Flatten", p, false), "Normal Flatten");
                EditorGUILayout.HelpBox("Soaks the masked area like fresh out of the shower: darkens the latex (water absorption), drives reflections to a near-mirror water film, adds a dielectric Fresnel sheen, and flattens micro-detail. Film Sheen rides on the clearcoat, so keep the Polish layer enabled for the strongest wet highlight.", MessageType.None);
                GUILayout.Space(4);
                EditorGUILayout.LabelField("Run-Off Rivulets", EditorStyles.miniBoldLabel);
                DrawProp(ed, FindProperty("_Drip_Density", p, false), "Density (Columns)");
                DrawProp(ed, FindProperty("_Drip_Width", p, false), "Rivulet Thinness");
                DrawProp(ed, FindProperty("_Drip_Coverage", p, false), "Coverage");
                DrawProp(ed, FindProperty("_Drip_Speed", p, false), "Flow Speed");
                DrawProp(ed, FindProperty("_Drip_Strength", p, false), "Streak Strength");
                DrawProp(ed, FindProperty("_Drip_Normal", p, false), "Streak Normal Bump");
                EditorGUILayout.HelpBox("Animated vertical streaks of water running off, layered on top of the soak. Flows along UV vertical. Higher Rivulet Thinness = narrower streaks; set Streak Strength to 0 for a still, evenly-soaked look.", MessageType.None);
                var _Drip3D = FindProperty("_Drip3D_Strength", p, false);
                if (_Drip3D != null)
                {
                    GUILayout.Space(4);
                    EditorGUILayout.LabelField("Clear 3D Drips (Geometry, PC only)", EditorStyles.miniBoldLabel);
                    DrawProp(ed, _Drip3D, "Clear Drip Amount");
                    DrawProp(ed, FindProperty("_Drip3D_Scale", p, false), "Droplet Size");
                    DrawProp(ed, FindProperty("_Drip3D_Sheen", p, false), "Glassiness");
                    DrawProp(ed, FindProperty("_Drip3D_Fall", p, false), "Fall Distance");
                    EditorGUILayout.HelpBox("Real water droplets emitted by a geometry shader: they swell on downward-facing wet areas, form a neck, pinch off, then fall away as free geometry and dry out (fade). Tinted to the Clearcoat Tint. PC only - it uses a geometry stage, so it is not present on the SPS shader (or Quest). The Wet mask picks where drips form; Coverage and Flow Speed are shared with the rivulets. Droplet Size is roughly millimetres; Fall Distance is how far a drop falls before it fades.", MessageType.None);
                    GUILayout.Space(2);
                    EditorGUILayout.LabelField("Physics & Collision", EditorStyles.miniBoldLabel);
                    DrawProp(ed, FindProperty("_Drip_Sway", p, false), "Sway / Wobble");
                    DrawProp(ed, FindProperty("_Drip_BodyFollow", p, false), "Surface Slide (Body)");
                    DrawProp(ed, FindProperty("_Drip_FloorCollide", p, false), "Floor Splat");
                    EditorGUILayout.HelpBox("Sway adds surface-tension wobble and a breeze to falling drops (grows the further they fall). Surface Slide makes an attached drop run down ALONG the body before it detaches (a faked body collision). Floor Splat pins drops to the shared world floor and spreads them into a fading puddle; the floor height is the Goo \"Ground / Floor Height\" below (shared by both effects). Drops always fall under world gravity, so they already track movement; true inertial trailing would need a PhysBone, not a shader.", MessageType.None);
                }
                EditorGUI.indentLevel--;
            }
            GUILayout.Space(10);

            // Goo - melting/runny vertex sag.
            EditorGUILayout.LabelField("Goo (Melting Sag)", EditorStyles.boldLabel);
            var _UseGo = FindProperty("_UseGoo", p, false);
            DrawProp(ed, _UseGo, "Enable Goo");
            if (_UseGo != null && _UseGo.floatValue > 0.5f)
            {
                EditorGUI.indentLevel++;
                DrawProp(ed, FindProperty("_GooMask", p, false), "Goo Mask (B&W)");
                DrawProp(ed, FindProperty("_GooMaskCh", p, false), "Goo Mask Channel");
                DrawProp(ed, FindProperty("_Goo_Strength", p, false), "Melt Amount");
                DrawProp(ed, FindProperty("_Goo_Reach", p, false), "Stretch Distance");
                DrawProp(ed, FindProperty("_Goo_Variation", p, false), "Strand Variation");
                DrawProp(ed, FindProperty("_Goo_Noise", p, false), "Tendril Scale");
                DrawProp(ed, FindProperty("_Goo_Speed", p, false), "Flow Speed");
                DrawProp(ed, FindProperty("_Goo_Droop", p, false), "Underside Bias");
                GUILayout.Space(2);
                DrawProp(ed, FindProperty("_Goo_ToGround", p, false), "Melt To Ground");
                DrawProp(ed, FindProperty("_Goo_GroundY", p, false), "Ground / Floor Height (World Y)");
                EditorGUILayout.HelpBox("Gravity-aligned vertex melt that mimics runny/melting latex. Runs in the displacement stage and benefits from tessellation on the base shader (more verts = smoother strands). Melt Amount is the master intensity; Stretch Distance dramatically extends how far it sags (world units); Strand Variation adds procedural FBM noise so tendrils range from uniform (0) to wildly uneven (1). Melt To Ground pulls the goo down toward the world ground plane (set Ground / Floor Height to your world floor's Y, usually 0) so strands can reach the floor regardless of avatar height. Note: extreme stretch can be frustum-culled when the body is off-screen unless the mesh bounds (or an Anchor Override) are expanded.", MessageType.None);
                GUILayout.Space(4);
                EditorGUILayout.LabelField("Physics & Collision", EditorStyles.miniBoldLabel);
                DrawProp(ed, FindProperty("_Goo_Sway", p, false), "Sway Amount");
                DrawProp(ed, FindProperty("_Goo_SwaySpeed", p, false), "Sway Speed");
                DrawProp(ed, FindProperty("_Goo_BodyFollow", p, false), "Surface Follow (Body Collide)");
                DrawProp(ed, FindProperty("_Goo_FloorCollide", p, false), "Floor Collision");
                DrawProp(ed, FindProperty("_Goo_Pool", p, false), "Floor Pooling");
                EditorGUILayout.HelpBox("Sway gives the tendrils a gentle pendulum swing (Sway Speed sets the rate), staggered per strand so they don't move in lock-step. Surface Follow flows goo ALONG the body instead of sinking straight through it (a faked body collision; 1 = full surface flow). Floor Collision clamps the melt to the Ground / Floor Height above, and Floor Pooling spreads landed strands sideways into a puddle. The goo re-aligns to gravity and the body surface every frame, so it already tracks posing and locomotion; true inertial lag/overshoot and per-bone body collision are not possible in a shader - drive a PhysBone chain over the goo region for that.", MessageType.None);
                EditorGUI.indentLevel--;
            }
            GUILayout.Space(10);

            EditorGUILayout.LabelField("Outline (Backface Extrusion)", EditorStyles.boldLabel);
            var _UseOL = FindProperty("_UseOutline", p, false);
            DrawProp(ed, _UseOL, "Enable Outline");
            if (_UseOL != null && _UseOL.floatValue > 0.5f)
            {
                EditorGUI.indentLevel++;
                DrawProp(ed, FindProperty("_OutlineColor", p, false), "Outline Color");
                DrawProp(ed, FindProperty("_OutlineEmis", p, false), "Outline Emission (HDR)");
                DrawProp(ed, FindProperty("_OutlineWidth", p, false), "Outline Width");
                DrawProp(ed, FindProperty("_MaxOutlineWidth", p, false), "Max Width (Distance Clamp)");
                DrawProp(ed, FindProperty("_OutlineViewFudge", p, false), "View Fudge");
                DrawProp(ed, FindProperty("_OutlineMask", p, false), "Outline Mask");
                DrawProp(ed, FindProperty("_OutlineMaskCh", p, false), "Outline Mask Channel");
                EditorGUILayout.HelpBox("Outline renders as a Cull Front backface extrusion along the world normal. Width auto-scales with eye depth so the outline stays a constant visual thickness; Max Width clamps the extrusion at distance. Set Mask Channel to None for a uniform outline; pick R/G/B/A for a textured mask.", MessageType.None);
                GUILayout.Space(4);
                EditorGUILayout.LabelField("Outline AudioLink", EditorStyles.miniBoldLabel);
                DrawProp(ed, FindProperty("_AL_Band_Outline", p, false), "Outline AL Band");
                DrawProp(ed, FindProperty("_AL_Outline_Mod", p, false), "Outline AL Emission Boost");
                EditorGUI.indentLevel--;
            }
        }
        // INTEGRATION
        else if (ActiveTab == 3)
        {
            DrawProp(ed, FindProperty("_EmissionColor", p, false), "Emission Color");
            DrawProp(ed, FindProperty("_EmissionMap", p, false), "Emission Map");
            DrawProp(ed, FindProperty("_Emis_Exp", p, false), "Emission Exposure");
            GUILayout.Space(10);

            EditorGUILayout.LabelField("Secondary Emission Layer (Poiyomi-style stack)", EditorStyles.boldLabel);
            var _UseEmis2 = FindProperty("_UseEmission2", p, false);
            DrawProp(ed, _UseEmis2, "Enable Secondary Emission Layer");
            if (_UseEmis2 != null && _UseEmis2.floatValue > 0.5f)
            {
                EditorGUI.indentLevel++;
                DrawProp(ed, FindProperty("_EmissionColor2", p, false), "Emission Color 2");
                DrawProp(ed, FindProperty("_EmissionMap2", p, false), "Emission Map 2");
                DrawProp(ed, FindProperty("_Emis2_MaskCh", p, false), "Emission 2 Mask Channel");
                DrawProp(ed, FindProperty("_AL_Band_Emis2", p, false), "Emission 2 AL Band");
                DrawProp(ed, FindProperty("_AL_Emis2_Mod", p, false), "Emission 2 AL Amplitude");
                EditorGUI.indentLevel--;
            }
            GUILayout.Space(10);

            EditorGUILayout.LabelField("Multi-Region Color Mask (RGB Zones)", EditorStyles.boldLabel);
            var _UseRegion = FindProperty("_UseRegionMask", p, false);
            DrawProp(ed, _UseRegion, "Enable Region Mask");
            if (_UseRegion != null && _UseRegion.floatValue > 0.5f)
            {
                EditorGUI.indentLevel++;
                DrawProp(ed, FindProperty("_RegionMask", p, false), "Region Mask (R/G/B Zones)");
                EditorGUILayout.HelpBox("Each channel acts as an independent zone mask. The zone tint multiplies into the albedo (white = no change), and the emission boost adds zone-colored glow on top.", MessageType.None);
                DrawProp(ed, FindProperty("_Region_R_Tint", p, false), "Red Zone Tint");
                DrawProp(ed, FindProperty("_Region_R_Emis", p, false), "Red Zone Emission Boost");
                GUILayout.Space(2);
                DrawProp(ed, FindProperty("_Region_G_Tint", p, false), "Green Zone Tint");
                DrawProp(ed, FindProperty("_Region_G_Emis", p, false), "Green Zone Emission Boost");
                GUILayout.Space(2);
                DrawProp(ed, FindProperty("_Region_B_Tint", p, false), "Blue Zone Tint");
                DrawProp(ed, FindProperty("_Region_B_Emis", p, false), "Blue Zone Emission Boost");
                EditorGUI.indentLevel--;
            }
            GUILayout.Space(10);

            EditorGUILayout.LabelField("MatCap Layer 1", EditorStyles.boldLabel);
            DrawProp(ed, FindProperty("_MatCap", p, false), "MatCap 1 Texture");
            DrawProp(ed, FindProperty("_MatCapMask", p, false), "MatCap 1 Mask");
            DrawProp(ed, FindProperty("_MatCap_MaskCh", p, false), "MatCap 1 Mask Channel");
            DrawProp(ed, FindProperty("_MatCap_Tint", p, false), "MatCap 1 Tint");
            DrawProp(ed, FindProperty("_MatCap_Rot", p, false), "MatCap 1 Rotation");
            DrawProp(ed, FindProperty("_MatCap_Int", p, false), "MatCap 1 Intensity");
            DrawProp(ed, FindProperty("_MatCap_Lit", p, false), "MatCap Lighting Mix (Both Layers)");
            GUILayout.Space(6);

            EditorGUILayout.LabelField("MatCap Layer 2 (Multi-Zone Stack)", EditorStyles.boldLabel);
            var _UseMC2 = FindProperty("_UseMatCap2", p, false);
            DrawProp(ed, _UseMC2, "Enable MatCap 2 Layer");
            if (_UseMC2 != null && _UseMC2.floatValue > 0.5f)
            {
                EditorGUI.indentLevel++;
                DrawProp(ed, FindProperty("_MatCap2", p, false), "MatCap 2 Texture");
                DrawProp(ed, FindProperty("_MatCap2_Mask", p, false), "MatCap 2 Mask");
                DrawProp(ed, FindProperty("_MatCap2_MaskCh", p, false), "MatCap 2 Mask Channel");
                DrawProp(ed, FindProperty("_MatCap2_Tint", p, false), "MatCap 2 Tint");
                DrawProp(ed, FindProperty("_MatCap2_Rot", p, false), "MatCap 2 Rotation");
                DrawProp(ed, FindProperty("_MatCap2_Int", p, false), "MatCap 2 Intensity");
                DrawProp(ed, FindProperty("_MatCap2_Blend", p, false), "MatCap 2 Blend Mode");
                EditorGUILayout.HelpBox("Drop a red/blue/black region mask into both Layer 1 Mask and Layer 2 Mask, then set Layer 1's channel to R and Layer 2's to B - each color zone now shows a different matcap.", MessageType.None);
                EditorGUI.indentLevel--;
            }
            GUILayout.Space(10);

            EditorGUILayout.LabelField("Light Volumes (VRC)", EditorStyles.boldLabel);
            DrawProp(ed, FindProperty("_LV_Int", p, false), "Light Volumes Intensity");
            DrawProp(ed, FindProperty("_LV_Spec_Mix", p, false), "Base Specular Mix");
            DrawProp(ed, FindProperty("_LV_CC_Spec_Mix", p, false), "Clearcoat Specular Mix");
            DrawProp(ed, FindProperty("_LV_Spec_Dominant", p, false), "Specular: Dominant Mode (faster)");
            GUILayout.Space(4);
            DrawProp(ed, FindProperty("_LV_Bias", p, false), "Normal Bias (push along normal)");
            DrawProp(ed, FindProperty("_LV_PosOffset", p, false), "Position Offset (world space)");
            GUILayout.Space(4);
            DrawProp(ed, FindProperty("_LV_AdditiveOnly", p, false), "Additive-Only Mode (preserve probes)");
            DrawProp(ed, FindProperty("_LV_ProbeDering", p, false), "Deringed Probes Fallback (Bakery L1)");
            GUILayout.Space(8);

            EditorGUILayout.LabelField("LTCGI (Area Lights)", EditorStyles.boldLabel);
            DrawProp(ed, FindProperty("_LTCGI_Int", p, false), "LTCGI Intensity");
            DrawProp(ed, FindProperty("_LTCGI_Spec_Mix", p, false), "LTCGI Specular Mix");
            DrawProp(ed, FindProperty("_LTCGI_Diff_Mix", p, false), "LTCGI Diffuse Mix");
        }
        // AUDIOLINK / KINETIC
        else if (ActiveTab == 4)
        {
            var _AL = FindProperty("_UseAudioLink", p, false);
            DrawProp(ed, _AL, "Enable AudioLink");
            if (_AL != null && _AL.floatValue > 0.5f)
            {
                GUILayout.Space(15);
                EditorGUILayout.LabelField("Environment & Media", EditorStyles.boldLabel);
                DrawProp(ed, FindProperty("_AL_ColorMode", p, false), "Global Color Source");
                DrawProp(ed, FindProperty("_AL_Strip_Pos", p, false), "ColorChord Strip Position");
                DrawProp(ed, FindProperty("_UseMediaState", p, false), "Power Down on Pause/Stop");
                GUILayout.Space(10);

                EditorGUILayout.LabelField("Chronotensity FX", EditorStyles.boldLabel);
                var _ChronoFX = FindProperty("_UseChronoFX", p, false);
                DrawProp(ed, _ChronoFX, "Enable Chronotensity FX");
                if (_ChronoFX != null && _ChronoFX.floatValue > 0.5f)
                {
                    EditorGUI.indentLevel++;
                    DrawProp(ed, FindProperty("_AL_Chrono_Idx", p, false), "Chronotensity Index (0-7)");
                    EditorGUI.indentLevel--;
                }
                GUILayout.Space(10);

                EditorGUILayout.LabelField("God Tier Cybernetics (HUD Overlays)", EditorStyles.boldLabel);
                var _Cyber = FindProperty("_UseCyber", p, false);
                DrawProp(ed, _Cyber, "Enable HUD Overlays");
                if (_Cyber != null && _Cyber.floatValue > 0.5f)
                {
                    EditorGUI.indentLevel++;
                    DrawProp(ed, FindProperty("_CyberMask", p, false), "B&W Window Mask");
                    DrawProp(ed, FindProperty("_Cyber_Hover", p, false), "Hover Height (Float Off Body)");
                    DrawProp(ed, FindProperty("_Cyber_Hover_Bob", p, false), "Hover Bob (Subtle Drift)");
                    GUILayout.Space(4);

                    var _UVU = FindProperty("_UseCyberVU", p, false);
                    DrawProp(ed, _UVU, "Enable VU Meter Segment");
                    if (_UVU != null && _UVU.floatValue > 0.5f)
                    {
                        DrawProp(ed, FindProperty("_Cyber_VU_Style", p, false), "VU Meter Style (Console / Bar)");
                        DrawProp(ed, FindProperty("_Cyber_VU_Str", p, false), "VU Meter Intensity");
                        DrawProp(ed, FindProperty("_Cyber_VU_Transform", p, false), "VU Meter Placement (X,Y,Scl,Rot)");
                    }
                    GUILayout.Space(4);

                    var _UCC = FindProperty("_UseCyberCC", p, false);
                    DrawProp(ed, _UCC, "Enable Spectrum Segment");
                    if (_UCC != null && _UCC.floatValue > 0.5f)
                    {
                        DrawProp(ed, FindProperty("_Cyber_CC_Band", p, false), "Spectrum Primary Band");
                        DrawProp(ed, FindProperty("_Cyber_CC_Str", p, false), "Spectrum Intensity");
                        DrawProp(ed, FindProperty("_Cyber_CC_Density", p, false), "Spectrum Bar Count");
                        DrawProp(ed, FindProperty("_Cyber_CC_Transform", p, false), "Spectrum Placement (X,Y,Scl,Rot)");
                    }
                    GUILayout.Space(4);

                    var _UWave = FindProperty("_UseCyberWave", p, false);
                    DrawProp(ed, _UWave, "Enable Waveform Line");
                    if (_UWave != null && _UWave.floatValue > 0.5f)
                    {
                        DrawProp(ed, FindProperty("_Cyber_Wave_Band", p, false), "Waveform Band");
                        DrawProp(ed, FindProperty("_Cyber_Wave_Str", p, false), "Waveform Intensity");
                        DrawProp(ed, FindProperty("_Cyber_Wave_Transform", p, false), "Waveform Placement (X,Y,Scl,Rot)");
                    }
                    GUILayout.Space(4);

                    var _UDMX = FindProperty("_UseCyberDMX", p, false);
                    DrawProp(ed, _UDMX, "Enable DMX Grid Segment");
                    if (_UDMX != null && _UDMX.floatValue > 0.5f)
                    {
                        DrawProp(ed, FindProperty("_Cyber_DMX_Band", p, false), "DMX Grid Band");
                        DrawProp(ed, FindProperty("_Cyber_DMX_Str", p, false), "DMX Grid Intensity");
                        DrawProp(ed, FindProperty("_Cyber_DMX_Transform", p, false), "DMX Placement (X,Y,Scl,Rot)");
                    }
                    GUILayout.Space(4);

                    var _UAuto = FindProperty("_UseCyberAuto", p, false);
                    DrawProp(ed, _UAuto, "Enable Autocorrelator Ring");
                    if (_UAuto != null && _UAuto.floatValue > 0.5f)
                    {
                        DrawProp(ed, FindProperty("_Cyber_Auto_Band", p, false), "Autocorrelator Band");
                        DrawProp(ed, FindProperty("_Cyber_AutoCorr_Str", p, false), "Autocorrelator Intensity");
                        DrawProp(ed, FindProperty("_Cyber_Auto_Transform", p, false), "Autocorrelator Placement (X,Y,Scl,Rot)");

                        EditorGUILayout.LabelField("Ring Effects (each toggle + its own reaction band)", EditorStyles.miniBoldLabel);
                        DrawProp(ed, FindProperty("_Cyber_Auto_Shimmer", p, false), "Shimmer");
                        DrawProp(ed, FindProperty("_Cyber_Auto_Shimmer_Band", p, false), "Shimmer Band");
                        DrawProp(ed, FindProperty("_Cyber_Auto_Pop", p, false), "Pop");
                        DrawProp(ed, FindProperty("_Cyber_Auto_Pop_Band", p, false), "Pop Band");
                        DrawProp(ed, FindProperty("_Cyber_Auto_Sizzle", p, false), "Sizzle");
                        DrawProp(ed, FindProperty("_Cyber_Auto_Sizzle_Band", p, false), "Sizzle Band");
                        DrawProp(ed, FindProperty("_Cyber_Auto_Electrify", p, false), "Electrify");
                        DrawProp(ed, FindProperty("_Cyber_Auto_Electrify_Band", p, false), "Electrify Band");
                    }
                    EditorGUI.indentLevel--;
                }
                GUILayout.Space(10);

                EditorGUILayout.LabelField("Kinetic Vertex Engine (SM5 Displacement & Shards)", EditorStyles.boldLabel);
                var _UseVtx = FindProperty("_UseVtxKinetic", p, false);
                DrawProp(ed, _UseVtx, "Enable Vertex Displacement & Shards");
                if (_UseVtx != null && _UseVtx.floatValue > 0.5f)
                {
                    EditorGUI.indentLevel++;
                    DrawProp(ed, FindProperty("_Vtx_Pump_Band", p, false), "Vertex Pump Band");
                    DrawProp(ed, FindProperty("_Vtx_Pump_Str", p, false), "Normal Inflate Distance");
                    GUILayout.Space(4);
                    DrawProp(ed, FindProperty("_Vtx_Fracture_Band", p, false), "Geometry Fracture Band");
                    DrawProp(ed, FindProperty("_Vtx_Fracture_Amount", p, false), "Fracture Amount (Hold/Animate)");
                    DrawProp(ed, FindProperty("_Vtx_Fracture_Dist", p, false), "Shard Hover Distance");
                    DrawProp(ed, FindProperty("_Vtx_Fracture_Spin", p, false), "Shard Tumble");
                    DrawProp(ed, FindProperty("_Vtx_Fracture_Str", p, false), "Shard AudioLink Jitter");
                    DrawProp(ed, FindProperty("_Vtx_Fracture_Spiral", p, false), "Shard Spiral");
                    DrawProp(ed, FindProperty("_Vtx_Fracture_Lift", p, false), "Shard Lift (Up/Down, Animate)");
                    DrawProp(ed, FindProperty("_Vtx_Fracture_Float", p, false), "Shard Float Drift");
                    DrawProp(ed, FindProperty("_Vtx_Fracture_Trail", p, false), "Shard Trail Length");
                    GUILayout.Space(4);
                    DrawProp(ed, FindProperty("_Shard_ColorMod", p, false), "Shard Hue Shift");
                    DrawProp(ed, FindProperty("_Shard_ColorMod_Speed", p, false), "Shard Hue Cycle Speed");
                    var _UShardCC = FindProperty("_UseShardCC", p, false);
                    DrawProp(ed, _UShardCC, "Shard AudioLink ColorChord");
                    if (_UShardCC != null && _UShardCC.floatValue > 0.5f)
                        DrawProp(ed, FindProperty("_Shard_CC_Str", p, false), "Shard ColorChord Blend");
                    GUILayout.Space(4);
                    DrawProp(ed, FindProperty("_Vtx_AutoCorr_Str", p, false), "Spherical Autocorrelator Ripple");
                    EditorGUI.indentLevel--;
                }
                GUILayout.Space(10);

                EditorGUILayout.LabelField("Kinetic UV Engine (Surface Maps)", EditorStyles.boldLabel);
                var _UseVortex = FindProperty("_UseALVortex", p, false);
                DrawProp(ed, _UseVortex, "Enable Vortex Twist");
                if (_UseVortex != null && _UseVortex.floatValue > 0.5f)
                {
                    EditorGUI.indentLevel++;
                    DrawProp(ed, FindProperty("_AL_Vortex_Band", p, false), "Vortex Band");
                    DrawProp(ed, FindProperty("_AL_Vortex_Str", p, false), "Vortex Twist Strength");
                    DrawProp(ed, FindProperty("_AL_Vortex_UV", p, false), "Vortex Transform (X,Y,Scl,Rot)");
                    EditorGUI.indentLevel--;
                }
                GUILayout.Space(2);

                var _UsePump = FindProperty("_UseALPump", p, false);
                DrawProp(ed, _UsePump, "Enable UV Bass Pump");
                if (_UsePump != null && _UsePump.floatValue > 0.5f)
                {
                    EditorGUI.indentLevel++;
                    DrawProp(ed, FindProperty("_AL_Pump_Band", p, false), "UV Pump Band");
                    DrawProp(ed, FindProperty("_AL_Pump_Str", p, false), "UV Pump Bounce");
                    DrawProp(ed, FindProperty("_AL_Pump_UV", p, false), "Pump Transform (X,Y,Scl,Rot)");
                    EditorGUI.indentLevel--;
                }
                GUILayout.Space(2);

                var _UseFracture = FindProperty("_UseALFracture", p, false);
                DrawProp(ed, _UseFracture, "Enable UV Fracture Shard");
                if (_UseFracture != null && _UseFracture.floatValue > 0.5f)
                {
                    EditorGUI.indentLevel++;
                    DrawProp(ed, FindProperty("_AL_Fracture_Band", p, false), "Fracture Band");
                    DrawProp(ed, FindProperty("_AL_Fracture_Str", p, false), "Fracture Strength");
                    DrawProp(ed, FindProperty("_AL_Fracture_UV", p, false), "Fracture Transform (X,Y,Scl,Rot)");
                    EditorGUI.indentLevel--;
                }
                GUILayout.Space(10);

                EditorGUILayout.LabelField("Global Material Modulations", EditorStyles.boldLabel);
                DrawProp(ed, FindProperty("_AL_Band_Emission", p, false), "Emission Reaction Band");
                DrawProp(ed, FindProperty("_AL_Emis_Mod", p, false), "Emission Amplitude");
                DrawProp(ed, FindProperty("_AL_Col_Blend", p, false), "Audio Color Blend");
                DrawProp(ed, FindProperty("_AL_Waveform_Mod", p, false), "Surface Waveform Ripple");
                DrawProp(ed, FindProperty("_AL_AutoCorr_Mod", p, false), "Surface Autocorrelator Ripple");
                GUILayout.Space(4);
                DrawProp(ed, FindProperty("_AL_DFT_Note", p, false), "DFT Note (0-11)");
                DrawProp(ed, FindProperty("_AL_DFT_Mod", p, false), "DFT Note Emission Amount");
                GUILayout.Space(10);

                EditorGUILayout.LabelField("Audio Scanlines", EditorStyles.boldLabel);
                DrawProp(ed, FindProperty("_AL_Band_Scanlines", p, false), "Scanline Reaction Band");
                DrawProp(ed, FindProperty("_AL_Scanlines", p, false), "Scanline Visibility Blend");
                DrawProp(ed, FindProperty("_AL_Scan_Density", p, false), "Scanline Density");
                DrawProp(ed, FindProperty("_AL_Scan_Speed", p, false), "Base Scan Speed");
                DrawProp(ed, FindProperty("_AL_Scan_React", p, false), "Scanline Chronotensity Reaction");
                GUILayout.Space(10);

                EditorGUILayout.LabelField("Physical Lobe Thump", EditorStyles.boldLabel);
                DrawProp(ed, FindProperty("_AL_Band_Film", p, false), "Film Expansion Band");
                DrawProp(ed, FindProperty("_AL_Film_Mod", p, false), "Thin Film Expansion");
                DrawProp(ed, FindProperty("_AL_Band_Parallax", p, false), "Parallax Thump Band");
                DrawProp(ed, FindProperty("_AL_Paralx_Mod", p, false), "Parallax Thump");
                DrawProp(ed, FindProperty("_AL_Band_Shatter", p, false), "Clearcoat Shatter Band");
                DrawProp(ed, FindProperty("_AL_CC_Shatter", p, false), "Clearcoat Shatter");
                DrawProp(ed, FindProperty("_AL_Band_Glitch", p, false), "Digital Tear Band");
                DrawProp(ed, FindProperty("_AL_Glitch_Mod", p, false), "Digital Domain Tear");
            }
        }
        // STAGE / VRSL
        else if (ActiveTab == 5)
        {
            var _VRSL = FindProperty("_UseVRSL", p, false);
            DrawProp(ed, _VRSL, "Enable VRSL Stage Hijack Protocol");
            if (_VRSL != null && _VRSL.floatValue > 0.5f)
            {
                GUILayout.Space(15);
                EditorGUILayout.LabelField("DMX Universe Routing", EditorStyles.boldLabel);
                DrawProp(ed, FindProperty("_DMX_Channel", p, false), "DMX Base Channel (Sector ID)");
                DrawProp(ed, FindProperty("_VRSL_Intensity", p, false), "Stage Hijack Override Power");
                GUILayout.Space(10);
                EditorGUILayout.LabelField("Kinetic Geo-Warping Engine", EditorStyles.boldLabel);
                DrawProp(ed, FindProperty("_VRSL_Geo_Warp", p, false), "Pan/Tilt Displacement Scale");
                GUILayout.Space(10);
                EditorGUILayout.LabelField("Color Hijack", EditorStyles.boldLabel);
                DrawProp(ed, FindProperty("_VRSL_Color_Hijack", p, false), "DMX RGB Color Override");
                GUILayout.Space(15);
                EditorGUILayout.HelpBox("VRSL Stage Hijack links this material directly to world-space DMX buffers. When active light intensity is detected from the stage, it will override native AudioLink color arrays and physically warp tessellated vertex positions to match stage configurations in real-time.", MessageType.Info);
            }
        }

        GUILayout.Space(8);

        // Per-tab "Reset to Defaults" - visible companion to the right-click menu entry.
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();

            GUIStyle resetBtn = new GUIStyle(EditorStyles.miniButton)
            {
                fontSize = 10,
                fontStyle = FontStyle.Bold,
                normal   = { textColor = new Color(1f, 0.55f, 0.75f) },
                hover    = { textColor = pink }
            };

            if (GUILayout.Button(new GUIContent(
                    $"↺  Reset {tabNames[ActiveTab]} to Defaults",
                    $"Restore all {tabNames[ActiveTab]} tab properties to the shader's declared defaults.\nUndo (Ctrl+Z) reverts."),
                resetBtn, GUILayout.Height(20), GUILayout.MinWidth(180)))
            {
                if (EditorUtility.DisplayDialog(
                    "Reset Tab to Defaults",
                    $"Reset all {tabNames[ActiveTab]} properties to shader defaults?\n\nThis affects {ed.targets.Length} material(s). Use Undo (Ctrl+Z) to revert.",
                    "Reset", "Cancel"))
                {
                    PerformReset(ed, p, ActiveTab);
                }
            }
        }

        EditorGUILayout.EndVertical();
        GUILayout.Space(10);

        // Render queue / instancing / double sided GI
        ed.RenderQueueField();
        ed.EnableInstancingField();
        ed.DoubleSidedGIField();

        // Ensure keywords are synced for all selected materials at end of GUI pass
        UpdateKeywordsForTargets(ed.targets);
    }
}

// BUILD-TIME KEYWORD CLEANUP - syncs material keywords to property toggles before variant stripping so stale keywords don't preserve dead variants.
public class VixenWearBuildPreprocessor : IPreprocessBuildWithReport
{
    public const string SHADER_NAME = "VixenWear/Latex Ultra";
    public const string SHADER_NAME_SPS = "VixenWear/Latex Ultra SPS";

    // Both variants share the same property layout and editor; the SPS variant drops tessellation so VRCFury's SPS patcher can wrap the vertex function without hitting a struct type mismatch in tessEdge.
    public static bool IsVixenWearShader(Shader s)
    {
        return s != null && (s.name == SHADER_NAME || s.name == SHADER_NAME_SPS);
    }

    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        CleanAllMaterials(verbose: true, saveToDisk: true);
    }

    [MenuItem("VixenTools/VixenWear/Clean Latex Material Keywords")]
    public static void CleanFromMenu()
    {
        CleanAllMaterials(verbose: true, saveToDisk: true);
    }

    // Promotes the current Hierarchy GameObject selection to its underlying VixenWear material assets - works around Unity's "-" inspector when renderers reference different .mat files, by walking children (incl. disabled wardrobe toggles), gathering unique materials, and swapping Selection.objects.
    [MenuItem("VixenTools/VixenWear/Edit Materials From Selection %#m")]
    public static void EditMaterialsFromSelection()
    {
        GameObject[] selectedGOs = Selection.gameObjects;
        if (selectedGOs == null || selectedGOs.Length == 0)
        {
            EditorUtility.DisplayDialog(
                "Edit VixenWear Materials",
                "Select one or more GameObjects in the Hierarchy first, then run this command to multi-edit their VixenWear materials.",
                "OK");
            return;
        }

        HashSet<Material> seen = new HashSet<Material>();
        List<UnityEngine.Object> mats = new List<UnityEngine.Object>();
        int rendererCount = 0;
        int skippedNonVixen = 0;

        foreach (GameObject go in selectedGOs)
        {
            if (go == null) continue;
            // includeInactive=true picks up wardrobe layers that are toggled off (very common for VRC clothing).
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in renderers)
            {
                if (r == null) continue;
                rendererCount++;
                foreach (Material mat in r.sharedMaterials)
                {
                    if (mat == null || mat.shader == null) continue;
                    if (!IsVixenWearShader(mat.shader)) { skippedNonVixen++; continue; }
                    if (seen.Add(mat)) mats.Add(mat);
                }
            }
        }

        if (mats.Count == 0)
        {
            EditorUtility.DisplayDialog(
                "Edit VixenWear Materials",
                $"Scanned {rendererCount} renderer(s) under {selectedGOs.Length} GameObject(s) and found no VixenWear materials.\n\n" +
                (skippedNonVixen > 0 ? $"({skippedNonVixen} non-VixenWear material slot(s) skipped.)" : "Make sure the renderers reference materials using the VixenWear/Latex Ultra or VixenWear/Latex Ultra SPS shader."),
                "OK");
            return;
        }

        Selection.objects = mats.ToArray();
        Debug.Log($"[Vixen Wear] Multi-edit ready: selected {mats.Count} unique VixenWear material(s) from {selectedGOs.Length} GameObject(s) ({rendererCount} renderer(s) scanned). Edits in the Inspector now apply to all of them.");
    }

    // Greys out the menu item when no GameObjects are selected so the affordance matches the actual capability.
    [MenuItem("VixenTools/VixenWear/Edit Materials From Selection %#m", true)]
    public static bool EditMaterialsFromSelection_Validate()
    {
        return Selection.gameObjects != null && Selection.gameObjects.Length > 0;
    }

    [MenuItem("VixenTools/VixenWear/Disable Media-State Gate On All Materials")]
    public static void DisableMediaStateOnAll()
    {
        int touched = 0;
        string[] guids = AssetDatabase.FindAssets("t:Material");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null || mat.shader == null) continue;
            if (!IsVixenWearShader(mat.shader)) continue;
            if (!mat.HasProperty("_UseMediaState")) continue;
            if (mat.GetFloat("_UseMediaState") <= 0.5f) continue;

            mat.SetFloat("_UseMediaState", 0f);
            mat.DisableKeyword("AL_MEDIA_STATE");
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssetIfDirty(mat);
            touched++;
        }
        Debug.Log($"[Vixen Wear] Disabled _UseMediaState on {touched} material(s). AudioLink should now run regardless of VRC video player state.");
    }

    public static void CleanAllMaterials(bool verbose, bool saveToDisk)
    {
        int touched = 0, scanned = 0;
        string[] guids = AssetDatabase.FindAssets("t:Material");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null || mat.shader == null) continue;
            if (!IsVixenWearShader(mat.shader)) continue;

            scanned++;

            string[] before = (string[])mat.shaderKeywords.Clone();
            MaterialGlobalIlluminationFlags giBefore = mat.globalIlluminationFlags;
            VixenWearEditor.SyncKeywords(mat);
            string[] after = mat.shaderKeywords;
            MaterialGlobalIlluminationFlags giAfter = mat.globalIlluminationFlags;

            // Persist either change - GI flag drift alone (the EmissiveIsBlack clear) still needs to hit disk so Unity's build pipeline doesn't strip _EmissionColor from VRCFury swap-target materials whose keywords were already in sync.
            if (!KeywordsEqual(before, after) || giBefore != giAfter)
            {
                if (saveToDisk)
                {
                    EditorUtility.SetDirty(mat);
                    AssetDatabase.SaveAssetIfDirty(mat);
                }
                touched++;
            }
        }

        if (verbose)
            Debug.Log($"[Vixen Wear] Keyword sync: {touched} material(s) updated, {scanned} scanned.");
    }

    private static bool KeywordsEqual(string[] a, string[] b)
    {
        if (a.Length != b.Length) return false;
        Array.Sort(a);
        Array.Sort(b);
        for (int i = 0; i < a.Length; i++)
            if (a[i] != b[i]) return false;
        return true;
    }
}

// PLAY-MODE KEYWORD SYNC - force keyword state on every VixenWear material before play so a stale toggle doesn't no-op on first frame.
[InitializeOnLoad]
public static class VixenWearPlayModeSync
{
    static VixenWearPlayModeSync()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange change)
    {
        // Sync just before we leave edit mode so the play-mode renderer sees current state.
        if (change == PlayModeStateChange.ExitingEditMode)
        {
            // In-memory sync only - don't dirty assets while transitioning play mode.
            VixenWearBuildPreprocessor.CleanAllMaterials(verbose: false, saveToDisk: false);
        }
    }
}

// VARIANT STRIPPER - drops unused variants in 3 layers: (1) managed feature kw not used by any material, (2) Deferred/Meta/MotionVectors passes, (3) built-in lightmap/LPPV keywords leaking past the pragma.
public class VixenWearVariantStripper : IPreprocessShaders
{
    public int callbackOrder => 100;

    // Lazy-cached set of keywords still enabled on any VixenWear material.
    private static HashSet<string> _liveKeywords;
    internal static int s_stripped;
    internal static int s_kept;

    // Managed shader_feature_local kws - drop variants where no material has them on (AL_ENABLE/CYBER_ENABLE removed: those paths are runtime-branched for VRCFury; alpha workflow kws _ALPHATEST_ON/_ALPHABLEND_ON/_ALPHAPREMULTIPLY_ON are also stripped per-mode).
    private static readonly string[] s_managedKeywords =
    {
        "VRSL_ENABLE", "LTCGI_ENABLE", "LIGHTVOLUMES_ENABLE", "_DETAIL_NORMAL",
        "_ALPHATEST_ON", "_ALPHABLEND_ON", "_ALPHAPREMULTIPLY_ON"
    };

    // Built-in keywords avatar clothing never uses. Belt-and-suspenders against Unity versions emitting variants the pragma already disabled.
    private static readonly string[] s_deadBuiltinKeywords =
    {
        "LIGHTMAP_ON",
        "DIRLIGHTMAP_COMBINED",
        "DYNAMICLIGHTMAP_ON",
        "LIGHTMAP_SHADOW_MIXING",
        "SHADOWS_SHADOWMASK",
        "LIGHTPROBE_SH",         // only matters in LPPV context, which we don't support
        "LOD_FADE_CROSSFADE"     // avatar skinned meshes don't sit in LOD groups
    };

    public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
    {
        if (!VixenWearBuildPreprocessor.IsVixenWearShader(shader)) return;

        if (_liveKeywords == null) _liveKeywords = CollectLiveKeywords();

        int before = data.Count;

        // Layer 2: drop Deferred/Meta/MotionVectors passes (Unity 2022.3.x has emitted them even with `nometa` - defensive strip).
        if (snippet.passType == PassType.Deferred ||
            snippet.passType == PassType.Meta ||
            snippet.passType == PassType.MotionVectors)
        {
            s_stripped += before;
            data.Clear();
            return;
        }

        // Layers 1 + 3: per-variant keyword checks.
        for (int i = data.Count - 1; i >= 0; i--)
        {
            var variant = data[i].shaderKeywordSet;
            bool drop = false;

            // Managed feature keywords: drop if no material has the keyword on.
            foreach (string kw in s_managedKeywords)
            {
                if (variant.IsEnabled(new ShaderKeyword(shader, kw)) && !_liveKeywords.Contains(kw))
                {
                    drop = true;
                    break;
                }
            }

            // Built-in dead keywords: drop any variant that has one of them set.
            if (!drop)
            {
                foreach (string kw in s_deadBuiltinKeywords)
                {
                    if (variant.IsEnabled(new ShaderKeyword(kw)))
                    {
                        drop = true;
                        break;
                    }
                }
            }

            if (drop) data.RemoveAt(i);
        }

        s_stripped += (before - data.Count);
        s_kept += data.Count;
    }

    private static HashSet<string> CollectLiveKeywords()
    {
        HashSet<string> live = new HashSet<string>(StringComparer.Ordinal);
        string[] guids = AssetDatabase.FindAssets("t:Material");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null || mat.shader == null) continue;
            if (!VixenWearBuildPreprocessor.IsVixenWearShader(mat.shader)) continue;
            foreach (string kw in mat.shaderKeywords) live.Add(kw);
        }
        return live;
    }

    [InitializeOnLoadMethod]
    private static void ClearCache()
    {
        _liveKeywords = null;
        s_stripped = 0;
        s_kept = 0;
    }
}

// Post-build report so users can see the strip count and verify the speedup.
public class VixenWearVariantStripReporter : IPostprocessBuildWithReport
{
    public int callbackOrder => 1000;
    public void OnPostprocessBuild(BuildReport report)
    {
        int s = VixenWearVariantStripper.s_stripped;
        int k = VixenWearVariantStripper.s_kept;
        if (s + k > 0)
            Debug.Log($"[Vixen Wear] Variant strip: kept {k}, stripped {s} (total {s + k}).");
        VixenWearVariantStripper.s_stripped = 0;
        VixenWearVariantStripper.s_kept = 0;
    }
}
#endif
