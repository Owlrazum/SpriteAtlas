using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace Orazum.TextureAtlas
{ 
    class TextureAtlasGenerator : MonoBehaviour
    {
        [SerializeField]
        string _texturesFolderPath;

        [SerializeField]
        string _textureAtlasFolderPath;

        Texture2D[] _textures;

        void Awake()
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(_texturesFolderPath);
            for (int i = 0; i < assets.Length; i++)
            {
                _textures[i] = (Texture2D)assets[i];
            }

            Array.Sort(_textures, new TextureAreaComparer());

            AtlasPackingByBinaryTree packer = new AtlasPackingByBinaryTree();
            for (int i = 0; i < assets.Length; i++)
            { 
            }
        }

        class TextureAreaComparer : IComparer<Texture2D>
        {
            public int Compare(Texture2D x, Texture2D y)
            {
                int a1 = x.width * x.height;
                int a2 = y.width * y.height;
                return a1.CompareTo(a2);
            }
        }
    }
}