using System;
using System.Collections.Generic;
using System.Linq;
using Extensions.System;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace Datas.Levels
{
    public class SoPrefabPickerWindow : EditorWindow
    {
        private const int EditorAssetPreviewSquare = 128;
        private string _assetsPath;
        private PrefabPickableSo[] _assets;
        private Vector2 _scrollPosition;
        private PrefabPickableSo _selectedAsset;
        private Type _assetType;
        private string _subTitle;
        private UnityAction<PrefabPickableSo> _onAccept;
        private UnityAction<PrefabPickableSo> _onSelection;
        private UnityAction _onClosed;
        private bool _groupByInheritedType;

        public static void ShowWindow
        (
            string title,
            string assetsPath,
            UnityAction<PrefabPickableSo> onAccept,
            UnityAction<PrefabPickableSo> onSelection,
            UnityAction onClosed,
            Type assetsType = null,
            string subtitle = null,
            bool groupByInheritedType = false
        )
        {
            SoPrefabPickerWindow newWin = GetWindow<SoPrefabPickerWindow>(title, true);

            newWin.Construct
            (
                subtitle,
                assetsPath,
                assetsType,
                onAccept,
                onSelection,
                onClosed,
                groupByInheritedType
            );

            newWin.LoadPrefabAssetsFromFolder();
        }

        private void Construct
        (
            string subtitle,
            string assetsPath,
            Type assetType,
            UnityAction<PrefabPickableSo> onAccept,
            UnityAction<PrefabPickableSo> onSelection,
            UnityAction onClosed,
            bool groupByInheritedType
        )
        {
            _subTitle = subtitle;
            _assetsPath = assetsPath;
            _assetType = assetType;
            _onAccept = onAccept;
            _onSelection = onSelection;
            _onClosed = onClosed;
            _groupByInheritedType = groupByInheritedType;
        }

        private void OnLostFocus()
        {
            Close();
        }

        private void OnDisable()
        {
            _onClosed?.Invoke();
        }

        private void OnGUI()
        {
            if (_assets == null) return;

            if (_assets.Length == 0) return;

            if (string.IsNullOrEmpty(_subTitle) == false)
            {
                GUILayout.Label(_subTitle, EditorStyles.boldLabel);
            }

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            GUILayout.BeginHorizontal();

            int buttonHorizontalCount = 0;
            Type lastType = null;

            foreach (PrefabPickableSo asset in _assets)
            {
                Texture2D assetPreview = AssetPreview.GetAssetPreview(asset.Prefab) ?? Texture2D.whiteTexture;

                if (buttonHorizontalCount != 0 && buttonHorizontalCount % 5 == 0)
                {
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }
                else
                {
                    TryGroupByType(ref lastType, asset, ref buttonHorizontalCount);
                }

                if (asset == _selectedAsset)
                {
                     GUILayout.Box(new GUIContent("Selected", assetPreview),GUILayout.Width(EditorAssetPreviewSquare), GUILayout.Height(EditorAssetPreviewSquare));   
                }
                else
                {
                    if (GUILayout.Button
                    (assetPreview, GUILayout.Width(EditorAssetPreviewSquare), GUILayout.Height(EditorAssetPreviewSquare)))
                    {
                        _selectedAsset = asset;
                        _onSelection?.Invoke(asset);
                        break;
                    }
                }

                buttonHorizontalCount ++;
            }
            
            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();
            
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Accept"))
            {
                if(_selectedAsset != null) _onAccept?.Invoke(_selectedAsset);
                Close();
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void TryGroupByType(ref Type lastType, PrefabPickableSo asset, ref int buttonHorizontalCount)
        {
            if (_groupByInheritedType == false)
            {
                lastType = null;
                return;
            }

            if (_assetType == null)
            {
                lastType = null;
                return;
            }

            if (lastType !=
                asset.Prefab.GetComponent(_assetType)
                .GetType())
            {
                buttonHorizontalCount = 0;

                lastType = asset.Prefab.GetComponent(_assetType)
                .GetType();

                GUILayout.EndHorizontal();
                GUILayout.BeginVertical();
                GUILayout.Label(lastType.Name);
                GUILayout.EndVertical();
                GUILayout.BeginHorizontal();
            }
        }

    
        private void LoadPrefabAssetsFromFolder()
        {
            string[] assetPaths = AssetDatabase.FindAssets
            (
                "",
                new[]
                {
                    _assetsPath
                }
            )
            .Select(AssetDatabase.GUIDToAssetPath)
            .ToArray();

            IEnumerable<PrefabPickableSo> assetsEnums = assetPaths.Select
            (AssetDatabase.LoadAssetAtPath<PrefabPickableSo>)
            .NotNull();

            if (_assetType != null)
            {
                PrefabPickableSo[] assetsByType = assetsEnums.Where
                (
                    e =>
                    {
                        Object component = e.Prefab.GetComponent(_assetType);

                        return component != null &&
                        (component.GetType() == _assetType ||
                            component.GetType()
                            .GetBaseTypes()
                            .Any(e1 => e1 == _assetType));
                    }
                )
                .ToArray();

                if (assetsByType.Length > 0)
                {
                    _assets = assetsByType.OrderBy
                    (
                        e => e.Prefab.GetComponent(_assetType)
                        .GetType()
                        .Name
                    )
                    .ToArray();
                }
            }
            else
            {
                _assets = assetsEnums.ToArray();
            }
        }
    }

    public class PrefabPickableSo : ScriptableObject
    {
        [SerializeField] private GameObject _prefab;

        public GameObject Prefab => _prefab;

        
    }
}