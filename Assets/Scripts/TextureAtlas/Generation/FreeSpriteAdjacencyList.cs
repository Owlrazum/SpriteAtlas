using System.Collections.Generic;
using UnityEngine.Assertions;
using Unity.Mathematics;

using UnityEngine;

namespace Orazum.SpriteAtlas.Generation
{
    /// <summary>
    /// This class was replaced with non generic class for freeSprite only, 
    /// because of custom logic required for merging of freeSprites.
    /// </summary>
    public class FreeSpriteAdjacencyList
    {
        Dictionary<int, FreeSprite> freeSprites;
        Dictionary<int, List<int>> edges;

        int idCount;
        readonly int edgesCapacity;
        readonly float maxAreaLossRatio;
        readonly float maxAreaLoss;

        public int Count { get { return freeSprites.Count; } }
        public int IdCount { get { return idCount; } }

        /// <summary>
        /// MergeTolerancy indicates what percentage of area loss is allowed to merge sprites.
        /// It is recommended to use a value between zero(almost no merge) and 0.2f
        /// </summary>
        public FreeSpriteAdjacencyList(float maxAreaLossRatio, float maxAreaLoss, int nodesCapacity, int edgesCapacity)
        {
            freeSprites = new(nodesCapacity);
            edges = new(nodesCapacity);
            this.edgesCapacity = edgesCapacity;
            this.maxAreaLossRatio = maxAreaLossRatio;
            this.maxAreaLoss = maxAreaLoss;
        }

        public void CopyNodesTo(FreeSprite[] array, int startIndex)
        {
            freeSprites.Values.CopyTo(array, startIndex);
        }

        public int Add(FreeSprite node)
        {
            int addedId = idCount;
            idCount++;

            freeSprites.Add(addedId, node);
            edges.Add(addedId, new(edgesCapacity));

            for (int i = 0; i < addedId; i++)
            {
                if (HasNode(i))
                {
                    if (node.IsAdjacent(freeSprites[i]))
                    {
                        edges[i].Add(addedId);
                        edges[addedId].Add(i);
                    }
                }
            }

            node.Id = addedId;
            MergeToMaximizeArea(addedId);

            return addedId;
        }

        void MergeToMaximizeArea(int addedId)
        {
            FreeSprite added = freeSprites[addedId];
            for (int i = 0; i < edges[addedId].Count; i++)
            {
                int otherIndex = edges[addedId][i];
                FreeSprite other = freeSprites[otherIndex];
                if (added.IsAdjacent(other, out Axis adjacencySide))
                {
                    int mergedId = Merge(added, other, adjacencySide);
                    if (mergedId > 0)
                    {
                        addedId = mergedId;
                        return;
                    }
                }
            }
        }

