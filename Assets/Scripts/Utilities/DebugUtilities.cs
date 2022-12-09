using UnityEngine;
using Unity.Mathematics;

namespace Orazum.Utilities
{
    public static class DebugUtilities
    {
        public static void DrawSprite(int2 pos, int2 dims, Color color, float duration)
        {
            Vector3 start = new Vector3(pos.x, pos.y, 0);
            Vector3 up = new Vector3(0, dims.y, 0);
            Vector3 right = new Vector3(dims.x, 0);
            Debug.DrawLine(start, start + up, color, duration);
            Debug.DrawLine(start + up, start + up + right, color, duration);
            Debug.DrawLine(start, start + right, color, duration);
            Debug.DrawLine(start + right, start + up + right, color, duration);
        }
    }
}