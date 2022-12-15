using UnityEditor;
using UnityEngine;

namespace Orazum.Utilities
{
    public static class AssetUtilities
    {
        public static Texture2D[] GetTextures(string texturesFolderPath)
        {
            string[] assetGUIDs = AssetDatabase.FindAssets($"t:texture2D", new[] { texturesFolderPath });
            Texture2D[] textures = new Texture2D[assetGUIDs.Length];
            for (int i = 0; i < assetGUIDs.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                textures[i] = texture;
            }

            return textures;
        }
    }
}
