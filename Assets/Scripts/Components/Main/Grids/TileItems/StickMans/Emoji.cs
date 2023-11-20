using Events.External;
using Extensions.DoTween;
using Extensions.Unity.MonoHelper;
using UnityEngine;
using Zenject;

namespace Components.Main.Grids.TileItems.StickMans
{
    public abstract class Emoji : EventListenerMono, ITweenContainerBind
    {
        [SerializeField] protected SpriteRenderer _spriteRenderer;
        [SerializeField] protected Transform _transform;
        protected Vector3 LookCamRot;
        [Inject] private CameraEvents CameraEvents { get; set; }
        public ITweenContainer TweenContainer { get; set; }

        private void Awake()
        {
            TweenContainer = TweenContain.Install(this);
            _spriteRenderer.enabled = false;
        }

        public abstract void DoAnim();

        protected override void RegisterEvents()
        {
            CameraEvents.CameraStarted += OnCameraStarted;
        }

        private void OnCameraStarted(Camera arg0)
        {
            LookCamRot = -1f * arg0.transform.eulerAngles;
        }

        protected override void UnRegisterEvents()
        {
            CameraEvents.CameraStarted -= OnCameraStarted;
            TweenContainer.Clear();
        }
    }
}