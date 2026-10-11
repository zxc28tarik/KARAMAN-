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
        public static Sprite White, Glow, Fog;

        public static void Initialize()
        {
            if (White) return;
            White = Procedural(4, (x, y) => Color.white);
            Glow = Procedural(96, (x, y) => { float d = new Vector2(x * 2 - 1, y * 2 - 1).magnitude; return new Color(1, 1, 1, Mathf.Pow(Mathf.Clamp01(1 - d), 3)); });
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
