using System.Collections.Generic;
using Components.Main.Grids.GridPathFinder;
using Components.Main.Grids.TileItems;
using Components.Main.Grids.TileItems.Cars;
using Datas.Levels;
using Events.External;
using Events.Internal;
using Extensions.System;
using Extensions.Unity;
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
        [Inject] private GameStateEvents GameStateEvents { get; set; }
        private readonly List<GameObject> _roadsRuntime = new();
        private int _carCount;
        [ShowInInspector] private Dictionary<Corner, Transform> _cornerRoads;
        [InlineEditor][OdinSerialize] private LevelData _levelData;
        [ShowIf("@false")][OdinSerialize] private Transform _myTrans;
        [ShowIf("@false")][OdinSerialize] private GameObject _roadCont;
        [ShowIf("@false")]
        [ReadOnly]
        [OdinSerialize]
        private Dictionary<Corner, Vector3> _roadRots = new();
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
        private float _tileSize;
        [ShowIf("@false")][OdinSerialize] private GameObject road;
        [ShowIf("@false")][OdinSerialize] private GameObject roadCorner;
        [ShowIf("@false")][OdinSerialize] private GameObject roadT;

        private void OnEnable()
        {
            RegisterEvents();
        }

        private void OnDisable()
        {
            UnRegisterEvents();
        }

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

        private void CreateGridCopyPlayMode()
        {
            int[] sizes = _tileData.GetSize();

            _tileSize = _tilePrefab.GetComponent<Tile>()
            .MeshRenderer.bounds.size.x;

            _runtimeGrid = new Tile[sizes[0], sizes[1]];
            List<TileItemData> tileItemsToGenerate = new();

            for (int x = 0; x < sizes[0]; x ++)
            for (int y = 0; y < sizes[1]; y ++)
            {
                TileData tileData = _tileData[x, y];
                Vector3 tilePosition = GridF.ToWorldPos(x, y, sizes, _tileSize);

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

                if (newTileItem.GetType() == typeof(Car))
                {
                    _carCount ++;
                }

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

            CreateRoads(_tileSize, GridInternalEvents);

            GridEvents.GridStarted?.Invoke(_runtimeGrid);
        }

        private void CreateRoads(float tileSize, GridInternalEvents gie)
        {
            int[] gSize = _runtimeGrid.GetSize();

            _roadCont = new GameObject
            ("RoadContainer") {transform = {position = Vector3.zero + 0.1f * Vector3.up}};

            _roadCont.transform.SetParent(_myTrans);

            _cornerRoads = new Dictionary<Corner, Transform>();

            for (int x = -1; x < gSize[0] + 1; x ++)
            {
                Transform cornerRoadTrans;

                if (x == -1)
                {
                    cornerRoadTrans = CreateRoad
                    (
                        x,
                        -1,
                        tileSize,
                        roadCorner,
                        gie,
                        Corner.LowerRight
                    );

                    _cornerRoads.Add(Corner.LowerRight, cornerRoadTrans);
                    
                    cornerRoadTrans = CreateRoad
                    (
                        x,
                        gSize[1],
                        tileSize,
                        roadCorner,
                        gie,
                        Corner.LowerLeft
                    );
                    
                    _cornerRoads.Add(Corner.LowerLeft, cornerRoadTrans);
                }
                else if (x == gSize[0])
                {
                    cornerRoadTrans = CreateRoad
                    (
                        x,
                        -1,
                        tileSize,
                        roadCorner,
                        gie,
                        Corner.UpperRight
                    );
                    _cornerRoads.Add(Corner.UpperRight, cornerRoadTrans);
                    cornerRoadTrans = CreateRoad
                    (
                        x,
                        gSize[1],
                        tileSize,
                        roadT,
                        gie,
                        Corner.UpperRight
                    );
                    _cornerRoads.Add(Corner.UpperLeft, cornerRoadTrans);
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

        private Transform CreateRoad
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

            return newRoad.transform;
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
            GridEvents.GetBorderNavTiles += OnGetBorderNavTiles;
            GridEvents.TileItemRemove += OnTileItemRemove;
            GridEvents.CarLeftGrid += OnCarLeftGrid;
        }

        private void OnCarLeftGrid()
        {
            _carCount --;

            if (_carCount == 0)
            {
                GameStateEvents.LevelSuccess?.Invoke();
            }
        }

        private void OnTileItemRemove(TileItem arg0)
        {
            RemoveItemFromGrid(arg0);
        }

        private List<INavNode> OnGetBorderNavTiles(TileItem arg)
        {
            return GridF.GetSideBorderNavTiles(arg, _runtimeGrid);
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

                Vector2Int endCoord = GridF.GetSizeUnitPos
                (
                    tileItemTrans.Size,
                    tileItemTrans.Size,
                    destNode.Coord,
                    tileItemTrans.GridRotation
                );

                bool canLeave = GridF.IsBorderTile(rotAxisIndex ,endCoord, _runtimeGrid.Size2Vect());
                List<Vector3> path = new();

                if (canLeave)
                {
                    GetExitPath(endCoord, path);
                }

                return new GridEvents.CarPathResult(canLeave, path, destNode, false);
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
                
                return new GridEvents.CarPathResult(false, null, destNode, false);
            }

            return new GridEvents.CarPathResult(true);
        }

        private void GetExitPath(Vector2Int endCoord, List<Vector3> path)
        {
            Vector3 firstPos = GridF.ToWorldPos(endCoord.x, endCoord.y, _runtimeGrid.GetSize(), _tileSize);
            if (endCoord.x == 0)
            {
                firstPos.x -= _tileSize;
                path.Add(firstPos);
                path.Add(_cornerRoads[Corner.LowerLeft].position);
                path.Add(_cornerRoads[Corner.UpperLeft].position);
            }
            else if (endCoord.x ==
                _runtimeGrid.Size2Vect()
                .x -
                1)
            {
                firstPos.x += _tileSize;
                path.Add(firstPos);
                path.Add(_cornerRoads[Corner.UpperLeft].position);

            }
            else if (endCoord.y == 0)
            {
                firstPos.z += _tileSize;
                path.Add(firstPos);
                path.Add(_cornerRoads[Corner.UpperLeft].position);

            }
            else if (endCoord.y ==
                _runtimeGrid.Size2Vect()
                .y -
                1)
            {
                firstPos.z += _tileSize;
                path.Add(firstPos);
                path.Add(_cornerRoads[Corner.UpperRight].position);
                path.Add(_cornerRoads[Corner.UpperLeft].position);
            }
            else
            {
                Debug.LogWarning(endCoord);
            }
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
            GridEvents.GetBorderNavTiles -= OnGetBorderNavTiles;
            GridEvents.TileItemRemove -= OnTileItemRemove;
            GridEvents.CarLeftGrid -= OnCarLeftGrid;
        }
    }
}