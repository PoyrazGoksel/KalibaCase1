using System.Collections.Generic;
using Components.Main.Grids.GridPathFinder;
using UnityEngine;

namespace Components.Main.Grids.TileItems.Cars
{
    public interface ITargetAbleCar
    {
        List<INavNode> Target();
        TileItemColor TileItemColor { get; }

        List<ICarDoor> GetDoors();

        void SetFull();

        bool IsFull { get; }
    }
}