using System;
using System.Collections.Generic;

using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Assertions;

using Orazum.Graphs;

using static Orazum.Utilities.Math;

namespace Orazum.SpriteAtlas
{
    /// It should be noted that the origin is in lower left corner.
    class AtlasPackerByFreeSpritesAndAdjacency : AtlasPacker
    {
        const int CycleLimit = 10000;

        int2 atlasDims;

        AdjacencyList<FreeSprite> freeSprites;

        List<CanditateForInsertion> _canditatesBuffer;

        int2 minFitInDims;

        public override void Pack(Texture2D[] textures, out Sprite[] packedSprites, out int2 atlasDimsOut)
        {
            packedSprites = new Sprite[textures.Length];
            for (int i = 0; i < textures.Length; i++)
            {
                packedSprites[i] = new() { Pos = int2.zero, Dims = new int2(textures[i].width, textures[i].height) };
            }

            freeSprites = new AdjacencyList<FreeSprite>(textures.Length, 5);
            _canditatesBuffer = new(packedSprites.Length / 2);
            SortByArea(textures);
            atlasDims = new int2(packedSprites[0].Dims);

            InitializeMinFitInDims(textures);

            for (int i = 1; i < packedSprites.Length; i++)
            {
                PrivatePackStep(packedSprites[i].Dims, out int2 fitInPos);
                packedSprites[i].Pos = fitInPos;
            }

            atlasDimsOut = atlasDims;
        }

        public override void PrepareAndPackFirst(Texture2D[] textures)
        {
            freeSprites = new AdjacencyList<FreeSprite>(textures.Length, 5);
            _canditatesBuffer = new(textures.Length / 2);
            SortByArea(textures);
            atlasDims = new int2(textures[0].width, textures[0].height);

            InitializeMinFitInDims(textures);
        }

        void InitializeMinFitInDims(Texture2D[] textures)
        {
            minFitInDims = new int2(textures[0].width, textures[0].height);
            for (int i = 1; i < textures.Length; i++)
            {
                int2 fitInDims = new int2(textures[i].width, textures[i].height);
                if (fitInDims.x < minFitInDims.y)
                {
                    minFitInDims.x = fitInDims.x;
                }

                if (fitInDims.y < minFitInDims.y)
                {
                    minFitInDims.y = fitInDims.y;
                }
            }
        }

        public override void PackStep(Texture2D texture, out Sprite packedSprite, out int2 atlasDimsOut)
        {
            packedSprite = new() { Pos = int2.zero, Dims = new int2(texture.width, texture.height) };
            PrivatePackStep(packedSprite.Dims, out int2 fitInPos);
            packedSprite.Pos = fitInPos;
            atlasDimsOut = atlasDims;
        }

        void PrivatePackStep(in int2 fitInDims, out int2 fitInPos)
        {
            if (FitInWithoutAtlasIncrease(fitInDims, out fitInPos))
            {
                return;
            }

            if (FitInWithAtlasIncrease(fitInDims, out fitInPos))
            {
                return;
            }

            PlaceOnBorder(fitInDims, out fitInPos);
        }

        public FreeSprite[] GetFreeSprites()
        {
            FreeSprite[] freeSpritesArray = new FreeSprite[freeSprites.Count];
            freeSprites.CopyNodesTo(freeSpritesArray, 0);
            return freeSpritesArray;
        }

