using System;
using System.Collections.Generic;

namespace Enigmatic.Core
{
    public static class PostActionCallerPool
    {
        private static Queue<DeferredActionCaller> sm_Callers = new Queue<DeferredActionCaller>();

        public static DeferredActionCaller GetCaller(Action<object[]> action, params object[] parameters)
        {
            if (sm_Callers.Count == 0)
                sm_Callers.Enqueue(new DeferredActionCaller());

            DeferredActionCaller caller = sm_Callers.Dequeue();
            caller.Init(parameters, action);
            return caller;
        }

        public static void ReturnCaller(DeferredActionCaller caller)
        {
            if (sm_Callers.Count > 100)
                return;

            sm_Callers.Enqueue(caller);
        }
    }
}