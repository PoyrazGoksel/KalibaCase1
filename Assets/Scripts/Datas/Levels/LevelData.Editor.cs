#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Components.Main.Grids;
using Components.Main.Grids.TileItems;
using Datas.Levels.Grids;
using Extensions.System;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Utils;
using Grid = Components.Main.Grids.Grid;

namespace Datas.Levels
{
    public partial class LevelData : IGridAccessEditor
    {
        [SerializeField] private GameObject _tilePrefab;
        [ShowInInspector] private TileItemData _overLappingItem;
        
        TileData[,] IGridAccessEditor.GridDataClone => (TileData[,])_tileGridData.Clone();

        private TileData DrawGridPreview(Rect tileRect, TileData tile)
        {
            const int refTileRectSize = 64;
            const int addButtonHeight = 15;
            const int addButtonWidth = 62;
            const int crossButtonHeight = 18;
            const int addButtonFontSize = 8;
            const string pickTileItemText = "Pick Tile Item";
            const string pickTileItemTextShort = "Pick...";

            float newRatio = tileRect.height / refTileRectSize;

            Rect addTileItemRect = new(tileRect);

            addTileItemRect = addTileItemRect.AlignBottom(addButtonHeight * newRatio);
            addTileItemRect = addTileItemRect.AlignCenter(addButtonWidth * newRatio);

            Rect crossButRect = new(tileRect);
            crossButRect = crossButRect.AlignRight(crossButtonHeight * newRatio);
            crossButRect = crossButRect.AlignTop(crossButtonHeight * newRatio);
            
            Rect rotateRect = new(tileRect);
            rotateRect.width -= crossButtonHeight;
            rotateRect = rotateRect.AlignRight(crossButtonHeight * newRatio);
            rotateRect = rotateRect.AlignTop(crossButtonHeight * newRatio);

            Rect spriteRect = new(tileRect);
            spriteRect = spriteRect.AlignTop(spriteRect.height);
            if (tileRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    if (addTileItemRect.Contains(Event.current.mousePosition))
                    {
                        SoPrefabPickerWindow.ShowWindow
                        (
                            pickTileItemText,
                            EnvironmentVariables.GetProjectPath(EnvironmentVariables.TileItemsSoPath),
                            OnPrefabPickerAccepted(tile),
                            null,
                            null,
                            typeof(TileItem),
                            groupByInheritedType: true
                        );

                        GUI.changed = true;
                        Event.current.Use();

                        return tile;
                    }

                    if (tile.TileItemData != null && tile.TileItemData.Coord == tile.Coord)
                    {
                        if (crossButRect.Contains(Event.current.mousePosition))
                        {
                            RemoveTileItemFromGrid(tile);

                            GUI.changed = true;
                            Event.current.Use();

                            return tile;
                        }
                    
                        if (rotateRect.Contains(Event.current.mousePosition))
                        {
                            RotateTileItem(tile);
                            GUI.changed = true;
                            Event.current.Use();

                            return tile;
                        }
                    }
                    
                    if (tile.OverlappingTileItemData != null && tile.OverlappingTileItemData.Coord == tile.Coord)
                    {
                    
                        if (rotateRect.Contains(Event.current.mousePosition))
                        {
                            RotateTileItem(tile);
                            GUI.changed = true;
                            Event.current.Use();

                            return tile;
                        }
                    }
                }
            }
            
            DrawPreviewTexture(tile, spriteRect);

            DrawAddButton(addButtonFontSize, newRatio, pickTileItemText,
                pickTileItemTextShort,
                addTileItemRect
            );

            DrawRotateAndCross(tile, rotateRect, crossButRect);

            DrawPivotTxt(tileRect, tile);

            SirenixEditorGUI.DrawBorders
            (
                tileRect.Padding(-2),
                ShouldDrawBlueBorder(tile, GridF.GridDown, 3),
                ShouldDrawBlueBorder(tile, GridF.GridUp, 3),
                ShouldDrawBlueBorder(tile, GridF.GridRight, 3),
                ShouldDrawBlueBorder(tile, GridF.GridLeft, 3),
                Color.blue
            );

            SirenixEditorGUI.DrawBorders
            (
                tileRect.Padding(-4),
                ShouldDrawRedBorder(tile, GridF.GridDown, 3),
                ShouldDrawRedBorder(tile, GridF.GridUp, 3),
                ShouldDrawRedBorder(tile, GridF.GridRight, 3),
                ShouldDrawRedBorder(tile, GridF.GridLeft, 3),
                Color.red
            );

            return tile;

        }

