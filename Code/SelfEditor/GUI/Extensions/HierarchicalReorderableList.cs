#if UNITY_EDITOR

using System;
using System.Collections.Generic;

using UnityEngine;

namespace Enigmatic.Core.Editor
{
    public class HierarchicalReorderableList<T> where T : TreeNode<T>
    {
        private List<T> m_List;

        private List<T> m_SelectedElements = new List<T>();
        private T m_HoverElement;
        private Rect m_HoverRect;

        private T m_ReorderParent;
        private int m_ReorderIndex;
        private Rect m_ReorderRect;

        private bool m_IsDragging;
        private bool m_IsClickOnSelectedElement;
        private T m_TrySelectElement;

        private DeferredActionCallerQueue m_PostActionCallerQueue = new DeferredActionCallerQueue();

        public bool IsDragging
        {
            get
            {
                return m_IsDragging;
            }
            set
            {
                if (m_IsDragging == false && value == true)
                    OnStartDragging?.Invoke();
                else if(m_IsDragging == true && value == false)
                    OnEndDragging?.Invoke();

                m_IsDragging = value;
            }
        }

        public event Action OnStartDragging;
        public event Action OnEndDragging;

        public event Action<T> OnDrawNode;
        public event Action<T> OnDrawBackground;
        public event Action<T> OnDrawHoverNodeBackground;
        public event Action<T> OnDrawSelectedNodeBackground;

        public event Action<T> OnSelectElement;

        public event Func<(T, T), bool> OnReparent;
        public event Func<(T, T), bool> OnReorder;

        public event Func<T, bool> OnDrawChild;

        public HierarchicalReorderableList()
        {
            OnEndDragging += () => 
            {
                if (Reorder() == false)
                    Reparent();
            };
        }

        public void AttachList(List<T> list)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            m_List = list;
        }

        public void Draw(float width, float elementSpacing = 0)
        {
            if (m_List == null)
                throw new InvalidOperationException(nameof(m_List));

            EditorInput.UpdateInput();

            ResetClick();
            DrawNodes(width, elementSpacing);

            DrawReorderLine();
            HandleDrag();

            SelectLastHoverElement();

            m_PostActionCallerQueue.CallAll();

            //UnselectAllElements();

            EnigmaticGUIUtility.Repaint();
        }

        private void DrawNodes(float width, float elementSpacing = 0)
        {
            EnigmaticGUILayout.BeginVertical(EnigmaticGUILayout.Width(width), EnigmaticGUILayout.Padding(0),
                EnigmaticGUILayout.ElementSpacing(elementSpacing), EnigmaticGUILayout.ExpandHeight(true));
            {
                foreach (T node in m_List)
                {
                    DrawNode(node, width);
                    Control(node);
                    DrawNodeChild(node, width);
                }
            }
            EnigmaticGUILayout.EndVertical();
        }

        private void DrawNode(T node, float width)
        {
            EnigmaticGUILayout.BeginHorizontal(EnigmaticGUILayout.Width(width), 
                EnigmaticGUILayout.Padding(0), EnigmaticGUILayout.ExpandHeight(true));
            {
                //EnigmaticGUILayout.Space(node.DepthLevel * 16);
                OnDrawNode?.Invoke(node);
            }
            EnigmaticGUILayout.EndHorizontal();

            if (m_SelectedElements.Contains(node))
            {
                if (OnDrawSelectedNodeBackground == null)
                    EnigmaticGUILayout.FillLastGroupColor(EnigmaticStyles.DarkThemeBlueElementSelected);
                else
                    OnDrawSelectedNodeBackground.Invoke(node);
            }
            else if (m_HoverElement == node)
            {
                if(OnDrawHoverNodeBackground == null)
                    EnigmaticGUILayout.FillLastGroupColor(EnigmaticStyles.DarkedBackgroundColor);
                else 
                    OnDrawHoverNodeBackground.Invoke(node);
            }
            else
            {
                OnDrawBackground?.Invoke(node);
            }
        }

        private void DrawNodeChild(T node, float width)
        {
            bool isDrawChild = OnDrawChild == null ? true : OnDrawChild.Invoke(node);

            if (isDrawChild)
            {
                node.ForEach((x) =>
                {
                    DrawNode(x, width);
                    Control(x);
                    DrawNodeChild(x, width);
                });
            }
        }

        private void Control(T node)
        {
            EnigmaticGUILayout.GetLastGroup().RecalculateSize();
            Rect rect = EnigmaticGUILayout.GetLastGroup().Rect;

            Vector2 mousePosition = EditorInput.GetMousePosition();

            HandleSelection(node, rect);
            HandleReorder(node, rect);

            if (EnigmaticGUIClip.UnClip(rect).Contains(mousePosition))
            {
                m_HoverElement = node;
                m_HoverRect = rect;
            }
        }

