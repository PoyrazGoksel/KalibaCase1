using Extensions.Unity;
using Extensions.Unity.MonoHelper;
using UnityEngine;

namespace Components.Main.Grids.TileItems
{
    public abstract class TileItem : EventListenerMono, IGridTileItem
    {
        [SerializeField] protected Transform _myTransform;
        [SerializeField] private Vector3 _wPos;
        [SerializeField] private GridRot _gridRotation;
        [SerializeField] private Vector2Int _gridCoord;
        [SerializeField] private int _gridSize;
        [SerializeField] private TileItemColor _tileItemColor;
        public TileItemColor TileItemColor => _tileItemColor;
        
        public Vector2Int GridCoord => _gridCoord;
        public Vector3 WPos => _wPos;
        public GridRot GridRotation => _gridRotation;
        public int GridSize => _gridSize;

        void IGridTileItem.Construct
        (TransformEncapsulated tileTransformEncapsulated, Vector2Int gridCoord, GridRot gridRot, int gridSize) =>
        Construct(tileTransformEncapsulated, gridCoord, gridRot, gridSize);
        private void Construct(TransformEncapsulated tileTransformEncapsulated, Vector2Int gridCoord, GridRot gridRot, int gridSize)
        {
            _gridCoord = gridCoord;
            _wPos = tileTransformEncapsulated.position;
            _myTransform.eulerAngles = GridF.GetWorldRotY(gridRot);
            _gridRotation = gridRot;
            _gridSize = gridSize;
                                
            transform.position = tileTransformEncapsulated.position;
            transform.SetParent(tileTransformEncapsulated);
        }

        protected void SetCoord(Vector2Int coord)
        {
            _gridCoord = coord;
        }
    }

    public interface IGridTileItem
    {
        void Construct
        (TransformEncapsulated tileTransformEncapsulated, Vector2Int gridCoord, GridRot gridRot, int gridSize);
    }
}