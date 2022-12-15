using System;
using System.Collections.Generic;

using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Assertions;

namespace Orazum.SpriteAtlas.Generation
{
    /// It should be noted that the origin is in lower left corner.
    public class AtlasPackerByFreeSpritesAndAdjacency : AtlasPacker
    {
        readonly float MaxAreaLoss;
        readonly float MaxAreaLossRatio;
        readonly float MaxAtlasIncreaseOnFit;

        int2 atlasDims;

        FreeSpriteAdjacencyList freeSprites;

        List<CanditateForInsertion> canditatesBuffer;

        public AtlasPackerByFreeSpritesAndAdjacency(
            float maxAreaLoss = 50 * 50, 
            float maxAreaLossRatio = 0.2f, 
            float maxAtlasIncreaseOnFit = 0.5f)
        {
            MaxAreaLoss = maxAreaLoss;
            MaxAreaLossRatio = maxAreaLossRatio;
            MaxAtlasIncreaseOnFit = maxAtlasIncreaseOnFit;
        }

        public override void Pack(Texture2D[] textures, out SpriteManaged[] packedSprites, out int2 atlasDimsOut)
        {
            PrepareAndPackFirstTexture(textures);

            packedSprites = new SpriteManaged[textures.Length];
            for (int i = 0; i < textures.Length; i++)
            {
                packedSprites[i] = new(int2.zero, new int2(textures[i].width, textures[i].height));
            }

            for (int i = 1; i < packedSprites.Length; i++)
            {
                PrivatePackStep(packedSprites[i].Dims, out int2 fitInPos);
                packedSprites[i].Pos = fitInPos;
            }

            atlasDimsOut = atlasDims;
        }

        public override void PrepareAndPackFirstTexture(Texture2D[] textures)
        {
            SortByArea(textures);
            freeSprites = new(MaxAreaLossRatio, MaxAreaLoss, textures.Length, 5);
            canditatesBuffer = new(textures.Length / 2);
            atlasDims = new int2(textures[0].width, textures[0].height);
        }

        public override void PackStep(Texture2D texture, out SpriteManaged packedSprite, out int2 atlasDimsOut)
        {
            packedSprite = new(int2.zero, new int2(texture.width, texture.height));
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
            for (int i = 0; i < freeSprites.IdCount; i++)
            {
                if (freeSprites.HasNode(i))
                {
                    SpriteManaged freeSprite = freeSprites.GetNode(i).SpriteData;

                    if (math.any(fitInDims > freeSprite.Dims))
                    {
                        continue;
                    }

                    if (math.all(fitInDims == freeSprite.Dims))
                    {
                        fitInPos = freeSprite.Pos;
                        freeSprites.RemoveIfFound(i);
                        return true;
                    }

                    canditatesBuffer.Add(new()
                    {
                        NodeIndex = i,
                        FreeAreaLeft = freeSprite.Area - (fitInDims.x * fitInDims.y)
                    });
                }
            }

            if (canditatesBuffer.Count > 0)
            {
                canditatesBuffer.Sort();
                int choiceIndex = canditatesBuffer[0].NodeIndex;
                FreeSprite freeSprite = freeSprites.GetNode(choiceIndex);
                Assert.IsTrue(math.all(freeSprite.SpriteData.Dims >= fitInDims));

                fitInPos = freeSprite.SpriteData.Pos;
                freeSprites.RemoveIfFound(choiceIndex); // important first to remove from adjacency list, before any merge, of loss and incorrect behaviour is possible.
                SplitAfterFit(freeSprite, fitInDims);

                canditatesBuffer.Clear();
                return true;
            }

            fitInPos = int2.zero;
            return false;

            void SplitAfterFit(in FreeSprite toSplitFreeSprite, in int2 fitInDims)
            {
                SpriteManaged toSplit = toSplitFreeSprite.SpriteData;

                int rightSize = toSplit.Dims.x - fitInDims.x;
                if (rightSize > 0)
                {
                    AddFreeSprite(
                        pos: new int2(toSplit.Pos.x + fitInDims.x, toSplit.Pos.y),
                        dims: new int2(rightSize, toSplit.Dims.y),
                        isBordering: toSplitFreeSprite.IsBorderingAtlas
                    );
                }

                int topSize = toSplit.Dims.y - fitInDims.y;
                if (topSize > 0)
                {
                    AddFreeSprite(
                        pos: new int2(toSplit.Pos.x, toSplit.Pos.y + fitInDims.y),
                        dims: new int2(fitInDims.x, topSize),
                        isBordering: new bool2(false, toSplitFreeSprite.IsBorderingAtlas.y)
                    );
                }
            }
        }

        bool FitInWithAtlasIncrease(in int2 fitInDims, out int2 fitInPos)
        {
            for (int i = 0; i < freeSprites.IdCount; i++)
            {
                if (freeSprites.HasNode(i))
                {
                    FreeSprite free = freeSprites.GetNode(i);
                    if (fitInDims.x <= free.Dims.x && free.IsBorderingAtlas.y)
                    {
                        int verticalIncrease = free.Pos.y + fitInDims.y - atlasDims.y;
                        if (verticalIncrease / (float)fitInDims.y < 0.5f)
                        {
                            freeSprites.RemoveIfFound(i);
                            PlaceIntoFreeSpriteWithVerticalIncrease(free, fitInDims, out fitInPos);
                            return true;
                        }
                    }

                    if (fitInDims.y <= free.SpriteData.Dims.y && free.IsBorderingAtlas.x)
                    {
                        int horizontalIncrease = free.Pos.x + fitInDims.x - atlasDims.x;
                        if (horizontalIncrease / (float)fitInDims.x < 0.5f)
                        {
                            freeSprites.RemoveIfFound(i);
                            PlaceIntoFreeSpriteWithHorizontalIncrease(free, fitInDims, out fitInPos);
                            return true;
                        }
                    }
                }
            }

            fitInPos = new int2(-1, -1);
            return false;
        }
        void PlaceIntoFreeSpriteWithVerticalIncrease(in FreeSprite free, in int2 fitInDims, out int2 fitInPos)
        {
            int2 prevAtlasDims = atlasDims;
            int2 freePos = free.SpriteData.Pos;
            int2 freeDims = free.SpriteData.Dims;
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
            int2 freePos = free.SpriteData.Pos;
            int2 freeDims = free.SpriteData.Dims;
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
            FreeSprite free = new(pos, dims, isBordering);
            freeSprites.Add(free);
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