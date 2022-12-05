using System;
using System.Collections.Generic;

using Unity.Mathematics;
namespace Orazum.SpriteAtlas
{
    /// <summary>
    /// The idea is to have sprites all located in the origin, 
    /// with packer gradually placing them in the correct positions
    /// </summary>
    abstract class AtlasPacker
    {
        public abstract void Pack(Sprite[] spritesToPack);

        protected void SortByArea(Sprite[] sprites)
        { 
            Array.Sort(sprites, new SpriteAreaComparer());
        }

        protected void SortByMaxDimension(Sprite[] sprites)
        { 
            Array.Sort(sprites, new SpriteDimensionComparer());
        }
    }

    class SpriteAreaComparer : IComparer<Sprite>
    {
        public int Compare(Sprite s1, Sprite s2)
        {
            int a1 = s1.Dims.x * s1.Dims.y;
            int a2 = s2.Dims.x * s2.Dims.y;
            return a2.CompareTo(a1); // Decreasing order
        }
    }

    class SpriteDimensionComparer : IComparer<Sprite>
    {
        public int Compare(Sprite s1, Sprite s2)
        {
            int m1 = math.max(s1.Dims.x, s1.Dims.y);
            int m2 = math.max(s2.Dims.x, s2.Dims.y);
            return m2.CompareTo(m1); // Decreasing order
        }
    }
}