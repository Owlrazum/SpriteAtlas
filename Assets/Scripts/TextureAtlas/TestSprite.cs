using System.Collections;

using UnityEngine;
using UnityEngine.UI;

using Orazum.Utilities;

namespace Orazum.SpriteAtlas
{
    [RequireComponent(typeof(Image))]
    class TestSprite : MonoBehaviour
    {
        [SerializeField]
        float _animationSpeed = 3;
        Image _image;
        void Awake()
        {
            TryGetComponent(out _image);
        }

        public void SetSize(Vector2 size)
        { 
            _image.rectTransform.sizeDelta = size;
        }

        public void SetSizeWithAnimation(Vector2 size)
        {
            StartCoroutine(SpawnAnimation(size));
        }

        IEnumerator SpawnAnimation(Vector2 size)
        {
            Vector2 largeSize = size * 1.2f;
            Vector2 currentSize = largeSize;
            _image.rectTransform.sizeDelta = currentSize;
            float lerpParam = 0;
            while (lerpParam < 1)
            {
                lerpParam += _animationSpeed * Time.deltaTime;
                currentSize = Vector2.Lerp(largeSize, size, lerpParam);
                _image.rectTransform.sizeDelta = currentSize;
                yield return null;
            }
        }

        public void RandomizeColor(float saturation = 1.0f, float value = 1.0f)
        {
            _image.color = ColorUtilities.RandomColor(saturation, value);
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}