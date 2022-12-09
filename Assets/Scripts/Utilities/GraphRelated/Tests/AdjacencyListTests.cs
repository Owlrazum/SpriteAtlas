using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Unity.Mathematics;

namespace Orazum.Graphs.Tests
{
    class AdjacencyListTests
    {
        [Test]
        public void SimplePairTest()
        {
            AdjacencyList<AdjacentableMock> list = new(2, 1);
            List<AdjacentableMock> adjs = FillSimplePair();

            int n1 = 0;
            list.AddNode(adjs[0]);
            int n2 = 1;
            list.AddNode(adjs[1]);

            List<int> adj = list.GetAdjacentNodes(n1);
            Assert.IsTrue(adj.Count == 1 && adj[0] == n2);
            adj = list.GetAdjacentNodes(n2);
            Assert.IsTrue(adj.Count == 1 && adj[0] == n1);
        }

        [Test]
        public void PetersonGraphTest()
        {
            AdjacencyList<AdjacentableMock> list = new(10, 3);
            List<AdjacentableMock> nodes = FillPetersonGraph();

            for (int i = 0; i < nodes.Count; i++)
            {
                list.AddNode(nodes[i]);
            }

            var checkList = GetPetersonGraphCheckList();
            CheckWithChecklist(list, checkList);

            list.RemoveNode(0);
            list.RemoveNode(3); // After failed test, changed list to dictionary in AdjacencyList
            checkList = new List<List<int>>
            {
                new List<int>(0),
                new List<int> { 2, 6},
                new List<int> { 1, 7},
                new List<int>(0),
                new List<int> { 9},

                new List<int> { 7, 8},
                new List<int> { 8, 9, 1},
                new List<int> { 9, 5, 2},
                new List<int> { 5, 6},
                new List<int> { 6, 7, 4}
            };

            CheckWithChecklist(list, checkList);
        }

        List<AdjacentableMock> FillSimplePair()
        {
            List<AdjacentableMock> adjs = new List<AdjacentableMock>(2);
            adjs.Add(new AdjacentableMock(0, 1));
            adjs.Add(new AdjacentableMock(1, 1));

            adjs[0].AddConnection(adjs[1]);
            return adjs;
        }

        List<List<int>> GetPetersonGraphCheckList()
        {
            List<List<int>> checkList = new List<List<int>>
            {
                new List<int> { 1, 4, 5},
                new List<int> { 2, 0, 6},
                new List<int> { 3, 1, 7},
                new List<int> { 4, 2, 8},
                new List<int> { 0, 3, 9},

                new List<int> { 7, 8, 0},
                new List<int> { 8, 9, 1},
                new List<int> { 9, 5, 2},
                new List<int> { 5, 6, 3},
                new List<int> { 6, 7, 4}
            };

            return checkList;
        }

        void CheckWithChecklist(AdjacencyList<AdjacentableMock> list, List<List<int>> checkList)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list.HasNode(i))
                { 
                    var adj = list.GetAdjacentNodes(i);
                    Assert.IsTrue(adj.Count == checkList[i].Count);
                    for (int j = 0; j < checkList[i].Count; j++)
                    {
                        Assert.IsTrue(checkList[i].Contains(adj[j]));
                    }
                }
            }
        }

        List<AdjacentableMock> FillPetersonGraph()
        {
            List<AdjacentableMock> adjs = new List<AdjacentableMock>(10);
            for (int i = 0; i < 10; i++)
            {
                adjs.Add(new AdjacentableMock(i, 3));
            }

            // 1 2 3 4 5 outer circle, 6 7 8 9 10 inner circle
            adjs[0].AddConnection(adjs[1]);
            adjs[1].AddConnection(adjs[2]);
            adjs[2].AddConnection(adjs[3]);
            adjs[3].AddConnection(adjs[4]);
            adjs[4].AddConnection(adjs[0]);

            adjs[0].AddConnection(adjs[5]);
            adjs[1].AddConnection(adjs[6]);
            adjs[2].AddConnection(adjs[7]);
            adjs[3].AddConnection(adjs[8]);
            adjs[4].AddConnection(adjs[9]);

            adjs[5].AddConnection(adjs[7]);
            adjs[7].AddConnection(adjs[9]);
            adjs[9].AddConnection(adjs[6]);
            adjs[6].AddConnection(adjs[8]);
            adjs[8].AddConnection(adjs[5]);

            return adjs;
        }
    }

}
