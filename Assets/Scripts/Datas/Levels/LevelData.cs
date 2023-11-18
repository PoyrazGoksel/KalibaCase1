using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using Utils;

namespace Datas.Levels
{
    [CreateAssetMenu
    (
        fileName = nameof(LevelData),
        menuName = EnvironmentVariables.Project + "/" + nameof(LevelData),
        order = 0
    )]
    public partial class LevelData : SerializedScriptableObject, IGridAccess
    {
        private string _path;

#if UNITY_EDITOR
        [TableMatrix
        (
            DrawElementMethod = nameof(DrawGridPreview),
            SquareCells = true,
            ResizableColumns = false
        )]
#endif
        [OdinSerialize]
        private TileData[,] _tileGridData = new TileData[0,0];


        TileData[,] IGridAccess.GridDataClone => (TileData[,])_tileGridData.Clone();
    }
    
    public interface IGridAccess
    {
        TileData[,] GridDataClone { get; }
    }
}