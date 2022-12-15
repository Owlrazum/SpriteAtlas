using System.Collections;

using UnityEngine;
using UnityEngine.UI;

using Orazum.Utilities;

namespace Orazum.SpriteAtlas.Generation.Tests
{
    [RequireComponent(typeof(Image))]
    class TestSprite : MonoBehaviour
    {
        [SerializeField]
        float animationSpeed = 3;
        Image image;
        void Awake()
        {
            TryGetComponent(out image);
        }

        public void SetSize(Vector2 size)
        { 
            image.rectTransform.sizeDelta = size;
        }

        public void SetSizeWithAnimation(Vector2 size)
        {
            StartCoroutine(SpawnAnimation(size));
        }

        IEnumerator SpawnAnimation(Vector2 size)
        {
            Vector2 largeSize = size * 1.2f;
            Vector2 currentSize = largeSize;
            image.rectTransform.sizeDelta = currentSize;
            float lerpParam = 0;
            while (lerpParam < 1)
            {
                lerpParam += animationSpeed * Time.deltaTime;
                currentSize = Vector2.Lerp(largeSize, size, lerpParam);
                image.rectTransform.sizeDelta = currentSize;
                yield return null;
            }
        }

        public void RandomizeColor(float saturation = 1.0f, float value = 1.0f)
        {
            image.color = ColorUtilities.RandomColor(saturation, value);
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}