using System.Collections.Generic;

using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Assertions;

namespace Orazum.SpriteAtlas
{
    /// It should be noted that the origin is in lower left corner.
    class AtlasPackerByFreeLinkedList : AtlasPacker
    {
        int2 _atlasDims;

        LinkedList<FreeSprite> _freeSprites;
        List<CanditatesForInsertion> _canditatesBuffer;

        public override void Pack(Texture2D[] textures, out Sprite[] packedSprites, out int2 atlasDims)
        {
            _freeSprites = new LinkedList<FreeSprite>();

            SortByArea(textures);
            packedSprites = new Sprite[textures.Length];
            for (int i = 0; i < textures.Length; i++)
            {
                packedSprites[i] = new() { Pos = int2.zero, Dims = new int2(textures[i].width, textures[i].height) };
            }

            _canditatesBuffer = new(packedSprites.Length / 2);

            _atlasDims = new int2(packedSprites[0].Dims);

            for (int i = 1; i < packedSprites.Length; i++)
            {
                if (FitInAtlasIfPossible(packedSprites[i].Dims, out int2 fitInPos))
                {
                    packedSprites[i].Pos = fitInPos;
                    continue;
                }

                FitAndIncreaseAtlas(packedSprites[i].Dims, out fitInPos);
                packedSprites[i].Pos = fitInPos;
            }

            atlasDims = _atlasDims;
        }

        public override void PrepareAndPackFirst(Texture2D[] textures)
        {
            _freeSprites = new LinkedList<FreeSprite>();
            _canditatesBuffer = new(textures.Length / 2);
            SortByArea(textures);
            _atlasDims = new int2(textures[0].width, textures[0].height);
        }

        public override void PackStep(Texture2D texture, out Sprite packedSprite, out int2 atlasDims)
        {
            packedSprite = new() { Pos = int2.zero, Dims = new int2(texture.width, texture.height) };
            if (FitInAtlasIfPossible(packedSprite.Dims, out int2 fitInPos))
            {
                packedSprite.Pos = fitInPos;
                atlasDims = _atlasDims;
                return;
            }

            FitAndIncreaseAtlas(packedSprite.Dims, out fitInPos);
            packedSprite.Pos = fitInPos;
            atlasDims = _atlasDims;
        }

        public FreeSprite[] GetFreeSprites()
        {
            FreeSprite[] freeSprites = new FreeSprite[_freeSprites.Count];
            _freeSprites.CopyTo(freeSprites, 0);
            return freeSprites;
        }

        /// No increase in atlas dimnesions
        bool FitInAtlasIfPossible(in int2 fitInDims, out int2 fitInPos)
        {
            LinkedListNode<FreeSprite> current = _freeSprites.First;

            int cycleLimit = 10000;
            int cycleCount = 0;
            while (current != null && cycleCount < cycleLimit)
            {
                cycleCount++;

                Sprite freeSprite = current.Value.SpriteData;
                if (math.any(fitInDims > freeSprite.Dims))
                {
                    current = current.Next;
                    continue;
                }

                if (math.all(fitInDims == freeSprite.Dims))
                {
                    fitInPos = freeSprite.Pos;
                    _freeSprites.Remove(current);
                    return true;
                }

                _canditatesBuffer.Add(new()
                {
                    node = current,
                    FreeAreaLeft = freeSprite.Area - (fitInDims.x * fitInDims.y)
                });

                current = current.Next;
            }

            if (_canditatesBuffer.Count > 0)
            {
                _canditatesBuffer.Sort(new CanditatesForInsertionComparer());
                LinkedListNode<FreeSprite> choice = _canditatesBuffer[0].node;
                Assert.IsTrue(math.all(choice.Value.SpriteData.Dims >= fitInDims));

                fitInPos = choice.Value.SpriteData.Pos;
                SplitAfterFit(choice.Value, fitInDims);
                _freeSprites.Remove(choice);

                _canditatesBuffer.Clear();
                return true;
            }

            fitInPos = int2.zero;
            return false;
        }
        void SplitAfterFit(in FreeSprite toSplitFreeSprite, in int2 fitInDims)
        {
            Debug.Log("Split");
            Sprite toSplit = toSplitFreeSprite.SpriteData;

            AddFreeSprite(
                pos: new int2(toSplit.Pos.x + fitInDims.x, toSplit.Pos.y),
                dims: new int2(toSplit.Dims.x - fitInDims.x, toSplit.Dims.y),
                isBordering: toSplitFreeSprite.IsBordering
            );

            AddFreeSprite(
                pos: new int2(toSplit.Pos.x, toSplit.Pos.y + fitInDims.y),
                dims: new int2(fitInDims.x, toSplit.Dims.y - fitInDims.y),
                isBordering: new bool2(false, toSplitFreeSprite.IsBordering.y)
            );
        }

