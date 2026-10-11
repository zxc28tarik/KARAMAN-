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
        readonly Color plane;
        readonly List<Weather> particles = new();
        readonly List<Light> lights = new();
        readonly List<Transform> banners = new();
        readonly List<Layer> foreground = new();
        readonly List<SpriteRenderer> shafts = new();
        readonly SpriteRenderer haze, horizon;
        readonly System.Random rng;
        sealed class Weather { public Transform transform; public Vector2 position, velocity; public float phase; public bool mist; }
        sealed class Layer { public Transform transform; public Vector2 home; public float parallax; }
        sealed class Light { public SpriteRenderer glow; public float phase; public Vector2 point; }

        public StageArt(ZoneDefinition definition, Transform parent)
        {
            zone = definition; rng = new System.Random(1566 + zone.index);
            // Oyun düzlemindeki taş ve eşyalar biraz koyulaşır; aydınlatılan karakterler bunların önünde okunur
            plane = zone.ambient * .78f; plane.a = 1;
            root = new GameObject(zone.title + " · Painted stage").transform; root.SetParent(parent, false);
            // Işıksız yerde: ilk bölümde batan güneşin sıcak ışığı sağdan, diğerlerinde soğuk ay ışığı üstten
            if (zone.index == 0) LightField.Reset(new Vector2(1, .35f), new Color(1, .6f, .3f), .42f);
            else LightField.Reset(new Vector2(-.35f, 1), new Color(.62f, .72f, 1f), .32f);
            // Arka plan önceden piksellenmiş halde, birebir ölçekle çizilir (her doku pikseli bir oyun pikseli)
            background = ArtLibrary.Draw("Distant painted vista", ArtLibrary.PixelBackground(zone.index, 13.8f), root, Vector2.zero, -100);
            // Uzaklık pusu: arka plan sisin rengine karışır, oynanan düzlem öne çıkar
            Color hazeColor = zone.index switch { 0 => new Color(.42f, .28f, .38f), 1 => new Color(.27f, .24f, .25f), 2 => new Color(.22f, .30f, .37f), _ => new Color(.50f, .53f, .62f) };
            Color mistColor = zone.index switch { 0 => new Color(.62f, .45f, .48f), 1 => new Color(.42f, .38f, .38f), 2 => new Color(.42f, .54f, .62f), _ => new Color(.7f, .74f, .84f) };
            Color shade = zone.index switch { 0 => new Color(.05f, .035f, .055f), 1 => new Color(.03f, .028f, .03f), 2 => new Color(.02f, .035f, .045f), _ => new Color(.04f, .045f, .065f) };
            Color shaft = zone.index switch { 0 => new Color(1, .72f, .5f, .05f), 1 => new Color(1, .6f, .36f, .04f), 2 => new Color(.62f, .8f, 1, .06f), _ => new Color(.85f, .9f, 1, .06f) };
            haze = ArtLibrary.Draw("Distance haze", ArtLibrary.Haze, root, Vector2.zero, -96, new Color(hazeColor.r, hazeColor.g, hazeColor.b, zone.index == 1 ? .36f : .55f));
            haze.transform.localScale = new Vector3(40, 16, 1);
            horizon = ArtLibrary.Draw("Horizon mist", ArtLibrary.Fog, root, Vector2.zero, -95, new Color(mistColor.r, mistColor.g, mistColor.b, .3f));
            horizon.transform.localScale = new Vector3(46, 4.6f, 1);
            Sprite stone = ArtLibrary.LoadWhole("stone"); var props = ArtLibrary.Props();
            foreach (var surface in zone.surfaces)
            {
                if (surface.wood)
                {
                    var wood = ArtLibrary.Draw("Walkable timber", props[6], root, new Vector2((surface.left + surface.right) * .5f, surface.y - 1.4f), 65, plane);
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
                var obj = ArtLibrary.Draw(i % 2 == 0 ? "Wood crate" : "Barrel", props[i % 2], root, new Vector2(x, y), 74, plane);
                float size = (i % 2 == 0 ? .92f : 1.05f) / obj.sprite.bounds.size.y;
                obj.transform.localScale = new Vector3(size, size, 1);
                if (i % 3 == 0)
                {
                    var barrel = ArtLibrary.Draw("Second barrel", props[1], root, new Vector2(x + .8f, y), 73, plane);
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
                LightField.Add(new Vector2(x, zone.floor + .72f), 2.2f, .55f, new Color(1, .78f, .38f), x);
                var glow = ArtLibrary.Draw("Shrine glow", ArtLibrary.Glow, root, new Vector2(x, zone.floor + .72f), 72, new Color(1, .74f, .30f, .3f));
                glow.transform.localScale = new Vector3(2, 2, 1);
            }
            foreach (float x in new[] { 8.8f, 25f, 39f, 45.3f }) Prop(props[5], "Ladder", x, zone.floor, 1.48f, 61);
            Prop(props[4], "Inner gate banner", zone.length - .8f, zone.floor, 3.9f, 62);
            Prop(props[3], "Gate lantern", zone.length - .8f, zone.floor + 2.85f, .70f, 65);
            // Katmanlı sis: yerde sürünen bantlar, bir kısmı karakterlerin önünden geçer
            for (int i = 0; i < 30; i++)
            {
                Vector2 at = new Vector2(Rand(-8, 64), Rand(-3.4f, .2f));
                var mist = ArtLibrary.Draw("Layered mist", ArtLibrary.Fog, root, at, i % 3 == 0 ? 155 : 58, new Color(mistColor.r, mistColor.g, mistColor.b, i % 3 == 0 ? .07f : .11f));
                mist.transform.localScale = new Vector3(Rand(4, 9), Rand(.6f, 1.5f), 1);
                particles.Add(new Weather { transform = mist.transform, position = at, velocity = new Vector2(Rand(.03f, .11f), 0), phase = Rand(0, 6), mist = true });
            }
            // Kar, kül ve kıvılcım: tek ya da iki oyun pikseli boyunda, piksel ızgarasına oturur
            for (int i = 0; i < (zone.snow ? 190 : 90); i++)
            {
                Vector2 at = new Vector2(Rand(-8, 64), Rand(-3, 5.5f));
                bool ash = zone.index == 1 && i % 2 == 0;
                Color color = zone.snow ? new Color(.86f, .92f, 1f, Rand(.55f, 1f)) : ash ? new Color(.55f, .52f, .5f, Rand(.5f, .9f)) : new Color(1, Rand(.45f, .8f), .2f, Rand(.7f, 1f));
                var p = ArtLibrary.Draw(zone.snow ? "Falling snow" : ash ? "Falling ash" : "Floating ember", ArtLibrary.White, root, at, 158, color);
                float size = PixelPipeline.PixelSize * (zone.snow && Rand(0, 1) > .72f ? 2 : 1); p.transform.localScale = new Vector3(size, size, 1);
                Vector2 velocity = zone.snow ? new Vector2(Rand(-.25f, .05f), Rand(-.75f, -.35f)) : ash ? new Vector2(Rand(-.1f, .15f), Rand(-.35f, -.12f)) : new Vector2(Rand(-.13f, .13f), Rand(.15f, .45f));
                particles.Add(new Weather { transform = p.transform, position = at, velocity = velocity, phase = Rand(0, 8) });
            }
            // Ön plan siluetleri: oyun düzleminin önünde, kameradan hızlı kayar; neredeyse siyah
            for (int i = 0; i < 13; i++)
            {
                var s = ArtLibrary.Draw("Foreground silhouette", ArtLibrary.Silhouette((i * 7 + zone.index) % 3, zone.index * 31 + i), root, Vector2.zero, 205, shade);
                foreground.Add(new Layer { transform = s.transform, home = new Vector2(-8 + i * 6.1f + Rand(-1, 1), -5.95f), parallax = 1.32f });
            }
            // Gökten süzülen ışık huzmeleri
            for (int i = 0; i < 6; i++)
            {
                var ray = ArtLibrary.Draw("Light shaft", ArtLibrary.Fog, root, new Vector2(2 + i * 10.5f + Rand(-2, 2), 2.2f), 150, shaft);
                ray.transform.localScale = new Vector3(Rand(1.4f, 2.6f), 15, 1); ray.transform.localRotation = Quaternion.Euler(0, 0, zone.index == 0 ? 24 : -20);
                shafts.Add(ray);
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
                    var tile = ArtLibrary.Draw("Painted stone masonry", sprite, root, new Vector2(x, s.y - height * (.5f + row)), 60, plane * Mathf.Lerp(1, .28f, row / 4f));
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
                var masonry = ArtLibrary.Draw("Arched painted rampart", piece, root, new Vector2(left, surface.y + topInset), 60, plane);
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
            var sr = ArtLibrary.Draw(name, sprite, root, new Vector2(x, y), order, plane);
            float s = height / sprite.bounds.size.y; sr.transform.localScale = new Vector3(s, s, 1); return sr;
        }
        void Torch(Vector2 point)
        {
            var glow = ArtLibrary.Draw("Warm brazier light", ArtLibrary.Glow, root, point, 156, new Color(1, .47f, .11f, .22f));
            glow.transform.localScale = new Vector3(3.4f, 3.4f, 1); var light = new Light { glow = glow, point = point, phase = Rand(0, 8) }; lights.Add(light);
            LightField.Add(point + Vector2.up * .25f, 3.4f, 1f, new Color(1, .55f, .22f), light.phase);
            for (int i = 0; i < 4; i++)
            {
                var flame = ArtLibrary.Draw("Flame tongue", ArtLibrary.Fog, root, point + Vector2.up * .17f, 160, new Color(1, i == 0 ? .83f : .44f, .13f, .65f));
                flame.transform.localScale = new Vector3(.12f + i * .025f, .42f - i * .04f, 1); lights.Add(new Light { glow = flame, point = point + Vector2.up * .17f, phase = Rand(0, 6) });
            }
        }
        float Rand(float a, float b) => a + (float)rng.NextDouble() * (b - a);

        public void Tick(float dt, float clock, Vector2 camera)
        {
            // Her şey piksel ızgarasına oturur: kamera ızgaraya kilitli olduğundan kayarken pikseller titremez
            background.transform.position = PixelPipeline.Snap(new Vector2(camera.x - (camera.x - 25) * .09f, camera.y + .85f));
            haze.transform.position = PixelPipeline.Snap(camera);
            horizon.transform.position = PixelPipeline.Snap(new Vector2(camera.x, zone.floor + 1.9f));
            foreach (var p in particles)
            {
                p.position += p.velocity * dt;
                if (!p.mist)
                {
                    p.position.x += Mathf.Sin(clock * .6f + p.phase) * dt * .05f;
                    if (p.position.y > 5.6f) p.position.y = -3; if (p.position.y < -3.1f) p.position.y = 5.3f;
                }
                else if (p.position.x > 66) p.position.x = -9;
                p.transform.position = PixelPipeline.Snap(p.position);
            }
            float lift = camera.y - GameView.GroundCameraY;
            foreach (var l in foreground)
                l.transform.position = PixelPipeline.Snap(new Vector2(camera.x + (l.home.x - camera.x) * l.parallax, l.home.y - lift * (l.parallax - 1)));
            float shaftAlpha = zone.index == 0 ? .05f : zone.index == 1 ? .04f : .06f;
            for (int i = 0; i < shafts.Count; i++)
            { var c = shafts[i].color; c.a = shaftAlpha * (.7f + .3f * Mathf.Sin(clock * .35f + i * 1.7f)); shafts[i].color = c; }
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
