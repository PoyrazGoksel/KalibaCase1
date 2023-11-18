using UnityEngine;

namespace Extensions.System
{
    public static class IntExt
    {
        public static int Sign(this int val)
        {
            return val > 0 ? 1 : -1;
        }
        
        public static int Negate(this int val)
        {
            return -1 * val;
        }
    }
}