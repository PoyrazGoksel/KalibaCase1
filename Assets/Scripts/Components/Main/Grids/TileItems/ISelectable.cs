using System.Collections.Generic;
using Components.Main.Grids.GridPathFinder;
using Components.Main.Grids.TileItems.Cars;
using UnityEngine;

namespace Components.Main.Grids.TileItems
{
    public interface ISelectable
    {
        void Select();

        void DeSelect();

        void SetDest(Vector2Int targetAbleGridCoord);

        void SetTarget(List<INavNode> borderTiles, TileItemColor tileItemColor, List<ICarDoor> doorTransList);
    }
}