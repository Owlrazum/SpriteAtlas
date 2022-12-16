using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Unity.Mathematics;

using Orazum.SpriteAtlas.Generation;
using Texture = UnityEngine.Texture2D;

namespace Orazum.SpriteAtlas
{
    [Serializable]
    public class SpriteAtlasManaged
    {
        static AtlasSpriteGenerator saverAsTexture;
        static SpriteAtlasSaver saver;
        static SpriteAtlasManaged()
        {
            saverAsTexture = new();
            saver = new();
        }

        [NonSerialized]
        UnityEngine.Texture2D texture;
        string textureFilePath;

        Dictionary<string, SpriteInfo> sprites;
        
        int2 atlasDims;
        SpriteManaged[] packedSprites;

        public SpriteAtlasManaged(Texture[] texturesToPack)
        {
            sprites = new(texturesToPack.Length);
            AtlasPackerByFreeSpritesAndAdjacency packer = new();
            packer.Pack(texturesToPack, out packedSprites, out atlasDims);
            for (int i = 0; i < packedSprites.Length; i++)
            {
                SpriteInfo sprite = ConvertSprite(packedSprites[i]);
                sprites.Add(texturesToPack[i].name, sprite);
            }

            this.texture = saverAsTexture.CombineTextures(texturesToPack, packedSprites, atlasDims);
        }

        SpriteInfo ConvertSprite(SpriteManaged sprite)
        {
            SpriteInfo converted = new();
            converted.X = sprite.Pos.x;
            converted.Y = sprite.Pos.y;
            converted.Width = sprite.Dims.x;
            converted.Height = sprite.Dims.y;
            return converted;
        }

        public static void SaveAtlas(string filePath, SpriteAtlasManaged atlas)
        {
            saver.SaveAtlas(atlas, filePath);
        }

#if UNITY_EDITOR
        public static void SaveAtlasTexture(string filePath, SpriteAtlasManaged atlas)
        {
            UnityEditor.AssetDatabase.CreateAsset(atlas.texture, filePath);
            UnityEditor.AssetDatabase.SaveAssets();
            atlas.textureFilePath = filePath;
        }
#endif

        public static SpriteAtlasManaged LoadAtlas(string filePath)
        {
            var atlas = saver.LoadAtlas(filePath);
#if UNITY_EDITOR
            atlas.texture = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>(atlas.textureFilePath);
#endif
            return atlas;
        }

        public static string[] GetSpriteNames(SpriteAtlasManaged atlas)
        {
            string[] names = new string[atlas.sprites.Count];
            atlas.sprites.Keys.CopyTo(names, 0);
            return names;
        }

        public static SpriteInfo GetSpriteByName(SpriteAtlasManaged atlas, string name)
        {
            return atlas.sprites[name];
        }

        public override string ToString()
        {
            string keys = "";
            foreach (var name in sprites.Keys)
            {
                keys += name + "\n";
            }
            return keys;
        }
    }
}