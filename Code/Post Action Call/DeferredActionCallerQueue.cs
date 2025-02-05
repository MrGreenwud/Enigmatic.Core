using System;
using System.Collections.Generic;

namespace Enigmatic.Core
{
    public class DeferredActionCallerQueue
    {
        private Queue<DeferredActionCaller> m_CallQueue = new Queue<DeferredActionCaller>();

        public void Enqueue(Action<object[]> action, params object[] parameters)
        {
            DeferredActionCaller caller = PostActionCallerPool.GetCaller(action, parameters);
            m_CallQueue.Enqueue(caller);
        }

        public void Enqueue(DeferredActionCaller caller)
        {
            m_CallQueue.Enqueue(caller);
        }

        public void CallAll()
        {
            while (m_CallQueue.Count > 0)
            {
                DeferredActionCaller caller = m_CallQueue.Dequeue();
                caller.Call();

                PostActionCallerPool.ReturnCaller(caller);
            }

            m_CallQueue.Clear();
        }
    }
}