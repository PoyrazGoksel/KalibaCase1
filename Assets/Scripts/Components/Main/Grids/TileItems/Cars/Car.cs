using System.Collections.Generic;
using System.Linq;
using Components.Main.Grids.GridPathFinder;
using DG.Tweening;
using Events.External;
using Extensions.DoTween;
using UnityEngine;
using Zenject;

namespace Components.Main.Grids.TileItems.Cars
{
    public class Car : TileItem, ITargetAbleCar, ITweenContainerBind, IClickAble
    {
        private static readonly int enableOutline = Shader.PropertyToID("_EnableOutline");
        [SerializeField] private float AccelerationLeanAngle = 15f;
        [SerializeField] private AnimationCurve _accRiseEase;
        [SerializeField] private AnimationCurve _accFallEase;
        [SerializeField] private MeshRenderer[] _myMeshRenderer;
        [SerializeField] private float _moveSpeed = 10f;
        [SerializeField] private float _accelerationAnimHalfDur = 0.5f;
        [SerializeField] private Transform _bodyTrans;
        [SerializeField] private AnimationCurve _moveStartEase;
        [SerializeField] private List<Door> _doorTransList;
        [Inject] private GridEvents GridEvents { get; set; }
        private Vector3 _bodyInitEul;
        private float _currSpeed;
        private INavNode _destTile;
        private Quaternion _initRot;
        private bool _isDriving;
        private MaterialPropertyBlock _lastMaterialPropertyBlock;
        List<ICarDoor> ITargetAbleCar.GetDoors()
        {
            return _doorTransList.Select(e => e as ICarDoor).ToList();
        }

        public ITweenContainer TweenContainer { get; set; }

        private void Awake()
        {
            TweenContainer = TweenContain.Install(this);
        }

        private void Start()
        {
            _bodyInitEul = _bodyTrans.eulerAngles;
        }

        void IClickAble.OnClick()
        {
            if (_isDriving) return;
            
            ShowTempOutline();

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

        List<INavNode> ITargetAbleCar.Target()
        {
            ShowTempOutline();
            return GridEvents.GetBorderNavTiles?.Invoke(this);
        }

        private void ShowOutline(bool isVisible)
        {
            int isVisInt = 0;
            if (isVisible) isVisInt = 1;

            foreach (MeshRenderer meshRenderer in _myMeshRenderer)
            {
                _lastMaterialPropertyBlock = new MaterialPropertyBlock();
                meshRenderer.GetPropertyBlock(_lastMaterialPropertyBlock);
                _lastMaterialPropertyBlock.SetInt(enableOutline, isVisInt);
                meshRenderer.SetPropertyBlock(_lastMaterialPropertyBlock);
            }
        }

        private void ShowTempOutline()
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
        }

        protected override void RegisterEvents() {}

        protected override void UnRegisterEvents()
        {
            TweenContainer.Clear();
        }
    }
}