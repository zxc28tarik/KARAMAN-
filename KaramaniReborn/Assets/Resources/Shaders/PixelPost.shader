// Yüksek çözünürlükte çizilen dünyayı piksel sanatına çevirir:
// 4x4 kutu filtresiyle küçültme, renk ayarı, dünyaya sabit Bayer titreşimi ve bölge paletine oturtma.
Shader "Karamani/PixelPost"
{
    Properties { _MainTex ("World", 2D) = "black" {} }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            sampler2D _MainTex; float4 _MainTex_TexelSize;
            float4 _Grade;      // x pozlama, y doygunluk, z kontrast, w kontrast merkezi
            float4 _ShadowTint, _LightTint;
            float4 _Look;       // x titreşim, y renk ağırlığı, z köşe kararması, w örnekleme katı
            float4 _Origin;     // xy dünya piksel kökeni, zw hedef boyutu
            float _PaletteCount;
            float4 _PaletteLab[64];
            float4 _PaletteRGB[64];

            float3 ToLab(float3 c)
            {
                float l = 0.4122214708 * c.r + 0.5363325363 * c.g + 0.0514459929 * c.b;
                float m = 0.2119034982 * c.r + 0.6806995451 * c.g + 0.1073969566 * c.b;
                float s = 0.0883024619 * c.r + 0.2817188376 * c.g + 0.6299787005 * c.b;
                l = pow(max(l, 1e-6), 1.0 / 3); m = pow(max(m, 1e-6), 1.0 / 3); s = pow(max(s, 1e-6), 1.0 / 3);
                return float3(0.2104542553 * l + 0.7936177850 * m - 0.0040720468 * s,
                              1.9779984951 * l - 2.4285922050 * m + 0.4505937099 * s,
                              0.0259040371 * l + 0.7827717662 * m - 0.8086757660 * s);
            }

            float Bayer(float2 cell)
            {
                const float m[16] = { 0, 8, 2, 10, 12, 4, 14, 6, 3, 11, 1, 9, 15, 7, 13, 5 };
                int index = (int)(cell.y * 4 + cell.x);
                return (m[index] + .5) / 16 - .5;
            }

            fixed4 frag(v2f_img i) : SV_Target
            {
                // Dört çift doğrusal örnek = 16 dünya pikselinin ortalaması
                float2 t = _MainTex_TexelSize.xy * (_Look.w * .25);
                float3 c = (tex2D(_MainTex, i.uv + float2(-t.x, -t.y)).rgb + tex2D(_MainTex, i.uv + float2(t.x, -t.y)).rgb
                          + tex2D(_MainTex, i.uv + float2(-t.x, t.y)).rgb + tex2D(_MainTex, i.uv + float2(t.x, t.y)).rgb) * .25;

                float3 g = LinearToGammaSpace(c) * _Grade.x;
                float luma = dot(g, float3(.299, .587, .114));
                // Parlak ve doygun ışıklar (ateş, meşale) rengini korur; geri kalan sahne solar
                float chroma = max(g.r, max(g.g, g.b)) - min(g.r, min(g.g, g.b));
                float keep = smoothstep(.42, .72, chroma) * smoothstep(.55, .88, luma);
                g = lerp(luma.xxx, g, lerp(_Grade.y, 1.1, keep));
                g = (g - _Grade.w) * _Grade.z + _Grade.w;
                luma = saturate(dot(g, float3(.299, .587, .114)));
                g *= lerp(_ShadowTint.rgb, _LightTint.rgb, smoothstep(.04, .7, luma));
                float2 q = i.uv - .5;
                g *= 1 - _Look.z * dot(q, q) * 2.4;

                float2 cell = fmod(floor(i.uv * _Origin.zw) + _Origin.xy + 4096, 4);
                g = saturate(g + Bayer(cell) * _Look.x);

                float3 lab = ToLab(GammaToLinearSpace(g));
                float best = 1e9; float3 result = 0;
                [loop] for (int k = 0; k < 64; k++)
                {
                    if (k >= _PaletteCount) break;
                    float3 e = lab - _PaletteLab[k].xyz;
                    float d = e.x * e.x + _Look.y * (e.y * e.y + e.z * e.z) + _PaletteLab[k].w;
                    if (d < best) { best = d; result = _PaletteRGB[k].rgb; }
                }
                return fixed4(result, 1);
            }
            ENDCG
        }
    }
}
