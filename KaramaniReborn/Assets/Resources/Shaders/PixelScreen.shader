// Piksel görüntüsünü ekrana keskin büyütür. Tamsayı olmayan ölçeklerde de pikseller eşit kalır
// (yalnız piksel sınırında bir ekran pikseli yumuşatılır). Kamera alt-piksel kaydırması burada uygulanır.
Shader "Karamani/PixelScreen"
{
    Properties { _MainTex ("Pixels", 2D) = "black" {} }
    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Opaque" }
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            sampler2D _MainTex; float4 _MainTex_TexelSize;
            float4 _View;   // xy görünen piksel sayısı, zw alt-piksel kayma

            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            v2f vert(float4 vertex : POSITION, float2 uv : TEXCOORD0)
            {
                // Dörtgen, nesne dönüşümünden bağımsız olarak kameranın tüm görüş alanını kaplar
                v2f o; o.pos = float4(vertex.xy * 2, .5, 1); o.pos.y *= _ProjectionParams.x; o.uv = uv; return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 size = _MainTex_TexelSize.zw;
                float2 pix = (i.uv - .5) * _View.xy + _View.zw + size * .5;
                float2 seam = floor(pix + .5);
                float2 step = max(fwidth(pix), 1e-5);
                pix = seam + clamp((pix - seam) / step, -.5, .5);
                return tex2D(_MainTex, pix / size);
            }
            ENDCG
        }
    }
}
