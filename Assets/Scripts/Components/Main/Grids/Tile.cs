using System;
using System.Collections.Generic;
using Components.Main.Grids.GridPathFinder;
using Components.Main.Grids.TileItems;
using DG.Tweening;
using Extensions.Unity;
using Extensions.Unity.MonoHelper;
using UnityEngine;

namespace Components.Main.Grids
{
    public class Tile : EventListenerMono, IGridTile, INavNode, ITargetAble
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
        public Vector2Int Coord => _coord;
        bool INavNode.WalkAble => _tileItem == null;
        Vector3 INavNode.WPos => _transformEncapsulated.position;
        Vector2Int ITargetAble.GridCoord => _coord;
        private Tween _selectionTween;
        void IGridTile.SetTileItem(TileItem tileItem)
        {
            _tileItem = tileItem;
        }

        void ITargetAble.Target()
        {
            MaterialPropertyBlock materialPropertyBlock = new();
            _lastMaterialPropertyBlock = materialPropertyBlock;
            _meshRenderer.GetPropertyBlock(_lastMaterialPropertyBlock);
            _selectionTween = DOVirtual.Float
            (
                0.1f,
                0.05f,
                0.5f,
                delegate(float value)
                {
                    _lastMaterialPropertyBlock.SetFloat(outLineSize, value);
                    _meshRenderer.SetPropertyBlock(_lastMaterialPropertyBlock);
                }
            );
        }

        protected override void RegisterEvents() {}

        protected override void UnRegisterEvents() {}

        public void Construct(Vector2Int vector2Int)
        {
            _coord = vector2Int;
            //transform.position = vector2Int.Transpose();
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