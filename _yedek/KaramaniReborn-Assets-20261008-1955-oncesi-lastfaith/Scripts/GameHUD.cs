using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KaramaniReborn
{
    public sealed class GameHUD
    {
        readonly GameRoot game;
        public readonly Canvas canvas;
        public readonly RectTransform root;
        readonly Font font;
        readonly Color gold = new Color(.76f, .65f, .43f), ivory = new Color(.93f, .89f, .81f), muted = new Color(.62f, .63f, .64f);
        readonly GameObject playUI, menuUI, pauseUI, deathUI, completeUI;
        readonly Image health, stamina, bossBar, miniHero, cooldownShade;
        readonly Text region, objective, notice, interaction, bossName, ammoCount, potionCount, chapter, selectedName;
        readonly List<Image> miniEnemies = new();
        readonly List<EnemyWidget> widgets = new();
        readonly GameObject bossGroup;
        sealed class EnemyWidget { public Actor actor; public RectTransform root; public Image health, intent; }
        // SotN tarzı hasar sayıları: darbede büyüyerek belirir, yükselir ve söner
        sealed class Number { public Text text; public Vector2 world; public float age; public Color color; }
        readonly List<Number> numbers = new();
        readonly Stack<Number> numberPool = new();
        readonly Transform numberRoot;
        readonly System.Random numberRandom = new(1566);
        float hpValue = 1, staminaValue = 1;

        public GameHUD(GameRoot owner)
        {
            game = owner;
            var go = new GameObject("Native painted HUD", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
            go.transform.SetParent(game.transform, false); root = go.GetComponent<RectTransform>();
            root.sizeDelta = new Vector2(GameView.CanvasWidth, GameView.CanvasHeight);
            canvas = go.GetComponent<Canvas>(); canvas.renderMode = RenderMode.WorldSpace; canvas.worldCamera = game.gameCamera; canvas.sortingOrder = 700;
            font = Font.CreateDynamicFontFromOSFont(new[] { "Georgia", "Times New Roman" }, 28);
            if (!font) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var vignette = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            for (int y = 0; y < 128; y++) for (int x = 0; x < 128; x++)
            { float d = new Vector2((x - 63.5f) / 80f, (y - 63.5f) / 88f).magnitude; vignette.SetPixel(x, y, new Color(0, .005f, .012f, Mathf.Pow(Mathf.Clamp01(d), 3) * .50f)); }
            vignette.Apply();
            var vs = Sprite.Create(vignette, new Rect(0, 0, 128, 128), new Vector2(.5f, .5f), 100);
            ImageAt(root, "Cinematic shade", 0, 0, GameView.CanvasWidth, GameView.CanvasHeight, Color.white, vs);
            playUI = Group(root, "Play HUD");
            var p = playUI.transform;
            var portraitFrame = ImageAt(p, "Portrait ornament", 45, 25, 82, 82, gold); CenterRect(portraitFrame.rectTransform); portraitFrame.transform.localRotation = Quaternion.Euler(0, 0, -45);
            var portraitInner = ImageAt(p, "Portrait inset", 50, 30, 72, 72, new Color(.023f, .025f, .026f)); CenterRect(portraitInner.rectTransform); portraitInner.transform.localRotation = Quaternion.Euler(0, 0, -45);
            ImageAt(p, "Painted portrait", 43, 18, 86, 104, Color.white, ArtLibrary.Rig(false)[0]).preserveAspect = true;
            health = FramedBar(p, 148, 42, 360, 18, new Color(.65f, .035f, .035f));
            stamina = FramedBar(p, 148, 73, 293, 10, new Color(.20f, .50f, .43f));
            Label(p, "K A R A M A N İ", 151, 98, 330, 26, 16, ivory);
            for (int i = 0; i < 3; i++) { var d = ImageAt(p, "Resolve diamond", 151 + i * 28, 132, 10, 10, gold); d.transform.localRotation = Quaternion.Euler(0, 0, 45); }
            ImageAt(p, "Minimap background", 1640, 30, 240, 120, new Color(.006f, .008f, .013f, .78f));
            Outline(p, 1640, 30, 240, 120);
            for (int i = 0; i < 3; i++) ImageAt(p, "Minimap rampart", 1660 + i * 69, 110, 62, 2, muted);
            for (int i = 0; i < 5; i++) ImageAt(p, "Minimap upper path", 1676 + i * 34, 79 - (i % 2) * 11, 21, 2, muted * .7f);
            miniHero = ImageAt(p, "Player location", 1670, 99, 8, 8, new Color(1, .86f, .50f)); miniHero.transform.localRotation = Quaternion.Euler(0, 0, 45);
            for (int i = 0; i < 6; i++) miniEnemies.Add(ImageAt(p, "Enemy location", 1700, 104, 4, 4, new Color(.75f, .18f, .13f)));
            region = Label(p, "", 1230, 175, 635, 32, 24, ivory, TextAnchor.MiddleRight);
            objective = Label(p, "", 1270, 215, 595, 55, 17, gold, TextAnchor.UpperRight);

            // Keep top/bottom margins and menu centering when changing from 16:9 to 4:3.
            // Offsets belong to groups, never to individual painted sprites.
            p = LayoutGroup(playUI.transform, "Bottom HUD", GameView.BottomOffset);
            Sprite[] gear = ArtLibrary.Gear();
            Sprite[] icons = { ArtLibrary.Rig(false)[14], gear[0], gear[3], gear[2] };
            string[] keys = { "J / F", "R", "Q", "H" };
            for (int i = 0; i < 4; i++)
            {
                float x = 43 + i * 106;
                ImageAt(p, "Equipment slot", x, 902, 90, 90, new Color(.015f, .017f, .024f, .88f)); Outline(p, x, 902, 90, 90);
                ImageAt(p, "Equipment painting", x + 8, 909, 74, 75, Color.white, icons[i]).preserveAspect = true;
                Label(p, keys[i], x, 1002, 90, 25, 19, ivory, TextAnchor.MiddleCenter);
            }
            cooldownShade = ImageAt(p, "Spirit cooldown", 255, 902, 90, 90, new Color(0, 0, 0, .7f)); cooldownShade.type = Image.Type.Filled; cooldownShade.fillMethod = Image.FillMethod.Vertical;
            ammoCount = Label(p, "", 207, 963, 26, 26, 17, ivory, TextAnchor.MiddleRight);
            potionCount = Label(p, "", 419, 963, 26, 26, 17, ivory, TextAnchor.MiddleRight);
            interaction = Label(p, "", 620, 860, 680, 40, 19, ivory, TextAnchor.MiddleCenter);
            notice = Label(p, "", 490, 818, 940, 40, 21, gold, TextAnchor.MiddleCenter);
            chapter = Label(LayoutGroup(playUI.transform, "Chapter heading", GameView.MiddleOffset), "", 460, 245, 1000, 180, 34, ivory, TextAnchor.MiddleCenter);
            Label(p, "A/D hareket  ·  Boşluk zıpla (havada tekrar)  ·  Havada S+J dalış  ·  W tırman  ·  Shift kaçış  ·  Sağ tık blok  ·  Esc menü  ·  F2 fotoğraf", 480, 1043, 1230, 25, 17, muted, TextAnchor.MiddleCenter);
            numberRoot = Group(playUI.transform, "Damage numbers").transform;
            bossGroup = Group(p, "Boss encounter");
            bossName = Label(bossGroup.transform, "", 500, 961, 920, 34, 23, ivory, TextAnchor.MiddleCenter);
            bossBar = FramedBar(bossGroup.transform, 500, 1003, 920, 13, new Color(.62f, .03f, .02f));

            menuUI = Group(root, "Journey menu");
            ImageAt(menuUI.transform, "Menu atmosphere", 0, 0, GameView.CanvasWidth, GameView.CanvasHeight, new Color(.005f, .009f, .015f, .54f));
            var m = LayoutGroup(menuUI.transform, "Centered menu layout", GameView.MiddleOffset);
            Label(m, "1566  ·  SINIRDAKİ SİS", 121, 148, 625, 35, 20, gold);
            Label(m, "K A R A M A N İ", 116, 207, 1000, 90, 54, ivory);
            ImageAt(m, "Title rule", 120, 322, 595, 1, gold * .6f);
            Label(m, "Surdan harabelere uzanan bir yolculuk.", 121, 349, 650, 75, 22, ivory);
            ButtonAt(m, "YOLCULUĞA BAŞLA", 121, 470, 382, 58, game.PlaySelected);
            ButtonAt(m, "KALDIĞIN YERDEN", 121, 545, 382, 54, game.ContinueJourney);
            ButtonAt(m, "TAM EKRAN / PENCERE", 121, 620, 382, 48, () => Screen.fullScreen = !Screen.fullScreen);
            ButtonAt(m, "ÇIKIŞ", 121, 685, 382, 48, () => Application.Quit());
            Label(m, "BİR BÖLGE SEÇ", 996, 398, 710, 28, 19, gold);
            for (int i = 0; i < 4; i++)
            {
                int index = i; float x = 996 + i % 2 * 328, y = 449 + i / 2 * 188;
                ButtonAt(m, "", x, y, 303, 160, () => game.PreviewZone(index));
                ImageAt(m, "Region painting", x + 7, y + 7, 289, 104, Color.white, ArtLibrary.Background(i)).preserveAspect = true;
                Label(m, ZoneDefinition.Create(i).title, x + 12, y + 118, 279, 30, 16, ivory, TextAnchor.MiddleCenter);
            }
            selectedName = Label(m, "", 996, 854, 641, 30, 18, gold, TextAnchor.MiddleCenter);
            Label(m, "Sol tık saldırı · F ağır darbe · Sağ tık savuşturma, ardından saldırı: infaz\nBoşluk zıplama (havada tekrar) · Havada S + sol tık dalış · W kenara tırman\nShift + yön kaçış, yalnız Shift geri adım · Q kurt · R tabanca · H iksir", 121, 816, 733, 89, 17, muted);

            pauseUI = CenterPanel("DURAKLATILDI", "Nefes al. Surlar bekler.", out var pauseParent);
            ButtonAt(pauseParent, "DEVAM ET", 724, 575, 472, 52, game.TogglePause);
            ButtonAt(pauseParent, "ANA MENÜ", 724, 650, 472, 52, game.ReturnToMenu);
            ButtonAt(pauseParent, "SES AÇ / KAPAT", 724, 725, 472, 47, () => AudioListener.volume = AudioListener.volume > 0 ? 0 : 1);
            deathUI = CenterPanel("KARAMANİ DÜŞTÜ", "Yol taşı seni hatırlar.", out var deathParent);
            ButtonAt(deathParent, "YOL TAŞINDA YENİDEN DOĞ", 690, 575, 540, 55, game.Respawn);
            ButtonAt(deathParent, "ANA MENÜ", 690, 650, 540, 52, game.ReturnToMenu);
            completeUI = CenterPanel("SINIRDAKİ SİS DAĞILDI", "Dört kapı aşıldı. Yolculuk tamamlandı.", out var completeParent);
            ButtonAt(completeParent, "ANA MENÜ", 690, 575, 540, 55, game.ReturnToMenu);
            SetPause(false); deathUI.SetActive(false); completeUI.SetActive(false); Follow(game.gameCamera);
        }

        public void Bind(GameWorld world)
        {
            foreach (var widget in widgets) { if (Application.isPlaying) UnityEngine.Object.Destroy(widget.root.gameObject); else UnityEngine.Object.DestroyImmediate(widget.root.gameObject); }
            widgets.Clear();
            foreach (var actor in world.enemies)
            {
                var go = Group(playUI.transform, "Enemy health"); var rt = (RectTransform)go.transform;
                var hp = FramedBar(rt, 0, 0, 98, 5, new Color(.61f, .06f, .03f));
                var intent = ImageAt(rt, "Attack telegraph", 0, 9, 98, 2, gold);
                intent.type = Image.Type.Filled; intent.fillMethod = Image.FillMethod.Horizontal;
                widgets.Add(new EnemyWidget { actor = actor, root = rt, health = hp, intent = intent });
            }
            foreach (var n in numbers) { n.text.gameObject.SetActive(false); numberPool.Push(n); }
            numbers.Clear(); world.damageEvents.Clear();
            hpValue = 1; staminaValue = 1; region.text = world.zone.title; objective.text = world.zone.objective;
            selectedName.text = "SEÇİLİ · " + world.zone.title; deathUI.SetActive(false); completeUI.SetActive(false);
        }

        public void Tick(float dt)
        {
            var w = game.world; var player = w.player;
            playUI.SetActive(!game.menu && !game.photoMode);
            hpValue = Mathf.MoveTowards(hpValue, player.health / player.maxHealth, dt * 1.8f); staminaValue = Mathf.MoveTowards(staminaValue, player.stamina / 100, dt * 2.8f);
            health.fillAmount = hpValue; stamina.fillAmount = staminaValue; cooldownShade.fillAmount = Mathf.Clamp01(player.skillCooldown / 8);
            ammoCount.text = player.ammo.ToString(); potionCount.text = player.potions.ToString();
            notice.text = w.notificationTime > 0 ? w.notification : ""; interaction.text = w.InteractionHint;
            objective.text = w.cleared ? "◇ İç kapı açık. Yoluna devam et." : "◇ İç kapıya ulaş. Nöbeti kır.";
            chapter.text = game.chapterTime > 0 ? "<size=18>" + w.zone.subtitle.ToUpperInvariant() + "</size>\n\n" + w.zone.title : "";
            chapter.color = new Color(ivory.r, ivory.g, ivory.b, Mathf.Clamp01(Mathf.Min(game.chapterTime, 3.2f - game.chapterTime) * 1.5f));
            SetRectPosition(miniHero.rectTransform, 1660 + Mathf.Clamp01(player.position.x / w.zone.length) * 200, 98);
            for (int i = 0; i < miniEnemies.Count; i++)
            { var a = w.enemies[i]; miniEnemies[i].enabled = !a.Dead; SetRectPosition(miniEnemies[i].rectTransform, 1660 + Mathf.Clamp01(a.position.x / w.zone.length) * 200, 107); }
            foreach (var widget in widgets)
            {
                var a = widget.actor; Vector3 point = game.gameCamera.WorldToViewportPoint(a.position + Vector2.up * (2.62f * a.size));
                widget.root.gameObject.SetActive(!a.Dead && point.x > -.05f && point.x < 1.05f && (a.health < a.maxHealth || a.Attacking || a.role == EnemyRole.Archer));
                SetRectPosition(widget.root, point.x * GameView.CanvasWidth - 49, (1 - point.y) * GameView.CanvasHeight);
                widget.health.fillAmount = a.health / a.maxHealth; widget.intent.enabled = a.Attacking; widget.intent.fillAmount = a.Progress;
            }
            TickNumbers(w, dt);
            var boss = w.enemies.Find(a => a.boss);
            bossGroup.SetActive(boss != null && !boss.Dead && Mathf.Abs(boss.position.x - player.position.x) < 9);
            if (boss != null) { bossName.text = w.zone.bossName; bossBar.fillAmount = boss.health / boss.maxHealth; }
            deathUI.SetActive(player.Dead && player.stateTime > .8f && !game.menu);
        }

        void TickNumbers(GameWorld w, float dt)
        {
            foreach (var e in w.damageEvents)
            {
                var n = numberPool.Count > 0 ? numberPool.Pop() : new Number { text = Label(numberRoot, "", 0, 0, 180, 60, 30, ivory, TextAnchor.MiddleCenter) };
                if (n.text.rectTransform.pivot.x < .5f) CenterRect(n.text.rectTransform);
                n.text.gameObject.SetActive(true); n.text.text = e.amount.ToString(); n.text.fontSize = e.strong ? 42 : 31;
                n.color = e.toPlayer ? new Color(.96f, .33f, .26f) : e.strong ? new Color(1f, .82f, .42f) : ivory;
                n.world = e.position + new Vector2((float)numberRandom.NextDouble() * .5f - .25f, 0); n.age = 0; numbers.Add(n);
            }
            w.damageEvents.Clear();
            const float life = .85f;
            for (int i = numbers.Count - 1; i >= 0; i--)
            {
                var n = numbers[i]; n.age += dt;
                if (n.age >= life) { n.text.gameObject.SetActive(false); numbers.RemoveAt(i); numberPool.Push(n); continue; }
                float t = n.age / life, rise = .65f * (1 - (1 - t) * (1 - t));
                Vector3 point = game.gameCamera.WorldToViewportPoint(n.world + Vector2.up * rise);
                SetRectPosition(n.text.rectTransform, point.x * GameView.CanvasWidth, (1 - point.y) * GameView.CanvasHeight);
                float pop = 1 + .55f * Mathf.Clamp01(1 - n.age * 9);
                n.text.rectTransform.localScale = new Vector3(pop, pop, 1);
                n.text.color = new Color(n.color.r, n.color.g, n.color.b, Mathf.Clamp01((1 - t) / .4f));
            }
        }

        public void Follow(Camera camera)
        { root.position = new Vector3(camera.transform.position.x, camera.transform.position.y, -2); float scale = camera.orthographicSize * 2 / GameView.CanvasHeight; root.localScale = new Vector3(scale, scale, scale); }
        public void SetMenu(bool enabled) { menuUI.SetActive(enabled); playUI.SetActive(!enabled); deathUI.SetActive(false); completeUI.SetActive(false); }
        public void SetPause(bool enabled) => pauseUI.SetActive(enabled);
        public void ShowComplete() => completeUI.SetActive(true);

        GameObject CenterPanel(string title, string subtitle, out Transform parent)
        {
            var go = Group(root, title);
            ImageAt(go.transform, "Screen veil", 0, 0, GameView.CanvasWidth, GameView.CanvasHeight, new Color(0, .004f, .009f, .73f));
            parent = LayoutGroup(go.transform, "Centered panel layout", GameView.MiddleOffset);
            ImageAt(parent, "Inset panel", 570, 330, 780, 518, new Color(.025f, .026f, .032f, .95f)); Outline(parent, 570, 330, 780, 518);
            Label(parent, title, 595, 395, 730, 58, 32, ivory, TextAnchor.MiddleCenter);
            Label(parent, subtitle, 595, 479, 730, 40, 19, gold, TextAnchor.MiddleCenter); return go;
        }

        static GameObject Group(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false); var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero; return go;
        }
        static Transform LayoutGroup(Transform parent, string name, float offsetY)
        {
            var go = Group(parent, name);
            ((RectTransform)go.transform).anchoredPosition = new Vector2(0, -offsetY);
            return go.transform;
        }
        RectTransform RectAt(Transform parent, string name, float x, float y, float width, float height)
        {
            var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false); var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0, 1); rt.pivot = new Vector2(0, 1); rt.anchoredPosition = new Vector2(x, -y); rt.sizeDelta = new Vector2(width, height); return rt;
        }
        Image ImageAt(Transform p, string name, float x, float y, float width, float height, Color color, Sprite sprite = null)
        {
            var rt = RectAt(p, name, x, y, width, height); var im = rt.gameObject.AddComponent<Image>(); im.sprite = sprite ? sprite : ArtLibrary.White;
            if (sprite) CenterRect(rt);
            im.color = color; im.raycastTarget = false; return im;
        }
        Text Label(Transform p, string value, float x, float y, float width, float height, int size, Color color, TextAnchor alignment = TextAnchor.UpperLeft)
        {
            var rt = RectAt(p, value, x, y, width, height); var t = rt.gameObject.AddComponent<Text>(); t.font = font; t.text = value; t.fontSize = size; t.color = color;
            t.alignment = alignment; t.supportRichText = true; t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Truncate; t.raycastTarget = false;
            var shadow = rt.gameObject.AddComponent<Shadow>(); shadow.effectColor = new Color(0, 0, 0, .9f); shadow.effectDistance = new Vector2(1, -2); return t;
        }
        Image FramedBar(Transform p, float x, float y, float width, float height, Color fill)
        {
            ImageAt(p, "Bar border", x - 1, y - 1, width + 2, height + 2, gold);
            ImageAt(p, "Bar inset", x, y, width, height, new Color(.025f, .022f, .023f));
            var bar = ImageAt(p, "Bar fill", x + 2, y + 2, width - 4, height - 4, fill); bar.type = Image.Type.Filled; bar.fillMethod = Image.FillMethod.Horizontal; return bar;
        }
        void Outline(Transform p, float x, float y, float width, float height)
        { ImageAt(p, "Top frame", x, y, width, 1, gold); ImageAt(p, "Bottom frame", x, y + height - 1, width, 1, gold); ImageAt(p, "Left frame", x, y, 1, height, gold); ImageAt(p, "Right frame", x + width - 1, y, 1, height, gold); }
        void ButtonAt(Transform p, string title, float x, float y, float width, float height, Action clicked)
        {
            var image = ImageAt(p, title, x, y, width, height, new Color(.046f, .045f, .040f, .92f)); image.raycastTarget = true;
            Outline(p, x, y, width, height); Label(p, title, x + 4, y + 4, width - 8, height - 8, 18, ivory, TextAnchor.MiddleCenter);
            var button = image.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            var colors = button.colors; colors.highlightedColor = new Color(1.35f, 1.28f, 1.15f); colors.pressedColor = new Color(.76f, .68f, .50f); button.colors = colors;
            button.onClick.AddListener(() => clicked());
        }
        static void SetRectPosition(RectTransform rt, float x, float y) => rt.anchoredPosition = new Vector2(x, -y);
        static void CenterRect(RectTransform rt)
        { rt.anchoredPosition += new Vector2(rt.sizeDelta.x * .5f, -rt.sizeDelta.y * .5f); rt.pivot = new Vector2(.5f, .5f); }
    }
}
