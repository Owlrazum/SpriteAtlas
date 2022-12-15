using System;
using Unity.Mathematics;

namespace Orazum.SpriteAtlas.Generation
{
    [System.Serializable]
    public struct SpriteManaged : IEquatable<SpriteManaged>
    {
        public int2 Pos;
        public int2 Dims;

        public int Area { get { return Dims.x * Dims.y; } }

        public int4 Borders { get { return new int4(Pos.x, Pos.x + Dims.x - 1, Pos.y, Pos.y + Dims.y - 1); } }
        public int RightBorder { get { return Pos.x + Dims.x - 1; } }
        public int TopBorder { get { return Pos.y + Dims.y - 1; } }

        public SpriteManaged(int2 pos, int2 dims)
        {
            Pos = pos;
            Dims = dims;
        }

        public SpriteManaged(int4 borders)
        {
            Pos = borders.xz;
            Dims = borders.yw - borders.xz + new int2(1, 1);
        }

        public bool Equals(SpriteManaged other)
        {
            return math.all(Pos == other.Pos) && math.all(Dims == other.Dims);
        }

        public override bool Equals(object obj)
        {
            if (obj is SpriteManaged other)
            {
                return Equals(other);
            }

            return false;
        }

        public static bool operator ==(SpriteManaged lhs, SpriteManaged rhs)
        {
            return math.all(lhs.Pos == rhs.Pos) && math.all(lhs.Dims == rhs.Dims);
        }

        public static bool operator !=(SpriteManaged lhs, SpriteManaged rhs)
        {
            return math.any(lhs.Pos != rhs.Pos) || math.any(lhs.Dims != rhs.Dims);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Pos, Dims);
        }

        public override string ToString()
        {
            return $"Sprite: Pos({Pos.x} {Pos.y}) Dims({Dims.x} {Dims.y})";
        }
    }
}