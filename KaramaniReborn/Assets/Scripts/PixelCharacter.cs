using System;
using System.Collections.Generic;
using UnityEngine;

namespace KaramaniReborn
{
    // Tek parça karakter: her kare, karakterin baştan sona tek bir çizimidir. Kol, bacak, gövde ayrı
    // parçalar olarak hareket etmez; çizim döndürülmez ya da esnetilmez. Kareleri Tools/pixel_characters.py
    // üretir (Assets/Resources/Characters/<tür>.png + .json). Burada yalnız oyun durumundan klip ve kare
    // seçilir, sprite oyun pikseli ızgarasına oturtulur; ışık, vurulma parlaması ve kılıç izi eklenir.
    // HD kareler (Resources/Characters/hd/<tür>, Tools/hd_sheet.py, 100 piksel = 1 birim) varsa o kliplerde
    // onlar kullanılır: karakter piksel ekrandan sonra ekran çözünürlüğünde, keskin çizilir (UILayer);
    // HD'de olmayan klipler piksel sayfasından gelir.
    public sealed class PixelCharacter
    {
#pragma warning disable CS0649 // JsonUtility doldurur
        [Serializable] sealed class FrameData { public float[] bladeRoot = new float[2], bladeTip = new float[2]; public float plantedN = -999, plantedF = -999; }
        [Serializable] sealed class ClipData { public string name; public int start, count; public float[] bounds = new float[0]; public float stride; }
        [Serializable] sealed class SheetData { public int frameWidth, frameHeight, pivotX, pivotY, columns; public float pixelsPerUnit; public FrameData[] frames; public ClipData[] clips; }
#pragma warning restore CS0649

        sealed class Sheet
        {
            public SheetData data; public Texture2D texture; public Sprite[] sprites;
            public float[] top, bottom;   // karenin görünen üst / alt sınırı, dayanaktan oyun pikseli
            public readonly Dictionary<string, ClipData> clips = new();
        }
        static readonly Dictionary<string, Sheet> sheets = new();

        // Düşman türlerinde çizilmemiş klipler için en yakın karşılık (oyuncuda hiç kullanılmamalı)
        static readonly Dictionary<string, string[]> Fallbacks = new()
        {
            ["light2"] = new[] { "light1" }, ["light3"] = new[] { "light1" }, ["heavy"] = new[] { "light1" }, ["spirit"] = new[] { "light1" },
            ["parry"] = new[] { "block" }, ["dive"] = new[] { "fall" }, ["flip"] = new[] { "rise" }, ["climb"] = new[] { "rise" }, ["hang"] = new[] { "fall" },
            ["roll"] = new[] { "hurt" }, ["backdash"] = new[] { "hurt" }, ["heal"] = new[] { "idle" }, ["skid"] = new[] { "idle" },
            ["land"] = new[] { "idle" }, ["apex"] = new[] { "fall", "rise" }, ["walk"] = new[] { "run" }, ["run"] = new[] { "walk" }
        };
        // HD sayfada ayrı çizilmemiş, aynı hareketle gösterilen klipler (yedek sayılmaz: tasarım gereği)
        static readonly Dictionary<string, string> HdAliases = new()
        {
            ["light2"] = "light1", ["light3"] = "light1", ["dive"] = "fall", ["backdash"] = "hurt", ["skid"] = "idle"
        };
        public static readonly string[] Variants = { "hero", "duelist", "shield", "archer", "captain", "ghoul" };

        enum Timing { Progress, Distance, Loop }

        public readonly Transform root;
        public readonly BladeTrail trail;
        public readonly string variant;
        readonly SpriteRenderer body, shadow;
        readonly Sheet pixel;
        Sheet hd; int hdZone = -1;
        Sheet sheet;
        readonly MaterialPropertyBlock block = new();
        readonly Color trailColor;
        static Material material;
        static readonly int FlashId = Shader.PropertyToID("_Flash"), RimColorId = Shader.PropertyToID("_RimColor"), RimDirId = Shader.PropertyToID("_RimDir"),
            RimWidthId = Shader.PropertyToID("_RimWidth"), RimStrengthId = Shader.PropertyToID("_RimStrength"), LightColorId = Shader.PropertyToID("_LightColor"),
            LightAmountId = Shader.PropertyToID("_LightAmount"), MoonId = Shader.PropertyToID("_MoonStrength"),
            VividId = Shader.PropertyToID("_CharacterVivid"), ContrastId = Shader.PropertyToID("_CharacterContrast"), CharacterRimId = Shader.PropertyToID("_CharacterRim");

