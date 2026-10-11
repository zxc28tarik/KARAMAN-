using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace KaramaniReborn.Editor
{
    public static class BuildTools
    {
        [MenuItem("Karamani/Projeyi hazırla")]
        public static void Configure()
        {
            ArtValidation.PrepareArt();
            Directory.CreateDirectory("Assets/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("Karamani Reborn").AddComponent<GameRoot>();
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Karamani.unity");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/Karamani.unity", true) };
            PlayerSettings.companyName = "Karamani"; PlayerSettings.productName = "KaramaniReborn";
            PlayerSettings.defaultScreenWidth = GameView.RenderWidth; PlayerSettings.defaultScreenHeight = GameView.RenderHeight;
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow; PlayerSettings.runInBackground = true;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, new[] { GraphicsDeviceType.Direct3D11 });
            QualitySettings.antiAliasing = 2; QualitySettings.vSyncCount = 1;
            AssetDatabase.SaveAssets(); Debug.Log("KARAMANI_CONFIGURED");
        }

        [MenuItem("Karamani/Sahneyi önizle")]
        public static void Preview()
        {
            ArtValidation.PrepareArt(); EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("Preview").AddComponent<GameRoot>(); root.Initialize(true); root.menu = false; root.hud.SetMenu(false); root.hud.Tick(.1f);
            Selection.activeGameObject = root.gameObject;
        }

        [MenuItem("Karamani/Windows sürümünü derle")]
        public static void BuildWindows()
        {
            Configure(); Directory.CreateDirectory("Build");
            var report = BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, "Build/KaramaniReborn.exe", BuildTarget.StandaloneWindows64, BuildOptions.None);
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Windows build failed: " + report.summary.result);
            Debug.Log("KARAMANI_BUILD_SUCCESS · " + report.summary.totalSize + " bytes");
        }

        [MenuItem("Karamani/Hareket ve savaş doğrulaması")]
        public static void Verify()
        {
            Configure();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("Verification").AddComponent<GameRoot>(); root.Initialize(true);
            VerifyPresentation(root);
            root.menu = false; root.hud.SetMenu(false);
            VerifyInput(); VerifyLocomotion(root); VerifyJump(root); VerifyCombat(root); VerifyRanged(root); VerifyJourney(root);
            CaptureChapters(root); CaptureAnimation(root);
            File.WriteAllText("Review/verification.txt", "PASS: 4:3 presentation and uniform pixel projection on 4:3, 16:9, ultrawide and portrait windows\nPASS: menu, pause and gameplay HUD stay inside the 4:3 viewport\nPASS: idle hero occupies 20-26 percent of viewport height without source-art resizing\nPASS: ten source PNG dimensions match imported Unity textures exactly\nPASS: no power-of-two resizing / no texture compression\nPASS: fixed sprite scales across all poses\nPASS: visible world-locked boot pivots (<0.001 units)\nPASS: visible two-link arm and leg endpoints (<0.001 units)\nPASS: high-refresh and hit-stop input latch\nPASS: buffered three-hit combo and actual blade intersection\nPASS: timed parry and dodge invulnerability\nPASS: jump takeoff, apex and landing\nPASS: pistol contact and elevated archer aim\nPASS: checkpoint interaction and respawn\nPASS: four chapter gate transitions and journey completion\nPASS: painted walkway / physical surface alignment (<0.001 units)\nPASS: four chapters rendered by Unity with native HUD\n");
            Debug.Log("KARAMANI_VERIFY_SUCCESS");
        }

        static void VerifyPresentation(GameRoot root)
        {
            foreach (var size in new[] { new Vector2Int(1440, 1080), new Vector2Int(1920, 1080), new Vector2Int(2560, 1080), new Vector2Int(1080, 1920) })
            {
                Rect viewport = GameView.ViewportFor(size.x, size.y);
                float pixelWidth = viewport.width * size.x, pixelHeight = viewport.height * size.y;
                ArtValidation.Assert(Mathf.Abs(pixelWidth / pixelHeight - GameView.Aspect) < .00001f, "Viewport changed the intended 4:3 aspect");
                float pixelsPerUnitX = pixelWidth / (GameView.OrthographicSize * 2 * GameView.Aspect);
                float pixelsPerUnitY = pixelHeight / (GameView.OrthographicSize * 2);
                ArtValidation.Assert(Mathf.Abs(pixelsPerUnitX - pixelsPerUnitY) < .0001f, "Window shape stretched painted sprites");
                ArtValidation.Assert(Mathf.Abs(viewport.center.x - .5f) < .00001f && Mathf.Abs(viewport.center.y - .5f) < .00001f, "Viewport is not centered");
            }
            ArtValidation.Assert(Mathf.Abs(root.gameCamera.aspect - GameView.Aspect) < .00001f, "Production camera is not 4:3");
            ArtValidation.Assert(root.hud.root.sizeDelta == new Vector2(GameView.CanvasWidth, GameView.CanvasHeight), "HUD canvas has the wrong aspect");
            root.hud.Follow(root.gameCamera);
            var hero = root.world.player;
            float heroHeight = hero.rig.root.Find("Head").GetComponent<SpriteRenderer>().bounds.max.y - hero.rig.root.Find("Boot").GetComponent<SpriteRenderer>().bounds.min.y;
            float occupancy = heroHeight / (root.gameCamera.orthographicSize * 2);
            ArtValidation.Assert(occupancy > .20f && occupancy < .26f, "Hero-to-viewport proportions changed unexpectedly: " + occupancy);
            AssertVisibleHUD(root);
            root.menu = false; root.hud.SetMenu(false); root.TogglePause(); root.hud.Tick(.1f); AssertVisibleHUD(root);
            root.TogglePause(); root.hud.Tick(.1f); AssertVisibleHUD(root);
            root.ReturnToMenu();
            Debug.Log($"KARAMANI_VIEW_PASS · 4:3, heroHeight={heroHeight:F4}, viewportOccupancy={occupancy:P2}, native art and HUD proportions preserved");
        }

        static void AssertVisibleHUD(GameRoot root)
        {
            Canvas.ForceUpdateCanvases(); var corners = new Vector3[4];
            foreach (var graphic in root.hud.root.GetComponentsInChildren<Graphic>())
            {
                if (!graphic.isActiveAndEnabled || graphic.transform.parent.name == "Enemy health") continue;
                graphic.rectTransform.GetWorldCorners(corners);
                foreach (var corner in corners)
                {
                    Vector3 point = root.gameCamera.WorldToViewportPoint(corner);
                    ArtValidation.Assert(point.x >= -.005f && point.x <= 1.005f && point.y >= -.005f && point.y <= 1.005f,
                        "Native HUD is cropped by the 4:3 viewport: " + graphic.name + " " + point);
                }
            }
        }

        static void VerifyLocomotion(GameRoot root)
        {
            root.LoadZone(0, true); var a = root.world.player; a.position.x = 2.4f;
            for (int i = 0; i < 240; i++) root.world.Simulate(1f / 120, new InputFrame { move = 1, slow = true }, false);
            for (int i = 0; i < 240; i++) root.world.Simulate(1f / 120, new InputFrame { move = -1 }, false);
            ArtValidation.Assert(a.rig.MaxBoneError < .001f, "Locomotion bone length changed: " + a.rig.MaxBoneError);
            ArtValidation.Assert(a.rig.MaxFootSlip < .001f, "Feet slid on ground");
            ArtValidation.Assert(a.rig.PaintScalePreserved, "Paint was stretched in motion");
            ArtValidation.Assert(a.rig.PlantedSamples > 300, "Not enough planted foot samples");
            Debug.Log($"KARAMANI_WALK_PASS · {a.rig.PlantedSamples} planted samples, bone={a.rig.MaxBoneError:F6}, slip={a.rig.MaxFootSlip:F6}");
        }

        static void VerifyInput()
        {
            var pending = new InputFrame();
            pending.Accumulate(new InputFrame { move = 1, light = true, jump = true, block = true });
            for (int i = 0; i < 4; i++) pending.Accumulate(new InputFrame { move = -1, block = false });
            ArtValidation.Assert(pending.light && pending.jump && pending.move == -1 && !pending.block, "Input edges were lost before a simulation step");
            pending.ConsumeButtons();
            ArtValidation.Assert(!pending.light && !pending.jump && pending.move == -1, "Input edges repeated after consumption");
            Debug.Log("KARAMANI_INPUT_PASS · high-refresh and hit-stop input latch");
        }

        static void VerifyJump(GameRoot root)
        {
            root.LoadZone(0, true); var a = root.world.player; a.position.x = 4;
            float max = a.position.y; bool airborne = false;
            for (int i = 0; i < 150; i++)
            { root.world.Simulate(1f / 120, new InputFrame { jump = i == 0, jumpHeld = true }, false); max = Mathf.Max(max, a.position.y); airborne |= !a.grounded; }
            ArtValidation.Assert(airborne && max - root.world.zone.floor > 1.7f && a.grounded, "Jump did not take off/apex/land correctly");
            ArtValidation.Assert(a.rig.PaintScalePreserved && a.rig.MaxBoneError < .001f, "Jump distorted art or disconnected bones");
            Debug.Log("KARAMANI_JUMP_PASS · height=" + (max - root.world.zone.floor));
        }

        static void VerifyCombat(GameRoot root)
        {
            root.LoadZone(0, true); var w = root.world; var a = w.player; var target = w.enemies[0];
            a.position.x = 4; target.position = new Vector2(5.3f, w.zone.floor); target.health = target.maxHealth = 300;
            a.rig.ResetContacts(); target.rig.ResetContacts();
            for (int i = 0; i < 160; i++) w.Simulate(1f / 120, new InputFrame { light = i == 0 || i == 22 || i == 74 }, false);
            ArtValidation.Assert(target.health < 260 && a.combo == 3, "Buffered combo did not connect three attacks");
            ArtValidation.Assert(a.rig.PaintScalePreserved && a.rig.MaxBoneError < .001f, "Combat changed painted part scales or bone lengths");
            root.LoadZone(0, true); w = root.world; a = w.player; target = w.enemies[0];
            a.position.x = 4; target.position = new Vector2(5.1f, w.zone.floor); target.face = -1;
            target.Enter(ActionState.Light, .7f); target.stateTime = .345f;
            w.Simulate(1f / 120, new InputFrame { block = true }, true);
            ArtValidation.Assert(a.health == a.maxHealth && target.state == ActionState.Hurt, "Timed parry did not prevent damage and stagger the attacker");
            a.Enter(ActionState.Idle); w.Simulate(1f / 120, new InputFrame { dodge = true }, false);
            float hp = a.health; w.Damage(a, 20, -1, false, false, target);
            ArtValidation.Assert(a.health == hp && a.state == ActionState.Dodge, "Dodge invulnerability failed");
            Debug.Log("KARAMANI_COMBAT_PASS · blade contact, buffered combo, parry, dodge");
        }

        static void CaptureChapters(GameRoot root)
        {
            for (int zone = 0; zone < 4; zone++)
            {
                root.LoadZone(zone, true); root.chapterTime = 0; root.world.player.position.x = 4.7f;
                ArtValidation.Assert(root.stage.MaxWalkEdgeError < .001f, "Painted walkway did not align with the physical surface");
                var enemy = root.world.enemies[0]; enemy.position = new Vector2(6.4f, root.world.zone.floor);
                root.world.player.Enter(ActionState.Light, .44f); root.world.player.stateTime = .21f;
                root.world.player.rig.ResetContacts(); enemy.rig.ResetContacts();
                root.world.player.rig.Render(root.world.player, .016f, 1); enemy.rig.Render(enemy, .016f, 1);
                root.world.effects.Impact(enemy.position + Vector2.up * 1.5f, 1, false, false);
                root.gameCamera.transform.position = new Vector3(5.8f, GameView.GroundCameraY, -10);
                root.stage.Tick(0, 1, root.gameCamera.transform.position); root.hud.Tick(.1f); root.hud.Follow(root.gameCamera);
                ArtValidation.Capture(root.gameCamera, "Review/chapter-" + (zone + 1) + ".png");
            }
            root.ReturnToMenu(); root.hud.Tick(.1f); root.hud.Follow(root.gameCamera);
            ArtValidation.Capture(root.gameCamera, "Review/menu.png");
        }

        static void VerifyJourney(GameRoot root)
        {
            root.LoadZone(0, true); var w = root.world;
            w.player.position.x = 21; w.Simulate(1f / 120, new InputFrame { interact = true }, false);
            ArtValidation.Assert(w.checkpoint == 21 && w.player.potions == 3, "Checkpoint interaction failed");
            w.Damage(w.player, 1000, 1, true, true); root.Respawn();
            ArtValidation.Assert(root.world.player.position.x == 21 && !root.world.player.Dead, "Checkpoint respawn failed");
            root.LoadZone(0, true);
            for (int zone = 0; zone < 4; zone++)
            {
                w = root.world; ArtValidation.Assert(w.zone.index == zone, "Unexpected chapter in journey");
                var boss = w.enemies.Find(a => a.boss); w.Damage(boss, 1000, 1, true, true);
                w.player.position.x = 55; w.Simulate(1f / 120, new InputFrame { interact = true }, false);
                ArtValidation.Assert(w.transitionRequested, "Defeated captain did not unlock the gate");
                root.AdvanceIfRequested();
            }
            ArtValidation.Assert(root.journeyComplete && !root.menu, "Fourth gate did not complete the journey");
            root.ReturnToMenu(); ArtValidation.Assert(root.menu && !root.journeyComplete, "Completion menu return failed");
            Debug.Log("KARAMANI_JOURNEY_PASS · checkpoints, death/respawn, four gates, completion");
        }

        static void VerifyRanged(GameRoot root)
        {
            root.LoadZone(0, true); var w = root.world;
            w.player.position.x = 4; w.enemies[0].position.x = 6;
            for (int i = 0; i < 120; i++) w.Simulate(1f / 120, new InputFrame { shoot = i == 0 }, false);
            ArtValidation.Assert(w.player.ammo == 7 && w.enemies[0].health < w.enemies[0].maxHealth, "Pistol muzzle, ammunition or projectile contact failed");
            root.LoadZone(0, true); w = root.world; w.player.position.x = 3;
            foreach (var enemy in w.enemies) enemy.position.x = 56;
            var archer = w.enemies[2]; archer.position = new Vector2(6, -.75f); archer.face = -1;
            archer.cooldown = archer.intentTime = 3; archer.Enter(ActionState.Shoot, .94f); archer.stateTime = .61f;
            archer.rig.ResetContacts(); archer.rig.Render(archer, .016f, 1);
            for (int i = 0; i < 100; i++) w.Simulate(1f / 120, new InputFrame(), true);
            ArtValidation.Assert(w.player.health < w.player.maxHealth, "Elevated archer did not aim at the ground-level player");
            Debug.Log("KARAMANI_RANGED_PASS · held-weapon origin, pistol contact, elevated archer aim");
        }

        static void CaptureAnimation(GameRoot root)
        {
            root.LoadZone(0, true); root.chapterTime = 0;
            for (int i = 0; i < 150; i++)
            {
                if (i == 61)
                {
                    var enemy = root.world.enemies[0]; enemy.position = root.world.player.position + Vector2.right * 1.3f;
                    enemy.health = enemy.maxHealth = 300; enemy.rig.ResetContacts();
                }
                root.world.Simulate(1f / 60, new InputFrame { move = i >= 9 && i < 60 ? 1 : 0, light = i == 63 || i == 78 || i == 100 }, false);
                root.world.effects.Tick(1f / 60); root.TickCamera(1f / 60, true);
                root.stage.Tick(1f / 60, root.world.clock, root.gameCamera.transform.position); root.hud.Tick(1f / 60); root.hud.Follow(root.gameCamera);
                if (i % 4 == 0) ArtValidation.Capture(root.gameCamera, "Review/animation/frame-" + i.ToString("D3") + ".png", GameView.MotionWidth, GameView.MotionHeight);
            }
            // Pose gallery: the original textures and their dimensions are identical in every state.
            var states = new[] { ActionState.Idle, ActionState.Move, ActionState.Jump, ActionState.Light, ActionState.Heavy, ActionState.Block, ActionState.Dodge, ActionState.Heal, ActionState.Shoot, ActionState.Spirit };
            for (int i = 0; i < states.Length; i++)
            {
                var a = root.world.player; a.position = new Vector2(3, root.world.zone.floor); a.face = 1; a.grounded = states[i] != ActionState.Jump;
                a.velocity = states[i] == ActionState.Move ? new Vector2(4.25f, 0) : states[i] == ActionState.Jump ? new Vector2(2, 3) : Vector2.zero;
                a.Enter(states[i], .8f); a.stateTime = .36f; a.rig.ResetContacts(); a.rig.Render(a, .016f, 2);
                root.gameCamera.transform.position = new Vector3(3, -1.03f, -10); root.gameCamera.orthographicSize = 1.65f;
                root.photoMode = true; root.hud.Tick(.1f); root.hud.Follow(root.gameCamera); root.stage.Tick(0, 2, root.gameCamera.transform.position);
                ArtValidation.Capture(root.gameCamera, "Review/poses/" + states[i] + ".png", 480, 360);
                ArtValidation.Assert(a.rig.PaintScalePreserved && a.rig.MaxBoneError < .001f, "Pose changed original painting: " + states[i]);
            }
        }
    }
}
