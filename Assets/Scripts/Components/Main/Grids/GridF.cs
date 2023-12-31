﻿using System;
using System.Collections.Generic;
using Components.Main.Grids.GridPathFinder;
using Components.Main.Grids.TileItems;
using Datas.Levels;
using Extensions.System;
using UnityEngine;

namespace Components.Main.Grids
{
    public enum GridRot
    {
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3
    }

    public static class GridF
    {
        public static readonly int UpWorldAxisIndex = 0;
        public static readonly int RightWorldAxisIndex = 2;
        public static readonly Vector2Int GridUp = new(1, 0);
        public static readonly Vector2Int GridRight = new(0, -1);
        public static readonly Vector2Int GridDown = new(-1, 0);
        public static readonly Vector2Int GridLeft = new(0, 1);
        private static readonly Vector3 gridWorldRotUp = new(0f, 90f, 0f);
        private static readonly Vector3 gridWorldRotRight = new(0f, 0f, 0f);
        private static readonly Vector3 gridWorldRotDown = new(0, 270f, 0f);
        private static readonly Vector3 gridWorldRotLeft = new(0f, 180f, 0f);
        public static int GetRotAxisIndexSign(GridRot newGridRot)
        {
            return newGridRot switch
            {
                GridRot.Up => 1,
                GridRot.Right => -1,
                GridRot.Down => -1,
                GridRot.Left => 1,
                _ => throw new ArgumentOutOfRangeException(nameof(newGridRot), newGridRot, null)
            };
        }

        public static int GetRotAxisIndex(GridRot newGridRot)
        {
            return newGridRot switch
            {
                GridRot.Up => 0,
                GridRot.Right => 1,
                GridRot.Down => 0,
                GridRot.Left => 1,
                _ => throw new ArgumentOutOfRangeException(nameof(newGridRot), newGridRot, null)
            };
        }

        public static GridRot GetGridRot(int axisSign, int axisIndex)
        {
            switch (axisSign)
            {
                case > 0 when axisIndex == 0:
                    return GridRot.Up;
                case < 0 when axisIndex == 0:
                    return GridRot.Down;
                case > 0 when axisIndex == 1:
                    return GridRot.Right;
                case < 0 when axisIndex == 1:
                    return GridRot.Left;
            }

            throw new ArgumentOutOfRangeException();
        }
        public static void PutTileItemInGridData
        (
            TileItemData tileItemData,
            int tileItemSize,
            Vector2Int coord,
            TileData[,] grid
        )
        {
            bool canFitIgnore = CanFitTileItemDataIgnoreSelf
            (
                tileItemData,
                tileItemSize,
                coord,
                grid
            );

            int rotAxisIndexSign = GetRotAxisIndexSign(tileItemData.GridRot);
            int rotAxisIndex = GetRotAxisIndex(tileItemData.GridRot);

            for (int i = 0; i < tileItemSize; i ++)
            {
                Vector2Int vector2Int = Vector2Int.zero;
                vector2Int[rotAxisIndex] = i * rotAxisIndexSign;
                Vector2Int coordToOccupy = coord + vector2Int;

                if (GridContainsCoord(coordToOccupy, grid.Size2Vect()) == false)
                {
                    continue;
                }
                
                TileData tileData = grid[coordToOccupy.x, coordToOccupy.y];
                
                if (canFitIgnore == false)
                {
                    tileData.OverlappingTileItemData = tileItemData;
                }
                else
                {
                    tileData.SetTileItem(tileItemData);
                }
            }
        }
        
        public static void PutTileItemInGrid
        (TileItem tileItem, Tile[,] grid) =>
        PutTileItemInGrid
        (
            tileItem,
            tileItem.GridSize,
            tileItem.GridCoord,
            grid
        );

        public static void PutTileItemInGrid
        (
            TileItem tileItem,
            int tileItemSize,
            Vector2Int coord,
            Tile[,] grid
        )
        {
            int rotAxisIndexSign = GetRotAxisIndexSign(tileItem.GridRotation);
            int rotAxisIndex = GetRotAxisIndex(tileItem.GridRotation);

            for (int i = 0; i < tileItemSize; i ++)
            {
                Vector2Int vector2Int = Vector2Int.zero;
                vector2Int[rotAxisIndex] = i * rotAxisIndexSign;
                Vector2Int coordToOccupy = coord + vector2Int;
                
                IGridTile gridTile = grid[coordToOccupy.x, coordToOccupy.y];
                gridTile.SetTileItem(tileItem);
            }
        }

