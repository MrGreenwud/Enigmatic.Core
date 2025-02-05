#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Enigmatic.Core.Editor
{
    public static class DragAndDropHandler
    {
        public static bool Handel(Rect rect, out string[] paths)
        {
            paths = null;

            if (rect.Contains(EditorInput.GetMousePosition()) == false)
                return false;

            if (EditorInput.Current.type == EventType.DragUpdated 
                || EditorInput.Current.type == EventType.DragPerform)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if(EditorInput.Current.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    paths = DragAndDrop.paths;
                    return true;
                }
            }

            return false;
        }
    }
}

#endif