        private static void DrawPivotTxt(Rect tileRect, TileData tile)
        {
            const string pivot = "Pivot";

            if (tile.TileItemData != null)
            {
                if (tile.TileItemData.Coord == tile.Coord)
                {

                    EditorGUI.LabelField
                    (
                        tileRect.Padding
                        (
                            20,
                            0,
                            0,
                            0
                        ),
                        pivot,
                        EditorStyles.miniLabel
                    );
                }
            }
            else if (tile.OverlappingTileItemData != null)
            {
                if (tile.OverlappingTileItemData.Coord == tile.Coord)
                {

                    EditorGUI.LabelField
                    (
                        tileRect.Padding
                        (
                            20,
                            0,
                            0,
                            0
                        ),
                        pivot,
                        EditorStyles.miniLabel
                    );
                }
            }
        }

        private static void DrawRotateAndCross(TileData tile, Rect rotateRect, Rect crossButRect)
        {
            if (tile.TileItemData != null)
            {
                if (tile.Coord == tile.TileItemData.Coord)
                {
                    EditorIcons.Rotate.Draw(rotateRect);
                    SirenixEditorGUI.DrawBorders(rotateRect, 1, Color.gray);
                    EditorIcons.X.Draw(crossButRect);
                    SirenixEditorGUI.DrawBorders(crossButRect, 1, Color.gray);
                }
            }

            if (tile.OverlappingTileItemData != null)
            {
                if (tile.Coord == tile.OverlappingTileItemData.Coord)
                {
                    EditorIcons.Rotate.Draw(rotateRect);
                    SirenixEditorGUI.DrawBorders(rotateRect, 1, Color.gray);
                }
            }
        }

        private static void DrawAddButton
        (
            int addButtonFontSize,
            float newRatio,
            string pickTileItemText,
            string pickTileItemTextShort,
            Rect addTileItemRect
        )
        {
            GUIStyle addButStyle = new(SirenixGUIStyles.Button);
            int newAddButtonSize = (int)(addButtonFontSize * newRatio);
            string addText = pickTileItemText;

            if (newAddButtonSize < 12)
            {
                newAddButtonSize = 12;
                addText = pickTileItemTextShort;
            }

            addButStyle.fontSize = newAddButtonSize;

            EditorGUI.LabelField(addTileItemRect, addText, addButStyle);
        }

        private static void DrawPreviewTexture(TileData tile, Rect spriteRect)
        {
            if (tile?.TileItemData != null)
            {
                if (tile.OverlappingTileItemData != null)
                {
                    EditorGUI.DrawPreviewTexture
                    (
                        spriteRect.Padding
                        (
                            5,
                            spriteRect.width / 2 + 2,
                            5,
                            5
                        ),
                        AssetPreview.GetAssetPreview(tile.TileItemData.Prefab)
                    );

                    EditorGUI.DrawPreviewTexture
                    (
                        spriteRect.Padding
                        (
                            spriteRect.width / 2 + 5,
                            5,
                            5,
                            5
                        ),
                        AssetPreview.GetAssetPreview(tile.OverlappingTileItemData.Prefab)
                    );
                }
                else
                {
                    EditorGUI.DrawPreviewTexture
                    (spriteRect.Padding(5), AssetPreview.GetAssetPreview(tile.TileItemData.Prefab));
                }
            }
            else if (tile?.OverlappingTileItemData != null)
            {
                EditorGUI.DrawPreviewTexture
                (spriteRect.Padding(5), AssetPreview.GetAssetPreview(tile.OverlappingTileItemData.Prefab));
            }
        }

        private int ShouldDrawRedBorder(TileData tile, Vector2Int dir, int borderSize)
        {
            if (tile.OverlappingTileItemData == null) return 0;

            Vector2Int neighTileCoord = tile.Coord + dir;

            if (GridF.GridContainsCoord(neighTileCoord, _tileGridData.Size2Vect()))
            {
                TileData neighTile = _tileGridData[neighTileCoord.x, neighTileCoord.y];

                if (neighTile.OverlappingTileItemData != null &&
                    neighTile.OverlappingTileItemData.Coord == tile.OverlappingTileItemData.Coord)
                {
                    return 0;
                }
            }

            return borderSize;
        }

        private int ShouldDrawBlueBorder(TileData tile, Vector2Int dir, int borderSize)
        {
            if (tile.TileItemData == null) return 0;

            Vector2Int neighTileCoord = tile.Coord + dir;

            if (GridF.GridContainsCoord(neighTileCoord, _tileGridData.Size2Vect()))
            {
                TileData neighTile = _tileGridData[neighTileCoord.x, neighTileCoord.y];

                if (neighTile.TileItemData != null &&
                    neighTile.TileItemData.Coord == tile.TileItemData.Coord)
                {
                    return 0;
                }
            }

            return borderSize;
        }

