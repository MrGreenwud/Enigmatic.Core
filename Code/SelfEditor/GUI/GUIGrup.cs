#if UNITY_EDITOR

using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using Unity.Profiling;

namespace Enigmatic.Core.Editor
{
    public class GUIGroup : GUIElement
    {
        public static ProfilerMarker CalculateGroupSize = new ProfilerMarker(ProfilerCategory.Render, "CalculateGroupSize");

        public GroupSortType SortType = GroupSortType.None;
        public float ElementSpacing = 3;
        public float Pudding = 3;
        public bool isCliped;

        public bool IsExpandWidth = false;
        public bool IsExpandHeight = false;

        public bool IsClickable = false;

        public GUIStyle Style = GUIStyle.none;
        public Color Color = Color.clear;

        private GUIGroup m_ParentGroup;
        private List<GUIElement> m_GUIElements = new List<GUIElement>();

        public int GUIElementsCount => m_GUIElements.Count;
        public GUIGroup ParentGroup => m_ParentGroup;

        public GUIGroup(Rect rect, GroupSortType sortType) : base(rect)
        {
            SortType = sortType;
        }

        public void Init(Rect rect, GroupSortType sortType)
        {
            base.Init(rect);
            Style = GUIStyle.none;
            Color = Color.clear;
            SortType = sortType;
            isCliped = false;
        }

        public void ApplyOptions(params EnigmaticGUILayoutOption[] options)
        {
            foreach (EnigmaticGUILayoutOption option in options)
            {
                switch (option.Type)
                {
                    case EnigmaticGUILayoutOption.TypeOption.SetPudding:
                        Pudding = (float)option.Value;
                        break;
                    case EnigmaticGUILayoutOption.TypeOption.SetElementSpacing:
                        ElementSpacing = (float)option.Value;
                        break;
                    case EnigmaticGUILayoutOption.TypeOption.SetExpandWidth:
                        IsExpandWidth = (bool)option.Value;
                        break;
                    case EnigmaticGUILayoutOption.TypeOption.SetExpandHeight:
                        IsExpandHeight = (bool)option.Value;
                        break;
                    case EnigmaticGUILayoutOption.TypeOption.SetWidth:
                        Rect.width = (float)option.Value;
                        break;
                    case EnigmaticGUILayoutOption.TypeOption.SetHeight:
                        Rect.height = (float)option.Value;
                        break;
                    case EnigmaticGUILayoutOption.TypeOption.SetClickable:
                        IsClickable = (bool)option.Value;
                        break;
                }
            }
        }

        public void SetParentGroup(GUIGroup parent)
        {
            if (m_ParentGroup != null)
                return;

            m_ParentGroup = parent;
        }

        public void AddElement(GUIElement element)
        {
            CalculateGroupSize.Begin();

            if (m_GUIElements.Contains(element))
                throw new Exception();

            m_GUIElements.Add(element);

            if (element is GUIGroup Group)
                Group.SetParentGroup(this);

            CalculateGroupSize.End();
        }

        public virtual Rect GetNext()
        {
            if (m_GUIElements.Count == 0)
            {
                //Rect.position => Vector2.zero
                return new Rect(Rect.position + Vector2.one * Pudding,
                        Rect.size - Vector2.one * (Pudding * 2));
            }

            return GetNextPosition();
        }

        public Rect GetNextPosition()
        {
            CalculateGroupSize.Begin();

            GUIElement element = m_GUIElements.Last();

            Rect rect = new Rect();

            if (SortType == GroupSortType.Horizontal)
            {
                rect.x = element.Rect.x + element.Rect.width + ElementSpacing;
                rect.y = Rect.y + Pudding;
            }
            else if (SortType == GroupSortType.Vertical)
            {
                rect.x = Rect.x + Pudding;
                rect.y = element.Rect.y + element.Rect.height + ElementSpacing;
            }

            CalculateGroupSize.End();

            return rect;
        }

        public Vector2 GetFreeArea()
        {
            float x = Rect.width - Pudding * 2;
            float y = Rect.height - Pudding * 2;

            if (m_GUIElements.Count > 0)
            {
                if (SortType == GroupSortType.Horizontal)
                {
                    x = Rect.width - Pudding * 2 - (m_GUIElements.Last().Rect.position
                        - Rect.position + Vector2.right * m_GUIElements.Last().Rect.width).x - ElementSpacing;
                }
                else if (SortType == GroupSortType.Vertical)
                {
                    y = Rect.height - Pudding * 2 - (m_GUIElements.Last().Rect.position
                        - Rect.position + Vector2.up * m_GUIElements.Last().Rect.width).y - ElementSpacing;
                }
            }

            return new Vector2(x, y);
        }

        public Rect GetUnClipedElement(Rect rect)
        {
            List<Vector2> positions = GetHierarchyGroupPosition();
            Rect result = new Rect(Vector2.zero, rect.size);

            foreach (Vector2 position in positions)
                result.position += position;

            result.position += result.position;
            return result;
        }

        public List<Vector2> GetHierarchyGroupPosition()
        {
            List<Vector2> positions = new List<Vector2>();

            if (isCliped)
                positions.Add(Rect.position);

            if (m_ParentGroup != null)
                positions.AddRange(m_ParentGroup.GetHierarchyGroupPosition());

            return positions;
        }

        public void RecalculateSize()
        {
            CalculateGroupSize.Begin();

            foreach (GUIElement element in m_GUIElements)
                CalculateSize(element);

            CalculateGroupSize.End();
        }

        public void CalculateSize(GUIElement element)
        {
            CalculateGroupSize.Begin();

            if (IsExpandHeight)
            {
                float extremePointElement = element.Rect.position.y + element.Rect.height;
                float extremePointThis = Rect.position.y + Rect.height;

                if (extremePointElement > extremePointThis)
                {
                    float expandValue = extremePointElement - extremePointThis + (Pudding * 2);
                    Rect.height += expandValue;
                }
            }

            if (IsExpandWidth)
            {
                float extremePointElement = element.Rect.position.x + element.Rect.width;
                float extremePointThis = Rect.position.x + Rect.width;

                if (extremePointElement > extremePointThis)
                {
                    float expandValue = extremePointElement - extremePointThis + (Pudding * 2);
                    Rect.width += expandValue;
                }
            }

            CalculateGroupSize.End();
        }

        public void BeginClip()
        {
            GUI.BeginClip(Rect);
        }

        public void EndClip()
        {
            GUI.EndClip();
        }
    }
}

#endif