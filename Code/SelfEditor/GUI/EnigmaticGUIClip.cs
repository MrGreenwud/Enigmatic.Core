#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Enigmatic.Core.Editor
{
    public static class EnigmaticGUIClip
    {
        private static List<Vector2> sm_ClipedAreas = new List<Vector2>();

        public static Action BeginClip(Rect rect)
        {
            sm_ClipedAreas.Add(rect.position);
            return () => GUI.BeginClip(rect);
        }

        public static Action EndClip()
        {
            sm_ClipedAreas.Remove(sm_ClipedAreas.Last());
            return () => GUI.EndClip();
        }

        public static Rect UnClip(Rect rect)
        {
            Rect result = new Rect(Vector2.zero, rect.size);

            foreach (Vector2 clipedArea in sm_ClipedAreas)
                result.position += clipedArea;

            result.position += rect.position;
            return result;
        }
    }
}

#endif