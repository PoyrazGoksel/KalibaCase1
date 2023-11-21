using System;
using System.Collections.Generic;
using Components.Main.Grids;
using Components.Main.Grids.GridPathFinder;
using Components.Main.Grids.TileItems;
using Datas.Levels;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

namespace Events.External
{
    [UsedImplicitly]
    public class GridEvents
    {
        public UnityAction<INavNode[,]> GridStarted;
        public UnityAction<TileData[,]> LoadGrid;
        public Func<TileItemTrans, CarPathResult> GetCarPath;
        public UnityAction<TileItem> TileItemMoveStart;
        public UnityAction<TileItem> TileItemMoveEnd;
        public UnityAction<TileItem> TileItemRemove;
        public Func<TileItem, List<INavNode>> GetBorderNavTiles;
        public UnityAction CarLeftGrid;
        public readonly struct CarPathResult
        {
            public readonly INavNode NavNode;
            public readonly bool IsBackwards;
            public readonly bool NoPath;
            public readonly bool CanLeaveGrid;
            public readonly List<Vector3> LeavePath;

            public CarPathResult(bool canLeaveGrid,List<Vector3> leavePath, INavNode navNode, bool isBackwards)
            {
                NavNode = navNode;
                IsBackwards = isBackwards;
                NoPath = default;
                CanLeaveGrid = canLeaveGrid;
                LeavePath = leavePath;
            }

            public CarPathResult(bool noPath)
            {
                NoPath = true;
                NavNode = default;
                IsBackwards = default;
                CanLeaveGrid = default;
                LeavePath = default;
            }
        }
        public readonly struct TileItemTrans
        {
            public readonly Vector2Int GridCoord;
            public readonly GridRot GridRotation;
            public readonly int Size;

            public TileItemTrans(TileItem tileItem)
            {
                GridCoord = tileItem.GridCoord;
                GridRotation = tileItem.GridRotation;
                Size = tileItem.GridSize;
            }
        }
    }
}