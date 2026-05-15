Shader "VixenWear/Latex Ultra"
{
    Properties
    {
        [Header(Base Settings)]
        _Color ("Color", Color) = (1,1,1,1)
        _CutOff ("CutOff", Range(0,1)) = 0.5
        _MainTex ("Albedo (RGB) Cutout (A)", 2D) = "white" {}
        _MinBrightness ("Minimum Brightness", Range(0, 1)) = 0.0

        [HideInInspector] [VectorLabel(UV_Rot, SpeedX, SpeedY, MatCap_Rot)]
        _UVParams ("UV Animation", Vector) = (0, 0, 0, 0)

        [Header(Surface Maps)]
        _MetallicGlossMap ("Packed PBR (R:Met G:AO B:Disp A:Smooth)", 2D) = "white" {}
        [Normal] _BumpMap ("Normal Map", 2D) = "bump" {}

        // Standardized PBR baseline (no blown-out shadows or normals)
        [HideInInspector] [VectorLabel(AO_Str, Spec_Occ, Shad_Hard, Norm_Str)]
        _PBRParams ("PBR Adjustments", Vector) = (1.0, 1.0, 1.0, 1.0)

        // Parallax and Displacement zeroed out for performance
        [HideInInspector] [VectorLabel(Parallax, Disp_Str, Tess_Edge, Emis_Exp)]
        _GeoEmisParams ("Geometry & Bloom", Vector) = (0.0, 0.0, 10.0, 1.0)

        [Header(Clearcoat Polish)]
        // The core latex look: Max strength, high smoothness, no flattening
        [HideInInspector] [VectorLabel(Strength, Smoothness, Spec_AA, CC_Flat)]
        _ClearcoatParams ("Clearcoat Params", Vector) = (1.0, 0.9, 0.0, 0.0)

        // Iridescence and Rim lighting zeroed out by default
        [HideInInspector] [VectorLabel(Film_Str, Film_Thick, Rim_Str, Rim_Power)]
        _FilmRimParams ("Film & Rim Params", Vector) = (0.0, 0.0, 0.0, 4.0)

        [Header(Deferred Translucency)]
        // SSS turned off
        [HideInInspector] [VectorLabel(SSS_Str, SSS_Dist, SSS_Power, Unused)]
        _SSSParams ("Subsurface Params", Vector) = (0.0, 0.1, 4.0, 0.0)

        [Header(Micro Details)]
        [Toggle(_DETAIL_NORMAL)] _UseDetailNormal ("Enable Micro Detail", Float) = 0
        [Normal] _DetailNormalMap ("Micro Detail Map", 2D) = "bump" {}
        
        [HideInInspector] [VectorLabel(Strength, UV_Tiling, Unused, Unused)]
        _DetailParams ("Detail Params", Vector) = (0.0, 1.0, 0, 0)

        [Header(Lighting Integration)]
        [HDR] _EmissionColor ("Emission Color", Color) = (0,0,0,1)
        _EmissionMap ("Emission Map", 2D) = "black" {}

        _MatCap ("MatCap Texture", 2D) = "black" {}
        _MatCapMask ("MatCap Mask", 2D) = "white" {}

        // MatCap intensity zeroed, external light integration at 1.0
        [HideInInspector] [VectorLabel(MatCap_Int, MatCap_Lit, LV_Int, LTCGI_Int)]
        _IntegrationParams ("Integration Params", Vector) = (0.0, 1.0, 1.0, 1.0)

        [Toggle(LIGHTVOLUMES_ENABLE)] _UseLightVolumes ("Enable Light Volumes", Float) = 0
        [Toggle(LTCGI_ENABLE)] _UseLTCGI ("Enable LTCGI", Float) = 0

        [Header(AudioLink)]
        [Toggle(AL_ENABLE)] _UseAudioLink ("Enable AudioLink", Float) = 0
        [Enum(Bass,0,Low Mid,1,High Mid,2,Treble,3)] _AL_EmissionBand ("Emission Band", Float) = 0

        // Modulations entirely zeroed out
        [HideInInspector] [VectorLabel(Emis_Mod, Col_Blend, Scanlines, Scan_Speed)]
        _ALParamsA ("AudioLink Modulation A", Vector) = (0.0, 0.0, 0.0, 0.0)

        [HideInInspector] [VectorLabel(Film_Mod, Paralx_Mod, CC_Shatter, Glitch_Mod)]
        _ALParamsB ("AudioLink Modulation B", Vector) = (0.0, 0.0, 0.0, 0.0)
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

        // Packed Parameters
        float4 _UVParams; 
        float4 _PBRParams; 
        float4 _GeoEmisParams; 
        float4 _ClearcoatParams; 
        float4 _FilmRimParams; 
        float4 _SSSParams; 
        float4 _DetailParams;
        float4 _IntegrationParams;
        float4 _ALParamsA; 
        float4 _ALParamsB;
        half _AL_EmissionBand;

        float4 tessEdge(appdata_full v0, appdata_full v1, appdata_full v2)
        {
            return UnityEdgeLengthBasedTess(v0.vertex, v1.vertex, v2.vertex, _GeoEmisParams.z);
        }

        void disp(inout appdata_full v)
        {
            float d = tex2Dlod(_MetallicGlossMap, float4(v.texcoord.xy,0,0)).b * _GeoEmisParams.y;
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
            half specOcc = lerp(1.0, s.Occlusion, _PBRParams.y);

            float shadowTrace = 1.0;
            if (nl > 0.0) 
            {
                float3 lightDirTangent = mul(s.WorldToTangent, light.dir);
                float2 lightDirUV = lightDirTangent.xy * s.ParallaxDepth; 
                float shadowHeight = tex2Dlod(_MetallicGlossMap, float4(s.UV + lightDirUV, 0, 0)).b;
                shadowTrace = saturate(1.0 - (s.Height - shadowHeight) * _PBRParams.z);
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

            half3 transLightDir = light.dir + s.Normal * _SSSParams.y;
            half transDot = pow(saturate(dot(viewDir, -transLightDir)), _SSSParams.z) * _SSSParams.x;
            half3 sssGlow = transDot * light.color * s.Thickness * s.NeonColor;

            half rim = pow(1.0 - cc_nv, _FilmRimParams.w) * _FilmRimParams.z * s.NeonColor * (gi.diffuse + 0.1);

            half3 finalColor = gi.diffuse * diffColor * energyConservation * s.Occlusion
                             + baseSpecular 
                             + ccSpecular
                             + gi.specular * specColor * energyConservation * specOcc
                             + clearcoatEnv * clearcoatMaskedFresnel * finalClearcoatColor * specOcc
                             + rim + sssGlow;

            half3 matcapEval = matcap * saturate(gi.diffuse + light.color * smoothstep(0.0, 0.15, saturate(dot(s.ClearcoatNormal, light.dir)))) * specOcc;
            finalColor = lerp(finalColor, finalColor + matcapEval, _IntegrationParams.y);

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
                    gi.indirect.diffuse = lerp(gi.indirect.diffuse, LightVolumeEvaluate(s.Normal, lv_L0, lv_L1r, lv_L1g, lv_L1b), _IntegrationParams.z);
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
                c.rgb += (ltc_s * s.ClearcoatStrength + ltc_d * s.Albedo) * _IntegrationParams.w * s.Occlusion;
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
            baseUV += _UVParams.yz * _Time.y;
            
            float uvRad = _UVParams.x * (UNITY_PI / 180.0);
            float uvS = sin(uvRad);
            float uvC = cos(uvRad);
            baseUV = mul(baseUV - 0.5, float2x2(uvC, -uvS, uvS, uvC)) + 0.5;

            float2 glitch = 0;
            if (al_treble > 0.5) glitch.x = step(0.9, frac(sin(dot(baseUV.y + al_chrono, 12.9898)) * 43758.5453)) * al_treble * _ALParamsB.w;
            
            float currentP = _GeoEmisParams.x + (al_bass * _ALParamsB.y);
            float2 finalUV = ParallaxRaymarching(baseUV + glitch, normalize(IN.viewDir), currentP);
            
            o.UV = finalUV; 
            o.ParallaxDepth = currentP;

            fixed4 c = tex2D(_MainTex, finalUV) * _Color;
            clip(c.a - _CutOff);
            o.Albedo = c.rgb; o.Alpha = c.a;

            float4 pbr = tex2D(_MetallicGlossMap, finalUV);
            o.Metallic = pbr.r; 
            o.Occlusion = lerp(1.0, pbr.g, _PBRParams.x);
            o.Height = pbr.b; 
            o.Smoothness = pbr.a; 
            o.Thickness = saturate(1.0 - pbr.b);
            
            o.ClearcoatStrength = _ClearcoatParams.x;
            o.ClearcoatSmoothness = saturate(_ClearcoatParams.y - (al_highmid * _ALParamsB.z));
            o.ThinFilmThickness = _FilmRimParams.y + (al_bass * _ALParamsB.x);
            o.ThinFilmStrength = _FilmRimParams.x;

            float facing = IN.facing > 0.5 ? 1.0 : -1.0;
            float3 nMain = UnpackScaleNormal(tex2D(_BumpMap, finalUV), _PBRParams.w);
            #if defined(_DETAIL_NORMAL)
                float3 nDet = UnpackScaleNormal(tex2D(_DetailNormalMap, finalUV * _DetailParams.y), _DetailParams.x);
                nMain = normalize(float3(nMain.xy + nDet.xy, nMain.z * nDet.z));
            #endif
            nMain.z *= facing;
            o.Normal = nMain;
            
            float3 v_ddx = ddx(o.Normal); float3 v_ddy = ddy(o.Normal);
            float var = _ClearcoatParams.z * (dot(v_ddx, v_ddx) + dot(v_ddy, v_ddy));
            o.BaseRoughness = PerceptualRoughnessToRoughness(SmoothnessToPerceptualRoughness(o.Smoothness));
            o.BaseRoughness = lerp(o.BaseRoughness, 1.0, saturate(var * 10.0));

            o.ClearcoatNormal = WorldNormalVector(IN, normalize(float3(o.Normal.xy * _ClearcoatParams.w, o.Normal.z)));
            o.LTCGINormal = o.ClearcoatNormal;

            float3 viewNormal = mul((float3x3)UNITY_MATRIX_V, o.ClearcoatNormal);
            float mcRad = _UVParams.w * (UNITY_PI / 180.0);
            float mcS = sin(mcRad);
            float mcC = cos(mcRad);
            viewNormal.xy = mul(viewNormal.xy, float2x2(mcC, -mcS, mcS, mcC));
            
            o.Matcap = tex2D(_MatCap, viewNormal.xy * 0.5 + 0.5).rgb * _IntegrationParams.x * tex2D(_MatCapMask, finalUV).r;
            
            float4 em = tex2D(_EmissionMap, finalUV);
            o.NeonColor = lerp(_EmissionColor.rgb, al_color.rgb, _ALParamsA.y);
            float scan = smoothstep(0.4, 0.6, sin(finalUV.y * _ALParamsA.z - al_chrono * _ALParamsA.w) * 0.5 + 0.5);
            o.Emission = em.rgb * o.NeonColor * em.a * (1.0 + al_amp * _ALParamsA.x) * (_ALParamsA.z > 0.1 ? scan : 1.0) * _GeoEmisParams.w;
        }
        ENDCG
    }
    FallBack "Standard"
}