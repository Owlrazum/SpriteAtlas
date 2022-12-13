using System;
using Unity.Mathematics;

using static Orazum.Utilities.Math;

namespace Orazum.SpriteAtlas
{
    public struct Sprite : IEquatable<Sprite>
    {
        public int2 Pos;
        public int2 Dims;

        public int Area { get { return Dims.x * Dims.y; } }

        public Sprite(int2 pos, int2 dims)
        {
            Pos = pos;
            Dims = dims;
        }

        public int4 Borders
        {
            get
            {
                return new int4(Pos.x, Pos.x + Dims.x - 1, Pos.y, Pos.y + Dims.y - 1);
            }
        }

        public int RightBorder
        {
            get
            {
                return Pos.x + Dims.x - 1;
            }
        }

        public int TopBorder
        {
            get
            {
                return Pos.y + Dims.y;
            }
        }

        public bool DoesFitHorizontally(int posX)
        {
            return IsBetween(Pos.x, RightBorder, posX);
        }
        public bool DoesFitVertically(int posY)
        {
            return IsBetween(Pos.y, TopBorder, posY);
        }

        public bool Equals(Sprite other)
        {
            return math.all(Pos == other.Pos) && math.all(Dims == other.Dims);
        }

        public override bool Equals(object obj)
        {
            if (obj is Sprite other)
            {
                return Equals(other);
            }

            return false;
        }

        public static bool operator ==(Sprite lhs, Sprite rhs)
        {
            return math.all(lhs.Pos == rhs.Pos) && math.all(lhs.Dims == rhs.Dims);
        }

        public static bool operator !=(Sprite lhs, Sprite rhs)
        {
            return math.any(lhs.Pos != rhs.Pos) || math.any(lhs.Dims != rhs.Dims);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Pos, Dims);
        }

        public override string ToString()
        {
            return $"Sprite(Pos:{Pos}, Dims:{Dims})";
        }
    }
}