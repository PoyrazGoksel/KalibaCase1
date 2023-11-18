using System.Collections.Generic;
using Components.Main.Grids.GridPathFinder;
using DG.Tweening;
using Events.External;
using Events.Internal;
using Extensions.DoTween;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Components.Main.Grids.TileItems
{
    public class StickMan : TileItem, ISelectable, ITweenContainerBind
    {
        private static readonly int mainTexColor = Shader.PropertyToID("_Color");
        private static readonly int outLineColor = Shader.PropertyToID("_OutlineColor");
        private static readonly int openDoorAnimTrig = Animator.StringToHash("OpenDoor");
        private static readonly int yellAnimTrig = Animator.StringToHash("Yell");
        private static readonly int isWalkingAnimBool = Animator.StringToHash("IsWalking");
        private static readonly int enableOutline = Shader.PropertyToID("_EnableOutline");
        private static readonly int outLineSize = Shader.PropertyToID("_OutlineSize");
        [SerializeField] private SkinnedMeshRenderer _myMeshRenderer;
        [SerializeField] private Animator _animator;
        [Inject] private GridAgentEvents GridAgentEvents { get; set; }
        [Inject] private GridEvents GridEvents { get; set; }
        private int LastAnimTrig { get; set; }
        private INavNode _currentTile;
        private Vector2Int? _destination;
        private bool _isWalking;
        private MaterialPropertyBlock _lastMaterialPropertyBlock;
        private Tween _selectionTween;
        private int _wpIndex;
        public ITweenContainer TweenContainer { get; set; }

        private void Awake()
        {
            TweenContainer = TweenContain.Install(this);
        }

        private void OnEnable()
        {
            RegisterEvents();
        }

        private void OnDisable()
        {
            UnRegisterEvents();
        }

        void ISelectable.Select()
        {
            if (_isWalking) return;

            MaterialPropertyBlock materialPropertyBlock = new();
            _lastMaterialPropertyBlock = materialPropertyBlock;
            _myMeshRenderer.GetPropertyBlock(_lastMaterialPropertyBlock);
            _selectionTween = DOVirtual.Float
            (
                0.1f,
                0.05f,
                0.5f,
                delegate(float value)
                {
                    _lastMaterialPropertyBlock.SetFloat(outLineSize, value);
                    _myMeshRenderer.SetPropertyBlock(_lastMaterialPropertyBlock);
                }
            );

            
            _selectionTween.SetLoops(-1, LoopType.Yoyo);
        }

        void ISelectable.DeSelect()
        {
            if (_selectionTween.IsActive()) _selectionTween.Kill();
        }

        void ISelectable.SetDest(Vector2Int targetAbleGridCoord)
        {
            SetDest(targetAbleGridCoord);
            _lastMaterialPropertyBlock.SetFloat(enableOutline, 0);
            _myMeshRenderer.SetPropertyBlock(_lastMaterialPropertyBlock);
        }

        private void SetTrigger(int trig)
        {
            _animator.ResetTrigger(LastAnimTrig);
            LastAnimTrig = trig;
            _animator.SetTrigger(trig);
        }

        [Button]
        private void OpenDoor(Transform doorTrans, float normTime = 0.3f)
        {
            SetTrigger(openDoorAnimTrig); // Trigger the door opening animation

            var weightMask = new MatchTargetWeightMask(new Vector3(0, 0, 0), 1);

            _animator.MatchTarget(
                doorTrans.position, 
                doorTrans.rotation, 
                AvatarTarget.RightHand, 
                weightMask,
                0, // Start time for matching in normalized time
                normTime // End time for matching in normalized time
            );
        }

        private void SetWalking(bool isWalking)
        {
            _animator.SetBool(isWalkingAnimBool, isWalking);
        }

        private void SetDest(Vector2Int targetAbleGridCoord)
        {
            _destination = targetAbleGridCoord;
            GridAgentEvents.DestinationSet?.Invoke(GridCoord, _destination.Value);
        }

        private void RegisterEvents()
        {
            GridAgentEvents.PathCalculated += OnPathCalculated;
            GridAgentEvents.ArrivedDest += OnArrivedDest;
        }

        private void OnArrivedDest(Vector2Int arg0)
        {
            SetCoord(arg0);
            GridEvents.TileItemMoveEnd?.Invoke(this);
            SetWalking(false);
        }

        private void OnPathCalculated(List<(Vector3, Vector2Int)> arg0)
        {
            if (arg0 != null)
            {
                GridEvents.TileItemMoveStart?.Invoke(this);
                SetWalking(true);
            }
        }

        private void UnRegisterEvents()
        {
            GridAgentEvents.PathCalculated -= OnPathCalculated;
            GridAgentEvents.ArrivedDest -= OnArrivedDest;
        }
    }
}