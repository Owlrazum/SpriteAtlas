using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Assertions;
using Orazum.Graphs;
using static Orazum.Utilities.Math;

namespace Orazum.SpriteAtlas
{
    public class FreeSprite : IAdjacentable<FreeSprite>
    {
        public bool2 IsBorderingAtlas;
        public int Id { get; set; }

        Sprite SpriteData;

        public FreeSprite(int2 pos, int2 dims)
        {
            SpriteData.Pos = pos;
            SpriteData.Dims = dims;
        }

        public FreeSprite(int2 pos, int2 dims, bool2 isBorderingAtlas)
        {
            SpriteData = new(pos, dims);
            IsBorderingAtlas = isBorderingAtlas;
        }

        public FreeSprite(int4 borders)
        {
            SpriteData = new Sprite(borders);
            Id = -1;
        }

        public static FreeSprite CloneForEvalutation(FreeSprite toClone)
        {
            FreeSprite clone = new FreeSprite(toClone.SpriteData.Pos, toClone.SpriteData.Dims);
            clone.IsBorderingAtlas = toClone.IsBorderingAtlas;
            clone.Id = -1;
            return clone;
        }

        public int2 Pos
        {
            get
            {
                return SpriteData.Pos;
            }
            set
            {
                SpriteData.Pos = value;
            }
        }
        public int2 Dims
        {
            get
            {
                return SpriteData.Dims;
            }
            set
            {
                SpriteData.Dims = value;
            }
        }
        public int Area
        {
            get
            {
                return SpriteData.Area;
            }
        }

        public int4 SpriteBorders
        {
            get
            {
                return SpriteData.Borders;
            }
        }

        public int RightBorder
        {
            get
            {
                return SpriteData.RightBorder;
            }
        }

        public int TopBorder
        {
            get
            {
                return SpriteData.TopBorder;
            }
        }

        public bool Contains(int2 pos)
        {
            int4 borders = SpriteBorders;
            return IsBetween(borders.xy, pos.x) && IsBetween(borders.zw, pos.y);
        }

        public bool Intersect(Sprite other)
        {
            int4 borders = SpriteBorders;
            int4 otherBorders = other.Borders;

            // Implement Separating Axis Theorem
            if (!DoesOverlap(borders.xy, otherBorders.xy))
            {
                return false;
            }

            if (!DoesOverlap(borders.zw, otherBorders.zw))
            {
                return false;
            }

            return true;

            bool DoesOverlap(int2 lhs, int2 rhs)
            {
                bool result;
                if (lhs.x < rhs.x)
                {
                    result = lhs.y > rhs.x;
                }
                else
                {
                    result = rhs.y > lhs.x;
                }
                return result;
            }
        }

        public bool Intersect(Sprite other, out Sprite intersection)
        {
            int4 borders = SpriteBorders;
            int4 otherBorders = other.Borders;
            if (Intersect(other))
            {
                int4 minBorder = int4.zero;
                minBorder.xz = math.max(borders.xz, otherBorders.xz);
                minBorder.yw = math.min(borders.yw, otherBorders.yw);
                Assert.IsTrue(math.all(minBorder.yw > minBorder.xz));

                intersection = new Sprite(minBorder);
                return true;
            }

            intersection = new();
            return false;
        }

        public bool IsAdjacent(FreeSprite other)
        {
            int4 borders = SpriteBorders;
            int4 otherBorders = other.SpriteBorders;
            int4 otherBordersArranged = otherBorders.yxwz;
            int4 delta = math.abs(SpriteBorders - otherBordersArranged);

            if (math.any(delta == 1))
            {
                if ((IsBetween(borders.xy, otherBorders.x) ||
                    IsBetween(borders.xy, otherBorders.y) ||
                    IsBetween(otherBorders.xy, borders.x) ||
                    IsBetween(otherBorders.xy, borders.y))
                    && math.any(delta.zw == 1))
                {
                    return true;
                }

                if ((IsBetween(borders.zw, otherBorders.z) ||
                    IsBetween(borders.zw, otherBorders.w) ||
                    IsBetween(otherBorders.zw, borders.z) ||
                    IsBetween(otherBorders.zw, borders.w))
                    && math.any(delta.xy == 1))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsAdjacent(FreeSprite other, out Axis adjacencyAxis)
        {
            int4 borders = SpriteBorders;
            int4 otherBorders = other.SpriteBorders;
            int4 otherBordersArranged = otherBorders.yxwz;
            int4 delta = math.abs(SpriteBorders - otherBordersArranged);

            if (math.any(delta == 1))
            {
                if ((IsBetween(borders.xy, otherBorders.x) ||
                    IsBetween(borders.xy, otherBorders.y) ||
                    IsBetween(otherBorders.xy, borders.x) ||
                    IsBetween(otherBorders.xy, borders.y))
                    && math.any(delta.zw == 1))
                {
                    adjacencyAxis = Axis.Y;
                    return true;
                }

                if ((IsBetween(borders.zw, otherBorders.z) ||
                    IsBetween(borders.zw, otherBorders.w) ||
                    IsBetween(otherBorders.zw, borders.z) ||
                    IsBetween(otherBorders.zw, borders.w))
                    && math.any(delta.xy == 1))
                {
                    adjacencyAxis = Axis.X;
                    return true;
                }
            }

            adjacencyAxis = Axis.X;
            return false;
        }

        public override string ToString()
        {
            return $"FreeSprite Pos({Pos.x} {Pos.y}) Dims({Dims.x} {Dims.y}) {IsBorderingAtlas} {Id}";
        }
    }

    public enum Axis
    {
        X,
        Y
    }
}