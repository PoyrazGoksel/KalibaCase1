using UnityEngine;

namespace Components.Main.Grids.TileItems
{
    public interface ISelectable
    {
        void Select();

        void DeSelect();

        void SetDest(Vector2Int targetAbleGridCoord);
    }
}