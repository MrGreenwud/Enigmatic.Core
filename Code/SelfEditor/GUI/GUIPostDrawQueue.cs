#if UNITY_EDITOR

using System;

namespace Enigmatic.Core.Editor
{
    internal static class GUIPostDrawQueue
    {
        private static DeferredActionCallerQueue sm_CallQueue = new DeferredActionCallerQueue();

        public static void Enqueue(Action<object[]> action, params object[] parameters)
        {
            sm_CallQueue.Enqueue(action, parameters);
        }

        public static void Enqueue(DeferredActionCaller caller)
        {
            sm_CallQueue.Enqueue(caller);
        }

        public static void CallAll()
        {
            sm_CallQueue.CallAll();
        }
    }
}

#endif