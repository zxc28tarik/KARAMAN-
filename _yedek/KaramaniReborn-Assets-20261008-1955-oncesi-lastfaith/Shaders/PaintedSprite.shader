// Boyalı karakter parçaları için sprite gölgelendiricisi.
// Kaynak resmi değiştirmez; yalnızca çizim anında ışık, kenar ışığı ve vurulma parlaması ekler.
// Kenar ışığı: pikselin ışık yönündeki komşusu saydamsa o piksel ışığa bakan kenardadır.
Shader "Karamani/PaintedSprite"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Flash ("Vurulma parlaması", Range(0, 1)) = 0
        _RimColor ("Kenar ışığı rengi", Color) = (1, 0.6, 0.3, 1)
        _RimDir ("Işık yönü (dünya xy)", Vector) = (1, 0.35, 0, 0)
        _RimWidth ("Kenar genişliği (doku pikseli)", Float) = 10
        _RimStrength ("Kenar ışığı gücü", Float) = 0.35
        _LightColor ("Yakın ışık rengi", Color) = (1, 0.62, 0.32, 1)
        _LightAmount ("Yakın ışık miktarı", Float) = 0
        _MoonStrength ("Üst soğuk ışık", Float) = 0.22
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" "CanUseSpriteAtlas" = "True" }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed _Flash;
            fixed4 _RimColor;
            float4 _RimDir;
            float _RimWidth, _RimStrength, _LightAmount, _MoonStrength;
            fixed4 _LightColor;

            struct appdata { float4 vertex : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; fixed4 color : COLOR; float2 uv : TEXCOORD0; float2 rimOffset : TEXCOORD1; float2 moonOffset : TEXCOORD2; };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                // Dünya yönünü parçanın kendi (dönmüş, aynalanmış) eksenine çevir; sprite UV'si bu eksenle hizalıdır.
                float2 lightDir = mul((float3x3)unity_WorldToObject, float3(_RimDir.xy, 0)).xy;
                float2 upDir = mul((float3x3)unity_WorldToObject, float3(0, 1, 0)).xy;
                o.rimOffset = normalize(lightDir + 1e-5) * _MainTex_TexelSize.xy * _RimWidth;
                o.moonOffset = normalize(upDir + 1e-5) * _MainTex_TexelSize.xy * _RimWidth * 0.75;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv);
                float edge = saturate(c.a - tex2D(_MainTex, i.uv + i.rimOffset).a);
                float top = saturate(c.a - tex2D(_MainTex, i.uv + i.moonOffset).a);
                c.rgb *= i.color.rgb;
                // Yakın ateş: rengi ısıtır ve hafifçe aydınlatır
                c.rgb *= lerp(float3(1, 1, 1), _LightColor.rgb * 1.3, saturate(_LightAmount) * 0.45);
                c.rgb += _RimColor.rgb * edge * _RimStrength;
                c.rgb += float3(0.52, 0.6, 0.86) * top * _MoonStrength;
                c.rgb = lerp(c.rgb, float3(1, 0.97, 0.92), _Flash);
                c.a *= i.color.a;
                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }
}