        string clip = ""; int frame = -1, face = 1;
        float clipTime, phase, landTimer, airTime, previousX;
        bool first = true;
        Vector2 origin;
        // Basan ayaklar: karenin başladığı anda çizilen ayağın dünyadaki yeri ölçülür
        readonly float[] anchor = { float.NaN, float.NaN };

        public int Steps { get; private set; }
        public float MaxFootSlip { get; private set; }
        public int PlantedSamples { get; private set; }
        public int FallbackFrames { get; private set; }
        public bool HasHd => hd != null;
        public bool HdActive => sheet == hd;
        public string Clip => clip;
        public int Frame => frame;
        float Ppu => sheet.data.pixelsPerUnit;
        FrameData Current => sheet.data.frames[frame];
        public Vector2 BladeRoot => World(Current.bladeRoot);
        public Vector2 BladeTip => World(Current.bladeTip);
        // Vuruş çizgisi gövdeden (el yüksekliğinde) kılıç ucuna uzanır: uzanan kol da darbenin parçasıdır,
        // bu yüzden kombonun ilerleyişiyle dibe giren rakip ıskalanmaz
        public Vector2 StrikeRoot => origin + new Vector2(0, Current.bladeRoot[1] / Ppu);
        // Görünen boy (dünya birimi); doku okunamıyorsa NaN
        public float VisibleHeight => (sheet.top[frame] - sheet.bottom[frame]) / Ppu;
        public bool ScalePreserved => Mathf.Abs(Mathf.Abs(root.localScale.x) - 1) < 1e-5f && Mathf.Abs(root.localScale.y - 1) < 1e-5f && root.localRotation == Quaternion.identity;
        public bool OnPixelGrid
        {
            get { float s = PixelPipeline.PixelSize; return Mathf.Abs(origin.x / s - Mathf.Round(origin.x / s)) < 1e-3f && Mathf.Abs(origin.y / s - Mathf.Round(origin.y / s)) < 1e-3f; }
        }

        public PixelCharacter(Actor actor, Transform parent)
        {
            variant = actor.player ? "hero" : actor.role.ToString().ToLowerInvariant();
            pixel = Load(variant, true); hd = TryLoad("hd/" + variant); sheet = pixel;
            var go = new GameObject(actor.player ? "Karamani · Pixel Character" : actor.role + " · Pixel Character");
            root = go.transform; root.SetParent(parent, false);
            body = go.AddComponent<SpriteRenderer>(); body.sortingOrder = actor.player ? 142 : 140;
            if (!material) material = new Material(Resources.Load<Shader>("Shaders/PaintedSprite")) { name = "Pixel character" };
            body.sharedMaterial = material;
            shadow = ArtLibrary.Draw("Contact shadow", ArtLibrary.Glow, parent, actor.position, 59, new Color(0, 0, 0, .75f));
            shadow.transform.localScale = new Vector3(1.5f * actor.size, .2f, 1);
            trailColor = actor.player ? new Color(1f, .86f, .62f, .85f) : new Color(1f, .42f, .26f, .5f);
            trail = new BladeTrail(parent, trailColor, 175);
            actor.view = this;
            Render(actor, 0, 0);
        }

        public void Destroy()
        {
            if (Application.isPlaying) { UnityEngine.Object.Destroy(root.gameObject); UnityEngine.Object.Destroy(shadow.gameObject); }
            else { UnityEngine.Object.DestroyImmediate(root.gameObject); UnityEngine.Object.DestroyImmediate(shadow.gameObject); }
            trail.Destroy();
        }

        public static bool HasClip(string variant, string name) => Load(variant, true).clips.ContainsKey(name);

        static Sheet TryLoad(string variant) => Resources.Load<TextAsset>("Characters/" + variant) ? Load(variant, false) : null;