        // I omit some rectangles here because of complexity.
        int Merge(FreeSprite added, FreeSprite other, Axis mergeSide)
        {
            bool isMergingAlongX = mergeSide == Axis.X;
            int2 addedBorder = isMergingAlongX ? new int2(added.Pos.y, added.TopBorder) : new int2(added.Pos.x, added.RightBorder);
            int2 otherBorder = isMergingAlongX ? new int2(other.Pos.y, other.TopBorder) : new int2(other.Pos.x, other.RightBorder);
            int2 common = new int2(math.max(addedBorder.x, otherBorder.x), math.min(addedBorder.y, otherBorder.y));

            int commonLength = common.y - common.x;
            int addedBorderLoss = math.abs(commonLength - (addedBorder.y - addedBorder.x));
            int otherBorderLoss = math.abs(commonLength - (otherBorder.y - otherBorder.x));

            int addedAreaLoss = addedBorderLoss * (isMergingAlongX ? added.Dims.x : added.Dims.y);
            int otherAreaLoss = otherBorderLoss * (isMergingAlongX ? other.Dims.x : other.Dims.y);
            int areaLoss = addedAreaLoss + otherAreaLoss;
            if (areaLoss > maxAreaLoss)
            {
                return -1;
            }

            float totalArea = added.Area + other.Area;
            float lossRatio = areaLoss / totalArea;
            if (lossRatio < maxAreaLossRatio && lossRatio >= 0)
            {
                RemoveIfFound(added.Id);
                RemoveIfFound(other.Id);

                if (isMergingAlongX)
                {
                    int2 newPos = new int2(math.min(added.Pos.x, other.Pos.x), common.x);
                    int2 newDims = new int2(added.Dims.x + other.Dims.x, common.y - common.x + 1);
                    bool2 newBordering = new bool2((added.Pos.x < other.Pos.x ? other.IsBorderingAtlas.x : added.IsBorderingAtlas.x), false);
                    FreeSprite merged = new(newPos, newDims, newBordering);
                    return Add(merged);
                }
                else
                { 
                    int2 newPos = new int2(common.x, math.min(added.Pos.y, other.Pos.y));
                    int2 newDims = new int2(common.y - common.x + 1, added.Dims.y + other.Dims.y);
                    bool2 newBordering = new bool2(false, (added.Pos.y < other.Pos.y ? other.IsBorderingAtlas.y : added.IsBorderingAtlas.y));
                    FreeSprite merged = new(newPos, newDims, newBordering);
                    return Add(merged);
                }
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// The reason is that free sprite can be removed in the result of the merging.
        /// </summary>
        public void RemoveIfFound(int nodeId)
        {
            if (!freeSprites.ContainsKey(nodeId))
            {
                return;
            }

            freeSprites.Remove(nodeId);
            for (int i = 0; i < edges[nodeId].Count; i++)
            {
                int connected = edges[nodeId][i];
                edges[connected].Remove(nodeId);
            }
            edges.Remove(nodeId);
        }

        public bool HasNode(int nodeId)
        {
            return freeSprites.ContainsKey(nodeId);
        }

        public FreeSprite GetNode(int nodeId)
        {
            CheckNodeId(nodeId);
            return freeSprites[nodeId];
        }

        public List<int> GetAdjacentNodes(int nodeId)
        {
            CheckNodeId(nodeId);
            return edges[nodeId];
        }

        public List<List<FreeSprite>> GetAdjacentGroups(int minConnections = 2)
        {
            List<List<FreeSprite>> adjacentGroups = new(freeSprites.Count / 3);
            HashSet<int> checkedIds = new HashSet<int>(freeSprites.Count);
            for (int i = 0; i < idCount; i++)
            {
                if (!HasNode(i))
                {
                    continue;
                }

                if (checkedIds.Contains(i))
                {
                    continue;
                }

                int nodeId = i;
                List<FreeSprite> group = new(edges[nodeId].Count + 1);
                group.Add(freeSprites[nodeId]);
                checkedIds.Add(nodeId);
                for (int j = 0; j < edges[nodeId].Count; j++)
                {
                    int id = edges[nodeId][j];
                    TraverseConnection(checkedIds, group, id);
                }

                if (group.Count >= minConnections)
                {
                    adjacentGroups.Add(group);
                }
            }

            return adjacentGroups;
        }

        public List<FreeSprite> GetAdjacentGroup(int nodeId)
        {
            CheckNodeId(nodeId);
            List<FreeSprite> group = new(edges[nodeId].Count + 1);
            HashSet<int> checkedIds = new HashSet<int>(group.Capacity);
            group.Add(freeSprites[nodeId]);
            checkedIds.Add(nodeId);
            for (int i = 0; i < edges[nodeId].Count; i++)
            {
                int id = edges[nodeId][i];
                TraverseConnection(checkedIds, group, id);
            }

            return group;
        }

        void TraverseConnection(HashSet<int> checkedIds, List<FreeSprite> group, int nodeId)
        {
            if (checkedIds.Contains(nodeId))
            {
                return;
            }

            group.Add(freeSprites[nodeId]);
            checkedIds.Add(nodeId);
            for (int i = 0; i < edges[nodeId].Count; i++)
            {
                int id = edges[nodeId][i];
                TraverseConnection(checkedIds, group, id);
            }
        }

        void CheckNodeId(int nodeId)
        {
            if (!freeSprites.ContainsKey(nodeId))
            {
                Debug.LogError("freeSprites do not contain key!");
            }
        }
    }
}