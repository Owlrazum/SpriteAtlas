using Unity.Mathematics;
using Orazum.Graphs;

namespace Orazum.SpriteAtlas
{ 
    public class FreeSprite : IAdjacentable<FreeSprite>
    {
        public Sprite SpriteData;
        public bool2 IsBorderingAtlas;
        public int Id { get; set; }

        public int4 SpriteBorders
        {
            get
            {
                return new int4(SpriteData.Pos.x, SpriteData.Pos.x + SpriteData.Dims.x,
                    SpriteData.Pos.y, SpriteData.Pos.y + SpriteData.Dims.y);
            }
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

        bool IsBetween(in int2 minMax, int value)
        {
            return value >= minMax.x && value <= minMax.y;
        }
    }
}