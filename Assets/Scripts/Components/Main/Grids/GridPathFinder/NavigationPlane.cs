using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Components.Main.Grids.GridPathFinder
{
    [Serializable]
    public class NavigationPlane
    {
        public NavNode[,] NavPlane => _navPlane;
        [TableMatrix(DrawElementMethod = nameof(DrawNavPlane), SquareCells = true)]
        [OdinSerialize]
        private NavNode[,] _navPlane;

        public NavigationPlane(int sizeX, int sizeY)
        {
            _navPlane = new NavNode[sizeX, sizeY];
        }

        private NavNode DrawNavPlane(Rect rect, NavNode node)
        {
            EditorGUI.LabelField(rect.Padding(0, -5), $"{node.X}, {node.Y}");
            EditorGUI.LabelField(rect.Padding(0, 5), $"Walk?{node.Walkable}");
            return node;
        }

        public void Add
        (
            INavNode navNode
        )
        {
            _navPlane[navNode.Coord.x, navNode.Coord.y] = new NavNode(navNode);
        }
    }
}