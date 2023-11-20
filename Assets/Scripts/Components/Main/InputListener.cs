using Components.Main.Grids.TileItems;
using Components.Main.Grids.TileItems.Cars;
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
                        SetSelected(selectable);
                    }
                    else if (_selected != null && raycastHit.transform.TryGetComponent(out ITargetAbleCar targetAbleCar))
                    {
                        targetAbleCar.Target();
                        _selected.SetTarget(targetAbleCar.Target(), targetAbleCar.TileItemColor, targetAbleCar.GetDoors());
                        ConsumeSelected();
                    }
                    else if (_selected != null && raycastHit.transform.TryGetComponent(out ITargetAble targetAble))
                    {
                        _selected.SetDest(targetAble.Target());
                        ConsumeSelected();
                    }
                    else if (raycastHit.transform.TryGetComponent(out IClickAble clickAble))
                    {
                        clickAble.OnClick();
                    }
                }
                else
                {
                    ConsumeSelected();
                }
            }
        }

        private void SetSelected(ISelectable selectable)
        {
            _selected = selectable;

            _selected?.Select();
        }

        private void ConsumeSelected()
        {
            _selected?.DeSelect();
            _selected = null;
        }

        protected override void RegisterEvents()
        {
            CameraEvents.CameraStarted += OnCameraStarted;
            GridEvents.TileItemMoveStart += OnTileItemMoveStart;
            GridEvents.TileItemMoveEnd += OnTileItemMoveEnd;
            GridEvents.TileItemRemove += OnTileItemRemove;
        }

        private void OnCameraStarted(Camera arg0)
        {
            _sceneCam = arg0;
            _inputListenerRoutine.StartCoroutine();
        }

        private void OnTileItemMoveStart(TileItem tileItem)
        {
            _inputListenerRoutine.SetPaused(true);
        }

        private void OnTileItemMoveEnd(TileItem arg0)
        {
            _inputListenerRoutine.SetPaused(false);
        }

        private void OnTileItemRemove(TileItem arg0)
        {
            _inputListenerRoutine.SetPaused(false);
        }

        protected override void UnRegisterEvents()
        {
            CameraEvents.CameraStarted -= OnCameraStarted;
            GridEvents.TileItemMoveStart -= OnTileItemMoveStart;
            GridEvents.TileItemMoveEnd -= OnTileItemMoveEnd;
            GridEvents.TileItemRemove -= OnTileItemRemove;
        }
    }
}