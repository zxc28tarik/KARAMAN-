using System;
using System.Collections.Generic;
using UnityEngine;

namespace Karamani
{
    public sealed class KaramaniGame : MonoBehaviour
    {
        const float Ground = -3.24f;
        readonly List<Guard> guards = new();
        readonly List<Ember> embers = new();
        Hero hero;
        Camera cam;
        Sprite pixel;
        Texture2D vignette;
        GUIStyle titleStyle, smallStyle, questStyle, keyStyle, centerStyle;
        float shake, intro = 4.5f, flash, finishTimer;
        string notice = "";
        float noticeTime;
        bool won, lost;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void EnsureGame()
        {
            if (FindFirstObjectByType<KaramaniGame>() == null)
                new GameObject("Karamani Game").AddComponent<KaramaniGame>();
        }

        void Awake()
        {
            Application.targetFrameRate = 120;
            QualitySettings.vSyncCount = 1;
            pixel = MakePixel();
            SetupCamera();
            SetupEnvironment();
            SpawnActors();
            Debug.Log("KARAMANI_GAME_READY");
        }

        Sprite MakePixel()
        {
            var t = new Texture2D(1, 1, TextureFormat.RGBA32, false) { name = "Runtime Pixel" };
            t.SetPixel(0, 0, Color.white); t.Apply();
            return Sprite.Create(t, new Rect(0, 0, 1, 1), new Vector2(.5f, .5f), 1);
        }

        void SetupCamera()
        {
            cam = Camera.main;
            if (!cam)
            {
                var go = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
                go.tag = "MainCamera"; cam = go.GetComponent<Camera>();
            }
            cam.orthographic = true; cam.orthographicSize = 5.4f;
            cam.transform.position = new Vector3(0, 0, -10);
            cam.backgroundColor = new Color(.018f, .025f, .035f);
            cam.clearFlags = CameraClearFlags.SolidColor;
        }

        void SetupEnvironment()
        {
            var bgSprite = Resources.Load<Sprite>("Art/fortress");
            var bg = new GameObject("Fortress · Cinematic Background").AddComponent<SpriteRenderer>();
            bg.sprite = bgSprite; bg.sortingOrder = -100;
            if (bgSprite)
            {
                float height = cam.orthographicSize * 2f;
                float width = height * cam.aspect;
                bg.transform.localScale = new Vector3(width / bgSprite.bounds.size.x, height / bgSprite.bounds.size.y, 1);
            }
            bg.transform.position = new Vector3(0, 0, 2);

            MakeRect("Walkable Rampart", new Vector2(0, Ground - .12f), new Vector2(19.3f, .48f), new Color(.035f, .028f, .025f, .88f), -4);
            MakeRect("Foreground Shade", new Vector2(0, -4.72f), new Vector2(19.3f, 2.3f), new Color(.005f, .007f, .009f, .72f), 50);
            for (int i = 0; i < 56; i++)
            {
                var e = new Ember();
                e.go = new GameObject("Ember");
                e.sr = e.go.AddComponent<SpriteRenderer>(); e.sr.sprite = pixel; e.sr.sortingOrder = -2;
                e.speed = UnityEngine.Random.Range(.08f, .32f); e.phase = UnityEngine.Random.value * 12;
                e.go.transform.position = new Vector3(UnityEngine.Random.Range(-9f, 9f), UnityEngine.Random.Range(-4.2f, 2.4f), 0);
                float s = UnityEngine.Random.Range(.012f, .038f); e.go.transform.localScale = new Vector3(s, s * UnityEngine.Random.Range(1.5f, 3.4f), 1);
                e.sr.color = new Color(1f, UnityEngine.Random.Range(.35f, .72f), .12f, UnityEngine.Random.Range(.2f, .85f));
                embers.Add(e);
            }
            for (int i = 0; i < 5; i++)
            {
                var fog = MakeRect("Drifting Fog", new Vector2(-7 + i * 3.6f, UnityEngine.Random.Range(-2.7f, .8f)), new Vector2(5.2f, .65f), new Color(.28f, .35f, .4f, .045f), -8);
                fog.transform.rotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(-5f, 5f));
                fog.AddComponent<FogDrift>().speed = UnityEngine.Random.Range(.035f, .075f);
            }
        }