        private UnityAction<PrefabPickableSo> OnPrefabPickerAccepted(TileData tileData)
        {
            RemoveOverlapState();

            return delegate(PrefabPickableSo prefabPickableSo)
            {
                TileItemSo tileItemSo = (TileItemSo)prefabPickableSo;
                if (tileData.TileItemData != null)
                {
                    RemoveTileItemFromGrid(tileData);
                }

                if (tileItemSo == null) return;

                TileItem tileItemPrefabComponent = tileItemSo.Prefab.GetComponent<TileItem>();

                if (GridF.CanFitTileItemData(tileItemSo.GridSize, GridRot.Up, tileData.Coord, _tileGridData) == false)
                {
                    Debug.LogWarning("Cant add item its colliding with other objects");
                    TileItemData newTileItemData = new
                    (
                        tileItemSo.Prefab,
                        tileData.Coord,
                        tileItemSo.GridSize,
                        tileItemPrefabComponent.GridRotation
                    );

                    GridF.PutTileItemInGridData
                    (
                        newTileItemData,
                        newTileItemData.Size,
                        newTileItemData.Coord,
                        _tileGridData
                    );
                }
                else
                {
                    TileItemData newTileItemData = new
                    (
                        tileItemSo.Prefab,
                        tileData.Coord,
                        tileItemSo.GridSize,
                        tileItemPrefabComponent.GridRotation
                    );

                    GridF.PutTileItemInGridData
                    (
                        newTileItemData,
                        newTileItemData.Size,
                        newTileItemData.Coord,
                        _tileGridData
                    );
                }
            };
        }

        private void RotateTileItem(TileData tileData)
        {
            TileItemData overlappingTileItemData = tileData.OverlappingTileItemData;

            if (overlappingTileItemData != null)
            {
                RemoveOverlapState();
                overlappingTileItemData.RotateClockwise();
                GridF.PutTileItemInGridData(overlappingTileItemData, overlappingTileItemData.Size, overlappingTileItemData.Coord, _tileGridData);
            }
            else
            {
                RemoveOverlapState();
                TileItemData tileItemData = tileData.TileItemData;
                tileItemData.RotateClockwise();
                RemoveTileItemFromGrid(tileData);
                GridF.PutTileItemInGridData(tileItemData, tileItemData.Size, tileItemData.Coord, _tileGridData);
            }
        }

        private void RemoveOverlapState()
        {
            foreach (TileData data in _tileGridData)
            {
                data.OverlappingTileItemData = null;
            }
        }

        private void RemoveTileItemFromGrid(TileData tile)
        {
            TileItemData itemToRemove = tile.TileItemData;

            foreach (TileData tileData in _tileGridData)
            {
                if (tileData.TileItemData == itemToRemove)
                {
                    tileData.SetTileItem(null);
                }
            }

            EditorUtility.SetDirty(this);
        }

        public void Construct(string newLevelPath)
        {
            _path = newLevelPath;
        }

        private void NewGrid(int x, int y)
        {
            _tileGridData = new TileData[x, y];
        }

        [InfoBox("Will change the active scene!", InfoMessageType.Warning)]
        [HideInInlineEditors]
        [Button]
        private void LoadLevelToEditor()
        {
            EditorSceneManager.OpenScene(EnvironmentVariables.LevelGeneratorScenePath);

            Grid firstGrid = FindObjectsOfType<Grid>()
            .FirstOrDefault();

            if (firstGrid != null)
            {
                firstGrid.LoadLevelEditor(this);
            }
            else
            {
                Scene activeScene = SceneManager.GetActiveScene();

                GameObject gridPrefab = AssetDatabase.LoadAssetAtPath<GameObject>
                (EnvironmentVariables.GridPrefabPath);

                GameObject newGridGo = (GameObject)PrefabUtility.InstantiatePrefab
                (gridPrefab, activeScene);

                newGridGo.GetComponent<Grid>()
                .LoadLevelEditor(Resources.Load<LevelData>(_path));

                Selection.SetActiveObjectWithContext(newGridGo, newGridGo);
            }
        }
        
        [Button]
        private void GenerateGrid(int sizeX, int sizeY)
        {
            if (sizeX == 0) sizeX = 5;

            if (sizeY == 0) sizeY = 5;

            NewGrid(sizeX, sizeY);

            for (int x = 0; x < sizeX; x ++)
            for (int y = sizeY - 1; y >= 0; y --)
            {
                Vector2Int coord = new(x, y);

                TileData newTileData = new (_tilePrefab, coord);

                _tileGridData[x, y] = newTileData;
            }

            EditorUtility.SetDirty(this);
        }
    }

    public interface IGridAccessEditor
    {
        TileData[,] GridDataClone { get; }
    }
}
#endif