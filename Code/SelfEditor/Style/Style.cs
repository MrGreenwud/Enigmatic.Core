using System;
using System.Collections.Generic;

using UnityEngine;

namespace Enigmatic.Core.Editor.Style
{
    [Serializable]
    public class Style
    {
        [SerializeField] private List<GUIStyle> m_GUIStyles = new List<GUIStyle>();
        private Dictionary<string, GUIStyle> m_GUIStylesRegisters;

        public void ReginsterGUIStyles()
        {
            m_GUIStylesRegisters = new Dictionary<string, GUIStyle>();

            foreach (GUIStyle style in m_GUIStyles)
                m_GUIStylesRegisters.Add(style.name, style);
        }

        public int IndexOf(GUIStyle style)
        {
            return m_GUIStyles.IndexOf(style);
        }

        public void AddGUIStyle(GUIStyle style)
        {
            m_GUIStyles.Add(style);
        }

        public void RemoveGUIStyle(int index)
        {
            m_GUIStyles.Remove(m_GUIStyles[index]);
        }

        public void Rename(int index, string newName)
        {
            m_GUIStyles[index].name = newName;
        }

        public void ForEach(Action<GUIStyle> action)
        {
            foreach (GUIStyle style in m_GUIStyles)
                action?.Invoke(style);
        }

        public GUIStyle GetGUIStyle(string name)
        {
            if (m_GUIStylesRegisters.ContainsKey(name) == false)
                return null;

            return m_GUIStylesRegisters[name];
        }
    }
}