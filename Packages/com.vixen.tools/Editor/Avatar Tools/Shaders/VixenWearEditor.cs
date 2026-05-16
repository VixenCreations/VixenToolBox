// VIXEN WEAR — NATIVE SHADERGUI INSPECTOR

using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

public class VectorLabelDrawer : MaterialPropertyDrawer
{
    private readonly string[] labels = new string[4];
    private readonly bool[] show = new bool[4];
    private readonly int visibleCount = 0;

    public VectorLabelDrawer(string x, string y = "", string z = "", string w = "")
    {
        labels[0] = Sanitize(x); labels[1] = Sanitize(y); labels[2] = Sanitize(z); labels[3] = Sanitize(w);
        for (int i = 0; i < 4; i++) {
            if (!string.IsNullOrEmpty(labels[i]) && labels[i] != "Unused" && labels[i] != "NONE") {
                show[i] = true; visibleCount++;
            }
        }
    }

    private string Sanitize(string s) => string.IsNullOrWhiteSpace(s) ? "" : s.Trim().Replace("_", " ");

    public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor) {
        if (prop.type != MaterialProperty.PropType.Vector || visibleCount == 0) return EditorGUIUtility.singleLineHeight;
        return (EditorGUIUtility.singleLineHeight * 3f) + 6f;
    }

    public override void OnGUI(Rect pos, MaterialProperty prop, GUIContent label, MaterialEditor editor) {
        if (prop.type != MaterialProperty.PropType.Vector) return;
        if (visibleCount == 0) return;

        Rect mainRect = new Rect(pos.x, pos.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(mainRect, label);

        float spacing = 4f; float startX = pos.x + EditorGUIUtility.labelWidth;
        float availableWidth = pos.width - EditorGUIUtility.labelWidth; float slotWidth = (availableWidth - (spacing * (visibleCount - 1))) / visibleCount;

        GUIStyle miniStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel) { normal = { textColor = new Color(0.7f, 0.7f, 0.8f) }, fontSize = 11, clipping = TextClipping.Clip, alignment = TextAnchor.LowerCenter };

        int drawnCount = 0; float currentY = pos.y + EditorGUIUtility.singleLineHeight + 2f; Vector4 v = prop.vectorValue;

        for (int i = 0; i < 4; i++) {
            if (!show[i]) continue;
            float currentX = startX + (slotWidth + spacing) * drawnCount;
            Rect labelRect = new Rect(currentX, currentY, slotWidth, EditorGUIUtility.singleLineHeight);
            Rect fieldRect = new Rect(currentX, currentY + EditorGUIUtility.singleLineHeight + 2f, slotWidth, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(labelRect, labels[i], miniStyle);
            float oldVal = (i == 0) ? v.x : (i == 1) ? v.y : (i == 2) ? v.z : v.w;
            float newVal = EditorGUI.FloatField(fieldRect, GUIContent.none, oldVal);
            if (i == 0) v.x = newVal; else if (i == 1) v.y = newVal; else if (i == 2) v.z = newVal; else v.w = newVal;
            drawnCount++;
        }
        prop.vectorValue = v;
    }
}

public class VixenALBandDrawer : MaterialPropertyDrawer {
    private readonly string tip; public VixenALBandDrawer(string t) => tip = t.Replace("_", " ").Replace("DOT", ".");
    public override void OnGUI(Rect r, MaterialProperty p, GUIContent l, MaterialEditor e) {
        GUIContent final = new GUIContent(l.text, tip); GUIContent[] names = { new GUIContent("Bass (0)"), new GUIContent("Low Mid (1)"), new GUIContent("High Mid (2)"), new GUIContent("Treble (3)") };
        int[] vals = { 0, 1, 2, 3 }; p.floatValue = EditorGUI.IntPopup(r, final, (int)p.floatValue, names, vals);
    }
}

