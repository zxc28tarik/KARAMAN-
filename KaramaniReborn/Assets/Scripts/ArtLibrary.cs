using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KaramaniReborn
{
    public enum PartId { Head, Torso, Hips, Cape, UpperArm, Forearm, FarUpperArm, FarForearm, Thigh, Shin, FarThigh, FarShin, Boot, FarBoot, Saber, Shield }

    // Sprites are rectangles into the original paintings. No resampling, morphing or mesh skinning.
    public static class ArtLibrary
    {
        static readonly Dictionary<string, Sprite[]> atlases = new();
        static readonly Dictionary<string, Sprite> single = new();
        static readonly Dictionary<Sprite, float> topInsets = new();
        public static Sprite White, Glow, Fog, Haze;

        public static void Initialize()
        {
            if (White) return;
            White = Procedural(4, (x, y) => Color.white);
            Glow = Procedural(96, (x, y) => { float d = new Vector2(x * 2 - 1, y * 2 - 1).magnitude; return new Color(1, 1, 1, Mathf.Pow(Mathf.Clamp01(1 - d), 3)); });
            // Uzaklık pusu: ufukta yoğun, gökyüzünde ve önde seyrek
            Haze = Procedural(64, (x, y) => new Color(1, 1, 1, .3f + .7f * Mathf.Exp(-Mathf.Pow((y - .47f) / .2f, 2))));
            Fog = Procedural(128, (x, y) => { float d = Mathf.Pow((x - .5f) * 2, 2) + Mathf.Pow((y - .5f) * 2, 2); return new Color(1, 1, 1, Mathf.Pow(Mathf.Clamp01(1 - d), 2) * Mathf.Lerp(.7f, 1, Mathf.PerlinNoise(x * 6, y * 6))); });
        }

        static Sprite Procedural(int size, Func<float, float, Color> pixel)
        {
            var t = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            var colors = new Color[size * size];
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++) colors[y * size + x] = pixel(x / (float)(size - 1), y / (float)(size - 1));
            t.SetPixels(colors); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, size, size), new Vector2(.5f, .5f), size, 0, SpriteMeshType.FullRect);
        }

        public static Sprite Background(int zone) => LoadWhole("background-" + zone, false);

        // Arka plan resmi ekrandaki piksel boyuna alan ortalamasıyla indirilir, sonra Kuwahara süzgeciyle
        // düz renk kümelerine ayrılır: elle piksellenmiş gibi temiz kümeler, karıncalanma yok.
        // Her doku pikseli tam bir oyun pikseli; boyutlar çift sayı, böylece merkez piksel ızgarasına oturur.
        // Uzak manzaranın dünya yüksekliği: görüş yüksekliği + kaydırma payı
        public const float BackgroundHeight = GameView.OrthographicSize * 2 + 1.6f;

        public static Sprite PixelBackground(int zone, float worldHeight)
        {
            string key = "pixel-background-" + zone;
            if (single.TryGetValue(key, out var found)) return found;
            var source = Background(zone).texture;
            int sw = source.width, sh = source.height;
            int h = Mathf.RoundToInt(worldHeight / PixelPipeline.PixelSize / 2) * 2;
            int w = Mathf.RoundToInt(sw * h / (float)sh / 2) * 2;
            var src = source.GetPixels32(); var small = new Color[w * h];
            for (int y = 0; y < h; y++)
            {
                int y0 = y * sh / h, y1 = Math.Max(y0 + 1, (y + 1) * sh / h);
                for (int x = 0; x < w; x++)
                {
                    int x0 = x * sw / w, x1 = Math.Max(x0 + 1, (x + 1) * sw / w);
                    float r = 0, g = 0, b = 0; int n = 0;
                    for (int yy = y0; yy < y1; yy++) for (int xx = x0; xx < x1; xx++) { var c = src[yy * sw + xx]; r += c.r; g += c.g; b += c.b; n++; }
                    small[y * w + x] = new Color(r / n / 255f, g / n / 255f, b / n / 255f, 1);
                }
            }
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = key };
            tex.SetPixels(Kuwahara(small, w, h, 2)); tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(.5f, .5f), 1 / PixelPipeline.PixelSize, 0, SpriteMeshType.FullRect);
            sprite.name = key; return single[key] = sprite;
        }

        // Menüdeki bölge resmi: oyundaki gibi piksellenmiş, renk ayarlı ve bölge paletine oturtulmuş
        public static Sprite MenuPainting(int zone)
        {
            string key = "menu-painting-" + zone;
            if (single.TryGetValue(key, out var found)) return found;
            var pixel = PixelBackground(zone, BackgroundHeight).texture;
            var tex = PixelLook.For(zone).Grade(pixel.GetPixels(), pixel.width, pixel.height);
            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect);
            sprite.name = key; return single[key] = sprite;
        }

        // Her piksel için dört komşu pencereden parlaklığı en düzgün olanın ortalaması alınır;
        // kenarlar korunur, yüzeyler düzleşir. Özet alan tablolarıyla her pencere sabit sürede.
        static Color[] Kuwahara(Color[] src, int w, int h, int r)
        {
            int stride = w + 1; var tables = new double[5][];
            for (int t = 0; t < 5; t++) tables[t] = new double[stride * (h + 1)];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    var c = src[y * w + x]; double l = .299 * c.r + .587 * c.g + .114 * c.b;
                    double[] v = { c.r, c.g, c.b, l, l * l };
                    int i = (y + 1) * stride + x + 1;
                    for (int t = 0; t < 5; t++) tables[t][i] = v[t] + tables[t][i - stride] + tables[t][i - 1] - tables[t][i - stride - 1];
                }
            double Sum(int t, int x0, int y0, int x1, int y1) => tables[t][(y1 + 1) * stride + x1 + 1] - tables[t][y0 * stride + x1 + 1] - tables[t][(y1 + 1) * stride + x0] + tables[t][y0 * stride + x0];
            var result = new Color[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    double best = double.MaxValue; Color pick = src[y * w + x];
                    for (int q = 0; q < 4; q++)
                    {
                        int x0 = Math.Max(0, (q & 1) == 0 ? x - r : x), x1 = Math.Min(w - 1, (q & 1) == 0 ? x : x + r);
                        int y0 = Math.Max(0, (q & 2) == 0 ? y - r : y), y1 = Math.Min(h - 1, (q & 2) == 0 ? y : y + r);
                        double n = (x1 - x0 + 1) * (y1 - y0 + 1), mean = Sum(3, x0, y0, x1, y1) / n, variance = Sum(4, x0, y0, x1, y1) / n - mean * mean;
                        if (variance < best) { best = variance; pick = new Color((float)(Sum(0, x0, y0, x1, y1) / n), (float)(Sum(1, x0, y0, x1, y1) / n), (float)(Sum(2, x0, y0, x1, y1) / n), 1); }
                    }
                    result[y * w + x] = pick;
                }
            return result;
        }

        static readonly Dictionary<int, Sprite> silhouettes = new();
        // Ön plan siluetleri elle piksel piksel üretilir: bir doku pikseli tam bir oyun pikseline denk gelir.
        // 0 kuru ot, 1 moloz, 2 demir parmaklık, 3 asılı zincir
        public static Sprite Silhouette(int kind, int seed)
        {
            int key = kind * 1000 + seed % 1000;
            if (silhouettes.TryGetValue(key, out var found)) return found;
            var rng = new System.Random(seed * 977 + kind);
            int w = kind switch { 0 => 44, 1 => 60, 2 => 72, _ => 8 }, h = kind switch { 0 => 34, 1 => 24, 2 => 46, _ => 70 + rng.Next(30) };
            var a = new bool[w * h];
            void Set(int x, int y) { if (x >= 0 && x < w && y >= 0 && y < h) a[y * w + x] = true; }
            switch (kind)
            {
                case 0:
                    for (int b = 0; b < 26; b++)
                    {
                        float x = 4 + (float)rng.NextDouble() * (w - 8), lean = ((float)rng.NextDouble() - .5f) * .9f; int len = 8 + rng.Next(h - 8);
                        for (int y = 0; y < len; y++) { float t = y / (float)len; int bx = Mathf.RoundToInt(x + lean * y * t * 1.2f); Set(bx, y); if (t < .35f) Set(bx + 1, y); }
                    }
                    for (int x = 0; x < w; x++) for (int y = 0; y < 3 + (int)(2 * Mathf.Sin(x * .3f)); y++) Set(x, y);
                    break;
                case 1:
                    for (int x = 0; x < w; x++)
                    {
                        float t = x / (float)(w - 1), top = (Mathf.Sin(t * Mathf.PI) * .8f + .2f) * (h - 4) * (.75f + .25f * Mathf.PerlinNoise(x * .21f, seed * .37f));
                        for (int y = 0; y < top; y++) Set(x, y);
                    }
                    for (int s = 0; s < 4; s++)
                    {
                        int cx = 8 + rng.Next(w - 16), cy = 6 + rng.Next(h - 10), r = 2 + rng.Next(4);
                        for (int y = -r; y <= r; y++) for (int x = -r - 1; x <= r + 1; x++) if (x * x * .7f + y * y <= r * r) Set(cx + x, cy + y);
                    }
                    break;
                case 2:
                    for (int x = 0; x < w; x++) { for (int y = 0; y < 3; y++) Set(x, y); Set(x, 12); Set(x, h - 14); Set(x, h - 13); }
                    for (int x = 3; x < w - 2; x += 7)
                    {
                        int top = h - 6 - x / 7 % 2 * 3;
                        for (int y = 0; y <= top + 2; y++) Set(x, y);
                        Set(x - 1, top - 1); Set(x + 1, top - 1); Set(x - 1, top - 2); Set(x + 1, top - 2);
                    }
                    break;
                default:
                    for (int y = 0; y < h; y++)
                    {
                        int k = y % 5;
                        if (y / 5 % 2 == 0) { if (k == 0 || k == 4) { Set(3, y); Set(4, y); } else { Set(2, y); Set(5, y); } }
                        else { Set(3, y); Set(4, y); }
                    }
                    break;
            }
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var colors = new Color32[w * h]; for (int i = 0; i < colors.Length; i++) colors[i] = new Color32(255, 255, 255, a[i] ? (byte)255 : (byte)0);
            tex.SetPixels32(colors); tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, w, h), kind == 3 ? new Vector2(.5f, 1) : new Vector2(.5f, 0), 1 / PixelPipeline.PixelSize, 0, SpriteMeshType.FullRect);
            sprite.name = "Silhouette " + kind;
            return silhouettes[key] = sprite;
        }

        public static Sprite LoadWhole(string name, bool trim = true)
        {
            if (single.TryGetValue(name, out var found)) return found;
            var tex = Resources.Load<Texture2D>("Art/" + name);
            if (!tex) throw new InvalidOperationException("Missing painting: " + name);
            Rect rect = new Rect(0, 0, tex.width, tex.height);
            if (trim) rect = FindIslands(tex).OrderByDescending(r => r.width * r.height).First();
            var s = Sprite.Create(tex, rect, new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect);
            s.name = name; single[name] = s; return s;
        }

        public static Sprite[] Rig(bool guard)
        {
            string name = guard ? "guard-parts" : "hero-parts";
            if (atlases.TryGetValue(name, out var cached)) return cached;
            var tex = Resources.Load<Texture2D>("Art/" + name);
            if (!tex) throw new InvalidOperationException("Missing atlas: " + name);
            var islands = FindIslands(tex);
            var result = new Sprite[16];
            for (int row = 0; row < 4; row++)
            {
                int r = row;
                var items = islands.Where(rect => Row(rect, tex.height) == r).OrderBy(rect => rect.center.x).ToArray();
                if (items.Length != 4) throw new InvalidOperationException($"{name}: row {row + 1} has {items.Length} parts, expected 4. " + string.Join(";", islands.Select(q => q.ToString())));
                for (int col = 0; col < 4; col++)
                {
                    int index = row * 4 + col;
                    Vector2 pivot = index switch { 0 => new Vector2(.68f, .23f), 1 => new Vector2(.50f, .10f), 2 => new Vector2(.5f, .9f), 3 => new Vector2(.52f, .98f), 12 or 13 => new Vector2(.3f, .80f), 14 => new Vector2(.16f, .52f), 15 => new Vector2(.5f, .55f), _ => new Vector2(.5f, .90f) };
                    var s = Sprite.Create(tex, items[col], pivot, 100, 0, SpriteMeshType.FullRect);
                    s.name = name + "_" + (PartId)index; result[index] = s;
                }
            }
            atlases[name] = result; return result;
        }

        public static float FlatTopInset(Sprite sprite)
        {
            if (topInsets.TryGetValue(sprite, out float inset)) return inset;
            var texture = sprite.texture; var pixels = texture.GetPixels32(); var r = sprite.rect;
            int left = (int)(r.x + r.width * .015f), right = (int)(r.xMax - r.width * .015f);
            for (int y = (int)r.yMax - 1; y > r.yMax - r.height * .15f; y--)
            {
                int opaque = 0;
                for (int x = left; x <= right; x++) if (pixels[y * texture.width + x].a > 220) opaque++;
                if (opaque >= (right - left + 1) * .95f) { inset = r.yMax - y - 1; break; }
            }
            topInsets[sprite] = inset; return inset;
        }

        static int Row(Rect r, float height)
        {
            float fromTop = 1 - r.center.y / height;
            return fromTop < .285f ? 0 : fromTop < .535f ? 1 : fromTop < .79f ? 2 : 3;
        }

        public static Sprite[] Props()
        {
            const string name = "props";
            if (atlases.TryGetValue(name, out var found)) return found;
            var tex = Resources.Load<Texture2D>("Art/props");
            var islands = FindIslands(tex);
            var sprites = new List<Sprite>();
            for (int row = 0; row < 2; row++)
            {
                int r = row;
                foreach (var rect in islands.Where(q => (q.center.y > tex.height * .5f ? 0 : 1) == r).OrderBy(q => q.center.x))
                    sprites.Add(Sprite.Create(tex, rect, new Vector2(.5f, 0), 100, 0, SpriteMeshType.FullRect));
            }
            if (sprites.Count != 8) throw new InvalidOperationException("Props require eight separated objects; found " + sprites.Count);
            return atlases[name] = sprites.ToArray();
        }

        public static Sprite[] Gear()
        {
            const string name = "equipment";
            if (atlases.TryGetValue(name, out var found)) return found;
            var tex = Resources.Load<Texture2D>("Art/" + name);
            var items = FindIslands(tex).OrderByDescending(q => q.center.y > tex.height * .45f).ThenBy(q => q.center.x).ToArray();
            if (items.Length != 4) throw new InvalidOperationException("Equipment atlas requires four objects; found " + items.Length);
            var sprites = new Sprite[4];
            for (int i = 0; i < 4; i++) sprites[i] = Sprite.Create(tex, items[i], i == 0 ? new Vector2(.20f, .30f) : new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect);
            return atlases[name] = sprites;
        }

        // Connected-alpha bounding boxes preserve even irregular atlas placement and all original pixels.
        public static List<Rect> FindIslands(Texture2D tex)
        {
            var pixels = tex.GetPixels32(); int w = tex.width, h = tex.height;
            var visited = new bool[pixels.Length]; var queue = new int[pixels.Length]; var rects = new List<Rect>();
            for (int seed = 0; seed < pixels.Length; seed++)
            {
                if (visited[seed] || pixels[seed].a < 24) continue;
                int read = 0, write = 1, count = 0, minX = w, minY = h, maxX = 0, maxY = 0;
                queue[0] = seed; visited[seed] = true;
                while (read < write)
                {
                    int p = queue[read++], x = p % w, y = p / w; count++;
                    minX = Math.Min(minX, x); minY = Math.Min(minY, y); maxX = Math.Max(maxX, x); maxY = Math.Max(maxY, y);
                    if (x > 0) Add(p - 1); if (x < w - 1) Add(p + 1); if (y > 0) Add(p - w); if (y < h - 1) Add(p + w);
                }
                if (count > 1800)
                {
                    minX = Math.Max(0, minX - 2); minY = Math.Max(0, minY - 2); maxX = Math.Min(w - 1, maxX + 2); maxY = Math.Min(h - 1, maxY + 2);
                    rects.Add(new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1));
                }
                void Add(int p) { if (!visited[p] && pixels[p].a >= 24) { visited[p] = true; queue[write++] = p; } }
            }
            return rects;
        }

        public static SpriteRenderer Draw(string name, Sprite sprite, Transform parent, Vector2 position, int order, Color? color = null)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.localPosition = position;
            var renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.sortingOrder = order; renderer.color = color ?? Color.white;
            return renderer;
        }
    }
}