        // HD sayfa piksel ekranın renk ayarından geçmez: bölgenin ayarı (pozlama, doygunluk, kontrast, ton) dokuya
        // bir kez uygulanır, palete oturtulmaz. Bölge başına önbelleklenir.
        static Sheet Graded(Sheet source, string variant, int zone)
        {
            string key = variant + "@" + zone;
            if (sheets.TryGetValue(key, out var cached) && cached.texture) return cached;
            // yalnız etkin bölgenin kopyası tutulur (HD sayfa büyük: her kopya ~30 MB)
            foreach (var old in new List<string>(sheets.Keys))
                if (old.StartsWith(variant + "@"))
                {
                    var o = sheets[old]; sheets.Remove(old);
                    foreach (var sp in o.sprites) if (sp) Release(sp);
                    if (o.texture) Release(o.texture);
                }
            var src = source.texture;
            var tex = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = key };
            tex.SetPixels32(PixelLook.For(zone).GradeOnly(src.GetPixels32())); tex.Apply(false, true);
            var s = new Sheet { data = source.data, texture = tex, top = source.top, bottom = source.bottom, sprites = new Sprite[source.sprites.Length] };
            for (int i = 0; i < s.sprites.Length; i++)
            {
                var o = source.sprites[i];
                s.sprites[i] = Sprite.Create(tex, o.rect, new Vector2(o.pivot.x / o.rect.width, o.pivot.y / o.rect.height), o.pixelsPerUnit, 0, SpriteMeshType.FullRect);
                s.sprites[i].name = o.name;
            }
            foreach (var c in source.clips) s.clips[c.Key] = c.Value;
            return sheets[key] = s;
        }

        static Sheet Load(string variant, bool gameGrid)
        {
            if (sheets.TryGetValue(variant, out var cached) && cached.texture) return cached;
            var json = Resources.Load<TextAsset>("Characters/" + variant);
            var tex = Resources.Load<Texture2D>("Characters/" + variant);
            if (!json || !tex) throw new InvalidOperationException("Character sheet missing: Resources/Characters/" + variant);
            tex.filterMode = FilterMode.Point; tex.wrapMode = TextureWrapMode.Clamp;
            var s = new Sheet { data = JsonUtility.FromJson<SheetData>(json.text), texture = tex };
            var d = s.data; int n = d.frames.Length;
            if (gameGrid && Mathf.Abs(d.pixelsPerUnit * PixelPipeline.PixelSize - 1) > .001f) throw new InvalidOperationException(variant + ": sprite pixels do not match the game-pixel grid");
            if (tex.width % d.frameWidth != 0 || tex.height < (n + d.columns - 1) / d.columns * d.frameHeight) throw new InvalidOperationException(variant + ": sheet size does not match its frame data");
            s.sprites = new Sprite[n]; s.top = new float[n]; s.bottom = new float[n];
            Color32[] pixels = tex.isReadable ? tex.GetPixels32() : null;
            var pivot = new Vector2(d.pivotX / (float)d.frameWidth, d.pivotY / (float)d.frameHeight);
            for (int i = 0; i < n; i++)
            {
                int x0 = i % d.columns * d.frameWidth, y0 = tex.height - (i / d.columns + 1) * d.frameHeight;
                s.sprites[i] = Sprite.Create(tex, new Rect(x0, y0, d.frameWidth, d.frameHeight), pivot, d.pixelsPerUnit, 0, SpriteMeshType.FullRect);
                s.sprites[i].name = variant + " " + i;
                s.top[i] = s.bottom[i] = float.NaN;
                if (pixels == null) continue;
                for (int y = d.frameHeight - 1; y >= 0 && float.IsNaN(s.top[i]); y--) if (RowHasInk(pixels, tex.width, x0, y0 + y, d.frameWidth)) s.top[i] = y + 1 - d.pivotY;
                for (int y = 0; y < d.frameHeight && float.IsNaN(s.bottom[i]); y++) if (RowHasInk(pixels, tex.width, x0, y0 + y, d.frameWidth)) s.bottom[i] = y - d.pivotY;
            }
            foreach (var c in d.clips) s.clips[c.name] = c;
            sheets[variant] = s;
            return s;
        }

        static void Release(UnityEngine.Object x) { if (Application.isPlaying) UnityEngine.Object.Destroy(x); else UnityEngine.Object.DestroyImmediate(x); }

        static bool RowHasInk(Color32[] pixels, int width, int x0, int y, int count)
        { for (int x = x0; x < x0 + count; x++) if (pixels[y * width + x].a > 8) return true; return false; }

        Vector2 World(float[] p) => origin + new Vector2(p[0] * face, p[1]) / Ppu;

        public void ResetContacts() { first = true; anchor[0] = anchor[1] = float.NaN; }

