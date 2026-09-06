Shader "Custom/SkillAnimationVfxSourceReadable"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _VfxGlobalAlpha ("VFX Global Alpha", Range(0,1)) = 1
        _VfxBodyOpacityGain ("Source Alpha Readability", Range(0,1)) = .65
        _VfxEmissionColor ("Glow Color", Color) = (.3,.85,1,1)
        _VfxEmissionIntensity ("Glow Strength", Range(0,.25)) = .08
        _OutlineColor ("Outline Color", Color) = (.25,.85,1,.28)
        _OutlineWidth ("Outline Width", Range(0,2)) = 1
        _AlphaClip ("Alpha Clip", Range(0,.1)) = 0
        _VfxSpriteUvRect ("Sprite UV Rect", Vector) = (0,0,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Cull Off Lighting Off ZWrite Off Blend One OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex:POSITION; float4 color:COLOR; float2 uv:TEXCOORD0; };
            struct v2f { float4 vertex:SV_POSITION; fixed4 color:COLOR; float2 uv:TEXCOORD0; };
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color, _VfxEmissionColor, _OutlineColor;
            float _VfxGlobalAlpha, _VfxBodyOpacityGain, _VfxEmissionIntensity, _OutlineWidth, _AlphaClip;
            float4 _VfxSpriteUvRect;

            v2f vert(appdata i)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(i.vertex);
                o.uv = i.uv;
                o.color = i.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 source = tex2D(_MainTex, i.uv) * i.color;
                float2 texel = _MainTex_TexelSize.xy * _OutlineWidth;
                float2 uvR = clamp(i.uv + float2(texel.x, 0), _VfxSpriteUvRect.xy, _VfxSpriteUvRect.zw);
                float2 uvL = clamp(i.uv - float2(texel.x, 0), _VfxSpriteUvRect.xy, _VfxSpriteUvRect.zw);
                float2 uvU = clamp(i.uv + float2(0, texel.y), _VfxSpriteUvRect.xy, _VfxSpriteUvRect.zw);
                float2 uvD = clamp(i.uv - float2(0, texel.y), _VfxSpriteUvRect.xy, _VfxSpriteUvRect.zw);
                float neighborAlpha = max(max(tex2D(_MainTex, uvR).a, tex2D(_MainTex, uvL).a),
                                          max(tex2D(_MainTex, uvU).a, tex2D(_MainTex, uvD).a));
                float envelope = saturate(_VfxGlobalAlpha);
                float alpha = lerp(source.a, sqrt(saturate(source.a)), saturate(_VfxBodyOpacityGain)) * envelope;
                if (source.a <= _AlphaClip && neighborAlpha > _AlphaClip)
                {
                    float outlineAlpha = _OutlineColor.a * neighborAlpha * envelope;
                    return fixed4(_OutlineColor.rgb * outlineAlpha, outlineAlpha);
                }
                clip(alpha - _AlphaClip - 0.00001);
                // Preserve authored RGB as the primary layer. Glow is a bounded in-shape lift,
                // never a replacement tint and never writes into transparent pixels.
                float luminance = dot(source.rgb, float3(.2126, .7152, .0722));
                float glowMask = saturate(luminance * .7 + source.a * .3);
                float3 rgb = source.rgb + _VfxEmissionColor.rgb * _VfxEmissionIntensity * glowMask;
                return fixed4(rgb * alpha, alpha);
            }
            ENDCG
        }
    }
}
