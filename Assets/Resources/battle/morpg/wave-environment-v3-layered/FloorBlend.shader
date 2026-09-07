Shader "MORPG/LayeredFloor"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _FloorHorizon ("World Horizon", Float) = 3.55
        _FloorBlend ("Blend Height", Float) = 0.6
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
            struct v2f { float4 vertex:SV_POSITION; fixed4 color:COLOR; float2 uv:TEXCOORD0; float worldY:TEXCOORD1; };
            sampler2D _MainTex;
            fixed4 _Color;
            float _FloorHorizon, _FloorBlend;
            v2f vert(appdata i)
            {
                v2f o;
                o.vertex=UnityObjectToClipPos(i.vertex);
                o.worldY=mul(unity_ObjectToWorld,i.vertex).y;
                o.uv=i.uv;o.color=i.color*_Color;
                return o;
            }
            fixed4 frag(v2f i):SV_Target
            {
                fixed4 c=tex2D(_MainTex,i.uv)*i.color;
                c.a*=smoothstep(0,1,saturate((_FloorHorizon-i.worldY)/max(_FloorBlend,.001)));
                c.rgb*=c.a;
                return c;
            }
            ENDCG
        }
    }
}
