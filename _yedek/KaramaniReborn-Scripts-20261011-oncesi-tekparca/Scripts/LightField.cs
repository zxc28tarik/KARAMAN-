using System.Collections.Generic;
using UnityEngine;

namespace KaramaniReborn
{
    // Sahnedeki sıcak ışık kaynakları (mangal, yol taşı). Karakterler her karede en yakın ışığı
    // sorgular; kenar ışığının yönü ve rengi buradan gelir. Işık yoksa bölgenin gökyüzü ışığı kullanılır.
    public static class LightField
    {
        struct Source { public Vector2 point; public float radius, intensity, phase; public Color color; }
        static readonly List<Source> sources = new();
        static Vector2 ambientDirection = new(1, .35f);
        static Color ambientColor = new(1, .62f, .32f);
        static float ambientStrength = .35f;
        public static float Clock;

        public static void Reset(Vector2 direction, Color color, float strength)
        {
            sources.Clear(); ambientDirection = direction.normalized; ambientColor = color; ambientStrength = strength;
        }

        public static void Add(Vector2 point, float radius, float intensity, Color color, float phase)
            => sources.Add(new Source { point = point, radius = radius, intensity = intensity, color = color, phase = phase });

        // amount: 0 = yalnızca gökyüzü ışığı, 1 = ateşin hemen yanında.
        public static void Sample(Vector2 at, out Vector2 direction, out Color color, out float amount, out float rimStrength)
        {
            Vector2 sum = Vector2.zero; Color mixed = Color.black; float total = 0;
            foreach (var s in sources)
            {
                Vector2 d = s.point - at; float distance = d.magnitude;
                if (distance >= s.radius) continue;
                float flicker = .86f + .1f * Mathf.Sin(Clock * 11 + s.phase) + .04f * Mathf.Sin(Clock * 23 + s.phase * 2);
                float w = Mathf.Pow(1 - distance / s.radius, 2) * s.intensity * flicker;
                sum += (distance > .001f ? d / distance : Vector2.up) * w; mixed += s.color * w; total += w;
            }
            amount = Mathf.Clamp01(total);
            float blend = Mathf.Clamp01(total * 1.6f);
            Vector2 fireDirection = total > 0 && sum.sqrMagnitude > .0001f ? sum.normalized : ambientDirection;
            direction = Vector2.Lerp(ambientDirection, fireDirection, blend).normalized;
            color = Color.Lerp(ambientColor, total > 0 ? mixed / total : ambientColor, blend);
            rimStrength = Mathf.Lerp(ambientStrength, 1.15f, amount);
        }
    }
}
