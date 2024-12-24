#if UNITY_EDITOR

using UnityEngine;

namespace Enigmatic.Core.Editor
{
    public class GUIElement
    {
        public Rect Rect;

        public GUIElement() { }

        public GUIElement(Rect rect)
        {
            Rect = rect;
        }

        public virtual void Init(Rect rect)
        {
            Rect = rect;
        }
    }
}

#endif