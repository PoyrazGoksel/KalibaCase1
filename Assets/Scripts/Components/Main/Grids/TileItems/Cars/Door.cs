using DG.Tweening;
using Extensions.DoTween;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Components.Main.Grids.TileItems.Cars
{
    public class Door : MonoBehaviour, ICarDoor, ITweenContainerBind
    {
        [SerializeField] private Transform _entranceTrans;
        [SerializeField] private Transform _myTrans;
        [SerializeField] private float _openRot = 90;
        [SerializeField] private Transform _seatTrans;
        private Vector3 _initRot;
        public Vector3 EntrancePoint => _entranceTrans.position;
        public Vector3 SeatPos => _seatTrans.position;
        public ITweenContainer TweenContainer { get; set; }

        private void Awake()
        {
            TweenContainer = TweenContain.Install(this);
        }

        private void Start()
        {
            _initRot = _myTrans.eulerAngles;
        }

        [Button]
        public void Open()
        {
            TweenContainer.Clear();
            TweenContainer.AddTween = _myTrans.DORotate(_initRot + Vector3.up * _openRot, 0.5f);
        }

        [Button]
        public void Close()
        {
            TweenContainer.Clear();
            TweenContainer.AddTween = _myTrans.DORotate(_initRot, 0.5f);
        }
    }

    public interface ICarDoor
    {
        Vector3 EntrancePoint { get; }
        Vector3 SeatPos { get; }
        void Open();
        void Close();
    }
}