public class VixenWearEditor : ShaderGUI
{
    public static int ActiveTab { get => EditorPrefs.GetInt("VixenWear_ActiveTab", 0); set => EditorPrefs.SetInt("VixenWear_ActiveTab", value); }

    private readonly Color cyan = new Color(0f, 0.898f, 1f); private readonly Color pink = new Color(1f, 0f, 0.667f);
    private readonly Color bgBanner = new Color(0.047f, 0.024f, 0.071f); private readonly Color bgTabIdle = new Color(0.12f, 0.12f, 0.12f); private readonly Color bgTabActive = new Color(0.18f, 0.18f, 0.18f);

    private readonly string[] tabNames = { "BASE", "SURFACE", "POLISH", "INTEGRATION", "AUDIOLINK", "STAGE" };
    private readonly string[] tabDesc = { "Base color, albedo, alpha cutoff, and UV animation.", "PBR maps, normals, and micro detail shaping.", "Clearcoat, thin film, rim, and volumetric SSS.", "Emission, MatCap, light volumes, and LTCGI illumination.", "AudioLink setup, kinetic engines, and diagnostic overlays.", "VRSL integration, DMX sector mapping, and kinetic geo-warping." };

    private readonly string[][] tabProps = new string[][] {
        new string[] { "_Color", "_CutOff", "_MainTex", "_MinBrightness", "_UV_Rot", "_SpeedX", "_SpeedY" },
        new string[] { "_MetallicGlossMap", "_BumpMap", "_AO_Str", "_Spec_Occ", "_Shad_Hard", "_Norm_Str", "_Parallax", "_Disp_Str", "_Tess_Edge", "_UseDetailNormal", "_DetailNormalMap", "_Det_Strength", "_Det_UV_Tiling" },
        new string[] { "_CC_Strength", "_CC_Smoothness", "_CC_Spec_AA", "_CC_Flat", "_Film_Str", "_Film_Thick", "_Rim_Str", "_Rim_Power", "_SSS_Str", "_SSS_Dist", "_SSS_Power" },
        new string[] { "_EmissionColor", "_EmissionMap", "_Emis_Exp", "_MatCap", "_MatCapMask", "_MatCap_Rot", "_MatCap_Int", "_MatCap_Lit", "_UseLightVolumes", "_LV_Int", "_UseLTCGI", "_LTCGI_Int" },
        new string[] { "_UseAudioLink", "_AL_ColorMode", "_UseMediaState", "_UseCyber", "_CyberMask", "_UseCyberVU", "_Cyber_VU_Band", "_Cyber_VU_Str", "_Cyber_VU_Transform", "_UseCyberCC", "_Cyber_CC_Band", "_Cyber_CC_Str", "_Cyber_CC_Transform", "_UseCyberWave", "_Cyber_Wave_Str", "_Cyber_Wave_Transform", "_UseCyberDMX", "_Cyber_DMX_Str", "_Cyber_DMX_Transform", "_Cyber_AutoCorr_Str", "_UseVtxKinetic", "_Vtx_Pump_Band", "_Vtx_Pump_Str", "_Vtx_Fracture_Band", "_Vtx_Fracture_Str", "_UseALVortex", "_AL_Vortex_Band", "_AL_Vortex_Str", "_AL_Vortex_UV", "_UseALPump", "_AL_Pump_Band", "_AL_Pump_Str", "_AL_Pump_UV", "_UseALFracture", "_AL_Fracture_Band", "_AL_Fracture_Str", "_AL_Fracture_UV", "_AL_Band_Emission", "_AL_Emis_Mod", "_AL_Col_Blend", "_AL_Waveform_Mod", "_AL_Band_Scanlines", "_AL_Scanlines", "_AL_Scan_Density", "_AL_Scan_Speed", "_AL_Scan_React", "_AL_Band_Film", "_AL_Film_Mod", "_AL_Band_Parallax", "_AL_Paralx_Mod", "_AL_Band_Shatter", "_AL_CC_Shatter", "_AL_Band_Glitch", "_AL_Glitch_Mod" },
        new string[] { "_UseVRSL", "_DMX_Channel", "_VRSL_Intensity", "_VRSL_Geo_Warp" }
    };

