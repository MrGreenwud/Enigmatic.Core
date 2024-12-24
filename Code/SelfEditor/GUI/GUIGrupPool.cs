#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace Enigmatic.Core.Editor
{
    public class GUIGrupPool
    {
        private Queue<GUIGrup> m_Pool = new Queue<GUIGrup>();
        public float size;

        public GUIGrup GetGUIGrup(Rect rect, GrupSortType grupSortType)
        {
            if (m_Pool.Count == 0)
                return new GUIGrup(rect, grupSortType);

            GUIGrup gUIGrup = m_Pool.Dequeue();
            gUIGrup.Init(rect, grupSortType);
            m_Pool.Enqueue(gUIGrup);

            return gUIGrup;
        }

        public void RemoveGrup(GUIGrup grup)
        {
            if (grup == null || m_Pool.Count == 100)
                return;

            Debug.Log(m_Pool.Count);

            m_Pool.Enqueue(grup);
        }
    }
}

#endif