#if UNITY_EDITOR

using System;
using System.Collections.Generic;

using UnityEngine;

namespace Enigmatic.Core.Editor
{
    public class ReorderableList<T>
    {
        public bool IsDrawBackGround;

        private List<T> m_List;
        private List<Vector2> m_ElementsSize;
        private Vector2 m_MovedElementSize;

        private int m_SelectedElement = -1;
        private int m_TargetElement = -1;
        private int m_LastSelectionElement = -1;

        private Rect m_InteractableRect;

        public int LastSelectionElementIndex => m_LastSelectionElement;
        
        public T LastSelectionElement
        {
            get
            {
                if (m_LastSelectionElement < 0 || m_LastSelectionElement > m_List.Count - 1)
                    return default(T);

                return m_List[m_LastSelectionElement];
            }
        }

        public event Action<int> OnDrawElementByIndex;
        public event Action<T> OnDrawElementByT;

        public event Action OnMoveElement;

        public event Action OnDrawSelectedBackground;
        public event Action OnDrawHoverBackground;
        public event Action OnDrawBackground;

        public event Action<T> OnSelectElementT;
        public event Action<int> OnSelectElementIndex;

        public void AttachList(List<T> list)
        {
            m_List = list;
        }

        public void Draw(float width, params EnigmaticGUILayoutOption[] options)
        {
            CheckValidSize();
            EditorInput.UpdateInput();

            EnigmaticGUILayout.BeginVertical(EnigmaticGUILayout.Width(width), EnigmaticGUILayout.Padding(0), 
                EnigmaticGUILayout.ElementSpacing(0), EnigmaticGUILayout.ExpandHeight(true));
            {
                EnigmaticGUILayout.GetActiveGroup().ApplyOptions(options);

                for (int i = 0; i < m_List.Count; i++)
                {
                    DrawElementByReorderable(i);
                    Control(i);
                }
            }
            EnigmaticGUILayout.EndVertical();

            if (m_SelectedElement < 0)
                return;

            DrawMovedElement();
            DrawBackGround(m_SelectedElement);

            EnigmaticGUIUtility.Repaint();
        }

        private void Control(int index)
        {
            Rect rect = EnigmaticGUIClip.UnClip(EnigmaticGUILayout.GetLastGroup().Rect);

            if (m_InteractableRect.Contains(EditorInput.GetMousePosition())
                && EditorInput.GetMouseButtonDown(0) && m_SelectedElement == -1)
            {
                m_SelectedElement = index;
                m_MovedElementSize = rect.size;

                //OnSelectElementIndex?.Invoke(index);
                //OnSelectElementT?.Invoke(m_List[index]);
                
                GUI.FocusControl(null);
            }

            if (rect.Contains(EditorInput.GetMousePosition())
                && EditorInput.GetMouseButtonDown(0))
            {
                m_LastSelectionElement = index;

                OnSelectElementIndex?.Invoke(index);
                OnSelectElementT?.Invoke(m_List[index]);
                
                GUI.FocusControl(null);
            }

            if (rect.Contains(EditorInput.GetMousePosition()))
            {
                m_TargetElement = index;
                EnigmaticGUIUtility.Repaint();
            }

            if (m_SelectedElement > -1 && m_TargetElement > -1)
            {
                GUI.FocusControl(null);

                if (m_SelectedElement != m_TargetElement)
                    MoveElements();

                m_LastSelectionElement = m_SelectedElement;

                if (EditorInput.GetMouseButtonUp(0))
                {
                    m_SelectedElement = -1;
                    m_TargetElement = -1;
                }

                EnigmaticGUIUtility.Repaint();
            }
        }

        private void DrawElementByReorderable(int index)
        {
            Vector2 position = EnigmaticGUILayout.GetActiveGroup().GetNext().position;
            Rect rect = new Rect(position, m_ElementsSize[index]);

            if (m_TargetElement == index && m_SelectedElement > -1)
            {
                DrawVoidElement();
                EnigmaticGUILayout.FillLastGroupColor(EnigmaticStyles.DarkThemeBlueElementSelected);
            }
            else
            {
                DrawElement(index);
                DrawBackGround(index);
            }
        }

