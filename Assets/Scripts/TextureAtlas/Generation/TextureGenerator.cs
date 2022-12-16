using Unity.Mathematics;
using Unity.Collections;

using UnityEngine;
using UnityEditor;

using Orazum.Utilities;

using rnd = UnityEngine.Random;

namespace Orazum.SpriteAtlas.Generation
{
    public class TextureGenerator : MonoBehaviour
    {
        [SerializeField]
        int2 minMaxWidth = new int2(16, 256);

        [SerializeField]
        int2 minMaxHeight = new int2(16, 256);

        Texture2D[] textures;

        public void GenerateRandomTextures(int texturesCount, string folderPath)
        {
#if UNITY_EDITOR
            textures = new Texture2D[texturesCount];
            for (int i = 0; i < texturesCount; i++)
            {
                textures[i] = GenerateRandomTextureBordered(RandomColor());
                AssetDatabase.CreateAsset(textures[i], folderPath + $"rnd_{i}.asset");
            }
            AssetDatabase.SaveAssets();
#endif 
        }

        Texture2D GenerateRandomTexture()
        {
            int width = rnd.Range(minMaxWidth.x, minMaxWidth.y + 1);
            int height = rnd.Range(minMaxHeight.x, minMaxHeight.y + 1);

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

            var data = GenerateColor32TextureData(width, height);

            texture.SetPixelData(data, mipLevel: 0);
            texture.Apply(updateMipmaps: false);

            return texture;
        }

        Texture2D GenerateRandomTextureBordered(Color32 borderColor)
        {
            int width = rnd.Range(minMaxWidth.x, minMaxWidth.y + 1);
            int height = rnd.Range(minMaxHeight.x, minMaxHeight.y + 1);

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

            var data = GenerateColor32TextureData(width, height);
            ApplyBorderColor32(width, height, borderWidth: 4, borderColor, data);

            texture.SetPixelData(data, mipLevel: 0);
            texture.Apply(updateMipmaps: false);

            return texture;
        }

        NativeArray<Color32> GenerateColor32TextureData(int width, int height)
        {
            int pixelCount = width * height;
            NativeArray<Color32> data = new NativeArray<Color32>(pixelCount, Allocator.Temp);
            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    int pixelIndex = IndexUtilities.XyToIndex(new int2(col, row), width);
                    data[pixelIndex] = RandomColor();
                }
            }

            return data;
        }

        void ApplyBorderColor32(int width, int height, int borderWidth, Color32 color, NativeArray<Color32> data)
        {
            for (int i = 0; i < borderWidth; i++)
            {
                for (int leftRight = 0; leftRight < height; leftRight++)
                {
                    int left = IndexUtilities.XyToIndex(new int2(i, leftRight), width);
                    int right = IndexUtilities.XyToIndex(new int2(width - i - 1, leftRight), width);

                    data[left] = color;
                    data[right] = color;
                }

                for (int downUp = 0; downUp < width; downUp++)
                {
                    int down = IndexUtilities.XyToIndex(new int2(downUp, i), width);
                    int up = IndexUtilities.XyToIndex(new int2(downUp, height - i - 1), width);

                    data[down] = color;
                    data[up] = color;
                }
            }
        }

        Color32 RandomColor()
        {
            int3 rndColor = new int3(rnd.Range(0, 255), rnd.Range(0, 255), rnd.Range(0, 255));
            return new Color32((byte)rndColor.x, (byte)rndColor.y, (byte)rndColor.z, 255);
        }
    }
}
