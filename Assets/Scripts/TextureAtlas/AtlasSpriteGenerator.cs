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
        string _texturesFolderPath;

        [SerializeField]
        string _spriteAtlasFolderPath;

        Texture2D[] _textures;

        public void GenerateSpriteAtlas()
        {
            string[] assetGUIDs = AssetDatabase.FindAssets($"t:texture2D", new[] { _texturesFolderPath });
            _textures = new Texture2D[assetGUIDs.Length];
            for (int i = 0; i < assetGUIDs.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                _textures[i] = texture;
            }

            AtlasPackerByFreeLinkedList packer = new();
            packer.Pack(_textures, out Sprite[] sprites, out int2 atlasDims);

            var atlas = new Texture2D(atlasDims.x, atlasDims.y, TextureFormat.RGBA32, false);
            NativeArray<Color32> atlasData = new NativeArray<Color32>(atlasDims.x * atlasDims.y, Allocator.Temp);

            for (int i = 0; i < sprites.Length; i++)
            {
                Sprite rect = sprites[i];
                NativeArray<Color32> textureData = _textures[i].GetPixelData<Color32>(0);

                for (int x = 0; x < rect.Dims.x; x++)
                {
                    for (int y = 0; y < rect.Dims.y; y++)
                    {
                        int texturePixelIndex = IndexUtilities.XyToIndex(x, y, rect.Dims.x);
                        int atlasPixelIndex = IndexUtilities.XyToIndex(x + rect.Pos.x, y + rect.Pos.y, atlasDims.x);
                        Color32 textureColor = textureData[texturePixelIndex];
                        atlasData[atlasPixelIndex] = textureColor;
                    }
                }
            }

            atlas.SetPixelData<Color32>(atlasData, mipLevel: 0);
            atlas.Apply(updateMipmaps: false);

            AssetDatabase.CreateAsset(atlas, _spriteAtlasFolderPath + $"atlas.asset");
        }

        public Texture2D[] GetTexturesForSteppedPacking()
        {
            string[] assetGUIDs = AssetDatabase.FindAssets($"t:texture2D", new[] { _texturesFolderPath });
            _textures = new Texture2D[assetGUIDs.Length];
            for (int i = 0; i < assetGUIDs.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                _textures[i] = texture;
            }

            return _textures;
        }
    }
}