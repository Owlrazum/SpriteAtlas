using System;
using System.Collections.Generic;

using Unity.Mathematics;

using UnityEngine;

namespace Orazum.SpriteAtlas
{
    /// <summary>
    /// The idea is to have sprites all located in the origin, 
    /// with packer gradually placing them in the correct positions
    /// </summary>
    abstract class AtlasPacker
    {
        public abstract void Pack(Texture2D[] textures, out Sprite[] spritesToPack, out int2 atlasDims);

        public abstract void PrepareAndPackFirst(Texture2D[] textures);
        public abstract void PackStep(Texture2D texture, out Sprite packedSprite, out int2 atlasDims);

        protected void SortByArea(Sprite[] sprites)
        {
            Array.Sort(sprites, new SpriteAreaComparer());
        }

        protected void SortByArea(Texture2D[] textures)
        {
            Array.Sort(textures, new TextureAreaComparer());
        }

        protected void SortByMaxDimension(Sprite[] sprites)
        {
            Array.Sort(sprites, new SpriteDimensionComparer());
        }

        protected void SortByMaxDimension(Texture2D[] textures)
        {
            Array.Sort(textures, new TextureDimensionComparer());
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

    class TextureAreaComparer : IComparer<Texture2D>
    {
        public int Compare(Texture2D t1, Texture2D t2)
        {
            int a1 = t1.width * t1.height;
            int a2 = t2.width * t2.height;
            return a2.CompareTo(a1); // Decreasing order
        }
    }

    class TextureDimensionComparer : IComparer<Texture2D>
    {
        public int Compare(Texture2D t1, Texture2D t2)
        {
            int m1 = math.max(t1.width, t1.height);
            int m2 = math.max(t1.width, t2.height);
            return m2.CompareTo(m1); // Decreasing order
        }
    }
}