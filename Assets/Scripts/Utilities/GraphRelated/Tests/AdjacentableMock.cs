using System;
using System.Collections.Generic;

namespace Orazum.Graphs.Tests
{
    /// <summary>
    /// AdjIds delta less than 10 is considered as adjacent
    /// </summary>
    class AdjacentableMock : IAdjacentable<AdjacentableMock>
    {
        HashSet<AdjacentableMock> connections;
        public int Id { get; set; }

        public AdjacentableMock(int idArg, int connectionsCapacity = 5)
        {
            connections = new HashSet<AdjacentableMock>(5);
            Id = idArg;
        }

        public void AddConnection(AdjacentableMock adj)
        {
            connections.Add(adj);
            adj.connections.Add(this);
        }

        public bool IsAdjacent(AdjacentableMock other)
        {
            return connections.Contains(other);
        }

        public override bool Equals(object obj)
        {
            if (obj is AdjacentableMock other)
            {
                return Id == other.Id;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}
