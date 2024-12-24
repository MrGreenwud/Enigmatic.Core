using System;
using UnityEngine;

namespace Enigmatic.Core.Editor.Style
{
    public class StyleContainer : ScriptableObject
    {
        public string Tag;

        [SerializeField] private Style m_Dark;
        [SerializeField] private Style m_Light;

        public void ReginsterGUIStyles()
        {
            m_Dark.ReginsterGUIStyles();
            m_Light.ReginsterGUIStyles();
        }

        public GUIStyle GetGUIStyle(bool isDark, string name)
        {
            if(isDark)
                return m_Dark.GetGUIStyle(name);
            else
                return m_Light.GetGUIStyle(name);
        }

        internal void Construct()
        {
            m_Dark = new Style();
            m_Light = new Style();
        }

        internal void AddGUIStyle(GUIStyle preset)
        {
            m_Dark.AddGUIStyle(new GUIStyle(preset));
            m_Light.AddGUIStyle(new GUIStyle(preset));
        }

        internal void RemoveGUIStyle(int index)
        {
            m_Dark.RemoveGUIStyle(index);
            m_Light.RemoveGUIStyle(index);
        }

        internal void RemoveGUIStyle(bool isDark, GUIStyle style)
        {
            int index;

            if (isDark)
                index = m_Dark.IndexOf(style);
            else
                index = m_Light.IndexOf(style);

            m_Dark.RemoveGUIStyle(index);
            m_Light.RemoveGUIStyle(index);
        }

        internal void Rename(int index, string newName)
        {
            m_Dark.Rename(index, newName);
            m_Light.Rename(index, newName);
        }

        internal int IndexOf(bool isDark, GUIStyle style)
        {
            if(isDark)
                return m_Dark.IndexOf(style);
            else
                return m_Light.IndexOf(style);
        }

        internal void ForEach(bool isDark, Action<GUIStyle> action)
        {
            if(isDark)
                m_Dark.ForEach(action);
            else
                m_Light.ForEach(action);
        }
    }
}
