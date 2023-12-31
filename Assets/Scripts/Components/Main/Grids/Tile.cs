﻿using System;
using System.Collections.Generic;
using Components.Main.Grids.GridPathFinder;
using Components.Main.Grids.TileItems;
using DG.Tweening;
using Extensions.DoTween;
using Extensions.Unity;
using Extensions.Unity.MonoHelper;
using UnityEngine;

namespace Components.Main.Grids
{
    public class Tile : MonoBehaviour, IGridTile, INavNode, ITargetAble, ITweenContainerBind
    {
        private static readonly int enableOutline = Shader.PropertyToID("_EnableOutline");
        private static readonly int outLineSize = Shader.PropertyToID("_OutlineSize");
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private TileItem _tileItem;
        [SerializeField] private Vector2Int _coord;
        [SerializeField] private TransformEncapsulated _transformEncapsulated;
        public TileItem TileItem => _tileItem;
        public MeshRenderer MeshRenderer => _meshRenderer;
        public TransformEncapsulated TransformEncapsulated => _transformEncapsulated;
        private MaterialPropertyBlock _lastMaterialPropertyBlock;
        private Tween _selectionTween;
        public Vector2Int Coord => _coord;
        bool INavNode.WalkAble => _tileItem == null;
        Vector3 INavNode.WPos => _transformEncapsulated.position;
        public ITweenContainer TweenContainer { get; set; }

        private void Awake()
        {
            TweenContainer = TweenContain.Install(this);
        }

        public void Construct(Vector2Int vector2Int)
        {
            _coord = vector2Int;
            //transform.position = vector2Int.Transpose();
        }

        private void OnDisable()
        {
            TweenContainer.Clear();
        }

        void IGridTile.SetTileItem(TileItem tileItem)
        {
            _tileItem = tileItem;
        }

        Vector2Int ITargetAble.Target()
        {
            ShowOutline(true);

            TweenContainer.AddTween = DOVirtual.DelayedCall
            (
                1f,
                delegate
                {
                    ShowOutline(false);
                }
            );

            return _coord;
        }

        private void ShowOutline(bool isVisible)
        {
            int isVis = 0;

            if (isVisible) isVis = 1;

            _lastMaterialPropertyBlock = new MaterialPropertyBlock();
            _meshRenderer.GetPropertyBlock(_lastMaterialPropertyBlock);
            _lastMaterialPropertyBlock.SetInt(enableOutline, isVis);
            _meshRenderer.SetPropertyBlock(_lastMaterialPropertyBlock);
        }

        [Serializable]
        public class Settings
        {
            [SerializeField] private List<TileItem> _tileItems;
            public List<TileItem> TileItems => _tileItems;
        }
    }

    public interface IGridTile
    {
        void SetTileItem(TileItem tileItem);
    }
}