#if UNITY_EDITOR

using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace Enigmatic.Core.Editor 
{
    public static class DrawingUtilities
    {
        public static void ColumnTitle(GUIStyle background, string title, float width, bool drawAddRemoveButton = false,
            float buttonSpace = 0, Action addAction = null, Action removeAction = null, Action additinalDraw = null)
        {
            GUIStyle titleLable = new GUIStyle(EditorStyles.miniBoldLabel);
            titleLable.fontSize = 12;

            EnigmaticGUILayout.BeginHorizontal(background, EnigmaticGUILayout.Width(width),
                EnigmaticGUILayout.Height(22), EnigmaticGUILayout.Padding(0));
            {
                EnigmaticGUILayout.Space(2);
                EnigmaticGUILayout.Lable(title, titleLable);

                EnigmaticGUILayout.Space(buttonSpace);

                if (drawAddRemoveButton)
                {
                    if (EnigmaticGUILayout.ButtonCentric("-", new Vector2(22, 20), new Vector2(0, 1), EnigmaticStyles.toolbarButton))
                        removeAction?.Invoke();

                    EnigmaticGUILayout.Space(-6);

                    if (EnigmaticGUILayout.ButtonCentric("+", new Vector2(22, 20), new Vector2(0, 1), EnigmaticStyles.toolbarButton))
                        addAction?.Invoke();
                }

                additinalDraw?.Invoke();
            }
            EnigmaticGUILayout.EndHorizontal();
        }

        public static void BeginToolBar(float width)
        {
            EnigmaticGUILayout.BeginHorizontal(EditorStyles.toolbar, EnigmaticGUILayout.Padding(0),
                EnigmaticGUILayout.ElementSpacing(0), EnigmaticGUILayout.Width(width),EnigmaticGUILayout.Height(21));
        }

        public static void EndToolBar()
        {
            EnigmaticGUILayout.EndHorizontal();
        }

        public static void BeginColum(float width, float height, float elementSpacing = 0, params EnigmaticGUILayoutOption[] options)
        {
            EnigmaticGUILayout.BeginVertical(EnigmaticStyles.columnBackground, EnigmaticGUILayout.Width(width),
                EnigmaticGUILayout.Height(height), EnigmaticGUILayout.Padding(0), EnigmaticGUILayout.ElementSpacing(elementSpacing));

            EnigmaticGUILayout.GetActiveGrup().ApplyOptions(options);
        }

        public static void EndColum()
        {
            EnigmaticGUILayout.EndVertical();
        }

        public static void BeginVerticalScrollViewByGrup(Vector2 scrollPosition, 
            Vector2 offsetPosition, float elementSpacing, params EnigmaticGUILayoutOption[] options)
        {
            Rect rect = EnigmaticGUILayout.GetActiveGrup().Rect;

            rect.y += offsetPosition.y;
            rect.x += offsetPosition.x;
            rect.height -= offsetPosition.y;
            rect.width -= offsetPosition.x - 1;

            EnigmaticGUILayout.BeginVerticalScrollView(rect, rect, scrollPosition, 
                EnigmaticGUILayout.Padding(-1), EnigmaticGUILayout.ElementSpacing(elementSpacing));

            EnigmaticGUILayout.Space(2);
        }

        public static Vector2 EndVerticalScrollViewByGrup()
        {
            return EnigmaticGUILayout.EndScrollView(() => { EnigmaticGUIUtility.Repaint(); });
        }

        public static void DrawFileExportSettings(FileExportSettings settings, string extension)
        {
            float width = EnigmaticGUILayout.GetActiveGrup().Rect.width;

            EnigmaticGUILayout.BeginHorizontal(EnigmaticGUILayout.Width(width),
                EnigmaticGUILayout.ExpandHeight(true), EnigmaticGUILayout.ElementSpacing(1),
                EnigmaticGUILayout.Padding(0));
            {
                float widthText = EnigmaticGUILayout.CalculateSize(extension).x;
                EnigmaticGUILayout.TextField("File Name:", settings.FileName, width - 40);

                EnigmaticGUILayout.BeginDisabledGroup(true);
                {
                    EnigmaticGUILayout.Lable(extension);
                }
                EnigmaticGUILayout.EndDisabledGroup();
            }
            EnigmaticGUILayout.EndHorizontal();

            DrawSelectedPath(settings.Path);
        }

        public static void DrawSelectedPath(string path)
        {
            float width = EnigmaticGUILayout.GetActiveGrup().Rect.width;

            EnigmaticGUILayout.BeginHorizontal(EnigmaticGUILayout.Width(width),
                EnigmaticGUILayout.ExpandHeight(true), EnigmaticGUILayout.Padding(0));
            {
                float widthText = EnigmaticGUILayout.CalculateSize("...").x;
                EnigmaticGUILayout.TextField("     Path: ", path, width - 40);
                EnigmaticGUILayout.Button("...");
            }
            EnigmaticGUILayout.EndHorizontal();
        }

        public static void RemoveSelectedElementFromList<T>(List<T> list, 
            ref T selectedElement, T defaultValue, Action<T> removeElementAction)
        {
            if (selectedElement == null)
                return;

            int selectionElementIndex = list.IndexOf(selectedElement);
            removeElementAction?.Invoke(selectedElement);

            if (list.Count > 0)
            {
                if (list.Count - 1 >= selectionElementIndex)
                    selectedElement = list[selectionElementIndex];
                else
                    selectedElement = list[selectionElementIndex - 1];
            }
            else
            {
                selectedElement = defaultValue;
            }
        }
    }
}

#endif