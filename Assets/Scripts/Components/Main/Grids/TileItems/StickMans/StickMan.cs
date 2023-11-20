using System.Collections.Generic;
using System.Linq;
using Components.Main.Grids.GridPathFinder;
using Components.Main.Grids.TileItems.Cars;
using DG.Tweening;
using Events.External;
using Events.Internal;
using Extensions.DoTween;
using Extensions.Unity;
using UnityEngine;
using Zenject;

namespace Components.Main.Grids.TileItems.StickMans
{
    public class StickMan : TileItem, ISelectable, ITweenContainerBind
    {
        private static readonly int openDoorAnimTrig = Animator.StringToHash("OpenDoor");
        private static readonly int yellAnimTrig = Animator.StringToHash("Yell");
        private static readonly int isWalkingAnimBool = Animator.StringToHash("IsWalking");
        private static readonly int enableOutline = Shader.PropertyToID("_EnableOutline");
        [SerializeField] private SkinnedMeshRenderer _myMeshRenderer;
        [SerializeField] private Animator _animator;
        [SerializeField] private GridAgent _gridAgent;
        [SerializeField] private Emoji _angryFace;
        [SerializeField] private Emoji _happyFace;
        
        [Inject] private GridAgentEvents GridAgentEvents { get; set; }
        [Inject] private GridEvents GridEvents { get; set; }
        private int LastAnimTrig { get; set; }
        private INavNode _currentTile;
        private Vector2Int? _destination;
        private bool _isWalking;
        private MaterialPropertyBlock _lastMaterialPropertyBlock;
        private int _wpIndex;
        public ITweenContainer TweenContainer { get; set; }
        private bool _isEnteringCar;
        private List<ICarDoor> _doors;
        private ITargetAbleCar _carToEnter;

        private void Awake()
        {
            TweenContainer = TweenContain.Install(this);
        }

        void ISelectable.Select()
        {
            if (_isWalking) return;

            ShowOutline(true);
        }

        private void ShowOutline(bool isVisible)
        {
            int isVis = 0;

            if (isVisible) isVis = 1;

            _lastMaterialPropertyBlock = new MaterialPropertyBlock();
            _myMeshRenderer.GetPropertyBlock(_lastMaterialPropertyBlock);
            _lastMaterialPropertyBlock.SetInt(enableOutline, isVis);
            _myMeshRenderer.SetPropertyBlock(_lastMaterialPropertyBlock);
        }

        void ISelectable.DeSelect()
        {
            ShowOutline(false);
        }

        void ISelectable.SetDest(Vector2Int targetAbleGridCoord)
        {
            SetDest(targetAbleGridCoord);
            _lastMaterialPropertyBlock.SetFloat(enableOutline, 0);
            _myMeshRenderer.SetPropertyBlock(_lastMaterialPropertyBlock);
        }

        void ISelectable.SetTarget
        (
            List<INavNode> borderTiles,
            TileItemColor tileItemColor,
            List<ICarDoor> doorTransList,
            ITargetAbleCar targetAbleCar
        )
        {
            if (tileItemColor!= TileItemColor || targetAbleCar.IsFull)
            {
                DoAngryFace();
                return;
            }

            if (borderTiles.Any(e => e.Coord == GridCoord))
            {
                GridEvents.TileItemMoveStart?.Invoke(this);
                SetEnteringCar(doorTransList, targetAbleCar);
                EnterCar();
                DoHappyFace();
                return;
            }
            
            float closestTileDist = Mathf.Infinity;
            INavNode closestTile = null;
            
            foreach (INavNode borderTile in borderTiles)
            {
                if (borderTile.WalkAble == false) continue;

                if (_gridAgent.CanSetDest(GridCoord, borderTile.Coord) == false) continue;

                float newDist = Vector3.Distance(_myTransform.position, borderTile.WPos);
                
                if (newDist < closestTileDist)
                {
                    closestTileDist = newDist;
                    closestTile = borderTile;
                }
            }   

            if (closestTile != null)
            {
                SetEnteringCar(doorTransList, targetAbleCar);
                SetDest(closestTile.Coord);
                DoHappyFace();
            }
            else
            {
                DoAngryFace();
            }
        }

        private void SetEnteringCar(List<ICarDoor> doorTransList, ITargetAbleCar targetAbleCar)
        {
            _isEnteringCar = true;
            _doors = doorTransList;
            _carToEnter = targetAbleCar;
        }

        private void DoAngryFace()
        {
            SetTrigger(yellAnimTrig);
            _angryFace.DoAnim();
        }
        
        private void DoHappyFace()
        {
            _happyFace.DoAnim();
        }

        private void SetTrigger(int trig)
        {
            _animator.ResetTrigger(LastAnimTrig);
            LastAnimTrig = trig;
            _animator.SetTrigger(trig);
        }
        
        private void SetWalking(bool isWalking)
        {
            _animator.SetBool(isWalkingAnimBool, isWalking);
        }

        private void SetDest(Vector2Int targetAbleGridCoord)
        {
            _destination = targetAbleGridCoord;
            List<(Vector3, Vector2Int)> path = _gridAgent.SetDest(GridCoord, _destination.Value);
            
            if (path != null)
            {
                GridEvents.TileItemMoveStart?.Invoke(this);
                SetWalking(true);
            }
        }

        protected override void RegisterEvents()
        {
            GridAgentEvents.ArrivedDest += OnArrivedDest;
        }

        private void OnArrivedDest(Vector2Int arg0)
        {
            if (_isEnteringCar)
            {
                EnterCar();

                return;
            }
            SetCoord(arg0);
            GridEvents.TileItemMoveEnd?.Invoke(this);
            SetWalking(false);
        }

        private void EnterCar()
        {
            ICarDoor closestDoor = _doors.OrderBy
            (e => Vector3.Distance(e.EntrancePoint, _myTransform.position))
            .First();

            TweenContainer.AddTween = _myTransform.DOMove(closestDoor.EntrancePoint, 3f);
            TweenContainer.AddedTween.SetSpeedBased();

            TweenContainer.AddedTween.onComplete += delegate
            {
                SetTrigger(openDoorAnimTrig);
                closestDoor.Open();
                TweenContainer.AddTween = _myTransform.DOMove(closestDoor.SeatPos, 1f);

                TweenContainer.AddedTween.onComplete += delegate
                {
                    closestDoor.Close();
                    SetWalking(false);
                    _carToEnter.SetFull();
                    GridEvents.TileItemRemove?.Invoke(this);
                };

                TweenContainer.AddTween = _myTransform.DOLookAt(closestDoor.SeatPos, 0.3f);
                TweenContainer.AddTween = _myTransform.DOScale(0.5f * Vector3.one, 1f);
            };

            TweenContainer.AddTween = _myTransform.DOLookAt(closestDoor.EntrancePoint, 0.3f);
        }

        protected override void UnRegisterEvents()
        {
            GridAgentEvents.ArrivedDest -= OnArrivedDest;
            TweenContainer.Clear();
        }
    }
}