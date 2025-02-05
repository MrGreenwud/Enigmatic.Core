#if UNITY_EDITOR

using UnityEngine;

namespace Enigmatic.Core.Editor
{
    public class GUIgUID
    {
        private static System.Random sm_Random = new System.Random();

        public static int Next()
        {
            return sm_Random.Next(int.MinValue, int.MaxValue);
        }

        public static int Next(int guid)
        {
            if (guid == 0)
                return Next();

            return guid;
        }

        public static int Next(string guidControlName, Rect rect, object value)
        {
            string combinedInput = $"{guidControlName}_{rect.x}_{rect.y}_{rect.width}_{rect.height}_{value}";
            return combinedInput.GetHashCode();
        }
    }
}

#endif