using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace KaramaniReborn
{
    // Opt-in audit in the actual Windows player. Normal launches never instantiate it.
    public sealed class RuntimeVerification : MonoBehaviour
    {
        public GameRoot game;
        string output;
        int frame;
        bool finished;
        float hpBefore;

        void Start()
        {
            var args = Environment.GetCommandLineArgs();
            int option = Array.IndexOf(args, "--karamani-output");
            output = option >= 0 && option + 1 < args.Length ? Path.GetFullPath(args[option + 1]) : Path.GetFullPath(Path.Combine(Application.dataPath, "../../Review"));
            Directory.CreateDirectory(output);
            // Exercise the production initialization, fonts, native UI and graphics device,
            // then drive the simulation deterministically without changing the user's save.
            game.reviewMode = true;
            AudioListener.volume = 0;
            Application.logMessageReceived += OnLog;
        }

        void Update()
        {
            if (finished) return;
            try
            {
                frame++;
                if (frame == 2) Capture("player-window.png", Screen.width, Screen.height, true);
                if (frame == 4)
                {
                    Check(game.menu && game.hud.canvas.isActiveAndEnabled, "Native menu did not initialize");
                    Check(Mathf.Abs(game.gameCamera.aspect - GameView.Aspect) < .00001f, "Native gameplay camera is not 4:3");
                    Rect viewport = game.gameCamera.rect;
                    Check(Mathf.Abs((viewport.width * Screen.width) / (viewport.height * Screen.height) - GameView.Aspect) < .0001f, "Native window distorted the 4:3 gameplay viewport");
                    Capture("player-menu.png");
                    bool clicked = false;
                    foreach (var button in game.hud.root.GetComponentsInChildren<Button>(true))
                        if (button.name == "YOLCULUĞA BAŞLA") { button.onClick.Invoke(); clicked = true; break; }
                    Check(clicked && !game.menu && !game.world.player.Dead, "Start button did not start a playable chapter");
                    game.chapterTime = 0;
                    Debug.Log("KARAMANI_PLAYER_MENU_PASS");
                }
                if (frame > 6 && frame <= 126)
                {
                    for (int step = 0; step < 2; step++)
                        game.world.Simulate(1f / 120, new InputFrame { move = frame < 55 ? 1 : 0, jump = frame == 56 && step == 0, jumpHeld = true }, false);
                    game.world.effects.Tick(1f / 60); game.TickCamera(1f / 60, true);
                    game.stage.Tick(1f / 60, game.world.clock, game.gameCamera.transform.position);
                    game.hud.Tick(1f / 60); game.hud.Follow(game.gameCamera);
                    if (frame == 80) Capture("player-jump.png");
                }
                if (frame == 127)
                {
                    var a = game.world.player;
                    Check(a.grounded && !a.Dead && a.position.x > 4, "Player motion, jump or landing failed");
                    Check(a.view.ScalePreserved && a.view.OnPixelGrid && a.view.FallbackFrames == 0 && (a.view.HasHd ? a.view.Steps > 2 : a.view.PlantedSamples > 5) && a.view.MaxFootSlip < PixelPipeline.PixelSize,
                        $"Single-piece player frames: scale {a.view.ScalePreserved}, grid {a.view.OnPixelGrid}, fallback {a.view.FallbackFrames}, hd {a.view.HasHd}, steps {a.view.Steps}, planted {a.view.PlantedSamples}, slip {a.view.MaxFootSlip}");
                    game.TogglePause(); Check(game.paused, "Pause did not stop gameplay");
                    game.TogglePause(); Check(!game.paused, "Resume failed");
                    a.position = new Vector2(4, game.world.zone.floor);
                    var enemy = game.world.enemies[0]; enemy.position = new Vector2(5.3f, game.world.zone.floor);
                    enemy.health = enemy.maxHealth = 300; hpBefore = enemy.health;
                    a.view.ResetContacts(); enemy.view.ResetContacts();
                    Debug.Log("KARAMANI_PLAYER_MOTION_PASS");
                }
                if (frame >= 128 && frame < 208)
                {
                    int attackFrame = (frame - 128) * 2;
                    for (int step = 0; step < 2; step++)
                        game.world.Simulate(1f / 120, new InputFrame { light = attackFrame + step == 0 || attackFrame + step == 22 || attackFrame + step == 74 }, false);
                    game.world.effects.Tick(1f / 60);
                    game.hud.Tick(1f / 60); game.hud.Follow(game.gameCamera);
                    if (frame == 143) Capture("player-combat.png");
                }
                if (frame == 208)
                {
                    Check(game.world.enemies[0].health < hpBefore - 40 && game.world.player.combo == 3, "Standalone blade contacts or combo failed");
                    game.world.player.position.x = 21; game.world.Simulate(1f / 120, new InputFrame { interact = true }, false);
                    Check(Mathf.Abs(game.world.checkpoint - 21) < .01f, "Checkpoint interaction failed");
                    game.Respawn();
                    Check(Mathf.Abs(game.world.player.position.x - 21) < .01f && !game.world.player.Dead, "Checkpoint respawn failed");
                    Debug.Log("KARAMANI_PLAYER_COMBAT_CHECKPOINT_PASS");
                }
                for (int zone = 0; zone < 4; zone++)
                {
                    if (frame == 214 + zone * 8) { game.LoadZone(zone, true); game.chapterTime = 0; }
                    if (frame == 218 + zone * 8)
                    {
                        Check(game.world.zone.index == zone && game.world.enemies.Count == 6, "Chapter initialization failed");
                        Check(game.stage.MaxWalkEdgeError < .001f, "Painted walkway shifted from the physical ground");
                        game.hud.Tick(.1f); game.hud.Follow(game.gameCamera);
                        Capture("player-chapter-" + (zone + 1) + ".png");
                    }
                }
                if (frame == 250)
                {
                    VerifyWindowCapture();
                    Debug.Log("KARAMANI_PLAYER_VERIFY_SUCCESS · actual Windows player, menu, movement, jump, blade contact, combo, pause, checkpoint, four chapters");
                    File.WriteAllText(Path.Combine(output, "player-verification.txt"), "PASS: standalone Windows player initialized without an exception\nPASS: 4:3 viewport without painting stretch\nPASS: window-sized native camera render, visible content and black aspect margins\nPASS: native menu Start action\nPASS: single-piece frame characters: unscaled, on the pixel grid, stride-locked feet\nPASS: movement, jump and landing\nPASS: real blade contacts and buffered three-hit combo\nPASS: pause / resume\nPASS: checkpoint interaction / respawn\nPASS: all four chapters initialized and rendered\nWindow: " + Screen.width + "x" + Screen.height + "\nGraphics: " + SystemInfo.graphicsDeviceName + "\nUnity: " + Application.unityVersion + "\n");
                    Finish(0);
                }
            }
            catch (Exception e) { Debug.LogError("KARAMANI_PLAYER_VERIFY_FAILED · " + e); Finish(1); }
        }

        static void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
        void VerifyWindowCapture()
        {
            string path = Path.Combine(output, "player-window.png");
            Check(File.Exists(path), "Window-sized native render was not produced");
            var image = new Texture2D(2, 2, TextureFormat.RGB24, false);
            Check(image.LoadImage(File.ReadAllBytes(path)), "Could not read the Unity window capture");
            Check(image.width == Screen.width && image.height == Screen.height, "Window capture has incorrect dimensions");
            Rect viewport = GameView.ViewportFor(image.width, image.height);
            bool visibleContent = false;
            for (int y = 1; y < 5; y++) for (int x = 1; x < 5; x++)
            {
                var color = image.GetPixel(Mathf.FloorToInt((viewport.xMin + viewport.width * x / 5) * (image.width - 1)),
                    Mathf.FloorToInt((viewport.yMin + viewport.height * y / 5) * (image.height - 1)));
                visibleContent |= color.r > .05f || color.g > .05f || color.b > .05f;
            }
            Check(visibleContent, "Window-sized render is blank; black margins alone are not proof");
            if (viewport.xMin > .01f)
            {
                int margin = Mathf.FloorToInt(viewport.xMin * image.width * .5f);
                Check(IsBlack(image.GetPixel(margin, image.height / 2)) && IsBlack(image.GetPixel(image.width - margin - 1, image.height / 2)), "Pillarbox margins are not black");
            }
            if (viewport.yMin > .01f)
            {
                int margin = Mathf.FloorToInt(viewport.yMin * image.height * .5f);
                Check(IsBlack(image.GetPixel(image.width / 2, margin)) && IsBlack(image.GetPixel(image.width / 2, image.height - margin - 1)), "Letterbox margins are not black");
            }
            Destroy(image);
            Debug.Log("KARAMANI_PLAYER_WINDOW_PASS · window-sized native render, visible content and aspect margins");
        }
        static bool IsBlack(Color color) => color.r < .02f && color.g < .02f && color.b < .02f;
        void OnLog(string condition, string trace, LogType type)
        { if (type == LogType.Exception || type == LogType.Assert || type == LogType.Error) Finish(1); }
        void Finish(int code)
        { if (finished) return; finished = true; Application.logMessageReceived -= OnLog; Application.Quit(code); }

        void Capture(string name) => Capture(name, GameView.RenderWidth, GameView.RenderHeight, false);

        void Capture(string name, int width, int height, bool includeMargins)
        {
            var camera = game.gameCamera; var oldTarget = camera.targetTexture; var oldRect = camera.rect;
            var oldMarginTarget = game.marginCamera.targetTexture;
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32); rt.Create();
            // Batch mode has no reliable final back buffer. Render the same production
            // cameras explicitly at the real window size, preserving its fitted viewport.
            game.marginCamera.targetTexture = rt; game.marginCamera.Render();
            camera.rect = includeMargins ? oldRect : new Rect(0, 0, 1, 1); camera.targetTexture = rt;
            Canvas.ForceUpdateCanvases(); camera.Render();
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            var old = RenderTexture.active; RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0); tex.Apply();
            File.WriteAllBytes(Path.Combine(output, name), tex.EncodeToPNG());
            RenderTexture.active = old; camera.targetTexture = oldTarget; camera.rect = oldRect;
            game.marginCamera.targetTexture = oldMarginTarget;
            Destroy(tex); Destroy(rt);
        }
    }
}