        void FitAndIncreaseAtlas(in int2 fitInDims, out int2 fitInPos)
        {
            // TODO:
            // LinkedListNode<FreeSprite> current = _freeSprites.First;

            // int cycleLimit = 10000;
            // int cycleCount = 0;
            // while (current != null && cycleCount < cycleLimit)
            // {
            //     cycleCount++;
            //     FreeSprite free = current.Value;
            //     if (fitInDims.x < free.SpriteData.Dims.x && free.IsBordering.y)
            //     {
            //         PlaceIntoFreeSpriteWithVerticalIncrease(free, fitInDims, out fitInPos);
            //         _freeSprites.Remove(current);
            //         return;
            //     }

            //     if (fitInDims.y < free.SpriteData.Dims.y && free.IsBordering.x)
            //     {
            //         PlaceIntoFreeSpriteWithHorizontalIncrease(free, fitInDims, out fitInPos);
            //         _freeSprites.Remove(current);
            //         return;
            //     }

            //     current = current.Next;
            // }

            if (fitInDims.x < fitInDims.y)
            {
                PlaceSpriteBorderRight(fitInDims, out fitInPos);
            }
            else
            {
                PlaceSpriteBorderUp(fitInDims, out fitInPos);
            }
        }

        void PlaceIntoFreeSpriteWithVerticalIncrease(in FreeSprite free, in int2 fitInDims, out int2 fitInPos)
        {
            int2 prevAtlasDims = _atlasDims;
            int2 freePos = free.SpriteData.Pos;
            int2 freeDims = free.SpriteData.Dims;
            fitInPos = freePos;

            int horizontalFree = freeDims.x - fitInDims.x;
            if (horizontalFree > 0)
            {
                AddFreeSprite(
                    pos: new int2(freePos.x + fitInDims.x, freePos.y),
                    dims: new int2(horizontalFree, fitInDims.y),
                    isBordering: new bool2(free.IsBordering.x, false)
                );
            }

            int verticalIncrease = freePos.y + fitInDims.y - prevAtlasDims.y;
            horizontalFree = freePos.x;
            if (horizontalFree > 0)
            {
                AddFreeSprite(
                    pos: new int2(0, prevAtlasDims.y),
                    dims: new int2(horizontalFree, verticalIncrease),
                    isBordering: new bool2(false, true)
                );
            }

            int rightPos = fitInPos.x + fitInDims.x;
            horizontalFree = prevAtlasDims.x - rightPos;
            if (horizontalFree > 0)
            {
                AddFreeSprite(
                    pos: new int2(rightPos, prevAtlasDims.y),
                    dims: new int2(horizontalFree, verticalIncrease),
                    isBordering: new bool2(true, true)
                );
            }

            _atlasDims.y += verticalIncrease;
        }

        /// Symmetical mirror of PlaceIntoFreeSpriteWithVerticalIncrease. Changed x to y.
        void PlaceIntoFreeSpriteWithHorizontalIncrease(in FreeSprite free, in int2 fitInDims, out int2 fitInPos)
        {
            int2 prevAtlasDims = _atlasDims;
            int2 freePos = free.SpriteData.Pos;
            int2 freeDims = free.SpriteData.Dims;
            fitInPos = freePos;

            int verticalFree = freeDims.y - fitInDims.y;
            if (verticalFree > 0)
            {
                AddFreeSprite(
                    pos: new int2(freePos.x, freePos.y + fitInDims.y),
                    dims: new int2(fitInDims.x, verticalFree),
                    isBordering: new bool2(false, free.IsBordering.y)
                );
            }

            int horizontalIncrease = freePos.x + fitInDims.x - prevAtlasDims.x;
            verticalFree = freePos.y;
            if (verticalFree > 0)
            {
                AddFreeSprite(
                    pos: new int2(prevAtlasDims.x, 0),
                    dims: new int2(horizontalIncrease, verticalFree),
                    isBordering: new bool2(true, false)
                );
            }

            int upPos = fitInPos.y + fitInDims.y;
            verticalFree = prevAtlasDims.y - upPos;
            if (verticalFree > 0)
            {
                AddFreeSprite(
                    pos: new int2(prevAtlasDims.x, upPos),
                    dims: new int2(horizontalIncrease, verticalFree),
                    isBordering: new bool2(true, true)
                );
            }

            _atlasDims.x += horizontalIncrease;
        }

