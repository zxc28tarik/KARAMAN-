using UnityEngine;

namespace KaramaniReborn
{
    // The Last Faith tarzı görüntü hattı.
    // Dünya 4 kat çözünürlükte ayrı bir kamerayla çizilir, 720x540 piksele kutu filtresiyle indirilir,
    // bölgenin sınırlı paletine oturtulur ve ekrana keskin büyütülür. Kamera piksel ızgarasına kilitlenir;
    // kalan alt-piksel kayma ekranda uygulanır, böylece kaydırma akıcı kalırken pikseller titremez.
    // Arayüz ayrı katmanda kalır ve ekran çözünürlüğünde, keskin çizilir.
    [ExecuteAlways]
    public sealed class PixelPipeline : MonoBehaviour
    {
        public const int Width = 720, Height = 540, Margin = 2, Supersample = 4, UILayer = 5;
        public static float PixelSize => GameView.OrthographicSize * 2 / Height;
        public static Vector2 Snap(Vector2 p) => new Vector2(Mathf.Round(p.x / PixelSize) * PixelSize, Mathf.Round(p.y / PixelSize) * PixelSize);

        public Camera display, world;
        public RenderTexture worldTarget, pixelTarget;
        public int PaletteCount { get; private set; }
        public static int Zone { get; private set; }   // etkin bölge görünümü (HD karakterlerin renk ayarı için)
        Material post, screen;
        static bool rendering;

