Shader "Universal Render Pipeline/Toon Lit"
{
    // TODO: Test "real" numeric type from ShaderLibrary.
    Properties
    {
        [Toggle(_NORMALMAP)]_UseNormalMap           ("Use Normal Map", Float)               = 0
                            _SpecularHighlighting   ("Specular Highlighting", Float)        = 1
                            _RimLighting            ("Rim Lighting", Float)                 = 1
                            _UseRampArray           ("Use Ramp Array", Float)               = 0
                            _UseAlphaClipping       ("Alpha Clipping", Float)               = 0
        [MainTexture]       _BaseMap                ("Base Map", 2D)                        = "white" {}
                            _DiffuseRampIndex       ("Diffuse Ramp Index", Int)             = 0
        [MainColor]         _BaseMapTint            ("Base Tint", Color)                    = (1, 1, 1, 1)
                            _AlphaClipThreshold     ("Alpha Clip Threshold", Range(0, 1))   = 0
                            _SpecularMap            ("Specular Map", 2D)                    = "white" {}
                            _SpecularRampIndex      ("Specular Ramp Index", Int)            = 1
                            _Smoothness             ("Smoothness", Range(0, 1))             = 0.5
                            _SpecularOpacity        ("Specular Opacity", Range(0, 1))       = 1
                            _SpecularBrightening    ("Specular Brightening", Range(0, 1))   = 0.1
        [Normal]            _NormalMap              ("Normal Map", 2D)                      = "bump" {}
        [NoScaleOffset]     _RampAtlas              ("Ramp Atlas", 2D)                      = "grey" {}
        [NoScaleOffset]     _RampArray              ("Ramp Atlas", 2DArray)                 = "" {}
                            _RimRampIndex           ("Rim Ramp Index", Int)                 = 2
        [PowerSlider(4)]    _RimFactor              ("Rim Factor", Range(0.2, 3))           = 0.5
                            _RimLightOpacity        ("Rim Light Opacity", Range(0, 1))      = 0.5
                            _RimBrightening         ("Rim Brightening", Range(0, 1))        = 0.1
                            _RampRowHeight          ("Ramp Row Height", Int)                = 3
                            
        // God this hurts to look at
        [HideInInspector]   _rCurvePt0_3            ("", Vector)                            = (0, 1, 0, 0)
        [HideInInspector]   _rCurvePt4_7            ("", Vector)                            = (0, 0, 0, 0)
        [HideInInspector]   _rCurvePt8_11           ("", Vector)                            = (0, 0, 0, 0)
        [HideInInspector]   _rCurvePt12_15          ("", Vector)                            = (0, 0, 0, 0)
        [HideInInspector]   _rCurvePt16_19          ("", Vector)                            = (0, 0, 0, 0)
        [HideInInspector]   _rCurvePt20_23          ("", Vector)                            = (0, 0, 0, 0)
        [HideInInspector]   _rCurvePt24_27          ("", Vector)                            = (0, 0, 0, 0)
        [HideInInspector]   _rCurvePt28_31          ("", Vector)                            = (0, 0, 0, 0)

        [HideInInspector]   _rCurveVl0_3            ("", Vector)                            = (1, 0, 0, 0)
        [HideInInspector]   _rCurveVl4_7            ("", Vector)                            = (0, 0, 0, 0)
        [HideInInspector]   _rCurveVl8_11           ("", Vector)                            = (0, 0, 0, 0)
        [HideInInspector]   _rCurveVl12_15          ("", Vector)                            = (0, 0, 0, 0)
        [HideInInspector]   _rCurveVl16_19          ("", Vector)                            = (0, 0, 0, 0)
        [HideInInspector]   _rCurveVl20_23          ("", Vector)                            = (0, 0, 0, 0)
        [HideInInspector]   _rCurveVl24_27          ("", Vector)                            = (0, 0, 0, 0)
        [HideInInspector]   _rCurveVl28_31          ("", Vector)                            = (0, 0, 0, 0)
        
        [HideInInspector]   _rCurveStepCount        ("", Int)                               = 2

    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque"
            "Queue"="AlphaTest"
            "RenderPipeline"="UniversalPipeline"
        }
        LOD 300

        Cull Back
        ZTest LEqual
        ZWrite On
        Blend One Zero

        HLSLINCLUDE

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        #if !defined(SM_POS_NORM_HALF) && defined(HAS_HALF)
            #define SM_POS_NORM_HALF exp2(-13)
        #endif

        #ifndef SM_POS_NORM_FLOAT
            #define SM_POS_NORM_FLOAT exp2(-125)
        #endif

        #pragma prefer_hlslcc gles
        #pragma exclude_renderers d3d11_9x gles
        #pragma target 4.5

        TEXTURE2D(_BaseMap);            SAMPLER(sampler_BaseMap);
        TEXTURE2D(_NormalMap);          SAMPLER(sampler_NormalMap);
        TEXTURE2D(_SpecularMap);        SAMPLER(sampler_SpecularMap);
        TEXTURE2D(_RampAtlas);          SAMPLER(sampler_RampAtlas);
        TEXTURE2D_ARRAY(_RampArray);    SAMPLER(sampler_RampArray);

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            float2 _RampAtlas_TexelSize;
            int _DiffuseRampIndex;
            int _SpecularRampIndex;
            int _RimRampIndex;
            int _RampRowHeight;
            float4 _BaseMapTint;
            float _Smoothness;
            float _RimFactor;
            float _AlphaClipThreshold;
            float _RimLightOpacity;
            float _SpecularOpacity;
            float _UseNormalMap;
            float _SpecularHighlighting;
            float _RimLighting;
            float _UseRampArray;
            float _UseAlphaClipping;
            float _SpecularBrightening;
            float _RimBrightening;


            float4 _rCurvePt0_3;
            float4 _rCurvePt4_7;
            float4 _rCurvePt8_11;
            float4 _rCurvePt12_15;
            float4 _rCurvePt16_19;
            float4 _rCurvePt20_23;
            float4 _rCurvePt24_27;
            float4 _rCurvePt28_31;

            float4 _rCurveVl0_3;
            float4 _rCurveVl4_7;
            float4 _rCurveVl8_11;
            float4 _rCurveVl12_15;
            float4 _rCurveVl16_19;
            float4 _rCurveVl20_23;
            float4 _rCurveVl24_27;
            float4 _rCurveVl28_31;

            int _rCurveStepCount;
            
        CBUFFER_END

        half AlphaClipTest(half alpha)
        {
            if (_UseAlphaClipping > 0.5)
            {
                clip(alpha - _AlphaClipThreshold);
            }
            return alpha;
        }

        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags
            {
                "LightMode"="UniversalForward"
            }

            HLSLPROGRAM

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _EMISSION

            // make fog work
            #pragma multi_compile_fog

            #pragma vertex vert
            #pragma fragment frag

            static const int _rCurveMaxSteps = 32;

            struct Attributes
            {
                float3 positionOS       : POSITION;
                float3 normalOS         : NORMAL;
                float4 tangentOS        : TANGENT;
                float2 uv               : TEXCOORD0;
                float2 lightmapUV       : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionCS       : SV_POSITION;
                float2 uv               : TEXCOORD0;
                float4 positionWSwFog   : TEXCOORD1;
                DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 2);

            #ifdef _NORMALMAP
                float4 normal           : TEXCOORD3;    // xyz: normal, w: viewDir.x
                float4 tangent          : TEXCOORD4;    // xyz: tangent, w: viewDir.y
                float4 bitangent        : TEXCOORD5;    // xyz: bitangent, w: viewDir.z
            #else
                float3 normal           : TEXCOORD3;
                float3 viewDirection    : TEXCOORD4;
            #endif
            };

            float4 remap(float4 inValue, float2 inRange, float2 outRange)
            {
                float4 outValue = outRange.x + (inValue - inRange.x) * (outRange.y - outRange.x) / (inRange.y - inRange.x);
                return outValue;
            }
            float3 remap(float3 inValue, float2 inRange, float2 outRange)
            {
                float3 outValue = outRange.x + (inValue - inRange.x) * (outRange.y - outRange.x) / (inRange.y - inRange.x);
                return outValue;
            }
            float2 remap(float2 inValue, float2 inRange, float2 outRange)
            {
                float2 outValue = outRange.x + (inValue - inRange.x) * (outRange.y - outRange.x) / (inRange.y - inRange.x);
                return outValue;
            }
            float remap(float inValue, float2 inRange, float2 outRange)
            {
                float outValue = outRange.x + (inValue - inRange.x) * (outRange.y - outRange.x) / (inRange.y - inRange.x);
                return outValue;
            }

            float4 multilerp(float points[_rCurveMaxSteps], float4 values[_rCurveMaxSteps], float time, int stops)
            {
                if (time <= points[0]) { return values[0]; }
                if (time >= points[stops - 1]) { return values[stops - 1]; }

                int end = 1;
                while (points[end] < time) { end++; }

                float s = (time - points[end - 1])/(points[end] - points[end - 1]);
                return lerp(values[end - 1], values[end], s);
            }
            float3 multilerp(float points[_rCurveMaxSteps], float3 values[_rCurveMaxSteps], float time, int stops)
            {
                if (time <= points[0]) { return values[0]; }
                if (time >= points[stops - 1]) { return values[stops - 1]; }

                int end = 1;
                while (points[end] < time) { end++; }

                float s = (time - points[end - 1])/(points[end] - points[end - 1]);
                return lerp(values[end - 1], values[end], s);
            }
            float2 multilerp(float points[_rCurveMaxSteps], float2 values[_rCurveMaxSteps], float time, int stops)
            {
                if (time <= points[0]) { return values[0]; }
                if (time >= points[stops - 1]) { return values[stops - 1]; }

                int end = 1;
                while (points[end] < time) { end++; }

                float s = (time - points[end - 1])/(points[end] - points[end - 1]);
                return lerp(values[end - 1], values[end], s);
            }
            float multilerp(float points[_rCurveMaxSteps], float values[_rCurveMaxSteps], float time, int stops)
            {
                if (time <= points[0]) { return values[0]; }
                if (time >= points[stops - 1]) { return values[stops - 1]; }

                int end = 1;
                while (points[end] < time) { end++; }

                float s = (time - points[end - 1])/(points[end] - points[end - 1]);
                return lerp(values[end - 1], values[end], s);
            }

            float4 ScreenBlend (float4 base, float4 blend, float opacity)
            {
                float4 result = clamp((1 - (1 - base) * (1 - blend)), float4(0,0,0,0), float4(1,1,1,1));
                return lerp(base, result, opacity);
            }

            float4 OverlayBlend(float4 base, float4 blend, float4 opacity)
            {
                float isLessOrEqual = step(base, .5);
                float4 outValue = lerp(2 * blend * base, 1 - (1 - 2 * (base - .5)) * (1 - blend), isLessOrEqual);
                outValue.a = 1.0;
                return lerp(base, outValue, opacity);
            }

            half3 SampleNormal(float2 uv, TEXTURE2D_PARAM(bumpMap, sampler_bumpMap))
            {
            #ifdef _NORMALMAP
                half4 n = SAMPLE_TEXTURE2D(bumpMap, sampler_bumpMap, uv);
                return UnpackNormal(n);
            #else
                return half3(0.0h, 0.0h, 1.0h);
            #endif
            }

            float ConvertSmoothness(float smoothness)
            {
                return pow(2, smoothness * 10 + 1);
            }

            float LambertianDiffuseAttenuation(float3 lightDir, float3 normal, half distanceAtten, half shadowAtten)
            {
                float NdotL = dot(lightDir, normal);
                return NdotL * distanceAtten * shadowAtten;
            }

            float BlinnPhongSpecularAttenuation(float3 lightDir, float3 viewDir, float3 normalWS)
            {
                float smoothness = ConvertSmoothness(_Smoothness);
                float3 H = normalize(lightDir + viewDir);
                float HdotN = saturate(dot(H, normalWS));
                return pow(HdotN, smoothness);
            }

            half4 ToonRamping(float atten, int index)
            {
                index += 1;
                if (_UseRampArray > 0.5)
                {
                    return SAMPLE_TEXTURE2D_ARRAY(_RampArray, sampler_RampArray, float2(atten, 0), index);
                }
                else
                {
                    return SAMPLE_TEXTURE2D(_RampAtlas, sampler_RampAtlas, 
                        float2(atten, _RampAtlas_TexelSize.y * (index * 3 - (3 / 2))));
                }
            }

            void CalculateLighting(Light light, half4 fragColor, half4 specularSample, half3 normal, half3 viewDir,
                                   half4 ambient, inout half4 diffuse, inout half4 specular, inout half4 rim)
            {
            #if REAL_IS_HALF == 1
                light.distanceAttenuation = step(SM_POS_NORM_HALF, light.distanceAttenuation);
            #else
                light.distanceAttenuation = step(SM_POS_NORM_FLOAT, light.distanceAttenuation);
            #endif
                half4 lightColor = half4(light.color, 1);
                half4 brightenedSpecularColor = saturate(lightColor + ((1 - lightColor) * _SpecularBrightening));
                half4 brightenedRimColor = saturate(lightColor + ((1 - lightColor) * _RimBrightening));

                normal = SafeNormalize(normal);
                float3 lightDir = SafeNormalize(light.direction);
                viewDir = SafeNormalize(viewDir);

                // Diffuse attenuation
                float diffuseAtten = LambertianDiffuseAttenuation(lightDir, 
                    normal, light.distanceAttenuation, light.shadowAttenuation);
                diffuseAtten = remap(diffuseAtten, float2(-1, 1), float2(0, 1));
                half4 toonDiffuseAtten = ToonRamping(diffuseAtten, _DiffuseRampIndex);

                diffuse += toonDiffuseAtten * lightColor;

                if (_SpecularHighlighting > 0.5)// && _SpecularOpacity > 0)
                {
                    // Specular attenuation
                    float specularAtten = BlinnPhongSpecularAttenuation(lightDir, viewDir, normal);
                    specularAtten = remap(specularAtten, float2(-1, 1), float2(0, 1));
                    half4 toonSpecularAtten = ToonRamping(specularAtten, _SpecularRampIndex);
                    specular += toonSpecularAtten 
                        * toonDiffuseAtten
                        * specularSample
                        * ScreenBlend(lightColor, brightenedSpecularColor, toonSpecularAtten);
                }
                
                if (_RimLighting > 0.5 && _RimLightOpacity > 0)
                {
                    // Why must things be the way that they are?
                    float points[_rCurveMaxSteps] = { 
                        _rCurvePt0_3.x, _rCurvePt0_3.y, _rCurvePt0_3.z, _rCurvePt0_3.w,
                        _rCurvePt4_7.x, _rCurvePt4_7.y, _rCurvePt4_7.z, _rCurvePt4_7.w ,
                        _rCurvePt8_11.x, _rCurvePt8_11.y, _rCurvePt8_11.z, _rCurvePt8_11.w,
                        _rCurvePt12_15.x, _rCurvePt12_15.y, _rCurvePt12_15.z, _rCurvePt12_15.w,
                        _rCurvePt16_19.x, _rCurvePt16_19.y, _rCurvePt16_19.z, _rCurvePt16_19.w,
                        _rCurvePt20_23.x, _rCurvePt20_23.y, _rCurvePt20_23.z, _rCurvePt20_23.w,
                        _rCurvePt24_27.x, _rCurvePt24_27.y, _rCurvePt24_27.z, _rCurvePt24_27.w,
                        _rCurvePt28_31.x, _rCurvePt28_31.y, _rCurvePt28_31.z, _rCurvePt28_31.w, };

                    float values[_rCurveMaxSteps] = { 
                        _rCurveVl0_3.x, _rCurveVl0_3.y, _rCurveVl0_3.z, _rCurveVl0_3.w,
                        _rCurveVl4_7.x, _rCurveVl4_7.y, _rCurveVl4_7.z, _rCurveVl4_7.w ,
                        _rCurveVl8_11.x, _rCurveVl8_11.y, _rCurveVl8_11.z, _rCurveVl8_11.w,
                        _rCurveVl12_15.x, _rCurveVl12_15.y, _rCurveVl12_15.z, _rCurveVl12_15.w,
                        _rCurveVl16_19.x, _rCurveVl16_19.y, _rCurveVl16_19.z, _rCurveVl16_19.w,
                        _rCurveVl20_23.x, _rCurveVl20_23.y, _rCurveVl20_23.z, _rCurveVl20_23.w,
                        _rCurveVl24_27.x, _rCurveVl24_27.y, _rCurveVl24_27.z, _rCurveVl24_27.w,
                        _rCurveVl28_31.x, _rCurveVl28_31.y, _rCurveVl28_31.z, _rCurveVl28_31.w, };

                    float fresnel = pow(1 - saturate(dot(normal, viewDir)), _RimFactor);

                    // Use curve to mask fresnel effect
                    float attenuatedFresnel = fresnel * multilerp(points, values, saturate(dot(lightDir, viewDir)), _rCurveStepCount);

                    float rimlighting = ToonRamping(attenuatedFresnel * toonDiffuseAtten, _RimRampIndex);
                    rim = ScreenBlend(rim, (rimlighting * (brightenedRimColor)), 1);
                }
            }

            //===== Vertex Function =====//

            Varyings vert (Attributes IN)
            {
                Varyings OUT;

                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS);

                // Fog data packed into world-space position
                OUT.positionWSwFog = float4(positionInputs.positionWS, ComputeFogFactor(positionInputs.positionCS.z));
                OUT.positionCS = positionInputs.positionCS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);

                half3 viewDirWS = GetWorldSpaceViewDir(positionInputs.positionWS);

            #ifdef _NORMALMAP
                OUT.normal = half4(normalInputs.normalWS, viewDirWS.x);
                OUT.tangent = half4(normalInputs.tangentWS, viewDirWS.y);
                OUT.bitangent = half4(normalInputs.bitangentWS, viewDirWS.z);
            #else
                OUT.normal = NormalizeNormalPerVertex(normalInputs.normalWS);
                OUT.viewDirection = viewDirWS;
            #endif

                // One of the following expands to "" depending on keywords
                OUTPUT_LIGHTMAP_UV(IN.lightmapUV, unity_LightmapST, IN.lightmapUV);
                OUTPUT_SH(OUT.normal.xyz, OUT.vertexSH);

                return OUT;
            }


            //====== Fragment Function =====//

            float4 frag (Varyings IN) : SV_TARGET
            {
                // Albedo color
                half4 baseFragColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseMapTint;

                // Alpha clip
                if (_UseAlphaClipping > 0.5)
                {
                    clip(baseFragColor.a - _AlphaClipThreshold);
                }

                // Fragment normal
                half3 normalTS = SampleNormal(IN.uv, TEXTURE2D_ARGS(_NormalMap, sampler_NormalMap));
            #ifdef _NORMALMAP
                half3 viewDirWS = half3(IN.normal.w, IN.tangent.w, IN.bitangent.w);
                IN.normal = float4(TransformTangentToWorld(normalTS,
                    half3x3(IN.tangent.xyz, IN.bitangent.xyz, IN.normal.xyz)), 0);
            #else
                half3 viewDirWS = IN.viewDirection;
            #endif
                half3 fragNormal = IN.normal.xyz;

                half4 Diffuse = 0;
                half4 Specular = 0;
                half4 Rim = 0;

                // Specular sample
                half4 specularSample = SAMPLE_TEXTURE2D(_SpecularMap, sampler_SpecularMap, IN.uv);

                // Main light
                float4 shadowCoord = TransformWorldToShadowCoord(IN.positionWSwFog.xyz);
                Light mainLight = GetMainLight(shadowCoord);
                
                // Ambient lighting
                half4 ambient = half4(SAMPLE_GI(IN.lightmapUV, IN.vertexSH, IN.normal), 1);
                MixRealtimeAndBakedGI(mainLight, IN.normal, ambient.rgb);
                Diffuse += ambient;

                CalculateLighting(mainLight, baseFragColor, specularSample, fragNormal, viewDirWS, ambient, Diffuse, Specular, Rim);

                #ifdef _ADDITIONAL_LIGHTS
                    uint pixelLightCount = GetAdditionalLightsCount();
                    for (uint lightIndex = 0U; lightIndex < pixelLightCount; ++lightIndex)
                    {
                        int perObjectLightIndex = GetPerObjectLightIndex(lightIndex);

                        // Additional light
                        Light light = GetAdditionalPerObjectLight(perObjectLightIndex, IN.positionWSwFog.xyz);
                        #ifdef _ADDITIONAL_LIGHT_SHADOWS
                            light.shadowAttenuation = AdditionalLightRealtimeShadow(perObjectLightIndex, IN.positionWSwFog.xyz);
                        #endif
                        CalculateLighting(light, baseFragColor, specularSample, fragNormal, viewDirWS, ambient, Diffuse, Specular, Rim);
                    }
                #endif

                half4 outColor = Diffuse * baseFragColor;
                outColor = ScreenBlend(outColor, Rim, _RimLightOpacity);
                outColor = ScreenBlend(outColor, Specular, _SpecularOpacity);
                return outColor;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

            ColorMask 0

            HLSLPROGRAM
            #pragma vertex shadowVert
            #pragma fragment shadowFrag

            float3 _LightDirection;

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 texcoord     : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv           : TEXCOORD0;
                float4 positionCS   : SV_POSITION;
            };

            Varyings shadowVert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS);
                
                OUT.uv = TRANSFORM_TEX(IN.texcoord, _BaseMap);
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionInputs.positionWS,
                    normalInputs.normalWS, _LightDirection));

                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #endif
                
                OUT.positionCS = positionCS;
                return OUT;
            }

            half4 shadowFrag(Varyings IN) : SV_TARGET
            {
                half alpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv).a;
                AlphaClipTest(alpha);
                return 0;
            }

            ENDHLSL
        }
        
        Pass
        {
            Tags { "LightMode"="DepthOnly" }

            ColorMask 0

            HLSLPROGRAM
            #pragma vertex depthVert
            #pragma fragment depthFrag

            struct Attributes
            {
                float4 position   : POSITION;
                float2 texcoord     : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv           : TEXCOORD0;
                float4 positionCS   : SV_POSITION;
            };

            Varyings depthVert(Attributes IN)
            {
                Varyings OUT;
                OUT.uv = TRANSFORM_TEX(IN.texcoord, _BaseMap);
                OUT.positionCS = TransformObjectToHClip(IN.position.xyz);
                return OUT;
            }

            half4 depthFrag(Varyings IN) : SV_TARGET
            {
                half alpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv).a;
                AlphaClipTest(alpha);
                return 0;
            }

            ENDHLSL
        }
    }
    CustomEditor "ToonShaderURP.ToonLitShaderGUI"
}
