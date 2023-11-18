using System;
using System.Collections.Generic;
using Extensions.System;
using Sirenix.Serialization;
using UnityEngine;

namespace Components.Main.Grids.GridPathFinder
{
    [Serializable]
    public class PathFinder
    {
        [OdinSerialize] private NavigationPlane _navigationPlane;
        private HeapNode[,] _pathHeap;

        public void GenerateEmptyGrid(int sizeX, int sizeY)
        {
            _navigationPlane = new NavigationPlane(sizeX, sizeY);
        }

        private void CreatePathHeap()
        {
            int[] size = _navigationPlane.NavPlane.GetSize();
            _pathHeap = new HeapNode[size[0], size[1]];

            for (int x = 0; x < _navigationPlane.NavPlane.GetLength(0); x ++)
            for (int y = 0; y < _navigationPlane.NavPlane.GetLength(1); y ++)
            {
                NavNode navNode = _navigationPlane.NavPlane[x, y];
                _pathHeap[x, y] = new HeapNode(navNode);
            }
        }

        public void AddNavNode(INavNode navNode)
        {
            _navigationPlane.Add(navNode);
        }

        public List<(Vector3, Vector2Int)> FindPath(Vector2Int startCoord, Vector2Int endCoord)
        {
            CreatePathHeap();

            HeapNode startNode = _pathHeap[startCoord.x, startCoord.y];
            HeapNode targetNode = _pathHeap[endCoord.x, endCoord.y];
            PriorityQueue<HeapNode> openSet = new();
            HashSet<HeapNode> closedSet = new();
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                HeapNode currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    return RetracePath(startNode, targetNode);
                }

                foreach (HeapNode neighbour in GetNeighbours(currentNode, _pathHeap))
                {
                    if (! neighbour.WalkAble || closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    int newMovementCostToNeighbour =
                    currentNode.GCost + GetDistance(currentNode, neighbour);

                    if (newMovementCostToNeighbour < neighbour.GCost ||
                        ! openSet.Contains(neighbour))
                    {
                        neighbour.GCost = newMovementCostToNeighbour;
                        neighbour.HCost = GetDistance(neighbour, targetNode);
                        neighbour.Parent = currentNode;

                        if (! openSet.Contains(neighbour))
                        {
                            openSet.Add(neighbour);
                        }
                        else
                        {
                            openSet.UpdateItem(neighbour);
                        }
                    }
                }
            }

            return null;
        }

        private List<(Vector3, Vector2Int)> RetracePath(HeapNode startNode, HeapNode endNode)
        {
            List<(Vector3, Vector2Int)> path = new();
            HeapNode currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add((currentNode.NodeWorldPos, currentNode.Coord));
                currentNode = currentNode.Parent;
            }

            path.Reverse();

            return path;
        }

        private List<HeapNode> GetNeighbours(HeapNode node, HeapNode[,] grid)
        {
            List<HeapNode> neighbours = new();

            for (int x = -1; x <= 1; x ++)
            {
                for (int y = -1; y <= 1; y ++)
                {
                    if (x == 0 && y == 0) continue;
                    if (x != 0 && y != 0) continue;

                    int checkX = node.GridX + x;
                    int checkY = node.GridY + y;

                    if (checkX >= 0 &&
                        checkX < grid.GetLength(0) &&
                        checkY >= 0 &&
                        checkY < grid.GetLength(1))
                    {
                        neighbours.Add(grid[checkX, checkY]);
                    }
                }
            }

            return neighbours;
        }

        private int GetDistance(HeapNode nodeA, HeapNode nodeB)
        {
            int distX = Mathf.Abs(nodeA.GridX - nodeB.GridX);
            int distY = Mathf.Abs(nodeA.GridY - nodeB.GridY);

            return distX + distY;
        }

        public class HeapNode : IHeapItem<HeapNode>
        {
            public readonly int GridX;
            public readonly int GridY;
            public readonly Vector3 NodeWorldPos;
            public readonly bool WalkAble;
            public HeapNode Parent { get; set; }
            public int GCost { get; set; }
            public int HCost { get; set; }
            public int HeapIndex { get; set; }
            public int FCost => GCost + HCost;
            public Vector2Int Coord => new(GridX, GridY);

            public HeapNode(NavNode node)
            {
                GridX = node.X;
                GridY = node.Y;
                WalkAble = node.Walkable;
                NodeWorldPos = node.WPos;
            }

            public int CompareTo(HeapNode other)
            {
                int compare = FCost.CompareTo(other.FCost);

                if (compare == 0)
                {
                    compare = HCost.CompareTo(other.HCost);
                }

                return -compare;
            }
        }
    }
}