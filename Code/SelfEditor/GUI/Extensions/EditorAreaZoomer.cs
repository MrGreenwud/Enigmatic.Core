#if UNITY_EDITOR

using System;
using UnityEngine;

namespace Enigmatic.Core.Editor
{
    public static class EditorAreaZoomer
    {
        private readonly static int s_WindowTopPadding = 20;

        public static Matrix4x4 s_GUIMatrixOrigin;
        public static Matrix4x4 s_CurrentMatrix;

        public static void BeginZoomedArea(float zoomScale, Rect zoomendArea)
        {
            GUI.EndGroup();

            Rect clippedArea = zoomendArea.ScaleSizeBy(1.0f / zoomScale, zoomendArea.TopLeft());
            clippedArea.y += s_WindowTopPadding;
            GUI.BeginGroup(clippedArea);

            s_GUIMatrixOrigin = GUI.matrix;
            Matrix4x4 translation = Matrix4x4.TRS(clippedArea.TopLeft(), Quaternion.identity, Vector3.one);
            Matrix4x4 scale = Matrix4x4.Scale(new Vector3(zoomScale, zoomScale, 1.0f));
            GUI.matrix = translation * scale * translation.inverse * GUI.matrix;
            s_CurrentMatrix = GUI.matrix;
        }

        public static void EndZoomedArea()
        {
            GUI.matrix = s_GUIMatrixOrigin;
            GUI.EndGroup();
            GUI.BeginGroup(new Rect(0.0f, s_WindowTopPadding, Screen.width, Screen.height));
        }

        public static void ZoomedAndMoveZoomedArea(Rect zoomedArea, ref float zoomScale, ref Vector2 zoomCordsOrigin)
        {
            Vector2 mousePosition = Event.current.mousePosition;

            if (zoomedArea.Contains(mousePosition) == false)
                return;

            if (Event.current.type == EventType.ScrollWheel)
            {
                float oldZoom = zoomScale;
                float zoom = zoomScale;

                Vector2 screenCoordsMousePosition = zoomedArea.size / 2;

                Vector2 zoomCordsMousePosition = ConvertScreenCoordsToZoomCoords(
                    screenCoordsMousePosition, zoomedArea, zoomScale, zoomCordsOrigin);

                zoom -= 0.1f * zoomScale * Math.Sign(Event.current.delta.y);

                zoomScale = Math.Clamp(zoom, 0.2f, 3f);

                zoomCordsOrigin += (zoomCordsMousePosition - zoomCordsOrigin)
                    - (oldZoom / zoomScale) * (zoomCordsMousePosition - zoomCordsOrigin);

                EnigmaticGUIUtility.Repaint();
            }

            if (EditorInput.GetMouseDrag(2))
            {
                zoomCordsOrigin -= Event.current.delta / zoomScale;
                EnigmaticGUIUtility.Repaint();
            }
        }

        public static Vector2 ConvertScreenCoordsToZoomCoords(Vector2 screenCoords,
            Rect zoomedArea, float zoomScale, Vector2 zoomCoordsOrigin)
        {
            return (screenCoords - zoomedArea.TopLeft()) / zoomScale + zoomCoordsOrigin;
        }

        public static Vector2 ConvertZoomCoordsToScreenCoords(Vector2 zoomCoords,
            Rect zoomedArea, float zoomScale, Vector2 zoomCoordsOrigin)
        {
            return (zoomCoords - zoomCoordsOrigin) * zoomScale + zoomedArea.TopLeft();
        }
    }
}

#endif