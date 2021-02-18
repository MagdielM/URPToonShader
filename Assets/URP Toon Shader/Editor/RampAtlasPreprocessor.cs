using UnityEditor;
using UnityEngine;

namespace ToonShaderURP
{
    public class RampAtlasPreprocessor : AssetPostprocessor
    {
        public static bool isRampTexture;

        private void OnPreprocessTexture()
        {
            if (isRampTexture)
            {
                TextureImporter textureImporter = (TextureImporter)assetImporter;
                textureImporter.mipmapEnabled = false;
                textureImporter.npotScale = TextureImporterNPOTScale.None;
                textureImporter.filterMode = FilterMode.Bilinear;
                textureImporter.wrapMode = TextureWrapMode.Clamp;
                isRampTexture = false;
            }
        }
    }
}