        GameObject MakeRect(string name, Vector2 position, Vector2 size, Color color, int order)
        {
            var go = new GameObject(name); var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = pixel; sr.color = color; sr.sortingOrder = order;
            go.transform.position = position; go.transform.localScale = size;
            return go;
        }

        void SpawnActors()
        {
            hero = new Hero(CreateActor("Karamani", Resources.Load<Sprite>("Art/hero"), new Vector2(-6.6f, Ground + 1.67f), .42f, 20), pixel);
            guards.Add(new Guard(CreateActor("Sınır Muhafızı", Resources.Load<Sprite>("Art/guard"), new Vector2(-1.8f, Ground + 1.58f), .39f, 18), -1.8f, false));
            guards.Add(new Guard(CreateActor("Kalkanlı Muhafız", Resources.Load<Sprite>("Art/guard"), new Vector2(2.15f, Ground + 1.58f), .40f, 18), 2.15f, false));
            guards.Add(new Guard(CreateActor("Yeniçeri Kaptanı", Resources.Load<Sprite>("Art/guard"), new Vector2(6.15f, Ground + 1.72f), .44f, 18), 6.15f, true));
        }

        ActorView CreateActor(string name, Sprite sprite, Vector2 position, float scale, int order)
        {
            var root = new GameObject(name); root.transform.position = position;
            var art = new GameObject("Painted Character"); art.transform.SetParent(root.transform, false);
            var sr = art.AddComponent<SpriteRenderer>(); sr.sprite = sprite; sr.sortingOrder = order;
            art.transform.localScale = Vector3.one * scale;
            var shadow = MakeRect("Contact Shadow", position + Vector2.down * 1.67f, new Vector2(1.2f, .18f), new Color(0, 0, 0, .5f), 17);
            shadow.transform.SetParent(root.transform); shadow.transform.localPosition = new Vector3(0, -1.68f, 0);
            return new ActorView { root = root.transform, art = art.transform, renderer = sr, baseScale = scale };
        }

        void SetupStyles()
        {
            vignette = new Texture2D(1, 1); vignette.SetPixel(0, 0, Color.white); vignette.Apply();
            titleStyle = Style(21, FontStyle.Bold, new Color(.93f, .83f, .64f), TextAnchor.UpperLeft);
            smallStyle = Style(13, FontStyle.Normal, new Color(.78f, .78f, .75f), TextAnchor.UpperLeft);
            questStyle = Style(18, FontStyle.Bold, new Color(.94f, .88f, .76f), TextAnchor.UpperRight);
            keyStyle = Style(12, FontStyle.Normal, new Color(.78f, .78f, .74f), TextAnchor.LowerCenter);
            centerStyle = Style(30, FontStyle.Bold, new Color(1f, .86f, .57f), TextAnchor.MiddleCenter);
        }

        GUIStyle Style(int size, FontStyle style, Color color, TextAnchor align)
        {
            return new GUIStyle(GUI.skin.label) { fontSize = size, fontStyle = style, normal = { textColor = color }, alignment = align, richText = true };
        }

        void Update()
        {
            float dt = Mathf.Min(Time.deltaTime, .033f);
            if (lost || won)
            {
                finishTimer += dt;
                if (Input.GetKeyDown(KeyCode.R)) UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            }
            else
            {
                hero.Tick(dt, guards, this);
                foreach (var guard in guards) guard.Tick(dt, hero, this);
                guards.RemoveAll(g => g.dead && g.death > 2.5f);
                if (hero.hp <= 0) { lost = true; notice = "KARAMANİ DÜŞTÜ"; }
                if (guards.Count == 0) { won = true; notice = "DOĞU KAPISI AÇILDI"; }
            }
            foreach (var e in embers)
            {
                var p = e.go.transform.position; p.y += e.speed * dt; p.x += Mathf.Sin(Time.time * .9f + e.phase) * dt * .05f;
                if (p.y > 3.4f) { p.y = -4.1f; p.x = UnityEngine.Random.Range(-9f, 9f); }
                e.go.transform.position = p;
            }
            shake = Mathf.MoveTowards(shake, 0, dt * 8); flash = Mathf.MoveTowards(flash, 0, dt * 4);
            cam.transform.position = new Vector3(UnityEngine.Random.Range(-shake, shake), UnityEngine.Random.Range(-shake, shake), -10);
            intro -= dt; noticeTime -= dt;
        }

