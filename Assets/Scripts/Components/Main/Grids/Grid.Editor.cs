#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Components.Main.Grids.TileItems;
using Datas.Levels;
using Extensions.System;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Utils;

namespace Components.Main.Grids
{
    public partial class Grid
    {
        [Button]
        private void NewLevel()
        {
            ResetEditor();
            _levelData = ScriptableObject.CreateInstance<LevelData>();

            string newLevelPath = EnvironmentVariables.GetProjectPath
            (EnvironmentVariables.LevelsPath);

            string[] assetsAtPath = AssetDatabase.FindAssets
            (
                "",
                new[]
                {
                    newLevelPath
                }
            );

            List<DirectoryInfo> assetDirs = assetsAtPath.Select
            (e => new DirectoryInfo(Application.dataPath + AssetDatabase.GUIDToAssetPath(e)))
            .ToList();

            int nextDirNum = 0;

            List<int> levelCounts = assetDirs.Where
            (
                e => e.Name.Contains
                (EnvironmentVariables.LevelSoPrefix) &&
                e.Name.Contains(EnvironmentVariables.SoSuffix)
            )
            .Select
            (
                delegate(DirectoryInfo info, int i)
                {
                    string num = info.Name.Replace(EnvironmentVariables.LevelSoPrefix, "");
                    num = num.Replace(EnvironmentVariables.SoSuffix, "");

                    if (int.TryParse(num, out int levelCount))
                    {
                        return levelCount;
                    }

                    return 0;
                }
            )
            .ToList();

            if (levelCounts.Count > 0)
            {
                nextDirNum = levelCounts.Max() + 1;
            }

            newLevelPath += EnvironmentVariables.LevelSoPrefix +
            nextDirNum +
            EnvironmentVariables.SoSuffix;

            AssetDatabase.CreateAsset(_levelData, newLevelPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            newLevelPath = EnvironmentVariables.GetResourcesPath(newLevelPath);
            _levelData = Resources.Load<LevelData>(newLevelPath);
            _levelData.Construct(newLevelPath);
        }

        [Button]
        private void LoadLevelButton()
        {
            string projectPath = EnvironmentVariables.GetProjectPath
            (EnvironmentVariables.LevelsPath);

            ObjectAssetPickerWindow.ShowWindow
            (
                "Levels",
                projectPath,
                OnLevelPickComplete,
                typeof(LevelData)
            );
        }

        private void OnLevelPickComplete(Object arg0)
        {
            if (arg0 != null)
            {
                ResetEditor();
                LoadLevelEditor((LevelData)arg0);
            }
        }

        public void LoadLevelEditor(LevelData levelData)
        {
            ResetEditor();
            _levelData = levelData;
            LoadGrid();
        }

        private void CreateRoads(float tileSize)
        {
            _roadCont = new GameObject
            ("RoadContainer") {transform = {position = Vector3.zero + 0.1f * Vector3.up}};

            _roadCont.transform.SetParent(_myTrans);

            Vector2Int gridSize = _tileData.Size2Vect();

            for (int x = -1; x < gridSize.x + 1; x ++)
            {
                if (x == -1)
                {
                    CreateRoad
                    (
                        x,
                        -1,
                        tileSize,
                        roadCorner,
                        Corner.LowerRight
                    );

                    CreateRoad
                    (
                        x,
                        gridSize.y,
                        tileSize,
                        roadCorner,
                        Corner.LowerLeft
                    );
                }
                else if (x == gridSize.x)
                {
                    CreateRoad
                    (
                        x,
                        -1,
                        tileSize,
                        roadCorner,
                        Corner.UpperRight
                    );

                    CreateRoad
                    (
                        x,
                        gridSize.y,
                        tileSize,
                        roadT,
                        Corner.UpperRight
                    );

                    for (int x1 = 1; x1 < RoadToOutLength; x1 ++)
                    {
                        CreateRoad
                        (
                            x + x1,
                            gridSize.y,
                            tileSize,
                            road,
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
                        Corner.UpperRight
                    );

                    CreateRoad
                    (
                        x,
                        gridSize.y,
                        tileSize,
                        road,
                        Corner.UpperRight
                    );
                }
            }

            for (int y = 0;
                y <
                _tileData.Size2Vect()
                .y;
                y ++)
            {
                CreateRoad
                (
                    -1,
                    y,
                    tileSize,
                    road
                );

                CreateRoad
                (
                    _tileData.Size2Vect()
                    .x,
                    y,
                    tileSize,
                    road
                );
            }
        }

        private void CreateRoad
        (
            int x,
            int y,
            float tileSize,
            GameObject prefab,
            Corner corner = Corner.NULL
        )
        {
            GameObject newRoad = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Vector3 tilePosition = new(x * tileSize, 0, y * tileSize);
            newRoad.transform.position = tilePosition;

            if (corner != Corner.NULL)
            {
                newRoad.transform.eulerAngles = _roadRots[corner];
            }

            newRoad.transform.SetParent(_roadCont.transform);
        }

        private void LoadGrid()
        {
            IGridAccessEditor gridAccess = _levelData;
            _tileData = gridAccess.GridDataClone;

            int[] gridSize = _tileData.GetSize();

            float tileSize = _tilePrefab.GetComponent<Tile>()
            .MeshRenderer.bounds.size.x;

            for (int x = 0; x < gridSize[0]; x ++)
            for (int y = 0; y < gridSize[1]; y ++)
            {
                TileData tileData = _tileData[x, y];
                Vector3 tilePosition = new(x * tileSize, 0, (gridSize[1] - 1 - y) * tileSize);

                GameObject newTileGo = (GameObject)PrefabUtility.InstantiatePrefab
                (tileData.Prefab, gameObject.scene);

                Tile newTile = newTileGo.GetComponent<Tile>();
                newTile.Construct(new Vector2Int(x, y));
                Transform newTileTrans = newTileGo.transform;
                newTileTrans.parent = _myTrans;
                newTileTrans.localPosition = tilePosition;

                if (tileData.TileItemData?.Prefab != null &&
                    tileData.TileItemData?.IsPivot(x, y) == true)
                {
                    TileItemData tileItemData = tileData.TileItemData;

                    GameObject newTileItemGo = (GameObject)PrefabUtility.InstantiatePrefab
                    (tileItemData.Prefab, gameObject.scene);

                    TileItem newTileItem = newTileItemGo.GetComponent<TileItem>();

                    IGridTileItem gridTileItem = newTileItem;

                    gridTileItem.Construct
                    (
                        newTile.TransformEncapsulated,
                        tileItemData.Coord,
                        tileItemData.GridRot,
                        tileItemData.Size
                    );
                }
            }

            CreateRoads(tileSize);
        }

        [Button]
        private void ResetEditor()
        {
            DestroyLastLevel();
            _levelData = null;
        }

        private void DestroyLastLevel()
        {
            List<GameObject> destroyList = new();

            for (int i = 0; i < transform.childCount; i ++)
            {
                destroyList.Add
                (
                    transform.GetChild(i)
                    .gameObject
                );
            }

            destroyList.DoToAll(DestroyImmediate);
        }
    }
}
#endif