using UnityEngine;

using Orazum.Utilities;

namespace Orazum.SpriteAtlas
{ 
    class TestSpriteAtlasManaged : MonoBehaviour
    {
        [SerializeField]
        string texturesFolderPath;

        [SerializeField]
        string atlasDataSavingPath;

        [SerializeField]
        string atlasTextureSavingPath;

        [SerializeField]
        bool shouldSave;

        [SerializeField]
        bool shouldLoad;

        void Start()
        {
            if (shouldSave)
            { 
                Texture2D[] textures = AssetUtilities.GetTextures(texturesFolderPath);
                SpriteAtlasManaged atlasToSave = new(textures);
                SpriteAtlasManaged.SaveAtlas(atlasDataSavingPath, atlasToSave);
                SpriteAtlasManaged.SaveAtlasTexture(atlasTextureSavingPath, atlasToSave);
            }

            if (shouldLoad)
            { 
                var loadedAtlas = SpriteAtlasManaged.LoadAtlas(atlasDataSavingPath);
                string[] spriteNames = SpriteAtlasManaged.GetSpriteNames(loadedAtlas);
                for (int i = 0; i < spriteNames.Length; i++)
                {
                    SpriteInfo info = SpriteAtlasManaged.GetSpriteByName(loadedAtlas, spriteNames[i]);
                    Debug.Log($"Sprite name:{spriteNames[i]}, SpriteInfo: {info}");
                }
            }
        }
    }
}
