using System.Collections.Generic;
using UnityEngine;

namespace KaramaniReborn
{
    public sealed class Effects
    {
        readonly Transform root;
        readonly List<Particle> active = new();
        readonly Stack<Particle> pool = new();
        readonly System.Random rng = new(1166);
        public float cameraShake, hitStop;
        public int ImpactCount { get; private set; }
        sealed class Particle { public SpriteRenderer renderer; public Vector2 velocity; public float age, lifetime, gravity, spin; public Color color; }
        public Effects(Transform parent) { root = new GameObject("Atmosphere and impacts").transform; root.SetParent(parent, false); }
        float Rand(float a, float b) => a + (float)rng.NextDouble() * (b - a);

        Particle Spawn(Sprite sprite, Vector2 position, Vector2 size, Color color, float life, int order = 180)
        {
            var p = pool.Count > 0 ? pool.Pop() : new Particle { renderer = ArtLibrary.Draw("FX particle", sprite, root, position, order) };
            p.renderer.gameObject.SetActive(true); p.renderer.sprite = sprite; p.renderer.sortingOrder = order; p.renderer.color = color;
            p.renderer.flipX = p.renderer.flipY = false;
            p.renderer.transform.position = position; p.renderer.transform.rotation = Quaternion.identity; p.renderer.transform.localScale = new Vector3(size.x, size.y, 1);
            p.velocity = Vector2.zero; p.gravity = p.spin = p.age = 0; p.lifetime = life; p.color = color; active.Add(p); return p;
        }

        public void Ghost(SpriteRenderer source, Color color)
        {
            var p = Spawn(source.sprite, source.transform.position, Vector2.one, color, .24f, source.sortingOrder - 20);
            // A mirrored parent changes the world basis of a rotated child. Reconstruct that
            // basis explicitly so left-facing ghosts match the original rigid painting exactly.
            Vector3 x = source.transform.TransformVector(Vector3.right), y = source.transform.TransformVector(Vector3.up);
            float determinant = x.x * y.y - x.y * y.x;
            p.renderer.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(x.y, x.x) * Mathf.Rad2Deg);
            p.renderer.transform.localScale = new Vector3(x.magnitude, y.magnitude * Mathf.Sign(determinant), 1);
        }

        public void Impact(Vector2 point, int face, bool blocked, bool heavy)
        {
            ImpactCount++; cameraShake = Mathf.Max(cameraShake, heavy ? .095f : .045f); hitStop = Mathf.Max(hitStop, heavy ? .045f : .025f);
            Color c = blocked ? new Color(.8f, .91f, 1) : new Color(1, .68f, .29f);
            Spawn(ArtLibrary.Glow, point, new Vector2(heavy ? 1.0f : .5f, heavy ? 1.0f : .5f), new Color(c.r, c.g, c.b, .8f), .13f);
            for (int i = 0; i < (heavy ? 22 : 13); i++)
            {
                var p = Spawn(ArtLibrary.White, point, new Vector2(.013f, Rand(.035f, .10f)), c, Rand(.14f, .42f));
                p.velocity = new Vector2(Rand(-1.2f, 4.0f) * face, Rand(-1, 3.3f)); p.gravity = 8; p.spin = Rand(-600, 600);
                p.renderer.transform.rotation = Quaternion.Euler(0, 0, Rand(-180, 180));
            }
            if (!blocked) for (int i = 0; i < 5; i++)
            {
                var p = Spawn(ArtLibrary.Glow, point, new Vector2(.08f, .06f), new Color(.48f, .03f, .025f, .8f), .32f);
                p.velocity = new Vector2(Rand(1, 3) * face, Rand(0, 2)); p.gravity = 10;
            }
        }

        public void SwordTrace(Vector2 a, Vector2 b, Color color)
        {
            float distance = Vector2.Distance(a, b); if (distance < .005f || distance > 2.4f) return;
            var p = Spawn(ArtLibrary.White, (a + b) * .5f, new Vector2(distance, .016f), color, .105f);
            p.renderer.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg);
        }

        public void Dust(Vector2 point, int face, bool snow = false)
        {
            for (int i = 0; i < 3; i++)
            { var p = Spawn(ArtLibrary.Fog, point + new Vector2(Rand(-.1f, .1f), .04f), new Vector2(.24f, .10f), snow ? new Color(.87f, .93f, 1, .35f) : new Color(.5f, .44f, .35f, .25f), .42f, 145); p.velocity = new Vector2(-face * Rand(.08f, .4f), Rand(.03f, .18f)); }
        }

        public void GlowBurst(Vector2 point, Color color)
        {
            Spawn(ArtLibrary.Glow, point, new Vector2(2.3f, 2.3f), new Color(color.r, color.g, color.b, .6f), .45f);
            for (int i = 0; i < 16; i++)
            { float angle = i * Mathf.PI * 2 / 16; var p = Spawn(ArtLibrary.Glow, point, new Vector2(.12f, .12f), color, .65f); p.velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 2.5f; }
        }

        public void Wolf(Vector2 point, int face)
        {
            var sprite = ArtLibrary.Gear()[3]; float scale = 3.3f / sprite.bounds.size.x;
            var p = Spawn(sprite, point + Vector2.up * 1.15f, Vector2.one * scale, new Color(1, 1, 1, .62f), .52f, 119);
            p.renderer.flipX = face < 0; p.velocity = Vector2.right * face * 8.2f;
        }

        public void Tick(float dt)
        {
            cameraShake = Mathf.MoveTowards(cameraShake, 0, dt * .6f); hitStop = Mathf.MoveTowards(hitStop, 0, dt);
            for (int i = active.Count - 1; i >= 0; i--)
            {
                var p = active[i]; p.age += dt;
                if (p.age >= p.lifetime) { p.renderer.gameObject.SetActive(false); active.RemoveAt(i); pool.Push(p); continue; }
                p.velocity += Vector2.down * p.gravity * dt;
                p.renderer.transform.position += (Vector3)(p.velocity * dt); p.renderer.transform.Rotate(0, 0, p.spin * dt);
                float opacity = 1 - p.age / p.lifetime; p.renderer.color = new Color(p.color.r, p.color.g, p.color.b, p.color.a * opacity);
            }
        }

        public void Clear() { foreach (var p in active) { p.renderer.gameObject.SetActive(false); pool.Push(p); } active.Clear(); }
    }
}