        private void Reparent()
        {
            //if (m_HoverElement == null)
            //    return;

            Vector2 mousePosition = EditorInput.GetMousePosition();

            EnigmaticGUILayout.GetLastGroup().RecalculateSize();
            Rect rect = EnigmaticGUILayout.GetLastGroup().Rect;

            if (m_SelectedElements.Count > 0
                && EditorInput.GetButtonPress(KeyCode.LeftControl) == false
                && EditorInput.GetMouseButtonUp(0))
            {
                if (EnigmaticGUIClip.UnClip(m_HoverRect).Contains(mousePosition) && m_HoverElement != null)
                {
                    foreach (T tNode in m_SelectedElements)
                    {
                        m_PostActionCallerQueue.Enqueue((x) =>
                        {
                            if (m_HoverElement.OnValidAddChild(tNode))
                            {
                                if (OnReparent == null || OnReparent.Invoke((m_HoverElement, tNode)))
                                {
                                    m_List.Remove(tNode);
                                    tNode.Parent?.RemoveChild(tNode);
                                    m_HoverElement.AddChild(tNode);
                                }
                            }
                        });
                    }
                }
                else if (rect.Contains(mousePosition))
                {
                    foreach (T tNode in m_SelectedElements)
                    {
                        m_PostActionCallerQueue.Enqueue((x) =>
                        {
                            if (m_HoverElement.OnValidAddChild(tNode))
                            {
                                if (OnReparent == null || OnReparent.Invoke((null, tNode)))
                                {
                                    m_List.Remove(tNode);
                                    tNode.Parent?.RemoveChild(tNode);
                                    m_List.Add(tNode);
                                }
                            }
                        });
                    }
                }
            }
        }

        private bool Reorder()
        {
            if (EnigmaticGUIClip.UnClip(m_ReorderRect).Contains(EditorInput.GetMousePosition()) == false)
                return false;

            foreach (T node in m_SelectedElements)
            {
                if (node == null || (OnReorder != null 
                    && OnReorder.Invoke((m_ReorderParent, node))) 
                    || m_ReorderParent == node)
                {
                    continue;
                }

                m_PostActionCallerQueue.Enqueue((x) =>
                {
                    m_List.Remove(node);
                    node.Parent?.RemoveChild(node);
                });

                if (m_ReorderParent == null)
                {
                    m_PostActionCallerQueue.Enqueue((x) =>
                    {
                        if (m_ReorderIndex < m_List.Count)
                            m_List.Insert(m_ReorderIndex, node);
                        else
                            m_List.Add(node);

                        m_ReorderIndex++;
                    });
                }
                else
                {
                    m_PostActionCallerQueue.Enqueue((x) =>
                    {
                        if (m_ReorderIndex < m_ReorderParent.ChildCount)
                            m_ReorderParent.Insert(m_ReorderIndex, node);
                        else
                            m_ReorderParent.AddChild(node);

                        m_ReorderIndex++;
                    });
                }
            }

            return true;
        }

        private void HandleSelection(T node, Rect rect)
        {
            rect = EnigmaticGUIClip.UnClip(rect);

            if (rect.Contains(EditorInput.GetMousePosition()) && EditorInput.GetMouseButtonDown(0))
            {
                if (m_SelectedElements.Contains(node))
                {
                    m_IsClickOnSelectedElement = true;
                    m_TrySelectElement = node;
                    EditorInput.UpdateLastMousePosition();
                }

                if (EditorInput.GetButtonPress(KeyCode.LeftControl))
                {
                    if (m_SelectedElements.Contains(node))
                    {
                        m_SelectedElements.Remove(node);
                        m_TrySelectElement = null;
                    }
                    else
                    {
                        AdditionalSelectElement(node);
                    }
                }
                else if (m_IsClickOnSelectedElement == false)
                {
                    SelectElement(node);
                    m_IsClickOnSelectedElement = true;
                }
            }
        }

        private void HandleReorder(T node, Rect rect)
        {
            Rect rect1 = new Rect(rect.position - (Vector2.up * 1.5f), new Vector2(rect.width, 3));
            Rect rect1Uncliped = EnigmaticGUIClip.UnClip(rect1);

            if (rect1Uncliped.Contains(EditorInput.GetMousePosition()))
            {
                m_ReorderParent = node.Parent;
                m_ReorderIndex = m_ReorderParent == null ? m_List.IndexOf(node) : m_ReorderParent.IndexOf(node);
                m_ReorderRect = rect1;
            }
        }

        private void SelectElement(T node)
        {
            m_SelectedElements.Clear();
            m_SelectedElements.Add(node);

            OnSelectElement?.Invoke(node);
        }

        private void AdditionalSelectElement(T node)
        {
            m_SelectedElements.Add(node);
            OnSelectElement?.Invoke(node);
        }

        private void ResetClick()
        {
            if (EditorInput.GetMouseButtonPress(0) == false)
                m_IsClickOnSelectedElement = false;
        }

        private void DrawReorderLine()
        {
            if (EnigmaticGUIClip.UnClip(m_ReorderRect).Contains(EditorInput.GetMousePosition()) && IsDragging)
                EnigmaticGUIDrawer.Rect(m_ReorderRect, EnigmaticStyles.AltDarkThemeBlueElementSelected);
        }

        private void SelectLastHoverElement()
        {
            if (IsDragging == false && EditorInput.GetMouseButtonUp(0)
                && m_HoverElement == m_TrySelectElement && m_HoverElement != null)
            {
                SelectElement(m_TrySelectElement);
            }
        }

        private void HandleDrag()
        {
            float delta = Mathf.Abs(EditorInput.GetMousePosition().y - EditorInput.GetLastMousePosition().y);

            IsDragging = (m_IsClickOnSelectedElement || EditorInput.GetMouseButtonPress(0)) 
                && delta > 5 && m_SelectedElements.Count > 0;
        }
    }
}

#endif