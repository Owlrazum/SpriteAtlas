using UnityEngine;
using Unity.Mathematics;

using Sprite = Orazum.SpriteAtlas.Sprite;

namespace Orazum.Utilities
{
    public static class DebugUtilities
    {
        public static void DrawSprite(Sprite sprite, Color color, float duration)
        {
            Vector3 start = new Vector3(sprite.Pos.x, sprite.Pos.y, 0);
            Vector3 up = new Vector3(0, sprite.Dims.y, 0);
            Vector3 right = new Vector3(sprite.Dims.x, 0);
            Debug.DrawLine(start, start + up, color, duration);
            Debug.DrawLine(start + up, start + up + right, color, duration);
            Debug.DrawLine(start, start + right, color, duration);
            Debug.DrawLine(start + right, start + up + right, color, duration);
        }
    }
}