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

        [Toggle(LIGHTVOLUMES_ENABLE)] _UseLightVolumes ("Enable Light Volumes", Float) = 0
        [Toggle(LTCGI_ENABLE)] _UseLTCGI ("Enable LTCGI", Float) = 0

        [Header(AudioLink)]
        [Toggle(AL_ENABLE)] _UseAudioLink ("Enable AudioLink", Float) = 0
        [Enum(Bass,0,Low Mid,1,High Mid,2,Treble,3)] _AL_EmissionBand ("Emission Band", Float) = 0

        _AL_Emis_Mod ("Emission Modulation", Range(0,1)) = 0.0
        _AL_Col_Blend ("Color Blend Modulation", Range(0,1)) = 0.0
        _AL_Scanlines ("Scanline Modulation", Range(0,1)) = 0.0
        _AL_Scan_Speed ("Scanline Speed", Float) = 0.0

        _AL_Film_Mod ("Thin Film Modulation", Range(0,1)) = 0.0
        _AL_Paralx_Mod ("Parallax Modulation", Range(0,1)) = 0.0
        _AL_CC_Shatter ("Clearcoat Shatter", Range(0,1)) = 0.0
        _AL_Glitch_Mod ("Glitch Modulation", Range(0,1)) = 0.0
    }

    CustomEditor "VixenWearEditor"

    SubShader
    {
        Tags { "RenderType"="Opaque" "VRCFallback"="ToonDoubleSided" "Queue"="Geometry" }
        LOD 500
        Cull Off

        CGPROGRAM
        #pragma surface surf StandardLatex fullforwardshadows addshadow vertex:disp tessellate:tessEdge
        #pragma target 5.0
        #pragma shader_feature_local LIGHTVOLUMES_ENABLE
        #pragma shader_feature_local AL_ENABLE
        #pragma shader_feature_local LTCGI_ENABLE
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
            float ParallaxDepth; 
            
            half3 NeonColor;
            half Thickness;
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
        
        fixed4 _Color, _EmissionColor;
        half _CutOff;
        half _MinBrightness;

        // Unpacked Parameters
        float _UV_Rot, _SpeedX, _SpeedY, _MatCap_Rot;
        float _AO_Str, _Spec_Occ, _Shad_Hard, _Norm_Str;
        float _Parallax, _Disp_Str, _Tess_Edge, _Emis_Exp;
        float _CC_Strength, _CC_Smoothness, _CC_Spec_AA, _CC_Flat;
        float _Film_Str, _Film_Thick, _Rim_Str, _Rim_Power;
        float _SSS_Str, _SSS_Dist, _SSS_Power;
        float _Det_Strength, _Det_UV_Tiling;
        float _MatCap_Int, _MatCap_Lit, _LV_Int, _LTCGI_Int;
        float _AL_Emis_Mod, _AL_Col_Blend, _AL_Scanlines, _AL_Scan_Speed;
        float _AL_Film_Mod, _AL_Paralx_Mod, _AL_CC_Shatter, _AL_Glitch_Mod;
        half _AL_EmissionBand;

        float4 tessEdge(appdata_full v0, appdata_full v1, appdata_full v2)
        {
            return UnityEdgeLengthBasedTess(v0.vertex, v1.vertex, v2.vertex, _Tess_Edge);
        }

        void disp(inout appdata_full v)
        {
            float d = tex2Dlod(_MetallicGlossMap, float4(v.texcoord.xy,0,0)).b * _Disp_Str;
            v.vertex.xyz += v.normal * d;
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            v.texcoord1.xyz = _WorldSpaceCameraPos.xyz - worldPos;
        }

        float2 ParallaxRaymarching(float2 uv, float3 viewDirTangent, float parallaxDepth)
        {
            float parallaxLimit = -length(viewDirTangent.xy) / max(viewDirTangent.z, 0.001);
            parallaxLimit *= parallaxDepth;
            float2 vOffsetDir = normalize(viewDirTangent.xy);
            float2 vMaxOffset = vOffsetDir * parallaxLimit;
            int numSteps = (int)lerp(48.0, 8.0, max(viewDirTangent.z, 0.0));
            float stepSize = 1.0 / (float)numSteps;
            float2 dx = ddx(uv); float2 dy = ddy(uv);
            
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
            
            return uv + (currentUVOffset - stepUVOffset * weight);
        }

        half4 BRDF3_Latex_Clearcoat(half3 diffColor, half3 matcap, half3 specColor, half oneMinusReflectivity, SurfaceOutputStandardLatex s, float3 viewDir, UnityLight light, UnityIndirect gi, half3 clearcoatEnv)
        {
            float3 halfDir = Unity_SafeNormalize(float3(light.dir) + viewDir);
            half nl = saturate(dot(s.Normal, light.dir));
            float nh = saturate(dot(s.Normal, halfDir));
            half nv = saturate(dot(s.Normal, viewDir));
            float lh = saturate(dot(light.dir, halfDir));

            float cc_nh = saturate(dot(s.ClearcoatNormal, halfDir));
            half cc_nv = saturate(dot(s.ClearcoatNormal, viewDir));
            half specOcc = lerp(1.0, s.Occlusion, _Spec_Occ);

            float shadowTrace = 1.0;
            if (nl > 0.0) 
            {
                float3 lightDirTangent = mul(s.WorldToTangent, light.dir);
                float2 lightDirUV = lightDirTangent.xy * s.ParallaxDepth; 
                float shadowHeight = tex2Dlod(_MetallicGlossMap, float4(s.UV + lightDirUV, 0, 0)).b;
                shadowTrace = saturate(1.0 - (s.Height - shadowHeight) * _Shad_Hard);
            }

            half coatFresnel = 0.04 + 0.96 * pow(1.0 - cc_nv, 5.0);
            half clearcoatMaskedFresnel = coatFresnel * s.ClearcoatStrength;
            half energyConservation = (1.0 - clearcoatMaskedFresnel);

            half3 finalClearcoatColor = 1.0;
            if (nl > 0.0)
            {
                half cosTheta2 = sqrt(1.0 - 0.44 * (1.0 - pow(cc_nv, 2.0))); 
                half pathLength = 2.0 * s.ThinFilmThickness * cosTheta2;
                half3 phase = (2.0 * UNITY_PI * pathLength) / half3(650.0, 510.0, 475.0);
                half3 iridescence = saturate(0.5 + 0.5 * cos(phase));
                finalClearcoatColor = lerp(1.0, iridescence, s.ThinFilmStrength * (1.0 - coatFresnel));
            }

            half a = s.BaseRoughness;
            float a2 = a * a;
            float d = nh * nh * (a2 - 1.f) + 1.00001f;
            float specularTerm = a2 / (max(0.1f, lh * lh) * (s.BaseRoughness + 0.5f) * (d * d) * 4);
            half3 baseSpecular = specularTerm * specColor * light.color * nl * energyConservation * shadowTrace;

            half ccRough = max(1.0 - s.ClearcoatSmoothness, 0.01);
            float cc_a2 = ccRough * ccRough;
            float cc_d = cc_nh * cc_nh * (cc_a2 - 1.f) + 1.00001f;
            float clearcoatSpecTerm = cc_a2 / (max(0.1f, lh * lh) * (ccRough + 0.5f) * (cc_d * cc_d) * 4);
            half3 ccSpecular = clearcoatSpecTerm * light.color * saturate(dot(s.ClearcoatNormal, light.dir)) * clearcoatMaskedFresnel * finalClearcoatColor * shadowTrace;

            half3 transLightDir = light.dir + s.Normal * _SSS_Dist;
            half transDot = pow(saturate(dot(viewDir, -transLightDir)), _SSS_Power) * _SSS_Str;
            half3 sssGlow = transDot * light.color * s.Thickness * s.NeonColor;

            half rim = pow(1.0 - cc_nv, _Rim_Power) * _Rim_Str * s.NeonColor * (gi.diffuse + 0.1);

            half3 finalColor = gi.diffuse * diffColor * energyConservation * s.Occlusion
                             + baseSpecular 
                             + ccSpecular
                             + gi.specular * specColor * energyConservation * specOcc
                             + clearcoatEnv * clearcoatMaskedFresnel * finalClearcoatColor * specOcc
                             + rim + sssGlow;

            half3 matcapEval = matcap * saturate(gi.diffuse + light.color * smoothstep(0.0, 0.15, saturate(dot(s.ClearcoatNormal, light.dir)))) * specOcc;
            finalColor = lerp(finalColor, finalColor + matcapEval, _MatCap_Lit);

            return half4(max(finalColor, diffColor * _MinBrightness), 1);
        }

        void LightingStandardLatex_GI(SurfaceOutputStandardLatex s, UnityGIInput data, inout UnityGI gi)
        {
            gi = UnityGI_Base(data, s.Occlusion, s.Normal);
            #if defined(LIGHTVOLUMES_ENABLE)
                if (LightVolumesEnabled() > 0.5)
                {
                    float3 lv_L0 = 0, lv_L1r = 0, lv_L1g = 0, lv_L1b = 0;
                    LightVolumeSH(data.worldPos, lv_L0, lv_L1r, lv_L1g, lv_L1b);
                    gi.indirect.diffuse = lerp(gi.indirect.diffuse, LightVolumeEvaluate(s.Normal, lv_L0, lv_L1r, lv_L1g, lv_L1b), _LV_Int);
                }
            #endif
            Unity_GlossyEnvironmentData g = UnityGlossyEnvironmentSetup(s.Smoothness, data.worldViewDir, s.Normal, lerp(unity_ColorSpaceDielectricSpec.rgb, s.Albedo, s.Metallic));
            gi.indirect.specular = UnityGI_IndirectSpecular(data, s.Occlusion, g);
        }

        inline half4 LightingStandardLatex(SurfaceOutputStandardLatex s, half3 viewDir, UnityGI gi)
        {
            float3 reflDir = reflect(-viewDir, s.ClearcoatNormal);
            #if UNITY_SPECCUBE_BOX_PROJECTION
                reflDir = BoxProjectedCubemapDirection(reflDir, s.WorldPos, unity_SpecCube0_ProbePosition, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
            #endif
            half mip = (1.0 - s.ClearcoatSmoothness) * 7.0;
            half3 clearcoatEnv = DecodeHDR(UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflDir, mip), unity_SpecCube0_HDR);

            half oneMinusReflectivity; half3 specColor;
            s.Albedo = DiffuseAndSpecularFromMetallic(s.Albedo, s.Metallic, specColor, oneMinusReflectivity);
            half outputAlpha;
            s.Albedo = PreMultiplyAlpha(s.Albedo, s.Alpha, oneMinusReflectivity, outputAlpha);

            half4 c = BRDF3_Latex_Clearcoat(s.Albedo, s.Matcap, specColor, oneMinusReflectivity, s, viewDir, gi.light, gi.indirect, clearcoatEnv);
            
            #if defined(LTCGI_ENABLE)
                float3 ltc_d = 0, ltc_s = 0;
                LTCGI_Contribution(s.WorldPos, s.LTCGINormal, viewDir, max(1.0 - s.ClearcoatSmoothness, 0.02), float2(0,0), ltc_d, ltc_s);
                c.rgb += (ltc_s * s.ClearcoatStrength + ltc_d * s.Albedo) * _LTCGI_Int * s.Occlusion;
            #endif

            c.a = outputAlpha;
            return c;
        }

        void surf (Input IN, inout SurfaceOutputStandardLatex o)
        {
            o.WorldPos = IN.worldPos;
            
            float3 tangentWorld = WorldNormalVector(IN, float3(1,0,0));
            float3 bitangentWorld = WorldNormalVector(IN, float3(0,1,0));
            float3 normalWorld = WorldNormalVector(IN, float3(0,0,1));
            o.WorldToTangent = float3x3(tangentWorld, bitangentWorld, normalWorld);

            // AudioLink Init
            half al_bass = 0, al_highmid = 0, al_treble = 0, al_amp = 0, al_chrono = _Time.y;
            float4 al_color = _EmissionColor;
            #if defined(AL_ENABLE)
                if (AudioLinkIsAvailable()) {
                    al_bass = AudioLinkData(ALPASS_AUDIOLINK + int2(0, 0)).r;
                    al_highmid = AudioLinkData(ALPASS_AUDIOLINK + int2(2, 0)).r;
                    al_treble = AudioLinkData(ALPASS_AUDIOLINK + int2(3, 0)).r;
                    int band = (int)_AL_EmissionBand;
                    al_amp = AudioLinkData(ALPASS_AUDIOLINK + int2(band, 0)).r;
                    al_color = AudioLinkData(ALPASS_CCCOLORS + int2(band, 0));
                    al_chrono = AudioLinkData(ALPASS_CHRONOTENSITY + int2(band, 0)).r;
                }
            #endif

            float2 baseUV = IN.uv_MainTex;
            baseUV += float2(_SpeedX, _SpeedY) * _Time.y;
            
            float uvRad = _UV_Rot * (UNITY_PI / 180.0);
            float uvS = sin(uvRad);
            float uvC = cos(uvRad);
            baseUV = mul(baseUV - 0.5, float2x2(uvC, -uvS, uvS, uvC)) + 0.5;

            float2 glitch = 0;
            if (al_treble > 0.5) glitch.x = step(0.9, frac(sin(dot(baseUV.y + al_chrono, 12.9898)) * 43758.5453)) * al_treble * _AL_Glitch_Mod;
            
            float currentP = _Parallax + (al_bass * _AL_Paralx_Mod);
            float2 finalUV = ParallaxRaymarching(baseUV + glitch, normalize(IN.viewDir), currentP);
            
            o.UV = finalUV; 
            o.ParallaxDepth = currentP;

            fixed4 c = tex2D(_MainTex, finalUV) * _Color;
            clip(c.a - _CutOff);
            o.Albedo = c.rgb; o.Alpha = c.a;

            float4 pbr = tex2D(_MetallicGlossMap, finalUV);
            o.Metallic = pbr.r; 
            o.Occlusion = lerp(1.0, pbr.g, _AO_Str);
            o.Height = pbr.b; 
            o.Smoothness = pbr.a; 
            o.Thickness = saturate(1.0 - pbr.b);
            
            o.ClearcoatStrength = _CC_Strength;
            o.ClearcoatSmoothness = saturate(_CC_Smoothness - (al_highmid * _AL_CC_Shatter));
            o.ThinFilmThickness = _Film_Thick + (al_bass * _AL_Film_Mod);
            o.ThinFilmStrength = _Film_Str;

            float facing = IN.facing > 0.5 ? 1.0 : -1.0;
            float3 nMain = UnpackScaleNormal(tex2D(_BumpMap, finalUV), _Norm_Str);
            #if defined(_DETAIL_NORMAL)
                float3 nDet = UnpackScaleNormal(tex2D(_DetailNormalMap, finalUV * _Det_UV_Tiling), _Det_Strength);
                nMain = normalize(float3(nMain.xy + nDet.xy, nMain.z * nDet.z));
            #endif
            nMain.z *= facing;
            o.Normal = nMain;
            
            float3 v_ddx = ddx(o.Normal); float3 v_ddy = ddy(o.Normal);
            float var = _CC_Spec_AA * (dot(v_ddx, v_ddx) + dot(v_ddy, v_ddy));
            o.BaseRoughness = PerceptualRoughnessToRoughness(SmoothnessToPerceptualRoughness(o.Smoothness));
            o.BaseRoughness = lerp(o.BaseRoughness, 1.0, saturate(var * 10.0));

            o.ClearcoatNormal = WorldNormalVector(IN, normalize(float3(o.Normal.xy * _CC_Flat, o.Normal.z)));
            o.LTCGINormal = o.ClearcoatNormal;

            float3 viewNormal = mul((float3x3)UNITY_MATRIX_V, o.ClearcoatNormal);
            float mcRad = _MatCap_Rot * (UNITY_PI / 180.0);
            float mcS = sin(mcRad);
            float mcC = cos(mcRad);
            viewNormal.xy = mul(viewNormal.xy, float2x2(mcC, -mcS, mcS, mcC));
            
            o.Matcap = tex2D(_MatCap, viewNormal.xy * 0.5 + 0.5).rgb * _MatCap_Int * tex2D(_MatCapMask, finalUV).r;
            
            float4 em = tex2D(_EmissionMap, finalUV);
            o.NeonColor = lerp(_EmissionColor.rgb, al_color.rgb, _AL_Col_Blend);
            float scan = smoothstep(0.4, 0.6, sin(finalUV.y * _AL_Scanlines - al_chrono * _AL_Scan_Speed) * 0.5 + 0.5);
            o.Emission = em.rgb * o.NeonColor * em.a * (1.0 + al_amp * _AL_Emis_Mod) * (_AL_Scanlines > 0.1 ? scan : 1.0) * _Emis_Exp;
        }
        ENDCG
    }
    FallBack "Standard"
}