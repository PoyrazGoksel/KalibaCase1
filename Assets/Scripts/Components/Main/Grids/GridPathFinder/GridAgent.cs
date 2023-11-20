using System;
using System.Collections.Generic;
using System.Linq;
using Events.External;
using Events.Internal;
using Extensions.Unity;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using Zenject;

namespace Components.Main.Grids.GridPathFinder
{
    public class GridAgent : SerializedMonoBehaviour
    {
        private const float AgentErrorMargin = 0.1f;
        [SerializeField] private float rotateSpeed = 3f;
        [SerializeField] private Animator _animator;
        [Inject] private GridEvents GridEvents { get; set; }
        [Inject] private GridAgentEvents GridAgentEvents { get; set; }
        private List<(Vector3, Vector2Int)> _activePath;
        private(Vector3, Vector2Int) _currentWayPoint;
        [OdinSerialize] private PathFinder _myPathFinder;
        [OdinSerialize] private Transform _myTrans;
        private RoutineHelper _walkRoutine;
        [OdinSerialize] private float _walkSpeed = 3f;
        private int _wpIndex;
        private bool _isWalking;

        private void Awake()
        {
            _walkRoutine = new RoutineHelper(this, new WaitForFixedUpdate(), WalkUpdate);
            _myPathFinder = new PathFinder();
        }

        private void OnEnable()
        {
            RegisterEvents();
        }

        private void OnDisable()
        {
            UnRegisterEvents();
        }

        private void OnDrawGizmos()
        {
            if (_activePath != null)
            {
                foreach ((Vector3, Vector2Int) node in _activePath)
                {
                    Gizmos.DrawSphere(node.Item1, 0.4f);
                }
            }
        }

        private void WalkUpdate()
        {
            Vector3 walkDir = (_currentWayPoint.Item1 - transform.position).normalized;
            Vector3 nextPos = _myTrans.position + walkDir * Time.deltaTime * _walkSpeed;

            if (Vector3.Distance(nextPos, _currentWayPoint.Item1) < AgentErrorMargin)
            {
                if (GetNextWp() == false)
                {
                    GridAgentEvents.DestinationReached?.Invoke();
                    _walkRoutine.StopCoroutine();

                    _isWalking = false;

                    return;
                }
            }

            if (Vector3.Angle(_myTrans.forward, nextPos - _myTrans.position) > AgentErrorMargin)
            {
                Vector3 targetDirection = (nextPos - _myTrans.position).normalized;
                Vector3 newDirection = Vector3.RotateTowards(_myTrans.forward, targetDirection, 0.1f, Time.deltaTime * 1f);
                _myTrans.rotation = Quaternion.LookRotation(newDirection);
            }

            _myTrans.position = nextPos;

            GridAgentEvents.WalkUpdate?.Invoke(_myTrans.position, _myTrans.rotation);
        }

        private bool GetNextWp()
        {
            GridAgentEvents.ArrivedWp?.Invoke(_currentWayPoint.Item2);
            _wpIndex ++;

            if (_wpIndex >= _activePath.Count)
            {
                GridAgentEvents.ArrivedDest?.Invoke
                (
                    _activePath.Last()
                    .Item2
                );

                _activePath = new List<(Vector3, Vector2Int)>();

                return false;
            }

            _currentWayPoint = _activePath[_wpIndex];
            GridAgentEvents.GoingNextWp?.Invoke(_currentWayPoint.Item1);
            return true;
        }

        private void GenerateNavPlane(INavNode[,] navGrid)
        {
            _myPathFinder.GenerateEmptyGrid(navGrid.GetLength(0), navGrid.GetLength(1));

            for (int x = 0; x < navGrid.GetLength(0); x ++)
            for (int y = 0; y < navGrid.GetLength(1); y ++)
            {
                INavNode navNode = navGrid[x, y];

                _myPathFinder.AddNavNode(navNode);
            }
        }

        public List<(Vector3, Vector2Int)> SetDest(Vector2Int from, Vector2Int to)
        {
            if (_isWalking) return null;

            _activePath = _myPathFinder.FindPath(from, to);

            if (_activePath is {Count: > 0})
            {
                _isWalking = true;
                _wpIndex = 0;
                _currentWayPoint = _activePath[0];
                _walkRoutine.StartCoroutine();
            }

            return _activePath;
        }
        
        public bool CanSetDest(Vector2Int arg0, Vector2Int arg1)
        {
            if (_isWalking) return false;

            return _myPathFinder.FindPath(arg0, arg1) != null;
        }

        private void RegisterEvents()
        {
            GridEvents.GridStarted += OnGridStarted;
        }

        private void OnGridStarted(INavNode[,] navGrid)
        {
            GenerateNavPlane(navGrid);
        }

        private void UnRegisterEvents()
        {
            GridEvents.GridStarted -= OnGridStarted;
        }
    }

    public interface IHeapItem<T> : IComparable<T>
    {
        int HeapIndex { get; set; }
    }
}