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
                pendingInput.Accumulate(InputFrame.Read()); accumulator += dt;
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
            float shake = photoMode ? 0 : world.effects.cameraShake;
            p.x += Mathf.Sin(visualClock * 77) * shake; p.y += Mathf.Cos(visualClock * 93) * shake;
            gameCamera.transform.position = p;
            if (!reviewMode) GameView.Fit(gameCamera, Screen.width, Screen.height);
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
        public void Play(string key)
        {
            if (!clips.TryGetValue(key, out var clip))
            {
                int rate = 22050, length = key == "spirit" || key == "shrine" ? 12000 : 4500;
                var data = new float[length]; var rng = new System.Random(key.GetHashCode()); float filtered = 0;
                bool metal = key is "block" or "parry", soft = key is "jump" or "dodge" or "swing";
                for (int i = 0; i < length; i++)
                {
                    float t = i / (float)rate, envelope = Mathf.Exp(-t * (soft ? 24 : metal ? 20 : 14));
                    filtered = Mathf.Lerp(filtered, (float)rng.NextDouble() * 2 - 1, soft ? .12f : .6f);
                    float frequency = key == "shot" ? 80 : metal ? 1740 : key == "heal" ? 660 : key == "shrine" ? 440 : key == "spirit" ? 220 : 120;
                    data[i] = (filtered * (metal ? .24f : .55f) + Mathf.Sin(2 * Mathf.PI * frequency * t) * .20f) * envelope;
                }
                clip = AudioClip.Create(key, length, 1, rate, false); clip.SetData(data, 0); clips[key] = clip;
            }
            source.PlayOneShot(clip);
        }
    }
}
