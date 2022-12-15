using Unity.Collections;
using Unity.Mathematics;

using UnityEngine;
using UnityEditor;

using Orazum.Utilities;

namespace Orazum.SpriteAtlas.Generation
{
    public class AtlasSpriteGenerator
    {
        Texture2D[] textures;

        public void GenerateAndSaveAtlas(Texture2D[] textures, string spriteAtlasFilePath)
        {
            AtlasPackerByFreeSpritesAndAdjacency packer = new(0.2f, 2500, 0.7f);
            packer.Pack(textures, out SpriteManaged[] sprites, out int2 atlasDims);

            var atlas = CombineTextures(textures, sprites, atlasDims);
            SaveAtlas(atlas, spriteAtlasFilePath);
        }

        public Texture2D CombineTextures(Texture2D[] textures, SpriteManaged[] packedSprites, int2 atlasDims)
        {
            var atlas = new Texture2D(atlasDims.x, atlasDims.y, TextureFormat.RGBA32, false);
            NativeArray<Color32> atlasData = new NativeArray<Color32>(atlasDims.x * atlasDims.y, Allocator.Temp);

            for (int i = 0; i < packedSprites.Length; i++)
            {
                SpriteManaged sprite = packedSprites[i];
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
            return atlas;
        }

        public void SaveAtlas(Texture2D atlas, string filePath)
        {
            AssetDatabase.CreateAsset(atlas, filePath);
            AssetDatabase.SaveAssets();
        }
    }
}