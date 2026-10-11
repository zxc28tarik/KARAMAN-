// Kılıç izi: köşe renkleriyle boyanan, eklemeli (parlayan) bir şerit. UV.y şeridin enine yumuşak kenar verir.
Shader "Karamani/BladeTrail"
{
    Properties { }
    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha One

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; fixed4 color : COLOR; float2 uv : TEXCOORD0; };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 c = i.color;
                // uç tarafı (uv.y = 1) keskin ve parlak, kök tarafı yumuşak
                c.a *= smoothstep(0, 0.55, i.uv.y) * (0.55 + 0.45 * i.uv.y);
                return c;
            }
            ENDCG
        }
    }
}
