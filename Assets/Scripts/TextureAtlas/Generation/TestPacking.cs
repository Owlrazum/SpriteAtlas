using System.Collections;

using UnityEngine;
using Unity.Mathematics;

using Orazum.Utilities;

namespace Orazum.SpriteAtlas.Generation.Tests
{
    class TestPacking : MonoBehaviour
    {
        [SerializeField]
        TextureGenerator textureGenerator;

        [SerializeField]
        string texturesFolderPath = "Assets/Textures/Random/";

        [SerializeField]
        string spriteAtlasFilePath = "Assets/Textures/Atlases/Atlas.asset";

        [SerializeField]
        int texturesCount = 16;
        [SerializeField]
        bool shouldGenerateTextures;
        [SerializeField]
        bool shouldGenerateAtlas;

        [Header("Debug")]
        [SerializeField]
        bool shouldStepDebug; 
        [SerializeField]
        Transform debugginParentInCanvas;

        [SerializeField]
        TestSprite testingSpritePrefab;

        [SerializeField]
        TestSprite testAtlasPrefab;

        [SerializeField]
        TestSprite testFreeSpritePrefab;

        [SerializeField]
        float steppingDeltaTime;

        TestSprite[] freeTestSprites;

        void Awake()
        {
            if (shouldGenerateTextures)
            {
                textureGenerator.GenerateRandomTextures(texturesCount, texturesFolderPath);
            }

            if (shouldStepDebug)
            { 
                StartCoroutine(StepPackingCoroutine());
            }

            if (shouldGenerateAtlas)
            {
                AtlasSpriteGenerator atlasGenerator = new();
                atlasGenerator.GenerateAndSaveAtlas(AssetUtilities.GetTextures(texturesFolderPath), spriteAtlasFilePath);
            }
        }

        IEnumerator StepPackingCoroutine()
        {
            Texture2D[] textures = AssetUtilities.GetTextures(texturesFolderPath);

            AtlasPackerByFreeSpritesAndAdjacency packer = new();
            packer.PrepareAndPackFirstTexture(textures);
            float2 textureDims = new float2(textures[0].width, textures[0].height);
            TestSprite testAtlas = CreateTestAtlas(textureDims);
            CreateTestSprite(new float3(0, 0, 0), textureDims);
            yield return new WaitForSeconds(steppingDeltaTime);

            for (int i = 1; i < textures.Length; i++)
            {
                packer.PackStep(textures[i], out SpriteManaged packedSprite, out int2 atlasDims);
                CreateTestSprite(packedSprite);
                testAtlas.SetSize((float2)atlasDims);

                var free = packer.GetFreeSprites();
                CreateFreeSprites(free);
                yield return new WaitForSeconds(steppingDeltaTime);
            }
        }

        void CreateFreeSprites(FreeSprite[] freeSprites)
        {
            if (freeTestSprites != null)
            { 
                foreach (var s in freeTestSprites)
                {
                    s.Destroy();
                }
            }

            freeTestSprites = new TestSprite[freeSprites.Length];
            for (int i = 0; i < freeSprites.Length; i++)
            {
                freeTestSprites[i] = CreateFreeSprite(freeSprites[i]);
            }
        }

        TestSprite CreateFreeSprite(FreeSprite freeSprite)
        { 
            float3 pos = new float3(freeSprite.SpriteData.Pos.x, freeSprite.SpriteData.Pos.y, 0);
            float2 size = new float2(freeSprite.SpriteData.Dims.x, freeSprite.SpriteData.Dims.y);
            TestSprite testSprite = Instantiate(testFreeSpritePrefab, pos, Quaternion.identity, debugginParentInCanvas);
            testSprite.RandomizeColor(0.5f, 0.5f);
            testSprite.SetSize(size);
            return testSprite;
        }

        void CreateTestSprite(SpriteManaged sprite)
        {
            float3 pos = new float3(sprite.Pos.x, sprite.Pos.y, 0);
            float2 size = new float2(sprite.Dims.x, sprite.Dims.y);
            TestSprite testSprite = Instantiate(testingSpritePrefab, pos, Quaternion.identity, debugginParentInCanvas);
            testSprite.RandomizeColor();
            testSprite.SetSizeWithAnimation(size);
        }

        void CreateTestSprite(float3 pos, float2 size)
        {
            TestSprite testSprite = Instantiate(testingSpritePrefab, pos, Quaternion.identity, debugginParentInCanvas);
            testSprite.RandomizeColor();
            testSprite.SetSizeWithAnimation(size);
        }

        TestSprite CreateTestAtlas(float2 size)
        { 
            TestSprite testAtlas = Instantiate(testAtlasPrefab, float3.zero, Quaternion.identity, debugginParentInCanvas);
            testAtlas.SetSize(size);
            return testAtlas;
        }
    }
}