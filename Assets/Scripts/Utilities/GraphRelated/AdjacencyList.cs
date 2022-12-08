using System;
using System.Collections.Generic;

namespace Orazum.Graphs
{
    public class AdjacencyList<T> where T : IAdjacentable<T>
    {
        Dictionary<int, T> nodes;
        Dictionary<int, List<int>> edges;
        
        int idCount;
        readonly int edgesCapacity;

        public int Count { get { return nodes.Count; } }

        public AdjacencyList(int nodesCapacityArg, int edgesCapacityArg)
        {
            nodes = new(nodesCapacityArg);
            edges = new(nodesCapacityArg);
            this.edgesCapacity = edgesCapacityArg;
        }

        public int AddNode(T node)
        {
            nodes.Add(idCount, node);
            edges.Add(idCount, new(edgesCapacity));
            idCount++;

            int addedNodeIndex = nodes.Count - 1;
            for (int i = 0; i < idCount; i++)
            {
                if (node.IsAdjacent(nodes[i]))
                {
                    edges[i].Add(addedNodeIndex);
                    edges[addedNodeIndex].Add(i);
                }
            }

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

        void CheckNodeId(int nodeId)
        { 
            if (nodeId < 0 || nodeId >= idCount)
            {
                throw new System.ArgumentOutOfRangeException($"Node index {nodeId} is out of range!");
            }
        }
    }
}