        bool FitInWithoutAtlasIncrease(in int2 fitInDims, out int2 fitInPos)
        {
            if (FitInExisitingFreeSpriteWithoutAtlasIncrease(fitInDims, out fitInPos))
            {
                return true;
            }

            return FitInMergedFreeSpriteWithoutAtlasIncrease(fitInDims, out fitInPos);
        }
        bool FitInExisitingFreeSpriteWithoutAtlasIncrease(in int2 fitInDims, out int2 fitInPos)
        {
            for (int i = 0; i < freeSprites.IdCount; i++)
            {
                if (freeSprites.HasNode(i))
                {
                    FreeSprite freeSprite = freeSprites.GetNode(i);

                    if (math.any(fitInDims > freeSprite.Dims))
                    {
                        continue;
                    }

                    if (math.all(fitInDims == freeSprite.Dims))
                    {
                        fitInPos = freeSprite.Pos;
                        freeSprites.RemoveNode(i);
                        return true;
                    }

                    _canditatesBuffer.Add(new()
                    {
                        NodeIndex = i,
                        FreeAreaLeft = freeSprite.Area - (fitInDims.x * fitInDims.y)
                    });
                }
            }

            if (_canditatesBuffer.Count > 0)
            {
                _canditatesBuffer.Sort();
                int choiceIndex = _canditatesBuffer[0].NodeIndex;
                FreeSprite freeSprite = freeSprites.GetNode(choiceIndex);
                Assert.IsTrue(math.all(freeSprite.Dims >= fitInDims));

                fitInPos = freeSprite.Pos;
                SplitAfterFitInExistingFreeSprite(freeSprite, fitInDims);
                freeSprites.RemoveNode(choiceIndex);

                _canditatesBuffer.Clear();
                return true;
            }

            fitInPos = int2.zero;
            return false;
        }
        void SplitAfterFitInExistingFreeSprite(in FreeSprite toSplit, in int2 fitInDims)
        {
            AddFreeSprite(
                pos: new int2(toSplit.Pos.x + fitInDims.x, toSplit.Pos.y),
                dims: new int2(toSplit.Dims.x - fitInDims.x, toSplit.Dims.y),
                isBordering: toSplit.IsBorderingAtlas
            );

            AddFreeSprite(
                pos: new int2(toSplit.Pos.x, toSplit.Pos.y + fitInDims.y),
                dims: new int2(fitInDims.x, toSplit.Dims.y - fitInDims.y),
                isBordering: new bool2(false, toSplit.IsBorderingAtlas.y)
            );
        }
        bool FitInMergedFreeSpriteWithoutAtlasIncrease(in int2 fitInDims, out int2 fitInPos)
        {
            var groups = freeSprites.GetAdjacentGroups(2);
            if (groups.Count == 0)
            {
                fitInPos = int2.zero;
                return false;
            }

            for (int i = 0; i < groups.Count; i++)
            {
                bool canFit = EvaluateGroupWithoutAtlasIncrease(fitInDims, groups[i], out fitInPos);
                if (!canFit)
                {
                    continue;
                }
                InsertIntoGroupWithoutAtlasIncrease(fitInPos, fitInDims, groups[i]);
                return true;
            }

            fitInPos = int2.zero;
            return false;
        }
        bool EvaluateGroupWithoutAtlasIncrease(in int2 fitInDims, List<FreeSprite> group, out int2 fitInPos)
        {
            /// It is also possible to use minimal freeAreaLeft heuristic, but I decided to not implement it now.
            /// It does not consider the order in which group's sprites are supplied, thus it is a point in which one can improve this method
            Sprite mergedSprite = new Sprite(group[0].Pos, group[0].Dims);
            if (CanFitInMergedBorders(fitInDims))
            {
                fitInPos = mergedSprite.Pos;
                return true;
            }

            for (int i = 0; i < group.Count; i++)
            {
                Sprite toMerge = new Sprite(group[i].Pos, group[i].Dims);
                UpdateMergedBorders(toMerge);
                if (CanFitInMergedBorders(fitInDims))
                {
                    fitInPos = mergedSprite.Pos;
                    return true;
                }
            }

            fitInPos = new int2(-1, -1);
            return false;

            bool CanFitInMergedBorders(in int2 fitInDims)
            {
                return fitInDims.x < mergedSprite.Dims.x && fitInDims.y < mergedSprite.Dims.y;
            }

            void UpdateMergedBorders(in Sprite toMerge)
            {
                if (toMerge.Pos.x > mergedSprite.RightBorder)
                {
                    if (toMerge.Pos.y > mergedSprite.TopBorder || toMerge.TopBorder < mergedSprite.Pos.y)
                    {
                        // ignore due to complexity related to supply order of orders
                        // can be implemented in the future
                    }
                    else
                    {
                        if (toMerge.Pos.y > mergedSprite.Pos.y)
                        {
                            mergedSprite.Pos.y = toMerge.Pos.y;
                        }
                        if (toMerge.TopBorder < mergedSprite.TopBorder)
                        {
                            int delta = toMerge.TopBorder - mergedSprite.TopBorder;
                            int2 newDims = new int2(mergedSprite.Dims.x, mergedSprite.Dims.y + delta);
                            mergedSprite.Dims += newDims;
                        }
                    }
                }
                else if (toMerge.Pos.y > mergedSprite.TopBorder)
                {
                    if (toMerge.Pos.x > mergedSprite.RightBorder || toMerge.RightBorder < mergedSprite.Pos.x)
                    {
                        // ignore due to complexity related to supply order of orders
                        // can be implemented in the future
                    }
                    else
                    {
                        if (toMerge.Pos.x > mergedSprite.Pos.x)
                        {
                            mergedSprite.Pos.x = toMerge.Pos.x;
                        }
                        if (toMerge.RightBorder < mergedSprite.RightBorder)
                        {
                            int delta = toMerge.RightBorder - mergedSprite.RightBorder;
                            int2 newDims = new int2(mergedSprite.Dims.x + delta, mergedSprite.Dims.y);
                            mergedSprite.Dims = newDims;
                        }
                    }
                }
                else
                {
                    // ignore due to complexity related to supply order of orders
                }
            }
        }

