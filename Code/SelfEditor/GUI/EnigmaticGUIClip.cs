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

        public static void BeginClip(Rect rect)
        {
            sm_ClipedAreas.Add(rect.position);
            GUIPostDrawQueue.Enqueue((x) => { GUI.BeginClip(rect); });
        }

        public static void EndClip()
        {
            sm_ClipedAreas.Remove(sm_ClipedAreas.Last());
            GUIPostDrawQueue.Enqueue((x) => { GUI.EndClip(); });
        }

        public static Rect UnClip(Rect rect)
        {
            Rect result = new Rect(UnClip(rect.position), rect.size);
            return result;
        }

        public static Vector2 UnClip(Vector2 position)
        {
            Vector2 result = Vector2.zero;

            foreach (Vector2 clipedArea in sm_ClipedAreas)
                result += clipedArea;

            result += position;
            return result;
        }
    }
}

#endif