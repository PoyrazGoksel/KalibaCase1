using System.Drawing;
using Components.Main.Grids.GridPathFinder;
using DG.Tweening;
using Events.External;
using Extensions.DoTween;
using Extensions.Unity;
using UnityEngine;
using Zenject;

namespace Components.Main.Grids.TileItems
{
    public class Car : TileItem, ITargetAble, ITweenContainerBind, ISelectable
    {
        private static readonly int mainTexColor = Shader.PropertyToID("_Color");
        private static readonly int outLineColor = Shader.PropertyToID("_OutLineColor");
        private static readonly int enableOutline = Shader.PropertyToID("_EnableOutline");
        private static readonly int outLineSize = Shader.PropertyToID("_OutlineSize");
        
        [SerializeField]private float WayPointReachDist = 0.1f;
        [SerializeField]private float AccelerationLeanAngle = 15f;
        [SerializeField]private AnimationCurve _accRiseEase;
        [SerializeField]private AnimationCurve _accFallEase;
        [SerializeField] private MeshRenderer[] _myMeshRenderer;
        [SerializeField] private Transform _rearAxleTrans;
        [SerializeField] private Transform _frontAxleTrans;
        [SerializeField] private float _moveSpeed = 10f;
        [SerializeField] private float _accelerationAnimHalfDur = 0.5f;
        [SerializeField] private Transform _bodyTrans;
        [SerializeField] private AnimationCurve _moveStartEase;
        [Inject] private GridEvents GridEvents { get; set; }
        private Vector3 _bodyInitEul;
        private float _currSpeed;
        private INavNode _destTile;
        private Quaternion _initRot;
        private bool _isDriving;
        private MaterialPropertyBlock _lastMaterialPropertyBlock;
        private Tween _selectionTween;
        public ITweenContainer TweenContainer { get; set; }

        private void Start()
        {
            _bodyInitEul = _bodyTrans.eulerAngles;
        }

        void ISelectable.Select()
        {
            if (_isDriving) return;

            foreach (MeshRenderer meshRenderer in _myMeshRenderer)
            {
                _lastMaterialPropertyBlock = new MaterialPropertyBlock();
                meshRenderer.GetPropertyBlock(_lastMaterialPropertyBlock);
                _selectionTween = DOVirtual.Float
                (
                    0.1f,
                    0.05f,
                    0.5f,
                    delegate(float value)
                    {
                        _lastMaterialPropertyBlock.SetFloat(outLineSize, value);
                        meshRenderer.SetPropertyBlock(_lastMaterialPropertyBlock);
                    }
                );
            }

            GridEvents.CarPathResult? carPathResult = GridEvents.GetCarPath?.Invoke(new GridEvents.TileItemTrans(this));

            if (carPathResult is {NoPath: false})
            {
                _destTile = carPathResult.Value.NavNode;
                _isDriving = true;
                GridEvents.TileItemMoveStart?.Invoke(this);

                Vector3 gridMoveDir = (_destTile.WPos - _myTransform.position).normalized;

                //gridMoveDir = GridF.PerpVect2D(gridMoveDir);
                
                Sequence accelerationAnim = DOTween.Sequence();
                
                Tween accelerationRise = _bodyTrans.DORotate(_bodyInitEul +
                    gridMoveDir * AccelerationLeanAngle, _accelerationAnimHalfDur);
                
                Tween accelerationFall = _bodyTrans.DORotate(_bodyInitEul, _accelerationAnimHalfDur);
                accelerationAnim.Append(accelerationRise);
                accelerationRise.SetEase(_accRiseEase);
                accelerationAnim.Append(accelerationFall);
                accelerationFall.SetEase(_accFallEase);

                Tween moveTween = _myTransform.DOMove(_destTile.WPos, _moveSpeed)
                .SetSpeedBased(true);

                moveTween.SetEase(_moveStartEase);
                moveTween.onComplete += delegate
                {
                    _isDriving = false;
                    SetCoord(_destTile.Coord);
                    GridEvents.TileItemMoveEnd?.Invoke(this);
                };
            }
        }

        public void DeSelect()
        {
            if (_selectionTween.IsActive()) _selectionTween.Kill();
        }

        void ISelectable.SetDest(Vector2Int targetAbleGridCoord) {}

        public void Target() {}

    }
}