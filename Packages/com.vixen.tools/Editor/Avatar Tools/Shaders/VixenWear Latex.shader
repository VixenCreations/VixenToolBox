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
        [Enum(R,0,G,1,B,2,A,3,None,4)] _PBR_AO_Ch ("AO Channel", Float) = 1
        [Enum(R,0,G,1,B,2,A,3)] _PBR_Height_Ch ("Height Channel", Float) = 2

        // Poiyomi/Mochie packed-map masks - reflection mask dims environment/probe reflections, specular mask dims direct highlights. Channel defaults (B/A) match Mochie "Metallic Maps" packing (R:Met G:Smooth B:ReflMask A:SpecMask). Default off so existing materials are unchanged.
        [Toggle] _UsePackedMasks ("Enable Reflection / Specular Masks", Float) = 0
        [Enum(R,0,G,1,B,2,A,3)] _ReflMask_Ch ("Reflection Mask Channel", Float) = 2
        [Toggle] _ReflMask_Inv ("Invert Reflection Mask", Float) = 0
        _ReflMask_Str ("Reflection Mask Strength", Range(0,1)) = 1
        [Enum(R,0,G,1,B,2,A,3)] _SpecMask_Ch ("Specular Mask Channel", Float) = 3
        [Toggle] _SpecMask_Inv ("Invert Specular Mask", Float) = 0
        _SpecMask_Str ("Specular Mask Strength", Range(0,1)) = 1

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

        // Polish layer master gate + B&W mask - scales the entire polish lighting layer (clearcoat, thin film, SSS, transmission, anisotropy, rim, multi-scatter) per-pixel. Toggle on + white mask preserves the historical look; runtime-gated (no keyword) so VRCFury can animate it.
        [Toggle] _UsePolish ("Enable Polish Layer", Float) = 1
        [NoScaleOffset] _PolishMask ("Polish Mask (B&W)", 2D) = "white" {}
        [Enum(R,0,G,1,B,2,A,3)] _PolishMaskCh ("Polish Mask Channel", Float) = 0

        // Drip - procedural vertical rivulets that mimic water running off the latex (per-pixel wet streaks). Own toggle so off = no cost.
        [Toggle] _UseDrip ("Enable Drip (Water Run-Off)", Float) = 0
        [NoScaleOffset] _DripMask ("Drip Mask (B&W)", 2D) = "white" {}
        [Enum(R,0,G,1,B,2,A,3)] _DripMaskCh ("Drip Mask Channel", Float) = 0
        _Drip_Density ("Drip Density (Columns)", Range(2, 200)) = 40
        _Drip_Width ("Rivulet Thinness", Range(1, 300)) = 90
        _Drip_Coverage ("Drip Coverage", Range(0, 1)) = 0.4
        _Drip_Speed ("Drip Flow Speed", Range(0, 2)) = 0.25
        _Drip_Strength ("Drip Run-Off Streak Strength", Range(0, 1)) = 0.7
        _Drip_Normal ("Drip Normal Bump", Range(0, 1)) = 0.5

        // Clear 3D drips - water beads that swell and pinch off, then run down the surface and dry out (fade away); shaded as clear water tinted to the clearcoat color. Vertex bulge plus surface glass, gated under the Wet toggle.
        _Drip3D_Strength ("Clear Drip Amount", Range(0, 1)) = 0
        _Drip3D_Scale ("Clear Drip Droplet Scale", Range(0.1, 20)) = 8.0
        _Drip3D_Sheen ("Clear Drip Glassiness", Range(0, 1)) = 0.8
        _Drip3D_Fall ("Clear Drip Fall Length", Range(0, 1)) = 0.6

        // Clear drip physics + collision - ambient sway/wobble, surface-slide down the body while attached, and a floor splat that pools on the shared world floor (_Goo_GroundY). All default off so existing droplet materials are unchanged.
        _Drip_Sway ("Droplet Sway / Wobble", Range(0, 1)) = 0
        _Drip_BodyFollow ("Droplet Surface Slide", Range(0, 1)) = 0
        [Toggle] _Drip_FloorCollide ("Droplet Floor Splat", Float) = 0

        // Wet soak - global "just out of the shower/pool" wetness layered under the run-off rivulets above.
        _Wet_Amount ("Wetness (Soaked)", Range(0, 1)) = 0.7
        _Wet_Darken ("Wet Darkening", Range(0, 1)) = 0.6
        _Wet_Smoothness ("Wet Smoothness", Range(0, 1)) = 0.95
        _Wet_Sheen ("Wet Film Sheen", Range(0, 1)) = 0.5
        _Wet_Flatten ("Wet Normal Flatten", Range(0, 1)) = 0.5

        // Goo - gravity-aligned vertex sag that mimics melting/runny latex or wax. Runs in disp(); own toggle.
        [Toggle] _UseGoo ("Enable Goo (Melting Sag)", Float) = 0
        [NoScaleOffset] _GooMask ("Goo Mask (B&W)", 2D) = "white" {}
        [Enum(R,0,G,1,B,2,A,3)] _GooMaskCh ("Goo Mask Channel", Float) = 0
        _Goo_Strength ("Goo Sag Distance", Range(0, 1)) = 0.0
        _Goo_Noise ("Goo Tendril Scale", Range(0.1, 20)) = 6.0
        _Goo_Speed ("Goo Flow Speed", Range(0, 2)) = 0.3
        _Goo_Droop ("Goo Underside Bias", Range(0, 1)) = 0.6
        _Goo_Reach ("Goo Stretch Distance", Range(0, 10)) = 0.3
        _Goo_Variation ("Goo Strand Variation", Range(0, 1)) = 0.7
        _Goo_ToGround ("Goo Melt To Ground", Range(0, 1)) = 0
        _Goo_GroundY ("Goo Ground Height (World Y)", Float) = 0

        // Goo physics + collision - ambient pendulum sway, surface-follow body collision, and a floor clamp with pooling. All default off so existing materials are unchanged; _Goo_GroundY is the shared world floor for both goo and droplet collision.
        _Goo_Sway ("Goo Sway Amount", Range(0, 1)) = 0
        _Goo_SwaySpeed ("Goo Sway Speed", Range(0, 3)) = 1.0
        _Goo_BodyFollow ("Goo Surface Follow (Body)", Range(0, 1)) = 0
        [Toggle] _Goo_FloorCollide ("Goo Floor Collision", Float) = 0
        _Goo_Pool ("Goo Floor Pooling", Range(0, 1)) = 0.3

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
        [NoScaleOffset] _CyberMask ("Cyber Mask (B&W Window)", 2D) = "black" {}
        _Cyber_Hover ("HUD Hover Height (Float Off Body)", Range(0, 0.15)) = 0.03
        _Cyber_Hover_Bob ("HUD Hover Bob (Subtle Drift)", Range(0, 1)) = 0.25

        [Toggle] _UseCyberVU ("Enable VU Meter", Float) = 0
        [Enum(Console,0,Bar,1)] _Cyber_VU_Style ("VU Meter Style", Float) = 0
        _Cyber_VU_Str ("VU Meter Intensity", Range(0,5)) = 1.0
        [VectorLabel(X Offset, Y Offset, Scale, Rotation)] _Cyber_VU_Transform ("VU Transform", Vector) = (0,0,1,0)

        [Toggle] _UseCyberCC ("Enable Spectrum", Float) = 0
        [Enum(Bass,0,Low Mid,1,High Mid,2,Treble,3)] _Cyber_CC_Band ("Spectrum Primary Band", Float) = 1
        _Cyber_CC_Str ("Spectrum Strip Intensity", Range(0,5)) = 1.0
        _Cyber_CC_Density ("Spectrum Bar Count", Range(4,64)) = 16
        [VectorLabel(X Offset, Y Offset, Scale, Rotation)] _Cyber_CC_Transform ("Spectrum Transform", Vector) = (0,0,1,0)

        [Toggle] _UseCyberWave ("Enable Waveform", Float) = 0
        [Enum(Bass,0,Low Mid,1,High Mid,2,Treble,3)] _Cyber_Wave_Band ("Waveform Band", Float) = 0
        _Cyber_Wave_Str ("Waveform Intensity", Range(0,5)) = 1.0
        [VectorLabel(X Offset, Y Offset, Scale, Rotation)] _Cyber_Wave_Transform ("Waveform Transform", Vector) = (0,0,1,0)

        [Toggle] _UseCyberDMX ("Enable DMX Grid Readout", Float) = 0
        [Enum(Bass,0,Low Mid,1,High Mid,2,Treble,3)] _Cyber_DMX_Band ("DMX Grid Band", Float) = 0
        _Cyber_DMX_Str ("DMX Grid Intensity", Range(0,5)) = 1.0
        [VectorLabel(X Offset, Y Offset, Scale, Rotation)] _Cyber_DMX_Transform ("DMX Grid Transform", Vector) = (0,0,1,0)

        [Toggle] _UseCyberAuto ("Enable Autocorrelator Ring", Float) = 0
        [Enum(Bass,0,Low Mid,1,High Mid,2,Treble,3)] _Cyber_Auto_Band ("Autocorrelator Band", Float) = 0
        _Cyber_AutoCorr_Str ("Autocorrelator Intensity", Range(0,5)) = 1.0
        [VectorLabel(X Offset, Y Offset, Scale, Rotation)] _Cyber_Auto_Transform ("Autocorrelator Transform", Vector) = (0,0,1,0)
        // Per-effect reactors for the Autocorrelator HUD ring. Each effect is toggled on/off and driven by its own AudioLink band.
        [Toggle] _Cyber_Auto_Shimmer ("AC Shimmer Effect", Float) = 1
        [Enum(Bass,0,Low Mid,1,High Mid,2,Treble,3)] _Cyber_Auto_Shimmer_Band ("AC Shimmer Band", Float) = 3
        [Toggle] _Cyber_Auto_Pop ("AC Pop Effect", Float) = 1
        [Enum(Bass,0,Low Mid,1,High Mid,2,Treble,3)] _Cyber_Auto_Pop_Band ("AC Pop Band", Float) = 0
        [Toggle] _Cyber_Auto_Sizzle ("AC Sizzle Effect", Float) = 1
        [Enum(Bass,0,Low Mid,1,High Mid,2,Treble,3)] _Cyber_Auto_Sizzle_Band ("AC Sizzle Band", Float) = 2
        [Toggle] _Cyber_Auto_Electrify ("AC Electrify Effect", Float) = 1
        [Enum(Bass,0,Low Mid,1,High Mid,2,Treble,3)] _Cyber_Auto_Electrify_Band ("AC Electrify Band", Float) = 1

        [Toggle] _UseVtxKinetic ("Enable Vertex Displacement", Float) = 0
        [Enum(Bass,0,Low Mid,1,High Mid,2,Treble,3)] _Vtx_Pump_Band ("Vertex Pump Band", Float) = 0
        _Vtx_Pump_Str ("Vertex Pump Distance", Range(0, 5)) = 0.0

        [Enum(Bass,0,Low Mid,1,High Mid,2,Treble,3)] _Vtx_Fracture_Band ("Vertex Fracture Band", Float) = 3
        _Vtx_Fracture_Str ("Vertex Fracture Scatter", Range(0, 5)) = 0.0
        _Vtx_Fracture_Amount ("Vertex Fracture Amount (Hold/Animate)", Range(0,1)) = 0.0
        _Vtx_Fracture_Dist ("Vertex Fracture Hover Distance", Range(0,2)) = 0.35
        _Vtx_Fracture_Spin ("Vertex Fracture Tumble", Range(0,1)) = 0.6
        _Vtx_Fracture_Spiral ("Vertex Fracture Spiral", Range(0,1)) = 0.0
        _Vtx_Fracture_Lift ("Vertex Fracture Lift (Up/Down, Animate)", Range(-2,2)) = 0.0
        _Vtx_Fracture_Float ("Vertex Fracture Float Drift", Range(0,1)) = 0.0
        _Vtx_Fracture_Trail ("Vertex Fracture Trail Length", Range(0,1)) = 0.0
        _Shard_ColorMod ("Shard Hue Shift", Range(0,1)) = 0.0
        _Shard_ColorMod_Speed ("Shard Hue Cycle Speed", Range(0,5)) = 0.0
        [Toggle] _UseShardCC ("Shard AudioLink ColorChord", Float) = 0
        _Shard_CC_Str ("Shard ColorChord Blend", Range(0,1)) = 0.0

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

        // Outline pass - Sylva-style Cull Front backface extrusion; toggle gates the entire variant so off = zero runtime cost.
        [Toggle(_OUTLINE_ON)] _UseOutline ("Enable Outline", Float) = 0
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        [HDR] _OutlineEmis ("Outline Emission (HDR Glow)", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0, 1000)) = 1.0
        _MaxOutlineWidth ("Max Outline Width (Distance Clamp)", Range(0, 1000)) = 850.0
        _OutlineViewFudge ("Outline View Fudge (Push Toward Camera)", Float) = 0.0
        [NoScaleOffset] _OutlineMask ("Outline Mask", 2D) = "white" {}
        [Enum(None,0,R,1,G,2,B,3,A,4)] _OutlineMaskCh ("Outline Mask Channel", Float) = 0
        [Enum(Bass,0,Low Mid,1,High Mid,2,Treble,3)] _AL_Band_Outline ("Outline AL Band", Float) = 0
        _AL_Outline_Mod ("Outline AL Emission Boost", Range(0,5)) = 0.0
    }

    SubShader
    {
        // Tags listed here are SubShader defaults - VixenWearEditor overrides RenderType/Queue/VRCFallback per material via SetOverrideTag to match the selected _Mode (Opaque/Cutout/Fade/Transparent).
        Tags { "RenderType"="Opaque" "VRCFallback"="ToonDoubleSided" "Queue"="Geometry" }
        LOD 500

        // PASS 0: OUTLINE (Cull Front backface extrusion - Sylva-style). Keyword-gated by _OUTLINE_ON so the unused variant is the no-keyword default and costs nothing at runtime. Always-opaque blend so the outline is solid regardless of the material's selected alpha mode.
        Cull Front
        ZWrite On
        Blend One Zero
        ColorMask RGBA

        CGPROGRAM
        // Minimal surface shader: no GI, no extra lights, no shadow/lightmap variants. Outline color goes to Emission; lighting fn returns black so the only contribution is the emission tint.
        #pragma surface outlineSurf Outline keepalpha noshadow noambient novertexlights nolightmap nodynlightmap nodirlightmap noshadowmask nometa nolppv noforwardadd vertex:outlineDisp
        #pragma target 5.0
        #pragma multi_compile_instancing
        #pragma skip_variants LOD_FADE_CROSSFADE LIGHTMAP_ON DIRLIGHTMAP_COMBINED DYNAMICLIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK DIRECTIONAL_COOKIE POINT_COOKIE SHADOWS_CUBE
        // Outline master toggle - when off, vertex skips extrusion and surface clips the pixel so the pass is effectively dead. Alpha keywords mirror the main pass so cutout textures don't cause outlines to float in transparent regions.
        #pragma shader_feature_local _OUTLINE_ON
        #pragma shader_feature_local _ALPHATEST_ON
        #pragma shader_feature_local _ALPHABLEND_ON
        #pragma shader_feature_local _ALPHAPREMULTIPLY_ON

        #include "UnityCG.cginc"
        // AudioLink for optional emission boost - runtime-gated by _UseAudioLink so it costs nothing when AL isn't in scene.
        #include "Packages/com.vixencreations.vixens-toolbox/Editor/Avatar Tools/Shaders/cginc/AudioLink.cginc"

        // _MainTex_ST is auto-declared by the surface compiler because Input.uv_MainTex is present; redeclaring it (or any *_ST for a used uv) collides at the FORWARD pass.
        sampler2D _MainTex;
        sampler2D _OutlineMask;
        fixed4 _Color;
        fixed4 _OutlineColor;
        fixed4 _OutlineEmis;
        half _CutOff;
        float _OutlineWidth, _MaxOutlineWidth, _OutlineViewFudge, _OutlineMaskCh;
        float _AL_Band_Outline, _AL_Outline_Mod;
        float _UseAudioLink, _UseMediaState;
        uniform float _MediaPlaying;

        struct Input
        {
            float2 uv_MainTex;
        };

        // None=0 (full strength), R/G/B/A=1..4 (matches inspector enum). Mirrored from main pass ChannelPick with the extra None slot for "no mask, just use everywhere".
        inline float OL_ChannelPick(fixed4 packed, float ch)
        {
            return (ch < 0.5) ? 1.0
                 : (ch < 1.5) ? packed.r
                 : (ch < 2.5) ? packed.g
                 : (ch < 3.5) ? packed.b
                 :              packed.a;
        }

        void outlineDisp(inout appdata_full v)
        {
        #if defined(_OUTLINE_ON)
            // Eye-depth scaling keeps the outline a visually constant thickness at distance instead of vanishing.
            float eyeDepth = -UnityObjectToViewPos(v.vertex.xyz).z;
            float3 worldN  = UnityObjectToWorldNormal(v.normal);

            // 0.0001 scale converts the 0-1000 slider into reasonable world-units; min() clamps so the outline doesn't blow up at far distance.
            float wBase = lerp(0.0, _OutlineWidth    * 0.0001, saturate(_OutlineWidth));
            float wMax  = lerp(0.0, _MaxOutlineWidth * 0.0001, saturate(_MaxOutlineWidth));
            float thickness = min(wBase + wBase * eyeDepth, wMax);

            float4 maskRGBA = tex2Dlod(_OutlineMask, float4(v.texcoord.xy, 0, 0));
            thickness *= OL_ChannelPick(maskRGBA, _OutlineMaskCh);

            // View fudge nudges the extruded shell toward the camera to mitigate z-fighting against the main pass when ZWrite is on for both.
            float3 worldPos  = mul(unity_ObjectToWorld, v.vertex).xyz;
            float3 worldView = normalize(UnityWorldSpaceViewDir(worldPos));
            float3 worldOffset = worldN * thickness + (-worldView * _OutlineViewFudge);

            // Convert world-space offset back to object space without translation.
            v.vertex.xyz += mul((float3x3)unity_WorldToObject, worldOffset);
        #endif
        }

        // Black direct lighting - emission carries the visible color so the outline doesn't pick up scene lighting.
        inline half4 LightingOutline(SurfaceOutput s, half3 lightDir, half atten)
        {
            return half4(0, 0, 0, s.Alpha);
        }

        void outlineSurf(Input IN, inout SurfaceOutput o)
        {
        #if !defined(_OUTLINE_ON)
            // Toggle off: kill every fragment. Cheaper than letting the BRDF math run; the un-extruded backfaces would z-fight with the main pass anyway.
            clip(-1);
        #endif

            fixed4 mainSample = tex2D(_MainTex, IN.uv_MainTex) * _Color;

            // Match the main pass cutout behavior so the outline respects the same alpha test.
        #if defined(_ALPHATEST_ON)
            clip(mainSample.a - _CutOff);
        #endif

            // Optional AL emission boost - runtime-gated, no keyword variant. Uses raw band amplitude (no Chronotensity) to keep this pass cheap.
            half3 alBoost = 0;
            if (_UseAudioLink > 0.5 && !(_UseMediaState > 0.5 && _MediaPlaying < 0.5) && AudioLinkIsAvailable())
            {
                int band = (int)_AL_Band_Outline;
                float amp = AudioLinkData(ALPASS_AUDIOLINK + int2(0, clamp(band, 0, 3))).r;
                amp = saturate(pow(amp * 4.0, 0.35));
                alBoost = _OutlineEmis.rgb * amp * _AL_Outline_Mod;
            }

            o.Albedo   = 0;
            o.Emission = _OutlineColor.rgb + _OutlineEmis.rgb + alBoost;
            o.Alpha    = mainSample.a;
        }
        ENDCG

        // Blend/ZWrite are property-driven so the editor flips them per-material without a recompile - Opaque/Cutout use One/Zero/ZWrite On; Fade uses SrcAlpha/OneMinusSrcAlpha/ZWrite Off; Transparent uses One/OneMinusSrcAlpha/ZWrite Off.
        Cull Off
        Blend [_SrcBlend] [_DstBlend]
        ZWrite [_ZWrite]

        // PASS 1: CORE PBR SURFACE (BASE SUIT, FRACTURE CLIP)
        CGPROGRAM
        // Surface pragma drops Deferred/Meta + LIGHTMAP/DIRLIGHTMAP/SHADOWMASK/LPPV variants (VRChat forward-only, avatar clothing never lightmapped); keepalpha preserves LightingStandardLatex alpha so Fade/Transparent get real alpha. noforwardadd skips the ForwardAdd pass entirely (avatar gets directional + probes + LV + LTCGI; loses realtime per-light additive contributions) - critical for ps_5_0 sampler budget because ForwardAdd's POINT/POINT_COOKIE + SHADOWS_CUBE built-in samplers stacked on our 13 texture samplers blew past the 16-register cap.
        #pragma surface surf StandardLatex keepalpha fullforwardshadows addshadow noforwardadd vertex:disp tessellate:tessEdge exclude_path:deferred exclude_path:prepass nolightmap nodynlightmap nodirlightmap noshadowmask nometa nolppv
        #pragma target 5.0

        // Defensive against Unity 2022.3.x emitting lightmap/LOD variants despite the no* directives above. Cookie + cube-shadow variants are also skipped for sampler budget - any directional cookie / point cube shadow would add 1-2 samplers, and avatars don't typically use them.
        #pragma skip_variants LOD_FADE_CROSSFADE LIGHTMAP_ON DIRLIGHTMAP_COMBINED DYNAMICLIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK DIRECTIONAL_COOKIE POINT_COOKIE SHADOWS_CUBE

        // VRChat single-pass stereo / GPU instancing - required for avatar batching in VR.
        #pragma multi_compile_instancing
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
            half  PolishMask;
            half  ReflectionMask;
            half  SpecularMask;
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

        // _MainTex uses an explicit texture + sampler so the fragment-stage B&W masks (_PolishMask, _DripMask, _CyberMask) can borrow its sampler instead of each consuming one of the 16 ps_5_0 sampler registers. A borrowed sampler only resolves in a stage where its donor texture is actually sampled, so _GooMask keeps its own combined sampler: it is read in the vertex/displacement stage (and the auto-generated shadow caster), where _MainTex is not sampled. Net sampler count is unchanged versus before these effects: _CyberMask gives up its register, _GooMask takes one.
        UNITY_DECLARE_TEX2D(_MainTex);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_PolishMask);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_DripMask);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_CyberMask);
        sampler2D _MetallicGlossMap, _BumpMap, _DetailNormalMap, _EmissionMap, _EmissionMap2, _RegionMask, _MatCap, _MatCapMask, _MatCap2, _MatCap2_Mask, _GooMask;
        fixed4 _Color, _EmissionColor, _EmissionColor2, _CC_Tint;
        fixed4 _Region_R_Tint, _Region_G_Tint, _Region_B_Tint;
        fixed4 _MatCap_Tint, _MatCap2_Tint;
        half _CutOff, _MinBrightness;
        float _UV_Rot, _SpeedX, _SpeedY, _MatCap_Rot;
        float _AO_Str, _Spec_Occ, _Shad_Hard, _Norm_Str;
        float _Parallax, _Disp_Str, _Tess_Edge, _Emis_Exp;
        // Poiyomi compat: PBR mask channel selectors + invert toggles.
        float _PBR_Met_Ch, _PBR_Met_Inv, _PBR_Smooth_Ch, _PBR_Smooth_Inv, _PBR_AO_Ch, _PBR_Height_Ch;
        float _UsePackedMasks, _ReflMask_Ch, _ReflMask_Inv, _ReflMask_Str, _SpecMask_Ch, _SpecMask_Inv, _SpecMask_Str;
        // Poiyomi compat: secondary emission layer + multi-region color mask.
        float _UseEmission2, _Emis2_MaskCh, _AL_Band_Emis2, _AL_Emis2_Mod;
        float _UseRegionMask, _Region_R_Emis, _Region_G_Emis, _Region_B_Emis;
        float _CC_Strength, _CC_Smoothness, _CC_Spec_AA, _CC_Flat, _CC_F0;
        float _Film_Str, _Film_Thick, _Rim_Str, _Rim_Power;
        float _SSS_Str, _SSS_Dist, _SSS_Power;
        float _Aniso, _AnisoRot;
        float _Trans_Str, _Trans_Dist, _Trans_Power;
        float _UseMultiScatter;
        // Polish master gate + B&W mask, plus the drip (surface) and goo (vertex) latex effects.
        float _UsePolish, _PolishMaskCh;
        float _UseDrip, _DripMaskCh, _Drip_Density, _Drip_Width, _Drip_Coverage, _Drip_Speed, _Drip_Strength, _Drip_Normal;
        float _Wet_Amount, _Wet_Darken, _Wet_Smoothness, _Wet_Sheen, _Wet_Flatten;
        float _UseGoo, _GooMaskCh, _Goo_Strength, _Goo_Noise, _Goo_Speed, _Goo_Droop, _Goo_Reach, _Goo_Variation, _Goo_ToGround, _Goo_GroundY;
        float _Goo_Sway, _Goo_SwaySpeed, _Goo_BodyFollow, _Goo_FloorCollide, _Goo_Pool;
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
        float _Vtx_Fracture_Amount, _Vtx_Fracture_Dist, _Vtx_Fracture_Spin;

        float _UseCyber, _Cyber_AutoCorr_Str, _Cyber_Hover, _Cyber_Hover_Bob;
        float _UseCyberVU, _Cyber_VU_Str; float4 _Cyber_VU_Transform;
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

        // Hash + smooth 3D value noise (0..1) driving the Goo melt's procedural per-strand variation.
        float gooHash3(float3 p) { return frac(sin(dot(p, float3(12.9898, 78.233, 37.719))) * 43758.5453); }
        float gooNoise3(float3 p)
        {
            float3 i = floor(p);
            float3 f = frac(p);
            f = f * f * (3.0 - 2.0 * f);
            float n000 = gooHash3(i + float3(0, 0, 0));
            float n100 = gooHash3(i + float3(1, 0, 0));
            float n010 = gooHash3(i + float3(0, 1, 0));
            float n110 = gooHash3(i + float3(1, 1, 0));
            float n001 = gooHash3(i + float3(0, 0, 1));
            float n101 = gooHash3(i + float3(1, 0, 1));
            float n011 = gooHash3(i + float3(0, 1, 1));
            float n111 = gooHash3(i + float3(1, 1, 1));
            float nx00 = lerp(n000, n100, f.x);
            float nx10 = lerp(n010, n110, f.x);
            float nx01 = lerp(n001, n101, f.x);
            float nx11 = lerp(n011, n111, f.x);
            return lerp(lerp(nx00, nx10, f.y), lerp(nx01, nx11, f.y), f.z);
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
                    al_color = AudioLinkData(ALPASS_CCCOLORS + int2((int)((uint)_AL_Band_Emission % 11u) + 1, 0));
                // Theme 0..3 live at uint2(0..3, 23), not CCCOLORS row+1.
                else if (colorMode >= 2 && colorMode <= 5)
                    al_color = AudioLinkData(ALPASS_THEME_COLOR0 + int2(colorMode - 2, 0));
                else if (colorMode == 6)
                    al_color = AudioLinkData(ALPASS_CCSTRIP + int2((int)(saturate(_AL_Strip_Pos) * 127.0), 0));

                float wavePhase = frac(uv.y * 2.0 - _Time.y * 0.2);
                raw_waveform = AudioLinkData(ALPASS_WAVEFORM + int2((int)(wavePhase * 128.0), 0)).r - 0.5;
                autoCorr = AudioLinkData(ALPASS_AUTOCORRELATOR + int2((int)(frac(uv.x) * 128.0), 0)).r * 0.007;
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

                // Vertex fracture is now a real geometry-shader effect (see "PASS 4: FRACTURE SHARDS"), driven by _Vtx_Fracture_Amount; the old in-place vertex scatter is removed.
            }

            // GOO - melting/runny latex. Gravity-aligned, masked, and procedurally varied so it forms uneven runny tendrils. Range is dramatically extendable via _Goo_Reach, and it can optionally melt all the way down to the world ground plane (_Goo_ToGround). Runs in disp(); own toggle, independent of the AL kinetic gate.
            if (_UseGoo > 0.5 && _Goo_Strength > 0.0001)
            {
                float gooMask = ChannelPick(tex2Dlod(_GooMask, float4(uv, 0, 0)), _GooMaskCh);
                if (gooMask > 0.001)
                {
                    // World position (for melt-to-ground) and world normal (downward-facing surfaces melt more).
                    float3 gooWorldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                    float3 worldN = UnityObjectToWorldNormal(v.normal);
                    float facingDown = saturate(dot(worldN, float3(0, -1, 0)));
                    float faceWeight = lerp(1.0, facingDown, saturate(_Goo_Droop));

                    // PROCEDURAL GENERATION - coarse per-strand identity (coherent tendrils) plus two octaves of value noise for organic, uneven melting. _Goo_Variation blends from a uniform melt (0) to wildly varying strand lengths (1).
                    float3 gooNP = v.vertex.xyz * _Goo_Noise;
                    float gooFbm = gooNoise3(gooNP) * 0.65 + gooNoise3(gooNP * 2.7 + 13.1) * 0.35;
                    float3 gooCell = floor(v.vertex.xyz * _Goo_Noise * 0.5);
                    float strandHash = gooHash3(gooCell);
                    float procReach = saturate(gooFbm * 0.6 + strandHash * 0.6);
                    float strandReach = lerp(1.0, procReach * 1.6, saturate(_Goo_Variation));

                    // Slow time wobble so the melt stays alive and runny; staggered per strand.
                    float wobble = 0.75 + 0.25 * sin(_Time.y * _Goo_Speed * 6.2831 + strandHash * 6.2831);

                    // Common melt weight (0..~1.5); some strands reach further than others.
                    float meltWeight = gooMask * faceWeight * strandReach * wobble * saturate(_Goo_Strength);

                    // DRAMATICALLY EXTENDED RANGE. Distance mode stretches down a large, settable distance (_Goo_Reach world units). Ground mode pulls each vertex down toward the world ground plane (Y = _Goo_GroundY) so strands reach the floor regardless of avatar height. Computed in world space, then converted to object space so non-uniform scale is handled.
                    float distDown   = _Goo_Reach * meltWeight;
                    float groundDown = max(gooWorldPos.y - _Goo_GroundY, 0.0) * saturate(meltWeight);
                    float down = lerp(distDown, groundDown, saturate(_Goo_ToGround));

                    // PHYSICS - lateral pendulum sway, growing with how far the strand has melted so the tip swings most, like a weighted strand. Staggered per strand so tendrils never move in lock-step.
                    float3 lateral = 0;
                    float swayPh = _Time.y * _Goo_SwaySpeed * 2.0 + strandHash * 6.2831;
                    lateral.x = sin(swayPh) * _Goo_Sway;
                    lateral.z = sin(swayPh * 0.8 + 1.7) * _Goo_Sway;
                    lateral *= down * 0.4;

                    float3 meltWorld = float3(lateral.x, -down, lateral.z);

                    // BODY COLLISION (best-effort) - project the melt onto the surface tangent plane so goo flows ALONG the body instead of tunnelling straight through it (1 = pure surface flow, 0 = straight gravity).
                    if (_Goo_BodyFollow > 0.0001)
                    {
                        float3 tangentFlow = meltWorld - worldN * dot(meltWorld, worldN);
                        float lenM = length(meltWorld);
                        float tfl = length(tangentFlow);
                        tangentFlow = (tfl > 1e-5) ? (tangentFlow / tfl * lenM) : meltWorld;
                        meltWorld = lerp(meltWorld, tangentFlow, saturate(_Goo_BodyFollow));
                    }

                    // FLOOR COLLISION - clamp the melted world position to the floor plane (_Goo_GroundY) and splay sideways into a shallow pool where it lands.
                    float3 meltedWP = gooWorldPos + meltWorld;
                    if (_Goo_FloorCollide > 0.5)
                    {
                        float below = _Goo_GroundY - meltedWP.y;
                        if (below > 0.0)
                        {
                            meltedWP.y = _Goo_GroundY;
                            float2 splay = float2(strandHash - 0.5, gooHash3(gooCell + 7.3) - 0.5);
                            float sl = length(splay);
                            splay = (sl > 1e-5) ? splay / sl : float2(1, 0);
                            meltedWP.xz += splay * below * _Goo_Pool;
                        }
                    }

                    // Back to object space (handles non-uniform scale).
                    v.vertex.xyz += mul((float3x3)unity_WorldToObject, meltedWP - gooWorldPos);
                }
            }

            // Static displacement
            v.vertex.xyz += v.normal * d;
        }

        // PBR HELPERS
        float2 ParallaxRaymarching(float2 uv, float3 viewDirTangent, float parallaxDepth)
        {
            // Derivatives are taken up front in uniform control flow so the tex2Dgrad calls inside the dynamic loop stay valid, and the function uses a single return path so FXC can prove every local is initialized (silences the "potentially uninitialized variable" warning in the shadow caster).
            float2 dx = ddx(uv);
            float2 dy = ddy(uv);
            float2 result = uv;

            // Early-out when depth ~= 0 - otherwise the loop below re-samples the same texel up to 50 times (stepUVOffset collapses to zero) and exits only when the heightmap value rises above the descending layer height, burning ~35 tex2Dgrad samples per pixel on any non-white surface map.
            [branch] if (parallaxDepth >= 1e-4)
            {
                float parallaxLimit = -length(viewDirTangent.xy) / max(viewDirTangent.z, 0.001);
                parallaxLimit *= parallaxDepth;
                float2 vOffsetDir = normalize(viewDirTangent.xy);
                float2 vMaxOffset = vOffsetDir * parallaxLimit;
                int numSteps = (int)lerp(48.0, 8.0, max(viewDirTangent.z, 0.0));
                float stepSize = 1.0 / (float)numSteps;

                float currentLayerHeight = 1.0;
                float2 currentUVOffset = 0.0;
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
                result = uv + (currentUVOffset - stepUVOffset * weight);
            }
            return result;
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

            half rawAO = s.Occlusion;

            // Polish layer master gate + per-pixel B&W mask. polish=0 collapses the whole polish layer to a flat GGX base: clearcoat off (so baseEnergy returns to 1), thin film neutral, no transmission, isotropic spec. Clearcoat/film/transmission/aniso scale here; SSS, rim, and multi-scatter pick it up below.
            half polish = saturate(s.PolishMask);
            s.ClearcoatStrength *= polish;
            s.ThinFilmStrength  *= polish;
            s.Transmission      *= polish;
            s.Anisotropy        *= polish;

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
            float sssTerm = (wrap * 0.6 + back * 0.4) * _SSS_Str * polish;
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
            half rim = pow(saturate(1.0 - NcV), rimExponent) * _Rim_Str * polish *
                       saturate(_Rim_Power * 100.0);
            half3 rimColor = rim * diffColor * (gi.diffuse + 0.1);

            // Indirect - Karis split-sum env BRDF. gi.specular is raw IBL (no Fresnel); we multiply F here.
            float2 dfg_base = EnvBRDFApprox_AB(aBase,   NdotV);
            float2 dfg_cc   = EnvBRDFApprox_AB(ccRough, NcV);
            half3 envBRDF_base = specColor * dfg_base.x + dfg_base.y;
            half3 envBRDF_cc   = (ccF0    * dfg_cc.x   + dfg_cc.y) * s.ClearcoatStrength;

            // Multi-scatter compensation (Filament). Skipped when toggle off.
            half3 baseMS = 1.0;
            if (_UseMultiScatter > 0.5 && polish > 0.001)
            {
                baseMS = EnergyCompensation(specColor, dfg_base);
                baseSpecular *= baseMS;
            }

            // Indirect base specular (energy-attenuated by clearcoat).
            half3 indirectBaseSpec = gi.specular * envBRDF_base * baseEnergy * baseSpecOcc * baseMS;

            // Indirect clearcoat specular (uses its own roughness-mip env color).
            half3 indirectCCSpec = clearcoatEnv * envBRDF_cc * thinFilmColor * ccSpecOcc;

            // Poiyomi/Mochie packed-map masks - specular mask dims direct light highlights, reflection mask dims environment/probe reflections (incl. clearcoat env, Light Volume, and LTCGI specular). Both are 1.0 (no effect) unless _UsePackedMasks is on.
            half specMask = s.SpecularMask;
            half reflMask = s.ReflectionMask;

            // Combine
            half3 finalColor =
                gi.diffuse * diffColor * baseEnergy * rawAO +           // indirect diffuse (Poiyomi-realistic: raw scalar AO, no multi-bounce)
                baseDiffuse +                                            // direct diffuse (Burley)
                sssColor +
                transmission +
                baseSpecular * specMask +
                ccSpecular * specMask +
                indirectBaseSpec * reflMask +
                indirectCCSpec * reflMask +
                s.LVSpec * baseEnergy * baseSpecOcc * reflMask +
                s.LVCCSpec * s.ClearcoatStrength * thinFilmColor * ccSpecOcc * reflMask +
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

                half3 ltcgiDiff     = base_ltc_d * diffColor * baseEnergy * rawAO * _LTCGI_Diff_Mix;
                half3 ltcgiBaseSpec = base_ltc_s * specColor * baseEnergy * baseSpecOcc * _LTCGI_Spec_Mix * baseMS;
                half3 ltcgiCCSpec   = cc_ltc_s * ccFresEnv * thinFilmColor * ccSpecOcc * _LTCGI_Spec_Mix;

                finalColor += (ltcgiDiff + (ltcgiBaseSpec + ltcgiCCSpec) * reflMask) * _LTCGI_Int;
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
            fixed4 c      = UNITY_SAMPLE_TEX2D(_MainTex, finalUV) * _Color;
            fixed4 packed = tex2D(_MetallicGlossMap, finalUV);

            // Fracture dissolve clip - the body opens up as the fracture progresses (manual _Vtx_Fracture_Amount plus AudioLink jitter). On non-SPS the removed region flies off as real shards in PASS 4; on SPS it simply dissolves.
            float fracProg = saturate(_Vtx_Fracture_Amount + (_UseAudioLink > 0.5 ? GET_AL_BAND(amps, _Vtx_Fracture_Band) * _Vtx_Fracture_Str * 0.2 : 0.0));
            if (_UseVtxKinetic > 0.5 && fracProg > 0.001)
            {
                float fractureNoise = frac(sin(dot(finalUV * 512.0, float2(12.9898,78.233))) * 43758.5453);
                clip(fractureNoise - fracProg);
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

            // AO (channel selectable); "None" (channel 4) yields a constant 1.0 so Poiyomi/Mochie packs without an AO channel don't read a wrong channel.
            float pbrAO = (_PBR_AO_Ch > 3.5) ? 1.0 : ChannelPick(packed, _PBR_AO_Ch);
            o.Occlusion = saturate(pbrAO * _AO_Str);
            if (_AL_Scanlines > 0.0 && _UseAudioLink > 0.5)
                o.Occlusion = lerp(o.Occlusion, 1.0, amp_scan * 0.2);

            // Height (channel selectable; parallax raymarch and BRDF shadow trace use the same channel).
            float pbrHeight = ChannelPick(packed, _PBR_Height_Ch);
            o.Height = pbrHeight * _Disp_Str;

            // Poiyomi/Mochie packed-map masks - reads reflection + specular masks from the packed PBR map so a Mochie "Metallic Maps" texture (R:Met G:Smooth B:ReflMask A:SpecMask) drives our masking. Default off keeps both masks neutral (1.0); applied in the BRDF combine - reflection mask dims environment/probe specular, specular mask dims direct highlights.
            o.ReflectionMask = 1.0;
            o.SpecularMask   = 1.0;
            if (_UsePackedMasks > 0.5)
            {
                float reflM = ChannelPick(packed, _ReflMask_Ch);
                if (_ReflMask_Inv > 0.5) reflM = 1.0 - reflM;
                o.ReflectionMask = lerp(1.0, reflM, saturate(_ReflMask_Str));

                float specM = ChannelPick(packed, _SpecMask_Ch);
                if (_SpecMask_Inv > 0.5) specM = 1.0 - specM;
                o.SpecularMask = lerp(1.0, specM, saturate(_SpecMask_Str));
            }

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

            // Polish layer master gate + B&W mask - sampled once here, applied to the whole polish layer in the BRDF. Default white mask + toggle on = 1 (full polish, historical look).
            o.PolishMask = _UsePolish * ChannelPick(UNITY_SAMPLE_TEX2D_SAMPLER(_PolishMask, _MainTex, finalUV), _PolishMaskCh);

            // WET - full "soaked / just out of the shower" look plus run-off rivulets. The soak (darken + near-mirror gloss + water-film sheen + flattened micro-normal) covers the whole masked area; animated UV-vertical rivulets add concentrated run-off streaks on top. UV-space keeps it stable on skinned avatars. Own toggle so it costs nothing when off.
            if (_UseDrip > 0.5)
            {
                float wetMaskTex = ChannelPick(UNITY_SAMPLE_TEX2D_SAMPLER(_DripMask, _MainTex, finalUV), _DripMaskCh);
                if (wetMaskTex > 0.001)
                {
                    // Run-off rivulets: animated vertical streaks where extra water is pouring down. Computed first; the normal tilt is applied last so streaks still pop over the flattened film.
                    float rivulet = 0;
                    float rivuletSlope = 0;
                    if (_Drip_Strength > 0.0001)
                    {
                        float colF    = finalUV.x * _Drip_Density;
                        float col     = floor(colF);
                        float colHash = frac(sin(col * 91.17) * 43758.5453);
                        // Coverage gate - only a fraction of columns carry a rivulet.
                        float hasCol  = step(1.0 - saturate(_Drip_Coverage), colHash);
                        // Gaussian rivulet across the column (centre is wettest); higher _Drip_Width = thinner streak.
                        float xInCol  = frac(colF) - 0.5;
                        float ridge   = exp(-xInCol * xInCol * _Drip_Width);
                        // Downward flow - per-column speed/phase variance so streaks don't march in lockstep.
                        float flow    = finalUV.y - _Time.y * _Drip_Speed * (0.6 + colHash) - colHash * 7.0;
                        // Travelling beads so it reads as running water; 0.35 floor keeps a continuous trickle between beads.
                        float bead    = sin(flow * 18.0) * 0.5 + 0.5;
                        bead          = bead * bead;
                        rivulet       = ridge * hasCol * saturate(0.35 + bead) * _Drip_Strength;
                        // Gaussian derivative across the streak - rounds it so it catches a glint.
                        rivuletSlope  = clamp(-2.0 * xInCol * _Drip_Width * ridge * hasCol, -4.0, 4.0);
                    }

                    // Total wetness: global soak + rivulet streaks, masked and clamped.
                    float wetness = saturate(_Wet_Amount + rivulet) * wetMaskTex;
                    if (wetness > 0.001)
                    {
                        // 1. Water absorption darkens the surface (deeper in the most-soaked areas).
                        o.Albedo *= lerp(1.0, 1.0 - _Wet_Darken * 0.65, wetness);
                        // 2. A water film is near-mirror smooth - drive smoothness toward the wet target.
                        o.Smoothness    = lerp(o.Smoothness, _Wet_Smoothness, wetness);
                        o.BaseRoughness = 1.0 - o.Smoothness;
                        // 3. The film fills micro-detail, flattening the shading normal toward the surface.
                        o.Normal = normalize(lerp(o.Normal, float3(0,0,1), wetness * _Wet_Flatten));
                        // 4. The thin water sheet reads as an extra dielectric clearcoat (F0~0.04 = water), giving the bright wet Fresnel sheen. Gated by the Polish layer in the BRDF.
                        o.ClearcoatStrength = saturate(o.ClearcoatStrength + wetness * _Wet_Sheen);
                        // Run-off streak tilt applied last so it survives the film flattening.
                        o.Normal = normalize(o.Normal + float3(rivuletSlope * _Drip_Normal * 0.15, 0, 0));
                    }
                }
            }

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
                // autoCorr is zero-centered via the 0.007 scale (matches the SPS variant); no -0.5 offset.
                emisUV.y += autoCorr * _AL_AutoCorr_Mod * 0.2;
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

            // CYBER HUD now renders as real lifted geometry in its own pass (see "PASS 3: CYBER HUD HOVER" below) instead of being parallax-faked onto the surface here.

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

        // PASS 2: CLEAR DRIP (geometry-amplified water droplets) - PC only. A real geometry stage emits camera-facing droplet billboards from downward-facing, wet-masked triangles; each droplet swells, forms a neck, pinches off, then falls away as free geometry and dries out (fades). Surface shaders cannot host a geometry stage, so this is its own custom vert/geom/frag pass. Runtime-gated by _UseDrip and _Drip3D_Strength so it stays VRCFury-animatable and emits zero vertices when off. Droplets are tinted to the clearcoat color.
        Pass
        {
            Tags { "LightMode" = "ForwardBase" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off

            CGPROGRAM
            #pragma vertex dripVert
            #pragma geometry dripGeom
            #pragma fragment dripFrag
            #pragma target 5.0
            #pragma require geometry
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            sampler2D _DripMask;
            float _DripMaskCh;
            fixed4 _CC_Tint;
            float _UseDrip, _Drip3D_Strength, _Drip3D_Scale, _Drip3D_Sheen, _Drip3D_Fall;
            float _Drip_Coverage, _Drip_Speed;
            float _Drip_Sway, _Drip_BodyFollow, _Drip_FloorCollide, _Goo_GroundY;

            struct dripAppdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct dripV2G
            {
                float3 wpos : TEXCOORD0;
                float3 wnormal : TEXCOORD1;
                float2 uv : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct dripG2F
            {
                float4 pos : SV_POSITION;
                float2 luv : TEXCOORD0;       // billboard local coords: x in [-1,1], y in [0,1] (top to bottom)
                float3 wpos : TEXCOORD1;
                float3 params : TEXCOORD2;    // x = beadCenterY, y = neck width factor, z = envelope alpha
                float3 bRight : TEXCOORD3;
                float3 bUp : TEXCOORD4;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float dripHash(float3 p) { return frac(sin(dot(p, float3(12.9898, 78.233, 37.719))) * 43758.5453); }
            float dripChan(fixed4 t, float c) { return (c < 0.5) ? t.r : (c < 1.5) ? t.g : (c < 2.5) ? t.b : t.a; }

            dripV2G dripVert(dripAppdata v)
            {
                dripV2G o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.wpos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.wnormal = UnityObjectToWorldNormal(v.normal);
                o.uv = v.uv;
                return o;
            }

            [maxvertexcount(4)]
            void dripGeom(triangle dripV2G p[3], inout TriangleStream<dripG2F> stream)
            {
                // Runtime gate - emit nothing when the effect is off.
                if (_UseDrip < 0.5 || _Drip3D_Strength < 0.0001) return;

                UNITY_SETUP_INSTANCE_ID(p[0]);

                float3 C = (p[0].wpos + p[1].wpos + p[2].wpos) / 3.0;
                float3 N = normalize(p[0].wnormal + p[1].wnormal + p[2].wnormal);
                float2 uv = (p[0].uv + p[1].uv + p[2].uv) / 3.0;

                // Drips form on downward-facing surfaces - skip up-facing triangles.
                float facingDown = saturate(-N.y);
                if (facingDown < 0.08) return;

                // Wet mask gate (same mask as the Wet layer).
                float mask = dripChan(tex2Dlod(_DripMask, float4(uv, 0, 0)), _DripMaskCh);
                if (mask < 0.1) return;

                // Per-triangle identity + sparse coverage so droplets scatter instead of covering every triangle.
                float h = dripHash(floor(C * 80.0));
                if (h > saturate(_Drip_Coverage) * 0.5) return;

                // Lifecycle phase (staggered per emitter).
                float phase = frac(_Time.y * _Drip_Speed * (0.5 + h) + h);
                float swell = smoothstep(0.0, 0.18, phase);
                float pinch = smoothstep(0.34, 0.46, phase);            // 0 attached, 1 detached
                float fall  = saturate((phase - 0.42) / 0.5);
                float dry   = 1.0 - smoothstep(0.85, 1.0, phase);
                float envAlpha = swell * dry * mask * facingDown;
                if (envAlpha < 0.01) return;

                // Sizes in world units (a droplet is a few millimetres).
                float beadR = (0.5 + 0.5 * swell) * _Drip3D_Scale * 0.004;
                float hangLen = beadR * 3.0 * (1.0 - pinch);            // neck length, retracts at pinch
                float fallDist = fall * fall * _Drip3D_Fall * 1.5;      // accelerating free-fall distance

                float3 worldDown = float3(0, -1, 0);

                // BODY SLIDE - while still attached, the bead clings and runs DOWN ALONG the surface (downhill tangent) rather than hanging straight from the centroid; a detached drop falls under gravity.
                float3 hangDir = worldDown;
                if (_Drip_BodyFollow > 0.0001)
                {
                    float3 tang = worldDown - N * dot(worldDown, N);
                    float tl = length(tang);
                    float3 surfDown = (tl > 1e-4) ? tang / tl : worldDown;
                    hangDir = normalize(lerp(worldDown, surfDown, saturate(_Drip_BodyFollow)) + 1e-5);
                }

                float3 beadCenter = C + hangDir * (hangLen + beadR) + worldDown * fallDist;

                // PHYSICS - sway (surface-tension wobble + breeze) grows with fall distance so a fresh bead barely moves while a long thread trails and swings.
                float swayPh = _Time.y * 3.0 + h * 6.2831;
                float2 swayXZ = float2(sin(swayPh), sin(swayPh * 0.7 + 1.3)) * (_Drip_Sway * 0.15 * fallDist);
                beadCenter.xz += swayXZ;

                float3 topPoint = lerp(C, beadCenter - worldDown * beadR, pinch);

                // FLOOR COLLISION - when the bead reaches the shared world floor (_Goo_GroundY) it pins to the floor and splats into a spreading puddle that fades as it dries.
                float splat = 0.0;
                if (_Drip_FloorCollide > 0.5)
                {
                    float below = (_Goo_GroundY + beadR) - beadCenter.y;
                    splat = saturate(below / max(beadR * 2.0, 1e-4));
                    if (splat > 0.0) beadCenter.y = _Goo_GroundY + beadR * 0.2;
                }

                // Camera-facing billboard basis with world-up kept vertical so the drop hangs naturally.
                float3 viewDir = normalize(_WorldSpaceCameraPos - beadCenter);
                float3 bRight = normalize(cross(float3(0, 1, 0), viewDir));
                float3 bUp = normalize(cross(viewDir, bRight));

                float3 colTop = topPoint;
                float3 colBot = beadCenter - bUp * beadR;
                float halfW = beadR * 1.3;
                float totalLen = max(length(colTop - colBot), 1e-4);
                float beadCenterY = saturate(length(colTop - beadCenter) / totalLen);
                float neckW = 1.0 - pinch;

                float3 vTL = colTop - bRight * halfW;
                float3 vTR = colTop + bRight * halfW;
                float3 vBL = colBot - bRight * halfW;
                float3 vBR = colBot + bRight * halfW;

                // SPLAT MORPH - collapse the vertical drop into a flat, ground-aligned puddle disc that grows as it spreads and fades out.
                if (splat > 0.001)
                {
                    float pr = beadR * (1.0 + splat * 4.0);
                    float3 pc = float3(beadCenter.x, _Goo_GroundY + 0.0005, beadCenter.z);
                    float3 pX = float3(pr, 0, 0);
                    float3 pZ = float3(0, 0, pr);
                    vTL = pc - pX - pZ; vTR = pc + pX - pZ;
                    vBL = pc - pX + pZ; vBR = pc + pX + pZ;
                    bRight = float3(1, 0, 0);
                    bUp = float3(0, 0, 1);
                    beadCenterY = 0.5;
                    neckW = 0.0;
                    envAlpha *= (1.0 - splat * 0.85);
                }

                dripG2F o;
                UNITY_INITIALIZE_OUTPUT(dripG2F, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.params = float3(beadCenterY, neckW, envAlpha);
                o.bRight = bRight;
                o.bUp = bUp;

                o.pos = UnityWorldToClipPos(vTL); o.luv = float2(-1, 0); o.wpos = vTL; stream.Append(o);
                o.pos = UnityWorldToClipPos(vTR); o.luv = float2( 1, 0); o.wpos = vTR; stream.Append(o);
                o.pos = UnityWorldToClipPos(vBL); o.luv = float2(-1, 1); o.wpos = vBL; stream.Append(o);
                o.pos = UnityWorldToClipPos(vBR); o.luv = float2( 1, 1); o.wpos = vBR; stream.Append(o);
            }

            fixed4 dripFrag(dripG2F i) : SV_Target
            {
                float beadCenterY = i.params.x;
                float neckW = i.params.y;
                float envAlpha = i.params.z;

                float x = i.luv.x;
                float y = i.luv.y;

                // Bead - a soft disc centred at (0, beadCenterY).
                float2 bp = float2(x, (y - beadCenterY) / max(1.0 - beadCenterY, 1e-4));
                float beadD = length(bp);
                float bead = smoothstep(1.0, 0.6, beadD);

                // Neck - a tapering column above the bead that vanishes as the drop pinches off.
                float neckHalf = lerp(0.12, 0.5, saturate(y / max(beadCenterY, 1e-4))) * neckW;
                float neck = smoothstep(neckHalf, neckHalf - 0.06, abs(x)) * step(y, beadCenterY) * neckW;

                float shape = saturate(max(bead, neck));
                if (shape < 0.02) discard;

                // Spherical normal across the bead for a glassy fresnel + reflection.
                float2 sp = clamp(bp, -1.0, 1.0);
                float3 nLocal = normalize(float3(sp.x, -sp.y, sqrt(saturate(1.0 - dot(sp, sp))) + 0.2));
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.wpos);
                float3 nWorld = normalize(i.bRight * nLocal.x + i.bUp * nLocal.y + viewDir * nLocal.z);

                float fres = pow(1.0 - saturate(dot(nWorld, viewDir)), 3.0);
                half4 cube = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflect(-viewDir, nWorld), 2);
                half3 sky = DecodeHDR(cube, unity_SpecCube0_HDR);

                float3 waterCol = _CC_Tint.rgb;
                float3 col = waterCol * 0.35 + sky * _Drip3D_Sheen * (0.4 + fres);
                float spec = pow(saturate(nLocal.z), 60.0);
                col += spec * _Drip3D_Sheen;

                float alpha = saturate(shape * envAlpha * (0.4 + 0.6 * fres) * _Drip3D_Strength);
                return fixed4(col, alpha);
            }
            ENDCG
        }

        // PASS 3: CYBER HUD HOVER (geometry-amplified holographic shell) - PC only. Each body triangle whose centroid falls inside the Cyber mask is duplicated and pushed out along its world normal by _Cyber_Hover (plus a subtle bob), so the masked HUD window literally floats off the suit instead of being parallax-faked onto it; the five HUD layers (VU, Spectrum, Waveform, DMX, Autocorrelator) are drawn on that lifted shell. Surface shaders cannot host a geometry stage, so this is its own vert/geom/frag pass, runtime-gated by _UseCyber so it emits zero vertices when off. Kept off the SPS variant because VRCFury's SPS patcher rewrites the vertex stage.
        Pass
        {
            Tags { "LightMode" = "ForwardBase" }
            Blend One One
            ZWrite Off
            ZTest LEqual
            Cull Off

            CGPROGRAM
            #pragma vertex hudVert
            #pragma geometry hudGeom
            #pragma fragment hudFrag
            #pragma target 5.0
            #pragma require geometry
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #include "Packages/com.vixencreations.vixens-toolbox/Editor/Avatar Tools/Shaders/cginc/AudioLink.cginc"

            sampler2D _CyberMask;
            sampler2D _Udon_DMXGridRenderTexture;
            fixed4 _EmissionColor;
            float _UseCyber, _Cyber_Hover, _Cyber_Hover_Bob;
            float _UseCyberVU, _Cyber_VU_Style, _Cyber_VU_Str; float4 _Cyber_VU_Transform;
            float _UseCyberCC, _Cyber_CC_Band, _Cyber_CC_Str, _Cyber_CC_Density; float4 _Cyber_CC_Transform;
            float _UseCyberWave, _Cyber_Wave_Band, _Cyber_Wave_Str; float4 _Cyber_Wave_Transform;
            float _UseCyberDMX, _Cyber_DMX_Band, _Cyber_DMX_Str; float4 _Cyber_DMX_Transform;
            float _UseCyberAuto, _Cyber_Auto_Band, _Cyber_AutoCorr_Str; float4 _Cyber_Auto_Transform;
            float _Cyber_Auto_Shimmer, _Cyber_Auto_Shimmer_Band, _Cyber_Auto_Pop, _Cyber_Auto_Pop_Band;
            float _Cyber_Auto_Sizzle, _Cyber_Auto_Sizzle_Band, _Cyber_Auto_Electrify, _Cyber_Auto_Electrify_Band;
            float _UseAudioLink, _UseMediaState, _AL_ColorMode, _AL_Strip_Pos, _AL_Band_Emission;
            uniform float _MediaPlaying;

            // Safe vector indexing, mirror of the surf-pass GET_AL_BAND macro.
            #define HUD_AL_BAND(vec, bandIdx) ( \
                ((int)(bandIdx) == 0) ? vec.x : \
                ((int)(bandIdx) == 1) ? vec.y : \
                ((int)(bandIdx) == 2) ? vec.z : \
                vec.w )

            // HUD layer placement (offset/scale/rotation), identical to the surf-pass TransformHUD.
            float2 HudTransform(float2 uv, float4 transform)
            {
                float2 outUV = uv - 0.5 - transform.xy;
                outUV /= max(0.001, transform.z);
                float rad = transform.w * (UNITY_PI / 180.0);
                float s = sin(rad), c = cos(rad);
                outUV = mul(outUV, float2x2(c, -s, s, c));
                return outUV + 0.5;
            }

            // Footprint placement only (offset + scale, rotation ignored). Effect bounds use this so spinning
            // an effect via Rotation never reshapes its lit/emission area - it only orients the meter graphic,
            // which is still sampled from the full HudTransform above.
            float2 HudPlace(float2 uv, float4 transform)
            {
                return HudTransform(uv, float4(transform.xy, transform.z, 0.0));
            }

            // Per-effect ColorChord/Theme colour. Each HUD layer passes its own band so it can light up
            // with a different note colour; Theme and Strip modes ignore the band. Emission is the idle
            // fallback when AudioLink is off or paused.
            float3 HudBandColor(int band)
            {
                float3 c = _EmissionColor.rgb;
                bool active = (_UseAudioLink > 0.5) && !(_UseMediaState > 0.5 && _MediaPlaying < 0.5);
                if (active && AudioLinkIsAvailable())
                {
                    int colorMode = (int)_AL_ColorMode;
                    if (colorMode == 1)
                        c = AudioLinkData(ALPASS_CCCOLORS + int2((int)((uint)band % 11u) + 1, 0)).rgb;
                    else if (colorMode >= 2 && colorMode <= 5)
                        c = AudioLinkData(ALPASS_THEME_COLOR0 + int2(colorMode - 2, 0)).rgb;
                    else if (colorMode == 6)
                        c = AudioLinkData(ALPASS_CCSTRIP + int2((int)(saturate(_AL_Strip_Pos) * 127.0), 0)).rgb;
                }
                return c;
            }

            // The VU meter listens to every band at once: an amplitude-weighted blend of the four band
            // colours (a small floor keeps a silent mix as an even blend instead of going black).
            float3 HudAllBandColor(float4 amps)
            {
                float4 w = amps + 0.05;
                float3 c = HudBandColor(0) * w.x + HudBandColor(1) * w.y
                         + HudBandColor(2) * w.z + HudBandColor(3) * w.w;
                return c / (w.x + w.y + w.z + w.w);
            }

            // Band-independent feeds shared by every HUD layer: the four band amplitudes and the scrolling
            // raw waveform. Per-effect colour now comes from HudBandColor so each layer can pick its own band.
            void HudFetchAL(float2 uv, out float4 amps, out float raw_waveform)
            {
                amps = 0;
                raw_waveform = 0;

                bool active = (_UseAudioLink > 0.5) && !(_UseMediaState > 0.5 && _MediaPlaying < 0.5);
                if (active && AudioLinkIsAvailable())
                {
                    float4 a;
                    a.x = AudioLinkData(ALPASS_AUDIOLINK + int2(0, 0)).r;
                    a.y = AudioLinkData(ALPASS_AUDIOLINK + int2(0, 1)).r;
                    a.z = AudioLinkData(ALPASS_AUDIOLINK + int2(0, 2)).r;
                    a.w = AudioLinkData(ALPASS_AUDIOLINK + int2(0, 3)).r;
                    amps = saturate(pow(a * 4.0, 0.35));

                    float wavePhase = frac(uv.y * 2.0 - _Time.y * 0.2);
                    raw_waveform = AudioLinkData(ALPASS_WAVEFORM + int2((int)(wavePhase * 128.0), 0)).r - 0.5;
                }
            }

            struct hudAppdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct hudV2G
            {
                float3 opos : TEXCOORD0;
                float3 onormal : TEXCOORD1;
                float2 uv : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct hudG2F
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            hudV2G hudVert(hudAppdata v)
            {
                hudV2G o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.opos = v.vertex.xyz;
                o.onormal = v.normal;
                o.uv = v.uv;
                return o;
            }

            [maxvertexcount(3)]
            void hudGeom(triangle hudV2G p[3], inout TriangleStream<hudG2F> stream)
            {
                // Runtime gate - emit nothing when the HUD is off.
                if (_UseCyber < 0.5) return;

                UNITY_SETUP_INSTANCE_ID(p[0]);

                // Mask gate: lift any triangle with at least one corner on the white side of the mask, so
                // boundary triangles survive for the fragment stage to razor-clip and the shell never covers
                // the black (transparent) region of the body.
                float m0 = tex2Dlod(_CyberMask, float4(p[0].uv, 0, 0)).r;
                float m1 = tex2Dlod(_CyberMask, float4(p[1].uv, 0, 0)).r;
                float m2 = tex2Dlod(_CyberMask, float4(p[2].uv, 0, 0)).r;
                if (max(m0, max(m1, m2)) < 0.5) return;

                // World-space lift distance along the surface normal, with the subtle bob from the old hover sliders.
                float lift = _Cyber_Hover + sin(_Time.y * 1.6) * _Cyber_Hover * _Cyber_Hover_Bob * 0.25;

                hudG2F o;
                UNITY_INITIALIZE_OUTPUT(hudG2F, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                [unroll]
                for (int k = 0; k < 3; k++)
                {
                    float3 wp = mul(unity_ObjectToWorld, float4(p[k].opos, 1.0)).xyz;
                    float3 wn = UnityObjectToWorldNormal(p[k].onormal);
                    wp += wn * lift;
                    o.pos = UnityWorldToClipPos(wp);
                    o.uv = p[k].uv;
                    stream.Append(o);
                }
            }

            // ===== LIVING VU CONSOLE =====
            // A self-playing AudioLink control panel ported from AudioLinkUI-Functions.cginc. The slider/handle INPUTS
            // (band thresholds, gain, hit-fade, exp-falloff) are fed live audio instead of user values, so the console
            // animates itself. MSDF icon buttons (power/reset/autogain) and the HSV theme pickers are omitted - they need
            // textures this shader doesn't ship. SDF primitives transcribed from the upstream panel.
            static const float3 VU_BG       = 0.033;
            static const float3 VU_FG       = 0.075;
            static const float3 VU_INACTIVE = 0.13;
            static const float3 VU_ACTIVE   = 0.8;
            static const float3 VU_BASS    = pow(float3(147.0/255.0, 39.0/255.0, 143.0/255.0), 2.2);
            static const float3 VU_LOWMID  = pow(float3(76.0/255.0, 53.0/255.0, 18.0/255.0), 2.2);
            static const float3 VU_HIGHMID = pow(float3(42.0/255.0, 60.0/255.0, 19.0/255.0), 2.2);
            static const float3 VU_HIGH    = pow(float3(12.0/255.0, 52.0/255.0, 68.0/255.0), 2.2);
            static const float VU_CORNER  = 0.025;
            static const float VU_MARGIN  = 0.03;
            static const float VU_HANDLE  = 0.007;
            static const float VU_OUTLINE = 0.002;

            // Shared HDR glow multiplier so every HUD toggle reaches comparable brightness at a given
            // intensity slider value. The VU console scales this up (its SDR panel palette tops out well
            // below 1.0 once the dark background floor is subtracted, see hudFrag).
            #define HUD_GLOW 10.0

            #define vu_remap(value, low1, high1, low2, high2) ((low2) + ((value) - (low1)) * ((high2) - (low2)) / ((high1) - (low1)))
            #define VU_COHERENT(condition) ((condition) || any(fwidth(condition)))
            #define VU_ADD(existing, elementColor, elementDist) [branch] if (VU_COHERENT(elementDist <= 0.01)) vuAddElement(existing, elementColor, elementDist)

            float3 vuSelectColor(uint i, float3 a, float3 b, float3 c, float3 d)
            {
                return float4x4(float4(a, 0.0), float4(b, 0.0), float4(c, 0.0), float4(d, 0.0))[i % 4];
            }
            float3 vuGetBandColor(uint i) { return vuSelectColor(i, VU_BASS, VU_LOWMID, VU_HIGHMID, VU_HIGH); }
            float3 vuSelectColorLerp(float i, float3 a, float3 b, float3 c, float3 d)
            {
                int me = floor(i);
                float3 meColor = vuSelectColor(me, a, b, c, d);
                if (VU_COHERENT(distance(frac(i), 0.5) < 0.1)) return meColor;
                int side = sign(frac(i) - 0.5);
                int other = clamp(me + side, 0, 3);
                float3 otherColor = vuSelectColor(other, a, b, c, d);
                float dist = round(i) - i;
                const float pd = sqrt(2.0) / 2.0 * side;
                float ddl = sqrt(pow(ddx(dist), 2) + pow(ddy(dist), 2));
                return lerp(otherColor, meColor, smoothstep(-pd, pd, dist / ddl));
            }
            float3 vuGetBandColorLerp(float i) { return vuSelectColorLerp(i, VU_BASS, VU_LOWMID, VU_HIGHMID, VU_HIGH); }
            float vuGetBandAmplitudeLerp(float i, float delay)
            {
                int me = floor(i);
                float meStrength = AudioLinkLerp(float2(delay, me)).r;
                if (VU_COHERENT(distance(frac(i), 0.5) < 0.1)) return meStrength;
                int side = sign(frac(i) - 0.5);
                int other = clamp(me + side, 0, 3);
                float otherStrength = AudioLinkLerp(float2(delay, other)).r;
                float dist = round(i) - i;
                const float pd = sqrt(2.0) / 2.0 * side;
                float ddl = sqrt(pow(ddx(dist), 2) + pow(ddy(dist), 2));
                return lerp(otherStrength, meStrength, smoothstep(-pd, pd, dist / ddl));
            }

            float2x2 vuRotMat(float a) { return float2x2(cos(a), -sin(a), sin(a), cos(a)); }
            float2 vuTranslate(float2 p, float2 o) { return p - o; }
            float2 vuRotate(float2 p, float a) { return mul(vuRotMat(a), p); }
            float vuShell(float d, float t) { return abs(d) - t; }
            float vuInflate(float d, float t) { return d - t; }
            float vuLerpstep(float a, float b, float x) { return saturate((x - a) / (b - a)); }
            void vuAddElement(inout float3 existing, float3 elementColor, float elementDist)
            {
                const float pd = sqrt(2.0) / 2.0;
                float ddl = sqrt(pow(ddx(elementDist), 2) + pow(ddy(elementDist), 2));
                existing = lerp(elementColor, existing, vuLerpstep(-pd, pd, elementDist / ddl));
            }
            float vuRoundedBoxC(float2 p, float2 b, float4 r)
            {
                r.xy = (p.x > 0.0) ? r.xy : r.zw;
                r.x = (p.y > 0.0) ? r.x : r.y;
                float2 q = abs(p) - b * 0.5 + r.x;
                return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - r.x;
            }
            float vuRoundedBoxTL(float2 p, float2 b, float4 r) { return vuRoundedBoxC(vuTranslate(p, b * 0.5), b, r); }
            float vuRoundedBoxBR(float2 p, float2 b, float4 r) { return vuRoundedBoxC(vuTranslate(p, float2(b.x, -b.y) * 0.5), b, r); }
            float vuSphere(float2 p, float r) { return length(p) - r; }
            float vuTriIso(float2 p, float2 q)
            {
                p.x = abs(p.x);
                float2 a = p - q * clamp(dot(p, q) / dot(q, q), 0.0, 1.0);
                float2 b = p - q * float2(clamp(p.x / q.x, 0.0, 1.0), 1.0);
                float s = -sign(q.y);
                float2 d = min(float2(dot(a, a), s * (p.x * q.y - p.y * q.x)), float2(dot(b, b), s * (p.y - q.y)));
                return -sqrt(d.x) * sign(d.y);
            }
            float vuTriRight(float2 p, float hw, float hh)
            {
                float2 end = float2(hw, -hh);
                float2 d = p - end * clamp(dot(p, end) / dot(end, end), -1.0, 1.0);
                if (max(d.x, d.y) > 0.0) return length(d);
                p += float2(hw, hh);
                if (max(p.x, p.y) > 0.0) return -min(length(d), min(p.x, p.y));
                return length(p);
            }

            // Top spectrum area: 4 threshold/crossover boxes + handles over the live DFT waveform. threshold[]/crossover[]/gain are audio-driven.
            float3 vuDrawTopArea(float2 uv, float threshold[4], float crossover[4], float gain)
            {
                float3 color = VU_FG;
                float areaWidth = 1.0 - VU_MARGIN * 2;
                float areaHeight = 0.35;
                float handleWidth = 0.015 * areaWidth;
                float xo[4] = { crossover[0] * areaWidth, crossover[1] * areaWidth, crossover[2] * areaWidth, crossover[3] * areaWidth };

                uint start = 0; uint stop = 4;
                float currentBoxOffset = xo[0];
                float boxOffsets[4] = {0, 0, 0, 0};
                float boxWidths[4] = {0, 0, 0, 0};
                for (uint bi = 0; bi < 4; bi++)
                {
                    // if/else (not a ternary) so FXC dead-code-eliminates the xo[bi+1] read at bi==3 - a ternary evaluates both operands and reads xo[4] out of bounds (X3504).
                    float boxWidth;
                    if (bi == 3)
                        boxWidth = areaWidth - currentBoxOffset;
                    else
                        boxWidth = xo[bi + 1] - xo[bi];
                    boxOffsets[bi] = currentBoxOffset;
                    boxWidths[bi] = boxWidth;
                    if (VU_COHERENT(uv.x > currentBoxOffset + VU_OUTLINE)) start = bi;
                    if (VU_COHERENT(uv.x < currentBoxOffset + boxWidth - handleWidth)) stop = min(stop, bi + 1);
                    currentBoxOffset += boxWidth;
                }

                uint totalBins = AUDIOLINK_EXPBINS * AUDIOLINK_EXPOCT;
                uint noteno = AudioLinkRemap(uv.x, 0., 1., AUDIOLINK_4BAND_FREQFLOOR * totalBins, AUDIOLINK_4BAND_FREQCEILING * totalBins);
                float notenof = AudioLinkRemap(uv.x, 0., 1., AUDIOLINK_4BAND_FREQFLOOR * totalBins, AUDIOLINK_4BAND_FREQCEILING * totalBins);
                float4 specLow  = AudioLinkData(float2(fmod(noteno, 128), (noteno / 128) + 4.0));
                float4 specHigh = AudioLinkData(float2(fmod(noteno + 1, 128), ((noteno + 1) / 128) + 4.0));
                float4 intensity = lerp(specLow, specHigh, frac(notenof)) * gain;
                float bandIntensity = AudioLinkData(float2(0., start ^ 0));
                float funcY = areaHeight - (intensity.g * areaHeight);
                float waveformDist = smoothstep(0.005, 0.003, funcY - uv.y);
                float waveformDistAbs = abs(smoothstep(0.005, 0.003, abs(funcY - uv.y)));
                color = lerp(color, color * 2, waveformDist);
                color = lerp(color, color * 2, waveformDistAbs);

                #if defined(UNITY_PBS_USE_BRDF2) || defined(SHADER_API_MOBILE)
                [loop] for (uint i = start; i < min(stop, 4); i++)
                #else
                for (uint i = 0; i < 4; i++)
                #endif
                {
                    float boxHeight = threshold[i] * areaHeight;
                    float boxWidth = boxWidths[i];
                    float boxOffset = boxOffsets[i];
                    float leftR = i == 0 ? VU_CORNER : 0.0;
                    float rightR = i == 3 ? VU_CORNER : 0.0;
                    float boxDist = vuRoundedBoxBR(vuTranslate(uv, float2(boxOffset, areaHeight)), float2(boxWidth, boxHeight), float4(rightR, VU_CORNER, leftR, VU_CORNER));
                    float3 innerColor = vuGetBandColor(i);
                    innerColor = lerp(innerColor, innerColor * 3, waveformDist);
                    innerColor = lerp(innerColor, lerp(innerColor * 3, 1.0, bandIntensity > threshold[i]), waveformDistAbs);
                    VU_ADD(color, innerColor, boxDist + VU_OUTLINE);
                    float shellDist = vuShell(boxDist, VU_OUTLINE);
                    VU_ADD(color, VU_ACTIVE, shellDist);
                    float handleDist = vuSphere(vuTranslate(uv, float2(boxWidth * 0.5 + boxOffset, areaHeight - boxHeight)), VU_HANDLE);
                    VU_ADD(color, 1.0, handleDist);
                    handleDist = vuRoundedBoxC(vuTranslate(uv, float2(boxOffset, areaHeight - boxHeight * 0.5)), float2(handleWidth, 0.35 * boxHeight), VU_HANDLE);
                    VU_ADD(color, 1.0, handleDist);
                }
                return color;
            }

            float3 vuDrawGainArea(float2 uv, float2 size, float gain)
            {
                float3 color = VU_FG;
                float t = gain / 2.0;
                const float sliderOffsetLeft = 0.04;
                const float sliderOffsetRight = 0.02;
                float maxTriangleWidth = size.x - sliderOffsetLeft - sliderOffsetRight;
                float bgTri = vuInflate(vuTriIso(vuRotate(vuTranslate(uv, float2(sliderOffsetLeft, size.y * 0.5)), UNITY_PI * 0.5), float2(size.y * 0.3, maxTriangleWidth)), 0.002);
                VU_ADD(color, VU_INACTIVE, bgTri);
                float curW = maxTriangleWidth * t;
                float curTri = max(bgTri, uv.x - curW - sliderOffsetLeft);
                VU_ADD(color, VU_ACTIVE, curTri);
                float handleDist = vuSphere(vuTranslate(uv, float2(curW + sliderOffsetLeft, size.y * 0.5)), VU_HANDLE);
                VU_ADD(color, VU_ACTIVE, handleDist);
                float gripDist = abs(uv.x - curW - sliderOffsetLeft) - VU_OUTLINE;
                VU_ADD(color, VU_ACTIVE, gripDist);
                return color;
            }

            float3 vuDrawHitFadeArea(float2 uv, float2 size, float hitFade)
            {
                float3 color = VU_FG;
                float2 triUV = -(uv - float2(size.x / 2, size.y / 2));
                float hw = 0.45 * size.x; float hh = 0.37 * size.y;
                float fullW = hw * 2;
                float bgTri = vuInflate(vuTriRight(triUV, hw, hh), 0.002);
                VU_ADD(color, VU_INACTIVE, bgTri);
                float marginX = (size.x - fullW) / 2;
                float invHF = 1 - hitFade;
                triUV.x += hw * invHF;
                float fgTri = vuInflate(vuTriRight(triUV, hw * hitFade, hh), 0.002);
                VU_ADD(color, VU_ACTIVE, fgTri);
                float handleDist = vuSphere(vuTranslate(uv, float2(invHF * fullW + marginX, size.y * 0.5)), VU_HANDLE);
                VU_ADD(color, VU_ACTIVE, handleDist);
                float gripDist = abs(uv.x - invHF * hw * 2 - marginX) - VU_OUTLINE;
                VU_ADD(color, VU_ACTIVE, gripDist);
                return color;
            }

            float3 vuDrawExpFalloffArea(float2 uv, float2 size, float expFalloff)
            {
                float3 color = VU_FG;
                float2 triUV = -(uv - float2(size.x / 2, size.y / 2));
                float hw = 0.45 * size.x; float hh = 0.37 * size.y;
                float fullW = hw * 2; float fullH = hh * 2;
                float bgTri = vuInflate(vuTriRight(triUV, hw, hh), 0.002);
                VU_ADD(color, VU_INACTIVE, bgTri);
                float marginX = (size.x - fullW) / 2; float marginY = (size.y - fullH) / 2;
                float tx = vu_remap(uv.x, marginX, size.x - marginX, 0, 1);
                float ty = vu_remap(uv.y, marginY, size.y - marginY, 0, 1);
                float efY = (1.0 + (pow(tx, 4.0) * expFalloff) - expFalloff) * tx;
                float fgD = vuInflate((1.0 - ty) - efY, 0.02);
                VU_ADD(color, VU_ACTIVE, max(bgTri, fgD * 0.1));
                float handleDist = vuSphere(vuTranslate(uv, float2(expFalloff * fullW + marginX, size.y * 0.5)), VU_HANDLE);
                VU_ADD(color, VU_ACTIVE, handleDist);
                float gripDist = abs(uv.x - expFalloff * hw * 2 - marginX) - VU_OUTLINE;
                VU_ADD(color, VU_ACTIVE, gripDist);
                return color;
            }

            float3 vuDrawFourBandArea(float2 uv, float2 size)
            {
                float2 sliceSize = float2(size.x, size.y / 4.0);
                float strength = vuGetBandAmplitudeLerp((uv.y / size.y) * 4.0, uv.x / size.x * 64.0);
                float3 sliceColor = vuGetBandColorLerp(uv.y / sliceSize.y);
                return saturate(lerp(sliceColor, sliceColor * 15, strength));
            }

            // Cheap hash used for the autocorrelator's electric fizzle sparks.
            float hudHash21(float2 p)
            {
                p = frac(p * float2(123.34, 345.45));
                p += dot(p, p + 34.345);
                return frac(p.x * p.y);
            }

            float3 vuDrawAutoCorr(float2 uv, float2 size)
            {
                float2 scaledUV = uv / size;
                float2 mirroredUV = abs(2 * (scaledUV - 0.5));
                float3 ac = AudioLinkLerp(ALPASS_AUTOCORRELATOR + float2(mirroredUV.x * AUDIOLINK_WIDTH, 0)).rrr;
                float scaledAC = abs(ac.r * 0.007);
                float middle = size.y * 0.5;
                float acDistAbs = abs(smoothstep(0.005, 0.003, abs(middle - uv.y) - scaledAC));
                float vuI = saturate(AudioLinkData(ALPASS_FILTEREDVU_INTENSITY).r * 2.5);
                float3 acColor = lerp(VU_FG, VU_ACTIVE, vuI);
                acColor = lerp(acColor, VU_FG, smoothstep(0, 1, mirroredUV.x));
                return lerp(VU_BG * 0.8, acColor, acDistAbs);
            }

            // Lay out the console in a normalized panel and feed every slider live audio.
            float3 vuDrawConsole(float2 uv, float4 amps, float vuLevel, float3 tint)
            {
                float3 color = VU_BG;

                // ===== the "manipulate its sliders to match the audio" part =====
                float threshold[4] = { amps.x, amps.y, amps.z, amps.w };       // box heights pulse per band
                float crossover[4] = { 0.0, 0.25, 0.5, 0.75 };                 // stable layout
                float gain = saturate(vuLevel) * 2.0;                          // gain handle tracks the VU level
                float hitFade = saturate(amps.x * 0.8 + 0.1);                  // bass drives hit-fade
                float expFalloff = saturate(amps.w * 0.8 + 0.1);               // treble drives exp-falloff

                float currentY = 0;
                float2 topO = vuTranslate(uv, VU_MARGIN);
                float2 topS = float2(1.0 - VU_MARGIN * 2, 0.35);
                VU_ADD(color, vuDrawTopArea(topO, threshold, crossover, gain), vuRoundedBoxTL(topO, topS, VU_CORNER));
                currentY += topS.y + VU_MARGIN;

                const float gainH = 0.13;
                const float fadeH = 0.19;

                float2 gO = vuTranslate(uv, VU_MARGIN + float2(0, currentY));
                float2 gS = float2(topS.x, gainH);
                VU_ADD(color, vuDrawGainArea(gO, gS, gain), vuRoundedBoxTL(gO, gS, VU_CORNER));
                currentY += gS.y + VU_MARGIN;

                float2 hfO = vuTranslate(uv, VU_MARGIN + float2(0, currentY));
                float2 hfS = float2(topS.x * 0.5 - VU_MARGIN * 0.5, fadeH);
                VU_ADD(color, vuDrawHitFadeArea(hfO, hfS, hitFade), vuRoundedBoxTL(hfO, hfS, VU_CORNER));

                float2 efO = vuTranslate(uv, VU_MARGIN + float2(hfS.x + VU_MARGIN, currentY));
                float2 efS = float2(topS.x * 0.5 - VU_MARGIN * 0.5, fadeH);
                VU_ADD(color, vuDrawExpFalloffArea(efO, efS, expFalloff), vuRoundedBoxTL(efO, efS, VU_CORNER));
                currentY += fadeH + VU_MARGIN;

                float2 fbO = vuTranslate(uv, VU_MARGIN + float2(0, currentY));
                float2 fbS = float2(topS.x, fadeH);
                VU_ADD(color, vuDrawFourBandArea(fbO, fbS), vuRoundedBoxTL(fbO, fbS, VU_CORNER));
                currentY += fbS.y + VU_MARGIN;

                float2 acO = vuTranslate(uv, VU_MARGIN + float2(0, currentY));
                float2 acS = float2(topS.x, gainH);
                VU_ADD(color, vuDrawAutoCorr(acO, acS), vuRoundedBoxTL(acO, acS, VU_CORNER));

                // Gentle ColorChord/Theme tint so the console takes on the music's color.
                color = lerp(color, color * (tint * 1.5 + 0.001), 0.25);
                return color;
            }

            fixed4 hudFrag(hudG2F i) : SV_Target
            {
                float2 hudUV = i.uv;

                // Razor-edged mask: a hard 0.5 cutoff with a 1px antialiased rim, so the HUD lands exactly
                // on the white of the emission mask. Black is fully transparent (discarded) with no soft
                // bleed past the edge; white shows at full strength. fwidth keeps the edge ~1px regardless
                // of how blurry the mask texture's ramp is, collapsing it to the 0.5 contour.
                float maskRaw = tex2D(_CyberMask, hudUV).r;
                float maskEdge = max(fwidth(maskRaw), 1e-5);
                float cyberMask = smoothstep(0.5 - maskEdge, 0.5 + maskEdge, maskRaw);
                if (cyberMask <= 0.0) discard;

                float4 amps; float raw_waveform;
                HudFetchAL(hudUV, amps, raw_waveform);

                float3 hud = 0;

                // VU Meter
                if (_UseCyberVU > 0.5)
                {
                    float2 vuUV = HudTransform(hudUV, _Cyber_VU_Transform);
                    float2 vuPlace = HudPlace(hudUV, _Cyber_VU_Transform);
                    float vuInBounds = step(0.0, vuPlace.x) * step(vuPlace.x, 1.0)
                                     * step(0.0, vuPlace.y) * step(vuPlace.y, 1.0);

                    if (_Cyber_VU_Style < 0.5)
                    {
                        // Living AudioLink console, lifted from SDR into HDR (see consoleCol below). Listens to
                        // all bands: overall level drives the gain handle, the all-band blend tints it.
                        float3 al_color = HudAllBandColor(amps);
                        float vu = max(max(amps.x, amps.y), max(amps.z, amps.w));
                        float2 cUV = float2(vuUV.x, vuUV.y * 1.14);
                        // The console palette is SDR and dominated by dark chrome (VU_BG); on an additive HUD that
                        // floor reads as a dim grey wash, which is why the meter looked extremely dim even at
                        // max intensity. Subtract it so only the lit content glows, then push it into HDR.
                        float3 consoleCol = max(0.0, vuDrawConsole(cUV, amps, vu, al_color) - VU_BG);
                        hud += consoleCol * _Cyber_VU_Str * vuInBounds * (HUD_GLOW * 1.4);
                    }
                    else
                    {
                        // Multi-band bar - one horizontal lane per band, filled to its own level and lit in
                        // its own ColorChord colour, so the bar displays every band across the HUD emission.
                        float lane = saturate(vuUV.y) * 4.0;
                        int bandIdx = clamp((int)lane, 0, 3);
                        float bandAmp = HUD_AL_BAND(amps, bandIdx);
                        float laneGap = step(0.12, frac(lane)) * step(frac(lane), 0.88);
                        float bar = step(vuUV.x, bandAmp) * laneGap * vuInBounds;
                        hud += bar * _Cyber_VU_Str * HudBandColor(bandIdx) * HUD_GLOW;
                    }
                }

                // Spectrum (CC) bars
                if (_UseCyberCC > 0.5)
                {
                    float3 al_color = HudBandColor((int)_Cyber_CC_Band);
                    float2 ccUV = HudTransform(hudUV, _Cyber_CC_Transform);
                    float2 ccPlace = HudPlace(hudUV, _Cyber_CC_Transform);
                    float density = max(2.0, _Cyber_CC_Density);
                    if (ccPlace.x >= 0.0 && ccPlace.x <= 1.0 && ccPlace.y >= 0.0 && ccPlace.y <= 1.0)
                    {
                        int barIdx = (int)floor(ccUV.x * density);
                        int sampleX = (int)((float)barIdx / density * 127.0);

                        float magnitude = 0;
                        if (_UseAudioLink > 0.5 && AudioLinkIsAvailable())
                            magnitude = AudioLinkData(ALPASS_AUDIOLINK + int2(sampleX, (int)_Cyber_CC_Band)).r;

                        float barShape = step(1.0 - ccUV.y, saturate(magnitude * 4.0));
                        float barCenter = (floor(ccUV.x * density) + 0.5) / density;
                        float inBar = step(abs(ccUV.x - barCenter), 0.45 / density);
                        hud += barShape * inBar * _Cyber_CC_Str * al_color * HUD_GLOW;
                    }
                }

                // Waveform
                if (_UseCyberWave > 0.5)
                {
                    float3 al_color = HudBandColor((int)_Cyber_Wave_Band);
                    float waveBand = HUD_AL_BAND(amps, _Cyber_Wave_Band);
                    float2 waveUV = HudTransform(hudUV, _Cyber_Wave_Transform);
                    float2 wavePlace = HudPlace(hudUV, _Cyber_Wave_Transform);
                    float waveInBounds = step(0.0, wavePlace.x) * step(wavePlace.x, 1.0)
                                       * step(0.0, wavePlace.y) * step(wavePlace.y, 1.0);
                    // The waveform feed is full-spectrum PCM, so the selected band breathes its amplitude
                    // (and tints it) to give this layer a distinct band source.
                    float wave = abs((waveUV.y - 0.5) - raw_waveform * lerp(0.1, 0.3, waveBand));
                    wave = (1.0 - smoothstep(0.0, 0.02, wave)) * waveInBounds;
                    hud += wave * _Cyber_Wave_Str * al_color * HUD_GLOW;
                }

                // DMX grid mini-readout
                if (_UseCyberDMX > 0.5)
                {
                    float dmxBand = HUD_AL_BAND(amps, _Cyber_DMX_Band);
                    float2 dmxUV = HudTransform(hudUV, _Cyber_DMX_Transform);
                    float2 dmxPlace = HudPlace(hudUV, _Cyber_DMX_Transform);
                    if (dmxPlace.x >= 0.0 && dmxPlace.x <= 1.0 && dmxPlace.y >= 0.0 && dmxPlace.y <= 1.0)
                    {
                        float3 dmxSample = tex2D(_Udon_DMXGridRenderTexture, dmxUV).rgb;
                        // The DMX feed is VRSL data, not audio, so the selected band pulses the readout
                        // brightness (floored so the grid stays legible) to give it a band source.
                        hud += dmxSample * lerp(0.4, 1.0, dmxBand) * _Cyber_DMX_Str * HUD_GLOW;
                    }
                }

                // Autocorrelator scope ring - a polar-wrapped mirror of the in-world panel oscilloscope
                // trace (drawAutoCorrelatorArea / vuDrawAutoCorr): the autocorrelation swells a soft scope
                // line out from a baseline circle and the brightness tracks FilteredVU intensity.
                if (_UseCyberAuto > 0.5)
                {
                    float3 al_color = HudBandColor((int)_Cyber_Auto_Band);
                    float autoBand = HUD_AL_BAND(amps, _Cyber_Auto_Band);
                    float2 acUV = HudTransform(hudUV, _Cyber_Auto_Transform);
                    float2 centered = acUV - 0.5;
                    float r = length(centered) * 2.0;
                    
                    if (r <= 1.0)
                    {
                        float angle = atan2(centered.y, centered.x);
                        float acPos = abs(angle / UNITY_PI); // Maps radial angle to linear 0-1
                        
                        float acVal = 0;
                        float vuI = 0;
                        bool alLive = (_UseAudioLink > 0.5 && AudioLinkIsAvailable());
                        if (alLive)
                        {
                            // Identical fetch + 0.007 deflection scale to the panel trace; abs() so the
                            // band swells symmetrically. FilteredVU drives brightness like the panel.
                            acVal = abs(AudioLinkLerp(ALPASS_AUTOCORRELATOR + float2(acPos * AUDIOLINK_WIDTH, 0)).r * 0.007);
                            vuI   = saturate(AudioLinkData(ALPASS_FILTEREDVU_INTENSITY).r * 2.5);
                        }

                        // Per-effect drivers: each effect listens to its OWN AudioLink band, so the user can route
                        // bass / low-mid / high-mid / treble to shimmer / pop / sizzle / electrify independently, and
                        // each is gated by its toggle. With no live AudioLink we fall back to an idle animated level so
                        // every enabled effect stays visible while authoring in the editor.
                        float shimmerAmp   = alLive ? HUD_AL_BAND(amps, _Cyber_Auto_Shimmer_Band)   : 0.6;
                        float popAmp       = alLive ? HUD_AL_BAND(amps, _Cyber_Auto_Pop_Band)       : pow(0.5 + 0.5 * sin(_Time.y * 3.0), 8.0);
                        float sizzleAmp    = alLive ? HUD_AL_BAND(amps, _Cyber_Auto_Sizzle_Band)    : 0.6;
                        float electrifyAmp = alLive ? HUD_AL_BAND(amps, _Cyber_Auto_Electrify_Band) : 0.6;
                        shimmerAmp   *= _Cyber_Auto_Shimmer;
                        popAmp       *= _Cyber_Auto_Pop;
                        sizzleAmp    *= _Cyber_Auto_Sizzle;
                        electrifyAmp *= _Cyber_Auto_Electrify;

                        // POP: sharp beat flash that swells the ring and goes white-hot, driven by its band.
                        float pop = pow(saturate(popAmp), 3.0);
                        acVal += pop * 0.05;

                        // SIZZLE: crackling noise jitters the swell radius so the trace spits, scaled by its band.
                        float crackle = hudHash21(float2(floor(acPos * 90.0), floor(_Time.y * 28.0))) - 0.5;
                        acVal += crackle * 0.014 * sizzleAmp;

                        // Soft filled band around the baseline radius - the ring equivalent of the panel
                        // trace that swells out from its centerline as the correlation grows.
                        const float baselineR = 0.6;
                        float bandDist = abs(r - baselineR) - acVal;
                        float trace = abs(smoothstep(0.02, 0.01, bandDist));

                        // SHIMMER: thin highlight bands chasing around the ring, intensity tied to its band.
                        float shimmer = pow(0.5 + 0.5 * sin(acPos * 36.0 - _Time.y * 6.0 + acVal * 400.0), 4.0) * shimmerAmp;

                        // ELECTRIFY: lightning arc filaments crossing the disc, brightening with its band.
                        float arcField = sin(acPos * 64.0 + _Time.y * 9.0) + sin(r * 26.0 - _Time.y * 7.0 + acPos * 12.0);
                        float electrify = pow(saturate(1.0 - abs(arcField) * 2.5), 3.0) * electrifyAmp;

                        // POP blooms a soft halo just off the trace.
                        float halo = smoothstep(0.06 + pop * 0.06, 0.0, abs(bandDist)) * pop;

                        // Base ring brightness; shimmer lifts it, pop punches it.
                        float bright = lerp(0.15, 1.0, max(vuI, autoBand));
                        bright *= 1.0 + shimmer * 1.6 + pop * 2.0;
                        float tailFade = 1.0 - smoothstep(0.0, 1.0, acPos);

                        // SIZZLE sparks: rare bright specks skittering along the trace edge, density on its band.
                        float spark = pow(hudHash21(float2(floor(acPos * 160.0), floor(_Time.y * 36.0))), 9.0);
                        float sizzle = spark * smoothstep(0.025, 0.0, abs(bandDist)) * (0.3 + sizzleAmp * 3.0);

                        float3 acRing = al_color * trace * bright + al_color * halo * 0.6;
                        acRing = lerp(acRing, 1.0, saturate(trace * pop));          // POP white-hot core
                        acRing += float3(0.5, 0.8, 1.0) * sizzle;                   // SIZZLE electric-blue sparks
                        acRing += float3(0.45, 0.8, 1.0) * electrify;              // ELECTRIFY arc filaments

                        hud += acRing * tailFade * _Cyber_AutoCorr_Str * HUD_GLOW;
                    }
                }

                float3 col = hud * cyberMask;
                if (max(col.r, max(col.g, col.b)) < 0.002) discard;
                return fixed4(col, 1);
            }
            ENDCG
        }

        // PASS 4: FRACTURE SHARDS (geometry-amplified solid chunks) - PC only. Each triangle in the fracturing region (manual _Vtx_Fracture_Amount + AudioLink jitter) detaches as a real tetrahedral shard that tumbles around its centroid and flies outward along its face normal to a hover distance, while the main pass clips that region of the body away so the suit appears to break apart. Surface shaders cannot host a geometry stage, so this is its own vert/geom/frag pass, gated by _UseVtxKinetic and per-shard progress so it emits nothing where the suit is still intact. Kept off the SPS variant because VRCFury's SPS patcher rewrites the vertex stage.
        Pass
        {
            Tags { "LightMode" = "ForwardBase" }
            ZWrite On
            ZTest LEqual
            Cull Off
            Blend Off

            CGPROGRAM
            #pragma vertex shardVert
            #pragma geometry shardGeom
            #pragma fragment shardFrag
            #pragma target 5.0
            #pragma require geometry
            #pragma multi_compile_instancing
            #pragma multi_compile_fwdbase
            #pragma shader_feature_local _ALPHATEST_ON
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "Packages/com.vixencreations.vixens-toolbox/Editor/Avatar Tools/Shaders/cginc/AudioLink.cginc"

            UNITY_DECLARE_TEX2D(_MainTex);
            sampler2D _MetallicGlossMap, _BumpMap, _EmissionMap, _RegionMask;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _EmissionColor;
            fixed4 _Region_R_Tint, _Region_G_Tint, _Region_B_Tint;
            half _CutOff;
            float _Emis_Exp, _Norm_Str;
            float _PBR_Met_Ch, _PBR_Met_Inv, _PBR_Smooth_Ch, _PBR_Smooth_Inv;
            float _UseRegionMask, _Region_R_Emis, _Region_G_Emis, _Region_B_Emis;
            float _UseVtxKinetic, _UseAudioLink, _UseMediaState;
            float _Vtx_Fracture_Band, _Vtx_Fracture_Str;
            float _Vtx_Fracture_Amount, _Vtx_Fracture_Dist, _Vtx_Fracture_Spin;
            float _Vtx_Fracture_Spiral, _Vtx_Fracture_Lift, _Vtx_Fracture_Float;
            float _Shard_ColorMod, _Shard_ColorMod_Speed, _UseShardCC, _Shard_CC_Str;
            uniform float _MediaPlaying;

            // Rotate vector v around unit axis by angle (Rodrigues).
            float3 shardRotate(float3 v, float3 axis, float angle)
            {
                float s = sin(angle), c = cos(angle);
                return v * c + cross(axis, v) * s + axis * dot(axis, v) * (1.0 - c);
            }

            // Packed-map channel picker (mirror of the surf-pass ChannelPick - this pass is its own program).
            inline float shardChannel(fixed4 packed, float ch)
            {
                return (ch < 0.5) ? packed.r : (ch < 1.5) ? packed.g : (ch < 2.5) ? packed.b : packed.a;
            }

            // Hue-rotate an RGB color by 'angle' radians in YIQ space (cheap, no HSV stack). Drives shard color-mod.
            float3 shardHueRotate(float3 col, float angle)
            {
                float c = cos(angle), s = sin(angle);
                float3x3 toYIQ = float3x3(0.299, 0.587, 0.114, 0.596, -0.274, -0.322, 0.211, -0.523, 0.312);
                float3x3 toRGB = float3x3(1.0, 0.956, 0.621, 1.0, -0.272, -0.647, 1.0, -1.106, 1.703);
                float3 yiq = mul(toYIQ, col);
                float2 iq = float2(yiq.y * c - yiq.z * s, yiq.y * s + yiq.z * c);
                return mul(toRGB, float3(yiq.x, iq.x, iq.y));
            }

            // Shared shard motion: returns object-space displacement (push) for a chunk and outputs its tumble axis/angle and velocity direction.
            // Keeps PASS 4 (solid shards) and PASS 5 (trails) in lockstep so a tail always trails its own shard.
            void shardMotion(float3 center, float3 faceN, float h, float shardProg,
                             out float3 push, out float3 axis, out float ang, out float3 velDir)
            {
                axis = normalize(float3(frac(h * 1.0) * 2.0 - 1.0, frac(h * 1.37) * 2.0 - 1.0, frac(h * 3.11) * 2.0 - 1.0) + 1e-4);
                ang = shardProg * _Vtx_Fracture_Spin * 6.2831853 + _Time.y * 0.6 * _Vtx_Fracture_Spin * (h - 0.5);

                // Outward fly-out, eased (sqrt pops fast then holds = hover), with a subtle bob.
                float travel = sqrt(shardProg) * _Vtx_Fracture_Dist + sin(_Time.y * 1.3 + h * 6.2831) * 0.01 * shardProg;

                // Spiral: orbit the fly-out direction around object-up and add a helical rise.
                const float3 up = float3(0.0, 1.0, 0.0);
                float spiralAng = (_Time.y * 1.2 + shardProg * 6.2831853) * _Vtx_Fracture_Spiral;
                float3 pushDir = shardRotate(faceN, up, spiralAng);
                push = pushDir * travel;
                push += up * _Vtx_Fracture_Spiral * shardProg * _Vtx_Fracture_Dist * 0.5;

                // Float: per-shard buoyant low-frequency drift on all axes.
                push += float3(sin(_Time.y * 0.8 + h * 6.2831),
                               sin(_Time.y * 0.6 + h * 12.566 + 1.3),
                               cos(_Time.y * 0.7 + h * 9.42 + 2.1)) * (_Vtx_Fracture_Float * 0.08 * shardProg);

                // Lift: net vertical offset (animatable up/down).
                push += up * (_Vtx_Fracture_Lift * shardProg);

                velDir = (length(push) > 1e-4) ? normalize(push) : pushDir;
            }

            struct shardAppdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct shardV2G
            {
                float3 opos : TEXCOORD0;
                float3 onormal : TEXCOORD1;
                float2 uv : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct shardG2F
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                float3 worldTangent : TEXCOORD3;
                nointerpolation float2 shardData : TEXCOORD4; // x = per-shard hash, y = detach progress
                UNITY_VERTEX_OUTPUT_STEREO
            };

            shardV2G shardVert(shardAppdata v)
            {
                shardV2G o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.opos = v.vertex.xyz;
                o.onormal = v.normal;
                o.uv = v.uv;
                return o;
            }

            [maxvertexcount(12)]
            void shardGeom(triangle shardV2G p[3], inout TriangleStream<shardG2F> stream)
            {
                if (_UseVtxKinetic < 0.5) return;

                UNITY_SETUP_INSTANCE_ID(p[0]);

                float3 center = (p[0].opos + p[1].opos + p[2].opos) / 3.0;
                float3 faceN  = normalize(cross(p[1].opos - p[0].opos, p[2].opos - p[0].opos));
                float2 uvC    = (p[0].uv + p[1].uv + p[2].uv) / 3.0;

                // Per-shard hash from the grid-snapped centroid (stable per chunk).
                float h = frac(sin(dot(floor(center * 23.0), float3(12.9898, 78.233, 37.719))) * 43758.5453);

                // AudioLink jitter layered on the manual amount.
                float jitter = 0;
                if (_UseAudioLink > 0.5 && !(_UseMediaState > 0.5 && _MediaPlaying < 0.5) && AudioLinkIsAvailable())
                {
                    int band = (int)_Vtx_Fracture_Band;
                    float amp = AudioLinkData(ALPASS_AUDIOLINK + int2(0, clamp(band, 0, 3))).r;
                    amp = saturate(pow(amp * 4.0, 0.35));
                    jitter = amp * _Vtx_Fracture_Str * 0.2;
                }
                float progress = saturate(_Vtx_Fracture_Amount + jitter);

                // Stagger onset per shard; emit nothing until this shard detaches (the body still covers it).
                float onset = h * 0.35;
                float shardProg = saturate((progress - onset) / max(1.0 - onset, 1e-3));
                if (shardProg <= 0.001) return;

                // Tumble + fly-out + spiral/float/lift (shared with the trail pass so a tail always follows its shard).
                float3 push, axis, velDir; float ang;
                shardMotion(center, faceN, h, shardProg, push, axis, ang, velDir);

                // Rotated/translated base verts (object space).
                float3 v0 = center + shardRotate(p[0].opos - center, axis, ang) + push;
                float3 v1 = center + shardRotate(p[1].opos - center, axis, ang) + push;
                float3 v2 = center + shardRotate(p[2].opos - center, axis, ang) + push;

                // Tetra apex for thickness (along the rotated face normal).
                float3 rotN = shardRotate(faceN, axis, ang);
                float avgEdge = (length(p[1].opos - p[0].opos) + length(p[2].opos - p[0].opos) + length(p[2].opos - p[1].opos)) / 3.0;
                float3 apex = center + push + rotN * avgEdge * 0.5 * (0.4 + 0.6 * shardProg);

                // Tangent basis from the base-tri UV gradient (rotated with the shard), reused for all faces - good enough for small tumbling chunks.
                float3 te1 = p[1].opos - p[0].opos;
                float3 te2 = p[2].opos - p[0].opos;
                float2 tduv1 = p[1].uv - p[0].uv;
                float2 tduv2 = p[2].uv - p[0].uv;
                float tDenom = tduv1.x * tduv2.y - tduv2.x * tduv1.y;
                float tR = (abs(tDenom) < 1e-8) ? 0.0 : 1.0 / tDenom;
                float3 tangentO = (te1 * tduv2.y - te2 * tduv1.y) * tR;
                tangentO = (dot(tangentO, tangentO) > 1e-10) ? normalize(tangentO) : normalize(te1);
                float3 wTan = normalize(UnityObjectToWorldDir(shardRotate(tangentO, axis, ang)));

                // World-space verts.
                float3 wv0 = mul(unity_ObjectToWorld, float4(v0, 1.0)).xyz;
                float3 wv1 = mul(unity_ObjectToWorld, float4(v1, 1.0)).xyz;
                float3 wv2 = mul(unity_ObjectToWorld, float4(v2, 1.0)).xyz;
                float3 wap = mul(unity_ObjectToWorld, float4(apex, 1.0)).xyz;

                shardG2F o;
                UNITY_INITIALIZE_OUTPUT(shardG2F, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.worldTangent = wTan;
                o.shardData = float2(h, shardProg);

                // Base
                o.worldNormal = normalize(cross(wv1 - wv0, wv2 - wv0));
                o.worldPos = wv0; o.uv = p[0].uv; o.pos = UnityWorldToClipPos(wv0); stream.Append(o);
                o.worldPos = wv1; o.uv = p[1].uv; o.pos = UnityWorldToClipPos(wv1); stream.Append(o);
                o.worldPos = wv2; o.uv = p[2].uv; o.pos = UnityWorldToClipPos(wv2); stream.Append(o);
                stream.RestartStrip();

                // Side 1
                o.worldNormal = normalize(cross(wv1 - wv0, wap - wv0));
                o.worldPos = wv0; o.uv = p[0].uv; o.pos = UnityWorldToClipPos(wv0); stream.Append(o);
                o.worldPos = wv1; o.uv = p[1].uv; o.pos = UnityWorldToClipPos(wv1); stream.Append(o);
                o.worldPos = wap; o.uv = uvC;     o.pos = UnityWorldToClipPos(wap); stream.Append(o);
                stream.RestartStrip();

                // Side 2
                o.worldNormal = normalize(cross(wv2 - wv1, wap - wv1));
                o.worldPos = wv1; o.uv = p[1].uv; o.pos = UnityWorldToClipPos(wv1); stream.Append(o);
                o.worldPos = wv2; o.uv = p[2].uv; o.pos = UnityWorldToClipPos(wv2); stream.Append(o);
                o.worldPos = wap; o.uv = uvC;     o.pos = UnityWorldToClipPos(wap); stream.Append(o);
                stream.RestartStrip();

                // Side 3
                o.worldNormal = normalize(cross(wv0 - wv2, wap - wv2));
                o.worldPos = wv2; o.uv = p[2].uv; o.pos = UnityWorldToClipPos(wv2); stream.Append(o);
                o.worldPos = wv0; o.uv = p[0].uv; o.pos = UnityWorldToClipPos(wv0); stream.Append(o);
                o.worldPos = wap; o.uv = uvC;     o.pos = UnityWorldToClipPos(wap); stream.Append(o);
                stream.RestartStrip();
            }

            fixed4 shardFrag(shardG2F i) : SV_Target
            {
                float2 uv = i.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                fixed4 c = UNITY_SAMPLE_TEX2D(_MainTex, uv) * _Color;
                #if defined(_ALPHATEST_ON)
                    clip(c.a - _CutOff);
                #endif

                float h = i.shardData.x;
                float shardProg = i.shardData.y;
                float3 albedo = c.rgb;

                // Region tints + region emission boost (mirror of the body surface).
                float regionEmis = 0.0;
                if (_UseRegionMask > 0.5)
                {
                    fixed4 rm = tex2D(_RegionMask, uv);
                    albedo = lerp(albedo, albedo * _Region_R_Tint.rgb, rm.r);
                    albedo = lerp(albedo, albedo * _Region_G_Tint.rgb, rm.g);
                    albedo = lerp(albedo, albedo * _Region_B_Tint.rgb, rm.b);
                    regionEmis = rm.r * _Region_R_Emis + rm.g * _Region_G_Emis + rm.b * _Region_B_Emis;
                }

                // Metallic / smoothness from the packed PBR map (Poiyomi-style channel pick + invert).
                fixed4 mg = tex2D(_MetallicGlossMap, uv);
                float metallic = shardChannel(mg, _PBR_Met_Ch);
                if (_PBR_Met_Inv > 0.5) metallic = 1.0 - metallic;
                float smoothness = shardChannel(mg, _PBR_Smooth_Ch);
                if (_PBR_Smooth_Inv > 0.5) smoothness = 1.0 - smoothness;

                // Two-sided geometric normal (flip toward camera under Cull Off), then apply the tangent-space normal map.
                float3 N = normalize(i.worldNormal);
                float3 V = normalize(_WorldSpaceCameraPos - i.worldPos);
                if (dot(N, V) < 0.0) N = -N;
                float3 T = normalize(i.worldTangent - N * dot(N, i.worldTangent));
                float3 B = normalize(cross(N, T));
                float3 nTS = UnpackNormal(tex2D(_BumpMap, uv));
                nTS.xy *= _Norm_Str;
                float3 Nm = normalize(T * nTS.x + B * nTS.y + N * nTS.z);

                // Emission (map * color + region boost).
                float3 emis = tex2D(_EmissionMap, uv).rgb * _EmissionColor.rgb * _Emis_Exp;
                emis += albedo * regionEmis;

                // Color-mod: per-shard hue cycle (speed 0 = static per-shard offset = shattered rainbow).
                if (_Shard_ColorMod > 0.001)
                {
                    float hueAng = (_Time.y * _Shard_ColorMod_Speed + h) * 6.2831853;
                    albedo = lerp(albedo, shardHueRotate(albedo, hueAng), _Shard_ColorMod);
                    emis   = lerp(emis,   shardHueRotate(emis,   hueAng), _Shard_ColorMod);
                }

                // AudioLink ColorChord: each shard takes a different live note color from the CC strip.
                if (_UseShardCC > 0.5 && _UseAudioLink > 0.5 && !(_UseMediaState > 0.5 && _MediaPlaying < 0.5) && AudioLinkIsAvailable())
                {
                    float ccPos = frac(h + _Time.y * 0.05);
                    float3 ccCol = AudioLinkData(ALPASS_CCSTRIP + int2((int)(saturate(ccPos) * 127.0), 0)).rgb;
                    albedo = lerp(albedo, ccCol, _Shard_CC_Str * 0.8);
                    emis  += ccCol * _Shard_CC_Str * shardProg;
                }

                // Compact metallic-workflow BRDF + SH9 ambient - keeps shards consistent with the body without the full surface stack.
                float3 Ldir = normalize(_WorldSpaceLightPos0.xyz);
                float ndl = saturate(dot(Nm, Ldir));
                float3 Hh = normalize(Ldir + V);
                float ndh = saturate(dot(Nm, Hh));
                float specPow = exp2(smoothness * 10.0 + 1.0);
                float3 F0 = lerp(0.04, albedo, metallic);
                float3 spec = F0 * pow(ndh, specPow) * (specPow + 8.0) / 25.13274;
                float3 diffuse = albedo * (1.0 - metallic);
                float3 amb = ShadeSH9(float4(Nm, 1.0));
                float3 lit = diffuse * (_LightColor0.rgb * ndl + amb) + spec * _LightColor0.rgb * ndl;
                lit += emis;
                return fixed4(lit, 1.0);
            }
            ENDCG
        }

        // PASS 5: FRACTURE SHARD TRAILS (additive comet tails) - PC only. Optional per-shard streak trailing each flying chunk along its velocity, gated by _Vtx_Fracture_Trail (0 = off, emits nothing). Re-derives the exact PASS 4 motion via shardMotion so a tail always follows its own shard, and inherits the shard's hue-mod / ColorChord color. Separate additive pass so tails glow without disturbing the solid shards. Kept off the SPS variant for the same reason as the shard pass.
        Pass
        {
            Tags { "LightMode" = "ForwardBase" }
            Blend One One
            ZWrite Off
            ZTest LEqual
            Cull Off

            CGPROGRAM
            #pragma vertex trailVert
            #pragma geometry trailGeom
            #pragma fragment trailFrag
            #pragma target 5.0
            #pragma require geometry
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #include "Packages/com.vixencreations.vixens-toolbox/Editor/Avatar Tools/Shaders/cginc/AudioLink.cginc"

            sampler2D _MainTex;     // plain sampler here (own program) so the geometry stage can use tex2Dlod - no derivatives in a geom shader.
            float4 _MainTex_ST;
            fixed4 _Color;
            float _UseVtxKinetic, _UseAudioLink, _UseMediaState;
            float _Vtx_Fracture_Band, _Vtx_Fracture_Str;
            float _Vtx_Fracture_Amount, _Vtx_Fracture_Dist, _Vtx_Fracture_Spin;
            float _Vtx_Fracture_Spiral, _Vtx_Fracture_Lift, _Vtx_Fracture_Float, _Vtx_Fracture_Trail;
            float _Shard_ColorMod, _Shard_ColorMod_Speed, _UseShardCC, _Shard_CC_Str;
            uniform float _MediaPlaying;

            // Duplicated from the shard pass - separate CGPROGRAMs cannot share functions; kept byte-for-byte identical so trails track shards exactly.
            float3 shardRotate(float3 v, float3 axis, float angle)
            {
                float s = sin(angle), c = cos(angle);
                return v * c + cross(axis, v) * s + axis * dot(axis, v) * (1.0 - c);
            }

            float3 shardHueRotate(float3 col, float angle)
            {
                float c = cos(angle), s = sin(angle);
                float3x3 toYIQ = float3x3(0.299, 0.587, 0.114, 0.596, -0.274, -0.322, 0.211, -0.523, 0.312);
                float3x3 toRGB = float3x3(1.0, 0.956, 0.621, 1.0, -0.272, -0.647, 1.0, -1.106, 1.703);
                float3 yiq = mul(toYIQ, col);
                float2 iq = float2(yiq.y * c - yiq.z * s, yiq.y * s + yiq.z * c);
                return mul(toRGB, float3(yiq.x, iq.x, iq.y));
            }

            void shardMotion(float3 center, float3 faceN, float h, float shardProg,
                             out float3 push, out float3 axis, out float ang, out float3 velDir)
            {
                axis = normalize(float3(frac(h * 1.0) * 2.0 - 1.0, frac(h * 1.37) * 2.0 - 1.0, frac(h * 3.11) * 2.0 - 1.0) + 1e-4);
                ang = shardProg * _Vtx_Fracture_Spin * 6.2831853 + _Time.y * 0.6 * _Vtx_Fracture_Spin * (h - 0.5);
                float travel = sqrt(shardProg) * _Vtx_Fracture_Dist + sin(_Time.y * 1.3 + h * 6.2831) * 0.01 * shardProg;
                const float3 up = float3(0.0, 1.0, 0.0);
                float spiralAng = (_Time.y * 1.2 + shardProg * 6.2831853) * _Vtx_Fracture_Spiral;
                float3 pushDir = shardRotate(faceN, up, spiralAng);
                push = pushDir * travel;
                push += up * _Vtx_Fracture_Spiral * shardProg * _Vtx_Fracture_Dist * 0.5;
                push += float3(sin(_Time.y * 0.8 + h * 6.2831),
                               sin(_Time.y * 0.6 + h * 12.566 + 1.3),
                               cos(_Time.y * 0.7 + h * 9.42 + 2.1)) * (_Vtx_Fracture_Float * 0.08 * shardProg);
                push += up * (_Vtx_Fracture_Lift * shardProg);
                velDir = (length(push) > 1e-4) ? normalize(push) : pushDir;
            }

            struct trailAppdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct trailV2G
            {
                float3 opos : TEXCOORD0;
                float3 onormal : TEXCOORD1;
                float2 uv : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct trailG2F
            {
                float4 pos : SV_POSITION;
                float3 col : TEXCOORD0;
                float2 luv : TEXCOORD1;   // x = cross (-1..1), y = lengthwise (1 head -> 0 tail)
                UNITY_VERTEX_OUTPUT_STEREO
            };

            trailV2G trailVert(trailAppdata v)
            {
                trailV2G o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.opos = v.vertex.xyz;
                o.onormal = v.normal;
                o.uv = v.uv;
                return o;
            }

            [maxvertexcount(4)]
            void trailGeom(triangle trailV2G p[3], inout TriangleStream<trailG2F> stream)
            {
                if (_UseVtxKinetic < 0.5 || _Vtx_Fracture_Trail < 0.001) return;
                UNITY_SETUP_INSTANCE_ID(p[0]);

                float3 center = (p[0].opos + p[1].opos + p[2].opos) / 3.0;
                float3 faceN  = normalize(cross(p[1].opos - p[0].opos, p[2].opos - p[0].opos));
                float2 uvC    = (p[0].uv + p[1].uv + p[2].uv) / 3.0;

                float h = frac(sin(dot(floor(center * 23.0), float3(12.9898, 78.233, 37.719))) * 43758.5453);

                float jitter = 0;
                if (_UseAudioLink > 0.5 && !(_UseMediaState > 0.5 && _MediaPlaying < 0.5) && AudioLinkIsAvailable())
                {
                    int band = (int)_Vtx_Fracture_Band;
                    float amp = AudioLinkData(ALPASS_AUDIOLINK + int2(0, clamp(band, 0, 3))).r;
                    amp = saturate(pow(amp * 4.0, 0.35));
                    jitter = amp * _Vtx_Fracture_Str * 0.2;
                }
                float progress = saturate(_Vtx_Fracture_Amount + jitter);
                float onset = h * 0.35;
                float shardProg = saturate((progress - onset) / max(1.0 - onset, 1e-3));
                if (shardProg <= 0.02) return;

                float3 push, axis, velDir; float ang;
                shardMotion(center, faceN, h, shardProg, push, axis, ang, velDir);

                float3 headW = mul(unity_ObjectToWorld, float4(center + push, 1.0)).xyz;
                float3 wVel  = normalize(UnityObjectToWorldDir(velDir) + 1e-5);

                float len = _Vtx_Fracture_Trail * _Vtx_Fracture_Dist * (0.4 + 0.6 * shardProg) * 1.5;
                if (len < 1e-4) return;
                float3 tailW = headW - wVel * len;

                float3 viewDir = normalize(_WorldSpaceCameraPos - headW);
                float3 side = normalize(cross(wVel, viewDir) + 1e-5);
                float halfW = _Vtx_Fracture_Dist * 0.04 * (0.5 + 0.5 * shardProg);

                float3 col = (tex2Dlod(_MainTex, float4(uvC * _MainTex_ST.xy + _MainTex_ST.zw, 0, 0)) * _Color).rgb;
                if (_Shard_ColorMod > 0.001)
                {
                    float hueAng = (_Time.y * _Shard_ColorMod_Speed + h) * 6.2831853;
                    col = lerp(col, shardHueRotate(col, hueAng), _Shard_ColorMod);
                }
                if (_UseShardCC > 0.5 && _UseAudioLink > 0.5 && !(_UseMediaState > 0.5 && _MediaPlaying < 0.5) && AudioLinkIsAvailable())
                {
                    float ccPos = frac(h + _Time.y * 0.05);
                    float3 ccCol = AudioLinkData(ALPASS_CCSTRIP + int2((int)(saturate(ccPos) * 127.0), 0)).rgb;
                    col = lerp(col, ccCol, _Shard_CC_Str);
                }

                trailG2F o;
                UNITY_INITIALIZE_OUTPUT(trailG2F, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.col = col;

                float3 hL = headW - side * halfW;
                float3 hR = headW + side * halfW;
                float3 tL = tailW - side * halfW * 0.15;
                float3 tR = tailW + side * halfW * 0.15;

                o.pos = UnityWorldToClipPos(hL); o.luv = float2(-1, 1); stream.Append(o);
                o.pos = UnityWorldToClipPos(hR); o.luv = float2( 1, 1); stream.Append(o);
                o.pos = UnityWorldToClipPos(tL); o.luv = float2(-1, 0); stream.Append(o);
                o.pos = UnityWorldToClipPos(tR); o.luv = float2( 1, 0); stream.Append(o);
            }

            fixed4 trailFrag(trailG2F i) : SV_Target
            {
                float along = i.luv.y;
                float edge = 1.0 - smoothstep(0.5, 1.0, abs(i.luv.x));
                float fade = along * along * edge;
                if (fade < 0.003) discard;
                return fixed4(i.col * fade * 1.5, 1.0);
            }
            ENDCG
        }
    }

    CustomEditor "VixenWearEditor"
    FallBack "Standard"
}