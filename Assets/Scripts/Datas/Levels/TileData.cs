using System;
using UnityEngine;

namespace Datas.Levels
{
    [Serializable]
    public class TileData
    {
        [SerializeField] private GameObject _prefab;
        [SerializeField] private Vector2Int _coord;
        [SerializeField] private TileItemData _tileItemData;
        [SerializeField] public TileItemData OverlappingTileItemData;
        public GameObject Prefab => _prefab;
        public Vector2Int Coord => _coord;
        public TileItemData TileItemData => _tileItemData;

        public void SetTileItem(TileItemData tileItemData)
        {
            _tileItemData = tileItemData;
        }
        
        public TileData(GameObject prefab, Vector2Int coord)
        {
            _prefab = prefab;
            _coord = coord;
        }
    }
}