        /// Place sprite on the right border, increasing atlas dimensions at least on fitInDims.x
        void PlaceSpriteBorderRight(in int2 fitInDims, out int2 fitInPos)
        {
            int2 prevAtlasDims = _atlasDims;
            fitInPos = new int2(_atlasDims.x, 0);
            _atlasDims.x += fitInDims.x;
            UnsetBorderedHorizontallyFreeSprites();

            int verticalDelta = prevAtlasDims.y - fitInDims.y;
            if (verticalDelta == 0)
            {
                return;
            }

            if (verticalDelta > 0)
            {
                AddFreeSprite(
                    pos: new int2(prevAtlasDims.x, fitInDims.y),
                    dims: new int2(fitInDims.x, verticalDelta),
                    isBordering: new bool2(true, false)
                );
            }
            else
            {
                int verticalIncrease = math.abs(verticalDelta);
                _atlasDims.y += verticalIncrease;
                UnsetBorderedVerticallyFreeSprites();

                AddFreeSprite(
                    pos: new int2(0, prevAtlasDims.y),
                    dims: new int2(prevAtlasDims.x, verticalIncrease),
                    isBordering: new bool2(false, true)
                );
            }
        }

        /// Symmetical mirror of PlaceSpriteBorderRight. Swapped x and y.
        void PlaceSpriteBorderUp(in int2 fitInDims, out int2 fitInPos)
        {
            int2 prevAtlasDims = _atlasDims;
            fitInPos = new int2(0, _atlasDims.y);
            _atlasDims.y += fitInDims.y;
            UnsetBorderedVerticallyFreeSprites();

            int horizontalDelta = prevAtlasDims.x - fitInDims.x;
            if (horizontalDelta == 0)
            {
                return;
            }

            if (horizontalDelta > 0)
            {
                AddFreeSprite(
                    pos: new int2(fitInDims.x, prevAtlasDims.y),
                    dims: new int2(horizontalDelta, fitInDims.y),
                    isBordering: new bool2(false, true)
                );
            }
            else
            {
                int horizontalIncrease = math.abs(horizontalDelta);
                _atlasDims.x += horizontalIncrease;
                UnsetBorderedHorizontallyFreeSprites();

                AddFreeSprite(
                    pos: new int2(prevAtlasDims.x, 0),
                    dims: new int2(horizontalIncrease, prevAtlasDims.y),
                    isBordering: new bool2(true, false)
                );
            }
        }

        void UnsetBorderedHorizontallyFreeSprites()
        {
            LinkedListNode<FreeSprite> current = _freeSprites.First;
            while (current != null)
            {
                current.Value.IsBordering.x = false;
                current = current.Next;
            }
        }

        void UnsetBorderedVerticallyFreeSprites()
        {
            LinkedListNode<FreeSprite> current = _freeSprites.First;
            while (current != null)
            {
                current.Value.IsBordering.y = false;
                current = current.Next;
            }
        }

        void AddFreeSprite(in int2 pos, in int2 dims, in bool2 isBordering)
        {
            FreeSprite free = new()
            {
                SpriteData = new()
                {
                    Pos = pos,
                    Dims = dims
                },
                IsBordering = isBordering
            };
            _freeSprites.AddLast(free);
        }

        class CanditatesForInsertion
        {
            public LinkedListNode<FreeSprite> node;
            public int FreeAreaLeft;
        }

        class CanditatesForInsertionComparer : IComparer<CanditatesForInsertion>
        {
            public int Compare(CanditatesForInsertion x, CanditatesForInsertion y)
            {
                return x.FreeAreaLeft.CompareTo(y.FreeAreaLeft); // Increasing order
            }
        }
    }

    class FreeSprite
    {
        public Sprite SpriteData;
        public bool2 IsBordering;
    }
}