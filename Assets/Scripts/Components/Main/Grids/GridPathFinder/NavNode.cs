using System;
using UnityEngine;

namespace Components.Main.Grids.GridPathFinder
{
    [Serializable]
    public class NavNode
    {
        public int Y => _navNode.Coord.y;
        public int X => _navNode.Coord.x;
        public bool Walkable => _navNode.WalkAble;
        public Vector3 WPos => _navNode.WPos;
        private INavNode _navNode;

        public NavNode
        (INavNode navNode)
        {
            _navNode = navNode;
        }
    }
}