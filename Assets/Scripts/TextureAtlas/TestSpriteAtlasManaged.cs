using UnityEngine;

using TMPro;

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

        [SerializeField]
        TextMeshProUGUI textMesh;

        void Start()
        {
#if UNITY_EDITOR
            if (shouldSave)
            {
                Texture2D[] textures = AssetUtilities.GetTextures(texturesFolderPath);
                SpriteAtlasManaged atlasToSave = new(textures);
                SpriteAtlasManaged.SaveAtlas(atlasDataSavingPath, atlasToSave);
                SpriteAtlasManaged.SaveAtlasTexture(atlasTextureSavingPath, atlasToSave);
            }
#endif

            if (shouldLoad)
            {
                try
                {
                    var loadedAtlas = SpriteAtlasManaged.LoadAtlas(atlasDataSavingPath);
                    string[] spriteNames = SpriteAtlasManaged.GetSpriteNames(loadedAtlas);
                    for (int i = 0; i < spriteNames.Length; i++)
                    {
                        SpriteInfo info = SpriteAtlasManaged.GetSpriteByName(loadedAtlas, spriteNames[i]);
                        string log = $"Sprite name:{spriteNames[i]}, SpriteInfo: {info}";
                        // Debug.Log(log);
                        if (textMesh != null)
                        {
                            textMesh.text += log + "\n";
                        }
                    }
                }
                catch (System.Exception e)
                {
                    if (textMesh != null)
                    {
                        textMesh.text = e.ToString();
                    }
                }
            }
        }
    }
}