        public static Vector3 GetWorldRotY(GridRot gridRot)
        {
            return gridRot switch
            {
                GridRot.Up => gridWorldRotUp,
                GridRot.Right => gridWorldRotRight,
                GridRot.Down => gridWorldRotDown,
                GridRot.Left => gridWorldRotLeft,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static bool CanFitTileItemData
        (int tileItemSize, GridRot gridRot, Vector2Int coord, TileData[,] grid)
        {
            int rotAxisIndexSign = GetRotAxisIndexSign(gridRot);
            int rotAxisIndex = GetRotAxisIndex(gridRot);

            for (int i = 0; i < tileItemSize; i ++)
            {
                Vector2Int vector2Int = Vector2Int.zero;
                vector2Int[rotAxisIndex] = i * rotAxisIndexSign;
                Vector2Int coordToOccupy = coord + vector2Int;
                
                if (GridContainsCoord(coordToOccupy, grid.Size2Vect()) == false) return false;

                TileItemData tileItem = grid[coordToOccupy.x, coordToOccupy.y].TileItemData;

                if (tileItem != null)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool CanFitTileItemDataIgnoreSelf
        (
            TileItemData tileItemData,
            int tileItemSize,
            Vector2Int coord,
            TileData[,] grid
        )
        {
            int rotAxisIndexSign = GetRotAxisIndexSign(tileItemData.GridRot);
            int rotAxisIndex = GetRotAxisIndex(tileItemData.GridRot);

            for (int i = 0; i < tileItemSize; i ++)
            {
                Vector2Int vector2Int = Vector2Int.zero;
                vector2Int[rotAxisIndex] = i * rotAxisIndexSign;
                Vector2Int coordToOccupy = coord + vector2Int;
                
                if (GridContainsCoord(coordToOccupy, grid.Size2Vect()) == false) return false;

                TileItemData tileItemInCoord =
                grid[coordToOccupy.x, coordToOccupy.y].TileItemData;

                if (tileItemInCoord == tileItemData) continue;

                if (tileItemInCoord != null)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool GridContainsCoord(Vector2Int gridCoord, Vector2Int gridSize)
        {
            if (gridCoord.x < 0) return false;

            if (gridCoord.x >= gridSize.x) return false;

            if (gridCoord.y < 0) return false;

            if (gridCoord.y >= gridSize.y) return false;

            return true;
        }

        public static bool IsBorderTile(int rotAxisIndex, Vector2Int coord, Vector2Int gridSize)
        {
            if (rotAxisIndex == 0)
            {
                return coord.x == 0 || coord.x == gridSize.x - 1;
            }
            else
            {
                return coord.y == 0 || coord.y == gridSize.y - 1;
            }
        }

        public static Vector2Int GetSizeUnitPos(int axisVal, int tileItemSize , Vector2Int gridCoord, GridRot gridRot)
        {
            if (axisVal <= 0 || axisVal > tileItemSize)
            {
                throw new ArgumentOutOfRangeException();
            }
            
            int sizeIncludingPivot = axisVal - 1 - 1; //exclude pivot exclude index to length diff
            
            Vector2Int tileItemPivot = gridCoord;

            tileItemPivot[GetRotAxisIndex(gridRot)] += GetRotAxisIndexSign
            (gridRot) +
            sizeIncludingPivot;

            return tileItemPivot;
        }
        
        public static List<INavNode> GetBorderNavTiles(TileItem arg, Tile[,] runtimeGrid)
        {
            List<INavNode> borderNavTiles = new();

            int rotAxisIndex = GetRotAxisIndex(arg.GridRotation);
            int rotAxisSign = GetRotAxisIndexSign(arg.GridRotation);
            
            for (int i = 0; i < arg.GridSize; i ++)
            {
                Vector2Int borderCoordLength = arg.GridCoord;
                borderCoordLength[rotAxisIndex] += i * rotAxisSign;
                
                if (i == 0)
                {
                    borderCoordLength[rotAxisIndex] -= rotAxisSign;
                    if (GridContainsCoord(borderCoordLength, runtimeGrid.Size2Vect()))
                    {
                        borderNavTiles.Add(runtimeGrid[borderCoordLength.x, borderCoordLength.y]);
                    }
                }
                else if (i == arg.GridSize - 1)
                {
                    borderCoordLength[rotAxisIndex] += rotAxisSign;

                    if (GridContainsCoord(borderCoordLength, runtimeGrid.Size2Vect()))
                    {
                        borderNavTiles.Add(runtimeGrid[borderCoordLength.x, borderCoordLength.y]);
                    }
                }

                Vector2Int borderCoordWidth1 = arg.GridCoord;
                borderCoordWidth1[rotAxisIndex] += i * rotAxisSign;
                Vector2Int borderCoordWidth2 = arg.GridCoord;
                borderCoordWidth2[rotAxisIndex] += i * rotAxisSign;
                borderCoordWidth1[GetPerpAxisIndex2D(rotAxisIndex)] -= 1;
                borderCoordWidth2[GetPerpAxisIndex2D(rotAxisIndex)] += 1;
                
                if (GridContainsCoord(borderCoordWidth1, runtimeGrid.Size2Vect()))
                {
                    borderNavTiles.Add(runtimeGrid[borderCoordWidth1.x, borderCoordWidth1.y]);
                }

                if (GridContainsCoord(borderCoordWidth2, runtimeGrid.Size2Vect()))
                {
                    borderNavTiles.Add(runtimeGrid[borderCoordWidth2.x, borderCoordWidth2.y]);
                }
            }
            
            return borderNavTiles;
        }
        
        public static List<INavNode> GetSideBorderNavTiles(TileItem arg, Tile[,] runtimeGrid)
        {
            List<INavNode> borderNavTiles = new();

            int rotAxisIndex = GetRotAxisIndex(arg.GridRotation);
            int rotAxisSign = GetRotAxisIndexSign(arg.GridRotation);
            
            for (int i = 0; i < arg.GridSize; i ++)
            {
                Vector2Int borderCoordLength = arg.GridCoord;
                borderCoordLength[rotAxisIndex] += i * rotAxisSign;

                Vector2Int borderCoordWidth1 = arg.GridCoord;
                borderCoordWidth1[rotAxisIndex] += i * rotAxisSign;
                Vector2Int borderCoordWidth2 = arg.GridCoord;
                borderCoordWidth2[rotAxisIndex] += i * rotAxisSign;
                borderCoordWidth1[GetPerpAxisIndex2D(rotAxisIndex)] -= 1;
                borderCoordWidth2[GetPerpAxisIndex2D(rotAxisIndex)] += 1;
                
                if (GridContainsCoord(borderCoordWidth1, runtimeGrid.Size2Vect()))
                {
                    borderNavTiles.Add(runtimeGrid[borderCoordWidth1.x, borderCoordWidth1.y]);
                }

                if (GridContainsCoord(borderCoordWidth2, runtimeGrid.Size2Vect()))
                {
                    borderNavTiles.Add(runtimeGrid[borderCoordWidth2.x, borderCoordWidth2.y]);
                }
            }
            
            return borderNavTiles;
        }
        
        public static Vector2Int GetRotatedSize
        (TileItem tileItem) =>
        GetRotatedSize(tileItem.GridRotation, tileItem.GridSize);
        
        public static Vector2Int GetRotatedSize(GridRot gridRot, int tileItemSize)
        {
            Vector2Int rotatedSize = Vector2Int.zero;
            rotatedSize[GetRotAxisIndex(gridRot)] = tileItemSize * GetRotAxisIndexSign(gridRot);

            return rotatedSize;
        }

        public static int DistToPivotOnSizeAtFromDir(int tileItemSize, GridRot gridRot, int fromDirAxisSign, int fromDirAxisIndex)
        {
            Vector2Int rotatedSize = GetRotatedSize(gridRot, tileItemSize);

            int axisSize = rotatedSize[fromDirAxisIndex];

            axisSize = axisSize.Sign() * (Mathf.Abs(axisSize) - 1);

            return fromDirAxisSign.Sign() != axisSize.Sign() ? Mathf.Abs(axisSize) : 0;
        }

        public static int GetPerpAxisIndex2D(int axisIndex)
        {
            return Mathf.Abs(axisIndex - 1);
        }

        public static Vector3 ToWorldPos(int x, int y, int[] sizes, float tileSize)
        {
            return new Vector3(x * tileSize, 0, (sizes[1] - 1 - y) * tileSize);
        }
    }
}