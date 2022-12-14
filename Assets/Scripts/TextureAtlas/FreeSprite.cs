using Unity.Mathematics;
using Orazum.Graphs;

using UnityEngine.Assertions;

namespace Orazum.SpriteAtlas
{
    public class FreeSprite : IAdjacentable<FreeSprite>
    {
        public Sprite SpriteData;
        public bool2 IsBorderingAtlas;
        public int Id { get; set; }

        public int2 Pos { get { return SpriteData.Pos; } }
        public int2 Dims { get { return SpriteData.Dims; } }
        public int4 SpriteBorders { get { return SpriteData.Borders; } }
        public int RightBorder { get { return SpriteData.RightBorder; } }
        public int TopBorder { get { return SpriteData.TopBorder; } }

        public bool DoesIntersect(Sprite other)
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

        public bool DoesIntersect(Sprite other, out Sprite intersection)
        {
            int4 borders = SpriteBorders;
            int4 otherBorders = other.Borders;
            if (DoesIntersect(other))
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

        bool IsBetween(in int2 minMax, int value)
        {
            return value >= minMax.x && value <= minMax.y;
        }
    }

    public enum Axis
    {
        X,
        Y
    }
}