        void InsertIntoGroupWithoutAtlasIncrease(in int2 fitInPos, in int2 fitInDims, List<FreeSprite> group)
        {
            Sprite fitInSprite = new Sprite(fitInPos, fitInDims);
            for (int i = 0; i < group.Count; i++)
            {
                if (!group[i].Intersect(fitInSprite, out Sprite intersection))
                {
                    continue;
                }

                SplitFreeSpriteAfterIntersection(group[i], intersection);
                freeSprites.RemoveNode(group[i].Id);
            }
        }
        void SplitFreeSpriteAfterIntersection(FreeSprite freeSprite, in Sprite intersection)
        {
            int leftSize = intersection.Pos.x - freeSprite.Pos.x;
            int bottomSize = intersection.Pos.y - freeSprite.Pos.y;
            int rightSize = freeSprite.RightBorder - intersection.RightBorder;
            int topSize = freeSprite.TopBorder - intersection.TopBorder;

            if (leftSize > 0)
            {
                int2 leftDims = new int2(leftSize, freeSprite.Dims.y - topSize);
                AddFreeSprite(freeSprite.Pos, leftDims, new bool2(false, false));
            }

            if (bottomSize > 0)
            {
                int2 bottomDims = new int2(freeSprite.Dims.x - leftSize - rightSize, bottomSize);
                int2 bottomPos = new int2(freeSprite.Pos.x + leftSize, freeSprite.Pos.y);
                AddFreeSprite(bottomPos, bottomDims, new bool2(false, false));
            }

            if (rightSize > 0)
            {
                int2 rightDims = new int2(rightSize, freeSprite.Dims.y);
                int2 rightPos = new int2(freeSprite.RightBorder - rightSize, freeSprite.Pos.y);
                AddFreeSprite(rightPos, rightDims, freeSprite.IsBorderingAtlas);
            }

            if (topSize > 0)
            {
                int2 topDims = new int2(freeSprite.Dims.x - rightSize, topSize);
                int2 topPos = new int2(freeSprite.Pos.x, freeSprite.TopBorder - topSize);
                AddFreeSprite(topPos, topDims, new bool2(false, freeSprite.IsBorderingAtlas.y));
            }
        }

        bool FitInWithAtlasIncrease(in int2 fitInDims, out int2 fitInPos)
        {
            if (FitInExistingFreeSpriteWithAtlasIncrease(fitInDims, out fitInPos))
            {
                return true;
            }

            if (FitInMergedFreeSpriteWithAtlasIncrease(fitInDims, out fitInPos))
            {
                return true;
            }

            return false;
        }
        bool FitInExistingFreeSpriteWithAtlasIncrease(in int2 fitInDims, out int2 fitInPos)
        {
            for (int i = 0; i < freeSprites.IdCount; i++)
            {
                if (freeSprites.HasNode(i))
                {
                    FreeSprite free = freeSprites.GetNode(i);
                    if (fitInDims.x <= free.Dims.x && free.IsBorderingAtlas.y)
                    {
                        freeSprites.RemoveNode(i);
                        PlaceIntoFreeSpriteWithVerticalIncrease(free, fitInDims, out fitInPos);
                        return true;
                    }

                    if (fitInDims.y <= free.Dims.y && free.IsBorderingAtlas.x)
                    {
                        freeSprites.RemoveNode(i);
                        PlaceIntoFreeSpriteWithHorizontalIncrease(free, fitInDims, out fitInPos);
                        return true;
                    }
                }
            }

            fitInPos = new int2(-1, -1);
            return false;
        }
        bool FitInMergedFreeSpriteWithAtlasIncrease(in int2 fitInDims, out int2 fitInPos)
        {
            fitInPos = new int2(-1, -1);
            return false;
        }