        public void Render(Actor actor, float dt, float clock)
        {
            if (first || face != actor.face) { previousX = actor.position.x; anchor[0] = anchor[1] = float.NaN; first = false; }
            face = actor.face;
            float dx = actor.position.x - previousX; previousX = actor.position.x;
            // İniş: yalnız gerçek bir havada kalıştan sonra kısa çökme karesi
            if (!actor.grounded) airTime += dt;
            else { if (airTime > .12f) landTimer = .1f; airTime = 0; }
            landTimer = Mathf.Max(0, landTimer - dt);

            if (hd != null && hdZone != PixelPipeline.Zone && hd.texture.isReadable)
            {
                bool active = sheet == hd;
                hd = Graded(TryLoad("hd/" + variant), "hd/" + variant, hdZone = PixelPipeline.Zone);
                if (active) sheet = hd;
            }
            string wanted = Pick(actor, out var timing, out float value);
            string name;
            var use = pixel;
            if (hd != null && (hd.clips.ContainsKey(wanted) || HdAliases.TryGetValue(wanted, out var alias) && hd.clips.ContainsKey(alias)))
            { use = hd; name = hd.clips.ContainsKey(wanted) ? wanted : HdAliases[wanted]; }
            else { name = Resolve(wanted); if (name != wanted) FallbackFrames++; }
            if (use != sheet)
            {
                sheet = use; frame = -1;
                root.gameObject.layer = sheet == hd ? PixelPipeline.UILayer : 0;
            }
            if (name != clip || frame < 0) { clip = name; clipTime = 0; anchor[0] = anchor[1] = float.NaN; if (timing != Timing.Distance) phase = 0; }
            else clipTime += dt;
            var c = sheet.clips[clip];
            float strideUnits = Mathf.Max(.01f, c.stride / Ppu);
            if (timing == Timing.Distance) phase = Mathf.Repeat(phase + Mathf.Abs(dx) / strideUnits, 1);
            int index = timing switch
            {
                Timing.Distance => Mathf.Min(c.count - 1, (int)(phase * c.count)),
                Timing.Loop => (int)(clipTime * value) % c.count,
                _ => ByProgress(c, value)
            };
            int next = c.start + index;
            if (next != frame) { frame = next; TrackFeet(actor, timing, c, index, strideUnits, Mathf.Sign(dx)); }
            if (timing != Timing.Distance) anchor[0] = anchor[1] = float.NaN;

            // HD asılma / tırmanma kareleri kenar noktasına göre çizildi: karakter kenarın köşesinde durur
            bool ledgeClip = sheet == hd && (clip == "hang" || clip == "climb");
            origin = PixelPipeline.Snap(ledgeClip ? actor.ledge : actor.position);
            root.position = origin;
            root.localScale = new Vector3(face, 1, 1);
            root.localRotation = Quaternion.identity;
            body.sprite = sheet.sprites[frame];
            Color tint = Color.white;
            if (actor.Dead) tint = Color.Lerp(tint, new Color(.34f, .34f, .38f, .35f), Mathf.Clamp01(actor.stateTime / 3));
            body.color = tint;
            ApplyLight(actor);

            float p = actor.Progress;
            bool diving = actor.state == ActionState.Light && actor.combo == 4, heavy = actor.state == ActionState.Heavy;
            bool slash = actor.state is ActionState.Light or ActionState.Heavy;
            float windEnd = heavy ? .44f : .27f, strikeEnd = heavy ? .69f : .58f;
            trail.lifetime = heavy ? .17f : .12f;
            trail.color = actor.riposting ? new Color(1f, .74f, .3f, 1f) : trailColor;
            // HD karelerde savurma izi çizimin içinde (hareket lekesi); ek iz yalnız piksel karelerde
            trail.Step(Vector2.Lerp(BladeRoot, BladeTip, .3f), BladeTip, sheet != hd && slash && (diving || p > windEnd * .8f && p < strikeEnd + .07f), dt);
            shadow.transform.position = new Vector3(actor.position.x, actor.position.y + .005f, 0);
            shadow.color = new Color(0, 0, 0, actor.grounded ? .7f : .22f);
        }

