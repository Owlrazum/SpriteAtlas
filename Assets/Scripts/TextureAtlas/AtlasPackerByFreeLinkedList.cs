using System.Collections.Generic;

using Unity.Mathematics;

using UnityEngine.Assertions;

namespace Orazum.SpriteAtlas
{
    /// It should be noted that the origin is in lower left corner.
    class AtlasPackerByPass : AtlasPacker
    {
        LinkedList<Sprite> _freeSprites;
        int2 _atlasDims;
        
        public override void Pack(Sprite[] spritesToPack)
        {
            SortByArea(spritesToPack);

            _atlasDims = new int2(spritesToPack[0].Dims);

            for (int i = 1; i < spritesToPack.Length; i++)
            {
                if (FitInAtlasIfPossible(spritesToPack[i].Dims, out int2 fitInPos))
                {
                    spritesToPack[i].Pos = fitInPos;
                    continue;
                }

                FitAndIncreaseAtlas(spritesToPack[i].Dims, out fitInPos);
                spritesToPack[i].Pos = fitInPos;
            }
        }

        bool FitInAtlasIfPossible(in int2 fitInDims, out int2 fitInPos)
        {
            LinkedListNode<Sprite> first = _freeSprites.First;
            LinkedListNode<Sprite> current = first;

            List<CanditatesForInsertion> canditatesForInsertion = new(_freeSprites.Count / 2);

            while (current != null)
            {
                Sprite freeSprite = current.Value;
                if (math.any(freeSprite.Dims > fitInDims))
                {
                    current = current.Next;
                    continue;
                }

                if (math.all(freeSprite.Dims == fitInDims))
                {
                    fitInPos = freeSprite.Pos;
                    _freeSprites.Remove(current);
                    return true;
                }

                canditatesForInsertion.Add(new() 
                { 
                    node = current, 
                    FreeAreaLeft = freeSprite.Area - (fitInDims.x * fitInDims.y)
                });
            }

            if (canditatesForInsertion.Count > 0)
            {
                canditatesForInsertion.Sort(new CanditatesForInsertionComparer());
                LinkedListNode<Sprite> choice = canditatesForInsertion[0].node;
                _freeSprites.Remove(choice);
                fitInPos = choice.Value.Pos;

                (Sprite first, Sprite second) newFreeSprites = SplitAfterFit(choice.Value, fitInDims);
                _freeSprites.AddLast(newFreeSprites.first);
                _freeSprites.AddLast(newFreeSprites.second);
                return true;
            }

            fitInPos = int2.zero;
            return false;
        }

        (Sprite, Sprite) SplitAfterFit(in Sprite toSplit, in int2 fitInDims)
        {
            Assert.IsTrue(math.all(toSplit.Dims > fitInDims));

            int2 firstPos = new int2(toSplit.Pos.x + fitInDims.x, toSplit.Pos.y);
            int2 firstDims = new int2(toSplit.Dims.x - fitInDims.x, toSplit.Dims.y);

            Sprite first = new() { Pos = firstPos, Dims = firstDims };

            int2 secondPos = new int2(toSplit.Pos.x, toSplit.Pos.x + fitInDims.y);
            int2 secondDims = new int2(toSplit.Dims.x, toSplit.Dims.y - fitInDims.x);

            Sprite second = new() { Pos = secondPos, Dims = secondDims };

            return (first, second);
        }

        void FitAndIncreaseAtlas(in int2 fitInDims, out int2 fitInPos)
        {
            //TODO:
            throw new System.NotImplementedException();
        }

        class CanditatesForInsertion
        {
            public LinkedListNode<Sprite> node;
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
}