    public class TabClipboard {
        public int TabIndex; public Dictionary<string, float> Floats = new Dictionary<string, float>(); public Dictionary<string, Color> Colors = new Dictionary<string, Color>(); public Dictionary<string, Vector4> Vectors = new Dictionary<string, Vector4>(); public Dictionary<string, Texture> Textures = new Dictionary<string, Texture>(); public Dictionary<string, Vector2> TexOffsets = new Dictionary<string, Vector2>(); public Dictionary<string, Vector2> TexScales = new Dictionary<string, Vector2>();
    }
    private static TabClipboard _clipboard = null;

    private GUIStyle Card => new GUIStyle("HelpBox") { padding = new RectOffset(10, 10, 10, 10), margin = new RectOffset(4, 4, 8, 8) };

    private void DrawProp(MaterialEditor ed, MaterialProperty prop, string label) {
        if (prop == null) return; float height = ed.GetPropertyHeight(prop, label); Rect r = EditorGUILayout.GetControlRect(true, height); Rect labelRect = new Rect(r.x, r.y, EditorGUIUtility.labelWidth, r.height);
        if (Event.current != null && Event.current.type == EventType.ContextClick && labelRect.Contains(Event.current.mousePosition)) {
            GenericMenu menu = new GenericMenu(); string animPath = $"material.{prop.name}";
            menu.AddItem(new GUIContent($"Copy Animation Path ({prop.name})"), false, () => { EditorGUIUtility.systemCopyBuffer = animPath; Debug.Log($"[Vixen Wear] Copied to clipboard: {animPath}"); });
            menu.ShowAsContext(); Event.current.Use(); 
        }
        EditorGUI.BeginChangeCheck(); ed.ShaderProperty(r, prop, label);
        if (EditorGUI.EndChangeCheck()) { ed.PropertiesChanged(); }
    }

    private void PerformPaste(MaterialEditor ed, MaterialProperty[] p, int tabIndex, bool includeTextures) {
        Undo.RecordObjects(ed.targets, "Paste Tab Settings");
        foreach (MaterialProperty prop in p) {
            if (Array.IndexOf(tabProps[tabIndex], prop.name) < 0) continue;
            switch(prop.type) {
                case MaterialProperty.PropType.Float: case MaterialProperty.PropType.Range: if (_clipboard.Floats.TryGetValue(prop.name, out float fVal)) prop.floatValue = fVal; break;
                case MaterialProperty.PropType.Color: if (_clipboard.Colors.TryGetValue(prop.name, out Color cVal)) prop.colorValue = cVal; break;
                case MaterialProperty.PropType.Vector: if (_clipboard.Vectors.TryGetValue(prop.name, out Vector4 vVal)) prop.vectorValue = vVal; break;
                case MaterialProperty.PropType.Texture:
                    if (!includeTextures) break;
                    if (_clipboard.Textures.TryGetValue(prop.name, out Texture tVal)) prop.textureValue = tVal;
                    if (_clipboard.TexOffsets.TryGetValue(prop.name, out Vector2 offset) && _clipboard.TexScales.TryGetValue(prop.name, out Vector2 scale)) {
                        foreach(var tgt in ed.targets) { Material m = (Material)tgt; m.SetTextureOffset(prop.name, offset); m.SetTextureScale(prop.name, scale); }
                    } break;
            }
        }
        ed.PropertiesChanged(); Debug.Log($"[Vixen Wear] Pasted {tabNames[tabIndex]} tab settings {(includeTextures ? "with" : "without")} textures. Applied to {ed.targets.Length} materials.");
    }

