#if UNITY_EDITOR

using UnityEngine;

namespace Enigmatic.Core.Editor
{
    public class ScrollView : GUIGroup
    {
        public Rect ViewRect { get; private set; }
        public Vector2 ScrollPosition { get; private set; }

        public bool AlwaysShowHorizontal { get; private set; }
        public bool AlwaysShowVertical { get; private set; }

        public GUIStyle HorizontalScrollbar { get; private set; }
        public GUIStyle VerticalScrollbar { get; private set; }
        public GUIStyle Background { get; private set; }

        public ScrollView(Rect rect, Rect viewRect, Vector2 scrollPosition, bool alwaysShowHorizontal,
            bool alwaysShowVertical, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar,
            GUIStyle background, GroupSortType sortType)
            : base(rect, sortType)
        {
            Rect.position = Vector2.zero;
            ViewRect = viewRect;
            ScrollPosition = scrollPosition;

            AlwaysShowHorizontal = alwaysShowHorizontal;
            AlwaysShowVertical = alwaysShowVertical;

            HorizontalScrollbar = horizontalScrollbar;
            VerticalScrollbar = verticalScrollbar;
            Background = background;
        }

        public void UpdatePosition()
        {
            Rect.position += ScrollPosition;
        }
    }
}

#endif