using System;
using System.Collections.Generic;
using System.Linq;
using Extensions.System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace Datas.Levels
{
    public class ObjectAssetPickerWindow : EditorWindow
    {
        private const int EditorAssetPreviewSquare = 128;
        private string _assetsPath;
        private Object[] _assets;
        private Vector2 _scrollPosition;
        private Object _selectedAsset;
        private Type _assetType;
        private string _subTitle;
        private UnityAction<Object> _onComplete;
        private bool _groupByInheritedType;

        public static void ShowWindow
        (
            string title,
            string assetsPath,
            UnityAction<Object> onComplete,
            Type assetsType = null,
            string subtitle = null,
            bool groupByInheritedType = false
        )
        {
            ObjectAssetPickerWindow newWin = GetWindow<ObjectAssetPickerWindow>(title, true);

            newWin.Construct
            (
                subtitle,
                assetsPath,
                assetsType,
                onComplete,
                groupByInheritedType
            );

            newWin.LoadPrefabAssetsFromFolder();
        }

        private void Construct
        (
            string subtitle,
            string assetsPath,
            Type assetType,
            UnityAction<Object> onComplete,
            bool groupByInheritedType
        )
        {
            _subTitle = subtitle;
            _assetsPath = assetsPath;
            _assetType = assetType;
            _onComplete = onComplete;
            _groupByInheritedType = groupByInheritedType;
        }

        private void OnLostFocus()
        {
            Close();
        }

        private void OnDisable()
        {
            _onComplete?.Invoke(_selectedAsset);
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

            foreach (Object asset in _assets)
            {
                Texture2D assetPreview = AssetPreview.GetAssetPreview(asset);

                if (buttonHorizontalCount != 0 && buttonHorizontalCount % 5 == 0)
                {
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }
                else
                {
                    TryGroupByType(ref lastType, asset, ref buttonHorizontalCount);
                }

                if (assetPreview == null)
                {
                    if (GUILayout.Button
                    (
                        new GUIContent(asset.name),
                        GUILayout.Width(EditorAssetPreviewSquare),
                        GUILayout.Height(EditorAssetPreviewSquare)
                    ))
                    {
                        _selectedAsset = asset;
                        Close();

                        break;
                    }    
                }
                else
                {
                    if (GUILayout.Button
                    (
                        assetPreview,
                        GUILayout.Width(EditorAssetPreviewSquare),
                        GUILayout.Height(EditorAssetPreviewSquare)
                    ))
                    {
                        _selectedAsset = asset;
                        Close();

                        break;
                    }
                
                }

                buttonHorizontalCount ++;
            }

            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();
        }

        private void TryGroupByType(ref Type lastType, Object asset, ref int buttonHorizontalCount)
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

            if (lastType != asset.GetType())
            {
                buttonHorizontalCount = 0;

                lastType = asset.GetType();

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

            IEnumerable<Object> assetsEnums = assetPaths.Select
            (AssetDatabase.LoadAssetAtPath<Object>)
            .NotNull();

            if (_assetType != null)
            {
                Object[] assetsByType = assetsEnums.Where(e => e.GetType() == _assetType)
                .ToArray();

                if (assetsByType.Length > 0)
                {
                    _assets = assetsByType.OrderBy
                    (
                        e => e.GetType()
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
}