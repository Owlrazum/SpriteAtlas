using UnityEngine;
using Unity.Mathematics;

namespace Orazum.Utilities
{
    public static class DebugUtilities
    {
        public static void DrawRectangle(Rectangle rectangle, float duration)
        {
            Vector3 start = new Vector3(rectangle.Pos.x, rectangle.Pos.y, 0);
            Vector3 up = new Vector3(0, rectangle.Dims.y, 0);
            Vector3 right = new Vector3(rectangle.Dims.x, 0);
            Debug.DrawLine(start, start + up, Color.red, duration);
            Debug.DrawLine(start + up, start + up + right, Color.red, duration);
            Debug.DrawLine(start, start + right, Color.red, duration);
            Debug.DrawLine(start + right, start + up + right, Color.red, duration);
        }
    }
}