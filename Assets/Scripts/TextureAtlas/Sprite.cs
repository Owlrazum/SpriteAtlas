using System;
using Unity.Mathematics;

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