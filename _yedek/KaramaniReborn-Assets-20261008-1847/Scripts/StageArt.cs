using System.Collections.Generic;
using UnityEngine;

namespace KaramaniReborn
{
    public sealed class StageArt
    {
        public readonly Transform root;
        public float MaxWalkEdgeError { get; private set; }
        readonly SpriteRenderer background;
        readonly ZoneDefinition zone;
        readonly List<Weather> particles = new();
        readonly List<Light> lights = new();
        readonly List<Transform> banners = new();
        readonly System.Random rng;
        sealed class Weather { public Transform transform; public Vector2 velocity; public float phase; public bool mist; }
        sealed class Light { public SpriteRenderer glow; public float phase; public Vector2 point; }

        public StageArt(ZoneDefinition definition, Transform parent)
        {
            zone = definition; rng = new System.Random(1566 + zone.index);
            root = new GameObject(zone.title + " · Painted stage").transform; root.SetParent(parent, false);
            background = ArtLibrary.Draw("Distant painted vista", ArtLibrary.Background(zone.index), root, Vector2.zero, -100);
            float scale = 13.8f / background.sprite.bounds.size.y;
            background.transform.localScale = new Vector3(scale, scale, 1);
            Sprite stone = ArtLibrary.LoadWhole("stone"); var props = ArtLibrary.Props();
            foreach (var surface in zone.surfaces)
            {
                if (surface.wood)
                {
                    var wood = ArtLibrary.Draw("Walkable timber", props[6], root, new Vector2((surface.left + surface.right) * .5f, surface.y - 1.4f), 65, zone.ambient);
                    float ws = (surface.right - surface.left) / wood.sprite.bounds.size.x;
                    wood.transform.localScale = new Vector3(ws, ws, 1);
                    // The upper edge of the painting is the physical platform plane.
                    wood.transform.position = new Vector2((surface.left + surface.right) * .5f, surface.y - wood.sprite.bounds.size.y * ws);
                }
                else if (surface.oneWay) TileStone(stone, surface);
                else TileRampart(ArtLibrary.LoadWhole("rampart"), surface, props[3]);
            }
            for (int i = 0; i < 14; i++)
            {
                float x = -2 + i * 4.35f;
                float y = zone.floor;
                if (!GroundAt(x)) continue;
                var obj = ArtLibrary.Draw(i % 2 == 0 ? "Wood crate" : "Barrel", props[i % 2], root, new Vector2(x, y), 74, zone.ambient);
                float size = (i % 2 == 0 ? .92f : 1.05f) / obj.sprite.bounds.size.y;
                obj.transform.localScale = new Vector3(size, size, 1);
                if (i % 3 == 0)
                {
                    var barrel = ArtLibrary.Draw("Second barrel", props[1], root, new Vector2(x + .8f, y), 73, zone.ambient);
                    float bs = .95f / barrel.sprite.bounds.size.y; barrel.transform.localScale = new Vector3(bs, bs, 1);
                }
            }
            for (int i = 0; i < 10; i++)
            {
                float x = -3 + i * 6.4f;
                Prop(props[2], "Brazier", x, zone.floor, 1.15f, 78);
                Torch(new Vector2(x, zone.floor + .94f));
                var banner = Prop(props[4], "Crescent banner", x + 1.65f, zone.floor + .1f, 2.4f, 46);
                banners.Add(banner.transform);
            }
            foreach (float x in new[] { 2f, 21f, 39f })
            {
                Prop(props[7], "Yol taşı", x, zone.floor, 1.45f, 66);
                var glow = ArtLibrary.Draw("Shrine glow", ArtLibrary.Glow, root, new Vector2(x, zone.floor + .72f), 72, new Color(1, .74f, .30f, .3f));
                glow.transform.localScale = new Vector3(2, 2, 1);
            }
            foreach (float x in new[] { 8.8f, 25f, 39f, 45.3f }) Prop(props[5], "Ladder", x, zone.floor, 1.48f, 61);
            Prop(props[4], "Inner gate banner", zone.length - .8f, zone.floor, 3.9f, 62);
            Prop(props[3], "Gate lantern", zone.length - .8f, zone.floor + 2.85f, .70f, 65);
            for (int i = 0; i < 26; i++)
            {
                var mist = ArtLibrary.Draw("Layered mist", ArtLibrary.Fog, root, new Vector2(Rand(-8, 64), Rand(-3.5f, 0)), i % 2 == 0 ? 58 : 155, new Color(zone.fog.r, zone.fog.g, zone.fog.b, .065f));
                mist.transform.localScale = new Vector3(Rand(3, 7), Rand(.5f, 1.3f), 1);
                particles.Add(new Weather { transform = mist.transform, velocity = new Vector2(Rand(.015f, .07f), 0), phase = Rand(0, 6), mist = true });
            }
            for (int i = 0; i < (zone.snow ? 160 : 65); i++)
            {
                var p = ArtLibrary.Draw(zone.snow ? "Falling snow" : "Floating ember", ArtLibrary.Glow, root, new Vector2(Rand(-8, 64), Rand(-3, 5.5f)), 158,
                    zone.snow ? new Color(.85f, .94f, 1f, Rand(.3f, .65f)) : new Color(1, Rand(.45f, .75f), .2f, Rand(.35f, .85f)));
                float ps = Rand(.012f, .035f); p.transform.localScale = new Vector3(ps, ps * (zone.snow ? 1 : 1.8f), 1);
                particles.Add(new Weather { transform = p.transform, velocity = new Vector2(Rand(-.13f, .13f), zone.snow ? Rand(-.55f, -.25f) : Rand(.08f, .30f)), phase = Rand(0, 8) });
            }
            // Foreground plants/props provide occlusion without obscuring the combat silhouette.
            for (int i = 0; i < 8; i++)
            {
                var near = Prop(props[1], "Near silhouette", -5 + i * 8.8f, -6.15f, 1.4f, 200);
                near.color = new Color(.13f, .14f, .17f, .9f);
            }
        }

