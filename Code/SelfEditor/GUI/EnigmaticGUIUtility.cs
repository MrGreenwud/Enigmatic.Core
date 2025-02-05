#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Enigmatic.Core.Editor
{
    public static class EnigmaticGUIUtility
    {
        public static bool OnClick(Rect rect, int mouseButton)
        {
            Event e = Event.current;

            if (e.type == EventType.MouseUp && e.button == mouseButton)
            {
                if (EnigmaticGUIClip.UnClip(rect).Contains(e.mousePosition))
                    return true;
            }

            return false;
        }

        public static bool IsHover(Rect rect)
        {
            Event e = Event.current;
            return rect.Contains(e.mousePosition);
        }

        public static bool TryGetActiveWindow(out EditorWindow window)
        {
            window = GetActiveWindow();
            return window != null;
        }

        public static EditorWindow GetActiveWindow()
        {
            EditorWindow[] windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            Event e = Event.current;

            Vector2 mousePosition = GUIUtility.GUIToScreenPoint(e.mousePosition);

            foreach (EditorWindow window in windows)
            {
                if (window.position.Contains(mousePosition))
                    return window;
            }

            return null;
        }

        public static GUIStyle GetHasSelected(bool condition, GUIStyle unSelectedStyle, GUIStyle selectedStyle)
        {
            if (condition)
                return selectedStyle;

            return unSelectedStyle;
        }

        public static void Repaint()
        {
            EditorWindow.focusedWindow?.Repaint();
        }
    }
}

#endif