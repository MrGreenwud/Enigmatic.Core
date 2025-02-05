#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace Enigmatic.Core.Editor
{
    internal class GUIElementPool
    {
        private Queue<GUIElement> m_ElementsPool = new Queue<GUIElement>();

        public GUIElement GetGUIElement(Rect rect)
        {
            if (m_ElementsPool.Count == 0)
                return new GUIElement(rect);

            GUIElement element = m_ElementsPool.Dequeue();
            element.Init(rect);
            m_ElementsPool.Enqueue(element);

            return element;
        }

        public void RemoveElement(GUIElement element)
        {
            if (element == null || m_ElementsPool.Count == 100)
                return;

            m_ElementsPool.Enqueue(element);
        }
    }
}

#endif