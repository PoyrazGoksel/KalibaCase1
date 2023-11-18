using System.Collections.Generic;
using Components.Main.Grids.GridPathFinder;
using Components.Main.Grids.TileItems;
using Datas.Levels;
using Events.External;
using Events.Internal;
using Extensions.System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace Components.Main.Grids
{
    public partial class Grid : SerializedMonoBehaviour
    {
        public enum Corner
        {
            NULL = -1,
            UpperLeft = 0,
            UpperRight = 1,
            LowerLeft = 2,
            LowerRight = 3
        }

        private const int RoadToOutLength = 10;
        [Inject] private GridEvents GridEvents { get; set; }
        [Inject] private GridInternalEvents GridInternalEvents { get; set; }
        private readonly List<GameObject> _roadsRuntime = new();
        [InlineEditor][OdinSerialize] private LevelData _levelData;
        [ShowIf("@false")][OdinSerialize] private Transform _myTrans;
        [ShowIf("@false")][OdinSerialize] private GameObject _roadCont;
#if UNITY_EDITOR
        [TableMatrix
        (
            DrawElementMethod = nameof(DebugRuntimeGrid),
            SquareCells = true,
            ResizableColumns = false
        )]
        [OdinSerialize]
#endif
        private Tile[,] _runtimeGrid;
        private TileData[,] _tileData;
        [ShowIf("@false")][OdinSerialize] private GameObject _tilePrefab;
        [ShowIf("@false")][OdinSerialize] private GameObject road;
        [ShowIf("@false")][OdinSerialize] private GameObject roadCorner;
        [ShowIf("@false")][OdinSerialize] private GameObject roadT;
        [ShowIf("@false")]
        [ReadOnly]
        [OdinSerialize]
        private Dictionary<Corner, Vector3> _roadRots = new();

        private Tile DebugRuntimeGrid(Rect rect, Tile tile)
        {
            if (tile.TileItem)
            {
                EditorGUI.LabelField
                (
                    rect,
                    tile.TileItem.GetType()
                    .Name
                );
            }

            return tile;
        }

        private void OnEnable()
        {
            RegisterEvents();
        }

        private void OnDisable()
        {
            UnRegisterEvents();
        }

        private void CreateGridCopyPlayMode()
        {
            int[] sizes = _tileData.GetSize();

            float tileSize = _tilePrefab.GetComponent<Tile>()
            .MeshRenderer.bounds.size.x;

            _runtimeGrid = new Tile[sizes[0], sizes[1]];
            List<TileItemData> tileItemsToGenerate = new();

            for (int x = 0; x < sizes[0]; x ++)
            for (int y = 0; y < sizes[1]; y ++)
            {
                TileData tileData = _tileData[x, y];
                Vector3 tilePosition = new(x * tileSize, 0, (sizes[1] - 1 - y) * tileSize);

                GameObject newTileGo = GridInternalEvents.InstantiatePrefab?.Invoke
                (tileData.Prefab);

                Tile newTile = newTileGo.GetComponent<Tile>();
                newTile.Construct(new Vector2Int(x, y));
                _runtimeGrid[x, y] = newTile;
                Transform newTileTrans = newTileGo.transform;
                newTileTrans.parent = _myTrans;
                newTileTrans.localPosition = tilePosition;

                if (tileData.TileItemData?.Prefab != null &&
                    tileData.TileItemData?.IsPivot(x, y) == true)
                {
                    tileItemsToGenerate.Add(tileData.TileItemData);
                }
            }

            foreach (TileItemData tileItemData in tileItemsToGenerate)
            {
                GameObject newTileItemGo = GridInternalEvents.InstantiatePrefab?.Invoke
                (tileItemData.Prefab);

                TileItem newTileItem = newTileItemGo.GetComponent<TileItem>();

                IGridTileItem gridTileItem = newTileItem;

                gridTileItem.Construct
                (
                    _runtimeGrid[tileItemData.Coord.x, tileItemData.Coord.y].TransformEncapsulated,
                    tileItemData.Coord,
                    tileItemData.GridRot,
                    tileItemData.Size
                );

                GridF.PutTileItemInGrid
                (
                    newTileItem,
                    newTileItem.GridSize,
                    tileItemData.Coord,
                    _runtimeGrid
                );
            }

            CreateRoads(tileSize, GridInternalEvents);

            GridEvents.GridStarted?.Invoke(_runtimeGrid);
        }

        private void CreateRoads(float tileSize, GridInternalEvents gie)
        {
            int[] gSize = _runtimeGrid.GetSize();

            _roadCont = new GameObject
            ("RoadContainer") {transform = {position = Vector3.zero + 0.1f * Vector3.up}};

            _roadCont.transform.SetParent(_myTrans);

            for (int x = -1; x < gSize[0] + 1; x ++)
            {
                if (x == -1)
                {
                    CreateRoad
                    (
                        x,
                        -1,
                        tileSize,
                        roadCorner,
                        gie,
                        Corner.LowerRight
                    );

                    CreateRoad
                    (
                        x,
                        gSize[1],
                        tileSize,
                        roadCorner,
                        gie,
                        Corner.LowerLeft
                    );
                }
                else if (x == gSize[0])
                {
                    CreateRoad
                    (
                        x,
                        -1,
                        tileSize,
                        roadCorner,
                        gie,
                        Corner.UpperRight
                    );

                    CreateRoad
                    (
                        x,
                        gSize[1],
                        tileSize,
                        roadT,
                        gie,
                        Corner.UpperRight
                    );

                    for (int x1 = 1; x1 < RoadToOutLength; x1 ++)
                    {
                        CreateRoad
                        (
                            x + x1,
                            gSize[1],
                            tileSize,
                            road,
                            gie,
                            Corner.UpperRight
                        );
                    }
                }
                else
                {
                    CreateRoad
                    (
                        x,
                        -1,
                        tileSize,
                        road,
                        gie,
                        Corner.UpperRight
                    );

                    CreateRoad
                    (
                        x,
                        gSize[1],
                        tileSize,
                        road,
                        gie,
                        Corner.UpperRight
                    );
                }
            }

            for (int y = 0; y < gSize[1]; y ++)
            {
                CreateRoad
                (
                    -1,
                    y,
                    tileSize,
                    road,
                    gie
                );

                CreateRoad
                (
                    gSize[0],
                    y,
                    tileSize,
                    road,
                    gie
                );
            }
        }

        private void CreateRoad
        (
            int x,
            int y,
            float tileSize,
            GameObject prefab,
            GridInternalEvents gie,
            Corner corner = Corner.NULL
        )
        {
            GameObject newRoad = gie.InstantiatePrefab(prefab);
            Vector3 tilePosition = new(x * tileSize, 0, y * tileSize);
            newRoad.transform.position = tilePosition;

            if (corner != Corner.NULL)
            {
                newRoad.transform.eulerAngles = _roadRots[corner];
            }

            newRoad.transform.SetParent(_roadCont.transform);
            _roadsRuntime.Add(newRoad);
        }

        private INavNode GetLastTileAtDir
        (
            Vector2Int firstFwdTileCoord,
            int tileRotAxisIndex,
            int gridAxisSize,
            int tileSize,
            GridRot gridRot,
            int dirAxisSign
        )
        {
            int axisEnd = dirAxisSign >= 0 ? gridAxisSize : -1;

            Vector2Int lastTileCoord = firstFwdTileCoord;
            Vector2Int nextTileCoord = firstFwdTileCoord;

            for (int i = firstFwdTileCoord[tileRotAxisIndex]; i != axisEnd; i += dirAxisSign)
            {
                nextTileCoord[tileRotAxisIndex] = i;

                if (_runtimeGrid[nextTileCoord.x, nextTileCoord.y].TileItem == null)
                {
                    lastTileCoord = nextTileCoord;
                }
                else
                {
                    break;
                }
            }
            
            int distToPivotOnSizeAtFromDir = GridF.DistToPivotOnSizeAtFromDir
            (tileSize, gridRot, dirAxisSign * -1, tileRotAxisIndex);

            lastTileCoord[tileRotAxisIndex] += distToPivotOnSizeAtFromDir * dirAxisSign * -1;
            INavNode navTileToGo = _runtimeGrid[lastTileCoord.x, lastTileCoord.y];

            return navTileToGo;
        }

        private void RemoveItemFromGrid(TileItem arg0)
        {
            int rotAxisIndex = GridF.GetRotAxisIndex(arg0.GridRotation);
            int rotSign = GridF.GetRotAxisIndexSign(arg0.GridRotation);

            for (int i = 0; i < arg0.GridSize; i ++)
            {
                Vector2Int vector2Int = Vector2Int.zero;
                vector2Int[rotAxisIndex] = i * rotSign;
                Vector2Int coordToOccupy = arg0.GridCoord + vector2Int;

                IGridTile gridTile = _runtimeGrid[coordToOccupy.x, coordToOccupy.y];
                gridTile.SetTileItem(null);
            }
        }

        private void RegisterEvents()
        {
            GridEvents.LoadGrid += OnLoadGrid;
            GridEvents.GetCarPath += GetCarPath;
            GridEvents.TileItemMoveStart += OnTileItemMoveStart;
            GridEvents.TileItemMoveEnd += OnTileItemMoveEnd;
        }

        private void OnTileItemMoveEnd(TileItem arg0)
        {
            GridF.PutTileItemInGrid(arg0, _runtimeGrid);
        }

        private void OnTileItemMoveStart(TileItem arg0)
        {
            RemoveItemFromGrid(arg0);
        }

        private GridEvents.CarPathResult GetCarPath(GridEvents.TileItemTrans tileItemTrans)
        {
            int rotAxisIndex = GridF.GetRotAxisIndex(tileItemTrans.GridRotation);
            int rotAxisSign = GridF.GetRotAxisIndexSign(tileItemTrans.GridRotation);

            Vector2Int firstFwdTileCoord = GridF.GetSizeUnitPos
            (
                tileItemTrans.Size,
                tileItemTrans.Size,
                tileItemTrans.GridCoord,
                tileItemTrans.GridRotation
            );

            firstFwdTileCoord[rotAxisIndex] += rotAxisSign;
            Vector2Int firstBwdTileCoord = tileItemTrans.GridCoord;
            firstBwdTileCoord[rotAxisIndex] -= rotAxisSign;

            if (GridF.GridContainsCoord
                (firstFwdTileCoord, _runtimeGrid.Size2Vect()) &&
                _runtimeGrid[firstFwdTileCoord.x, firstFwdTileCoord.y].TileItem == null)
            {
                int dirAxisSign = rotAxisSign; 

                INavNode destNode = GetLastTileAtDir
                (
                    firstFwdTileCoord,
                    rotAxisIndex,
                    _runtimeGrid.GetSize()[rotAxisIndex],
                    tileItemTrans.Size,
                    tileItemTrans.GridRotation,
                    dirAxisSign
                );

                bool canLeave = GridF.IsBorderTile(destNode.Coord, _runtimeGrid.Size2Vect());

                return new GridEvents.CarPathResult(destNode, false, canLeave);
            }

            if (GridF.GridContainsCoord
                (firstBwdTileCoord, _runtimeGrid.Size2Vect()) &&
                _runtimeGrid[firstBwdTileCoord.x, firstBwdTileCoord.y].TileItem == null)
            {
                int dirAxisSign = rotAxisSign * -1;
                
                INavNode destNode = GetLastTileAtDir
                (
                    firstBwdTileCoord,
                    rotAxisIndex,
                    _runtimeGrid.GetSize()[rotAxisIndex],
                    tileItemTrans.Size,
                    tileItemTrans.GridRotation,
                    dirAxisSign
                );

                bool canLeave = GridF.IsBorderTile(destNode.Coord, _runtimeGrid.Size2Vect());

                return new GridEvents.CarPathResult(destNode, false, canLeave);
            }

            return new GridEvents.CarPathResult(true);
        }

        private void OnLoadGrid(TileData[,] arg0)
        {
            _tileData = arg0;
            CreateGridCopyPlayMode();
        }

        private void UnRegisterEvents()
        {
            GridEvents.LoadGrid -= OnLoadGrid;
            GridEvents.GetCarPath -= GetCarPath;
            GridEvents.TileItemMoveStart -= OnTileItemMoveStart;
            GridEvents.TileItemMoveEnd -= OnTileItemMoveEnd;
        }
    }
}