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
        public float cameraShake, hitStop, zoomPunch, slowMotion;
        public Vector2 shakeDirection = Vector2.right;
        public Actor lastStruck;
        public int ImpactCount { get; private set; }
        int decals;
        const int MaxDecals = 140;
        sealed class Particle
        {
            public SpriteRenderer renderer; public Vector2 velocity, growFrom, growTo; public float age, lifetime, gravity, spin, groundY = float.NaN, growTime; public Color color;
            public bool alignToVelocity, decal;
        }
        public Effects(Transform parent) { root = new GameObject("Atmosphere and impacts").transform; root.SetParent(parent, false); }
        float Rand(float a, float b) => a + (float)rng.NextDouble() * (b - a);

        Particle Spawn(Sprite sprite, Vector2 position, Vector2 size, Color color, float life, int order = 180)
        {
            var p = pool.Count > 0 ? pool.Pop() : new Particle { renderer = ArtLibrary.Draw("FX particle", sprite, root, position, order) };
            p.renderer.gameObject.SetActive(true); p.renderer.sprite = sprite; p.renderer.sortingOrder = order; p.renderer.color = color;
            p.renderer.flipX = p.renderer.flipY = false;
            p.renderer.transform.position = position; p.renderer.transform.rotation = Quaternion.identity; p.renderer.transform.localScale = new Vector3(size.x, size.y, 1);
            p.velocity = Vector2.zero; p.gravity = p.spin = p.age = p.growTime = 0; p.groundY = float.NaN; p.alignToVelocity = p.decal = false;
            p.lifetime = life; p.color = color; active.Add(p); return p;
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

        // groundY: kan damlalarının düşüp iz bırakacağı zemin yüksekliği (NaN ise iz yok). stop: duraksama süresi.
        public void Impact(Vector2 point, int face, bool blocked, bool heavy, float groundY = float.NaN, float stop = -1)
        {
            ImpactCount++;
            cameraShake = Mathf.Max(cameraShake, heavy ? .16f : blocked ? .06f : .075f);
            shakeDirection = new Vector2(face == 0 ? 1 : face, .3f).normalized;
            zoomPunch = Mathf.Max(zoomPunch, heavy ? .035f : blocked ? .008f : .014f);
            hitStop = Mathf.Max(hitStop, stop >= 0 ? stop : heavy ? .09f : .055f);
            Color c = blocked ? new Color(.8f, .91f, 1) : new Color(1, .68f, .29f);
            Spawn(ArtLibrary.Glow, point, new Vector2(heavy ? 1.0f : .5f, heavy ? 1.0f : .5f), new Color(c.r, c.g, c.b, .8f), .13f);
            for (int i = 0; i < (heavy ? 22 : 13); i++)
            {
                var p = Spawn(ArtLibrary.White, point, new Vector2(.013f, Rand(.035f, .10f)), c, Rand(.14f, .42f));
                p.velocity = new Vector2(Rand(-1.2f, 4.0f) * face, Rand(-1, 3.3f)); p.gravity = 8; p.spin = Rand(-600, 600);
                p.renderer.transform.rotation = Quaternion.Euler(0, 0, Rand(-180, 180));
            }
            if (!blocked) Blood(point, face, heavy, groundY);
        }

        // Kan: darbe yönünde fışkıran damlalar ve dağılan kan buharı; damlalar zemine inince iz bırakır
        void Blood(Vector2 point, int face, bool heavy, float groundY)
        {
            int drops = heavy ? 26 : 16;
            for (int i = 0; i < drops; i++)
            {
                float shade = Rand(.32f, .55f);
                var p = Spawn(ArtLibrary.White, point + new Vector2(Rand(-.05f, .05f), Rand(-.08f, .08f)), new Vector2(Rand(.018f, .03f), Rand(.035f, .09f)),
                    new Color(shade, .02f, .025f, .95f), Rand(.5f, 1.1f), 178);
                p.velocity = new Vector2(face * Rand(1.2f, heavy ? 7.5f : 5.5f), Rand(-.6f, heavy ? 4.6f : 3.6f));
                p.gravity = 15; p.alignToVelocity = true; p.groundY = groundY;
            }
            for (int i = 0; i < (heavy ? 6 : 4); i++)
            {
                var p = Spawn(ArtLibrary.Glow, point, new Vector2(Rand(.18f, .32f), Rand(.14f, .24f)), new Color(.42f, .02f, .03f, .55f), Rand(.25f, .45f), 177);
                p.velocity = new Vector2(face * Rand(.6f, 2.6f), Rand(-.4f, 1.2f)); p.gravity = 2;
            }
        }

        void Decal(Vector2 point, Vector2 size, Color color, float life, float growTime = 0, int order = 63)
        {
            if (decals >= MaxDecals)
            {
                foreach (var old in active) if (old.decal && old.age < old.lifetime - 3) { old.age = old.lifetime - 3; break; }
            }
            var p = Spawn(ArtLibrary.Glow, point, growTime > 0 ? size * .15f : size, color, life, order);
            p.decal = true; p.growTime = growTime; p.growFrom = size * .15f; p.growTo = size; decals++;
        }

        public void BloodPool(Vector2 feet, float size)
            => Decal(feet + new Vector2(0, .015f), new Vector2(1.35f * size, .16f * size), new Color(.17f, .008f, .012f, .9f), 32, 1.8f, 62);

        // Saldırı uyarısı: kılıç ucunda kısa, parlak bir yıldız
        public void Glint(Vector2 point, bool strong)
        {
            float s = strong ? 1.3f : 1;
            Spawn(ArtLibrary.Glow, point, new Vector2(.42f, .42f) * s, new Color(1, .93f, .76f, .95f), .22f, 181);
            for (int i = 0; i < 2; i++)
            {
                var p = Spawn(ArtLibrary.White, point, new Vector2(.62f * s, .012f), new Color(1, .96f, .86f, .95f), .2f, 182);
                p.renderer.transform.rotation = Quaternion.Euler(0, 0, i * 90 + 15);
            }
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
            cameraShake = Mathf.MoveTowards(cameraShake, 0, dt * .9f); hitStop = Mathf.MoveTowards(hitStop, 0, dt);
            zoomPunch = Mathf.MoveTowards(zoomPunch, 0, dt * .22f); slowMotion = Mathf.MoveTowards(slowMotion, 0, dt);
            if (hitStop <= 0) lastStruck = null;
            for (int i = active.Count - 1; i >= 0; i--)
            {
                var p = active[i]; p.age += dt;
                if (p.age >= p.lifetime)
                {
                    if (p.decal) decals--;
                    p.renderer.gameObject.SetActive(false); active.RemoveAt(i); pool.Push(p); continue;
                }
                if (p.decal)
                {
                    if (p.growTime > 0)
                    {
                        float g = 1 - Mathf.Pow(1 - Mathf.Clamp01(p.age / p.growTime), 3);
                        Vector2 size = Vector2.Lerp(p.growFrom, p.growTo, g); p.renderer.transform.localScale = new Vector3(size.x, size.y, 1);
                    }
                    float fade = Mathf.Clamp01((p.lifetime - p.age) / 3);
                    p.renderer.color = new Color(p.color.r, p.color.g, p.color.b, p.color.a * fade);
                    continue;
                }
                Vector2 before = p.renderer.transform.position;
                p.velocity += Vector2.down * p.gravity * dt;
                Vector2 after = before + p.velocity * dt;
                // Zemine inen kan damlası bir iz bırakıp kaybolur
                if (!float.IsNaN(p.groundY) && p.velocity.y < 0 && after.y <= p.groundY + .02f)
                {
                    Decal(new Vector2(after.x, p.groundY + .012f), new Vector2(Rand(.07f, .2f), Rand(.018f, .034f)), new Color(Rand(.2f, .32f), .01f, .015f, .85f), 24);
                    p.age = p.lifetime; continue;
                }
                p.renderer.transform.position = after; p.renderer.transform.Rotate(0, 0, p.spin * dt);
                if (p.alignToVelocity && p.velocity.sqrMagnitude > .01f)
                    p.renderer.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(p.velocity.y, p.velocity.x) * Mathf.Rad2Deg - 90);
                float opacity = 1 - p.age / p.lifetime; p.renderer.color = new Color(p.color.r, p.color.g, p.color.b, p.color.a * opacity);
            }
        }

        public void Clear() { foreach (var p in active) { p.renderer.gameObject.SetActive(false); pool.Push(p); } active.Clear(); decals = 0; }
    }
}
