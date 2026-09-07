Shader "ProjectBS/CharacterCastProgress"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _NpcCastOutlineEnabled ("NPC Cast Outline", Float) = 0
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width", Float) = 2.09
        _CastEnabled ("Cast Enabled", Float) = 0
        _CastProgress ("Cast Progress", Range(0,1)) = 0
        _CastTime ("Cast Local Time", Float) = 0
        _CastBaseColor ("Cast Base", Color) = (0.525,0.686,0.784,1)
        _CastAccentColor ("Cast Accent", Color) = (0.831,0.416,0.271,1)
        _CastPulseHzMin ("Pulse Min Hz", Float) = 0.8
        _CastPulseHzMax ("Pulse Max Hz", Float) = 1.8
        _CastEdgeIntensityMin ("Edge Min", Float) = 0.08
        _CastEdgeIntensityMax ("Edge Max", Float) = 0.32
        _CastInteriorLiftMax ("Interior Lift Max", Float) = 0.08
        _CastTerminalStart ("Terminal Start", Float) = 0.88
        _CastTerminalIntensity ("Terminal Intensity", Float) = 0.4
        _CastCompleteFlash ("Complete Flash", Float) = 0
        _CastReducedFlash ("Reduced Flash", Float) = 0
        _CastReducedMotion ("Reduced Motion", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Cull Off Lighting Off ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct v2f { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _NpcCastOutlineEnabled, _OutlineWidth;
            fixed4 _OutlineColor;
            fixed4 _Color, _CastBaseColor, _CastAccentColor;
            float _CastEnabled, _CastProgress, _CastTime;
            float _CastPulseHzMin, _CastPulseHzMax;
            float _CastEdgeIntensityMin, _CastEdgeIntensityMax, _CastInteriorLiftMax;
            float _CastTerminalStart, _CastTerminalIntensity, _CastCompleteFlash;
            float _CastReducedFlash, _CastReducedMotion;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv) * i.color;
                if (_NpcCastOutlineEnabled > 0.5 && _CastEnabled > 0.5)
                {
                    float2 stepUV = _MainTex_TexelSize.xy * _OutlineWidth;
                    float neighbor = 0.0;
                    neighbor = max(neighbor, tex2D(_MainTex, i.uv + float2(stepUV.x, 0)).a);
                    neighbor = max(neighbor, tex2D(_MainTex, i.uv - float2(stepUV.x, 0)).a);
                    neighbor = max(neighbor, tex2D(_MainTex, i.uv + float2(0, stepUV.y)).a);
                    neighbor = max(neighbor, tex2D(_MainTex, i.uv - float2(0, stepUV.y)).a);
                    neighbor = max(neighbor, tex2D(_MainTex, i.uv + stepUV).a);
                    neighbor = max(neighbor, tex2D(_MainTex, i.uv - stepUV).a);
                    neighbor = max(neighbor, tex2D(_MainTex, i.uv + float2(stepUV.x, -stepUV.y)).a);
                    neighbor = max(neighbor, tex2D(_MainTex, i.uv + float2(-stepUV.x, stepUV.y)).a);
                    if (tex.a <= 0.01 && neighbor > 0.01)
                    {
                        float alpha = _OutlineColor.a * neighbor * i.color.a;
                        return fixed4(_OutlineColor.rgb * alpha, alpha);
                    }
                }
                float progress = saturate(_CastProgress);
                float eased = smoothstep(0.0, 1.0, progress);
                float hz = _CastReducedMotion > 0.5 ? 0.0 : min(1.99, lerp(_CastPulseHzMin, _CastPulseHzMax, eased));
                float wave = hz <= 0.0 ? 0.5 : 0.5 + 0.5 * sin(_CastTime * 6.2831853 * hz);
                float amplitude = lerp(0.04, 0.14, eased) * (_CastReducedFlash > 0.5 ? 0.5 : 1.0);
                float edgeBase = lerp(_CastEdgeIntensityMin, min(_CastEdgeIntensityMax, 0.32), eased);
                float edge = min(0.38, edgeBase + wave * amplitude);
                float accent = smoothstep(_CastTerminalStart, 1.0, progress);
                fixed3 cue = lerp(_CastBaseColor.rgb, _CastAccentColor.rgb, accent);
                float lift = min(_CastInteriorLiftMax, lerp(0.01, 0.08, eased));
                float terminal = _CastCompleteFlash * min(_CastTerminalIntensity, 0.40);
                float safeCeiling = _CastReducedFlash > 0.5 ? 0.10 : 0.199;
                float localizedLift = min(safeCeiling, edge * 0.22 + lift * 0.35 + terminal * 0.20);
                float enabled = saturate(_CastEnabled);
                tex.rgb = tex.rgb * tex.a + cue * tex.a * localizedLift * enabled;
                return tex;
            }
            ENDCG
        }
    }
}
