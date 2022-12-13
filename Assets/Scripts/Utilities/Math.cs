using Unity.Mathematics;

namespace Orazum.Utilities
{ 
    public static class Math
    {
        public static bool IsBetween(int min, int max, int value)
        {
            return value > min && value < max;
        }

        public static bool IsBetween(int2 minMax, int value)
        {
            return value > minMax.x && value < minMax.y;
        }

        public static bool IsBetweenInclusively(int2 minMax, int value)
        { 
            return value >= minMax.x && value <= minMax.y;
        }
    }
}