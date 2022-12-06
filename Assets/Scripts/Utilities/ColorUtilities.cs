using UnityEngine;

namespace Orazum.Utilities
{
    public static class ColorUtilities
    {
        static Unity.Mathematics.Random random = Unity.Mathematics.Random.CreateFromIndex(1234);

        public static Color RandomColor()
        {
            // Helper to create any amount of colors as distinct from each other as possible.
            // The logic behind this approach is detailed at the following address:
            // https://martin.ankerl.com/2009/12/09/how-to-create-random-colors-programmatically/

            // 0.618034005f == 2 / (math.sqrt(5) + 1) == inverse of the golden ratio
            float hue = random.NextFloat();
            hue = (hue + 0.618034005f) % 1;
            Color color = Color.HSVToRGB(hue, 1.0f, 1.0f);
            return color;
        }
    }

}