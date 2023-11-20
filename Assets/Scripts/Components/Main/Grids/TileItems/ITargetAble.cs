using System.Collections.Generic;
using Components.Main.Grids.GridPathFinder;
using UnityEngine;

namespace Components.Main.Grids.TileItems
{
    public interface ITargetAble
    {
        Vector2Int Target();
    }
}