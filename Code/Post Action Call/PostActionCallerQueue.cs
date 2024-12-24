using System;
using System.Collections.Generic;

namespace Enigmatic.Core
{
    public class PostActionCallerQueue
    {
        private Queue<PostActionCaller> m_CallQueue = new Queue<PostActionCaller>();

        public void Enqueue(Action<object[]> action, params object[] parameters)
        {
            PostActionCaller caller = PostActionCallerPool.GetCaller(action, parameters);
            m_CallQueue.Enqueue(caller);
        }

        public void Enqueue(PostActionCaller caller)
        {
            m_CallQueue.Enqueue(caller);
        }

        public void CallAll()
        {
            while (m_CallQueue.Count > 0)
            {
                PostActionCaller caller = m_CallQueue.Dequeue();
                caller.Call();

                PostActionCallerPool.ReturnCaller(caller);
            }

            m_CallQueue.Clear();
        }
    }
}