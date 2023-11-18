using System.Collections.Generic;
using System.Linq;
using Components.Main.Grids;
using Components.Main.Grids.GridPathFinder;
using Events.External;
using Extensions.System;
using Extensions.Unity;
using Extensions.Unity.MonoHelper;
using UnityEngine;
using Zenject;

namespace Components.Main
{
    public class SceneCam : EventListenerMono
    {
        [SerializeField] private Camera _myCam;
        [SerializeField] private Transform _transform;
        [Inject] private CameraEvents CameraEvents { get; set; }
        [Inject] private GridEvents GridEvents { get; set; }

        
        private void Start()
        {
            CameraEvents.CameraStarted?.Invoke(_myCam);
        }

        protected override void RegisterEvents()
        {
            GridEvents.GridStarted += OnGridStarted;
        }

        private void OnGridStarted(INavNode[,] arg0)
        {
            _myCam.Encapsulate(_transform, arg0.ToList().Select(e => e.WPos).ToList(), (-10f * Vector3.forward) + (10f * Vector3.up));
        }



        protected override void UnRegisterEvents()
        {
            GridEvents.GridStarted -= OnGridStarted;
        }
    }
}