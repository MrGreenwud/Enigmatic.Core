using System;
using System.Collections.Generic;

namespace Enigmatic.Core
{
    public static class PostActionCallerPool
    {
        private static Queue<PostActionCaller> sm_Callers = new Queue<PostActionCaller>();

        public static PostActionCaller GetCaller(Action<object[]> action, params object[] parameters)
        {
            if (sm_Callers.Count == 0)
                sm_Callers.Enqueue(new PostActionCaller());

            PostActionCaller drawer = sm_Callers.Dequeue();
            drawer.Init(parameters, action);
            return drawer;
        }

        public static void ReturnCaller(PostActionCaller caller)
        {
            if (sm_Callers.Count > 100)
                return;

            sm_Callers.Enqueue(caller);
        }
    }
}