        void PlaceIntoFreeSpriteWithVerticalIncrease(in FreeSprite free, in int2 fitInDims, out int2 fitInPos)
        {
            int2 prevAtlasDims = atlasDims;
            int2 freePos = free.Pos;
            int2 freeDims = free.Dims;
            fitInPos = freePos;

            bool2 freeIsBordering = free.IsBorderingAtlas;

            int verticalIncrease = freePos.y + fitInDims.y - prevAtlasDims.y;
            atlasDims.y += verticalIncrease;
            UnsetBorderedVerticallyFreeSprites();

            int horizontalFree = freeDims.x - fitInDims.x;
            if (horizontalFree > 0)
            {
                AddFreeSprite(
                    pos: new int2(freePos.x + fitInDims.x, freePos.y),
                    dims: new int2(horizontalFree, prevAtlasDims.y - freePos.y),
                    isBordering: new bool2(freeIsBordering.x, false)
                );
            }

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
        }
        void PlaceIntoFreeSpriteWithHorizontalIncrease(in FreeSprite free, in int2 fitInDims, out int2 fitInPos)
        {
            int2 prevAtlasDims = atlasDims;
            int2 freePos = free.Pos;
            int2 freeDims = free.Dims;
            fitInPos = freePos;

            bool2 freeIsBordering = free.IsBorderingAtlas;

            int horizontalIncrease = freePos.x + fitInDims.x - prevAtlasDims.x;
            atlasDims.x += horizontalIncrease;
            UnsetBorderedHorizontallyFreeSprites();

            int verticalFree = freeDims.y - fitInDims.y;
            if (verticalFree > 0)
            {
                AddFreeSprite(
                    pos: new int2(freePos.x, freePos.y + fitInDims.y),
                    dims: new int2(prevAtlasDims.x - freePos.x, verticalFree),
                    isBordering: new bool2(false, freeIsBordering.y)
                );
            }

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
        }

        void PlaceOnBorder(in int2 fitInDims, out int2 fitInPos)
        {
            if (fitInDims.x < fitInDims.y)
            {
                PlaceOnRightBorder(fitInDims, out fitInPos);
            }
            else
            {
                PlaceOnUpBorder(fitInDims, out fitInPos);
            }
        }
        void PlaceOnRightBorder(in int2 fitInDims, out int2 fitInPos)
        {
            int2 prevAtlasDims = atlasDims;
            fitInPos = new int2(atlasDims.x, 0);
            atlasDims.x += fitInDims.x;
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
                atlasDims.y += verticalIncrease;
                UnsetBorderedVerticallyFreeSprites();

                AddFreeSprite(
                    pos: new int2(0, prevAtlasDims.y),
                    dims: new int2(prevAtlasDims.x, verticalIncrease),
                    isBordering: new bool2(false, true)
                );
            }
        }
        void PlaceOnUpBorder(in int2 fitInDims, out int2 fitInPos)
        {
            int2 prevAtlasDims = atlasDims;
            fitInPos = new int2(0, atlasDims.y);
            atlasDims.y += fitInDims.y;
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
                atlasDims.x += horizontalIncrease;
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
            for (int i = 0; i < freeSprites.IdCount; i++)
            {
                if (freeSprites.HasNode(i))
                {
                    freeSprites.GetNode(i).IsBorderingAtlas.x = false;
                }
            }
        }
        void UnsetBorderedVerticallyFreeSprites()
        {
            for (int i = 0; i < freeSprites.IdCount; i++)
            {
                if (freeSprites.HasNode(i))
                {
                    freeSprites.GetNode(i).IsBorderingAtlas.y = false;
                }
            }
        }

        void AddFreeSprite(in int2 pos, in int2 dims, in bool2 isBordering)
        {
            FreeSprite free = new(pos, dims)
            {
                IsBorderingAtlas = isBordering
            };
            free.Id = freeSprites.AddNode(free);
        }

        class CanditateForInsertion : IComparable<CanditateForInsertion>
        {
            public int NodeIndex;
            public int FreeAreaLeft;

            public int CompareTo(CanditateForInsertion other)
            {
                return FreeAreaLeft.CompareTo(other.FreeAreaLeft); // Increasing order
            }
        }
    }
}