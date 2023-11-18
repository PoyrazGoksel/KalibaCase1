using UnityEngine;

namespace Extensions.Unity
{
    public static class Vector2IntExt
    {
        public static Vector3Int Transpose(this Vector2Int vector2Int)
        {
            return new Vector3Int(vector2Int.x, 0, vector2Int.y);
        }
        
    }
}