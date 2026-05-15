Shader "VixenWear/Latex Ultra"
{
    Properties
    {
        [Header(Base Properties)]
        _Color ("Color", Color) = (1,1,1,1)
        _CutOff ("CutOff", Range(0,1)) = 0.5
        _MainTex ("Albedo (RGB) Cutout (A)", 2D) = "white" {}

        [Header(Packed PBR and Displacement)]
        _MetallicGlossMap ("Packed PBR Map (R:Met G:AO B:Disp A:Smooth)", 2D) = "white" {}
        
        [Header(Parallax and Depth)]
        _Parallax ("Parallax Depth", Range(0, 0.1)) = 0.02
        _NormalShadowHardness ("Micro-Shadow Hardness", Range(0, 5)) = 2.0
        
        [Header(Tessellation)]
        _TessellationEdgeLength ("Tessellation Edge Length", Range(2, 50)) = 15
        _DisplacementStrength ("Displacement Strength", Range(0, 0.2)) = 0.05
        
        [Header(Clearcoat and Polish)]
        _ClearcoatStrength ("Clearcoat Strength", Range(0, 1)) = 1.0
        _ClearcoatSmoothness ("Clearcoat Smoothness", Range(0, 1)) = 0.95
        _SpecAAVariance ("Specular AA Variance", Range(0, 1)) = 0.15
        
        [Header(Thin Film Interference)]
        _ThinFilmStrength ("Iridescence Strength", Range(0, 1)) = 0.5
        _ThinFilmThickness ("Base Polish Layer Thickness (nm)", Range(100, 1000)) = 400

        [Header(Normals and Micro Detail)]
        [Normal] _BumpMap ("Normal Map", 2D) = "bump" {}
        _BumpScale ("Normal Strength", Range(0, 5)) = 1.0
        _ClearcoatBumpScale ("Clearcoat Normal Flattening", Range(0, 1)) = 0.15
        [Normal] _DetailNormalMap ("Micro Detail Normal", 2D) = "bump" {}
        _DetailNormalScale ("Detail Strength", Range(0, 5)) = 0.5
        _DetailUVScale ("Detail UV Tiling", Float) = 10.0

        [Header(Advanced Shadows and Occlusion)]
        _AOStrength ("Ambient Occlusion Strength", Range(0, 1)) = 1.0
        _SpecularOcclusion ("Specular Occlusion Strength", Range(0, 1)) = 1.0

        [Header(Emission)]
        [HDR] _EmissionColor ("Emission Color", Color) = (0,0,0,1)
        _EmissionMap ("Emission Map", 2D) = "black" {}

        [Header(MatCap Environment)]
        _MatCap ("MatCap Texture", 2D) = "black" {}
        _MatCapMask ("MatCap Mask", 2D) = "white" {}
        _MatCapIntensity ("MatCap Intensity", Range(0, 5)) = 1
        _MatCapLighting ("Matcap Lighting Mix", Range(0, 1)) = 0.8

        [Header(VRC Light Volumes)]
        [Toggle(LIGHTVOLUMES_ENABLE)] _UseLightVolumes ("Enable Light Volumes", Float) = 0
        _LightVolumeIntensity ("Light Volume Intensity", Range(0, 5)) = 1.0

        [Header(AudioLink Integration)]
        [Toggle(AL_ENABLE)] _UseAudioLink ("Enable AudioLink", Float) = 0
        _AudioLinkFilmMod ("Iridescence Bass Pulse (nm)", Range(0, 500)) = 200
        _AudioLinkEmissionMod ("Emission Treble Pulse", Range(0, 5)) = 0.0

        [Header(LTCGI Integration)]
        [Toggle(LTCGI_ENABLE)] _UseLTCGI ("Enable LTCGI", Float) = 0
        _LTCGIIntensity ("LTCGI Intensity", Range(0, 5)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "VRCFallback"="ToonDoubleSided" "Queue"="Geometry" }
        LOD 400
        Cull Off

        CGPROGRAM
        #pragma surface surf StandardLatex fullforwardshadows addshadow vertex:disp tessellate:tessEdge
        #pragma target 5.0
        #pragma shader_feature_local LIGHTVOLUMES_ENABLE
        #pragma shader_feature_local AL_ENABLE
        #pragma shader_feature_local LTCGI_ENABLE

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
            fixed3 Albedo;
            float3 Normal;
            float3 ClearcoatNormal; 
            float3 LTCGINormal;     
            float3 WorldPos; 
            float2 UV;
            float3x3 WorldToTangent;
            
            half3 Emission;
            half3 Matcap;
            
            half Metallic;
            half Smoothness;
            half BaseRoughness;
            half ClearcoatSmoothness;
            half ClearcoatStrength;
            half ThinFilmStrength;
            half ThinFilmThickness;
            
            half Occlusion;
            half Height;
            fixed Alpha;
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

        sampler2D _MainTex, _MetallicGlossMap, _BumpMap, _DetailNormalMap, _EmissionMap, _MatCap, _MatCapMask;
        half _CutOff, _BumpScale, _ClearcoatBumpScale, _DetailNormalScale, _DetailUVScale;
        half _TessellationEdgeLength, _DisplacementStrength;
        half _ClearcoatStrength, _ClearcoatSmoothness, _ThinFilmStrength, _ThinFilmThickness, _SpecAAVariance;
        half _AOStrength, _SpecularOcclusion, _Parallax, _NormalShadowHardness;
        fixed4 _Color, _EmissionColor;
        fixed _MatCapIntensity, _MatCapLighting;
        half _LightVolumeIntensity, _AudioLinkFilmMod, _AudioLinkEmissionMod, _LTCGIIntensity;

        float4 tessEdge(appdata_full v0, appdata_full v1, appdata_full v2)
        {
            return UnityEdgeLengthBasedTess(v0.vertex, v1.vertex, v2.vertex, _TessellationEdgeLength);
        }

        void disp(inout appdata_full v)
        {
            float d = tex2Dlod(_MetallicGlossMap, float4(v.texcoord.xy,0,0)).b * _DisplacementStrength;
            v.vertex.xyz += v.normal * d;
            
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            v.texcoord1.xyz = _WorldSpaceCameraPos.xyz - worldPos;
        }

        float2 ParallaxRaymarching(float2 uv, float3 viewDirTangent)
        {
            float parallaxLimit = -length(viewDirTangent.xy) / max(viewDirTangent.z, 0.001);
            parallaxLimit *= _Parallax;

            float2 vOffsetDir = normalize(viewDirTangent.xy);
            float2 vMaxOffset = vOffsetDir * parallaxLimit;

            int numSteps = (int)lerp(48.0, 8.0, max(viewDirTangent.z, 0.0));
            float stepSize = 1.0 / (float)numSteps;

            float2 dx = ddx(uv);
            float2 dy = ddy(uv);

            float currentLayerHeight = 1.0;
            float2 currentUVOffset = 0.0;
            float2 stepUVOffset = vMaxOffset * stepSize;

            float currentMapHeight = tex2Dgrad(_MetallicGlossMap, uv, dx, dy).b;

            UNITY_LOOP
            for(int i = 0; i < 50; i++)
            {
                if (currentMapHeight >= currentLayerHeight) break;
                currentLayerHeight -= stepSize;
                currentUVOffset += stepUVOffset;
                currentMapHeight = tex2Dgrad(_MetallicGlossMap, uv + currentUVOffset, dx, dy).b;
            }

            float prevLayerHeight = currentLayerHeight + stepSize;
            float prevMapHeight = tex2Dgrad(_MetallicGlossMap, uv + currentUVOffset - stepUVOffset, dx, dy).b;

            float weight = (currentLayerHeight - currentMapHeight) / max((currentLayerHeight - currentMapHeight) + (prevMapHeight - prevLayerHeight), 0.0001);
            float2 finalUVOffset = currentUVOffset - stepUVOffset * weight;

            return uv + finalUVOffset;
        }

        inline half3 BlendMatcapNatural(
            half3 matcap,
            half3 normal,
            half3 viewDir,
            half smoothness,
            half intensity)
        {
            // Fresnel for edge emphasis
            half nv = saturate(dot(normal, viewDir));
            half fres = pow(1.0 - nv, 5.0);

            // Roughness attenuates matcap clarity
            half rough = 1.0 - smoothness;
            half clarity = saturate(1.0 - rough * 1.25);

            // Energy normalization
            half3 mc = matcap * clarity * (0.25 + fres * 0.75);

            return mc * intensity;
        }

        half4 BRDF3_Latex_Clearcoat(
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
            float3 halfDir = Unity_SafeNormalize(float3(light.dir) + viewDir);

            half nl = saturate(dot(s.Normal, light.dir));
            float nh = saturate(dot(s.Normal, halfDir));
            half nv = saturate(dot(s.Normal, viewDir));
            float lh = saturate(dot(light.dir, halfDir));

            // Smooth easing of direct light near the light boundary to avoid "popping"
            half nlSmooth = smoothstep(0.0, 0.15, nl);

            float shadowTrace = 1.0;
            if (nl > 0.0) {
                float3 lightDirTangent = mul(s.WorldToTangent, light.dir);
                float2 lightDirUV = lightDirTangent.xy * _Parallax;
                float shadowHeight = tex2Dlod(_MetallicGlossMap, float4(s.UV + lightDirUV, 0, 0)).b;
                shadowTrace = saturate(1.0 - (s.Height - shadowHeight) * _NormalShadowHardness);
            }
            half normalShadowMask = shadowTrace;
    
            half cc_nl = saturate(dot(s.ClearcoatNormal, light.dir)) * normalShadowMask;
            half cc_nlSmooth = smoothstep(0.0, 0.15, cc_nl);
            float cc_nh = saturate(dot(s.ClearcoatNormal, halfDir));
            half cc_nv = saturate(dot(s.ClearcoatNormal, viewDir));

            half specOcc = lerp(1.0, s.Occlusion, _SpecularOcclusion);

            half a = s.BaseRoughness;
            float a2 = a * a;
            float d = nh * nh * (a2 - 1.f) + 1.00001f;
            float specularTerm = a2 / (max(0.1f, lh * lh) * (s.BaseRoughness + 0.5f) * (d * d) * 4);
    
            half ccPerceptualRoughness = SmoothnessToPerceptualRoughness(s.ClearcoatSmoothness);
            half ccRoughness = max(PerceptualRoughnessToRoughness(ccPerceptualRoughness), 0.01);
            float cc_a2 = ccRoughness * ccRoughness;
            float cc_d = cc_nh * cc_nh * (cc_a2 - 1.f) + 1.00001f;
            float clearcoatTerm = cc_a2 / (max(0.1f, lh * lh) * (ccRoughness + 0.5f) * (cc_d * cc_d) * 4);

            half Fc = pow(1.0 - cc_nv, 5.0);
            half clearcoatFresnel = lerp(0.04, 1.0, Fc) * s.ClearcoatStrength;

            half cosTheta2 = sqrt(1.0 - pow(1.0 / 1.5, 2.0) * (1.0 - pow(cc_nv, 2.0))); 
            half pathLength = 2.0 * s.ThinFilmThickness * cosTheta2;
            half3 wavelengths = half3(650.0, 510.0, 475.0);
            half3 phase = (2.0 * UNITY_PI * pathLength) / wavelengths;
            half3 iridescence = saturate(0.5 + 0.5 * cos(phase));
            half3 finalClearcoatColor = lerp(float3(1,1,1), iridescence, s.ThinFilmStrength * (1.0 - Fc));

            half3 baseEnv = gi.specular; 
            half grazingTerm = saturate(s.Smoothness + (1 - oneMinusReflectivity));
            half surfaceReduction = 1.0 - s.BaseRoughness * s.BaseRoughness * (0.6 - 0.08 * s.BaseRoughness);
    
            half energyConservation = (1.0 - clearcoatFresnel);
    
            half3 indirectBaseSpec = surfaceReduction * baseEnv * FresnelLerpFast(specColor, grazingTerm, nv) * specOcc * energyConservation;
            half ccSurfaceReduction = 1.0 - ccRoughness * ccPerceptualRoughness * (0.6 - 0.08 * ccPerceptualRoughness);
            half3 indirectClearcoatSpec = ccSurfaceReduction * clearcoatEnv * clearcoatFresnel * specOcc;

            // Existing matcap lighting mix
            matcap = matcap * lerp(float3(1,1,1), saturate(gi.diffuse + light.color * nlSmooth), _MatCapLighting) * specOcc;
    
            // Base BRDF color (without Patch 4’s cross‑fade)
            half3 color =   gi.diffuse * diffColor
                          + diffColor * light.color * nlSmooth * normalShadowMask
                          + specularTerm * specColor * light.color * nlSmooth * energyConservation * normalShadowMask
                          + indirectBaseSpec
                          + clearcoatTerm * clearcoatFresnel * finalClearcoatColor * light.color * cc_nlSmooth
                          + indirectClearcoatSpec * finalClearcoatColor; 

            // --- Patch 4: reflection–matcap harmony ---
            half3 reflectionColor = color;
            half reflectionStrength = saturate(s.Smoothness * 1.2);

            half3 matcapBlend = lerp(
                BlendMatcapNatural(matcap, s.ClearcoatNormal, viewDir, s.ClearcoatSmoothness, _MatCapIntensity),
                reflectionColor,
                reflectionStrength
            );

            color = lerp(color, matcapBlend, _MatCapLighting);
            // --- end Patch 4 ---

            return half4(color, 1);
        }

        inline void LightingStandardLatex_GI(SurfaceOutputStandardLatex s, UnityGIInput data, inout UnityGI gi)
        {
            Unity_GlossyEnvironmentData g = UnityGlossyEnvironmentSetup(s.Smoothness, data.worldViewDir, s.Normal, lerp(unity_ColorSpaceDielectricSpec.rgb, s.Albedo, s.Metallic));
            gi = UnityGI_Base(data, s.Occlusion, s.Normal);

            #if defined(LIGHTVOLUMES_ENABLE)
                if (LightVolumesEnabled() > 0.5)
                {
                    float3 lv_L0 = 0;
                    float3 lv_L1r = 0;
                    float3 lv_L1g = 0;
                    float3 lv_L1b = 0;

                    LightVolumeSH(data.worldPos, lv_L0, lv_L1r, lv_L1g, lv_L1b);

                    float3 lvColor = LightVolumeEvaluate(s.Normal, lv_L0, lv_L1r, lv_L1g, lv_L1b);

                    gi.indirect.diffuse = lerp(gi.indirect.diffuse, lvColor, _LightVolumeIntensity);
                }
            #endif

            gi.indirect.specular = UnityGI_IndirectSpecular(data, s.Occlusion, g);
        }

        inline half4 LightingStandardLatex(SurfaceOutputStandardLatex s, half3 viewDir, UnityGI gi)
        {
            float3 reflDir = reflect(-viewDir, s.ClearcoatNormal);
            #if UNITY_SPECCUBE_BOX_PROJECTION
                reflDir = BoxProjectedCubemapDirection(reflDir, s.WorldPos, unity_SpecCube0_ProbePosition, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
            #endif
            
            half ccPerceptualRoughness = 1.0 - s.ClearcoatSmoothness;
            #if defined(UNITY_SPECCUBE_LOD_STEPS)
                half mip = ccPerceptualRoughness * UNITY_SPECCUBE_LOD_STEPS;
            #else
                half mip = ccPerceptualRoughness * 6.0;
            #endif
            
            float4 envSample = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflDir, mip);
            half3 clearcoatEnv = DecodeHDR(envSample, unity_SpecCube0_HDR);

            half oneMinusReflectivity;
            half3 specColor;
            s.Albedo = DiffuseAndSpecularFromMetallic(s.Albedo, s.Metallic, specColor, oneMinusReflectivity);

            half outputAlpha;
            s.Albedo = PreMultiplyAlpha(s.Albedo, s.Alpha, oneMinusReflectivity, outputAlpha);

            half4 c = BRDF3_Latex_Clearcoat(s.Albedo, s.Matcap, specColor, oneMinusReflectivity, s, viewDir, gi.light, gi.indirect, clearcoatEnv);
            
            #if defined(LTCGI_ENABLE)
                float3 ltcgi_diffuse = 0; float3 ltcgi_specular = 0;
                float ltcgi_roughness = max(1.0 - s.ClearcoatSmoothness, 0.02);
                
                LTCGI_Contribution(s.WorldPos, s.LTCGINormal, viewDir, ltcgi_roughness, float2(0,0), ltcgi_diffuse, ltcgi_specular);
                
                float ltc_specOcc = lerp(1.0, s.Occlusion, _SpecularOcclusion);
                c.rgb += ltcgi_specular * _LTCGIIntensity * s.ClearcoatStrength * ltc_specOcc;
                c.rgb += ltcgi_diffuse * _LTCGIIntensity * s.Occlusion * s.Albedo; 
            #endif

            #if defined(LIGHTVOLUMES_ENABLE)
                if (LightVolumesEnabled() > 0.5)
                {
                    float3 lv_L0 = 0;
                    float3 lv_L1r = 0;
                    float3 lv_L1g = 0;
                    float3 lv_L1b = 0;

                    LightVolumeSH(s.WorldPos, lv_L0, lv_L1r, lv_L1g, lv_L1b);

                    float3 lvDiffuse = LightVolumeEvaluate(s.Normal, lv_L0, lv_L1r, lv_L1g, lv_L1b);
                    c.rgb += lvDiffuse * s.Albedo * _LightVolumeIntensity;

                    float3 lvSpec = LightVolumeSpecular(s.Albedo, s.Smoothness, s.Metallic, normalize(s.Normal), normalize(viewDir), lv_L0, lv_L1r, lv_L1g, lv_L1b);
                    float lvspecOcc = lerp(1.0, s.Occlusion, _SpecularOcclusion);
                    c.rgb += lvSpec * _LightVolumeIntensity * lvspecOcc;
                }
            #endif

            c.a = outputAlpha;
            return c;
        }

        void surf (Input IN, inout SurfaceOutputStandardLatex o)
        {
            o.WorldPos = IN.worldPos;

            // Explicit initialization to avoid garbage before WorldNormalVector
            o.Normal = float3(0, 0, 1);

            float3 viewDirWorld = normalize(IN.viewDir);
            float3 tangentWorld = WorldNormalVector(IN, float3(1,0,0));
            float3 bitangentWorld = WorldNormalVector(IN, float3(0,1,0));
            float3 normalWorld = WorldNormalVector(IN, float3(0,0,1));
            
            float3x3 worldToTangent = float3x3(tangentWorld, bitangentWorld, normalWorld);
            float3 viewDirTangent = mul(worldToTangent, viewDirWorld);
            
            o.WorldToTangent = worldToTangent;
            
            float2 finalUV = ParallaxRaymarching(IN.uv_MainTex, viewDirTangent);
            o.UV = finalUV;

            fixed4 c = tex2D(_MainTex, finalUV) * _Color;
            clip(c.a - _CutOff);
            o.Albedo = c.rgb;
            o.Alpha = c.a;

            float4 packedPBR = tex2D(_MetallicGlossMap, finalUV);
            o.Metallic = packedPBR.r;
            o.Occlusion = lerp(1.0, packedPBR.g, _AOStrength);  
            o.Height = packedPBR.b;
            o.Smoothness = packedPBR.a; 
            
            o.ClearcoatStrength = _ClearcoatStrength;
            o.ClearcoatSmoothness = _ClearcoatSmoothness;
            o.ThinFilmStrength = _ThinFilmStrength;
            
            half al_bass = 0;
            half al_treble = 0;
            
            #if defined(AL_ENABLE)
                if (AudioLinkIsAvailable())
                {
                    al_bass = AudioLinkData(ALPASS_AUDIOLINK + int2(0, 0)).r; 
                    al_treble = AudioLinkData(ALPASS_AUDIOLINK + int2(3, 0)).r; 
                }
            #endif

            o.ThinFilmThickness = _ThinFilmThickness + (al_bass * _AudioLinkFilmMod);

            float facing = IN.facing > 0.5 ? 1.0 : -1.0;

            float3 mainNormal = UnpackScaleNormal(tex2D(_BumpMap, finalUV), _BumpScale);
            float3 detailNormal = UnpackScaleNormal(tex2D(_DetailNormalMap, finalUV * _DetailUVScale), _DetailNormalScale);
            
            float3 tangentBaseNormal = normalize(float3(mainNormal.xy + detailNormal.xy, mainNormal.z * detailNormal.z));
            tangentBaseNormal.z *= facing;
            
            float3 tangentClearcoatNormal = UnpackScaleNormal(tex2D(_BumpMap, finalUV), _BumpScale * _ClearcoatBumpScale);
            tangentClearcoatNormal = normalize(tangentClearcoatNormal);
            tangentClearcoatNormal.z *= facing;

            o.Normal = tangentBaseNormal;
            o.ClearcoatNormal = WorldNormalVector(IN, tangentClearcoatNormal);
            o.LTCGINormal = o.ClearcoatNormal;

            float3 normalDdx = ddx(o.Normal);
            float3 normalDdy = ddy(o.Normal);
            float variance = max(0.0, _SpecAAVariance * (dot(normalDdx, normalDdx) + dot(normalDdy, normalDdy)));
            
            half perceptualRoughness = SmoothnessToPerceptualRoughness(o.Smoothness);
            half baseRoughness = PerceptualRoughnessToRoughness(perceptualRoughness);
            o.BaseRoughness = max(baseRoughness, min(variance, 0.2));

            float matcapMask = tex2D(_MatCapMask, finalUV).r; 
            float3 viewNormal = mul((float3x3)UNITY_MATRIX_V, o.ClearcoatNormal);
            half2 capCoord = viewNormal.xy * 0.5 + 0.5;

            half3 env0 = tex2D(_MatCap, capCoord).rgb * _MatCapIntensity;
            o.Matcap = env0 * matcapMask;
            
            float4 em = tex2D(_EmissionMap, finalUV);
            o.Emission = em.rgb * _EmissionColor.rgb * em.a * (1.0 + (al_treble * _AudioLinkEmissionMod));
        }
        ENDCG
    }
    FallBack "Standard"
}