        void TileStone(Sprite sprite, Surface s)
        {
            float tileWidth = s.oneWay ? 3.2f : 4.9f;
            float scale = tileWidth / sprite.bounds.size.x, height = sprite.bounds.size.y * scale;
            int rows = s.oneWay ? 1 : Mathf.CeilToInt(s.thickness / height);
            for (int row = 0; row < rows; row++)
            {
                float width = s.right - s.left;
                int columns = Mathf.CeilToInt(width / (tileWidth - .07f));
                for (int col = 0; col < columns; col++)
                {
                    float x = s.left + tileWidth * .5f + col * (tileWidth - .07f);
                    if (col == columns - 1) x = s.right - tileWidth * .5f;
                    var tile = ArtLibrary.Draw("Painted stone masonry", sprite, root, new Vector2(x, s.y - height * (.5f + row)), 60, zone.ambient * Mathf.Lerp(1, .28f, row / 4f));
                    tile.transform.localScale = new Vector3(scale, scale, 1);
                }
            }
        }

        void TileRampart(Sprite sprite, Surface surface, Sprite lantern)
        {
            const float tileWidth = 11;
            float scale = tileWidth / sprite.bounds.size.x, height = sprite.bounds.size.y * scale;
            float topInset = ArtLibrary.FlatTopInset(sprite) / sprite.pixelsPerUnit * scale;
            var depth = ArtLibrary.Draw("Undercroft depth", ArtLibrary.White, root,
                new Vector2((surface.left + surface.right) * .5f, surface.y - height * .5f), 57, new Color(.009f, .016f, .028f, .62f));
            depth.transform.localScale = new Vector3(surface.right - surface.left, height, 1);
            for (float left = surface.left; left < surface.right - .01f; left += tileWidth)
            {
                // The last module is a native rectangle into the source painting, never a
                // squeezed copy. The walkable painted edge and physical surface coincide.
                float width = Mathf.Min(tileWidth, surface.right - left);
                var rect = sprite.rect; rect.width *= width / tileWidth;
                var piece = Sprite.Create(sprite.texture, rect, new Vector2(0, 1), 100, 0, SpriteMeshType.FullRect);
                var masonry = ArtLibrary.Draw("Arched painted rampart", piece, root, new Vector2(left, surface.y + topInset), 60, zone.ambient);
                masonry.transform.localScale = new Vector3(scale, scale, 1);
                float paintedEdge = masonry.transform.TransformPoint(new Vector3(0, -ArtLibrary.FlatTopInset(sprite) / sprite.pixelsPerUnit, 0)).y;
                MaxWalkEdgeError = Mathf.Max(MaxWalkEdgeError, Mathf.Abs(paintedEdge - surface.y));
                for (int arch = 0; arch < 3; arch++)
                {
                    float x = left + tileWidth * (.19f + arch * .31f);
                    if (x > surface.right - .4f) continue;
                    Prop(lantern, "Undercroft lantern", x, surface.y - 2.75f, .43f, 58);
                    var light = ArtLibrary.Draw("Undercroft warmth", ArtLibrary.Glow, root, new Vector2(x, surface.y - 2.58f), 59, new Color(1, .47f, .15f, .38f));
                    light.transform.localScale = new Vector3(2.1f, 2.1f, 1);
                }
            }
        }

