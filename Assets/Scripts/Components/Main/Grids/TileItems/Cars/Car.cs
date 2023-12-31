﻿using System.Collections.Generic;
using System.Linq;
using Components.Main.Grids.GridPathFinder;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Events.External;
using Extensions.DoTween;
using Extensions.Unity;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Components.Main.Grids.TileItems.Cars
{
    public class Car : TileItem, ITargetAbleCar, ITweenContainerBind, IClickAble
    {
        private const float AgentErrorMargin = 0.1f;
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
        [SerializeField] private Vector3 _bodyRot;
        [Inject] private GridEvents GridEvents { get; set; }
        private Vector3 _bodyInitLocEul;
        private Vector3 _currentLeavePath;
        private float _currSpeed;
        private INavNode _destTile;
        private Quaternion _initRot;
        private bool _isDriving;
        private bool _isFull;
        private MaterialPropertyBlock _lastMaterialPropertyBlock;
        private List<Vector3> _leavePath;
        private RoutineHelper _leaveRoutine;
        private int _wpIndex;
        bool ITargetAbleCar.IsFull => _isFull;
        public ITweenContainer TweenContainer { get; set; }

        private void Awake()
        {
            TweenContainer = TweenContain.Install(this);
            _leaveRoutine = new RoutineHelper(this, new WaitForFixedUpdate(), LeaveUpdate);
        }

        private void Start()
        {
            _bodyInitLocEul = _bodyTrans.localEulerAngles;
        }

        void IClickAble.OnClick()
        {
            if (_isDriving) return;
            
            ShowTempOutline();

            GridEvents.CarPathResult? carPathResult = GridEvents.GetCarPath?.Invoke(new GridEvents.TileItemTrans(this));

            if (TryLeaveGrid(carPathResult)) return;
            
            if (carPathResult is {NoPath: false})
            {
                _destTile = carPathResult.Value.NavNode;
                _isDriving = true;
                GridEvents.TileItemMoveStart?.Invoke(this);

                DoAccelerationAnim(_destTile.WPos);

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

        List<ICarDoor> ITargetAbleCar.GetDoors()
        {
            return _doorTransList.Select(e => e as ICarDoor).ToList();
        }

        public void SetFull()
        {
            _isFull = true;
        }

        List<INavNode> ITargetAbleCar.Target()
        {
            ShowTempOutline();
            return GridEvents.GetBorderNavTiles?.Invoke(this);
        }

        private void LeaveUpdate()
        {
            Vector3 walkDir = (_currentLeavePath - transform.position).normalized;
            Vector3 nextPos = _myTransform.position + walkDir * Time.deltaTime * _moveSpeed;

            if (Vector3.Distance(nextPos, _currentLeavePath) < AgentErrorMargin)
            {
                if (GetNextWp() == false)
                {
                    GridEvents.TileItemRemove?.Invoke(this);
                    GridEvents.CarLeftGrid?.Invoke();
                    _leaveRoutine.StopCoroutine();

                    return;
                }
            }
            
            if (Vector3.Angle(_myTransform.forward, nextPos - _myTransform.position) > AgentErrorMargin)
            {
                Vector3 targetDirection = (nextPos - _myTransform.position).normalized;
                Vector3 newDirection = Vector3.RotateTowards(_myTransform.forward, targetDirection, 0.1f, Time.deltaTime * 1f);
                _myTransform.rotation = Quaternion.LookRotation(newDirection);
            }

            _myTransform.position = nextPos;
        }

        private bool GetNextWp()
        {
            _wpIndex ++;

            if (_wpIndex >= _leavePath.Count)
            {
                _leavePath = new List<Vector3>();

                return false;
            }

            _currentLeavePath = _leavePath[_wpIndex];
            return true;
        }

        private bool TryLeaveGrid(GridEvents.CarPathResult? carPathResult)
        {
            if (carPathResult.Value.CanLeaveGrid && _isFull)
            {
                SetLeaving(carPathResult);
                DoAccelerationAnim(carPathResult.Value.LeavePath[0]);

                TweenContainer.AddSequence = DOTween.Sequence();
                
                for (int i = 0; i < carPathResult.Value.LeavePath.Count; i ++)
                {
                    Vector3 path = carPathResult.Value.LeavePath[i];
                
                    float leavePathDur = 0.5f;
                    Tween moveTween = _myTransform.DOMove(path, leavePathDur);
                    moveTween.SetEase(Ease.Linear);
                    TweenContainer.AddedSeq.Insert(i * leavePathDur, moveTween);
                
                    if (i + 1 < carPathResult.Value.LeavePath.Count)
                    {
                        TweenContainer.AddedSeq.Insert
                        (i * leavePathDur, _myTransform.DOLookAt(carPathResult.Value.LeavePath[i] - _myTransform.position, 0.3f));
                    }
                }
                
                TweenContainer.AddedSeq.onComplete += delegate
                {
                    GridEvents.TileItemRemove?.Invoke(this);
                    GridEvents.CarLeftGrid?.Invoke();
                };
                
                return true;
            }

            return false;
        }

        private void SetLeaving(GridEvents.CarPathResult? carPathResult)
        {
            _wpIndex = 0;
            _leavePath = carPathResult.Value.LeavePath;
            _currentLeavePath = _leavePath[_wpIndex];
        }

        private void DoAccelerationAnim(Vector3 destTileWPos)
        {
            Vector3 gridMoveDir = (destTileWPos - _myTransform.position).normalized;
            int maxAxisIndex = gridMoveDir.GetMaxAxisIndex();

            Sequence accelerationAnim = DOTween.Sequence();

            Tween accelerationRise = _bodyTrans.DOLocalRotate
            (
                _bodyInitLocEul + _bodyRot * -1f * gridMoveDir[maxAxisIndex] * AccelerationLeanAngle,
                _accelerationAnimHalfDur
            );

            Tween accelerationFall = _bodyTrans.DOLocalRotate(_bodyInitLocEul, _accelerationAnimHalfDur);
            accelerationAnim.Append(accelerationRise);
            accelerationRise.SetEase(_accRiseEase);
            accelerationAnim.Append(accelerationFall);
            accelerationFall.SetEase(_accFallEase);
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