using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Assertions;

using UnityEditor;

using Orazum.Utilities;

namespace Orazum.TextureAtlas
{
    // Link that helped me: http://blackpawn.com/texts/lightmaps/default.html
    // Somewhere in the middle tried to use two lists, but they seemed a little more complex somehow.
    class AtlasPackingByBinaryTree
    {
        Node _root;

        private int2 _totalDims;
        public int2 TotalDims { get { return _totalDims; } }

        public AtlasPackingByBinaryTree()
        {
            _root = new Node();
        }

        public Rectangle Insert(int width, int height)
        {
            Node placementNode = _root.Insert(new int2(width, height));
            Assert.IsNotNull(placementNode);
            Rectangle rectangle = placementNode.GetRectangle();
            UpdateTotalDims(rectangle);

            return rectangle;
        }

        void UpdateTotalDims(Rectangle rectangle)
        {
            int x = rectangle.Pos.x + rectangle.Dims.x;
            int y = rectangle.Pos.y + rectangle.Dims.y;
            if (x > _totalDims.x)
            {
                _totalDims.x = x;
            }

            if (y > TotalDims.y)
            {
                _totalDims.y = y;
            }
        }

        public int2 GetDims()
        {
            return _totalDims;
        }

        public int GetWastedArea()
        {
            return -1;
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

        static int s_callStack = 0;
        const int MaxCallStack = 100000;

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
            if (s_callStack > MaxCallStack)
            {
                return null;
            }
            else
            {
                s_callStack++;
            }

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

            if (_rectangle.Dims.x == Infinite && _rectangle.Dims.y == Infinite)
            {
                isHorizontalSplit = spriteDims.x > spriteDims.y;
                Debug.Log("xy inf");
            }
            else if (_rectangle.Dims.x == Infinite)
            {
                isHorizontalSplit = false;
                Debug.Log("x inf");
            }
            else if (_rectangle.Dims.y == Infinite)
            {
                isHorizontalSplit = true;
                Debug.Log("y inf");
            }
            else
            { 
                int2 delta = _rectangle.Dims - spriteDims;
                isHorizontalSplit = delta.x < delta.y;
                Debug.Log($"isHoriaontalSplit = {delta.x} < {delta.y}");
            }

            if (isHorizontalSplit)
            {
                // split line dividing rectangles goes horizontally
                int2 newDim = new int2(_rectangle.Dims.x, spriteDims.y);
                _left._rectangle = new Rectangle(_rectangle.Pos, newDim);
                int2 newPos = new int2(_rectangle.Pos.x, _rectangle.Pos.y + spriteDims.y);
                newDim = new int2(_rectangle.Dims.x, _rectangle.Dims.y);
                _right._rectangle = new Rectangle(newPos, newDim);
                Debug.Log($"Horizontal, {_left._rectangle} {_right._rectangle}");

                DebugUtilities.DrawRectangle(_left._rectangle, Color.red, 10000);
                DebugUtilities.DrawRectangle(_right._rectangle, Color.green, 10000);
            }
            else
            {
                // split line dividing rectangles goes vertically
                int2 newDim = new int2(spriteDims.x, _rectangle.Dims.y);
                _left._rectangle = new Rectangle(_rectangle.Pos, newDim);
                int2 newPos = new int2(_rectangle.Pos.x + spriteDims.x, _rectangle.Pos.y);
                newDim = new int2(_rectangle.Dims.x, _rectangle.Dims.y);
                _right._rectangle = new Rectangle(newPos, newDim);
                Debug.Log($"Vertical, {_left._rectangle} {_right._rectangle}");

                DebugUtilities.DrawRectangle(_left._rectangle, Color.red, 10000);
                DebugUtilities.DrawRectangle(_right._rectangle, Color.green, 10000);
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