    public override void OnGUI(MaterialEditor ed, MaterialProperty[] p)
    {
        ed.SetDefaultGUIWidths(); EditorGUIUtility.labelWidth = Mathf.Min(220f, EditorGUIUtility.currentViewWidth * 0.55f); GUILayout.Space(4);

        Rect banner = GUILayoutUtility.GetRect(100, 36); EditorGUI.DrawRect(banner, bgBanner); EditorGUI.DrawRect(new Rect(banner.x, banner.y + banner.height - 2, banner.width, 2), pink);
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter, fontSize = 13, normal = { textColor = Color.white } };
        GUI.Label(banner, "LATEX ULTRA CONFIGURATION", titleStyle); GUILayout.Space(4);

        Rect tabGroupRect = GUILayoutUtility.GetRect(10f, 26f, GUILayout.ExpandWidth(true)); float tabWidth = tabGroupRect.width / tabNames.Length;

        for (int i = 0; i < tabNames.Length; i++) {
            Rect btnRect = new Rect(tabGroupRect.x + (i * tabWidth), tabGroupRect.y, tabWidth, tabGroupRect.height); bool isActive = (ActiveTab == i);
            EditorGUI.DrawRect(btnRect, isActive ? bgTabActive : bgTabIdle); if (isActive) EditorGUI.DrawRect(new Rect(btnRect.x, btnRect.y + btnRect.height - 2, btnRect.width, 2), cyan);

            if (Event.current.type == EventType.ContextClick && btnRect.Contains(Event.current.mousePosition)) {
                GenericMenu menu = new GenericMenu(); int tabIndex = i; 
                menu.AddItem(new GUIContent($"Copy {tabNames[tabIndex]} Settings"), false, () => {
                    _clipboard = new TabClipboard { TabIndex = tabIndex }; Material sourceMat = (Material)ed.target;
                    foreach(string propName in tabProps[tabIndex]) {
                        MaterialProperty prop = FindProperty(propName, p, false); if (prop == null) continue;
                        switch(prop.type) {
                            case MaterialProperty.PropType.Float: case MaterialProperty.PropType.Range: _clipboard.Floats[propName] = prop.floatValue; break;
                            case MaterialProperty.PropType.Color: _clipboard.Colors[propName] = prop.colorValue; break;
                            case MaterialProperty.PropType.Vector: _clipboard.Vectors[propName] = prop.vectorValue; break;
                            case MaterialProperty.PropType.Texture: _clipboard.Textures[propName] = prop.textureValue; if (sourceMat.HasProperty(propName)) { _clipboard.TexOffsets[propName] = sourceMat.GetTextureOffset(propName); _clipboard.TexScales[propName] = sourceMat.GetTextureScale(propName); } break;
                        }
                    } Debug.Log($"[Vixen Wear] Copied {tabNames[tabIndex]} tab settings.");
                });
                if (_clipboard != null && _clipboard.TabIndex == tabIndex) {
                    menu.AddItem(new GUIContent($"Paste {tabNames[tabIndex]} Settings (Values Only)"), false, () => PerformPaste(ed, p, tabIndex, false));
                    menu.AddItem(new GUIContent($"Paste {tabNames[tabIndex]} Settings (With Textures)"), false, () => PerformPaste(ed, p, tabIndex, true));
                } else {
                    menu.AddDisabledItem(new GUIContent($"Paste {tabNames[tabIndex]} Settings (Values Only)")); menu.AddDisabledItem(new GUIContent($"Paste {tabNames[tabIndex]} Settings (With Textures)"));
                }
                menu.ShowAsContext(); Event.current.Use();
            }

            if (Event.current.type == EventType.MouseDown && btnRect.Contains(Event.current.mousePosition)) { ActiveTab = i; Event.current.Use(); }
            GUIStyle labelStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel) { normal = { textColor = isActive ? cyan : new Color(0.6f, 0.6f, 0.6f) }, fontStyle = FontStyle.Bold, fontSize = 10 };
            GUI.Label(btnRect, tabNames[i], labelStyle);
        }

        GUILayout.Space(6); GUIStyle descStyle = new GUIStyle(EditorStyles.wordWrappedLabel) { fontSize = 11, normal = { textColor = new Color(0.7f, 0.7f, 0.7f) }, alignment = TextAnchor.MiddleCenter };
        GUILayout.Label(tabDesc[ActiveTab], descStyle); GUILayout.Space(4);

        EditorGUILayout.BeginVertical(Card);
        
        if (ActiveTab == 0) {
            DrawProp(ed, FindProperty("_Color", p, false), "Base Color"); DrawProp(ed, FindProperty("_MainTex", p, false), "Albedo (RGB) Cutout (A)"); DrawProp(ed, FindProperty("_CutOff", p, false), "Alpha Cutoff"); DrawProp(ed, FindProperty("_MinBrightness", p, false), "Minimum Brightness"); GUILayout.Space(10);
            DrawProp(ed, FindProperty("_UV_Rot", p, false), "UV Rotation"); DrawProp(ed, FindProperty("_SpeedX", p, false), "UV Speed X"); DrawProp(ed, FindProperty("_SpeedY", p, false), "UV Speed Y");
        } else if (ActiveTab == 1) {
            DrawProp(ed, FindProperty("_MetallicGlossMap", p, false), "Packed PBR Map"); DrawProp(ed, FindProperty("_BumpMap", p, false), "Normal Map"); GUILayout.Space(10);
            DrawProp(ed, FindProperty("_AO_Str", p, false), "Multi-Bounce AO Strength"); DrawProp(ed, FindProperty("_Spec_Occ", p, false), "Physical Specular Occlusion"); DrawProp(ed, FindProperty("_Shad_Hard", p, false), "Shadow Hardness"); DrawProp(ed, FindProperty("_Norm_Str", p, false), "Normal Strength"); GUILayout.Space(10);
            DrawProp(ed, FindProperty("_Parallax", p, false), "Parallax Depth"); DrawProp(ed, FindProperty("_Disp_Str", p, false), "Displacement Strength"); DrawProp(ed, FindProperty("_Tess_Edge", p, false), "Tessellation Edge"); GUILayout.Space(10);
            var _UseDet = FindProperty("_UseDetailNormal", p, false); DrawProp(ed, _UseDet, "Enable Micro Detail");
            if (_UseDet != null && _UseDet.floatValue > 0.5f) { DrawProp(ed, FindProperty("_DetailNormalMap", p, false), "Micro Detail Map"); DrawProp(ed, FindProperty("_Det_Strength", p, false), "Detail Strength"); DrawProp(ed, FindProperty("_Det_UV_Tiling", p, false), "Detail UV Tiling"); }
        } else if (ActiveTab == 2) {
            DrawProp(ed, FindProperty("_CC_Strength", p, false), "Clearcoat Strength"); DrawProp(ed, FindProperty("_CC_Smoothness", p, false), "Clearcoat Smoothness"); DrawProp(ed, FindProperty("_CC_Spec_AA", p, false), "Specular Anti-Aliasing"); DrawProp(ed, FindProperty("_CC_Flat", p, false), "Clearcoat Flattening"); GUILayout.Space(10);
            DrawProp(ed, FindProperty("_Film_Str", p, false), "Thin Film Iridescence"); DrawProp(ed, FindProperty("_Film_Thick", p, false), "Film Thickness (100-2000nm)"); DrawProp(ed, FindProperty("_Rim_Str", p, false), "Rim Light Strength"); DrawProp(ed, FindProperty("_Rim_Power", p, false), "Rim Light Power"); GUILayout.Space(10);
            DrawProp(ed, FindProperty("_SSS_Str", p, false), "Volumetric SSS Strength"); DrawProp(ed, FindProperty("_SSS_Dist", p, false), "Terminator Bleed (Wrap)"); DrawProp(ed, FindProperty("_SSS_Power", p, false), "Backscatter Focus");
        } else if (ActiveTab == 3) {
            DrawProp(ed, FindProperty("_EmissionColor", p, false), "Emission Color"); DrawProp(ed, FindProperty("_EmissionMap", p, false), "Emission Map"); DrawProp(ed, FindProperty("_Emis_Exp", p, false), "Emission Exposure"); GUILayout.Space(10);
            DrawProp(ed, FindProperty("_MatCap", p, false), "MatCap Texture"); DrawProp(ed, FindProperty("_MatCapMask", p, false), "MatCap Mask"); DrawProp(ed, FindProperty("_MatCap_Rot", p, false), "MatCap Rotation"); DrawProp(ed, FindProperty("_MatCap_Int", p, false), "MatCap Intensity"); DrawProp(ed, FindProperty("_MatCap_Lit", p, false), "MatCap Lighting Mix"); GUILayout.Space(10);
            var _LV = FindProperty("_UseLightVolumes", p, false); DrawProp(ed, _LV, "Enable Light Volumes"); if (_LV != null && _LV.floatValue > 0.5f) DrawProp(ed, FindProperty("_LV_Int", p, false), "Light Volumes Intensity");
            var _LTCGI = FindProperty("_UseLTCGI", p, false); DrawProp(ed, _LTCGI, "Enable LTCGI"); if (_LTCGI != null && _LTCGI.floatValue > 0.5f) DrawProp(ed, FindProperty("_LTCGI_Int", p, false), "LTCGI Intensity");
        } else if (ActiveTab == 4) {
            var _AL = FindProperty("_UseAudioLink", p, false); DrawProp(ed, _AL, "Enable AudioLink");
            if (_AL != null && _AL.floatValue > 0.5f) {
                GUILayout.Space(15);
                EditorGUILayout.LabelField("Environment & Media", EditorStyles.boldLabel);
                DrawProp(ed, FindProperty("_AL_ColorMode", p, false), "Global Color Source"); DrawProp(ed, FindProperty("_UseMediaState", p, false), "Power Down on Pause/Stop"); GUILayout.Space(10);

                EditorGUILayout.LabelField("God Tier Cybernetics (HUD Overlays)", EditorStyles.boldLabel);
                var _Cyber = FindProperty("_UseCyber", p, false); DrawProp(ed, _Cyber, "Enable HUD Overlays");
                if (_Cyber != null && _Cyber.floatValue > 0.5f) {
                    EditorGUI.indentLevel++; DrawProp(ed, FindProperty("_CyberMask", p, false), "B&W Window Mask"); DrawProp(ed, FindProperty("_Cyber_AutoCorr_Str", p, false), "AutoCorrelator Mask Warp"); GUILayout.Space(4);
                    var _UVU = FindProperty("_UseCyberVU", p, false); DrawProp(ed, _UVU, "Enable VU Meter Segment");
                    if (_UVU != null && _UVU.floatValue > 0.5f) { DrawProp(ed, FindProperty("_Cyber_VU_Band", p, false), "VU Meter Band"); DrawProp(ed, FindProperty("_Cyber_VU_Str", p, false), "VU Meter Intensity"); DrawProp(ed, FindProperty("_Cyber_VU_Transform", p, false), "VU Meter Placement (X,Y,Scl,Rot)"); } GUILayout.Space(4);
                    var _UCC = FindProperty("_UseCyberCC", p, false); DrawProp(ed, _UCC, "Enable Spectrum Segment");
                    if (_UCC != null && _UCC.floatValue > 0.5f) { DrawProp(ed, FindProperty("_Cyber_CC_Band", p, false), "Spectrum Primary Band"); DrawProp(ed, FindProperty("_Cyber_CC_Str", p, false), "Spectrum Intensity"); DrawProp(ed, FindProperty("_Cyber_CC_Transform", p, false), "Spectrum Placement (X,Y,Scl,Rot)"); } GUILayout.Space(4);
                    var _UWave = FindProperty("_UseCyberWave", p, false); DrawProp(ed, _UWave, "Enable Waveform Line");
                    if (_UWave != null && _UWave.floatValue > 0.5f) { DrawProp(ed, FindProperty("_Cyber_Wave_Str", p, false), "Waveform Intensity"); DrawProp(ed, FindProperty("_Cyber_Wave_Transform", p, false), "Waveform Placement (X,Y,Scl,Rot)"); } GUILayout.Space(4);
                    var _UDMX = FindProperty("_UseCyberDMX", p, false); DrawProp(ed, _UDMX, "Enable DMX Grid Segment");
                    if (_UDMX != null && _UDMX.floatValue > 0.5f) { DrawProp(ed, FindProperty("_Cyber_DMX_Str", p, false), "DMX Grid Intensity"); DrawProp(ed, FindProperty("_Cyber_DMX_Transform", p, false), "DMX Placement (X,Y,Scl,Rot)"); } EditorGUI.indentLevel--;
                } GUILayout.Space(10);

                EditorGUILayout.LabelField("Kinetic Vertex Engine (Dual-Pass Geometry)", EditorStyles.boldLabel);
                var _UseVtx = FindProperty("_UseVtxKinetic", p, false); DrawProp(ed, _UseVtx, "Enable Geometry Shatter");
                if (_UseVtx != null && _UseVtx.floatValue > 0.5f) {
                    EditorGUI.indentLevel++; DrawProp(ed, FindProperty("_Vtx_Pump_Band", p, false), "Vertex Pump Band"); DrawProp(ed, FindProperty("_Vtx_Pump_Str", p, false), "Normal Inflate Distance"); GUILayout.Space(4);
                    DrawProp(ed, FindProperty("_Vtx_Fracture_Band", p, false), "Geometry Fracture Band"); DrawProp(ed, FindProperty("_Vtx_Fracture_Str", p, false), "Geometry Shard Scatter"); EditorGUI.indentLevel--;
                } GUILayout.Space(10);
                
                EditorGUILayout.LabelField("Kinetic UV Engine (Surface Maps)", EditorStyles.boldLabel);
                var _UseVortex = FindProperty("_UseALVortex", p, false); DrawProp(ed, _UseVortex, "Enable Vortex Twist");
                if (_UseVortex != null && _UseVortex.floatValue > 0.5f) { EditorGUI.indentLevel++; DrawProp(ed, FindProperty("_AL_Vortex_Band", p, false), "Vortex Band"); DrawProp(ed, FindProperty("_AL_Vortex_Str", p, false), "Vortex Twist Strength"); DrawProp(ed, FindProperty("_AL_Vortex_UV", p, false), "Vortex Transform (X,Y,Scl,Rot)"); EditorGUI.indentLevel--; } GUILayout.Space(2);
                var _UsePump = FindProperty("_UseALPump", p, false); DrawProp(ed, _UsePump, "Enable UV Bass Pump");
                if (_UsePump != null && _UsePump.floatValue > 0.5f) { EditorGUI.indentLevel++; DrawProp(ed, FindProperty("_AL_Pump_Band", p, false), "UV Pump Band"); DrawProp(ed, FindProperty("_AL_Pump_Str", p, false), "UV Pump Bounce"); DrawProp(ed, FindProperty("_AL_Pump_UV", p, false), "Pump Transform (X,Y,Scl,Rot)"); EditorGUI.indentLevel--; } GUILayout.Space(2);
                var _UseFracture = FindProperty("_UseALFracture", p, false); DrawProp(ed, _UseFracture, "Enable UV Fracture Shard");
                if (_UseFracture != null && _UseFracture.floatValue > 0.5f) { EditorGUI.indentLevel++; DrawProp(ed, FindProperty("_AL_Fracture_Band", p, false), "Fracture Band"); DrawProp(ed, FindProperty("_AL_Fracture_Str", p, false), "Fracture Strength"); DrawProp(ed, FindProperty("_AL_Fracture_UV", p, false), "Fracture Transform (X,Y,Scl,Rot)"); EditorGUI.indentLevel--; } GUILayout.Space(10);

                EditorGUILayout.LabelField("Global Material Modulations", EditorStyles.boldLabel);
                DrawProp(ed, FindProperty("_AL_Band_Emission", p, false), "Reaction Band"); DrawProp(ed, FindProperty("_AL_Emis_Mod", p, false), "Emission Amplitude"); DrawProp(ed, FindProperty("_AL_Col_Blend", p, false), "Audio Color Blend"); DrawProp(ed, FindProperty("_AL_Waveform_Mod", p, false), "Surface Waveform Ripple"); GUILayout.Space(10);
                
                EditorGUILayout.LabelField("Audio Scanlines", EditorStyles.boldLabel);
                DrawProp(ed, FindProperty("_AL_Band_Scanlines", p, false), "Reaction Band"); DrawProp(ed, FindProperty("_AL_Scanlines", p, false), "Scanline Visibility Blend"); DrawProp(ed, FindProperty("_AL_Scan_Density", p, false), "Scanline Density"); DrawProp(ed, FindProperty("_AL_Scan_Speed", p, false), "Base Scan Speed"); DrawProp(ed, FindProperty("_AL_Scan_React", p, false), "Audio Speed Reactivity"); GUILayout.Space(10);
                
                EditorGUILayout.LabelField("Physical Lobe Thump", EditorStyles.boldLabel);
                DrawProp(ed, FindProperty("_AL_Band_Film", p, false), "Film Expansion Band"); DrawProp(ed, FindProperty("_AL_Film_Mod", p, false), "Thin Film Expansion"); DrawProp(ed, FindProperty("_AL_Band_Parallax", p, false), "Parallax Thump Band"); DrawProp(ed, FindProperty("_AL_Paralx_Mod", p, false), "Parallax Thump"); DrawProp(ed, FindProperty("_AL_Band_Shatter", p, false), "Clearcoat Shatter Band"); DrawProp(ed, FindProperty("_AL_CC_Shatter", p, false), "Clearcoat Shatter"); DrawProp(ed, FindProperty("_AL_Band_Glitch", p, false), "Digital Tear Band"); DrawProp(ed, FindProperty("_AL_Glitch_Mod", p, false), "Digital Domain Tear");
            }
        } else if (ActiveTab == 5) {
            var _VRSL = FindProperty("_UseVRSL", p, false); DrawProp(ed, _VRSL, "Enable VRSL Stage Hijack Protocol");
            if (_VRSL != null && _VRSL.floatValue > 0.5f) {
                GUILayout.Space(15); EditorGUILayout.LabelField("DMX Universe Routing", EditorStyles.boldLabel); DrawProp(ed, FindProperty("_DMX_Channel", p, false), "DMX Base Channel (Sector ID)"); DrawProp(ed, FindProperty("_VRSL_Intensity", p, false), "Stage Hijack Override Power"); GUILayout.Space(10);
                EditorGUILayout.LabelField("Kinetic Geo-Warping Engine", EditorStyles.boldLabel); DrawProp(ed, FindProperty("_VRSL_Geo_Warp", p, false), "Pan/Tilt Displacement Scale"); GUILayout.Space(15);
                EditorGUILayout.HelpBox("VRSL Stage Hijack links this material directly to world-space DMX buffers. When active light intensity is detected from the stage, it will override native AudioLink color arrays and physically warp tessellated vertex positions to match stage configurations in real-time.", MessageType.Info);
            }
        }

        EditorGUILayout.EndVertical(); GUILayout.Space(10); ed.RenderQueueField(); ed.EnableInstancingField(); ed.DoubleSidedGIField();
    }
}
#endif