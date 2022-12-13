using System.Collections;

using UnityEngine;
using UnityEngine.Assertions;

using Unity.Mathematics;

using Orazum.Utilities;

namespace Orazum.SpriteAtlas
{
    public class TestPacking : MonoBehaviour
    {
        [SerializeField]
        TextureGenerator _textureGenerator;

        [SerializeField]
        AtlasSpriteGenerator _atlasGenerator;

        [SerializeField]
        int _texturesCount = 16;
        [SerializeField]
        bool _shouldGenerateTextures;

        [Header("Debug")]
        [SerializeField]
        bool _shouldStepDebug; 
        [SerializeField]
        Transform _debugginParentInCanvas;

        [SerializeField]
        TestSprite _testingSpritePrefab;

        [SerializeField]
        TestSprite _testAtlasPrefab;

        [SerializeField]
        TestSprite _testFreeSpritePrefab;

        [SerializeField]
        float _steppingDeltaTime;

        TestSprite[] _freeTestSprites;

        void Start()
        {
            if (_shouldGenerateTextures)
            {
                _textureGenerator.GenerateRandomTextures(_texturesCount);
            }
            if (_shouldStepDebug)
            { 
                StartCoroutine(StepPackingCoroutine());
            }
            else
            { 
                _atlasGenerator.GenerateSpriteAtlas();
            }
        }

        IEnumerator StepPackingCoroutine()
        {
            Texture2D[] textures = _atlasGenerator.GetTexturesForSteppedPacking();

            AtlasPackerByFreeSpritesAndAdjacency packer = new();
            packer.PrepareAndPackFirst(textures);
            float2 textureDims = new float2(textures[0].width, textures[0].height);
            CreateTestSprite(new float3(0, 0, 0), textureDims);
            TestSprite testAtlas = CreateTestAtlas(textureDims);
            yield return new WaitForSeconds(_steppingDeltaTime);

            for (int i = 1; i < textures.Length; i++)
            {
                packer.PackStep(textures[i], out Sprite packedSprite, out int2 atlasDims);
                CreateTestSprite(packedSprite);
                testAtlas.SetSize((float2)atlasDims);

                var free = packer.GetFreeSprites();
                CreateFreeSprites(free);
                yield return new WaitForSeconds(_steppingDeltaTime);
            }
        }

        void CreateFreeSprites(FreeSprite[] freeSprites)
        {
            if (_freeTestSprites != null)
            { 
                foreach (var s in _freeTestSprites)
                {
                    s.Destroy();
                }
            }

            _freeTestSprites = new TestSprite[freeSprites.Length];
            for (int i = 0; i < freeSprites.Length; i++)
            {
                _freeTestSprites[i] = CreateFreeSprite(freeSprites[i]);
            }
        }

        TestSprite CreateFreeSprite(FreeSprite freeSprite)
        { 
            float3 pos = new float3(freeSprite.Pos.x, freeSprite.Pos.y, 0);
            float2 size = new float2(freeSprite.Dims.x, freeSprite.Dims.y);
            TestSprite testSprite = Instantiate(_testFreeSpritePrefab, pos, Quaternion.identity, _debugginParentInCanvas);
            testSprite.RandomizeColor(0.5f, 0.5f);
            testSprite.SetSize(size);
            return testSprite;
        }

        void CreateTestSprite(Sprite sprite)
        {
            float3 pos = new float3(sprite.Pos.x, sprite.Pos.y, 0);
            float2 size = new float2(sprite.Dims.x, sprite.Dims.y);
            TestSprite testSprite = Instantiate(_testingSpritePrefab, pos, Quaternion.identity, _debugginParentInCanvas);
            testSprite.RandomizeColor();
            testSprite.SetSizeWithAnimation(size);
        }

        void CreateTestSprite(float3 pos, float2 size)
        {
            TestSprite testSprite = Instantiate(_testingSpritePrefab, pos, Quaternion.identity, _debugginParentInCanvas);
            testSprite.RandomizeColor();
            testSprite.SetSizeWithAnimation(size);
        }

        TestSprite CreateTestAtlas(float2 size)
        { 
            TestSprite testAtlas = Instantiate(_testAtlasPrefab, float3.zero, Quaternion.identity, _debugginParentInCanvas);
            testAtlas.SetSize(size);
            return testAtlas;
        }
    }
}