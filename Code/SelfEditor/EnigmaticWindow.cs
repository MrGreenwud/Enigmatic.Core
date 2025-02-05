#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Enigmatic.Core.Editor
{
    public class EnigmaticWindow : EditorWindow
    {
        protected bool IsInit { get; private set; }
        protected Rect Rect { get; private set; }

        private Vector2 m_Size;

        private void OnEnable()
        {
            IsInit = false;
        }

        private void OnDisable()
        {
            OnClose();
        }

        private void OnGUI()
        {
            if (IsInit == false)
            {
                OnOpen();
                IsInit = true;
            }

            EditorInput.UpdateInput();
            EnigmaticGUILayout.Reset();

            if (m_Size != position.size)
            {
                m_Size = position.size;
                OnResize();
            }

            EnigmaticGUILayout.BeginVertical(EnigmaticGUILayout.Width(position.width), 
                EnigmaticGUILayout.Height(position.height), EnigmaticGUILayout.ElementSpacing(0), 
                EnigmaticGUILayout.Padding(0));
            {
                Rect = EnigmaticGUILayout.GetActiveGroup().Rect;
                OnDraw();
            }
            EnigmaticGUILayout.EndVertical();
        }

        protected virtual void OnOpen() { }

        protected virtual void OnClose() { }

        protected virtual void OnDraw() { }

        protected virtual void OnResize() { }
    }
}

#endif