        public static PixelPipeline Create(Camera display, Color background)
        {
            var go = new GameObject("Pixel screen", typeof(MeshFilter), typeof(MeshRenderer)); go.layer = UILayer;
            go.transform.SetParent(display.transform, false); go.transform.localPosition = new Vector3(0, 0, 1);
            var mesh = new Mesh { name = "Screen quad" };
            mesh.vertices = new[] { new Vector3(-.5f, -.5f), new Vector3(.5f, -.5f), new Vector3(-.5f, .5f), new Vector3(.5f, .5f) };
            mesh.uv = new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) };
            mesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
            mesh.bounds = new Bounds(Vector3.zero, Vector3.one);
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            var p = go.AddComponent<PixelPipeline>(); p.display = display; p.Setup(background); return p;
        }

        void Setup(Color background)
        {
            int w = Width + 2 * Margin, h = Height + 2 * Margin;
            worldTarget = new RenderTexture(w * Supersample, h * Supersample, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) { name = "Painted world", filterMode = FilterMode.Bilinear, antiAliasing = 1 };
            pixelTarget = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) { name = "Pixel world", filterMode = FilterMode.Bilinear };
            worldTarget.Create(); pixelTarget.Create();
            var cam = new GameObject("Pixel world camera", typeof(Camera)); cam.transform.SetParent(display.transform.parent, false);
            world = cam.GetComponent<Camera>(); world.enabled = false; world.orthographic = true; world.cullingMask = ~(1 << UILayer);
            world.clearFlags = CameraClearFlags.SolidColor; world.backgroundColor = background; world.nearClipPlane = display.nearClipPlane; world.farClipPlane = display.farClipPlane;
            world.allowMSAA = false; world.allowHDR = false; world.targetTexture = worldTarget;
            display.cullingMask = 1 << UILayer;
            post = new Material(Load("PixelPost")); screen = new Material(Load("PixelScreen")) { mainTexture = pixelTarget };
            GetComponent<MeshRenderer>().sharedMaterial = screen;
            Shader.SetGlobalFloat("_CharacterVivid", 1.75f); Shader.SetGlobalFloat("_CharacterContrast", 1.2f); Shader.SetGlobalFloat("_CharacterRim", 1.35f);
            SetLook(0);
        }

        static Shader Load(string name)
        {
            var shader = Resources.Load<Shader>("Shaders/" + name);
            if (!shader) throw new System.InvalidOperationException("Missing shader: " + name);
            return shader;
        }

        public void SetLook(int zone)
        {
            Zone = zone; var look = PixelLook.For(zone);
            var colors = look.Palette(out var bias);
            var lab = new Vector4[64]; var rgb = new Vector4[64];
            for (int i = 0; i < colors.Length; i++) { var c = colors[i].linear; lab[i] = Oklab(c); lab[i].w = bias[i]; rgb[i] = new Vector4(c.r, c.g, c.b, 1); }
            PaletteCount = colors.Length;
            post.SetVectorArray("_PaletteLab", lab); post.SetVectorArray("_PaletteRGB", rgb); post.SetFloat("_PaletteCount", PaletteCount);
            post.SetVector("_Grade", look.grade);
            post.SetVector("_ShadowTint", look.shadowTint); post.SetVector("_LightTint", look.lightTint);
            post.SetVector("_Look", new Vector4(look.dither, look.chroma, look.vignette, Supersample));
        }

        void OnWillRenderObject()
        {
            if (rendering || Camera.current != display) return;
            Render();
        }

        // Ekran kamerası çizilmeden hemen önce dünyayı piksel hedefine çizer (yakalamalarda da çalışır)
        public void Render()
        {
            rendering = true;
            try
            {
                float pixel = PixelSize;
                Vector3 p = display.transform.position;
                float sx = Mathf.Round(p.x / pixel), sy = Mathf.Round(p.y / pixel);
                world.transform.SetPositionAndRotation(new Vector3(sx * pixel, sy * pixel, p.z), display.transform.rotation);
                world.orthographicSize = GameView.OrthographicSize * (Height + 2f * Margin) / Height;
                world.aspect = (Width + 2f * Margin) / (Height + 2f * Margin);
                world.Render();
                post.SetVector("_Origin", new Vector4(sx, sy, pixelTarget.width, pixelTarget.height));
                var active = RenderTexture.active;
                Graphics.Blit(worldTarget, pixelTarget, post);
                RenderTexture.active = active;
                // Vuruştaki yakınlaşma pikselleri büyütür; piksel ızgarası hiç değişmez
                float zoom = display.orthographicSize / GameView.OrthographicSize;
                screen.SetVector("_View", new Vector4(Height * display.aspect * zoom, Height * zoom, p.x / pixel - sx, p.y / pixel - sy));
            }
            finally { rendering = false; }
        }

        internal static Vector4 Oklab(Color c)
        {
            float l = .4122214708f * c.r + .5363325363f * c.g + .0514459929f * c.b;
            float m = .2119034982f * c.r + .6806995451f * c.g + .1073969566f * c.b;
            float s = .0883024619f * c.r + .2817188376f * c.g + .6299787005f * c.b;
            l = Mathf.Pow(Mathf.Max(l, 1e-6f), 1f / 3); m = Mathf.Pow(Mathf.Max(m, 1e-6f), 1f / 3); s = Mathf.Pow(Mathf.Max(s, 1e-6f), 1f / 3);
            return new Vector4(.2104542553f * l + .7936177850f * m - .0040720468f * s, 1.9779984951f * l - 2.4285922050f * m + .4505937099f * s, .0259040371f * l + .7827717662f * m - .8086757660f * s, 0);
        }

        void OnDestroy()
        {
            if (worldTarget) worldTarget.Release();
            if (pixelTarget) pixelTarget.Release();
        }
    }

    // Bölge görünümü: tek renk ailesinde ana rampa (gölgede soğuk, ışıkta sıcak kayan ton) ve
    // ortak vurgu rampaları. Sahne bu paletin dışında renk üretemez.
    public sealed class PixelLook
    {
        public string[] ramp;
        public Vector4 grade;            // pozlama, doygunluk, kontrast, kontrast merkezi
        public Color shadowTint = Color.white, lightTint = Color.white;
        public float dither = .016f, chroma = 1.1f, vignette = .35f;

        // Vurgu rampaları ve seçilme cezaları: renksiz pikseller ana rampada kalır,
        // yalnız gerçekten renkli yerler (ateş, kan, sancak, altın) vurguya geçer
        static readonly (string hex, float bias)[] Accents =
        {
            ("2b1a17", .0015f), ("553329", .0015f), ("8a5a44", .0015f), ("b9856a", .0015f), ("dcb294", .0015f),           // ten
            ("1c0709", .0008f), ("3d0d12", .0008f), ("6b1419", .0008f), ("9c2423", .0008f), ("c94335", .0008f),           // kızıl kumaş, kan
            ("2e2310", .0025f), ("5e4520", .0025f), ("93702f", .0025f), ("c69e4f", .0025f), ("ecd38f", .0025f),           // pirinç, altın
            ("5a1606", .002f), ("9a330a", .002f), ("d9611a", .002f), ("f7983a", .002f), ("ffd27a", .002f), ("fff4d6", .002f), // ateş
            ("1f1814", .003f), ("3a2c24", .003f), ("574236", .003f), ("7a5c48", .003f),                                     // ahşap
        };

        public static PixelLook For(int zone) => zone switch
        {
            // Kızıl Surlar: gün batımı, gül-mor gökyüzü
            0 => new PixelLook
            {
                ramp = new[] { "07050a", "120b16", "1e1325", "2b1b33", "3b2442", "4d2f50", "623b5c", "7a4a66", "955c70", "b2737e", "cd9592", "e8bcb0" },
                grade = new Vector4(.92f, .5f, 1.22f, .38f), shadowTint = new Color(.9f, .84f, 1.08f), lightTint = new Color(1.08f, .94f, .98f)
            },
            // Yanmış Mahalle: kömür ve kül, uzakta yangın
            1 => new PixelLook
            {
                ramp = new[] { "060607", "0e0d0f", "181618", "242022", "322c2c", "413937", "534844", "675a53", "7f6f66", "9a887c", "b8a597", "d8c8b8" },
                grade = new Vector4(.86f, .55f, 1.2f, .38f), shadowTint = new Color(.9f, .9f, 1.02f), lightTint = new Color(1.05f, .98f, .94f)
            },
            // Kurt Geçidi: ay ışığı, mavi-yeşil
            2 => new PixelLook
            {
                ramp = new[] { "04070a", "0a1117", "111c25", "1a2834", "243645", "304657", "3e586b", "506d80", "668597", "829fae", "a6bfca", "d0e0e6" },
                grade = new Vector4(.86f, .55f, 1.18f, .38f), shadowTint = new Color(.84f, .92f, 1.05f), lightTint = new Color(.98f, 1f, 1.02f)
            },
            // Karanlık Kale: kar, soluk lavanta-gri
            _ => new PixelLook
            {
                ramp = new[] { "06070c", "0e1019", "181b28", "242838", "32374a", "42485d", "545b72", "6a7289", "838ca2", "a1aabd", "c3cad8", "e6ebf2" },
                grade = new Vector4(.9f, .48f, 1.18f, .42f), shadowTint = new Color(.88f, .88f, 1.06f), lightTint = new Color(1f, 1f, 1.02f)
            },
        };

        // Görüntü hattının renk ayarı ve palete oturtmasının işlemci karşılığı (menü resimleri için)
        public Texture2D Grade(Color[] pixels, int width, int height)
        {
            var colors = Palette(out var bias); var labs = new Vector4[colors.Length];
            for (int i = 0; i < colors.Length; i++) labs[i] = PixelPipeline.Oklab(colors[i].linear);
            var result = new Color[pixels.Length];
            for (int p = 0; p < pixels.Length; p++)
            {
                Vector3 g = new Vector3(pixels[p].r, pixels[p].g, pixels[p].b) * grade.x;
                float luma = Vector3.Dot(g, new Vector3(.299f, .587f, .114f));
                float colorfulness = Mathf.Max(g.x, Mathf.Max(g.y, g.z)) - Mathf.Min(g.x, Mathf.Min(g.y, g.z));
                float keep = Smooth(.42f, .72f, colorfulness) * Smooth(.55f, .88f, luma);
                g = Vector3.LerpUnclamped(Vector3.one * luma, g, Mathf.Lerp(grade.y, 1.1f, keep));
                g = (g - Vector3.one * grade.w) * grade.z + Vector3.one * grade.w;
                luma = Mathf.Clamp01(Vector3.Dot(g, new Vector3(.299f, .587f, .114f)));
                var tint = Color.Lerp(shadowTint, lightTint, Smooth(.04f, .7f, luma));
                var lab = PixelPipeline.Oklab(new Color(Mathf.Clamp01(g.x * tint.r), Mathf.Clamp01(g.y * tint.g), Mathf.Clamp01(g.z * tint.b)).linear);
                float best = float.MaxValue; int pick = 0;
                for (int k = 0; k < labs.Length; k++)
                {
                    Vector4 e = lab - labs[k];
                    float d = e.x * e.x + chroma * (e.y * e.y + e.z * e.z) + bias[k];
                    if (d < best) { best = d; pick = k; }
                }
                result[p] = colors[pick];
            }
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            tex.SetPixels(result); tex.Apply(); return tex;
        }

        // Yalnız renk ayarı (PixelPost ile aynı sıra), palete oturtma yok: ekran çözünürlüğündeki HD karakterler için
        public Color32[] GradeOnly(Color32[] pixels)
        {
            var result = new Color32[pixels.Length];
            for (int p = 0; p < pixels.Length; p++)
            {
                var px = pixels[p];
                if (px.a == 0) { result[p] = px; continue; }
                Vector3 g = new Vector3(px.r, px.g, px.b) / 255f * grade.x;
                float luma = Vector3.Dot(g, new Vector3(.299f, .587f, .114f));
                float colorfulness = Mathf.Max(g.x, Mathf.Max(g.y, g.z)) - Mathf.Min(g.x, Mathf.Min(g.y, g.z));
                float keep = Smooth(.42f, .72f, colorfulness) * Smooth(.55f, .88f, luma);
                g = Vector3.LerpUnclamped(Vector3.one * luma, g, Mathf.Lerp(grade.y, 1.1f, keep));
                g = (g - Vector3.one * grade.w) * grade.z + Vector3.one * grade.w;
                luma = Mathf.Clamp01(Vector3.Dot(g, new Vector3(.299f, .587f, .114f)));
                var tint = Color.Lerp(shadowTint, lightTint, Smooth(.04f, .7f, luma));
                result[p] = new Color32((byte)(Mathf.Clamp01(g.x * tint.r) * 255 + .5f), (byte)(Mathf.Clamp01(g.y * tint.g) * 255 + .5f), (byte)(Mathf.Clamp01(g.z * tint.b) * 255 + .5f), px.a);
            }
            return result;
        }

        static float Smooth(float a, float b, float x) { float t = Mathf.Clamp01((x - a) / (b - a)); return t * t * (3 - 2 * t); }

        public Color[] Palette(out float[] bias)
        {
            var all = new Color[ramp.Length + Accents.Length]; bias = new float[all.Length];
            for (int i = 0; i < all.Length; i++)
            {
                string hex = i < ramp.Length ? ramp[i] : Accents[i - ramp.Length].hex;
                bias[i] = i < ramp.Length ? 0 : Accents[i - ramp.Length].bias;
                ColorUtility.TryParseHtmlString("#" + hex, out all[i]);
            }
            return all;
        }
    }
}
