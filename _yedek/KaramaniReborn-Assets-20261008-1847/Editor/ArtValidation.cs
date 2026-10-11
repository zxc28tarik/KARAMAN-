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

        [MenuItem("Karamani/Animation/Review Painted Rigs")]
        public static void ReviewRigs()
        {
            PrepareArt(); EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var parent = new GameObject("Review").transform;
            var stage = new StageArt(ZoneDefinition.Create(0), parent);
            var camera = CameraForReview(3.5f); camera.transform.position = new Vector3(2, -.5f, -10);
            var hero = new Actor { player = true, position = new Vector2(0, -2.25f) }; new PaintedRig(hero, parent);
            var guard = new Actor { position = new Vector2(4.4f, -2.25f), face = -1, role = EnemyRole.Shield }; new PaintedRig(guard, parent);
            stage.Tick(0, 1, camera.transform.position);
            hero.rig.Render(hero, 0, 1); guard.rig.Render(guard, 0, 1);
            Capture(camera, "Review/rig-closeup.png");
            float dt = 1f / 120;
            for (int frame = 0; frame < 360; frame++)
            { hero.velocity.x = 4.25f; hero.position.x += hero.velocity.x * dt; hero.state = ActionState.Move; hero.rig.Render(hero, dt, frame * dt); }
            Assert(hero.rig.PaintScalePreserved, "Paint scale changed in locomotion");
            Assert(hero.rig.MaxFootSlip < .001f, "Foot sliding: " + hero.rig.MaxFootSlip);
            Assert(hero.rig.MaxBoneError < .001f, "Bone changed length: " + hero.rig.MaxBoneError);
            Assert(hero.rig.PlantedSamples > 200, "Insufficient stance samples");
            Debug.Log($"KARAMANI_RIG_PASS boneError={hero.rig.MaxBoneError:F6} footSlip={hero.rig.MaxFootSlip:F6} planted={hero.rig.PlantedSamples} rigidPainting={hero.rig.PaintScalePreserved}");
        }

        public static void Assert(bool ok, string message) { if (!ok) throw new InvalidOperationException(message); }
    }
}
