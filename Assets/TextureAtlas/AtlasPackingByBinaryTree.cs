using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Assertions;

namespace Orazum.TextureAtlas
{
    // Link that helped me: http://blackpawn.com/texts/lightmaps/default.html
    // Somewhere in the middle tried to use two lists, but they seemed a little more complex somehow.
    class AtlasPackingByBinaryTree
    {
        Node _root;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int FreeArea { get; private set; }

        public AtlasPackingByBinaryTree()
        {
            _root = new Node();
        }

        public Rectangle Insert(int width, int height)
        {
            Node placementNode = _root.Insert(new int2(width, height));
            Assert.IsNotNull(placementNode);
            return placementNode.GetRectangle();
        }
    }

    class Node
    {
        Node _left;
        Node _right; // free rectangle
        Rectangle _rectangle;
        bool _isEmpty;

        bool _isLeaf => _left == null;

        const int Infinite = -1;

        public Node()
        {
            _isEmpty = true;
            _rectangle = new Rectangle(int2.zero, new int2(Infinite, Infinite));
        }

        public Rectangle GetRectangle()
        {
            return _rectangle;
        }

        public Node Insert(int2 spriteDims)
        {
            if (!_isLeaf)
            {
                Node insertPlace = _left.Insert(spriteDims);
                if (insertPlace != null)
                {
                    return insertPlace;
                }

                return _right.Insert(spriteDims);
            }
            else
            {
                if (!_isEmpty)
                {
                    return null;
                }

                if (!DoesFitRectangle(spriteDims, _rectangle))
                {
                    return null;
                }

                if (DoesFitPerfectly(spriteDims, _rectangle))
                {
                    _isEmpty = false;
                    return this;
                }

                Node placementNode = Split(spriteDims);
                return placementNode.Insert(spriteDims);
            }
        }

        Node Split(int2 spriteDims)
        {
            _left = new Node();
            _right = new Node();

            bool isHorizontalSplit = false;

            if (_rectangle.Dims.x != Infinite)
            {
                isHorizontalSplit = true;
            }
            else if (spriteDims.x < spriteDims.y)
            {
                isHorizontalSplit = true;
            }

            if (isHorizontalSplit)
            {
                // split line dividing rectangles goes horizontally
                int2 newDim = new int2(Infinite, spriteDims.y);
                _left._rectangle = new Rectangle(_rectangle.Pos, newDim);
                int2 newPos = new int2(_rectangle.Pos.x, spriteDims.y + 1);
                newDim = new int2(Infinite, Infinite);
                _right._rectangle = new Rectangle(newPos, newDim);
            }
            else
            {
                // split line dividing rectangles goes vertically
                int2 newDim = new int2(spriteDims.x, Infinite);
                _left._rectangle = new Rectangle(_rectangle.Pos, newDim);
                int2 newPos = new int2(spriteDims.x + 1, _rectangle.Pos.y);
                newDim = new int2(Infinite, Infinite);
                _right._rectangle = new Rectangle(newPos, newDim);
            }

            return _left;
        }

        bool DoesFitRectangle(int2 spriteDims, Rectangle rectangle)
        {
            if (rectangle.Dims.x != Infinite && spriteDims.x > rectangle.Dims.x)
            {
                return false;
            }

            if (rectangle.Dims.y != Infinite && spriteDims.y > rectangle.Dims.y)
            {
                return false;
            }

            return true;
        }

        bool DoesFitPerfectly(int2 spriteDims, Rectangle rectangle)
        {
            if (math.all(spriteDims == rectangle.Dims))
            {
                return true;
            }

            return false;
        }
    }
}