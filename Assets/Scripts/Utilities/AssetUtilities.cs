using UnityEngine;

namespace Orazum.Utilities
{
    public static class AssetUtilities
    {
        public static Texture2D[] GetTextures(string texturesFolderPath)
        {
#if UNITY_EDITOR
            string[] assetGUIDs = UnityEditor.AssetDatabase.FindAssets($"t:texture2D", new[] { texturesFolderPath });
            Texture2D[] textures = new Texture2D[assetGUIDs.Length];
            for (int i = 0; i < assetGUIDs.Length; i++)
            {
                string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);
                Texture2D texture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                textures[i] = texture;
            }

            return textures;
#else
            return null;
#endif
        }
    }
}