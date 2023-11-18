using UnityEngine;

namespace Components.Main.Grids.GridPathFinder
{
    public interface INavNode
    {
        bool WalkAble { get; }
        Vector3 WPos { get; }
        Vector2Int Coord { get; }
    }
}