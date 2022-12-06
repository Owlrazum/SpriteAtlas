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
        float _steppingDeltaTime;

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

            AtlasPackerByFreeLinkedList packer = new();
            packer.PrepareAndPackFirst(textures);
            CreateTestSprite(new float3(0, 0, 0), new float2(textures[0].width, textures[0].height));
            yield return new WaitForSeconds(_steppingDeltaTime);

            for (int i = 1; i < textures.Length; i++)
            {
                packer.PackStep(textures[i], out Sprite packedSprite);
                CreateTestSprite(packedSprite);
                yield return new WaitForSeconds(_steppingDeltaTime);
            }
        }

        void CreateTestSprite(Sprite sprite)
        {
            float3 pos = new float3(sprite.Pos.x, sprite.Pos.y, 0);
            float2 size = new float2(sprite.Dims.x, sprite.Dims.y);
            TestSprite testSprite = Instantiate(_testingSpritePrefab, pos, Quaternion.identity, _debugginParentInCanvas);
            testSprite.RandomizeColor();
            testSprite.SetSize(size);
        }

        void CreateTestSprite(float3 pos, float2 size)
        {
            TestSprite testSprite = Instantiate(_testingSpritePrefab, pos, Quaternion.identity, _debugginParentInCanvas);
            testSprite.RandomizeColor();
            testSprite.SetSize(size);
        }
    }
}