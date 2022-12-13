using Unity.Mathematics;

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

        public bool Intersect(Sprite other, out Sprite intersection)
        {
            int4 borders = SpriteBorders;
            int4 otherBorders = other.Borders;

            int4 delta = borders - otherBorders;

            bool horizontalIntersect = math.all(delta.xy > 0) || math.all(delta.xy < 0);
            bool verticalIntersect = math.all(delta.zw > 0) || math.all(delta.zw < 0);
            bool isIntersecting = horizontalIntersect && verticalIntersect;
            if (!isIntersecting)
            {
                intersection = new();
                return false;
            }

            int4 minBorder = int4.zero;
            minBorder.xz = math.max(borders.xz, otherBorders.xz);
            minBorder.yw = math.min(borders.yw, otherBorders.yw);
            Assert.IsTrue(math.all(minBorder.yw > minBorder.xz));

            intersection = new Sprite(minBorder.xz, minBorder.yw - minBorder.xz);
            return true;
        }

        public bool IsAdjacent(FreeSprite other)
        {
            int4 borders = SpriteBorders;
            int4 otherBorders = other.SpriteBorders;
            int4 otherBordersArranged = otherBorders.yxwz;
            int4 delta = math.abs(SpriteBorders - otherBordersArranged);

            // 0, 100, 0, 100
            // 101, 201, 0, 100
            // 201, 101, 100, 0

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
    }
}