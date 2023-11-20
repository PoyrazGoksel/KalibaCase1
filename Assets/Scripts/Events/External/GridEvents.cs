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
        public readonly struct CarPathResult
        {
            public readonly INavNode NavNode;
            public readonly bool IsBackwards;
            public readonly bool NoPath;
            public readonly bool CanLeaveGrid;

            public CarPathResult(INavNode navNode, bool isBackwards, bool canLeaveGrid)
            {
                NavNode = navNode;
                IsBackwards = isBackwards;
                NoPath = default;
                CanLeaveGrid = canLeaveGrid;
            }

            public CarPathResult(bool noPath)
            {
                NoPath = true;
                NavNode = null;
                IsBackwards = false;
                CanLeaveGrid = false;
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