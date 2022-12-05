using System;
using System.Collections.Generic;

using Unity.Collections;
using Unity.Mathematics;

using UnityEngine;
using UnityEditor;

using Orazum.Utilities;

namespace Orazum.SpriteAtlas
{
    class TextureAtlasGenerator : MonoBehaviour
    {
        [SerializeField]
        string _texturesFolderPath;

        [SerializeField]
        string _textureAtlasFolderPath;

        Texture2D[] _textures;

        void Start()
        {
            string[] assetGUIDs = AssetDatabase.FindAssets($"t:texture2D", new[]{_texturesFolderPath});
            _textures = new Texture2D[assetGUIDs.Length];
            for (int i = 0; i < assetGUIDs.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                _textures[i] = texture;
            }

            Debug.Log($"Assets length {_textures.Length}");

            AtlasPackerByBinaryTree packer = new AtlasPackerByBinaryTree(); // TODO: replace or obsolete
            Sprite[] rectangles = new Sprite[_textures.Length];
            for (int i = 0; i < _textures.Length; i++)
            {
                Sprite rect = packer.Insert(_textures[i].width, _textures[i].height);
                Debug.Log(rect);
                rectangles[i] = rect;
            }

            int2 atlasDims = packer.GetDims();

            var atlas = new Texture2D(atlasDims.x, atlasDims.y, TextureFormat.RGBA32, false);
            NativeArray<Color32> atlasData = new NativeArray<Color32>(atlasDims.x * atlasDims.y, Allocator.Temp);
            Debug.Log($"Atlas dims: {atlasDims}");

            for (int i = 0; i < rectangles.Length; i++)
            {
                Sprite rect = rectangles[i];
                NativeArray<Color32> textureData = _textures[i].GetPixelData<Color32>(0);

                for (int x = 0; x < rect.Dims.x; x++)
                {
                    for (int y = 0; y < rect.Dims.y; y++)
                    {
                        int texturePixelIndex = IndexUtilities.XyToIndex(x, y, rect.Dims.x);
                        int atlasPixelIndex = IndexUtilities.XyToIndex(x + rect.Pos.x, y + rect.Pos.y, atlasDims.x);
                        atlasData[atlasPixelIndex] = textureData[texturePixelIndex];
                    }
                }
            }

            atlas.SetPixelData<Color32>(atlasData, mipLevel: 0);
            atlas.Apply(updateMipmaps: false);

            AssetDatabase.CreateAsset(atlas, _textureAtlasFolderPath + $"atlas.asset");
        }
    }
}