        private void DrawMovedElement()
        {
            Rect rect = EnigmaticGUILayout.GetLastGroup().Rect;

            float x = rect.x + 1;
            float y = EditorInput.GetMousePosition().y - EnigmaticGUIClip.UnClip(Vector2.zero).y - 8;

            float maxYPosition = rect.y + rect.height - 21;

            y = Mathf.Clamp(y, rect.y, maxYPosition);

            Rect rect2 = new Rect(x, y, 0, 0);

            EnigmaticGUILayout.BeginHorizontal(rect2, EnigmaticGUILayout.ExpandWidth(true),
                EnigmaticGUILayout.ExpandHeight(true), EnigmaticGUILayout.Padding(0));
            {
                DrawElement(m_SelectedElement);
            }
            EnigmaticGUILayout.EndHorizontal();
        }

        private void DrawElement(int index)
        {
            EnigmaticGUILayout.BeginVertical(EnigmaticGUILayout.Padding(0), EnigmaticGUILayout.ElementSpacing(0),
                EnigmaticGUILayout.ExpandWidth(true), EnigmaticGUILayout.ExpandHeight(true));
            {
                EnigmaticGUILayout.Space(-2);

                EnigmaticGUILayout.BeginHorizontal(EnigmaticGUILayout.ExpandWidth(true),
                    EnigmaticGUILayout.ExpandHeight(true), EnigmaticGUILayout.Padding(0),
                    EnigmaticGUILayout.ElementSpacing(0));
                {
                    EnigmaticGUILayout.Image(new Vector2(16, 16), EnigmaticStyles.elementMoveIcon);
                    m_InteractableRect = EnigmaticGUILayout.GetLastGUIRect();

                    OnDrawElementByIndex?.Invoke(index);
                    OnDrawElementByT?.Invoke(m_List[index]);
                }
                EnigmaticGUILayout.EndHorizontal();
            }
            EnigmaticGUILayout.EndVertical();
        }

        private void DrawBackGround(int index)
        {
            if (m_SelectedElement == index || (m_LastSelectionElement == index && m_SelectedElement == -1))
            {
                if (OnDrawSelectedBackground == null)
                    EnigmaticGUILayout.FillLastGroupColor(EnigmaticStyles.DarkThemeBlueElementSelected);
                else
                    OnDrawSelectedBackground.Invoke();
            }
            else if (m_TargetElement == index)
            {
                if (OnDrawHoverBackground == null)
                    EnigmaticGUILayout.FillLastGroupColor(EnigmaticStyles.DarkedBackgroundColor);
                else
                    OnDrawHoverBackground.Invoke();
            }
            else if (OnDrawBackground != null)
            {
                OnDrawBackground.Invoke();
            }
        }

        private void DrawVoidElement()
        {
            EnigmaticGUILayout.BeginHorizontal(EnigmaticGUILayout.Width(m_MovedElementSize.x),
                EnigmaticGUILayout.Height(m_MovedElementSize.y));
            EnigmaticGUILayout.EndHorizontal();
        }

        private void MoveElements()
        {
            T tempList = m_List[m_SelectedElement];

            m_List[m_SelectedElement] = m_List[m_TargetElement];
            m_List[m_TargetElement] = tempList;

            m_SelectedElement = m_TargetElement;
            m_TargetElement = -1;

            OnMoveElement?.Invoke();

            EnigmaticGUIUtility.Repaint();
        }

        private void CheckValidSize()
        {
            if(m_ElementsSize == null)
                m_ElementsSize = new List<Vector2>(m_List.Count);

            if (m_List.Count == m_ElementsSize.Count || m_List.Count - m_ElementsSize.Count < 0)
                return;

            m_ElementsSize.AddRange(ListExtensions.CreateFilledList(m_List.Count - m_ElementsSize.Count, Vector2.zero));
        }
    }
}

#endif