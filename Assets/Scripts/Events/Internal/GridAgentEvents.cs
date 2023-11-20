using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Events.Internal
{
    public class GridAgentEvents
    {
        public UnityAction DestinationReached;
        public UnityAction<List<(Vector3, Vector2Int)>> PathCalculated;
        public UnityAction<Vector3, Quaternion> WalkUpdate;
        public UnityAction<Vector2Int> ArrivedWp;
        public UnityAction<Vector2Int> ArrivedDest;
        public UnityAction<Vector3> GoingNextWp;
    }
}