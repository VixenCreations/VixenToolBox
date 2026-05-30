Shader "VixenWear/Latex Ultra"
{
    Properties
    {
        // Rendering mode drives the alpha workflow - Opaque (no clip/blend), Cutout (clip on _CutOff), Fade (straight alpha - everything fades), Transparent (premultiplied - specular survives); defaults to Cutout for historical clip(c.a - _CutOff) behavior.
        [Enum(Opaque,0,Cutout,1,Fade,2,Transparent,3)] _Mode ("Rendering Mode", Float) = 1
        [HideInInspector] _SrcBlend ("__src", Float) = 1
        [HideInInspector] _DstBlend ("__dst", Float) = 0
        [HideInInspector] _ZWrite ("__zw", Float) = 1

        _Color ("Color", Color) = (1,1,1,1)
        _CutOff ("CutOff", Range(0,1)) = 0.5
        _MainTex ("Albedo (RGB) Cutout (A)", 2D) = "white" {}
        _MinBrightness ("Minimum Brightness", Range(0, 1)) = 0.0

        _UV_Rot ("UV Rotation", Float) = 0
        _SpeedX ("UV Speed X", Float) = 0
        _SpeedY ("UV Speed Y", Float) = 0
        _MatCap_Rot ("MatCap Rotation", Float) = 0

        [NoScaleOffset] _MetallicGlossMap ("Packed PBR Mask", 2D) = "white" {}
        [NoScaleOffset][Normal] _BumpMap ("Normal Map", 2D) = "bump" {}

        // Poiyomi PBR Mask compatibility - per-channel selectors so Poiyomi/Substance/Marmoset-packed masks drop in without re-authoring; defaults match VixenWear's native packing (R:Met G:AO B:Disp A:Smooth).
        [Enum(R,0,G,1,B,2,A,3)] _PBR_Met_Ch ("Metallic Channel", Float) = 0
        [Toggle] _PBR_Met_Inv ("Invert Metallic", Float) = 0
        [Enum(R,0,G,1,B,2,A,3)] _PBR_Smooth_Ch ("Smoothness Channel", Float) = 3
        [Toggle] _PBR_Smooth_Inv ("Channel Stores Roughness (Invert)", Float) = 0
        [Enum(R,0,G,1,B,2,A,3)] _PBR_AO_Ch ("AO Channel", Float) = 1
        [Enum(R,0,G,1,B,2,A,3)] _PBR_Height_Ch ("Height Channel", Float) = 2

        _AO_Str ("AO Strength", Range(0,1)) = 1.0
        _Spec_Occ ("Specular Occlusion", Range(0,1)) = 1.0
        _Shad_Hard ("Shadow Hardness", Range(0,1)) = 1.0
        _Norm_Str ("Normal Strength", Range(0,5)) = 1.0

        _Parallax ("Parallax Depth", Range(0,0.1)) = 0.0
        _Disp_Str ("Displacement Strength", Range(0,1)) = 0.0
        _Tess_Edge ("Tessellation Edge Length", Range(1,50)) = 10.0
        _Emis_Exp ("Emission Exposure", Float) = 1.0

        _CC_Strength ("Clearcoat Strength", Range(0,1)) = 1.0
        _CC_Smoothness ("Clearcoat Smoothness", Range(0,1)) = 0.9
        _CC_Spec_AA ("Specular Anti-Aliasing", Range(0,1)) = 0.0
        _CC_Flat ("Clearcoat Flattening", Range(0,1)) = 1.0
        _CC_Tint ("Clearcoat Tint", Color) = (1,1,1,1)
        _CC_F0 ("Clearcoat F0", Range(0.01, 0.2)) = 0.04

        _Film_Str ("Thin Film Strength", Range(0,1)) = 0.0
        _Film_Thick ("Thin Film Thickness", Float) = 0.0
        _Rim_Str ("Rim Light Strength", Range(0,5)) = 0.0
        _Rim_Power ("Rim Light Power", Range(0.1,10)) = 4.0

        _SSS_Str ("Subsurface Strength", Range(0,1)) = 0.0
        _SSS_Dist ("Subsurface Distance", Range(0,1)) = 0.1
        _SSS_Power ("Subsurface Power", Range(0.1,10)) = 4.0

        _Aniso ("Anisotropy", Range(-1,1)) = 0.0
        _AnisoRot ("Anisotropy Rotation", Range(0,360)) = 0.0

        _Trans_Str ("Transmission Strength", Range(0,1)) = 0.0
        _Trans_Dist ("Transmission Distance", Range(0.001,1)) = 0.25
        _Trans_Power ("Transmission Falloff", Range(0.1,10)) = 2.0

        [Toggle] _UseMultiScatter ("Multi-Scatter Energy Compensation", Float) = 1

        [Toggle(_DETAIL_NORMAL)] _UseDetailNormal ("Enable Micro Detail", Float) = 0
        [NoScaleOffset][Normal] _DetailNormalMap ("Micro Detail Map", 2D) = "bump" {}

        _Det_Strength ("Detail Strength", Range(0,2)) = 0.0
        _Det_UV_Tiling ("Detail UV Tiling", Float) = 1.0

        [HDR] _EmissionColor ("Emission Color", Color) = (0,0,0,1)
        [NoScaleOffset] _EmissionMap ("Emission Map (RGB tint, A mask)", 2D) = "black" {}

        // Poiyomi-style secondary emission layer - independent texture, color, mask, and AL band reactor.
        [Toggle] _UseEmission2 ("Enable Secondary Emission Layer", Float) = 0
        [HDR] _EmissionColor2 ("Emission Color 2", Color) = (0,0,0,1)
        [NoScaleOffset] _EmissionMap2 ("Emission Map 2 (RGB tint, A mask)", 2D) = "black" {}
        [Enum(R,0,G,1,B,2,A,3)] _Emis2_MaskCh ("Emission 2 Mask Channel", Float) = 3
        [Enum(Bass,0,Low Mid,1,High Mid,2,Treble,3)] _AL_Band_Emis2 ("Emission 2 AL Band", Float) = 1
        _AL_Emis2_Mod ("Emission 2 AL Amplitude", Range(0,1)) = 0.0

        // Poiyomi-style multi-region color mask - RGB zones each drive an albedo tint and emission boost.
        [Toggle] _UseRegionMask ("Enable Multi-Region Color Mask", Float) = 0
        [NoScaleOffset] _RegionMask ("Region Mask (R/G/B Zones)", 2D) = "black" {}
        _Region_R_Tint ("Red Zone Tint", Color) = (1,1,1,1)
        _Region_R_Emis ("Red Zone Emission Boost", Range(0,5)) = 0
        _Region_G_Tint ("Green Zone Tint", Color) = (1,1,1,1)
        _Region_G_Emis ("Green Zone Emission Boost", Range(0,5)) = 0
        _Region_B_Tint ("Blue Zone Tint", Color) = (1,1,1,1)
        _Region_B_Emis ("Blue Zone Emission Boost", Range(0,5)) = 0

        [NoScaleOffset] _MatCap ("MatCap 1 Texture", 2D) = "black" {}
        [NoScaleOffset] _MatCapMask ("MatCap 1 Mask", 2D) = "white" {}
        // Mask channel pick - defaults to R for single-channel mask compat; set to G/B/A to drive layer 1 from a different channel of an RGB region mask.
        [Enum(R,0,G,1,B,2,A,3)] _MatCap_MaskCh ("MatCap 1 Mask Channel", Float) = 0
        _MatCap_Tint ("MatCap 1 Tint", Color) = (1,1,1,1)

        _MatCap_Int ("MatCap 1 Intensity", Range(0,2)) = 0.0
        _MatCap_Lit ("MatCap 1 Lighting Mix", Range(0,1)) = 1.0

        // Second matcap layer - own texture/mask/channel/tint/intensity/rotation/blend mode; common workflow drops the same red/blue/black region mask into both layers and picks R for layer 1, B for layer 2 so each zone shows a different matcap.
        [Toggle] _UseMatCap2 ("Enable MatCap 2 Layer", Float) = 0
        [NoScaleOffset] _MatCap2 ("MatCap 2 Texture", 2D) = "black" {}
        [NoScaleOffset] _MatCap2_Mask ("MatCap 2 Mask", 2D) = "white" {}
        [Enum(R,0,G,1,B,2,A,3)] _MatCap2_MaskCh ("MatCap 2 Mask Channel", Float) = 2
        _MatCap2_Tint ("MatCap 2 Tint", Color) = (1,1,1,1)
        _MatCap2_Int ("MatCap 2 Intensity", Range(0,2)) = 1.0
        _MatCap2_Rot ("MatCap 2 Rotation", Float) = 0
        [Enum(Add,0,Replace,1,Multiply,2)] _MatCap2_Blend ("MatCap 2 Blend Mode", Float) = 0
        _LV_Int ("Light Volumes Intensity", Range(0,1)) = 1.0
        _LV_Spec_Mix ("Light Volumes Specular Mix", Range(0,2)) = 1.0
        [Toggle] _LV_Spec_Dominant ("Light Volumes Specular (Dominant Mode)", Float) = 0
        _LV_CC_Spec_Mix ("Light Volumes Clearcoat Specular", Range(0,2)) = 1.0
        _LV_Bias ("Light Volumes Normal Bias", Float) = 0.0
        [VectorLabel(X, Y, Z, NONE)] _LV_PosOffset ("Light Volumes Position Offset", Vector) = (0,0,0,0)
        [Toggle] _LV_AdditiveOnly ("Light Volumes Additive-Only Mode", Float) = 0
        [Toggle] _LV_ProbeDering ("Use Deringed Probes (Bakery L1, opt-in)", Float) = 0
        _LTCGI_Int ("LTCGI Intensity", Range(0,1)) = 1.0
        _LTCGI_Spec_Mix ("LTCGI Specular Mix", Range(0,2)) = 1.0
        _LTCGI_Diff_Mix ("LTCGI Diffuse Mix", Range(0,2)) = 1.0

        [Toggle(VRSL_ENABLE)] _UseVRSL ("Enable VRSL DMX Link", Float) = 0
        _DMX_Channel ("DMX Base Channel (Sector start)", Int) = 1
        _VRSL_Intensity ("VRSL Override Strength", Range(0, 1)) = 1.0
        _VRSL_Geo_Warp ("Pan/Tilt Geo-Warping", Range(0, 5)) = 1.0
        _VRSL_Color_Hijack ("VRSL Color Hijack", Range(0, 1)) = 0.0

        [Toggle] _UseAudioLink ("Enable AudioLink", Float) = 0
        [Enum(Manual,0,ColorChord,1,Theme 0,2,Theme 1,3,Theme 2,4,Theme 3,5,ColorChord Strip,6)] _AL_ColorMode ("Color Source", Float) = 1
        [Toggle] _UseMediaState ("Power Down on Pause/Stop", Float) = 0
        _AL_Strip_Pos ("ColorChord Strip Position", Range(0,1)) = 0.5

        [Enum(Idx 0,0,Idx 1,1,Idx 2,2,Idx 3,3,Idx 4,4,Idx 5,5,Idx 6,6,Idx 7,7)] _AL_Chrono_Idx ("Chronotensity Index", Float) = 0
        [Toggle] _UseChronoFX ("Enable Chronotensity FX", Float) = 0

        [Toggle] _UseCyber ("Enable Cybernetic Overlays", Float) = 0
        [NoScaleOffset] _CyberMask ("Cyber Mask (B&W Window)", 2D) = "white" {}
        _Cyber_Hover ("HUD Hover Height (Float Off Body)", Range(0, 0.15)) = 0.03
        _Cyber_Hover_Bob ("HUD Hover Bob (Subtle Drift)", Range(0, 1)) = 0.25

        [Toggle] _UseCyberVU ("Enable VU Meter", Float) = 0
        [Enum(Bass,0,Low Mid,1,High Mid,2,Treble,3)] _Cyber_VU_Band ("VU Band", Float) = 0
        _Cyber_VU_Str ("VU Meter Intensity", Range(0,5)) = 1.0
        [VectorLabel(X Offset, Y Offset, Scale, Rotation)] _Cyber_VU_Transform ("VU Transform", Vector) = (0,0,1,0)

        [Toggle] _UseCyberCC ("Enable Spectrum", Float) = 0
        [Enum(Bass,0,Low Mid,1,High Mid,2,Treble,3)] _Cyber_CC_Band ("Spectrum Primary Band", Float) = 1
        _Cyber_CC_Str ("Spectrum Strip Intensity", Range(0,5)) = 1.0
        _Cyber_CC_Density ("Spectrum Bar Count", Range(4,64)) = 16
        [VectorLabel(X Offset, Y Offset, Scale, Rotation)] _Cyber_CC_Transform ("Spectrum Transform", Vector) = (0,0,1,0)

        [Toggle] _UseCyberWave ("Enable Waveform", Float) = 0
        _Cyber_Wave_Str ("Waveform Intensity", Range(0,5)) = 1.0
        [VectorLabel(X Offset, Y Offset, Scale, Rotation)] _Cyber_Wave_Transform ("Waveform Transform", Vector) = (0,0,1,0)

        [Toggle] _UseCyberDMX ("Enable DMX Grid Readout", Float) = 0
        _Cyber_DMX_Str ("DMX Grid Intensity", Range(0,5)) = 1.0
        [VectorLabel(X Offset, Y Offset, Scale, Rotation)] _Cyber_DMX_Transform ("DMX Grid Transform", Vector) = (0,0,1,0)

        [Toggle] _UseCyberAuto ("Enable Autocorrelator Ring", Float) = 0
        _Cyber_AutoCorr_Str ("Autocorrelator Intensity", Range(0,5)) = 1.0
        [VectorLabel(X Offset, Y Offset, Scale, Rotation)] _Cyber_Auto_Transform ("Autocorrelator Transform", Vector) = (0,0,1,0)

        [Toggle] _UseVtxKinetic ("Enable Vertex Displacement", Float) = 0
        [Enum(Bass,0,Low Mid,1,High Mid,2,Treble,3)] _Vtx_Pump_Band ("Vertex Pump Band", Float) = 0
        _Vtx_Pump_Str ("Vertex Pump Distance", Range(0, 5)) = 0.0

        [Enum(Bass,0,Low Mid,1,High Mid,2,Treble,3)] _Vtx_Fracture_Band ("Vertex Fracture Band", Float) = 3
        _Vtx_Fracture_Str ("Vertex Fracture Scatter", Range(0, 5)) = 0.0

        _Vtx_AutoCorr_Str ("Vertex Autocorrelator Ripple", Range(0,5)) = 0.0

        [Toggle] _UseALVortex ("Enable Vortex Twist", Float) = 0
        [Enum(Bass,0,Low Mid,1,High Mid,2,Treble,3)] _AL_Vortex_Band ("Vortex Band", Float) = 2
        _AL_Vortex_Str ("Vortex Twist Strength", Range(0, 10)) = 2.5
        [VectorLabel(X Offset, Y Offset, Scale, Rotation)] _AL_Vortex_UV ("Vortex Transform", Vector) = (0,0,1,0)

        [Toggle] _UseALPump ("Enable UV Bass Pump", Float) = 0
        [Enum(Bass,0,Low Mid,1,High Mid,2,Treble,3)] _AL_Pump_Band ("UV Pump Band", Float) = 0
        _AL_Pump_Str ("UV Pump Bounce", Range(0, 1)) = 0.35
        [VectorLabel(X Offset, Y Offset, Scale, Rotation)] _AL_Pump_UV ("Pump Transform", Vector) = (0,0,1,0)

        [Toggle] _UseALFracture ("Enable UV Fracture", Float) = 0
        [Enum(Bass,0,Low Mid,1,High Mid,2,Treble,3)] _AL_Fracture_Band ("Fracture Band", Float) = 3
        _AL_Fracture_Str ("Fracture Strength", Range(0, 2)) = 0.4
        [VectorLabel(X Offset, Y Offset, Scale, Rotation)] _AL_Fracture_UV ("Fracture Transform", Vector) = (0,0,1,0)

        [Enum(Bass,0,Low Mid,1,High Mid,2,Treble,3)] _AL_Band_Emission ("Emission Band", Float) = 0
        _AL_Emis_Mod ("Emission Amplitude", Range(0,1)) = 0.0
        _AL_Col_Blend ("Color Blend Strength", Range(0,1)) = 0.0
        _AL_Waveform_Mod ("Surface Waveform Ripple", Range(0,1)) = 0.0
        _AL_AutoCorr_Mod ("Surface Autocorrelator Ripple", Range(0,1)) = 0.0
        _AL_DFT_Note ("DFT Note Mod (0-11)", Range(0,11)) = 0
        _AL_DFT_Mod ("DFT Note Emission Amount", Range(0,2)) = 0.0

        [Enum(Bass,0,Low Mid,1,High Mid,2,Treble,3)] _AL_Band_Scanlines ("Scanline Band", Float) = 1
        _AL_Scanlines ("Scanline Visibility Blend", Range(0,1)) = 0.0
        _AL_Scan_Density ("Scanline Density", Float) = 50.0
        _AL_Scan_Speed ("Base Scan Speed", Float) = 1.0
        _AL_Scan_React ("Scanline Chronotensity Reaction", Range(0,2)) = 0.0

        [Enum(Bass,0,Low Mid,1,High Mid,2,Treble,3)] _AL_Band_Film ("Thin Film Band", Float) = 2
        _AL_Film_Mod ("Thin Film Expansion", Range(0,1)) = 0.0

        [Enum(Bass,0,Low Mid,1,High Mid,2,Treble,3)] _AL_Band_Parallax ("Parallax Band", Float) = 0
        _AL_Paralx_Mod ("Parallax Thump", Range(0,1)) = 0.0

        [Enum(Bass,0,Low Mid,1,High Mid,2,Treble,3)] _AL_Band_Shatter ("Clearcoat Shatter Band", Float) = 2
        _AL_CC_Shatter ("Clearcoat Shatter", Range(0,1)) = 0.0

        [Enum(Bass,0,Low Mid,1,High Mid,2,Treble,3)] _AL_Band_Glitch ("Glitch Band", Float) = 3
        _AL_Glitch_Mod ("Digital Glitch Tear", Range(0,1)) = 0.0
    }

    SubShader
    {
        // Tags listed here are SubShader defaults - VixenWearEditor overrides RenderType/Queue/VRCFallback per material via SetOverrideTag to match the selected _Mode (Opaque/Cutout/Fade/Transparent).
        Tags { "RenderType"="Opaque" "VRCFallback"="ToonDoubleSided" "Queue"="Geometry" }
        LOD 500
        Cull Off

        // Blend/ZWrite are property-driven so the editor flips them per-material without a recompile - Opaque/Cutout use One/Zero/ZWrite On; Fade uses SrcAlpha/OneMinusSrcAlpha/ZWrite Off; Transparent uses One/OneMinusSrcAlpha/ZWrite Off.
        Blend [_SrcBlend] [_DstBlend]
        ZWrite [_ZWrite]

        // PASS 1: CORE PBR SURFACE (BASE SUIT, FRACTURE CLIP)
        CGPROGRAM
        // Surface pragma drops Deferred/Meta + LIGHTMAP/DIRLIGHTMAP/SHADOWMASK/LPPV variants (VRChat forward-only, avatar clothing never lightmapped); keepalpha preserves LightingStandardLatex alpha so Fade/Transparent get real alpha. noforwardadd skips the ForwardAdd pass entirely (avatar gets directional + probes + LV + LTCGI; loses realtime per-light additive contributions) - critical for ps_5_0 sampler budget because ForwardAdd's POINT/POINT_COOKIE + SHADOWS_CUBE built-in samplers stacked on our 13 texture samplers blew past the 16-register cap.
        #pragma surface surf StandardLatex keepalpha fullforwardshadows addshadow noforwardadd vertex:disp tessellate:tessEdge exclude_path:deferred exclude_path:prepass nolightmap nodynlightmap nodirlightmap noshadowmask nometa nolppv
        #pragma target 5.0

        // Defensive against Unity 2022.3.x emitting lightmap/LOD variants despite the no* directives above. Cookie + cube-shadow variants are also skipped for sampler budget - any directional cookie / point cube shadow would add 1-2 samplers, and avatars don't typically use them.
        #pragma skip_variants LOD_FADE_CROSSFADE LIGHTMAP_ON DIRLIGHTMAP_COMBINED DYNAMICLIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK DIRECTIONAL_COOKIE POINT_COOKIE SHADOWS_CUBE

        // AudioLink always compiled and runtime-gated via _UseAudioLink so VRCFury material-toggle animations can flip it without a build-time variant (VRC materials can't change keywords at runtime); VRSL_ENABLE is referenced in disp() so it needs full per-stage variants - the rest are fragment-only.
        #pragma shader_feature_local VRSL_ENABLE
        #pragma shader_feature_local_fragment LIGHTVOLUMES_ENABLE
        #pragma shader_feature_local_fragment LTCGI_ENABLE
        #pragma shader_feature_local_fragment _DETAIL_NORMAL
        // Alpha workflow keywords - set by VixenWearEditor based on _Mode. Mutually exclusive; Opaque mode = none on.
        #pragma shader_feature_local _ALPHATEST_ON
        #pragma shader_feature_local _ALPHABLEND_ON
        #pragma shader_feature_local _ALPHAPREMULTIPLY_ON

        #include "UnityPBSLighting.cginc"
        #include "Tessellation.cginc"
        #include "UnityCG.cginc"

        #if defined(LIGHTVOLUMES_ENABLE)
            #include "Packages/com.vixencreations.vixens-toolbox/Editor/Avatar Tools/Shaders/cginc/LightVolumes.cginc"
        #endif
        // AudioLink.cginc is always included (runtime-gated by _UseAudioLink) so VRCFury toggles work without keyword variants.
        #include "Packages/com.vixencreations.vixens-toolbox/Editor/Avatar Tools/Shaders/cginc/AudioLink.cginc"
        #if defined(LTCGI_ENABLE)
            #include "Packages/com.vixencreations.vixens-toolbox/Editor/Avatar Tools/Shaders/cginc/LTCGI.cginc"
        #endif

        // VRChat mirror cameras leave _WorldSpaceCameraPos at the player's head - view-dependent math (specular, parallax, cubemap) renders wrong in the mirror; UNITY_MATRIX_I_V._m03_m13_m23 is the actual rendering camera world pos (per-eye correct under single-pass instanced).
        float3 vw_CameraPos()    { return UNITY_MATRIX_I_V._m03_m13_m23; }
        float3 vw_WorldViewDir(float3 worldPos) { return normalize(vw_CameraPos() - worldPos); }

        struct SurfaceOutputStandardLatex
        {
            fixed3 Albedo;
            float3 Normal;
            float3 ClearcoatNormal;
            float3 WorldPos;
            float2 UV;
            float3x3 WorldToTangent;

            half3 Emission;
            half3 Matcap;
            half  Metallic;
            half  Smoothness;

            half  BaseRoughness;
            half  ClearcoatSmoothness;
            half  ClearcoatStrength;
            half  ThinFilmStrength;
            half  ThinFilmThickness;
            half  Occlusion;
            half  Height;

            fixed Alpha;
            float ParallaxDepth;
            half  Thickness;
            half  SpecAA;
            half3 LVSpec;
            half3 LVCCSpec;
            half3 LVDiffuse;
            half  LVActive;

            half  Anisotropy;
            half  AnisoRotation;
            half  Transmission;
        };

        struct Input
        {
            float2 uv_MainTex;
            fixed facing : VFACE;
            float3 viewDir;
            float3 worldPos;
            float3 worldNormal;
            INTERNAL_DATA
        };

        sampler2D _MainTex, _MetallicGlossMap, _BumpMap, _DetailNormalMap, _EmissionMap, _EmissionMap2, _RegionMask, _MatCap, _MatCapMask, _MatCap2, _MatCap2_Mask, _CyberMask;
        fixed4 _Color, _EmissionColor, _EmissionColor2, _CC_Tint;
        fixed4 _Region_R_Tint, _Region_G_Tint, _Region_B_Tint;
        fixed4 _MatCap_Tint, _MatCap2_Tint;
        half _CutOff, _MinBrightness;
        float _UV_Rot, _SpeedX, _SpeedY, _MatCap_Rot;
        float _AO_Str, _Spec_Occ, _Shad_Hard, _Norm_Str;
        float _Parallax, _Disp_Str, _Tess_Edge, _Emis_Exp;
        // Poiyomi compat: PBR mask channel selectors + invert toggles.
        float _PBR_Met_Ch, _PBR_Met_Inv, _PBR_Smooth_Ch, _PBR_Smooth_Inv, _PBR_AO_Ch, _PBR_Height_Ch;
        // Poiyomi compat: secondary emission layer + multi-region color mask.
        float _UseEmission2, _Emis2_MaskCh, _AL_Band_Emis2, _AL_Emis2_Mod;
        float _UseRegionMask, _Region_R_Emis, _Region_G_Emis, _Region_B_Emis;
        float _CC_Strength, _CC_Smoothness, _CC_Spec_AA, _CC_Flat, _CC_F0;
        float _Film_Str, _Film_Thick, _Rim_Str, _Rim_Power;
        float _SSS_Str, _SSS_Dist, _SSS_Power;
        float _Aniso, _AnisoRot;
        float _Trans_Str, _Trans_Dist, _Trans_Power;
        float _UseMultiScatter;
        float _Det_Strength, _Det_UV_Tiling;
        float _MatCap_Int, _MatCap_Lit, _MatCap_MaskCh;
        float _UseMatCap2, _MatCap2_MaskCh, _MatCap2_Int, _MatCap2_Rot, _MatCap2_Blend;
        float _LV_Int, _LV_Spec_Mix, _LV_Spec_Dominant;
        float _LV_CC_Spec_Mix, _LV_Bias, _LV_AdditiveOnly, _LV_ProbeDering;
        float4 _LV_PosOffset;
        float _LTCGI_Int, _LTCGI_Spec_Mix, _LTCGI_Diff_Mix;

        float _UseALVortex, _AL_Vortex_Band, _AL_Vortex_Str; float4 _AL_Vortex_UV;
        float _UseALPump, _AL_Pump_Band, _AL_Pump_Str; float4 _AL_Pump_UV;
        float _UseALFracture, _AL_Fracture_Band, _AL_Fracture_Str; float4 _AL_Fracture_UV;

        float _UseVtxKinetic, _Vtx_Pump_Band, _Vtx_Pump_Str;
        float _Vtx_Fracture_Band, _Vtx_Fracture_Str, _Vtx_AutoCorr_Str;

        float _UseCyber, _Cyber_AutoCorr_Str, _Cyber_Hover, _Cyber_Hover_Bob;
        float _UseCyberVU, _Cyber_VU_Band, _Cyber_VU_Str; float4 _Cyber_VU_Transform;
        float _UseCyberCC, _Cyber_CC_Band, _Cyber_CC_Str, _Cyber_CC_Density; float4 _Cyber_CC_Transform;
        float _UseCyberWave, _Cyber_Wave_Str; float4 _Cyber_Wave_Transform;
        float _UseCyberDMX, _Cyber_DMX_Str; float4 _Cyber_DMX_Transform;
        float _UseCyberAuto; float4 _Cyber_Auto_Transform;

        float _UseAudioLink, _AL_ColorMode, _UseMediaState, _AL_Waveform_Mod, _AL_AutoCorr_Mod;
        float _AL_Strip_Pos, _AL_Chrono_Idx, _UseChronoFX;
        float _AL_DFT_Note, _AL_DFT_Mod;
        float _AL_Band_Emission, _AL_Band_Scanlines, _AL_Band_Film, _AL_Band_Parallax, _AL_Band_Shatter, _AL_Band_Glitch;
        float _AL_Emis_Mod, _AL_Col_Blend, _AL_Scanlines, _AL_Scan_Density, _AL_Scan_Speed, _AL_Scan_React;
        float _AL_Film_Mod, _AL_Paralx_Mod, _AL_CC_Shatter, _AL_Glitch_Mod;

        int _DMX_Channel; float _UseVRSL, _VRSL_Intensity, _VRSL_Geo_Warp, _VRSL_Color_Hijack;
        uniform sampler2D _Udon_DMXGridRenderTexture;
        uniform float4 _Udon_DMXGridRenderTexture_TexelSize;
        // _Udon_DMXGridStrobeOutput dropped - declared but never sampled in this shader, just consumed a sampler register.
        uniform sampler2D _Udon_DMXGridRenderTextureMovement;
        uniform float _MediaPlaying;

        // HELPERS
        float FetchVRSLChannel(uint absoluteChannel, sampler2D tex, float4 texelSize)
        {
            uint universe = ceil((float)absoluteChannel / 512.0);
            int targetColor = floor((float)(universe - 1) / 3.0);
            uint uZero = universe - 1;
            uint channelLocal = (targetColor > 0) ? absoluteChannel - (((uZero - (uZero % 3)) * 512)) - (targetColor * 24) : absoluteChannel;
            uint x = channelLocal % 13; x = (x == 0) ? 13 : x;
            float y = (float)channelLocal / 13.0; y = (frac(y) == 0.0) ? y - 1.0 : floor(y);

            if(x == 13.0) {
                y = (channelLocal >= 90 && channelLocal <= 101) ? y - 1.0 : y;
                y = (channelLocal >= 160 && channelLocal <= 205) ? y - 1.0 : y;
                y = (channelLocal >= 326 && channelLocal <= 404) ? y - 1.0 : y;
                y = (channelLocal >= 676 && channelLocal <= 819) ? y - 1.0 : y;
                y = (channelLocal >= 1339) ? y - 1.0 : y;
            }

            float resMultiplierX = (texelSize.z / 13.0);
            float2 xyUV = float2(((x * resMultiplierX) * texelSize.x) - 0.015, (((y + 1.0) * resMultiplierX) * texelSize.y) - 0.001915);
            float4 sampleData = tex2Dlod(tex, float4(xyUV, 0, 0));

            float value = sampleData.r;
            if (targetColor == 1) value = sampleData.g;
            if (targetColor == 2) value = sampleData.b;
            return value;
        }

        float2 TransformHUD(float2 uv, float4 transform)
        {
            float2 outUV = uv - 0.5 - transform.xy;
            outUV /= max(0.001, transform.z);
            float rad = transform.w * (UNITY_PI / 180.0);
            float s = sin(rad), c = cos(rad);
            outUV = mul(outUV, float2x2(c, -s, s, c));
            return outUV + 0.5;
        }

        float2 TransformUV(float2 uv, float4 trans)
        {
            float2 outUV = uv - 0.5 - trans.xy;
            outUV /= max(0.001, trans.z);
            float rad = trans.w * (UNITY_PI / 180.0);
            float s = sin(rad), c = cos(rad);
            outUV = mul(outUV, float2x2(c, -s, s, c));
            return outUV + 0.5;
        }

        float2 RotateUVDeg(float2 uv, float deg)
        {
            float rad = deg * (UNITY_PI / 180.0);
            float s = sin(rad), c = cos(rad);
            float2 centered = uv - 0.5;
            return mul(centered, float2x2(c, -s, s, c)) + 0.5;
        }

        // Hue (0..1) to RGB - cheap triangle-wave approximation, no HSV stack required.
        inline float3 HUEtoRGB(float h)
        {
            h = frac(h);
            float r = abs(h * 6.0 - 3.0) - 1.0;
            float g = 2.0 - abs(h * 6.0 - 2.0);
            float b = 2.0 - abs(h * 6.0 - 4.0);
            return saturate(float3(r, g, b));
        }

        float4 tessEdge(appdata_full v0, appdata_full v1, appdata_full v2)
        {
            return UnityEdgeLengthBasedTess(v0.vertex, v1.vertex, v2.vertex, _Tess_Edge);
        }

        // Poiyomi-style packed PBR channel picker. Channel index: 0=R, 1=G, 2=B, 3=A.
        inline float ChannelPick(fixed4 packed, float ch)
        {
            return (ch < 0.5) ? packed.r
                 : (ch < 1.5) ? packed.g
                 : (ch < 2.5) ? packed.b
                 :              packed.a;
        }

        // Returns true if AudioLink should be considered active for this frame.
        bool AL_Active()
        {
            if (_UseAudioLink < 0.5) return false;
            if (_UseMediaState > 0.5 && _MediaPlaying < 0.5) return false;
            return true;
        }

        void FetchAudioLinkBands(out float4 amps, out float4 chronos, out float4 al_color, out float raw_waveform, out float autoCorr, float2 uv)
        {
            amps = float4(0,0,0,0);
            chronos = float4(0,0,0,0);
            al_color = _EmissionColor;
            raw_waveform = 0.0;
            autoCorr = 0.0;

            if (AudioLinkIsAvailable())
            {
                float4 al_amps = float4(0,0,0,0);
                al_amps.x = AudioLinkData(ALPASS_AUDIOLINK + int2(0, 0)).r;
                al_amps.y = AudioLinkData(ALPASS_AUDIOLINK + int2(0, 1)).r;
                al_amps.z = AudioLinkData(ALPASS_AUDIOLINK + int2(0, 2)).r;
                al_amps.w = AudioLinkData(ALPASS_AUDIOLINK + int2(0, 3)).r;

                // stronger mapping for visible reaction
                amps.x = saturate(pow(al_amps.x * 4.0, 0.35));
                amps.y = saturate(pow(al_amps.y * 4.0, 0.35));
                amps.z = saturate(pow(al_amps.z * 4.0, 0.35));
                amps.w = saturate(pow(al_amps.w * 4.0, 0.35));

                // Chronotensity is opt-in via _UseChronoFX to avoid 4 extra texture samples for amplitude-only users.
                if (_UseChronoFX > 0.5)
                {
                    uint cIdx = (uint)_AL_Chrono_Idx;
                    chronos.x = AudioLinkGetChronoTime(cIdx, 0);
                    chronos.y = AudioLinkGetChronoTime(cIdx, 1);
                    chronos.z = AudioLinkGetChronoTime(cIdx, 2);
                    chronos.w = AudioLinkGetChronoTime(cIdx, 3);
                }

                int colorMode = (int)_AL_ColorMode;
                // CCCOLORS index 0 is always black, so band → note is offset by +1.
                if (colorMode == 1)
                    al_color = AudioLinkData(ALPASS_CCCOLORS + int2(((int)_AL_Band_Emission % 11) + 1, 0));
                // Theme 0..3 live at uint2(0..3, 23), not CCCOLORS row+1.
                else if (colorMode >= 2 && colorMode <= 5)
                    al_color = AudioLinkData(ALPASS_THEME_COLOR0 + int2(colorMode - 2, 0));
                else if (colorMode == 6)
                    al_color = AudioLinkData(ALPASS_CCSTRIP + int2((int)(saturate(_AL_Strip_Pos) * 127.0), 0));

                float wavePhase = frac(uv.y * 2.0 - _Time.y * 0.2);
                raw_waveform = AudioLinkData(ALPASS_WAVEFORM + int2((int)(wavePhase * 128.0), 0)).r - 0.5;
                autoCorr = AudioLinkData(ALPASS_AUTOCORRELATOR + int2((int)(frac(uv.x) * 128.0), 0)).r;
            }

            // Respect media state: when enabled, mute effects if media is NOT playing
            if (_UseMediaState > 0.5 && _MediaPlaying < 0.5)
            {
                amps = float4(0,0,0,0);
                chronos = float4(0,0,0,0);
                al_color = _EmissionColor;
                raw_waveform = 0.0;
                autoCorr = 0.0;
            }
        }

        // Vertex displacement + AudioLink-driven pump/fracture/autocorrelator.
        void disp(inout appdata_full v)
        {
            float2 uv = v.texcoord.xy;

            // Base displacement from packed PBR map (channel chosen by _PBR_Height_Ch for Poiyomi-pack compat).
            float dispHeight = ChannelPick(tex2Dlod(_MetallicGlossMap, float4(uv, 0, 0)), _PBR_Height_Ch);
            float d = dispHeight * _Disp_Str;

            // VRSL geometric warp
            #if defined(VRSL_ENABLE)
            if (_UseVRSL > 0.5)
            {
                uint dmxBase = (uint)_DMX_Channel;
                float pan  = FetchVRSLChannel(dmxBase + 1, _Udon_DMXGridRenderTextureMovement, _Udon_DMXGridRenderTexture_TexelSize) * 2.0 - 1.0;
                float tilt = FetchVRSLChannel(dmxBase + 2, _Udon_DMXGridRenderTextureMovement, _Udon_DMXGridRenderTexture_TexelSize) * 2.0 - 1.0;
                float dmxWarp = (pan * v.normal.x + tilt * v.normal.z) * _VRSL_Geo_Warp * 0.05;
                d += dmxWarp * dispHeight * _VRSL_Intensity;
            }
            #endif

            // AudioLink-driven pump + fracture (runtime-gated so VRCFury toggle controls activation) - all vertex effects masked by _UseVtxKinetic so sliders alone do nothing without the master toggle.
            if (_UseAudioLink > 0.5 && _UseVtxKinetic > 0.5)
            {
                // Fetch AudioLink bands for this vertex UV
                float4 amps; float4 chronos; float4 al_color; float raw_wave; float autoCorr;
                FetchAudioLinkBands(amps, chronos, al_color, raw_wave, autoCorr, uv);

                // Vertex pump (inflate along normal)
                if (_Vtx_Pump_Str > 0.0001)
                {
                    int pumpBand = (int)_Vtx_Pump_Band;
                    float pumpAmp = (pumpBand == 0) ? amps.x : (pumpBand == 1) ? amps.y : (pumpBand == 2) ? amps.z : amps.w;
                    v.vertex.xyz += v.normal * (pumpAmp * _Vtx_Pump_Str);
                }

                // Spherical autocorrelator ripple (object-space coords) - only fires with live AL data, never falls back to a static slider value.
                if (_Vtx_AutoCorr_Str > 0.0001 && AudioLinkIsAvailable())
                {
                    float ac = AudioLinkGetSphericalMappedAutoCorrelatorValue(normalize(v.vertex.xyz));
                    v.vertex.xyz += v.normal * (ac - 0.6) * _Vtx_AutoCorr_Str * 0.1;
                }

                // Vertex fracture: scatter + pivot+rotate. Driven strictly by AL band amplitude - no manual fallback so the avatar isn't shattered in silent worlds.
                if (_Vtx_Fracture_Str > 0.0001)
                {
                    int fracBand = (int)_Vtx_Fracture_Band;
                    float fracAmp = (fracBand == 0) ? amps.x : (fracBand == 1) ? amps.y : (fracBand == 2) ? amps.z : amps.w;
                    fracAmp *= _Vtx_Fracture_Str;

                    if (fracAmp > 0.0001)
                    {
                        // Snap to a 3D grid so same-chunk verts hash identically and move together under tessellation.
                        float3 cell = floor(v.vertex.xyz * 25.0); // 25.0 controls physical shard size
                        float hash = frac(sin(dot(cell, float3(12.9898,78.233,37.719))) * 43758.5453);

                        float3 randDir = normalize(float3(frac(hash * 1.0) * 2.0 - 1.0, frac(hash * 1.37) * 2.0 - 1.0, frac(hash * 3.11) * 2.0 - 1.0));
                        float rotSeed = frac(hash * 7.13);

                        float scatter = fracAmp * 0.06;
                        float3 pivotOffset = v.normal * (0.02 + fracAmp * 0.02);
                        float3 pivot = v.vertex.xyz - pivotOffset;

                        // rotation around random axis (Rodrigues)
                        float angle = rotSeed * fracAmp * 6.2831853;
                        float s = sin(angle), c = cos(angle);
                        float3 axis = normalize(randDir + 0.0001);
                        float3 rel = v.vertex.xyz - pivot;
                        float3 relRot = rel * c + cross(axis, rel) * s + axis * dot(axis, rel) * (1.0 - c);
                        v.vertex.xyz = pivot + relRot;

                        // scatter and subtle scale
                        v.vertex.xyz += randDir * scatter;
                        float scale = 1.0 + fracAmp * 0.08;
                        v.vertex.xyz = pivot + (v.vertex.xyz - pivot) * scale;
                    }
                }
            }

            // Static displacement
            v.vertex.xyz += v.normal * d;
        }

        // PBR HELPERS
        float2 ParallaxRaymarching(float2 uv, float3 viewDirTangent, float parallaxDepth)
        {
            // Early-out when depth ~= 0 - otherwise the loop below re-samples the same texel up to 50 times (stepUVOffset collapses to zero) and exits only when the heightmap value rises above the descending layer height, burning ~35 tex2Dgrad samples per pixel on any non-white surface map.
            [branch] if (parallaxDepth < 1e-4) return uv;
            float parallaxLimit = -length(viewDirTangent.xy) / max(viewDirTangent.z, 0.001);
            parallaxLimit *= parallaxDepth;
            float2 vOffsetDir = normalize(viewDirTangent.xy);
            float2 vMaxOffset = vOffsetDir * parallaxLimit;
            int numSteps = (int)lerp(48.0, 8.0, max(viewDirTangent.z, 0.0));
            float stepSize = 1.0 / (float)numSteps;
            float2 dx = ddx(uv); float2 dy = ddy(uv);

            float currentLayerHeight = 1.0; float2 currentUVOffset = 0.0;
            float2 stepUVOffset = vMaxOffset * stepSize;
            float currentMapHeight = ChannelPick(tex2Dgrad(_MetallicGlossMap, uv, dx, dy), _PBR_Height_Ch);

            UNITY_LOOP
            for(int i = 0; i < 50; i++)
            {
                if (currentMapHeight >= currentLayerHeight) break;
                currentLayerHeight -= stepSize;
                currentUVOffset += stepUVOffset;
                currentMapHeight = ChannelPick(tex2Dgrad(_MetallicGlossMap, uv + currentUVOffset, dx, dy), _PBR_Height_Ch);
            }

            float prevLayerHeight = currentLayerHeight + stepSize;
            float prevMapHeight = ChannelPick(tex2Dgrad(_MetallicGlossMap, uv + currentUVOffset - stepUVOffset, dx, dy), _PBR_Height_Ch);
            float weight = (currentLayerHeight - currentMapHeight) /
                           max((currentLayerHeight - currentMapHeight) + (prevMapHeight - prevLayerHeight), 0.0001);
            return uv + (currentUVOffset - stepUVOffset * weight);
        }

        inline half3 GTAOMultiBounce(half visibility, half3 albedo)
        {
            half3 a =  2.0404 * albedo - 0.3324;
            half3 b = -4.7951 * albedo + 0.6417;
            half3 c =  2.7552 * albedo + 0.6903;
            return max(visibility, ((visibility * a + b) * visibility + c) * visibility);
        }

        inline half HDRPSpecularOcclusion(half NdotV, half AO, half roughness)
        {
            return saturate(pow(max(AO, 0.01), exp2(-roughness)) * (NdotV + AO));
        }

        // Geometric specular AA - Toksvig-style filtering on screen-space normal derivative variance.
        inline half GeometricSpecAA(float3 worldNormal, half roughness, half strength)
        {
            if (strength <= 0.0001) return roughness;
            float3 dndu = ddx(worldNormal);
            float3 dndv = ddy(worldNormal);
            float variance = dot(dndu, dndu) + dot(dndv, dndv);
            float kernelRoughness2 = saturate(variance * 2.0 * strength);
            float r2 = roughness * roughness + kernelRoughness2;
            return sqrt(saturate(r2));
        }

        // GGX BRDF HELPERS: D=Trowbridge-Reitz, V=Smith Joint, F=Schlick, Diffuse=Burley, Indirect=Karis split-sum, MS=Filament.
        inline float D_GGX(float NdotH, float a2)
        {
            float d = NdotH * NdotH * (a2 - 1.0) + 1.0;
            return a2 / max(UNITY_PI * d * d, 1e-7);
        }

        inline float V_SmithJointGGX(float NdotL, float NdotV, float a2)
        {
            float lambdaV = NdotL * sqrt(NdotV * NdotV * (1.0 - a2) + a2);
            float lambdaL = NdotV * sqrt(NdotL * NdotL * (1.0 - a2) + a2);
            return 0.5 / max(lambdaV + lambdaL, 1e-7);
        }

        // Anisotropic GGX (Burley 2012)
        inline float D_GGX_Aniso(float NdotH, float TdotH, float BdotH, float ax, float ay)
        {
            float a2 = ax * ay;
            float3 v = float3(ay * TdotH, ax * BdotH, a2 * NdotH);
            float v2 = dot(v, v);
            float w2 = a2 / max(v2, 1e-7);
            return a2 * w2 * w2 / UNITY_PI;
        }

        inline float V_SmithJointGGX_Aniso(
            float NdotL, float NdotV,
            float TdotV, float BdotV,
            float TdotL, float BdotL,
            float ax, float ay)
        {
            float lambdaV = NdotL * length(float3(ax * TdotV, ay * BdotV, NdotV));
            float lambdaL = NdotV * length(float3(ax * TdotL, ay * BdotL, NdotL));
            return 0.5 / max(lambdaV + lambdaL, 1e-7);
        }

        inline float3 F_Schlick(float u, float3 F0)
        {
            float p = pow(saturate(1.0 - u), 5.0);
            return F0 + (1.0 - F0) * p;
        }

        // Burley/Disney diffuse. Returns scalar (caller multiplies by NdotL and color).
        inline float Burley_Diffuse(float NdotV, float NdotL, float LdotH, float roughness)
        {
            float fd90 = 0.5 + 2.0 * LdotH * LdotH * roughness;
            float lightScatter = 1.0 + (fd90 - 1.0) * pow(saturate(1.0 - NdotL), 5.0);
            float viewScatter  = 1.0 + (fd90 - 1.0) * pow(saturate(1.0 - NdotV), 5.0);
            return lightScatter * viewScatter * UNITY_INV_PI;
        }

        // Karis split-sum env BRDF: AB.x = F0 scale, AB.y = bias; env_brdf = F0*AB.x + AB.y.
        inline float2 EnvBRDFApprox_AB(float roughness, float NdotV)
        {
            const float4 c0 = float4(-1.0, -0.0275, -0.572,  0.022);
            const float4 c1 = float4( 1.0,  0.0425,  1.040, -0.040);
            float4 r = roughness * c0 + c1;
            float a004 = min(r.x * r.x, exp2(-9.28 * NdotV)) * r.x + r.y;
            return float2(-1.04, 1.04) * a004 + r.zw;
        }

        inline float3 EnvBRDFApprox(float3 F0, float roughness, float NdotV)
        {
            float2 AB = EnvBRDFApprox_AB(roughness, NdotV);
            return F0 * AB.x + AB.y;
        }

        // Filament/Frostbite multi-scatter compensation. Returns 1 + F0*((1-E)/E), E≈dfg_AB.x+dfg_AB.y.
        inline float3 EnergyCompensation(float3 F0, float2 dfg_AB)
        {
            float E = dfg_AB.x + dfg_AB.y;
            return 1.0 + F0 * (1.0 / max(E, 1e-3) - 1.0);
        }

        // BRDF: GGX base + clearcoat, optional anisotropy/MS-compensation, Burley diffuse/transmission/SSS, parallax shadow, thin film, rim, LTCGI, matcap.
        half4 BRDF_Latex_GGX(
            half3 diffColor,
            half3 matcap,
            half3 specColor,
            half oneMinusReflectivity,
            SurfaceOutputStandardLatex s,
            float3 viewDir,
            UnityLight light,
            UnityIndirect gi,
            half3 clearcoatEnv)
        {
            float3 L = light.dir;
            float3 V = viewDir;
            float3 N = s.Normal;
            float3 Nc = s.ClearcoatNormal;
            float3 H = Unity_SafeNormalize(L + V);

            half NdotL  = saturate(dot(N,  L));
            half NdotV  = abs(dot(N, V)) + 1e-5;
            half NdotH  = saturate(dot(N, H));
            float LdotH = saturate(dot(L, H));
            half NcL    = saturate(dot(Nc, L));
            half NcV    = abs(dot(Nc, V)) + 1e-5;
            half NcH    = saturate(dot(Nc, H));

            half rawAO         = s.Occlusion;
            half3 multiBounceAO = GTAOMultiBounce(rawAO, diffColor);

            // Geometric specular AA: roughens normals based on screen-space variance.
            half aBase   = GeometricSpecAA(N,  s.BaseRoughness, s.SpecAA);
            half ccRough = max(1.0 - s.ClearcoatSmoothness, 0.0089);
            ccRough      = GeometricSpecAA(Nc, ccRough, s.SpecAA);

            // Roughness squared (alpha2) - used in GGX D/V.
            half a2_base = max(aBase   * aBase,   1e-5);
            half a2_cc   = max(ccRough * ccRough, 1e-5);

            half baseSpecOcc = lerp(1.0, HDRPSpecularOcclusion(NdotV, rawAO, aBase),   _Spec_Occ);
            half ccSpecOcc   = lerp(1.0, HDRPSpecularOcclusion(NcV,   rawAO, ccRough), _Spec_Occ);

            // Thin film (Schlick base reflectance, wavelength-dependent phase).
            half3 thinFilmColor = 1.0;
            if (s.ThinFilmStrength > 0.001)
            {
                half cosTheta2 = sqrt(1.0 - 0.44 * (1.0 - NcV * NcV));
                half nmThickness = lerp(100.0, 2000.0, s.ThinFilmThickness);
                half pathLength = 2.0 * nmThickness * cosTheta2;
                half3 phase = (2.0 * UNITY_PI * pathLength) / half3(650.0, 510.0, 475.0);
                half3 iridescence = saturate(cos(phase) * 0.5 + 0.5) * 2.0;
                thinFilmColor = lerp(1.0, iridescence, s.ThinFilmStrength);
            }

            // Parallax shadowing (POM-coupled self-shadowing) - gated on ParallaxDepth so a bound surface map with parallax disabled skips the tex2Dlod entirely.
            float shadowTrace = 1.0;
            if (NdotL > 0.0 && s.ParallaxDepth > 1e-4)
            {
                float3 lightDirTangent = mul(s.WorldToTangent, L);
                float2 lightDirUV = lightDirTangent.xy * s.ParallaxDepth;
                float shadowHeight = ChannelPick(tex2Dlod(_MetallicGlossMap, float4(s.UV + lightDirUV, 0, 0)), _PBR_Height_Ch);
                shadowTrace = saturate(1.0 - (s.Height - shadowHeight) * _Shad_Hard);
            }

            // Tinted dielectric clearcoat - white tint at F0=0.04 reproduces standard dielectric exactly.
            half3 ccF0      = _CC_F0 * _CC_Tint.rgb;
            half3 ccFresEnv = (ccF0 + (1.0 - ccF0) * pow(saturate(1.0 - NcV),  5.0)) * s.ClearcoatStrength;
            half3 ccFresDir = (ccF0 + (1.0 - ccF0) * pow(saturate(1.0 - LdotH),5.0)) * s.ClearcoatStrength;

            // Per-channel base attenuation; with a tinted coat this gives the under-layer a complementary cast.
            half3 baseEnergy = 1.0 - ccFresEnv;

            // BASE LAYER - direct specular (GGX, optionally anisotropic)
            float D_base;
            float V_base;

            half aniso = clamp(s.Anisotropy, -0.95, 0.95);
            if (abs(aniso) > 0.005)
            {
                // Rotate world tangent by AnisoRotation around N to align with stretch direction.
                float3 worldTangent   = s.WorldToTangent[0];
                float3 worldBitangent = s.WorldToTangent[1];
                float rad = s.AnisoRotation * (UNITY_PI / 180.0);
                float sR = sin(rad), cR = cos(rad);
                float3 T = normalize(worldTangent * cR + worldBitangent * sR);
                float3 B = normalize(cross(N, T));

                // Anisotropic alpha split (Burley) - pass aBase, not a2_base; D_GGX_Aniso squares internally.
                float ax = max(aBase * (1.0 + aniso), 1e-4);
                float ay = max(aBase * (1.0 - aniso), 1e-4);

                float TdotH = dot(T, H);
                float BdotH = dot(B, H);
                float TdotV = dot(T, V);
                float BdotV = dot(B, V);
                float TdotL = dot(T, L);
                float BdotL = dot(B, L);

                D_base = D_GGX_Aniso(NdotH, TdotH, BdotH, ax, ay);
                V_base = V_SmithJointGGX_Aniso(NdotL, NdotV, TdotV, BdotV, TdotL, BdotL, ax, ay);
            }
            else
            {
                D_base = D_GGX(NdotH, a2_base);
                V_base = V_SmithJointGGX(NdotL, NdotV, a2_base);
            }

            half3 F_base       = F_Schlick(LdotH, specColor);
            half3 baseSpecular = D_base * V_base * F_base * light.color * NdotL * baseEnergy * shadowTrace;

            // BASE LAYER - direct diffuse (Burley)
            float burley     = Burley_Diffuse(NdotV, NdotL, LdotH, aBase);
            half3 baseDiffuse = diffColor * light.color * NdotL * burley * UNITY_PI * baseEnergy;

            // CLEARCOAT - direct specular (GGX isotropic)
            float D_cc = D_GGX(NcH, a2_cc);
            float V_cc = V_SmithJointGGX(NcL, NcV, a2_cc);
            half3 ccSpecular = D_cc * V_cc * ccFresDir * thinFilmColor *
                               light.color * NcL * shadowTrace;

            // SSS - wrap + back-scatter
            float wrap = saturate((NdotL + _SSS_Dist) / max(1e-5, 1.0 + _SSS_Dist));
            float back = pow(saturate(dot(V, -L)), _SSS_Power);
            float sssTerm = (wrap * 0.6 + back * 0.4) * _SSS_Str;
            half3 absorption = 1.0 - diffColor;
            half3 sssProfile = exp2(-s.Thickness * absorption * 4.0);
            half3 sssColor = diffColor * light.color * sssTerm * sssProfile * s.Thickness;

            // Transmission - back-light through thin parts (Burley/Filament)
            half3 transmission = 0;
            if (s.Transmission > 0.001)
            {
                float invNdotL = saturate(dot(-N, L)); // back-side illumination via flipped normal
                half3 transTint = exp(-(1.0 - diffColor) * (1.0 / max(_Trans_Dist, 1e-3))); // Beer-Lambert absorption
                float vFall = pow(saturate(dot(V, -L)), _Trans_Power); // view-aligned back-light falloff
                transmission = light.color * diffColor * transTint *
                               (invNdotL * 0.5 + vFall * 0.5) *
                               s.Transmission * baseEnergy;
            }

            // Rim - fake atmospheric edge
            half rimExponent = lerp(30.0, 0.1, saturate(_Rim_Power / 10.0));
            half rim = pow(saturate(1.0 - NcV), rimExponent) * _Rim_Str *
                       saturate(_Rim_Power * 100.0);
            half3 rimColor = rim * diffColor * (gi.diffuse + 0.1);

            // Indirect - Karis split-sum env BRDF. gi.specular is raw IBL (no Fresnel); we multiply F here.
            float2 dfg_base = EnvBRDFApprox_AB(aBase,   NdotV);
            float2 dfg_cc   = EnvBRDFApprox_AB(ccRough, NcV);
            half3 envBRDF_base = specColor * dfg_base.x + dfg_base.y;
            half3 envBRDF_cc   = (ccF0    * dfg_cc.x   + dfg_cc.y) * s.ClearcoatStrength;

            // Multi-scatter compensation (Filament). Skipped when toggle off.
            half3 baseMS = 1.0;
            if (_UseMultiScatter > 0.5)
            {
                baseMS = EnergyCompensation(specColor, dfg_base);
                baseSpecular *= baseMS;
            }

            // Indirect base specular (energy-attenuated by clearcoat).
            half3 indirectBaseSpec = gi.specular * envBRDF_base * baseEnergy * baseSpecOcc * baseMS;

            // Indirect clearcoat specular (uses its own roughness-mip env color).
            half3 indirectCCSpec = clearcoatEnv * envBRDF_cc * thinFilmColor * ccSpecOcc;

            // Combine
            half3 finalColor =
                gi.diffuse * diffColor * baseEnergy * multiBounceAO +   // indirect diffuse
                baseDiffuse +                                            // direct diffuse (Burley)
                sssColor +
                transmission +
                baseSpecular +
                ccSpecular +
                indirectBaseSpec +
                indirectCCSpec +
                s.LVSpec * baseEnergy * baseSpecOcc +
                s.LVCCSpec * s.ClearcoatStrength * thinFilmColor * ccSpecOcc +
                rimColor;

            // LTCGI (area lights)
            #if defined(LTCGI_ENABLE)
            {
                half3 base_ltc_d = 0, base_ltc_s = 0;
                float base_sIntensity = 0, base_dIntensity = 0;
                LTCGI_Contribution(s.WorldPos, N, V, aBase, float2(0,0), base_ltc_d, base_ltc_s, base_sIntensity, base_dIntensity);

                half3 cc_ltc_d = 0, cc_ltc_s = 0;
                float cc_sIntensity = 0, cc_dIntensity = 0;
                LTCGI_Contribution(s.WorldPos, Nc, V, ccRough, float2(0,0), cc_ltc_d, cc_ltc_s, cc_sIntensity, cc_dIntensity);

                half3 ltcgiDiff     = base_ltc_d * diffColor * baseEnergy * multiBounceAO * _LTCGI_Diff_Mix;
                half3 ltcgiBaseSpec = base_ltc_s * specColor * baseEnergy * baseSpecOcc * _LTCGI_Spec_Mix * baseMS;
                half3 ltcgiCCSpec   = cc_ltc_s * ccFresEnv * thinFilmColor * ccSpecOcc * _LTCGI_Spec_Mix;

                finalColor += (ltcgiDiff + ltcgiBaseSpec + ltcgiCCSpec) * _LTCGI_Int;
            }
            #endif

            // Matcap
            half3 matcapEval = matcap * saturate(gi.diffuse + light.color * smoothstep(0.0, 0.15, NcL)) * baseSpecOcc;
            finalColor = lerp(finalColor, finalColor + matcapEval, _MatCap_Lit);

            // Emission + AL neon overlay
            finalColor += s.Emission * _Emis_Exp;

            return half4(max(finalColor, diffColor * _MinBrightness), 1);
        }

        void LightingStandardLatex_GI(SurfaceOutputStandardLatex s, UnityGIInput data, inout UnityGI gi)
        {
            // Same mirror-camera fix as LightingStandardLatex - UnityGIInput.worldViewDir was filled from _WorldSpaceCameraPos and drives the indirect specular reflection direction below.
            data.worldViewDir = vw_WorldViewDir(s.WorldPos);

            gi = UnityGI_Base(data, 1.0, s.Normal);

            // Light Volume diffuse (pre-baked into s.LVDiffuse in surf) - Additive mode ADDs to Unity's probe diffuse (volumes layer on top); Full/deringed mode REPLACES it (LV is the authoritative SH source).
            if (s.LVActive > 0.5)
            {
                if (_LV_AdditiveOnly > 0.5)
                    gi.indirect.diffuse += s.LVDiffuse * _LV_Int;
                else
                    gi.indirect.diffuse = lerp(gi.indirect.diffuse, s.LVDiffuse, _LV_Int);
            }

            // Roughness-blurred IBL (no Fresnel - applied per-layer in BRDF). Occlusion=1 here; specOcc is per-layer.
            Unity_GlossyEnvironmentData g =
                UnityGlossyEnvironmentSetup(s.Smoothness, data.worldViewDir, s.Normal,
                                            lerp(unity_ColorSpaceDielectricSpec.rgb, s.Albedo, s.Metallic));
            gi.indirect.specular = UnityGI_IndirectSpecular(data, 1.0, g);
        }

        inline half4 LightingStandardLatex(SurfaceOutputStandardLatex s, half3 viewDir, UnityGI gi)
        {
            // Unity's surface-shader plumbing computes incoming viewDir from _WorldSpaceCameraPos in the generated vertex stage (wrong in VRChat mirrors); reproject from the actual rendering camera so clearcoat reflections and BRDF NdotV are correct.
            viewDir = vw_WorldViewDir(s.WorldPos);

            float3 reflDir = reflect(-viewDir, s.ClearcoatNormal);
            #if UNITY_SPECCUBE_BOX_PROJECTION
                reflDir = BoxProjectedCubemapDirection(reflDir, s.WorldPos, unity_SpecCube0_ProbePosition, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
            #endif

            half mip = (1.0 - s.ClearcoatSmoothness) * 7.0;
            half3 clearcoatEnv = DecodeHDR(UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflDir, mip), unity_SpecCube0_HDR);

            half oneMinusReflectivity;
            half3 specColor;
            s.Albedo = DiffuseAndSpecularFromMetallic(s.Albedo, s.Metallic, specColor, oneMinusReflectivity);

            // Alpha workflow branches by mode keyword - Opaque+Cutout force outputAlpha=1 (SubShader Blend is One/Zero so value would be discarded, but explicit avoids surprises); Fade uses straight alpha (SrcAlpha/OneMinusSrcAlpha); Transparent uses Unity's PreMultiplyAlpha so specular survives at low opacity.
            half outputAlpha = 1.0;
            #if defined(_ALPHAPREMULTIPLY_ON)
                s.Albedo = PreMultiplyAlpha(s.Albedo, s.Alpha, oneMinusReflectivity, outputAlpha);
            #elif defined(_ALPHABLEND_ON)
                outputAlpha = s.Alpha;
            #endif

            half4 c = BRDF_Latex_GGX(s.Albedo, s.Matcap, specColor, oneMinusReflectivity, s, viewDir, gi.light, gi.indirect, clearcoatEnv);
            c.a = outputAlpha;
            return c;
        }

        // Safe vector indexing macro to bypass HLSL arrayification bugs
        #define GET_AL_BAND(vec, bandIdx) ( \
            ((int)(bandIdx) == 0) ? vec.x : \
            ((int)(bandIdx) == 1) ? vec.y : \
            ((int)(bandIdx) == 2) ? vec.z : \
            vec.w )

        // SURFACE FUNCTION
        void surf (Input IN, inout SurfaceOutputStandardLatex o)
        {
            o.WorldPos = IN.worldPos;

            float3 tangentWorld   = WorldNormalVector(IN, float3(1,0,0));
            float3 bitangentWorld = WorldNormalVector(IN, float3(0,1,0));
            float3 normalWorld    = WorldNormalVector(IN, float3(0,0,1));
            o.WorldToTangent = float3x3(tangentWorld, bitangentWorld, normalWorld);

            // Animation time stays on real time; chronotensity is opt-in per FX via _UseChronoFX.
            float animTime = _Time.y;

            float2 baseUV = IN.uv_MainTex;

            // AudioLink bands (zeroed by default; FetchAudioLinkBands only runs when the master toggle is on).
            float4 amps = float4(0,0,0,0);
            float4 chronos = float4(0,0,0,0);
            float4 al_color = _EmissionColor;
            float raw_waveform = 0.0;
            float autoCorr = 0.0;

            if (_UseAudioLink > 0.5)
            {
                FetchAudioLinkBands(amps, chronos, al_color, raw_waveform, autoCorr, baseUV);
            }

            // DFT note pull-out (across all octaves), used to bias emission
            float dftAmp = 0.0;
            if (AL_Active() && AudioLinkIsAvailable() && _AL_DFT_Mod > 0.001)
            {
                dftAmp = AudioLinkGetAmplitudesAtNote(clamp(_AL_DFT_Note, 0.0, 11.0)).r;
            }

            float vPump = ((int)_Vtx_Pump_Band == 0) ? amps.x :
                          ((int)_Vtx_Pump_Band == 1) ? amps.y :
                          ((int)_Vtx_Pump_Band == 2) ? amps.z : amps.w;

            float amp_emis  = GET_AL_BAND(amps, _AL_Band_Emission);
            float chr_emis  = GET_AL_BAND(chronos, _AL_Band_Emission);
            float amp_scan  = GET_AL_BAND(amps, _AL_Band_Scanlines);
            float chr_scan  = GET_AL_BAND(chronos, _AL_Band_Scanlines);
            float amp_film  = GET_AL_BAND(amps, _AL_Band_Film);
            float amp_para  = GET_AL_BAND(amps, _AL_Band_Parallax);
            float amp_shat  = GET_AL_BAND(amps, _AL_Band_Shatter);
            float amp_glit  = GET_AL_BAND(amps, _AL_Band_Glitch);
            float chr_glit  = GET_AL_BAND(chronos, _AL_Band_Glitch);

            // Standard time-driven UV scroll (chronotensity drive removed - was unpredictable).
            baseUV += float2(_SpeedX, _SpeedY) * _Time.y;

            // Bio pulse
            half heartbeat  = amps.x * 0.65 + amp_emis * 0.35;
            half neuroSpike = amps.w * (0.5 + amp_emis * 0.5);
            half tension    = amps.z * 1.3;
            half bio = saturate(heartbeat * 0.55 + tension * 0.25 + neuroSpike * 0.20);
            bio += sin(fmod(chr_emis, 1.0) * 6.2831) * 0.05;
            bio = saturate(bio);

            // Audio Color Blend cycles AL tint through rainbow (time + bio + worldPos.y). Applied before VRSL hijack.
            if (_UseAudioLink > 0.5 && _AL_Col_Blend > 0.001)
            {
                float hue = frac(_Time.y * 0.2 + bio * 0.5 + IN.worldPos.y * 0.05);
                float3 rainbow = HUEtoRGB(hue);
                al_color.rgb = lerp(al_color.rgb, rainbow, saturate(_AL_Col_Blend));
            }

            // VRSL color hijack (DMX colour wash override for AL color)
            #if defined(VRSL_ENABLE)
            if (_UseVRSL > 0.5 && _VRSL_Color_Hijack > 0.001)
            {
                uint dmxBase = (uint)_DMX_Channel;
                float dr = FetchVRSLChannel(dmxBase + 3, _Udon_DMXGridRenderTexture, _Udon_DMXGridRenderTexture_TexelSize);
                float dg = FetchVRSLChannel(dmxBase + 4, _Udon_DMXGridRenderTexture, _Udon_DMXGridRenderTexture_TexelSize);
                float db = FetchVRSLChannel(dmxBase + 5, _Udon_DMXGridRenderTexture, _Udon_DMXGridRenderTexture_TexelSize);
                float4 dmxColor = float4(dr, dg, db, 1);
                al_color = lerp(al_color, dmxColor, saturate(_VRSL_Color_Hijack * _VRSL_Intensity));
            }
            #endif

            // (Geometry-level primID fracture clip removed - broke under tessellation. Per-pixel noise clip below handles shards.)

            // UV AUDIO DISTORTION CHAIN: vortex → pump → fracture → rotation → glitch tear → parallax (compounding).
            float2 cUV = baseUV;

            // Per-fragment fracture pop mask - read by parallax stage; declared outside AL guard.
            float fracturePop = 0;

            // UV distortion effects all funnel through band amplitudes which are zero when _UseAudioLink is off.
            if (_UseALVortex > 0.5)
            {
                float2 vUV = TransformUV(cUV, _AL_Vortex_UV);
                float2 centered = vUV - 0.5;
                float radius = length(centered);
                float angle = atan2(centered.y, centered.x);
                float bandAmp = GET_AL_BAND(amps, _AL_Vortex_Band);
                // Radial falloff - centre twists hardest. Chrono FX adds an oscillating breath.
                float chronoMod = (_UseChronoFX > 0.5) ? sin(GET_AL_BAND(chronos, _AL_Vortex_Band) * UNITY_PI) : 1.0;
                float twist = (1.0 - saturate(radius * 2.0)) * bandAmp * _AL_Vortex_Str * chronoMod;
                angle += twist;
                cUV = float2(cos(angle), sin(angle)) * radius + 0.5;
            }

            if (_UseALPump > 0.5)
            {
                // Radial scale around pump centre: pump<1 zooms in, pump>1 zooms out.
                float bandAmp = GET_AL_BAND(amps, _AL_Pump_Band);
                float2 pUV = TransformUV(cUV, _AL_Pump_UV);
                float pump = 1.0 - (bandAmp * _AL_Pump_Str);
                cUV = (pUV - 0.5) * pump + 0.5;
            }

            if (_UseALFracture > 0.5)
            {
                float bandAmp = GET_AL_BAND(amps, _AL_Fracture_Band);
                if (bandAmp > 0.05 && _AL_Fracture_Str > 0.001)
                {
                    // Two-axis slice hash advancing with time so shards re-roll instead of locking.
                    float2 fUV = TransformUV(cUV, _AL_Fracture_UV);
                    float fracTime = _Time.y * 4.0;
                    if (_UseChronoFX > 0.5) fracTime += GET_AL_BAND(chronos, _AL_Fracture_Band) * 3.0;
                    float sliceY = floor(fUV.y * 60.0);
                    float sliceX = floor(fUV.x * 60.0);
                    float seedT  = floor(fmod(fracTime, 1000.0) * 17.0);
                    float fracHashX = frac(sin(dot(float3(sliceY, sliceX, seedT), float3(12.9898, 78.233, 37.719))) * 43758.5453);
                    float fracHashY = frac(sin(dot(float3(sliceX, seedT, sliceY), float3(39.346, 11.135, 91.17))) * 43758.5453);
                    float fractureMask = step(0.7, fracHashX) * bandAmp;
                    cUV.x += (fracHashX - 0.5) * _AL_Fracture_Str * 0.18 * fractureMask;
                    cUV.y += (fracHashY - 0.5) * _AL_Fracture_Str * 0.18 * fractureMask;
                    // Shard mask drives a tiny parallax pop (read at o.ParallaxDepth below).
                    fracturePop = fractureMask;
                }
            }

            // UV rotation applied after audio distortions so it composes with vortex/pump. Vortex+ChronoFX adds an audio-driven spin (~8.6 deg/unit).
            float uvRotDeg = _UV_Rot;
            if (_UseALVortex > 0.5 && _UseChronoFX > 0.5)
                uvRotDeg += chr_emis * 8.594;
            if (abs(uvRotDeg) > 0.001)
                cUV = RotateUVDeg(cUV, uvRotDeg);

            // Glitch UV tear - X skews with live waveform, Y micro-wobble reads as VHS tracking.
            float2 glitchOffset = 0;
            if (_AL_Waveform_Mod > 0.001)
            {
                float waveAmp = raw_waveform * _AL_Waveform_Mod;
                glitchOffset.x += waveAmp * 0.35;
                glitchOffset.y += sin(cUV.y * 120.0 + _Time.y * 6.0) * abs(waveAmp) * 0.04;
            }
            if (_AL_Glitch_Mod > 0.001 && neuroSpike > 0.05)
            {
                float glitchTime = _Time.y * 9.0;
                if (_UseChronoFX > 0.5) glitchTime += chr_glit * 4.0;
                float slice = floor(cUV.y * 120.0);
                float glitchSeed = floor(fmod(glitchTime, 1000.0) * 33.0);
                float glitchHash = frac(sin(dot(float2(slice, glitchSeed), float2(12.9898, 78.233))) * 43758.5453);
                float glitchMask = step(0.80, glitchHash) * amp_glit;
                glitchOffset.x += (frac(glitchHash * 41.41) - 0.5) * _AL_Glitch_Mod * 1.2 * glitchMask;
                glitchOffset.y += (frac(glitchHash * 93.93) - 0.5) * _AL_Glitch_Mod * 0.18 * glitchMask;
            }

            // Parallax over audio-distorted UV (fracturePop pushes shards a hair off the surface) - IN.viewDir would derive from _WorldSpaceCameraPos and break parallax in VRChat mirrors; vw_WorldViewDir reads the actual rendering camera via UNITY_MATRIX_I_V instead.
            float3 viewDirWorld   = vw_WorldViewDir(IN.worldPos);
            float3 viewDirTangent = mul(o.WorldToTangent, viewDirWorld);
            o.ParallaxDepth       = _Parallax + amp_para * _AL_Paralx_Mod + fracturePop * _AL_Fracture_Str * 0.025;
            float2 finalUV        = ParallaxRaymarching(cUV + glitchOffset, viewDirTangent, o.ParallaxDepth);

            // Base textures
            fixed4 c      = tex2D(_MainTex, finalUV) * _Color;
            fixed4 packed = tex2D(_MetallicGlossMap, finalUV);

            // Vertex-fracture per-pixel shard clip - also runtime-gated by the AL master toggle so silent worlds don't punch holes.
            if (_UseAudioLink > 0.5 && _UseVtxKinetic > 0.5 && _Vtx_Fracture_Str > 0.001)
            {
                float fractureNoise =
                    frac(
                        sin(
                            dot(finalUV * 512.0,
                                float2(12.9898,78.233))
                        ) * 43758.5453
                    );

                float fractureCut = GET_AL_BAND(amps, _Vtx_Fracture_Band) * _Vtx_Fracture_Str;
                clip(fractureNoise - fractureCut);
            }

            // Alpha workflow - Cutout: hard clip on _CutOff (also clips addshadow so shadows match silhouette); Fade/Transparent: discard fully invisible pixels so the shadow caster doesn't punch opaque shadow holes; Opaque: no clip, alpha ignored.
            #if defined(_ALPHATEST_ON)
                clip(c.a - _CutOff);
            #elif defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
                clip(c.a - 0.001);
            #endif
            o.Alpha = c.a;

            // Poiyomi-style multi-region color mask - RGB zones each multiply a tint into albedo and contribute emission boost later; channels are independent so overlapping zones stack.
            float regionEmis = 0;
            if (_UseRegionMask > 0.5)
            {
                fixed4 regionSample = tex2D(_RegionMask, finalUV);
                // Channels are independent masks (not blended) so authors can paint hard-edged feature zones.
                float3 regionTint = lerp(float3(1,1,1), _Region_R_Tint.rgb, regionSample.r)
                                  * lerp(float3(1,1,1), _Region_G_Tint.rgb, regionSample.g)
                                  * lerp(float3(1,1,1), _Region_B_Tint.rgb, regionSample.b);
                c.rgb *= regionTint;
                regionEmis = regionSample.r * _Region_R_Emis
                           + regionSample.g * _Region_G_Emis
                           + regionSample.b * _Region_B_Emis;
            }

            o.Albedo = c.rgb;

            // Metallic / smoothness with channel-selectable Poiyomi-pack support + AL modulation.
            float pbrMet    = ChannelPick(packed, _PBR_Met_Ch);
            if (_PBR_Met_Inv > 0.5) pbrMet = 1.0 - pbrMet;
            float pbrSmooth = ChannelPick(packed, _PBR_Smooth_Ch);
            if (_PBR_Smooth_Inv > 0.5) pbrSmooth = 1.0 - pbrSmooth;
            o.Metallic      = saturate(pbrMet    + amp_emis * _AL_Emis_Mod * 0.25);
            o.Smoothness    = saturate(pbrSmooth + amp_film * _AL_Film_Mod * 0.25);
            o.BaseRoughness = 1.0 - o.Smoothness;

            // AO (channel selectable)
            float pbrAO = ChannelPick(packed, _PBR_AO_Ch);
            o.Occlusion = saturate(pbrAO * _AO_Str);
            if (_AL_Scanlines > 0.0 && _UseAudioLink > 0.5)
                o.Occlusion = lerp(o.Occlusion, 1.0, amp_scan * 0.2);

            // Height (channel selectable; parallax raymarch and BRDF shadow trace use the same channel).
            float pbrHeight = ChannelPick(packed, _PBR_Height_Ch);
            o.Height = pbrHeight * _Disp_Str;

            // Normals
            float3 normalTS = UnpackNormal(tex2D(_BumpMap, finalUV));
            normalTS = normalize(lerp(float3(0,0,1), normalTS, _Norm_Str));
            #if defined(_DETAIL_NORMAL)
            {
                float2 detUV = finalUV * _Det_UV_Tiling;
                float3 detN  = UnpackNormal(tex2D(_DetailNormalMap, detUV));
                normalTS = normalize(lerp(normalTS, detN, _Det_Strength));
            }
            #endif
            o.Normal = normalTS;

            // Clearcoat + thin film with AL modulation
            o.ClearcoatStrength   = saturate(_CC_Strength + amp_shat * _AL_CC_Shatter);
            o.ClearcoatSmoothness = saturate(_CC_Smoothness + amp_film * _AL_Film_Mod * 0.5);
            o.ThinFilmStrength    = saturate(_Film_Str + amp_film * _AL_Film_Mod);
            o.ThinFilmThickness   = _Film_Thick;
            o.SpecAA              = saturate(_CC_Spec_AA);

            // Thickness (SSS) from bio pulse
            o.Thickness = bio;

            // Anisotropic specular controls (latex stretch direction).
            o.Anisotropy    = _Aniso;
            o.AnisoRotation = _AnisoRot;

            // Transmission (thin-part back-light), modulated by bio so SSS bleeds through audio-reactive regions.
            o.Transmission = saturate(_Trans_Str + bio * 0.1);

            // Matcap - world-anchored sphere mapping. The basis vectors come from view-direction + world-up instead of UNITY_MATRIX_V, because UNITY_MATRIX_V carries the camera's full rotation including roll - head tilt in VR (or any camera roll) would spin the matcap pattern around the view axis, making highlights swim instead of staying world-locked the way a real metal/latex surface would behave. vw_WorldViewDir reads from the actual rendering camera (UNITY_MATRIX_I_V), so this stays mirror-correct.
            float3 nWorld   = normalize(WorldNormalVector(IN, float3(0,0,1)));
            float3 viewDirW = vw_WorldViewDir(IN.worldPos);
            // Swap reference up when looking near-vertical so cross(refUp, viewDirW) doesn't collapse - using world Z as the fallback keeps the basis well-defined.
            float3 refUp    = (abs(dot(viewDirW, float3(0,1,0))) > 0.999) ? float3(0,0,1) : float3(0,1,0);
            float3 vRight   = normalize(cross(refUp, viewDirW));
            float3 vUp      = cross(viewDirW, vRight);
            float2 matcapBaseUV = float2(dot(vRight, nWorld), dot(vUp, nWorld)) * 0.5 + 0.5;

            // Layer 1 - channel-selectable mask + per-layer tint.
            float rad = _MatCap_Rot * (UNITY_PI / 180.0);
            float s = sin(rad), cc = cos(rad);
            float2 matcapUV = mul(matcapBaseUV - 0.5, float2x2(cc,-s,s,cc)) + 0.5;

            fixed4 matcapTex  = tex2D(_MatCap, matcapUV);
            fixed4 matcapMaskSample = tex2D(_MatCapMask, finalUV);
            float matcap1Mask = ChannelPick(matcapMaskSample, _MatCap_MaskCh);
            // Matcap audio boost gated by the user emission amount - without it the surface still pulses when AL is on with all sliders at zero.
            half3 matcap1 = matcapTex.rgb * _MatCap_Tint.rgb * matcap1Mask * _MatCap_Int * (1.0 + amp_emis * _AL_Emis_Mod * 0.5);
            o.Matcap = matcap1;

            // Layer 2 - independent matcap/mask channel/rotation/tint/blend mode; "Replace" blend uses the mask as a lerp so layer 2 takes over inside its mask zone.
            if (_UseMatCap2 > 0.5)
            {
                float rad2 = _MatCap2_Rot * (UNITY_PI / 180.0);
                float s2 = sin(rad2), cc2 = cos(rad2);
                float2 matcap2UV = mul(matcapBaseUV - 0.5, float2x2(cc2,-s2,s2,cc2)) + 0.5;

                fixed4 matcap2Tex = tex2D(_MatCap2, matcap2UV);
                fixed4 matcap2MaskSample = tex2D(_MatCap2_Mask, finalUV);
                float matcap2Mask = ChannelPick(matcap2MaskSample, _MatCap2_MaskCh);
                half3 matcap2 = matcap2Tex.rgb * _MatCap2_Tint.rgb * _MatCap2_Int * (1.0 + amp_emis * _AL_Emis_Mod * 0.5);

                int blendMode = (int)_MatCap2_Blend;
                if (blendMode == 1)
                    o.Matcap = lerp(o.Matcap, matcap2, matcap2Mask);              // Replace inside mask
                else if (blendMode == 2)
                    o.Matcap = lerp(o.Matcap, o.Matcap * matcap2, matcap2Mask);   // Multiply inside mask
                else
                    o.Matcap += matcap2 * matcap2Mask;                            // Add (default)
            }

            // EMISSION - autocorrelator vertically warps the emission UV so circuitry breathes without recolouring.
            float2 emisUV = finalUV;
            if (_UseAudioLink > 0.5 && _AL_AutoCorr_Mod > 0.001)
            {
                emisUV.y += (autoCorr - 0.5) * _AL_AutoCorr_Mod * 0.05;
            }
            float4 emisTex = tex2D(_EmissionMap, emisUV);
            float emisMask = emisTex.a;

            // Manual surface emission: circuitry lines ONLY
            float3 manualEmis = emisTex.rgb * _EmissionColor.rgb;

            float alWeight = saturate(_AL_Col_Blend);
            if (_UseAudioLink < 0.5) alWeight = 0.0;
            float3 alLayer = al_color.rgb * alWeight;

            // 1. BASE GLOW: Locked to circuitry lines
            float3 emisBase = (manualEmis + alLayer) * emisMask;

            // Emission boost via bio pulse (heartbeat + tension + neuroSpike + chrono breath).
            if (_UseAudioLink > 0.5)
                emisBase *= (1.0 + bio * _AL_Emis_Mod * 8.0);

            if (_AL_DFT_Mod > 0.001 && _UseAudioLink > 0.5)
                emisBase += emisTex.rgb * al_color.rgb * dftAmp * _AL_DFT_Mod * emisMask;

            // Poiyomi-style secondary emission layer - independent texture/color/mask, optional AL band reactor.
            if (_UseEmission2 > 0.5)
            {
                fixed4 emis2Tex = tex2D(_EmissionMap2, emisUV);
                float emis2Mask = ChannelPick(emis2Tex, _Emis2_MaskCh);
                float3 emis2Color = emis2Tex.rgb * _EmissionColor2.rgb;

                // Pull a band amp specifically for this layer so the artist can route bass/treble independently.
                float amp_emis2 = GET_AL_BAND(amps, _AL_Band_Emis2);
                if (_UseAudioLink > 0.5 && _AL_Emis2_Mod > 0.001)
                    emis2Color *= (1.0 + amp_emis2 * _AL_Emis2_Mod * 8.0);

                emisBase += emis2Color * emis2Mask;
            }

            // Region mask emission boost - each painted zone multiplies local emission so the user can brighten specific feature areas (panels, claws, paw-print decals) without a second map.
            if (_UseRegionMask > 0.5 && regionEmis > 0.001)
            {
                emisBase += o.Albedo * regionEmis;
            }

            // Dynamic effects bleed onto the emisMask.
            float effectMask = emisMask;

            if (_AL_Scanlines > 0.0 && _UseAudioLink > 0.5)
            {
                // CRT-bar scanline: smoothstep wave multiplied through emission. chr_scan is 0 unless ChronoFX is enabled.
                float scanTime = fmod((_Time.y * _AL_Scan_Speed * 1.8) + (chr_scan * _AL_Scan_React * 0.8), 628.318);
                float scanFreq = finalUV.y * _AL_Scan_Density;
                float scanWave = sin(scanFreq - scanTime) * 0.5 + 0.5;
                float scan = smoothstep(0.25, 0.75, scanWave + amp_scan * 0.4);
                float scanMask = lerp(1.0, scan, _AL_Scanlines);
                emisBase *= scanMask;
            }

            // Faint highlight on waveform peaks so the UV warp reads on dim backgrounds (decoration, not the main effect).
            float waveformRipple = raw_waveform * _AL_Waveform_Mod;
            if (_UseAudioLink > 0.5 && abs(waveformRipple) > 0.001)
            {
                emisBase += al_color.rgb * abs(waveformRipple) * 0.35 * effectMask;
            }

            // Autocorrelator ripple → EMISSION block; glitch tear → UV AUDIO DISTORTION CHAIN above.

            // CYBER HUD (masked, additive)
            float3 hud = 0;

            if (_UseCyber > 0.5)
            {
                // PARALLAX-OUT: shift HUD UV along tangent-space view direction so the panel reads as a plane floating at height h above the body; subtle vertical bob adds "alive" drift without rotating the panel.
                float hoverHeight = _Cyber_Hover + sin(_Time.y * 1.6) * _Cyber_Hover * _Cyber_Hover_Bob * 0.25;
                float2 hoverOffset = viewDirTangent.xy / max(viewDirTangent.z, 0.001) * hoverHeight;
                float2 hudUV = finalUV + hoverOffset;

                float cyberMask = tex2D(_CyberMask, hudUV).r;

                // VU Meter
                if (_UseCyberVU > 0.5)
                {
                    float vu = GET_AL_BAND(amps, _Cyber_VU_Band);
                    float2 vuUV = TransformHUD(hudUV, _Cyber_VU_Transform);
                    float vuInBounds = step(0.0, vuUV.x) * step(vuUV.x, 1.0)
                                     * step(0.0, vuUV.y) * step(vuUV.y, 1.0);
                    float bar =
                        step(vuUV.x, vu) *
                        step(abs(vuUV.y - 0.5), 0.04) *
                        vuInBounds;
                    hud += bar * _Cyber_VU_Str * al_color.rgb;
                }

                // Spectrum (CC) bars - sample N bars from the AudioLink band row
                if (_UseCyberCC > 0.5)
                {
                    float2 ccUV = TransformHUD(hudUV, _Cyber_CC_Transform);
                    float density = max(2.0, _Cyber_CC_Density);
                    if (ccUV.x >= 0.0 && ccUV.x <= 1.0 && ccUV.y >= 0.0 && ccUV.y <= 1.0)
                    {
                        int barIdx = (int)floor(ccUV.x * density);
                        int sampleX = (int)((float)barIdx / density * 127.0);

                        float magnitude = 0;
                        if (_UseAudioLink > 0.5 && AudioLinkIsAvailable())
                            magnitude = AudioLinkData(ALPASS_AUDIOLINK + int2(sampleX, (int)_Cyber_CC_Band)).r;

                        // Vertical bar grows from bottom (y=0) up
                        float barShape = step(1.0 - ccUV.y, saturate(magnitude * 4.0));
                        // Inter-bar gap
                        float barCenter = (floor(ccUV.x * density) + 0.5) / density;
                        float inBar = step(abs(ccUV.x - barCenter), 0.45 / density);
                        hud += barShape * inBar * _Cyber_CC_Str * al_color.rgb;
                    }
                }

                // Waveform
                if (_UseCyberWave > 0.5)
                {
                    float2 waveUV = TransformHUD(hudUV, _Cyber_Wave_Transform);
                    float waveInBounds = step(0.0, waveUV.x) * step(waveUV.x, 1.0)
                                       * step(0.0, waveUV.y) * step(waveUV.y, 1.0);
                    float wave = abs((waveUV.y - 0.5) - raw_waveform * 0.2);
                    wave = (1.0 - smoothstep(0.0, 0.02, wave)) * waveInBounds;
                    hud += wave * _Cyber_Wave_Str * al_color.rgb;
                }

                // DMX grid mini-readout
                if (_UseCyberDMX > 0.5)
                {
                    float2 dmxUV = TransformHUD(hudUV, _Cyber_DMX_Transform);
                    if (dmxUV.x >= 0.0 && dmxUV.x <= 1.0 && dmxUV.y >= 0.0 && dmxUV.y <= 1.0)
                    {
                        float3 dmxSample = tex2D(_Udon_DMXGridRenderTexture, dmxUV).rgb;
                        hud += dmxSample * _Cyber_DMX_Str;
                    }
                }

                // Autocorrelator radial ring
                if (_UseCyberAuto > 0.5)
                {
                    float2 acUV = TransformHUD(hudUV, _Cyber_Auto_Transform);
                    float2 centered = acUV - 0.5;
                    float r = length(centered) * 2.0;
                    if (r <= 1.0)
                    {
                        float acVal = 0;
                        if (_UseAudioLink > 0.5 && AudioLinkIsAvailable())
                            acVal = AudioLinkData(ALPASS_AUTOCORRELATOR + int2((int)(saturate(r) * 127.0), 0)).r;

                        float angle = atan2(centered.y, centered.x);
                        float spoke = 0.5 + 0.5 * sin(angle * 12.0 + animTime * 1.5);
                        float ring = exp(-pow((r - acVal), 2.0) * 80.0);
                        hud += ring * spoke * _Cyber_AutoCorr_Str * al_color.rgb;
                    }
                }

                // Float the HUD off the body - cyberMask (lifted UV) is the holographic viewport; emisMask provides a soft "kinship with emission" tint, never a hard clip.
                float hudLift = lerp(0.65, 1.0, saturate(emisMask));
                emisBase += hud * cyberMask * hudLift;
            }

            // Amplitude-driven flicker sparkle on top of the steady AL emission (decoration only) - gated by _AL_Emis_Mod so users can fully disable AL emission response with the slider.
            if (_UseAudioLink > 0.5 && amp_emis > 0.001 && _AL_Emis_Mod > 0.001)
            {
                float flickerTime = (_UseChronoFX > 0.5) ? (chr_emis * 40.0) : (_Time.y * 8.0);
                float alFlicker = sin(flickerTime + IN.worldPos.y * 12.0) * 0.5 + 0.5;
                emisBase += al_color.rgb * 2.0 * amp_emis * _AL_Emis_Mod * alFlicker * emisMask;
            }

            o.Emission = emisBase;

            // Clearcoat normal - flatten lerps shaded normal toward smooth geometric normal.
            float3 nClearcoat = normalize(nWorld);
            if (_CC_Flat > 0.001)
            {
                float3 worldN_FromMap = normalize(mul(o.Normal, o.WorldToTangent)); // tangent → world: row vec * matrix
                nClearcoat = normalize(lerp(worldN_FromMap, nClearcoat, saturate(_CC_Flat)));
            }
            o.ClearcoatNormal = nClearcoat;

            // LIGHT VOLUMES (stashes diffuse + base/clearcoat specular) - _LV_AdditiveOnly samples only additive volumes (preserves Unity probe baseline); _LV_Bias pushes along world normal as worldPosOffset to fix light bleed at sharp edges (matches official LV PBR); _LV_PosOffset is a manual world-space offset for thin/sleeve geometry; _LV_ProbeDering is an opt-in Bakery L1 fallback that swaps Unity SH9 for dering'd L0+L1 (without it, non-LV worlds keep Unity's full probe path preserving L2 detail and avoiding black-out from negative L1 reconstruction).
            o.LVDiffuse = 0;
            o.LVSpec    = 0;
            o.LVCCSpec  = 0;
            o.LVActive  = 0;
            #if defined(LIGHTVOLUMES_ENABLE)
            if (_LV_Int > 0.001)
            {
                bool lvAvailable = LightVolumesEnabled() > 0.5;
                bool doSample = lvAvailable || (_LV_ProbeDering > 0.5 && _LV_AdditiveOnly < 0.5);

                if (doSample)
                {
                    // World-space shaded normal (with normalmap) for diffuse fidelity.
                    float3 nWorldShaded = normalize(mul(o.Normal, o.WorldToTangent));

                    // Normal-bias offset + user-provided manual offset.
                    float3 lvOffset = nWorldShaded * _LV_Bias + _LV_PosOffset.xyz;

                    float3 lv_L0 = 0, lv_L1r = 0, lv_L1g = 0, lv_L1b = 0;
                    if (_LV_AdditiveOnly > 0.5)
                        LightVolumeAdditiveSH(IN.worldPos, lv_L0, lv_L1r, lv_L1g, lv_L1b, lvOffset);
                    else
                        LightVolumeSH(IN.worldPos, lv_L0, lv_L1r, lv_L1g, lv_L1b, lvOffset);

                    // Clamp evaluated diffuse to 0 - probe SH (especially Bakery's dering path) can produce negative values when L1 magnitude > L0, blacking out the avatar on default worlds.
                    o.LVDiffuse = max(LightVolumeEvaluate(nWorldShaded, lv_L0, lv_L1r, lv_L1g, lv_L1b), 0);
                    o.LVActive  = 1.0;

                    // _WorldSpaceCameraPos is the player's head, not the mirror camera - route through the helper.
                    float3 worldViewDir = vw_WorldViewDir(IN.worldPos);

                    // LV specular layers only fire when an actual LV system is in the scene - they need real L1 directionality, not dering'd probes which would duplicate Unity's reflection probes.
                    if (lvAvailable && _LV_Spec_Mix > 0.001)
                    {
                        half3 lvSpec = (_LV_Spec_Dominant > 0.5)
                            ? LightVolumeSpecularDominant(o.Albedo, o.Smoothness, o.Metallic, nWorldShaded, worldViewDir, lv_L0, lv_L1r, lv_L1g, lv_L1b)
                            : LightVolumeSpecular(o.Albedo, o.Smoothness, o.Metallic, nWorldShaded, worldViewDir, lv_L0, lv_L1r, lv_L1g, lv_L1b);
                        o.LVSpec = lvSpec * _LV_Spec_Mix * _LV_Int;
                    }

                    if (lvAvailable && _LV_CC_Spec_Mix > 0.001 && _CC_Strength > 0.001)
                    {
                        half3 ccF0_calc = _CC_F0 * _CC_Tint.rgb;
                        half3 lvCCSpec = (_LV_Spec_Dominant > 0.5)
                            ? LightVolumeSpecularDominant(ccF0_calc, _CC_Smoothness, nClearcoat, worldViewDir, lv_L0, lv_L1r, lv_L1g, lv_L1b)
                            : LightVolumeSpecular(ccF0_calc, _CC_Smoothness, nClearcoat, worldViewDir, lv_L0, lv_L1r, lv_L1g, lv_L1b);
                        o.LVCCSpec = lvCCSpec * _LV_CC_Spec_Mix * _LV_Int;
                    }
                }
            }
            #endif

            // Store UV
            o.UV = finalUV;
        }
        ENDCG
    }

    CustomEditor "VixenWearEditor"
    FallBack "Standard"
}