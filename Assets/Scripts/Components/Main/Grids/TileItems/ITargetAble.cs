using UnityEngine;

namespace Components.Main.Grids.TileItems
{
    public interface ITargetAble
    {
        void Target();

        Vector2Int GridCoord { get; }
    }
}