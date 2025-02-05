#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace Enigmatic.Core.Editor
{
    internal class GUIGroupPool
    {
        private Queue<GUIGroup> m_Pool = new Queue<GUIGroup>();
        public float size;

        public GUIGroup GetGUIGroup(Rect rect, GroupSortType GroupSortType)
        {
            if (m_Pool.Count == 0)
                return new GUIGroup(rect, GroupSortType);

            GUIGroup gUIGroup = m_Pool.Dequeue();
            gUIGroup.Init(rect, GroupSortType);
            m_Pool.Enqueue(gUIGroup);

            return gUIGroup;
        }

        public void RemoveGroup(GUIGroup group)
        {
            if (group == null || m_Pool.Count >= 100)
                return;

            Debug.Log(m_Pool.Count);

            m_Pool.Enqueue(group);
        }
    }
}

#endif