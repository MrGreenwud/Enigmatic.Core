#if UNITY_EDITOR

using System;
using System.Collections.Generic;

namespace Enigmatic.Core.Editor
{
    internal static class GUIValueCasher
    {
        private static Dictionary<int, object> sm_ValueCashed = new Dictionary<int, object>();

        public static void CashValue(int guid, object value)
        {
            if (sm_ValueCashed.ContainsKey(guid))
                sm_ValueCashed[guid] = value;
            else
                sm_ValueCashed.Add(guid, value);
        }

        public static object GetValue(int guid)
        {
            if (sm_ValueCashed.ContainsKey(guid) == false)
                throw new Exception("");

            object value = sm_ValueCashed[guid];
            sm_ValueCashed.Remove(guid);
            return value;
        }

        public static bool TryGetValue(int guid, out object value)
        {
            value = null;

            if (sm_ValueCashed.ContainsKey(guid) == false)
                return false;

            value = sm_ValueCashed[guid];
            sm_ValueCashed.Remove(guid);
            return true;
        }

        public static bool TryGetValue<T>(int guid, out T value)
        {
            bool result = TryGetValue(guid, out object valueObj);
            value = (T)valueObj;

            return result;
        }

        public static void Clear() => sm_ValueCashed.Clear();
    }
}

#endif