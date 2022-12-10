using System.Collections.Generic;
using UnityEngine.Assertions;

namespace Orazum.Graphs
{
    public class AdjacencyList<T> where T : IAdjacentable<T>
    {
        Dictionary<int, T> nodes;
        Dictionary<int, List<int>> edges;

        int idCount;
        readonly int edgesCapacity;

        public int Count { get { return nodes.Count; } }
        public int IdCount { get { return idCount; } }

        public AdjacencyList(int nodesCapacityArg, int edgesCapacityArg)
        {
            nodes = new(nodesCapacityArg);
            edges = new(nodesCapacityArg);
            this.edgesCapacity = edgesCapacityArg;
        }

        public void CopyNodesTo(T[] array, int startIndex)
        {
            nodes.Values.CopyTo(array, startIndex);
        }

        public int AddNode(T node)
        {
            nodes.Add(idCount, node);
            edges.Add(idCount, new(edgesCapacity));

            int addedNodeIndex = idCount;
            for (int i = 0; i < idCount; i++)
            {
                if (HasNode(i))
                {
                    if (node.IsAdjacent(nodes[i]))
                    {
                        edges[i].Add(addedNodeIndex);
                        edges[addedNodeIndex].Add(i);
                    }
                }
            }

            idCount++;

            return idCount - 1;
        }

        public void RemoveNode(int nodeId)
        {
            CheckNodeId(nodeId);

            nodes.Remove(nodeId);
            for (int i = 0; i < edges[nodeId].Count; i++)
            {
                int connected = edges[nodeId][i];
                edges[connected].Remove(nodeId);
            }
            edges.Remove(nodeId);
        }

        public bool HasNode(int nodeId)
        {
            return nodes.ContainsKey(nodeId);
        }

        public T GetNode(int nodeId)
        {
            CheckNodeId(nodeId);
            return nodes[nodeId];
        }

        public List<int> GetAdjacentNodes(int nodeId)
        {
            CheckNodeId(nodeId);
            return edges[nodeId];
        }

        public List<List<T>> GetAdjacentGroups(int minConnections = 2)
        {
            List<List<T>> adjacentGroups = new(nodes.Count / 3 );
            HashSet<int> checkedIds = new HashSet<int>(nodes.Count);
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
                List<T> group = new(edges[nodeId].Count + 1);
                group.Add(nodes[nodeId]);
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

        public List<T> GetAdjacentGroup(int nodeId)
        {
            CheckNodeId(nodeId);
            List<T> group = new(edges[nodeId].Count + 1);
            HashSet<int> checkedIds = new HashSet<int>(group.Capacity);
            group.Add(nodes[nodeId]);
            checkedIds.Add(nodeId);
            for (int i = 0; i < edges[nodeId].Count; i++)
            {
                int id = edges[nodeId][i];
                TraverseConnection(checkedIds, group, id);
            }

            return group;
        }

        void TraverseConnection(HashSet<int> checkedIds, List<T> group, int nodeId)
        {
            if (checkedIds.Contains(nodeId))
            {
                return;
            }

            group.Add(nodes[nodeId]);
            checkedIds.Add(nodeId);
            for (int i = 0; i < edges[nodeId].Count; i++)
            {
                int id = edges[nodeId][i];
                TraverseConnection(checkedIds, group, id);
            }
        }

        void CheckNodeId(int nodeId)
        {
            Assert.IsTrue(nodes.ContainsKey(nodeId));
        }
    }
}