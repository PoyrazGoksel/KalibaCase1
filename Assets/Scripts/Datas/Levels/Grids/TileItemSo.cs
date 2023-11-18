using UnityEngine;

namespace Datas.Levels.Grids
{
    [CreateAssetMenu(fileName = nameof(TileItemSo), menuName = "Kaliba/" + nameof(TileItemSo), order = 0)]
    public class TileItemSo : PrefabPickableSo
    {
        [SerializeField] private int _gridSize;
        public int GridSize => _gridSize;
    }
}