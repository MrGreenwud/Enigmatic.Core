#if UNITY_EDITOR

using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using Unity.Profiling;

namespace Enigmatic.Core.Editor
{
    public class GUIGrup : GUIElement
    {
        public static ProfilerMarker CalculateGrupSize = new ProfilerMarker(ProfilerCategory.Render, "CalculateGrupSize");

        public GrupSortType SortType = GrupSortType.None;
        public float ElementSpacing = 3;
        public float Pudding = 3;
        public bool isCliped;

        public bool IsExpandWidth = false;
        public bool IsExpandHeight = false;

        public bool IsClicable = false;

        public GUIStyle Style = GUIStyle.none;

        private GUIGrup m_ParentGrup;
        private List<GUIElement> m_GUIElements = new List<GUIElement>();

        public int GUIElementsCount => m_GUIElements.Count;
        public GUIGrup ParentGrup => m_ParentGrup;

        public GUIGrup(Rect rect, float pudding = 3, GrupSortType sortType = GrupSortType.None,
            bool isExpandedWidth = false, bool isExpandedHeight = false,
            float elementSpacing = 3) : base(rect)
        {
            SortType = sortType;
            ElementSpacing = elementSpacing;
            Pudding = pudding;
            IsExpandWidth = isExpandedWidth;
            IsExpandHeight = isExpandedHeight;
        } //Delete

        public GUIGrup(Rect rect, GrupSortType sortType) : base(rect)
        {
            SortType = sortType;
        }

        public void Init(Rect rect, GrupSortType sortType)
        {
            base.Init(rect);
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
                        IsClicable = (bool)option.Value;
                        break;
                }
            }
        }

        public void SetParentGrup(GUIGrup parent)
        {
            if (m_ParentGrup != null)
                return;

            m_ParentGrup = parent;
        }

        public void AddElement(GUIElement element)
        {
            CalculateGrupSize.Begin();

            if (m_GUIElements.Contains(element))
                throw new Exception();

            m_GUIElements.Add(element);

            if (element is GUIGrup grup)
                grup.SetParentGrup(this);

            CalculateGrupSize.End();
        }

        public virtual Rect GetNext()
        {
            if (m_GUIElements.Count == 0)
            {
                //Rect.position => Vector2.zero
                return new Rect(Rect.position + Vector2.one * Pudding,
                        Rect.size - Vector2.one * (Pudding * 2));
            }

            return GetNextPostion();
        }

        public Rect GetNextPostion()
        {
            CalculateGrupSize.Begin();

            GUIElement element = m_GUIElements.Last();

            Rect rect = new Rect();

            if (SortType == GrupSortType.Horizontal)
            {
                rect.x = element.Rect.x + element.Rect.width + ElementSpacing;
                rect.y = Rect.y + Pudding;
            }
            else if (SortType == GrupSortType.Vertical)
            {
                rect.x = Rect.x + Pudding;
                rect.y = element.Rect.y + element.Rect.height + ElementSpacing;
            }

            CalculateGrupSize.End();

            return rect;
        }

        public Vector2 GetFreeArea()
        {
            float x = Rect.width - Pudding * 2;
            float y = Rect.height - Pudding * 2;

            if (m_GUIElements.Count > 0)
            {
                if (SortType == GrupSortType.Horizontal)
                {
                    x = Rect.width - Pudding * 2 - (m_GUIElements.Last().Rect.position
                        - Rect.position + Vector2.right * m_GUIElements.Last().Rect.width).x - ElementSpacing;
                }
                else if (SortType == GrupSortType.Vertical)
                {
                    y = Rect.height - Pudding * 2 - (m_GUIElements.Last().Rect.position
                        - Rect.position + Vector2.up * m_GUIElements.Last().Rect.width).y - ElementSpacing;
                }
            }

            return new Vector2(x, y);
        }

        public Rect GetUnClipedElement(Rect rect)
        {
            List<Vector2> positions = GetHierarchyGrupPosition();
            Rect result = new Rect(Vector2.zero, rect.size);

            foreach (Vector2 position in positions)
                result.position += position;

            result.position += result.position;
            return result;
        }

        public List<Vector2> GetHierarchyGrupPosition()
        {
            List<Vector2> positions = new List<Vector2>();

            if (isCliped)
                positions.Add(Rect.position);

            if (m_ParentGrup != null)
                positions.AddRange(m_ParentGrup.GetHierarchyGrupPosition());

            return positions;
        }

        public void ReconculateSize()
        {
            CalculateGrupSize.Begin();

            foreach (GUIElement element in m_GUIElements)
                ColculateSize(element);

            CalculateGrupSize.End();
        }

        public void ColculateSize(GUIElement element)
        {
            CalculateGrupSize.Begin();

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

            CalculateGrupSize.End();
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