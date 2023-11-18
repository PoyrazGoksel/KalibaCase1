using Components.Main;
using Components.Main.Grids;
using UnityEngine;

namespace Datas.Settings
{
    [CreateAssetMenu(fileName = nameof(TileSettings), menuName = nameof(TileSettings), order = 0)]
    public class TileSettings : ScriptableObject
    {
        [SerializeField] private Tile.Settings _settings;
        public Tile.Settings Settings => _settings;
    }
}