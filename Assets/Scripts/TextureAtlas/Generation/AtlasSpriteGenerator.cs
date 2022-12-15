using System;
using System.Collections;
using System.Collections.Generic;

using Unity.Collections;
using Unity.Mathematics;

using UnityEngine;
using UnityEditor;

using Orazum.Utilities;

namespace Orazum.SpriteAtlas
{
    class AtlasSpriteGenerator : MonoBehaviour
    {
        [SerializeField]
        string texturesFolderPath = "Assets/Textures/Random/";

        [SerializeField]
        string spriteAtlasFolderPath = "Assets/Textures/Atlases/";

        Texture2D[] textures;

        public void GenerateSpriteAtlas()
        {
            textures = GetTextures();

            AtlasPackerByFreeSpritesAndAdjacency packer = new(0.2f, 2500, 0.7f);
            packer.Pack(textures, out Sprite[] sprites, out int2 atlasDims);

            var atlas = new Texture2D(atlasDims.x, atlasDims.y, TextureFormat.RGBA32, false);
            NativeArray<Color32> atlasData = new NativeArray<Color32>(atlasDims.x * atlasDims.y, Allocator.Temp);

            for (int i = 0; i < sprites.Length; i++)
            {
                Sprite sprite = sprites[i];
                NativeArray<Color32> textureData = textures[i].GetPixelData<Color32>(0);

                for (int y = 0; y < sprite.Dims.y; y++)
                {
                    for (int x = 0; x < sprite.Dims.x; x++)
                    {
                        int texturePixelIndex = IndexUtilities.XyToIndex(x, y, sprite.Dims.x);
                        int atlasPixelIndex = IndexUtilities.XyToIndex(x + sprite.Pos.x, y + sprite.Pos.y, atlasDims.x);
                        Color32 textureColor = textureData[texturePixelIndex];
                        atlasData[atlasPixelIndex] = textureColor;
                    }
                }
            }

            atlas.SetPixelData<Color32>(atlasData, mipLevel: 0);
            atlas.Apply(updateMipmaps: false);

            AssetDatabase.CreateAsset(atlas, spriteAtlasFolderPath + $"atlas.asset");
        }

        public Texture2D[] GetTextures()
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