        bool GroundAt(float x)
        { foreach (var s in zone.surfaces) if (!s.oneWay && x > s.left + .45f && x < s.right - .45f) return true; return false; }

        SpriteRenderer Prop(Sprite sprite, string name, float x, float y, float height, int order)
        {
            var sr = ArtLibrary.Draw(name, sprite, root, new Vector2(x, y), order, zone.ambient);
            float s = height / sprite.bounds.size.y; sr.transform.localScale = new Vector3(s, s, 1); return sr;
        }
        void Torch(Vector2 point)
        {
            var glow = ArtLibrary.Draw("Warm brazier light", ArtLibrary.Glow, root, point, 156, new Color(1, .47f, .11f, .22f));
            glow.transform.localScale = new Vector3(3.4f, 3.4f, 1); lights.Add(new Light { glow = glow, point = point, phase = Rand(0, 8) });
            for (int i = 0; i < 4; i++)
            {
                var flame = ArtLibrary.Draw("Flame tongue", ArtLibrary.Fog, root, point + Vector2.up * .17f, 160, new Color(1, i == 0 ? .83f : .44f, .13f, .65f));
                flame.transform.localScale = new Vector3(.12f + i * .025f, .42f - i * .04f, 1); lights.Add(new Light { glow = flame, point = point + Vector2.up * .17f, phase = Rand(0, 6) });
            }
        }
        float Rand(float a, float b) => a + (float)rng.NextDouble() * (b - a);

        public void Tick(float dt, float clock, Vector2 camera)
        {
            background.transform.position = new Vector3(camera.x - (camera.x - 25) * .09f, camera.y + .85f, 0);
            foreach (var p in particles)
            {
                p.transform.position += (Vector3)(p.velocity * dt);
                if (!p.mist)
                {
                    var position = p.transform.position; position.x += Mathf.Sin(clock * .6f + p.phase) * dt * .03f;
                    if (position.y > 5.6f) position.y = -3; if (position.y < -3.1f) position.y = 5.3f;
                    p.transform.position = position;
                }
            }
            foreach (var l in lights)
            {
                float flicker = .8f + .2f * Mathf.Sin(clock * 11 + l.phase) + .05f * Mathf.Sin(clock * 23 + l.phase);
                l.glow.color = new Color(l.glow.color.r, l.glow.color.g, l.glow.color.b, l.glow.name == "Warm brazier light" ? .23f * flicker : .62f * flicker);
                l.glow.transform.position = l.point + Vector2.right * Mathf.Sin(clock * 7 + l.phase) * .028f;
            }
            for (int i = 0; i < banners.Count; i++) banners[i].localRotation = Quaternion.Euler(0, 0, Mathf.Sin(clock * 1.4f + i) * 1.5f);
        }
    }
}
