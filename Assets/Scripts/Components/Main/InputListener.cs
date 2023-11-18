using Components.Main.Grids.TileItems;
using Events.External;
using Extensions.Unity;
using Extensions.Unity.MonoHelper;
using UnityEngine;
using Zenject;

namespace Components.Main
{
    public class InputListener : EventListenerMono
    {
        [Inject] private CameraEvents CameraEvents { get; set; }
        [Inject] private GridEvents GridEvents { get; set; }
        private RoutineHelper _inputListenerRoutine;
        private Camera _sceneCam;
        private ISelectable _selected;

        private void Awake()
        {
            _inputListenerRoutine = new RoutineHelper(this, null, InputListenerUpdate);
        }

        private void InputListenerUpdate()
        {
            Vector3 mousePosition = Input.mousePosition; 

            if (Input.GetMouseButtonDown(0))
            {
                Vector3 zOffMousePos = mousePosition + (10f * Vector3.forward);
                Ray inputRay = _sceneCam.ScreenPointToRay(zOffMousePos);

                if (Physics.Raycast(inputRay, out RaycastHit raycastHit))
                {
                    if (raycastHit.transform.TryGetComponent(out ISelectable selectable))
                    {
                        selectable.Select();
                        _selected = selectable;
                    }
                    else if (raycastHit.transform.TryGetComponent(out ITargetAble targetAble))
                    {
                        if (_selected != null)
                        {
                            targetAble.Target();
                            _selected.SetDest(targetAble.GridCoord);
                        }
                    }
                    else
                    {
                        _selected = null;
                    }
                }
                
            }
        }

        protected override void RegisterEvents()
        {
            CameraEvents.CameraStarted += OnCameraStarted;
            GridEvents.TileItemMoveStart += OnTileItemMoveStart;
            GridEvents.TileItemMoveEnd += OnTileItemMoveEnd;
        }

        private void OnTileItemMoveStart(TileItem tileItem)
        {
            _inputListenerRoutine.SetPaused(true);
        }

        private void OnTileItemMoveEnd(TileItem arg0)
        {
            _inputListenerRoutine.SetPaused(false);
        }

        private void OnCameraStarted(Camera arg0)
        {
            _sceneCam = arg0;
            _inputListenerRoutine.StartCoroutine();
        }

        protected override void UnRegisterEvents()
        {
            CameraEvents.CameraStarted -= OnCameraStarted;
            GridEvents.TileItemMoveStart -= OnTileItemMoveStart;
            GridEvents.TileItemMoveEnd -= OnTileItemMoveEnd;
        }
    }
}