        public void Impact(Vector3 at, Color color, bool heavy)
        {
            shake = heavy ? .11f : .055f; flash = heavy ? .22f : .1f;
            for (int i = 0; i < (heavy ? 22 : 12); i++)
            {
                var spark = new GameObject("Impact Spark"); spark.transform.position = at;
                var sr = spark.AddComponent<SpriteRenderer>(); sr.sprite = pixel; sr.color = color; sr.sortingOrder = 40;
                spark.transform.localScale = new Vector3(.018f, UnityEngine.Random.Range(.08f, .2f), 1);
                spark.transform.rotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(0, 360));
                var fx = spark.AddComponent<Spark>(); fx.velocity = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(1.8f, 5.5f); fx.life = UnityEngine.Random.Range(.18f, .48f);
            }
        }

        public void Announce(string text) { notice = text; noticeTime = 1.1f; }

        void Bar(Rect rect, float value, Color fill, string label)
        {
            GUI.color = new Color(.015f, .018f, .021f, .88f); GUI.DrawTexture(rect, vignette);
            GUI.color = new Color(.5f, .43f, .32f, 1); GUI.DrawTexture(new Rect(rect.x - 1, rect.y - 1, rect.width + 2, rect.height + 2), vignette);
            GUI.color = new Color(.025f, .025f, .026f, 1); GUI.DrawTexture(rect, vignette);
            GUI.color = fill; GUI.DrawTexture(new Rect(rect.x + 2, rect.y + 2, (rect.width - 4) * Mathf.Clamp01(value), rect.height - 4), vignette);
            GUI.color = Color.white;
            if (!string.IsNullOrEmpty(label)) GUI.Label(new Rect(rect.x, rect.y - 20, rect.width, 18), label, smallStyle);
        }

        void OnGUI()
        {
            // GUI.skin is available only during an OnGUI callback.
            if (titleStyle == null)
            {
                SetupStyles();
                Debug.Log("KARAMANI_HUD_READY");
            }
            float s = Mathf.Min(Screen.width / 1920f, Screen.height / 1080f); GUI.matrix = Matrix4x4.Scale(Vector3.one * s);
            float w = Screen.width / s, h = Screen.height / s;
            GUI.color = new Color(.01f, .012f, .018f, .48f); GUI.DrawTexture(new Rect(0, 0, 470, 160), vignette);
            GUI.color = Color.white; GUI.Label(new Rect(46, 30, 370, 35), "KARAMANİ", titleStyle);
            GUI.Label(new Rect(46, 61, 370, 24), "SINIRDAKİ SİS  ·  1566", smallStyle);
            Bar(new Rect(47, 94, 315, 15), hero.hp / 100f, new Color(.62f, .08f, .07f), "");
            Bar(new Rect(47, 117, 255, 10), hero.stamina / 100f, new Color(.22f, .62f, .55f), "");

            GUI.Label(new Rect(w - 620, 35, 565, 32), "DOĞU KAPISINA ULAŞ", questStyle);
            GUI.Label(new Rect(w - 620, 72, 565, 26), guards.Count > 1 ? $"Muhafızları aş  ·  {guards.Count} düşman" : guards.Count == 1 ? "Yeniçeri Kaptanı'nı yen" : "Kapı açıldı", questStyle);

            var boss = guards.Find(g => g.boss && !g.dead);
            if (boss != null && Mathf.Abs(boss.view.root.position.x - hero.view.root.position.x) < 8.5f)
            {
                GUI.Label(new Rect(w * .25f, h - 122, w * .5f, 24), "YENİÇERİ KAPTANI", questStyle);
                Bar(new Rect(w * .25f, h - 91, w * .5f, 13), boss.hp / boss.maxHp, new Color(.65f, .075f, .055f), "");
            }

            GUI.Label(new Rect(0, h - 52, w, 30), "A/D Hareket  ·  Shift Kaçış  ·  Boşluk Zıpla  ·  Sol Tık/J Saldırı  ·  F Ağır  ·  Sağ Tık/K Blok", keyStyle);
            if (intro > 0) { float a = Mathf.Clamp01(Mathf.Min(intro, 4.5f - intro)); GUI.color = new Color(1, 1, 1, a); GUI.Label(new Rect(0, h * .27f, w, 120), "<size=22>SAHNE I</size>\nSURLARDAKİ ATEŞ", centerStyle); }
            if (noticeTime > 0 || won || lost)
            {
                GUI.color = new Color(0, 0, 0, .55f); GUI.DrawTexture(new Rect(w * .28f, h * .38f, w * .44f, 126), vignette);
                GUI.color = Color.white; GUI.Label(new Rect(w * .28f, h * .38f, w * .44f, 126), notice + ((won || lost) ? "\n<size=16>R ile yeniden başla</size>" : ""), centerStyle);
            }
            if (flash > 0) { GUI.color = new Color(1f, .72f, .4f, flash); GUI.DrawTexture(new Rect(0, 0, w, h), vignette); }
            GUI.color = Color.white;
        }

        sealed class Ember { public GameObject go; public SpriteRenderer sr; public float speed, phase; }
    }

    public sealed class ActorView
    {
        public Transform root, art; public SpriteRenderer renderer; public float baseScale;
        public void Face(bool right, bool originalRight) => renderer.flipX = originalRight ? !right : right;
        public void Pose(float speed, float attack, bool hurt, bool block, bool grounded)
        {
            float walk = Mathf.Sin(Time.time * Mathf.Lerp(4, 10, Mathf.Clamp01(speed / 5f))) * Mathf.Clamp01(speed) * .028f;
            float strike = attack > 0 ? Mathf.Sin(attack * Mathf.PI) : 0;
            art.localRotation = Quaternion.Euler(0, 0, (walk - strike * 7f + (hurt ? 7f : 0)) * (renderer.flipX ? -1 : 1));
            float squash = grounded ? Mathf.Abs(Mathf.Sin(Time.time * 8)) * Mathf.Clamp01(speed) * .018f : .035f;
            art.localScale = new Vector3(baseScale * (1 + strike * .05f), baseScale * (1 - squash), baseScale);
            renderer.color = hurt ? new Color(1f, .58f, .45f) : block ? new Color(.78f, .9f, 1f) : Color.white;
        }
    }

    public sealed class Hero
    {
        public readonly ActorView view;
        readonly Sprite pixel;
        public float hp = 100, stamina = 100;
        float vy, attack, attackDuration, cooldown, invuln, hurt, dodge;
        int combo;
        bool facingRight = true, grounded = true, blocking;

        public Hero(ActorView v, Sprite p) { view = v; pixel = p; }

        public void Tick(float dt, List<Guard> guards, KaramaniGame game)
        {
            cooldown -= dt; invuln -= dt; hurt -= dt; dodge -= dt;
            blocking = (Input.GetKey(KeyCode.K) || Input.GetMouseButton(1)) && attack <= 0 && grounded;
            float axis = (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1 : 0) - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1 : 0);
            if (axis != 0 && attack <= 0 && hurt <= 0) facingRight = axis > 0;
            float speed = blocking ? 1.1f : attack > 0 ? .55f : 3.65f;
            if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) && stamina >= 20 && grounded)
            { dodge = .42f; invuln = .48f; stamina -= 20; game.Announce("KAÇIŞ"); }
            if (dodge > 0) axis = facingRight ? 2.1f : -2.1f;
            if (hurt <= 0) view.root.position += Vector3.right * axis * speed * dt;
            var p = view.root.position; p.x = Mathf.Clamp(p.x, -8.1f, 8.1f);
            if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W)) && grounded && !blocking) { vy = 6.7f; grounded = false; }
            if (!grounded) { vy -= 17.5f * dt; p.y += vy * dt; if (p.y <= KGround) { p.y = KGround; vy = 0; grounded = true; } }
            view.root.position = p;
            if (attack <= 0 && cooldown <= 0 && !blocking && hurt <= 0)
            {
                if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.J)) && stamina >= 8) StartAttack(false, game);
                else if (Input.GetKeyDown(KeyCode.F) && stamina >= 25) StartAttack(true, game);
            }
            if (attack > 0)
            {
                float before = attack; attack -= dt; float normalized = 1f - attack / attackDuration;
                if (before > attackDuration * .58f && attack <= attackDuration * .58f)
                {
                    foreach (var g in guards)
                    {
                        if (g.dead) continue; float dx = g.view.root.position.x - view.root.position.x;
                        if (Mathf.Sign(dx) == (facingRight ? 1 : -1) && Mathf.Abs(dx) < (attackDuration > .55f ? 2.45f : 1.72f) && Mathf.Abs(g.view.root.position.y - view.root.position.y) < 1.2f)
                            g.TakeHit(attackDuration > .55f ? 38 : 18 + combo * 3, facingRight ? 1 : -1, attackDuration > .55f, game);
                    }
                }
                if (attack <= 0) cooldown = attackDuration > .55f ? .28f : .1f;
            }
            stamina = Mathf.MoveTowards(stamina, 100, dt * (blocking ? 6 : 18));
            view.Face(facingRight, true); view.Pose(Mathf.Abs(axis), attack > 0 ? 1f - attack / attackDuration : 0, hurt > 0, blocking, grounded);
        }

        const float KGround = -1.57f;
        void StartAttack(bool heavy, KaramaniGame game)
        {
            attackDuration = heavy ? .72f : .42f; attack = attackDuration; stamina -= heavy ? 25 : 8;
            combo = heavy ? 3 : combo % 3 + 1; game.Announce(heavy ? "AĞIR DARBE" : combo == 3 ? "ÜÇLÜ KOMBO" : "");
            var trail = new GameObject("Kilij Arc"); trail.transform.position = view.root.position + new Vector3(facingRight ? .8f : -.8f, .15f, 0);
            var tr = trail.AddComponent<TrailRenderer>(); tr.time = .18f; tr.startWidth = .12f; tr.endWidth = 0; tr.material = new Material(Shader.Find("Sprites/Default")); tr.startColor = new Color(1, .84f, .6f, .95f); tr.endColor = new Color(1, .3f, .1f, 0);
            trail.AddComponent<SlashTrail>().right = facingRight;
        }

        public void TakeHit(float damage, float direction, KaramaniGame game)
        {
            if (invuln > 0) return;
            if (blocking)
            {
                stamina -= damage * .8f; game.Impact(view.root.position + Vector3.up * .3f, new Color(.7f, .9f, 1), false); game.Announce("BLOK");
                if (stamina > 0) return;
            }
            hp = Mathf.Max(0, hp - damage); hurt = .36f; invuln = .55f; view.root.position += Vector3.right * direction * .35f;
            game.Impact(view.root.position + Vector3.up * .25f, new Color(1, .28f, .12f), damage > 20);
        }
    }

    public sealed class Guard
    {
        public readonly ActorView view; public readonly bool boss; public float hp, maxHp, death; public bool dead;
        readonly float home;
        float cooldown, attack, hurt, guard, think;
        bool facingRight;

        public Guard(ActorView v, float h, bool isBoss)
        { view = v; home = h; boss = isBoss; maxHp = hp = isBoss ? 190 : 82; guard = isBoss ? 90 : 48; cooldown = UnityEngine.Random.Range(.35f, 1.2f); }

        public void Tick(float dt, Hero hero, KaramaniGame game)
        {
            if (dead)
            {
                death += dt; view.art.localRotation = Quaternion.Lerp(view.art.localRotation, Quaternion.Euler(0, 0, facingRight ? -82 : 82), dt * 3);
                view.renderer.color = Color.Lerp(view.renderer.color, new Color(.18f, .12f, .1f, 0), dt * .7f); return;
            }
            cooldown -= dt; hurt -= dt; think -= dt; guard = Mathf.MoveTowards(guard, boss ? 90 : 48, dt * 6);
            float dx = hero.view.root.position.x - view.root.position.x; facingRight = dx > 0; float dist = Mathf.Abs(dx); float speed = 0;
            bool active = dist < (boss ? 9.5f : 7.2f);
            bool guarding = active && hurt <= 0 && attack <= 0 && dist < 2.25f && guard > 0 && Mathf.Sin(Time.time * 2.1f + home) > .25f;
            if (active && hurt <= 0 && attack <= 0 && !guarding)
            {
                if (dist > (boss ? 1.7f : 1.48f)) { speed = boss ? 1.45f : 1.12f; view.root.position += Vector3.right * Mathf.Sign(dx) * speed * dt; }
                else if (cooldown <= 0) { attack = boss ? .82f : .7f; cooldown = boss ? 1.05f : 1.42f; }
            }
            if (attack > 0)
            {
                float duration = boss ? .82f : .7f; float before = attack; attack -= dt;
                if (before > duration * .48f && attack <= duration * .48f && dist < (boss ? 2.05f : 1.72f)) hero.TakeHit(boss ? 27 : 16, facingRight ? 1 : -1, game);
            }
            view.Face(facingRight, false); view.Pose(speed, attack > 0 ? 1f - attack / (boss ? .82f : .7f) : 0, hurt > 0, guarding, true);
        }

        public void TakeHit(float damage, float direction, bool heavy, KaramaniGame game)
        {
            if (dead || hurt > 0) return;
            bool blocks = guard > 0 && !heavy && UnityEngine.Random.value < (boss ? .62f : .38f);
            if (blocks)
            {
                guard -= damage * 1.25f; cooldown = Mathf.Max(cooldown, .35f); game.Impact(view.root.position + Vector3.up * .2f, new Color(.65f, .88f, 1), false); game.Announce("KALKAN"); return;
            }
            hp -= damage; hurt = heavy ? .62f : .32f; attack = 0; view.root.position += Vector3.right * direction * (heavy ? .58f : .25f);
            game.Impact(view.root.position + Vector3.up * .25f, new Color(1f, .3f, .12f), heavy);
            if (hp <= 0) { dead = true; game.Announce(boss ? "KAPTAN YENİLDİ" : "MUHAFIZ DÜŞTÜ"); }
        }
    }

    public sealed class FogDrift : MonoBehaviour
    { public float speed; void Update() { transform.position += Vector3.right * speed * Time.deltaTime; if (transform.position.x > 11) transform.position += Vector3.left * 22; } }

    public sealed class Spark : MonoBehaviour
    {
        public Vector2 velocity; public float life; float total;
        void Start() => total = life;
        void Update() { life -= Time.deltaTime; velocity += Vector2.down * 8 * Time.deltaTime; transform.position += (Vector3)(velocity * Time.deltaTime); var sr = GetComponent<SpriteRenderer>(); sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, Mathf.Clamp01(life / total)); if (life <= 0) Destroy(gameObject); }
    }

    public sealed class SlashTrail : MonoBehaviour
    {
        public bool right; float t;
        void Update() { t += Time.deltaTime * 9; transform.position += new Vector3((right ? 1 : -1) * Time.deltaTime * 5, Mathf.Sin(t) * Time.deltaTime * 2, 0); if (t > 2.6f) Destroy(gameObject); }
    }
}
