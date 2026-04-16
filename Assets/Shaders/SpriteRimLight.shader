Shader "Custom/SpriteRimLight"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        [NoScaleOffset] _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalStrength ("Normal Strength", Range(0, 2)) = 1.0
        _Color ("Tint", Color) = (1,1,1,1)

        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth ("Outline Width", Range(0, 4)) = 1

        _RimColor ("Rim Color", Color) = (0.6, 0.9, 1.0, 1.0)
        _RimPower ("Rim Power", Range(0.1, 8)) = 2.5
        _RimIntensity ("Rim Intensity", Range(0, 3)) = 1.0

        _PulseIntensity ("Pulse Intensity", Range(0, 2)) = 0.0
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 2.0

        _SparkleIntensity ("Sparkle Intensity", Range(0, 2)) = 0.0
        _SparkleThreshold ("Sparkle Threshold", Range(0, 1)) = 0.95
        _SparkleSpeed ("Sparkle Speed", Range(0, 10)) = 2.0

        _FlowIntensity ("Flow Intensity", Range(0, 2)) = 0.0
        _FlowSpeedX ("Flow Speed X", Range(-5, 5)) = 0.5
        _FlowSpeedY ("Flow Speed Y", Range(-5, 5)) = 0.0

        _LightDir ("Fake Light Direction", Vector) = (1, 1, 0, 0)

        _RevealProgress ("Reveal Progress", Range(0, 1)) = 1.0
        _RevealSoftness ("Reveal Softness", Range(0.001, 0.5)) = 0.05
        _RevealEdgeWidth ("Reveal Edge Width", Range(0.001, 0.5)) = 0.08
        _RevealEdgeColor ("Reveal Edge Color", Color) = (0.6, 0.9, 1.0, 1.0)
        _RevealEdgeIntensity ("Reveal Edge Intensity", Range(0, 5)) = 1.5
        _RevealNoiseStrength ("Reveal Noise Strength", Range(0, 1)) = 0.15

        _DeathProgress ("Death Progress", Range(0, 1)) = 0.0
        _DeathSoftness ("Death Softness", Range(0.001, 0.5)) = 0.06
        _DeathEdgeWidth ("Death Edge Width", Range(0.001, 0.5)) = 0.1
        _DeathEdgeColor ("Death Edge Color", Color) = (0.75, 0.75, 0.75, 1.0)
        _DeathEdgeIntensity ("Death Edge Intensity", Range(0, 5)) = 1.4
        _DeathNoiseStrength ("Death Noise Strength", Range(0, 1)) = 0.25
        _DeathDriftY ("Death Drift Y", Range(0, 2)) = 0.35

        _AlphaClip ("Alpha Clip", Range(0,1)) = 0.01
        [MaterialToggle] _UsePixelSnap ("Use Pixel Snap", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"
            #include "UnityStandardUtils.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 uv       : TEXCOORD0;
                float2 screenUV : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _NormalMap;
            float _NormalStrength;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _OutlineColor;
            float _OutlineWidth;

            fixed4 _RimColor;
            float _RimPower;
            float _RimIntensity;

            float _PulseIntensity;
            float _PulseSpeed;

            float _SparkleIntensity;
            float _SparkleThreshold;
            float _SparkleSpeed;

            float _FlowIntensity;
            float _FlowSpeedX;
            float _FlowSpeedY;

            float4 _LightDir;
            float _RevealProgress;
            float _RevealSoftness;
            float _RevealEdgeWidth;
            fixed4 _RevealEdgeColor;
            float _RevealEdgeIntensity;
            float _RevealNoiseStrength;

            float _DeathProgress;
            float _DeathSoftness;
            float _DeathEdgeWidth;
            fixed4 _DeathEdgeColor;
            float _DeathEdgeIntensity;
            float _DeathNoiseStrength;
            float _DeathDriftY;

            float _AlphaClip;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.uv = IN.texcoord;
                OUT.color = IN.color * _Color;

                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif

                float4 screenPos = ComputeScreenPos(OUT.vertex);
                OUT.screenUV = screenPos.xy / screenPos.w;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 baseCol = tex2D(_MainTex, IN.uv) * IN.color;

                float2 texel = _MainTex_TexelSize.xy * _OutlineWidth;
                float aL = tex2D(_MainTex, IN.uv + float2(-texel.x, 0)).a;
                float aR = tex2D(_MainTex, IN.uv + float2( texel.x, 0)).a;
                float aD = tex2D(_MainTex, IN.uv + float2(0, -texel.y)).a;
                float aU = tex2D(_MainTex, IN.uv + float2(0,  texel.y)).a;
                float aUL = tex2D(_MainTex, IN.uv + float2(-texel.x,  texel.y)).a;
                float aUR = tex2D(_MainTex, IN.uv + float2( texel.x,  texel.y)).a;
                float aDL = tex2D(_MainTex, IN.uv + float2(-texel.x, -texel.y)).a;
                float aDR = tex2D(_MainTex, IN.uv + float2( texel.x, -texel.y)).a;

                float neighborAlpha = max(max(max(aL, aR), max(aD, aU)), max(max(aUL, aUR), max(aDL, aDR)));
                bool isOutline = baseCol.a <= _AlphaClip && neighborAlpha > _AlphaClip;

                if (isOutline)
                {
                    return fixed4(_OutlineColor.rgb, _OutlineColor.a * neighborAlpha);
                }

                float timeY = _Time.y;

                float revealNoise = frac(sin(dot((IN.uv + timeY * 0.15) * 53.0, float2(18.371, 42.117))) * 14375.8543);
                float revealThreshold = saturate(IN.uv.y + (revealNoise - 0.5) * _RevealNoiseStrength);
                float revealBody = 1.0 - smoothstep(_RevealProgress - _RevealSoftness, _RevealProgress + _RevealSoftness, revealThreshold);
                float revealEdgeMask = smoothstep(_RevealProgress - _RevealEdgeWidth, _RevealProgress, revealThreshold)
                                     * (1.0 - smoothstep(_RevealProgress, _RevealProgress + _RevealEdgeWidth, revealThreshold));

                float2 deathNoiseUV = IN.uv + float2(0.0, timeY * _DeathDriftY);
                float deathNoise = frac(sin(dot(deathNoiseUV * 61.0, float2(23.417, 51.913))) * 19642.3491);
                float deathThreshold = saturate(deathNoise + IN.uv.y * 0.35 + (deathNoise - 0.5) * _DeathNoiseStrength);
                float deathBody = 1.0 - smoothstep(_DeathProgress - _DeathSoftness, _DeathProgress + _DeathSoftness, deathThreshold);
                float deathEdgeMask = smoothstep(_DeathProgress - _DeathEdgeWidth, _DeathProgress, deathThreshold)
                                    * (1.0 - smoothstep(_DeathProgress, _DeathProgress + _DeathEdgeWidth, deathThreshold));

                baseCol.a *= revealBody;
                baseCol.a *= deathBody;
                clip(baseCol.a - _AlphaClip);

                float3 sampledNormal = UnpackNormal(tex2D(_NormalMap, IN.uv));
                sampledNormal.xy *= _NormalStrength;
                sampledNormal = normalize(sampledNormal);

                float3 lightDir3 = normalize(float3(_LightDir.x, _LightDir.y, 1.0));
                float ndl = saturate(dot(sampledNormal, lightDir3));
                float rim = pow(1.0 - ndl, _RimPower) * _RimIntensity;

                float pulse = (sin(timeY * _PulseSpeed) * 0.5 + 0.5) * _PulseIntensity;

                float2 flowUV = IN.uv + float2(_FlowSpeedX, _FlowSpeedY) * timeY;
                float flowNoise = frac(sin(dot(flowUV * 37.0, float2(12.9898, 78.233))) * 43758.5453);
                float flow = flowNoise * _FlowIntensity;

                float sparkleNoise = frac(sin(dot((IN.uv + timeY * _SparkleSpeed) * 91.7, float2(15.123, 47.321))) * 24634.6345);
                float sparkle = step(_SparkleThreshold, sparkleNoise) * _SparkleIntensity * baseCol.a;

                fixed3 finalRgb = baseCol.rgb;
                finalRgb += _RimColor.rgb * rim * baseCol.a;
                finalRgb += _RimColor.rgb * pulse * baseCol.a;
                finalRgb += _RimColor.rgb * flow * baseCol.a;
                finalRgb += sparkle;
                finalRgb += _RevealEdgeColor.rgb * revealEdgeMask * _RevealEdgeIntensity;
                finalRgb += _DeathEdgeColor.rgb * deathEdgeMask * _DeathEdgeIntensity;

                return fixed4(finalRgb, baseCol.a);
            }
            ENDCG
        }
    }
    // Quick test ideas:
    // 1) Soft pulse glow:
    //    _PulseIntensity = 0.35, _PulseSpeed = 2.0
    // 2) Magic sparkle:
    //    _SparkleIntensity = 0.6, _SparkleThreshold = 0.97, _SparkleSpeed = 3.0
    // 3) Energy flow:
    //    _FlowIntensity = 0.25, _FlowSpeedX = 0.8, _FlowSpeedY = 0.0
    // 4) Strong modern magic look:
    //    _RimIntensity = 1.2, _PulseIntensity = 0.4, _SparkleIntensity = 0.35, _FlowIntensity = 0.2
    // 5) Death dissolve smoky fade:
    //    _DeathProgress = 0 -> 1 over time
    //    _DeathSoftness = 0.06
    //    _DeathEdgeWidth = 0.1
    //    _DeathEdgeIntensity = 1.4
    //    _DeathNoiseStrength = 0.25
    //    _DeathDriftY = 0.35
    // 6) Reveal + Edge Glow spawn look:
    //    _RevealProgress = 0 -> 1 over time
    //    _RevealSoftness = 0.04
    //    _RevealEdgeWidth = 0.06
    //    _RevealEdgeIntensity = 1.8
    //    _RevealNoiseStrength = 0.12
}