        // Durum → klip. Saldırılar oyun zamanlamasıyla (Progress) birebir ilerler; yürüyüş katedilen
        // mesafeyle (adım uzunluğu) ilerler, böylece ayaklar zeminde kaymaz.
        string Pick(Actor a, out Timing timing, out float value)
        {
            timing = Timing.Progress; value = a.Progress;
            switch (a.state)
            {
                case ActionState.Dead: value = Mathf.Clamp01(a.stateTime / .75f); return "dead";
                case ActionState.Hang: timing = Timing.Loop; value = 2.5f; return "hang";
                case ActionState.Climb: return "climb";
                case ActionState.Hurt: return "hurt";
                case ActionState.Heal: return "heal";
                case ActionState.Spirit: return "spirit";
                case ActionState.Shoot: return "shoot";
                case ActionState.Dodge: return a.backdash ? "backdash" : "roll";
                case ActionState.Parry: return "parry";
                case ActionState.Block: value = Mathf.Clamp01(a.stateTime / .1f); return "block";
                case ActionState.Heavy: return "heavy";
                case ActionState.Light:
                    if (a.combo == 4) { timing = Timing.Loop; value = 12; return "dive"; }
                    return "light" + Mathf.Clamp(a.combo, 1, 3);
            }
            if (!a.grounded)
            {
                if (a.airFlip > 0) { value = 1 - a.airFlip / .38f; return "flip"; }
                timing = Timing.Loop; value = 10;
                return a.velocity.y > 2.2f ? "rise" : a.velocity.y > -2.2f ? "apex" : "fall";
            }
            if (landTimer > 0) { value = 1 - landTimer / .1f; return "land"; }
            if (a.skid > 0) { timing = Timing.Loop; value = 10; return "skid"; }
            if (a.state == ActionState.Move && Mathf.Abs(a.velocity.x) > .12f)
            { timing = Timing.Distance; return Mathf.Abs(a.velocity.x) > 3f ? "run" : "walk"; }
            timing = Timing.Loop; value = 7; return "idle";
        }

        string Resolve(string wanted)
        {
            if (pixel.clips.ContainsKey(wanted)) return wanted;
            if (Fallbacks.TryGetValue(wanted, out var options)) foreach (var o in options) if (pixel.clips.ContainsKey(o)) return o;
            return "idle";
        }

        static int ByProgress(ClipData c, float t)
        {
            t = Mathf.Clamp01(t);
            if (c.bounds != null && c.bounds.Length > 1)
            { int i = 0; while (i + 1 < c.bounds.Length && t >= c.bounds[i + 1]) i++; return Mathf.Min(i, c.count - 1); }
            return Mathf.Min(c.count - 1, (int)(t * c.count));
        }

        // Kare değiştiğinde basan ayağın dünyadaki yeri: karenin başladığı sınıra göre düzeltilir, böylece
        // ölçülen kayma çizimdeki ayak konumu ile adım uzunluğunun uyumunu gösterir (simülasyon adımını değil).
        void TrackFeet(Actor actor, Timing timing, ClipData c, int index, float strideUnits, float direction)
        {
            if (timing != Timing.Distance || !actor.grounded) return;
            float overshoot = (phase - index / (float)c.count) * strideUnits;
            float bodyX = actor.position.x - direction * overshoot;
            var f = Current;
            // Ayak teması işaretlenmemiş kareler (HD, yapay zekâ çizimi): adım, döngünün iki yarısının başında sayılır
            if (f.plantedN <= -998 && f.plantedF <= -998)
            { anchor[0] = anchor[1] = float.NaN; if (index == 0 || index == c.count / 2) Steps++; return; }
            for (int k = 0; k < 2; k++)
            {
                float x = k == 0 ? f.plantedN : f.plantedF;
                if (x <= -998) { anchor[k] = float.NaN; continue; }
                float foot = bodyX + face * x / Ppu;
                if (float.IsNaN(anchor[k])) { anchor[k] = foot; Steps++; }
                else { MaxFootSlip = Mathf.Max(MaxFootSlip, Mathf.Abs(foot - anchor[k])); PlantedSamples++; }
            }
        }

        // Sahne ışığı: en yakın ateşten kenar ışığı ve sıcaklık; vurulunca beyaz parlama. Piksel karakterin
        // kendi renk rampaları korunur: eski boyalı parçalar için yapılan doygunluk / kontrast telafisi kapalı.
        void ApplyLight(Actor actor)
        {
            LightField.Sample(actor.position + Vector2.up * 1.2f * actor.size, out var direction, out var color, out float amount, out float rim);
            block.SetFloat(FlashId, actor.hitFlash > 0 ? Mathf.Clamp01(actor.hitFlash / .14f) * .9f : 0);
            block.SetVector(RimDirId, new Vector4(direction.x, direction.y, 0, 0));
            block.SetFloat(RimWidthId, 1);
            block.SetColor(RimColorId, color);
            block.SetFloat(RimStrengthId, actor.Dead ? rim * .3f : rim);
            block.SetColor(LightColorId, color);
            block.SetFloat(LightAmountId, amount);
            block.SetFloat(MoonId, actor.Dead ? .08f : .22f);
            block.SetFloat(VividId, 1); block.SetFloat(ContrastId, 1); block.SetFloat(CharacterRimId, 1);
            body.SetPropertyBlock(block);
        }

        public void AfterImage(Effects effects, Color color) => effects.Ghost(body, color);
    }
}
