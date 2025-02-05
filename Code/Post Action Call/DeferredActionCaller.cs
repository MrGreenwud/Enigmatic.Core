using System;

namespace Enigmatic.Core
{
    public class DeferredActionCaller
    {
        protected object[] Parameters { get; private set; }
        private Action<object[]> m_Action;

        public void Init(object[] parameters, Action<object[]> action)
        {
            Parameters = parameters;
            m_Action = action;
        }

        public virtual void Call()
        {
            m_Action?.Invoke(Parameters);
        }
    }
}