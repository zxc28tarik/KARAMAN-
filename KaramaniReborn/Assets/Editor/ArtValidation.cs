using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace KaramaniReborn.Editor
{
    public static class ArtValidation
    {
        public static void PrepareArt()
        {
            foreach (var path in Directory.GetFiles("Assets/Resources/Art", "*.png"))
            {
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                var ti = AssetImporter.GetAtPath(path) as TextureImporter;
                ti.textureType = TextureImporterType.Default; ti.isReadable = true; ti.mipmapEnabled = false;
                ti.alphaIsTransparency = !Path.GetFileName(path).StartsWith("background");
                ti.npotScale = TextureImporterNPOTScale.None; ti.wrapMode = TextureWrapMode.Clamp;
                ti.filterMode = FilterMode.Bilinear; ti.textureCompression = TextureImporterCompression.Uncompressed; ti.maxTextureSize = 4096;
                ti.SaveAndReimport();
                byte[] header = File.ReadAllBytes(path);
                int width = ReadBigEndian(header, 16), height = ReadBigEndian(header, 20);
                var imported = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                Assert(imported.width == width && imported.height == height, "Unity resized the source painting: " + path);
                Debug.Log($"KARAMANI_ART_PASS · {Path.GetFileName(path)} {width}x{height}, uncompressed, no aspect / resolution changes");
            }
            ArtLibrary.Initialize();
        }

        static int ReadBigEndian(byte[] data, int offset) => (data[offset] << 24) | (data[offset + 1] << 16) | (data[offset + 2] << 8) | data[offset + 3];

        public static Camera CameraForReview(float height = GameView.OrthographicSize)
        {
            var camera = new GameObject("Review Camera").AddComponent<Camera>();
            camera.orthographic = true; camera.orthographicSize = height; camera.aspect = GameView.Aspect;
            camera.transform.position = new Vector3(4.0f, GameView.GroundCameraY, -10);
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.016f, .023f, .035f);
            return camera;
        }

        public static void Capture(Camera camera, string path, int width = GameView.RenderWidth, int height = GameView.RenderHeight)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            rt.Create(); var oldTarget = camera.targetTexture; var oldRect = camera.rect;
            camera.rect = new Rect(0, 0, 1, 1); camera.targetTexture = rt;
            Canvas.ForceUpdateCanvases(); camera.Render();
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            var old = RenderTexture.active; RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, width, height), 0, 0); tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            RenderTexture.active = old; camera.targetTexture = oldTarget; camera.rect = oldRect;
            UnityEngine.Object.DestroyImmediate(tex); UnityEngine.Object.DestroyImmediate(rt);
        }

        // Tek parça karakter kare ızgaralarını piksel sanatı ayarlarıyla yeniden içe aktarır (CharacterSheetImport)
        public static void PrepareCharacters()
        {
            if (!Directory.Exists("Assets/Resources/Characters")) return;
            foreach (var path in Directory.GetFiles("Assets/Resources/Characters", "*.png"))
            {
                string asset = path.Replace('\\', '/');
                AssetDatabase.ImportAsset(asset, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
                var ti = (TextureImporter)AssetImporter.GetAtPath(asset);
                Assert(ti.filterMode == FilterMode.Point && !ti.mipmapEnabled && ti.isReadable && ti.textureCompression == TextureImporterCompression.Uncompressed,
                    "Character sheet import settings are not pixel-art safe: " + asset);
            }
        }

        // Altı türün tek parça karakterleri yan yana: bekleme ve saldırı kareleri, oyun pikselinin tam 6 katı
        [MenuItem("Karamani/Animation/Review Characters")]
        public static void ReviewCharacters()
        {
            PrepareArt(); PrepareCharacters(); EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            LightField.Reset(new Vector2(1, .35f), new Color(1, .62f, .32f), .35f);
            var parent = new GameObject("Review").transform;
            var camera = CameraForReview(3.6f); camera.transform.position = new Vector3(4.4f, -.4f, -10);
            camera.backgroundColor = new Color(.27f, .2f, .3f);
            var actors = new Actor[6];
            EnemyRole[] roles = { EnemyRole.Nefer, EnemyRole.Nefer, EnemyRole.Spearman, EnemyRole.Archer, EnemyRole.Captain, EnemyRole.Sorcerer };
            for (int i = 0; i < actors.Length; i++)
            {
                var role = roles[i];
                actors[i] = new Actor { player = i == 0, role = role, boss = role == EnemyRole.Captain, position = new Vector2(.4f + i * 1.6f, -2.25f), size = i == 0 ? 1 : EnemyProfile.For(role).size };
                new PixelCharacter(actors[i], parent);
            }
            Capture(camera, "Review/characters/ingame-lineup-idle.png");
            foreach (var a in actors) { a.Enter(ActionState.Light, .44f); a.combo = 1; a.stateTime = .2f; a.view.Render(a, .016f, 1); }
            Capture(camera, "Review/characters/ingame-lineup-attack.png");
            var hero = actors[0]; hero.Enter(ActionState.Move); hero.view.ResetContacts();
            float dt = 1f / 120;
            for (int frame = 0; frame < 360; frame++)
            { hero.velocity.x = 4.25f; hero.position.x += hero.velocity.x * dt; hero.state = ActionState.Move; hero.view.Render(hero, dt, frame * dt); }
            Assert(hero.view.ScalePreserved && hero.view.OnPixelGrid, "Character frame scaled or off the pixel grid in locomotion");
            Assert(hero.view.MaxFootSlip < PixelPipeline.PixelSize, "Foot sliding: " + hero.view.MaxFootSlip);
            Assert(hero.view.HasHd ? hero.view.Steps > 4 : hero.view.PlantedSamples > 20, "Insufficient stance samples");
            Debug.Log($"KARAMANI_CHARACTER_REVIEW_PASS footSlip={hero.view.MaxFootSlip:F6} planted={hero.view.PlantedSamples} steps={hero.view.Steps} singlePiece=true");
        }

        public static void Assert(bool ok, string message) { if (!ok) throw new InvalidOperationException(message); }
    }
}
