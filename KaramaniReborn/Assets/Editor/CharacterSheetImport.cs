using UnityEditor;
using UnityEngine;

namespace KaramaniReborn.Editor
{
    // Tek parça karakter kare ızgaraları piksel sanatıdır: nokta süzme, sıkıştırmasız, mipmap yok.
    // Okunabilir doku, karelerin görünen boyunu (kahraman / ekran oranı denetimi) ölçmek için gerekir.
    public sealed class CharacterSheetImport : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/Resources/Characters/")) return;
            var ti = (TextureImporter)assetImporter;
            ti.textureType = TextureImporterType.Default; ti.isReadable = true; ti.mipmapEnabled = false;
            ti.alphaIsTransparency = false; ti.npotScale = TextureImporterNPOTScale.None; ti.wrapMode = TextureWrapMode.Clamp;
            ti.filterMode = FilterMode.Point; ti.textureCompression = TextureImporterCompression.Uncompressed; ti.maxTextureSize = 4096;
        }
    }
}
