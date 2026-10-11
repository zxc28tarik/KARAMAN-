using System;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KaramaniReborn
{
    public sealed class GameRoot : MonoBehaviour
    {
        public GameWorld world;
        public StageArt stage;
        public Camera gameCamera, marginCamera;
        public GameHUD hud;
        public bool menu = true, paused, photoMode, reviewMode, journeyComplete;
        public int selectedZone;
        public float chapterTime;
        Transform worldRoot;
        GameAudio audioGame;
        float accumulator, visualClock, cameraVelocity;
        InputFrame pendingInput;
        bool initialized;
        SaveData save;
        [Serializable] public sealed class SaveData { public int version = 1, zone; public float checkpoint = 1.7f; }

        void Awake()
        {
            if (!Application.isPlaying) return;
            Initialize(false);
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "--karamani-verify") >= 0)
                gameObject.AddComponent<RuntimeVerification>().game = this;
        }

        public void Initialize(bool review)
        {
            if (initialized) return; initialized = true; reviewMode = review;
            ArtLibrary.Initialize();
            Application.targetFrameRate = 60; QualitySettings.vSyncCount = 1;
            var cameraObject = new GameObject("Main Camera", typeof(Camera)); cameraObject.tag = "MainCamera"; cameraObject.transform.SetParent(transform, false);
            gameCamera = cameraObject.GetComponent<Camera>(); gameCamera.orthographic = true; gameCamera.orthographicSize = GameView.OrthographicSize;
            gameCamera.backgroundColor = new Color(.007f, .011f, .018f); gameCamera.clearFlags = CameraClearFlags.SolidColor; gameCamera.nearClipPlane = .1f;
            gameCamera.transform.position = new Vector3(4, GameView.GroundCameraY, -10);
            // Clear the complete native window before the fitted gameplay camera draws.
            // Pillarbox/letterbox margins stay black after a resize or fullscreen switch.
            var clearObject = new GameObject("Aspect margin clear", typeof(Camera)); clearObject.transform.SetParent(transform, false);
            marginCamera = clearObject.GetComponent<Camera>(); marginCamera.depth = -10; marginCamera.cullingMask = 0;
            marginCamera.clearFlags = CameraClearFlags.SolidColor; marginCamera.backgroundColor = Color.black;
            GameView.Fit(gameCamera, review ? GameView.RenderWidth : Screen.width, review ? GameView.RenderHeight : Screen.height);
            if (!review) { cameraObject.AddComponent<AudioListener>(); audioGame = new GameAudio(transform); }
            var events = new GameObject("UI Event System", typeof(EventSystem), typeof(StandaloneInputModule)); events.transform.SetParent(transform, false);
            save = ReadSave(); selectedZone = Mathf.Clamp(save.zone, 0, 3);
            LoadZone(selectedZone, false);
            hud = new GameHUD(this); hud.Bind(world); hud.SetMenu(true);
            Debug.Log("KARAMANI_REBORN_READY · Painted rig, world and native UI initialized");
        }

        public void LoadZone(int zone, bool playing, float checkpoint = 1.7f)
        {
            if (worldRoot) { if (Application.isPlaying) Destroy(worldRoot.gameObject); else DestroyImmediate(worldRoot.gameObject); }
            worldRoot = new GameObject("Chapter " + (zone + 1)).transform; worldRoot.SetParent(transform, false);
            world = new GameWorld(zone, worldRoot, checkpoint); stage = new StageArt(world.zone, worldRoot);
            world.sound = s => audioGame?.Play(s); world.checkpointSaved = SaveCheckpoint;
            selectedZone = zone; menu = !playing; paused = false; photoMode = false; journeyComplete = false; accumulator = 0; cameraVelocity = 0; chapterTime = playing ? 3.2f : 0;
            pendingInput = default;
            gameCamera.transform.position = new Vector3(Mathf.Clamp(checkpoint + 2, 4, 49), GameView.GroundCameraY, -10);
            stage.Tick(0, 1, gameCamera.transform.position);
            if (hud != null) { hud.Bind(world); hud.SetMenu(!playing); hud.SetPause(false); }
        }

        public void PreviewZone(int zone) => LoadZone(zone, false);
        public void PlaySelected() { LoadZone(selectedZone, true); SaveCheckpoint(); }
        public void ContinueJourney() => LoadZone(Mathf.Clamp(save.zone, 0, 3), true, Mathf.Clamp(save.checkpoint, 1.7f, 39));
        public void Respawn() => LoadZone(world.zone.index, true, world.checkpoint);
        public void ReturnToMenu() { menu = true; paused = false; journeyComplete = false; pendingInput = default; hud.SetMenu(true); hud.SetPause(false); }
        public void TogglePause() { if (menu || world.player.Dead || journeyComplete) return; paused = !paused; pendingInput = default; hud.SetPause(paused); }

        void Update()
        {
            if (reviewMode || !initialized) return;
            float dt = Mathf.Min(Time.unscaledDeltaTime, .08f); visualClock += dt;
            if (Input.GetKeyDown(KeyCode.Escape)) { if (menu) { } else TogglePause(); }
            if (Input.GetKeyDown(KeyCode.F2) && !menu) photoMode = !photoMode;
            if (!menu && !paused && !journeyComplete)
            {
                // Retain button edges across render-only frames and hit-stop. At refresh
                // rates above 120 Hz, a render frame may have no simulation step at all.
                // Savuşturmadan sonra kısa ağır çekim
                pendingInput.Accumulate(InputFrame.Read()); accumulator += dt * (world.effects.slowMotion > 0 ? .3f : 1f);
                if (world.effects.hitStop <= 0)
                {
                    int steps = 0;
                    while (accumulator >= 1f / 120 && steps++ < 12)
                    {
                        world.Simulate(1f / 120, pendingInput); accumulator -= 1f / 120;
                        pendingInput.ConsumeButtons();
                    }
                }
                else accumulator = 0;
                chapterTime = Mathf.Max(0, chapterTime - dt);
                AdvanceIfRequested();
            }
            else if (menu)
            { world.player.rig.Render(world.player, dt, visualClock); foreach (var enemy in world.enemies) enemy.rig.Render(enemy, dt, visualClock); }
            ShakeStruck();
            world.effects.Tick(dt);
            TickCamera(dt);
            stage.Tick(dt, visualClock, gameCamera.transform.position);
            hud.Tick(dt); hud.Follow(gameCamera);
        }

        public void AdvanceIfRequested()
        {
            if (!world.transitionRequested || journeyComplete) return;
            if (world.zone.index < 3) { LoadZone(world.zone.index + 1, true); SaveCheckpoint(); }
            else { journeyComplete = true; hud.ShowComplete(); }
        }

        public void TickCamera(float dt, bool immediate = false)
        {
            float target = Mathf.Clamp(world.player.position.x + world.player.face * 1.2f, 4, 49);
            Vector3 p = gameCamera.transform.position;
            p.x = immediate ? target : Mathf.SmoothDamp(p.x, target, ref cameraVelocity, .22f, 35, dt);
            float targetY = GameView.GroundCameraY + Mathf.Clamp((world.player.position.y - world.zone.floor - .4f) * .28f, 0, .75f);
            p.y = immediate ? targetY : Mathf.Lerp(p.y, targetY, 1 - Mathf.Exp(-dt * 4));
            // Sarsıntı darbe yönünde vurur, dik yönde hafif titrer; ağır vuruşta kamera kısa süre yaklaşır
            float shake = photoMode ? 0 : world.effects.cameraShake;
            Vector2 kick = world.effects.shakeDirection, side = new Vector2(-kick.y, kick.x);
            Vector2 offset = kick * Mathf.Sin(visualClock * 58) * shake + side * Mathf.Cos(visualClock * 91) * shake * .35f;
            p.x += offset.x; p.y += offset.y;
            gameCamera.transform.position = p;
            gameCamera.orthographicSize = GameView.OrthographicSize * (1 - (photoMode ? 0 : world.effects.zoomPunch));
            if (!reviewMode) GameView.Fit(gameCamera, Screen.width, Screen.height);
        }

        // Duraksama anında vurulan karakter yerinde titrer; duraksama bitince eski konumuna döner
        Actor jitterActor; Vector3 jitterBase;
        void ShakeStruck()
        {
            var struck = world.effects.lastStruck;
            if (world.effects.hitStop > 0 && struck?.rig != null)
            {
                if (jitterActor != struck) { RestoreJitter(); jitterActor = struck; jitterBase = struck.rig.root.position; }
                struck.rig.root.position = jitterBase + new Vector3(Mathf.Sin(visualClock * 140) * .045f, 0, 0);
            }
            else RestoreJitter();
        }
        void RestoreJitter()
        {
            if (jitterActor?.rig != null && jitterActor.rig.root) jitterActor.rig.root.position = jitterBase;
            jitterActor = null;
        }

        void OnApplicationFocus(bool focused) { if (!focused && initialized && !menu && !paused && !reviewMode) { paused = true; hud.SetPause(true); } }
        void SaveCheckpoint()
        {
            save = new SaveData { zone = world.zone.index, checkpoint = world.checkpoint };
            if (reviewMode) return;
            try { Directory.CreateDirectory(Application.persistentDataPath); File.WriteAllText(Path.Combine(Application.persistentDataPath, "journey.json"), JsonUtility.ToJson(save, true)); }
            catch (Exception e) { Debug.LogWarning("Save unavailable: " + e.Message); }
        }
        SaveData ReadSave()
        {
            if (reviewMode) return new SaveData();
            try { string path = Path.Combine(Application.persistentDataPath, "journey.json"); if (File.Exists(path)) return JsonUtility.FromJson<SaveData>(File.ReadAllText(path)) ?? new SaveData(); }
            catch (Exception e) { Debug.LogWarning("Save could not be loaded: " + e.Message); }
            return new SaveData();
        }
    }

    public sealed class GameAudio
    {
        readonly AudioSource source;
        readonly System.Collections.Generic.Dictionary<string, AudioClip> clips = new();
        public GameAudio(Transform parent)
        {
            var go = new GameObject("Sound"); go.transform.SetParent(parent, false); source = go.AddComponent<AudioSource>(); source.volume = .27f;
        }
        readonly System.Random pitchRandom = new(7);
        public void Play(string key)
        {
            if (!clips.TryGetValue(key, out var clip))
            {
                const int rate = 22050;
                var data = Synthesize(key, rate);
                clip = AudioClip.Create(key, data.Length, 1, rate, false); clip.SetData(data, 0); clips[key] = clip;
            }
            // Aynı ses art arda çalınınca tekdüze olmasın diye perde her seferinde biraz değişir
            bool varied = key is "hit" or "heavyhit" or "swing" or "heavy" or "block" or "step";
            source.pitch = varied ? .92f + (float)pitchRandom.NextDouble() * .16f : 1;
            source.PlayOneShot(clip, key is "hit" or "heavyhit" ? 1.25f : key is "swing" ? .8f : key is "step" ? .3f : key is "skid" ? .6f : 1);
        }

        // Kısa ses efektleri, dosya kullanmadan üretilir.
        static float[] Synthesize(string key, int rate)
        {
            var rng = new System.Random(key.GetHashCode());
            float duration = key switch { "shrine" => .9f, "spirit" => .6f, "heal" => .5f, "parry" => .7f, "shot" => .5f, "block" => .35f, "heavyhit" => .32f, "heavy" => .26f, "swing" => .2f, "hit" => .2f,
                "execute" => .7f, "skid" => .28f, "climb" => .3f, "land" => .2f, "grab" => .12f, "step" => .1f, _ => .16f };
            var data = new float[(int)(rate * duration)];
            float low = 0, low2 = 0, phase = 0, peak = .0001f;
            float[] partials = { 1740, 2610, 3720, 4980 };
            for (int i = 0; i < data.Length; i++)
            {
                float t = i / (float)rate, u = t / duration, noise = (float)rng.NextDouble() * 2 - 1, v;
                switch (key)
                {
                    case "swing":
                    case "heavy":
                    {
                        // Kılıç savurma: bant geçiren gürültü, açılıp kapanan zarf
                        float env = Mathf.Sin(Mathf.Clamp01(u) * Mathf.PI); env *= env;
                        float cutoff = Mathf.Lerp(.04f, key == "heavy" ? .22f : .38f, env);
                        low = Mathf.Lerp(low, noise, cutoff); low2 = Mathf.Lerp(low2, low, cutoff);
                        v = (low - low2) * env * 2.4f;
                        break;
                    }
                    case "hit":
                    case "heavyhit":
                    {
                        // Vuruş: düşen perdeli gövde darbesi ve kısa çatırtı
                        bool heavy = key == "heavyhit";
                        float f = Mathf.Lerp(heavy ? 120 : 160, 42, Mathf.Clamp01(t / .12f)); phase += f / rate;
                        float thump = Mathf.Sin(2 * Mathf.PI * phase) * Mathf.Exp(-t * (heavy ? 13 : 20));
                        low = Mathf.Lerp(low, noise, .55f);
                        v = thump * .95f + low * Mathf.Exp(-t * (heavy ? 32 : 45)) * .75f;
                        break;
                    }
                    case "block":
                    case "parry":
                    {
                        // Metal çınlaması: uyumsuz kısmi tonlar, savuşturmada daha parlak ve uzun
                        float ring = 0, shift = key == "parry" ? 1.12f : 1;
                        for (int k = 0; k < partials.Length; k++) ring += Mathf.Sin(2 * Mathf.PI * partials[k] * shift * t + k) * Mathf.Exp(-t * ((key == "parry" ? 5 : 8) + k * 4)) / (k + 1);
                        v = ring * .55f + noise * Mathf.Exp(-t * 60) * .6f;
                        break;
                    }
                    case "step":
                    case "land":
                    {
                        // Ayak sesi ve iniş: kısa, boğuk gövde darbesi
                        bool land = key == "land";
                        float f = Mathf.Lerp(land ? 95 : 85, 38, Mathf.Clamp01(t / .08f)); phase += f / rate;
                        low = Mathf.Lerp(low, noise, .3f);
                        v = Mathf.Sin(2 * Mathf.PI * phase) * Mathf.Exp(-t * (land ? 18 : 38)) + low * Mathf.Exp(-t * 60) * .5f;
                        break;
                    }
                    case "skid":
                    {
                        // Kayma: taşta sürtünen çakıl
                        float env = Mathf.Sin(Mathf.Clamp01(u) * Mathf.PI);
                        low = Mathf.Lerp(low, noise, .25f);
                        float grit = rng.NextDouble() > .88 ? noise : 0;
                        v = (low * .8f + grit * .6f) * env;
                        break;
                    }
                    case "grab":
                        low = Mathf.Lerp(low, noise, .5f);
                        v = low * Mathf.Exp(-t * 40);
                        break;
                    case "shot":
                        low = Mathf.Lerp(low, noise, .35f); phase += 58f / rate;
                        v = low * Mathf.Exp(-t * 9) * .9f + Mathf.Sin(2 * Mathf.PI * phase) * Mathf.Exp(-t * 10) * .7f;
                        break;
                    case "execute":
                    {
                        // İnfaz: derin darbe ve uzun çınlama
                        float f = Mathf.Lerp(110, 32, Mathf.Clamp01(t / .25f)); phase += f / rate;
                        float ring = 0;
                        for (int k = 0; k < partials.Length; k++) ring += Mathf.Sin(2 * Mathf.PI * partials[k] * .5f * t + k) * Mathf.Exp(-t * (5 + k * 3)) / (k + 1);
                        low = Mathf.Lerp(low, noise, .4f);
                        v = Mathf.Sin(2 * Mathf.PI * phase) * Mathf.Exp(-t * 7) + ring * .35f + low * Mathf.Exp(-t * 25) * .6f;
                        break;
                    }
                    case "jump":
                    case "jump2":
                    case "dodge":
                    case "climb":
                        low = Mathf.Lerp(low, noise, key == "jump2" ? .16f : .08f);
                        v = low * Mathf.Sin(Mathf.Clamp01(u) * Mathf.PI) * 1.6f;
                        break;
                    default:
                    {
                        // İyileşme, yol taşı, kurt: tınılı parıltı
                        float f = key == "spirit" ? 220 : key == "heal" ? 520 : 392;
                        v = (Mathf.Sin(2 * Mathf.PI * f * t) + .5f * Mathf.Sin(2 * Mathf.PI * f * 1.5f * t) + .3f * Mathf.Sin(2 * Mathf.PI * f * 2.01f * t)) * Mathf.Exp(-t * 3) * Mathf.Min(1, t * 30) * .35f
                            + noise * .04f * Mathf.Exp(-t * 4);
                        break;
                    }
                }
                // Sonda tık sesi olmasın diye kısa kapanış
                v *= Mathf.Clamp01((duration - t) * 40);
                data[i] = v; peak = Mathf.Max(peak, Mathf.Abs(v));
            }
            for (int i = 0; i < data.Length; i++) data[i] *= .9f / peak;
            return data;
        }
    }
}
