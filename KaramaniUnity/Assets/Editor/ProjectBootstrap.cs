using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Karamani.Editor
{
    public static class ProjectBootstrap
    {
        [MenuItem("Karamani/Projeyi Hazırla")]
        public static void Configure()
        {
            ConfigureTexture("Assets/Resources/Art/fortress.png", 100, false, 4096);
            ConfigureTexture("Assets/Resources/Art/hero.png", 100, true, 2048);
            ConfigureTexture("Assets/Resources/Art/guard.png", 100, true, 2048);
            Directory.CreateDirectory("Assets/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("Karamani Game").AddComponent<Karamani.KaramaniGame>();
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Karamani.unity");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/Karamani.unity", true) };
            PlayerSettings.productName = "Karamani - Sinirdaki Sis";
            PlayerSettings.companyName = "Karamani";
            PlayerSettings.defaultScreenWidth = 1920; PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.runInBackground = true;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            Debug.Log("KARAMANI_SETUP_OK");
        }

        static void ConfigureTexture(string path, float ppu, bool alpha, int maxSize)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            if (AssetImporter.GetAtPath(path) is not TextureImporter ti) return;
            ti.textureType = TextureImporterType.Sprite; ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = ppu; ti.alphaIsTransparency = alpha; ti.mipmapEnabled = false;
            ti.filterMode = FilterMode.Bilinear; ti.textureCompression = TextureImporterCompression.CompressedHQ;
            var settings = ti.GetDefaultPlatformTextureSettings(); settings.maxTextureSize = maxSize; ti.SetPlatformTextureSettings(settings);
            ti.SaveAndReimport();
        }

        [MenuItem("Karamani/Windows Build Al")]
        public static void BuildWindows()
        {
            Configure(); Directory.CreateDirectory("Build");
            var report = BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, "Build/Karamani.exe", BuildTarget.StandaloneWindows64, BuildOptions.None);
            Debug.Log("KARAMANI_BUILD_" + report.summary.result);
        }
    }
}
