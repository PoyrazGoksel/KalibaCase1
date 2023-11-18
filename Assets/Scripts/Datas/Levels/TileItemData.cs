using System;
using Components.Main.Grids;
using UnityEngine;

namespace Datas.Levels
{
    [Serializable]
    public class TileItemData
    {
        [SerializeField] private GameObject _prefab;
        [SerializeField] private Vector2Int _coord;
        [SerializeField] private GridRot _gridRotation;
        [SerializeField] private int _size;
        
        public GameObject Prefab => _prefab;
        public Vector2Int Coord => _coord;
        public int Size => _size;
        public GridRot GridRot => _gridRotation;

        public bool IsPivot(int x, int y) => IsPivot(new Vector2Int(x, y));
        public bool IsPivot(Vector2Int gridCoord)
        {
            return _coord == gridCoord;
        }

        public TileItemData(GameObject prefab, Vector2Int coord, int size, GridRot gridRot)
        {
            _prefab = prefab;
            _coord = coord;
            _size = size;
            _gridRotation = gridRot;
        }

        public void RotateClockwise()
        {
            int gridRotsLength = Enum.GetValues(typeof(GridRot))
            .GetLength(0);
        
            int currRot = (int)_gridRotation + 1;
            
            GridRot newClockwise = (GridRot)(currRot % gridRotsLength);
            Rotate(newClockwise);
        }
        
        public void Rotate(GridRot newGridRot)
        {
            _gridRotation = newGridRot;
        }
    }
}