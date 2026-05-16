Shader "VixenWear/Latex Ultra"
{
    Properties
    {
        [Header(Base Settings)]
        _Color ("Color", Color) = (1,1,1,1)
        _CutOff ("CutOff", Range(0,1)) = 0.5
        _MainTex ("Albedo (RGB) Cutout (A)", 2D) = "white" {}
        _MinBrightness ("Minimum Brightness", Range(0, 1)) = 0.0

        _UV_Rot ("UV Rotation", Float) = 0
        _SpeedX ("UV Speed X", Float) = 0
        _SpeedY ("UV Speed Y", Float) = 0
        _MatCap_Rot ("MatCap Rotation", Float) = 0

        [Header(Surface Maps)]
        _MetallicGlossMap ("Packed PBR (R:Met G:AO B:Disp A:Smooth)", 2D) = "white" {}
        [Normal] _BumpMap ("Normal Map", 2D) = "bump" {}

        _AO_Str ("AO Strength", Range(0,1)) = 1.0
        _Spec_Occ ("Specular Occlusion", Range(0,1)) = 1.0
        _Shad_Hard ("Shadow Hardness", Range(0,1)) = 1.0
        _Norm_Str ("Normal Strength", Range(0,5)) = 1.0

        _Parallax ("Parallax Depth", Range(0,0.1)) = 0.0
        _Disp_Str ("Displacement Strength", Range(0,1)) = 0.0
        _Tess_Edge ("Tessellation Edge Length", Range(1,50)) = 10.0
        _Emis_Exp ("Emission Exposure", Float) = 1.0

        [Header(Clearcoat Polish)]
        _CC_Strength ("Clearcoat Strength", Range(0,1)) = 1.0
        _CC_Smoothness ("Clearcoat Smoothness", Range(0,1)) = 0.9
        _CC_Spec_AA ("Specular Anti-Aliasing", Range(0,1)) = 0.0
        _CC_Flat ("Clearcoat Flattening", Range(0,1)) = 0.0

        _Film_Str ("Thin Film Strength", Range(0,1)) = 0.0
        _Film_Thick ("Thin Film Thickness", Float) = 0.0
        _Rim_Str ("Rim Light Strength", Range(0,5)) = 0.0
        _Rim_Power ("Rim Light Power", Range(0.1,10)) = 4.0

        [Header(Deferred Translucency)]
        _SSS_Str ("Subsurface Strength", Range(0,1)) = 0.0
        _SSS_Dist ("Subsurface Distance", Range(0,1)) = 0.1
        _SSS_Power ("Subsurface Power", Range(0.1,10)) = 4.0

        [Header(Micro Details)]
        [Toggle(_DETAIL_NORMAL)] _UseDetailNormal ("Enable Micro Detail", Float) = 0
        [Normal] _DetailNormalMap ("Micro Detail Map", 2D) = "bump" {}
        
        _Det_Strength ("Detail Strength", Range(0,2)) = 0.0
        _Det_UV_Tiling ("Detail UV Tiling", Float) = 1.0

        [Header(Lighting Integration)]
        [HDR] _EmissionColor ("Emission Color", Color) = (0,0,0,1)
        _EmissionMap ("Emission Map", 2D) = "black" {}

        _MatCap ("MatCap Texture", 2D) = "black" {}
        _MatCapMask ("MatCap Mask", 2D) = "white" {}

        _MatCap_Int ("MatCap Intensity", Range(0,2)) = 0.0
        _MatCap_Lit ("MatCap Lighting Mix", Range(0,1)) = 1.0
        _LV_Int ("Light Volumes Intensity", Range(0,1)) = 1.0
        _LTCGI_Int ("LTCGI Intensity", Range(0,1)) = 1.0

        [Header(VRSL Stage Hijack Protocol)]
        [Toggle(VRSL_ENABLE)] _UseVRSL ("Enable VRSL DMX Link", Float) = 0
        _DMX_Channel ("DMX Base Channel (Sector start)", Int) = 1
        _VRSL_Intensity ("VRSL Override Strength", Range(0, 1)) = 1.0
        _VRSL_Geo_Warp ("Pan/Tilt Geo-Warping", Range(0, 5)) = 1.0

        [Header(AudioLink Global Setup)]
        [Toggle(AL_ENABLE)] _UseAudioLink ("Enable AudioLink", Float) = 0
        [Enum(Manual,0,ColorChord,1,Theme 0,2,Theme 1,3,Theme 2,4,Theme 3,5)] _AL_ColorMode ("Color Source", Float) = 1
        [Toggle(AL_MEDIA_STATE)] _UseMediaState ("Power Down on Pause/Stop", Float) = 1
        
        [Header(God Tier Cybernetics (HUD Overlays))]
        [Toggle(CYBER_ENABLE)] _UseCyber ("Enable Cybernetic Overlays", Float) = 0
        _CyberMask ("Cyber Mask (B&W Window)", 2D) = "black" {}
        
        [Toggle] _UseCyberVU ("Enable VU Meter", Float) = 0
        [Enum(Bass,0,Low Mid,1,High Mid,2,Treble,3)] _Cyber_VU_Band ("VU Band", Float) = 0
        _Cyber_VU_Str ("VU Meter Intensity", Range(0,5)) = 1.0
        [VectorLabel(X Offset, Y Offset, Scale, Rotation)] _Cyber_VU_Transform ("VU Transform", Vector) = (0,0,1,0)

        [Toggle] _UseCyberCC ("Enable Spectrum", Float) = 0
        [Enum(Bass,0,Low Mid,1,High Mid,2,Treble,3)] _Cyber_CC_Band ("Spectrum Primary Band", Float) = 1
        _Cyber_CC_Str ("Spectrum Strip Intensity", Range(0,5)) = 1.0
        [VectorLabel(X Offset, Y Offset, Scale, Rotation)] _Cyber_CC_Transform ("Spectrum Transform", Vector) = (0,0,1,0)

        [Toggle] _UseCyberWave ("Enable Waveform", Float) = 0
        _Cyber_Wave_Str ("Waveform Intensity", Range(0,5)) = 1.0
        [VectorLabel(X Offset, Y Offset, Scale, Rotation)] _Cyber_Wave_Transform ("Waveform Transform", Vector) = (0,0,1,0)

        [Toggle] _UseCyberDMX ("Enable DMX Grid Readout", Float) = 0
        _Cyber_DMX_Str ("DMX Grid Intensity", Range(0,5)) = 1.0
        [VectorLabel(X Offset, Y Offset, Scale, Rotation)] _Cyber_DMX_Transform ("DMX Grid Transform", Vector) = (0,0,1,0)

        [Header(Kinetic Vertex Engine (SM5 Displacement))]
        [Toggle] _UseVtxKinetic ("Enable Vertex Displacement", Float) = 0
        [Enum(Bass,0,Low Mid,1,High Mid,2,Treble,3)] _Vtx_Pump_Band ("Vertex Pump Band", Float) = 0
        _Vtx_Pump_Str ("Vertex Pump Distance", Range(0, 5)) = 0.0
        
        [Enum(Bass,0,Low Mid,1,High Mid,2,Treble,3)] _Vtx_Fracture_Band ("Vertex Fracture Band", Float) = 3
        _Vtx_Fracture_Str ("Vertex Fracture Scatter", Range(0, 5)) = 0.0

        [Header(Kinetic UV Engine (Surface Maps))]
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

        [Header(Global Material Modulations)]
        [Enum(Bass,0,Low Mid,1,High Mid,2,Treble,3)] _AL_Band_Emission ("Emission Band", Float) = 0
        _AL_Emis_Mod ("Emission Amplitude", Range(0,1)) = 0.0
        _AL_Col_Blend ("Color Blend Strength", Range(0,1)) = 0.0
        _AL_Waveform_Mod ("Surface Waveform Ripple", Range(0,1)) = 0.0

        [Enum(Bass,0,Low Mid,1,High Mid,2,Treble,3)] _AL_Band_Scanlines ("Scanline Band", Float) = 1
        _AL_Scanlines ("Scanline Visibility Blend", Range(0,1)) = 0.0
        _AL_Scan_Density ("Scanline Density", Float) = 50.0
        _AL_Scan_Speed ("Base Scan Speed", Float) = 1.0
        _AL_Scan_React ("Audio Speed Reactivity", Float) = 0.5

        [Enum(Bass,0,Low Mid,1,High Mid,2,Treble,3)] _AL_Band_Film ("Thin Film Band", Float) = 2
        _AL_Film_Mod ("Thin Film Expansion", Range(0,1)) = 0.0

        [Enum(Bass,0,Low Mid,1,High Mid,2,Treble,3)] _AL_Band_Parallax ("Parallax Band", Float) = 0
        _AL_Paralx_Mod ("Parallax Thump", Range(0,1)) = 0.0

        [Enum(Bass,0,Low Mid,1,High Mid,2,Treble,3)] _AL_Band_Shatter ("Clearcoat Shatter Band", Float) = 2
        _AL_CC_Shatter ("Clearcoat Shatter", Range(0,1)) = 0.0

        [Enum(Bass,0,Low Mid,1,High Mid,2,Treble,3)] _AL_Band_Glitch ("Glitch Band", Float) = 3
        _AL_Glitch_Mod ("Digital Glitch Tear", Range(0,1)) = 0.0
    }

    CustomEditor "VixenWearEditor"

    SubShader
    {
        Tags { "RenderType"="Opaque" "VRCFallback"="ToonDoubleSided" "Queue"="Geometry" }
        LOD 500
        Cull Off

        // ---------------------------------------------------------------------------------
        // PASS 1: CORE PBR SURFACE SHADER (CLIPS OUT FRACTURED POLYGONS)
        // ---------------------------------------------------------------------------------
        CGPROGRAM
        #pragma surface surf StandardLatex fullforwardshadows addshadow vertex:disp tessellate:tessEdge
        #pragma target 5.0
        #pragma shader_feature_local LIGHTVOLUMES_ENABLE
        #pragma shader_feature_local AL_ENABLE
        #pragma shader_feature_local CYBER_ENABLE
        #pragma shader_feature_local LTCGI_ENABLE
        #pragma shader_feature_local VRSL_ENABLE
        #pragma shader_feature_local _DETAIL_NORMAL

        #include "UnityPBSLighting.cginc"
        #include "Tessellation.cginc"
        #include "UnityCG.cginc"

        #if defined(LIGHTVOLUMES_ENABLE)
            #include "Packages/com.vixencreations.vixens-toolbox/Editor/Avatar Tools/Shaders/cginc/LightVolumes.cginc"
        #endif
        #if defined(AL_ENABLE)
            #include "Packages/com.vixencreations.vixens-toolbox/Editor/Avatar Tools/Shaders/cginc/AudioLink.cginc"
        #endif
        #if defined(LTCGI_ENABLE)
            #include "Packages/com.vixencreations.vixens-toolbox/Editor/Avatar Tools/Shaders/cginc/LTCGI.cginc"
        #endif

        struct SurfaceOutputStandardLatex
        {
            fixed3 Albedo; float3 Normal; float3 ClearcoatNormal; float3 LTCGINormal;     
            float3 WorldPos; float2 UV; float3x3 WorldToTangent;
            half3 Emission; half3 Matcap; half Metallic; half Smoothness;
            half BaseRoughness; half ClearcoatSmoothness; half ClearcoatStrength;
            half ThinFilmStrength; half ThinFilmThickness; half Occlusion; half Height;
            fixed Alpha; float ParallaxDepth; half3 NeonColor; half Thickness;
        };

        struct Input
        {
            float2 uv_MainTex; fixed facing : VFACE; float3 viewDir;         
            float3 worldPos; float3 worldNormal;
            uint primID : SV_PrimitiveID; 
            INTERNAL_DATA 
        };

        sampler2D _MainTex, _MetallicGlossMap, _BumpMap, _DetailNormalMap, _EmissionMap, _MatCap, _MatCapMask, _CyberMask;
        fixed4 _Color, _EmissionColor; half _CutOff, _MinBrightness;
        float _UV_Rot, _SpeedX, _SpeedY, _MatCap_Rot;
        float _AO_Str, _Spec_Occ, _Shad_Hard, _Norm_Str;
        float _Parallax, _Disp_Str, _Tess_Edge, _Emis_Exp;
        float _CC_Strength, _CC_Smoothness, _CC_Spec_AA, _CC_Flat;
        float _Film_Str, _Film_Thick, _Rim_Str, _Rim_Power;
        float _SSS_Str, _SSS_Dist, _SSS_Power;
        float _Det_Strength, _Det_UV_Tiling;
        float _MatCap_Int, _MatCap_Lit, _LV_Int, _LTCGI_Int;

        float _UseALVortex, _AL_Vortex_Band, _AL_Vortex_Str; float4 _AL_Vortex_UV;
        float _UseALPump, _AL_Pump_Band, _AL_Pump_Str; float4 _AL_Pump_UV;
        float _UseALFracture, _AL_Fracture_Band, _AL_Fracture_Str; float4 _AL_Fracture_UV;
        
        float _UseVtxKinetic, _Vtx_Pump_Band, _Vtx_Pump_Str;
        float _Vtx_Fracture_Band, _Vtx_Fracture_Str;

        float _UseCyber; float _Cyber_AutoCorr_Str;
        float _UseCyberVU, _Cyber_VU_Band, _Cyber_VU_Str; float4 _Cyber_VU_Transform;
        float _UseCyberCC, _Cyber_CC_Band, _Cyber_CC_Str; float4 _Cyber_CC_Transform;
        float _UseCyberWave, _Cyber_Wave_Str; float4 _Cyber_Wave_Transform;
        float _UseCyberDMX, _Cyber_DMX_Str; float4 _Cyber_DMX_Transform;

        float _AL_ColorMode, _UseMediaState, _AL_Waveform_Mod;
        float _AL_Band_Emission, _AL_Band_Scanlines, _AL_Band_Film, _AL_Band_Parallax, _AL_Band_Shatter, _AL_Band_Glitch;
        float _AL_Emis_Mod, _AL_Col_Blend, _AL_Scanlines, _AL_Scan_Density, _AL_Scan_Speed, _AL_Scan_React;
        float _AL_Film_Mod, _AL_Paralx_Mod, _AL_CC_Shatter, _AL_Glitch_Mod;

        int _DMX_Channel; float _VRSL_Intensity, _VRSL_Geo_Warp;
        uniform sampler2D _Udon_DMXGridRenderTexture;
        uniform float4 _Udon_DMXGridRenderTexture_TexelSize;
        uniform sampler2D _Udon_DMXGridStrobeOutput;
        uniform sampler2D _Udon_DMXGridRenderTextureMovement;
        uniform float _MediaPlaying;

        float FetchVRSLChannel(uint absoluteChannel, sampler2D tex, float4 texelSize) {
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

        float2 TransformHUD(float2 uv, float4 transform) {
            float2 outUV = uv - 0.5 - transform.xy;
            outUV /= max(0.001, transform.z);
            float rad = transform.w * (UNITY_PI / 180.0);
            float s = sin(rad), c = cos(rad);
            outUV = mul(outUV, float2x2(c, -s, s, c));
            return outUV + 0.5;
        }

        float2 TransformUV(float2 uv, float4 trans) {
            float2 outUV = uv - 0.5 - trans.xy;
            outUV /= max(0.001, trans.z);
            float rad = trans.w * (UNITY_PI / 180.0);
            float s = sin(rad), c = cos(rad);
            outUV = mul(outUV, float2x2(c, -s, s, c));
            return outUV + 0.5;
        }

        float4 tessEdge(appdata_full v0, appdata_full v1, appdata_full v2) {
            return UnityEdgeLengthBasedTess(v0.vertex, v1.vertex, v2.vertex, _Tess_Edge);
        }

        // ------------------------------------------------------------
        // VERTEX DISPLACEMENT ENGINE (FIXED BUGS)
        // ------------------------------------------------------------
        void disp(inout appdata_full v) {
            float d = tex2Dlod(_MetallicGlossMap, float4(v.texcoord.xy,0,0)).b * _Disp_Str;
            
            #if defined(VRSL_ENABLE)
                uint dmxBase = (uint)_DMX_Channel;
                float pan = FetchVRSLChannel(dmxBase + 1, _Udon_DMXGridRenderTextureMovement, _Udon_DMXGridRenderTexture_TexelSize) * 2.0 - 1.0;
                float tilt = FetchVRSLChannel(dmxBase + 2, _Udon_DMXGridRenderTextureMovement, _Udon_DMXGridRenderTexture_TexelSize) * 2.0 - 1.0;
                float dmxWarp = (pan * v.normal.x + tilt * v.normal.z) * _VRSL_Geo_Warp * 0.05;
                d += dmxWarp * tex2Dlod(_MetallicGlossMap, float4(v.texcoord.xy,0,0)).b;
            #endif

            #if defined(AL_ENABLE)
                if (_UseVtxKinetic > 0.5) 
                {
                    float b0 = 0, b1 = 0, b2 = 0, b3 = 0;
                    if (AudioLinkIsAvailable()) {
                        // FIXED: AL bands are mapped to the Y axis (0,1,2,3), not X axis.
                        b0 = saturate(AudioLinkData(ALPASS_AUDIOLINK + int2(0, 0)).r);
                        b1 = saturate(AudioLinkData(ALPASS_AUDIOLINK + int2(0, 1)).r);
                        b2 = saturate(AudioLinkData(ALPASS_AUDIOLINK + int2(0, 2)).r);
                        b3 = saturate(AudioLinkData(ALPASS_AUDIOLINK + int2(0, 3)).r);
                    }
                    float vtxAmps[4] = { pow(b0*2.2, 0.55), pow(b1*2.5, 0.5), pow(b2*2.5, 0.5), pow(b3*3.0, 0.45) };
                    float vPump = vtxAmps[(int)_Vtx_Pump_Band];
                    if (_UseMediaState > 0.5 && _MediaPlaying > 1.5) { vPump = 0; }

                    // FIXED: Displacing strictly along v.normal to inflate volumetrically. 
                    // No more "Move Tool" sliding behavior.
                    v.vertex.xyz += v.normal * (vPump * _Vtx_Pump_Str);
                }
            #endif

            v.vertex.xyz += v.normal * d;
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            v.texcoord1.xyz = _WorldSpaceCameraPos.xyz - worldPos;
        }

        // PBR Helper Logic...
        float2 ParallaxRaymarching(float2 uv, float3 viewDirTangent, float parallaxDepth) {
            float parallaxLimit = -length(viewDirTangent.xy) / max(viewDirTangent.z, 0.001);
            parallaxLimit *= parallaxDepth;
            float2 vOffsetDir = normalize(viewDirTangent.xy);
            float2 vMaxOffset = vOffsetDir * parallaxLimit;
            int numSteps = (int)lerp(48.0, 8.0, max(viewDirTangent.z, 0.0));
            float stepSize = 1.0 / (float)numSteps;
            float2 dx = ddx(uv); float2 dy = ddy(uv);
            
            float currentLayerHeight = 1.0; float2 currentUVOffset = 0.0;
            float2 stepUVOffset = vMaxOffset * stepSize;
            float currentMapHeight = tex2Dgrad(_MetallicGlossMap, uv, dx, dy).b;

            UNITY_LOOP
            for(int i = 0; i < 50; i++) {
                if (currentMapHeight >= currentLayerHeight) break;
                currentLayerHeight -= stepSize; currentUVOffset += stepUVOffset;
                currentMapHeight = tex2Dgrad(_MetallicGlossMap, uv + currentUVOffset, dx, dy).b;
            }
            float prevLayerHeight = currentLayerHeight + stepSize;
            float prevMapHeight = tex2Dgrad(_MetallicGlossMap, uv + currentUVOffset - stepUVOffset, dx, dy).b;
            float weight = (currentLayerHeight - currentMapHeight) / max((currentLayerHeight - currentMapHeight) + (prevMapHeight - prevLayerHeight), 0.0001);
            return uv + (currentUVOffset - stepUVOffset * weight);
        }

        inline half3 GTAOMultiBounce(half visibility, half3 albedo) {
            half3 a =  2.0404 * albedo - 0.3324; half3 b = -4.7951 * albedo + 0.6417; half3 c =  2.7552 * albedo + 0.6903;
            return max(visibility, ((visibility * a + b) * visibility + c) * visibility);
        }

        inline half HDRPSpecularOcclusion(half NdotV, half AO, half roughness) {
            return saturate(pow(max(AO, 0.01), exp2(-roughness)) * (NdotV + AO));
        }

        half4 BRDF3_Latex_Clearcoat(half3 diffColor, half3 matcap, half3 specColor, half oneMinusReflectivity, SurfaceOutputStandardLatex s, float3 viewDir, UnityLight light, UnityIndirect gi, half3 clearcoatEnv) {
            float3 L = light.dir, V = viewDir, N = s.Normal, Nc = s.ClearcoatNormal, H = Unity_SafeNormalize(L + V);
            half NdotL = saturate(dot(N, L)), NdotV = saturate(dot(N, V)), NdotH = saturate(dot(N, H));
            float LdotH = saturate(dot(L, H)); half NcNdotV = saturate(dot(Nc, V)), NcNdotH = saturate(dot(Nc, H));

            half rawAO = s.Occlusion; 
            half3 multiBounceAO = GTAOMultiBounce(rawAO, diffColor);
            half specOcc = lerp(1.0, HDRPSpecularOcclusion(NcNdotV, rawAO, s.BaseRoughness), _Spec_Occ);

            half3 thinFilmColor = 1.0;
            if (s.ThinFilmStrength > 0.001) {
                half cosTheta2 = sqrt(1.0 - 0.44 * (1.0 - pow(NcNdotV, 2.0)));
                half nmThickness = lerp(100.0, 2000.0, s.ThinFilmThickness);
                half pathLength = 2.0 * nmThickness * cosTheta2;
                half3 phase = (2.0 * UNITY_PI * pathLength) / half3(650.0, 510.0, 475.0);
                half3 iridescence = saturate(cos(phase) * 0.5 + 0.5) * 2.0;
                thinFilmColor = lerp(1.0, iridescence, s.ThinFilmStrength);
            }

            float shadowTrace = 1.0;
            if (NdotL > 0.0) {
                float3 lightDirTangent = mul(s.WorldToTangent, L);
                float2 lightDirUV = lightDirTangent.xy * s.ParallaxDepth;
                float shadowHeight = tex2Dlod(_MetallicGlossMap, float4(s.UV + lightDirUV, 0, 0)).b;
                shadowTrace = saturate(1.0 - (s.Height - shadowHeight) * _Shad_Hard);
            }

            half envFresnel = 0.04 + 0.96 * pow(1.0 - NcNdotV, 5.0);
            half envMaskedFresnel = envFresnel * s.ClearcoatStrength;
            half energyConservation = (1.0 - envMaskedFresnel);
            half directFresnel = 0.04 + 0.96 * pow(1.0 - NcNdotH, 5.0);
            half directMaskedFresnel = directFresnel * s.ClearcoatStrength;

            half a = s.BaseRoughness; float a2 = a * a; float d = NdotH * NdotH * (a2 - 1.f) + 1.00001f;
            float specularTerm = a2 / max(0.00001f, max(0.1f, LdotH * LdotH) * (s.BaseRoughness + 0.5f) * (d * d) * 4.0f);
            half3 baseSpecular = specularTerm * specColor * light.color * NdotL * energyConservation * shadowTrace;

            half ccRough = max(1.0 - s.ClearcoatSmoothness, 0.01); float cc_a2 = ccRough * ccRough;
            float cc_d = NcNdotH * NcNdotH * (cc_a2 - 1.f) + 1.00001f;
            float clearcoatSpecTerm = cc_a2 / max(0.00001f, max(0.1f, LdotH * LdotH) * (ccRough + 0.5f) * (cc_d * cc_d) * 4.0f);
            half3 ccSpecular = clearcoatSpecTerm * light.color * saturate(dot(Nc, L)) * directMaskedFresnel * thinFilmColor * shadowTrace;

            float wrap = saturate((NdotL + _SSS_Dist) / max(0.00001f, 1.0f + _SSS_Dist));
            float back = pow(saturate(dot(V, -L)), _SSS_Power);
            float sssTerm = (wrap * 0.6 + back * 0.4) * _SSS_Str;
            half3 absorption = 1.0 - diffColor;
            half3 sssProfile = exp2(-s.Thickness * absorption * 4.0);
            half3 sssColor = diffColor * light.color * sssTerm * sssProfile * s.Thickness;

            half rimExponent = lerp(30.0, 0.1, saturate(_Rim_Power / 10.0));
            half rim = pow(1.0 - NcNdotV, rimExponent) * _Rim_Str * diffColor * (gi.diffuse + 0.1) * saturate(_Rim_Power * 100.0);

            half3 finalColor = gi.diffuse * diffColor * energyConservation * multiBounceAO + sssColor + baseSpecular + ccSpecular + gi.specular * specColor * energyConservation * specOcc + clearcoatEnv * envMaskedFresnel * thinFilmColor * specOcc + rim;

            #if defined(LTCGI_ENABLE)
                float3 base_ltc_d = 0, base_ltc_s = 0; LTCGI_Contribution(s.WorldPos, N, V, s.BaseRoughness, float2(0,0), base_ltc_d, base_ltc_s);
                float3 cc_ltc_d = 0, cc_ltc_s = 0; LTCGI_Contribution(s.WorldPos, Nc, V, ccRough, float2(0,0), cc_ltc_d, cc_ltc_s);
                half ltcBaseSpecOcc = lerp(1.0, HDRPSpecularOcclusion(NdotV, rawAO, s.BaseRoughness), _Spec_Occ);
                half ltcCCSpecOcc = lerp(1.0, HDRPSpecularOcclusion(NcNdotV, rawAO, ccRough), _Spec_Occ);
                half3 ltcgiDiff = base_ltc_d * diffColor * energyConservation * multiBounceAO;
                half3 ltcgiBaseSpec = base_ltc_s * specColor * energyConservation * ltcBaseSpecOcc;
                half3 ltcgiCCSpec = cc_ltc_s * envMaskedFresnel * thinFilmColor * ltcCCSpecOcc;
                finalColor += (ltcgiDiff + ltcgiBaseSpec + ltcgiCCSpec) * _LTCGI_Int;
            #endif

            half3 matcapEval = matcap * saturate(gi.diffuse + light.color * smoothstep(0.0, 0.15, saturate(dot(Nc, L)))) * specOcc;
            finalColor = lerp(finalColor, finalColor + matcapEval, _MatCap_Lit);
            return half4(max(finalColor, diffColor * _MinBrightness), 1);
        }

        void LightingStandardLatex_GI(SurfaceOutputStandardLatex s, UnityGIInput data, inout UnityGI gi) {
            gi = UnityGI_Base(data, 1.0, s.Normal);
            #if defined(LIGHTVOLUMES_ENABLE)
                if (LightVolumesEnabled() > 0.5) {
                    float3 lv_L0 = 0, lv_L1r = 0, lv_L1g = 0, lv_L1b = 0; LightVolumeSH(data.worldPos, lv_L0, lv_L1r, lv_L1g, lv_L1b);
                    gi.indirect.diffuse = lerp(gi.indirect.diffuse, LightVolumeEvaluate(s.Normal, lv_L0, lv_L1r, lv_L1g, lv_L1b), _LV_Int);
                }
            #endif
            Unity_GlossyEnvironmentData g = UnityGlossyEnvironmentSetup(s.Smoothness, data.worldViewDir, s.Normal, lerp(unity_ColorSpaceDielectricSpec.rgb, s.Albedo, s.Metallic));
            gi.indirect.specular = UnityGI_IndirectSpecular(data, 1.0, g);
        }

        inline half4 LightingStandardLatex(SurfaceOutputStandardLatex s, half3 viewDir, UnityGI gi) {
            float3 reflDir = reflect(-viewDir, s.ClearcoatNormal);
            #if UNITY_SPECCUBE_BOX_PROJECTION
                reflDir = BoxProjectedCubemapDirection(reflDir, s.WorldPos, unity_SpecCube0_ProbePosition, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
            #endif
            half mip = (1.0 - s.ClearcoatSmoothness) * 7.0;
            half3 clearcoatEnv = DecodeHDR(UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflDir, mip), unity_SpecCube0_HDR);

            half oneMinusReflectivity; half3 specColor;
            s.Albedo = DiffuseAndSpecularFromMetallic(s.Albedo, s.Metallic, specColor, oneMinusReflectivity);
            half outputAlpha; s.Albedo = PreMultiplyAlpha(s.Albedo, s.Alpha, oneMinusReflectivity, outputAlpha);
            half4 c = BRDF3_Latex_Clearcoat(s.Albedo, s.Matcap, specColor, oneMinusReflectivity, s, viewDir, gi.light, gi.indirect, clearcoatEnv);
            c.a = outputAlpha; return c;
        }

        void surf (Input IN, inout SurfaceOutputStandardLatex o)
        {
            o.WorldPos = IN.worldPos;
            float3 tangentWorld = WorldNormalVector(IN, float3(1,0,0));
            float3 bitangentWorld = WorldNormalVector(IN, float3(0,1,0));
            float3 normalWorld = WorldNormalVector(IN, float3(0,0,1));
            o.WorldToTangent = float3x3(tangentWorld, bitangentWorld, normalWorld);

            float amps[4] = { 0, 0, 0, 0 }; float chronos[4] = { 0, 0, 0, 0 };
            float4 al_color = _EmissionColor; float raw_waveform = 0; float autoCorr = 0;
            float2 baseUV = IN.uv_MainTex; baseUV += float2(_SpeedX, _SpeedY) * _Time.y;

            #if defined(AL_ENABLE)
            if (AudioLinkIsAvailable()) {
                float4 al_amps = 0;
                al_amps.x = AudioLinkData(ALPASS_AUDIOLINK + int2(0, 0)).r;
                al_amps.y = AudioLinkData(ALPASS_AUDIOLINK + int2(0, 1)).r;
                al_amps.z = AudioLinkData(ALPASS_AUDIOLINK + int2(0, 2)).r;
                al_amps.w = AudioLinkData(ALPASS_AUDIOLINK + int2(0, 3)).r;

                amps[0] = pow(saturate(al_amps.x * 2.2), 0.55);
                amps[1] = pow(saturate(al_amps.y * 2.5), 0.50);
                amps[2] = pow(saturate(al_amps.z * 2.5), 0.50);
                amps[3] = pow(saturate(al_amps.w * 3.0), 0.45);

                chronos[0] = AudioLinkData(ALPASS_CHRONOTENSITY + int2(0, 0)).r;
                chronos[1] = AudioLinkData(ALPASS_CHRONOTENSITY + int2(1, 0)).r;
                chronos[2] = AudioLinkData(ALPASS_CHRONOTENSITY + int2(2, 0)).r;
                chronos[3] = AudioLinkData(ALPASS_CHRONOTENSITY + int2(3, 0)).r;
                
                int colorMode = (int)_AL_ColorMode;
                if (colorMode == 1) { al_color = AudioLinkData(ALPASS_CCCOLORS + int2((int)_AL_Band_Emission, 0)); } 
                else if (colorMode >= 2) { al_color = AudioLinkData(ALPASS_CCCOLORS + int2(colorMode - 2, 1)); }
                
                float wavePhase = frac(baseUV.y * 2.0 - _Time.y * 0.2);
                raw_waveform = AudioLinkData(ALPASS_WAVEFORM + int2(wavePhase * 128.0, 0)).r - 0.5;
                autoCorr = AudioLinkData(ALPASS_AUTOCORRELATOR + int2(frac(baseUV.x) * 128.0, 0)).r;
            }

            if (_UseMediaState > 0.5 && _MediaPlaying > 1.5) {
                amps[0] = 0; amps[1] = 0; amps[2] = 0; amps[3] = 0; al_color = _EmissionColor; raw_waveform = 0; autoCorr = 0;
            }
            #endif

            // The Geometry Sub-Routine (Clips out base polygons so Pass 2 can draw the 3D shards)
            #if defined(AL_ENABLE)
            if (_UseVtxKinetic > 0.5 && _Vtx_Fracture_Str > 0.001)
            {
                uint pid = IN.primID;
                float hashX = frac(sin(pid * 12.9898) * 43758.5453);
                float shardMask = step(0.6, hashX) * amps[(int)_Vtx_Fracture_Band];
                if (shardMask > 0.05) clip(-1); // Delete polygon from the base suit
            }
            #endif

            float amp_emis = amps[(int)_AL_Band_Emission]; float chr_emis = chronos[(int)_AL_Band_Emission];
            float amp_scan = amps[(int)_AL_Band_Scanlines]; float chr_scan = chronos[(int)_AL_Band_Scanlines];
            float amp_film = amps[(int)_AL_Band_Film]; float amp_para = amps[(int)_AL_Band_Parallax];
            float amp_shat = amps[(int)_AL_Band_Shatter]; float amp_glit = amps[(int)_AL_Band_Glitch]; float chr_glit = chronos[(int)_AL_Band_Glitch];

            half heartbeat  = amps[0] * 0.65 + amp_emis * 0.35;
            half neuroSpike = amps[3] * (0.5 + amp_emis * 0.5);
            half tension    = amps[2] * 1.3;
            half bio = saturate(heartbeat * 0.55 + tension * 0.25 + neuroSpike * 0.20);
            bio += sin(fmod(chr_emis, 1.0) * 6.2831) * 0.05; bio = saturate(bio);

            #if defined(VRSL_ENABLE)
                uint dmxBase = (uint)_DMX_Channel;
                float dmxIntensity = FetchVRSLChannel(dmxBase + 0, _Udon_DMXGridRenderTexture, _Udon_DMXGridRenderTexture_TexelSize);
                float dmxStrobe = FetchVRSLChannel(dmxBase + 4, _Udon_DMXGridStrobeOutput, _Udon_DMXGridRenderTexture_TexelSize);
                float3 dmxColor = float3(FetchVRSLChannel(dmxBase + 7, _Udon_DMXGridRenderTexture, _Udon_DMXGridRenderTexture_TexelSize), FetchVRSLChannel(dmxBase + 8, _Udon_DMXGridRenderTexture, _Udon_DMXGridRenderTexture_TexelSize), FetchVRSLChannel(dmxBase + 9, _Udon_DMXGridRenderTexture, _Udon_DMXGridRenderTexture_TexelSize));
                float vrslActive = smoothstep(0.01, 0.05, dmxIntensity) * _VRSL_Intensity;
                al_color.rgb = lerp(al_color.rgb, dmxColor, vrslActive); bio = lerp(bio, dmxStrobe * dmxIntensity, vrslActive);
            #endif

            float2 cUV = baseUV;

            if (_UseALVortex > 0.5) {
                float2 vUV = TransformUV(baseUV, _AL_Vortex_UV); float radius = length(vUV - 0.5); float angle = atan2(vUV.y - 0.5, vUV.x - 0.5);
                float twist = (1.0 - saturate(radius * 2.0)) * amps[(int)_AL_Vortex_Band] * _AL_Vortex_Str * sin(chr_glit * UNITY_PI);
                angle += twist; cUV = float2(cos(angle), sin(angle)) * radius + 0.5;
            }

            if (_UseALPump > 0.5) {
                float2 pUV = TransformUV(baseUV, _AL_Pump_UV); float pump = 1.0 - (amps[(int)_AL_Pump_Band] * _AL_Pump_Str);
                pUV = (pUV - 0.5) * pump + 0.5; cUV = pUV;
            }

            if (_UseALFracture > 0.5 && amps[(int)_AL_Fracture_Band] > 0.05 && _AL_Fracture_Str > 0.001) {
                float2 fUV = TransformUV(baseUV, _AL_Fracture_UV); float2 shardGrid = floor(fUV * 12.0); 
                float hashX = frac(sin(dot(shardGrid, float2(12.9898, 78.233))) * 43758.5453); float hashY = frac(sin(dot(shardGrid, float2(39.346, 11.135))) * 43758.5453);
                float fractureMask = step(0.6, hashX) * amps[(int)_AL_Fracture_Band];
                fUV += float2(hashX - 0.5, hashY - 0.5) * fractureMask * _AL_Fracture_Str; cUV = fUV;
            }

            float uvRad = _UV_Rot * (UNITY_PI / 180.0); if (_UseALVortex > 0.5) { uvRad += (chr_emis * 0.15); }
            float uvS = sin(uvRad), uvC = cos(uvRad); cUV = mul(cUV - 0.5, float2x2(uvC, -uvS, uvS, uvC)) + 0.5; baseUV = cUV;

            float3 cyberEmission = 0; float cyberMask = tex2D(_CyberMask, IN.uv_MainTex).r;

            #if defined(AL_ENABLE) && defined(CYBER_ENABLE)
            if (AudioLinkIsAvailable() && cyberMask > 0.01) {
                if (_UseCyberVU > 0.5) {
                    float2 uv = TransformHUD(baseUV, _Cyber_VU_Transform);
                    if (uv.x >= 0 && uv.x <= 1 && uv.y >= 0 && uv.y <= 1) {
                        float lvl = amps[(int)_Cyber_VU_Band]; float vuFill = step(frac(uv.y * 20.0), 0.8) * step(uv.y, lvl);
                        float3 vuColor = lerp(float3(0,1,1), float3(1,0,1), uv.y); cyberEmission += vuColor * vuFill * _Cyber_VU_Str;
                    }
                }
                if (_UseCyberCC > 0.5) {
                    float2 uv = TransformHUD(baseUV, _Cyber_CC_Transform);
                    if (uv.x >= 0 && uv.x <= 1 && uv.y >= 0 && uv.y <= 1) {
                        float stripReact = AudioLinkData(ALPASS_AUDIOLINK + int2(frac(uv.x) * 64.0, 0)).r; cyberEmission += al_color.rgb * pow(stripReact, 2.0) * _Cyber_CC_Str;
                    }
                }
                if (_UseCyberWave > 0.5) {
                    float2 uv = TransformHUD(baseUV, _Cyber_Wave_Transform);
                    if (uv.x >= 0 && uv.x <= 1 && uv.y >= 0 && uv.y <= 1) {
                        float phase = frac(uv.x - _Time.y * 0.2); float wave = AudioLinkData(ALPASS_WAVEFORM + int2(phase * 128.0, 0)).r - 0.5;
                        float waveDist = abs((uv.y - 0.5) * 2.0 - wave); float waveGlow = smoothstep(0.08, 0.0, waveDist);
                        cyberEmission += al_color.rgb * waveGlow * _Cyber_Wave_Str;
                    }
                }
                #if defined(VRSL_ENABLE)
                if (_UseCyberDMX > 0.5) {
                    float2 uv = TransformHUD(baseUV, _Cyber_DMX_Transform);
                    if (uv.x >= 0 && uv.x <= 1 && uv.y >= 0 && uv.y <= 1) {
                        uint dmxCh = (uint)(uv.x * 12.0) + (uint)_DMX_Channel; float dmxVal = FetchVRSLChannel(dmxCh, _Udon_DMXGridRenderTexture, _Udon_DMXGridRenderTexture_TexelSize);
                        float fill = step(frac(uv.y * 10.0), 0.8) * step(uv.y, dmxVal); cyberEmission += float3(1,1,1) * fill * _Cyber_DMX_Str;
                    }
                }
                #endif
            }
            cyberEmission *= cyberMask; float2 cyberUV = baseUV; cyberUV.y += autoCorr * cyberMask * _Cyber_AutoCorr_Str * 0.1; baseUV = cyberUV;
            #endif

            float2 glitch = 0;
            if (_AL_Waveform_Mod > 0.001) { glitch.x += raw_waveform * _AL_Waveform_Mod * 0.15; }
            if (_AL_Glitch_Mod > 0.001 && neuroSpike > 0.05) {
                float slice = floor(baseUV.y * 120.0); float glitchTime = fmod((_Time.y * 9.0) + (chr_glit * 4.0), 1000.0);
                float hash = frac(sin(dot(float2(slice, floor(glitchTime * 33.0)), float2(12.9898, 78.233))) * 43758.5453);
                float glitchMask = step(0.80, hash) * amp_glit;
                glitch.x += (frac(hash * 41.41) - 0.5) * _AL_Glitch_Mod * 1.2 * glitchMask; glitch.y += (frac(hash * 93.93) - 0.5) * _AL_Glitch_Mod * 0.18 * glitchMask;
            }

            float currentP = saturate(_Parallax + (heartbeat * _AL_Paralx_Mod * 0.05));
            float2 finalUV = ParallaxRaymarching(baseUV + glitch, normalize(IN.viewDir), currentP);

            o.UV = finalUV; o.ParallaxDepth = currentP;

            fixed4 c = tex2D(_MainTex, finalUV) * _Color; clip(c.a - _CutOff); o.Albedo = c.rgb; o.Alpha = c.a;

            float4 pbr = tex2D(_MetallicGlossMap, finalUV); o.Metallic = pbr.r; o.Occlusion = lerp(1.0, pbr.g, _AO_Str); o.Height = pbr.b; o.Smoothness = pbr.a; o.Thickness = max(0.05, saturate(1.0 - pbr.b));

            o.ClearcoatStrength = _CC_Strength; o.ClearcoatSmoothness = saturate(_CC_Smoothness - amp_shat * _AL_CC_Shatter * 3.0);
            o.ThinFilmThickness = saturate(_Film_Thick + (amp_film * _AL_Film_Mod * 0.5)); o.ThinFilmStrength = _Film_Str;

            float facing = IN.facing > 0.5 ? 1.0 : -1.0; float3 nMain = UnpackScaleNormal(tex2D(_BumpMap, finalUV), _Norm_Str);
            #if defined(_DETAIL_NORMAL)
            float3 nDet = UnpackScaleNormal(tex2D(_DetailNormalMap, finalUV * _Det_UV_Tiling), _Det_Strength); nMain = normalize(float3(nMain.xy + nDet.xy, nMain.z * nDet.z));
            #endif

            nMain.z *= facing; o.Normal = nMain;

            float3 v_ddx = ddx(o.Normal); float3 v_ddy = ddy(o.Normal); float var = _CC_Spec_AA * (dot(v_ddx, v_ddx) + dot(v_ddy, v_ddy));
            o.BaseRoughness = PerceptualRoughnessToRoughness(SmoothnessToPerceptualRoughness(o.Smoothness)); o.BaseRoughness = lerp(o.BaseRoughness, 1.0, saturate(var * 10.0));
            o.ClearcoatNormal = WorldNormalVector(IN, normalize(float3(o.Normal.xy * _CC_Flat, o.Normal.z))); o.LTCGINormal = o.ClearcoatNormal;

            float3 viewNormal = mul((float3x3)UNITY_MATRIX_V, o.ClearcoatNormal); float mcRad = _MatCap_Rot * (UNITY_PI / 180.0); float mcS = sin(mcRad); float mcC = cos(mcRad);
            viewNormal.xy = mul(viewNormal.xy, float2x2(mcC, -mcS, mcS, mcC)); o.Matcap = tex2D(_MatCap, viewNormal.xy * 0.5 + 0.5).rgb * _MatCap_Int * tex2D(_MatCapMask, finalUV).r;

            float4 em = tex2D(_EmissionMap, finalUV); o.NeonColor = lerp(_EmissionColor.rgb, al_color.rgb, saturate(_AL_Col_Blend * 1.5));
            float scanTime = fmod((_Time.y * _AL_Scan_Speed * 1.8) + (chr_scan * _AL_Scan_React * 0.8), 628.318); float scanFreq = finalUV.y * _AL_Scan_Density; float scanWave = sin(scanFreq - scanTime) * 0.5 + 0.5;
            float scan = smoothstep(0.25, 0.75, scanWave + amp_scan * 0.4); float scanMask = lerp(1.0, scan, _AL_Scanlines);
            float emisBoost = 1.0 + bio * _AL_Emis_Mod * 8.0; o.Emission = (em.rgb * o.NeonColor * em.a * emisBoost * scanMask * _Emis_Exp) + cyberEmission;
        }
        ENDCG

        // ---------------------------------------------------------------------------------
        // PASS 2: THE TRUE SM5 GEOMETRY SHADER ENGINE (RENDERS THE FLYING SHATTERED SHARDS)
        // ---------------------------------------------------------------------------------
        Pass 
        {
            Name "GEOMETRY_FRACTURE"
            Tags { "LightMode" = "ForwardBase" }
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #pragma target 5.0
            #pragma shader_feature_local AL_ENABLE

            #include "UnityCG.cginc"
            
            #if defined(AL_ENABLE)
                #include "Packages/com.vixencreations.vixens-toolbox/Editor/Avatar Tools/Shaders/cginc/AudioLink.cginc"
            #endif

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2g {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct g2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 color : COLOR;
            };

            float _UseVtxKinetic;
            float _Vtx_Fracture_Band;
            float _Vtx_Fracture_Str;
            float4 _EmissionColor;
            float4 _Color;
            sampler2D _MainTex;

            v2g vert(appdata v) {
                v2g o;
                o.vertex = v.vertex;
                o.uv = v.uv;
                return o;
            }

            [maxvertexcount(3)]
            void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream, uint primID : SV_PrimitiveID) 
            {
                #if defined(AL_ENABLE)
                if (_UseVtxKinetic < 0.5 || _Vtx_Fracture_Str < 0.001) return;

                // AudioLink Data mapped to Y Axis perfectly
                float b3 = 0;
                if (AudioLinkIsAvailable()) {
                    b3 = saturate(AudioLinkData(ALPASS_AUDIOLINK + int2(0, (int)_Vtx_Fracture_Band)).r);
                }
                float vFrac = pow(b3 * 3.0, 0.45);
                if (vFrac < 0.05) return;

                // Sync the geometry shader spawn hash with the surface shader clip hash
                float hashX = frac(sin(primID * 12.9898) * 43758.5453);
                float shardMask = step(0.6, hashX) * vFrac;
                
                // If it wasn't clipped in Pass 1, do not spawn a shard here
                if (shardMask < 0.05) return;

                // Hardware severing of indices: Calculates the absolute center and un-welds the shared vertices
                float3 edge1 = IN[1].vertex.xyz - IN[0].vertex.xyz;
                float3 edge2 = IN[2].vertex.xyz - IN[0].vertex.xyz;
                float3 faceNormal = normalize(cross(edge1, edge2));
                
                // FIXED: Renamed from reserved keyword 'centroid' to 'faceCenter'
                float3 faceCenter = (IN[0].vertex.xyz + IN[1].vertex.xyz + IN[2].vertex.xyz) / 3.0;

                for(int i = 0; i < 3; i++) {
                    g2f o;
                    float3 pos = IN[i].vertex.xyz;
                    
                    // Shrink the fractured shard slightly so you can see it physically separate
                    pos = lerp(pos, faceCenter, 0.15 + (vFrac * 0.25));
                    
                    // Blow the literal polygon outward along its own isolated face normal
                    pos += faceNormal * (shardMask * _Vtx_Fracture_Str);
                    
                    o.pos = UnityObjectToClipPos(float4(pos, 1.0));
                    o.uv = IN[i].uv;
                    o.color = _EmissionColor.rgb * (1.0 + vFrac * 4.0); // Make the flying polygons emit light
                    triStream.Append(o);
                }
                triStream.RestartStrip();
                #endif
            }

            fixed4 frag(g2f i) : SV_Target {
                fixed4 tex = tex2D(_MainTex, i.uv) * _Color;
                return fixed4(tex.rgb + i.color, 1.0);
            }
            ENDCG
        }
    }
    FallBack "Standard"
}