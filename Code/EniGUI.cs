using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using TMPro;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Enigmatic.Core.Editor.Experimental
{
    public enum GroupSortType
    {
        None,
        Vertical,
        Horizontal,
    }

    public class GUIElement
    {
        public Rect Rect;

        public GUIElement(Rect rect)
        {
            Rect = rect;
        }

        public GUIElement(Vector2 size)
        {
            Rect = new Rect(Vector2.zero, size);
        }
    }

    public class GUIGroup : GUIElement
    {
        public GroupSortType SortType;

        public string name; 

        public float ElementSpacing;
        public float Padding;
        public bool IsClipped;
        public bool IsClickable;

        public bool IsExpandWidth = false;
        public bool IsExpandHeight = false;

        public GUIStyle Style = GUIStyle.none;
        public Color Color = Color.clear;

        public GUIGroup Parent { get; private set; }

        private List<GUIElement> m_GUIElements = new List<GUIElement>();

        public GUIGroup() : base(Rect.zero) { name = Guid.NewGuid().ToString(); }

        public GUIGroup(Rect rect, params EniGUILayoutOption[] options) : base(rect)
        {
            ApplyOptions(options);
        }

        public void ForEach(Action<GUIElement> action)
        {
            foreach (GUIElement element in m_GUIElements)
                action?.Invoke(element);
        }

        public virtual void Reset()
        {
            Rect = Rect.zero;
            SortType = GroupSortType.None;
            Style = GUIStyle.none;
            Color = Color.clear;

            Padding = 3;
            ElementSpacing = 3;

            IsClipped = false;
            IsClickable = false;

            IsExpandWidth = false;
            IsExpandHeight = false;

            m_GUIElements.Clear();
            Parent = null;
        }

        public void ApplyOptions(params EniGUILayoutOption[] options)
        {
            foreach (EniGUILayoutOption option in options)
            {
                switch (option.Type)
                {
                    case EniGUILayoutOption.TypeOption.SetPudding:
                        Padding = (float)option.Value;
                        break;
                    case EniGUILayoutOption.TypeOption.SetElementSpacing:
                        ElementSpacing = (float)option.Value;
                        break;
                    case EniGUILayoutOption.TypeOption.SetExpandWidth:
                        IsExpandWidth = (bool)option.Value;
                        break;
                    case EniGUILayoutOption.TypeOption.SetExpandHeight:
                        IsExpandHeight = (bool)option.Value;
                        break;
                    case EniGUILayoutOption.TypeOption.SetWidth:
                        Rect.width = (float)option.Value;
                        break;
                    case EniGUILayoutOption.TypeOption.SetHeight:
                        Rect.height = (float)option.Value;
                        break;
                    case EniGUILayoutOption.TypeOption.SetClickable:
                        IsClickable = (bool)option.Value;
                        break;
                }
            }
        }

        public void AddElement(GUIElement element)
        {
            if (IsValidElement(element, out Exception exception) == false)
                throw exception;

            m_GUIElements.Add(element);

            if (element is GUIGroup group)
                group.Parent = this;
        }

        public virtual Rect GetNext()
        {
            if (m_GUIElements.Count == 0)
            {
                return new Rect(Rect.position + Vector2.one * Padding,
                        Rect.size - Vector2.one * (Padding * 2));
            }

            return GetNextPosition();
        }
        public Rect GetNextPosition()
        {
            GUIElement element = m_GUIElements.Last();

            Rect rect = new Rect();

            if (SortType == GroupSortType.Horizontal)
            {
                rect.x = element.Rect.x + element.Rect.width + ElementSpacing;
                rect.y = Rect.y + Padding;
            }
            else if (SortType == GroupSortType.Vertical)
            {
                rect.x = Rect.x + Padding;
                rect.y = element.Rect.y + element.Rect.height + ElementSpacing;
            }

            return rect;
        }
        public Vector2 GetFreeArea()
        {
            float x = Rect.width - Padding * 2;
            float y = Rect.height - Padding * 2;

            if (m_GUIElements.Count > 0)
            {
                if (SortType == GroupSortType.Horizontal)
                {
                    x = Rect.width - Padding * 2 - (m_GUIElements.Last().Rect.position
                        - Rect.position + Vector2.right * m_GUIElements.Last().Rect.width).x - ElementSpacing;
                }
                else if (SortType == GroupSortType.Vertical)
                {
                    y = Rect.height - Padding * 2 - (m_GUIElements.Last().Rect.position
                        - Rect.position + Vector2.up * m_GUIElements.Last().Rect.width).y - ElementSpacing;
                }
            }

            return new Vector2(x, y);
        }

        public void RecalculateSize()
        {
            foreach (GUIElement element in m_GUIElements)
                CalculateSize(element);
        }
        public void CalculateSize(GUIElement element)
        {
            if (IsExpandHeight)
            {
                float extremePointElement = element.Rect.position.y + element.Rect.height;
                float extremePointThis = Rect.position.y + Rect.height;

                if (extremePointElement > extremePointThis)
                {
                    float expandValue = extremePointElement - extremePointThis + (Padding * 2);
                    Rect.height += expandValue;
                }
            }

            if (IsExpandWidth)
            {
                float extremePointElement = element.Rect.position.x + element.Rect.width;
                float extremePointThis = Rect.position.x + Rect.width;

                if (extremePointElement > extremePointThis)
                {
                    float expandValue = extremePointElement - extremePointThis + (Padding * 2);
                    Rect.width += expandValue;
                }
            }
        }

        private bool IsValidElement(GUIElement element, out Exception exception)
        {
            if (m_GUIElements.Contains(element))
            {
                exception = new Exception("1");
                return false;
            }

            exception = null;
            return true;
        }
    }

    public class GUIScrollView : GUIGroup
    {
        public GUIGroup Container;

        public Vector2 ScrollPosition;

        public bool AlwaysShowHorizontal;
        public bool AlwaysShowVertical;

        public GUIStyle HorizontalScrollbar;
        public GUIStyle VerticalScrollbar;

        public Rect View => Container.Rect;

        public Action<Vector2> Action;

        public override void Reset()
        {
            base.Reset();

            Padding = 0;

            ScrollPosition = Vector2.zero;

            AlwaysShowHorizontal = false;
            AlwaysShowVertical = false;

            HorizontalScrollbar = null;
            VerticalScrollbar = null;

            Action = null;
        }

        public void UpdatePosition()
        {
            Container.Rect.position -= ScrollPosition;
        }
    }

    public static class EniGUI
    {
        private static float sm_LabelOriginWidth = 150;

        public static Rect GUILastRect { get; private set; }

        [InitializeOnLoadMethod]
        public static void Init()
        {
            sm_LabelOriginWidth = EditorGUIUtility.labelWidth;
        }

        public static void BeginScrollView(Rect rect, Vector2 scrollPosition, Rect viewRect, Action<Vector2> action)
        {
            BeginScrollView(rect, scrollPosition, viewRect, false, false, action);
        }
        public static void BeginScrollView(Rect rect, Vector2 scrollPosition, Rect viewRect, bool alwaysShowHorizontal, bool alwaysShowVertical, Action<Vector2> action)
        {
            BeginScrollView(rect, scrollPosition, viewRect, alwaysShowHorizontal, alwaysShowVertical, action, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar);
        }
        public static void BeginScrollView(Rect rect, Vector2 scrollPosition, Rect viewRect, Action<Vector2> action, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar)
        {
            BeginScrollView(rect, scrollPosition, viewRect, false, false, action, horizontalScrollbar, verticalScrollbar);
        }
        public static void BeginScrollView(Rect rect, Vector2 scrollPosition, Rect viewRect, bool alwaysShowHorizontal, bool alwaysShowVertical, Action<Vector2> action, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar)
        {
            Vector2 position = BeginScrollView(rect, scrollPosition, viewRect, alwaysShowHorizontal, alwaysShowVertical, horizontalScrollbar, verticalScrollbar);
            action?.Invoke(position);
        }
        public static Vector2 BeginScrollView(Rect rect, Vector2 scrollPosition, Rect viewRect)
        {
            return BeginScrollView(rect, scrollPosition, viewRect, false, false);
        }
        public static Vector2 BeginScrollView(Rect rect, Vector2 scrollPosition, Rect viewRect, bool alwaysShowHorizontal, bool alwaysShowVertical)
        {
            return BeginScrollView(rect, scrollPosition, viewRect, alwaysShowHorizontal, alwaysShowVertical, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar);
        }
        public static Vector2 BeginScrollView(Rect rect, Vector2 scrollPosition, Rect viewRect, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar)
        {
            return BeginScrollView(rect, scrollPosition, viewRect, false, false, horizontalScrollbar, verticalScrollbar);
        }
        public static Vector2 BeginScrollView(Rect rect, Vector2 scrollPosition, Rect viewRect, bool alwaysShowHorizontal, bool alwaysShowVertical, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar)
        {
            Vector2 position = GUI.BeginScrollView(rect, scrollPosition, viewRect, alwaysShowHorizontal, alwaysShowVertical, horizontalScrollbar, verticalScrollbar);
            UpdateLastRect(rect);

            return position;
        }

        public static void EndScrollView()
        {
            GUI.EndScrollView();
        }

        public static void PropertyField(Rect rect, SerializedProperty property, string label = "")
        {
            PropertyField(rect, sm_LabelOriginWidth, property, label);
        }
        public static void PropertyField(Rect rect, float labelWidth, SerializedProperty property, string label = "")
        {
            GUIContent labelContent = string.IsNullOrEmpty(label) ? null : new GUIContent(label);
            PropertyField(rect, labelWidth, property, labelContent);
        }
        public static void PropertyField(Rect rect, SerializedProperty property, GUIContent label = null)
        {
            PropertyField(rect, sm_LabelOriginWidth, property, label);
        }
        public static void PropertyField(Rect rect, float labelWidth, SerializedProperty property, GUIContent label = null)
        {
            label = label ?? new GUIContent(property.displayName);

            ChangeLabelWidth(labelWidth);

            EditorGUI.PropertyField(rect, property, label);
            UpdateLastRect(rect);

            ResetLabelWidth();
        }

        public static void ObjectField<T>(Rect rect, T value, Action<T> action, bool allowSceneObjects) where T : UnityEngine.Object
        {
            ObjectField(rect, "", value, action, allowSceneObjects);
        }
        public static void ObjectField<T>(Rect rect, string label, T value, Action<T> action, bool allowSceneObjects) where T : UnityEngine.Object
        {
            ObjectField(rect, sm_LabelOriginWidth, label, value, action, allowSceneObjects);
        }
        public static void ObjectField<T>(Rect rect, float labelWidth, string label, T value, Action<T> action, bool allowSceneObjects) where T : UnityEngine.Object
        {
            ObjectField(rect, labelWidth, new GUIContent(label), value, action, allowSceneObjects);
        }
        public static void ObjectField<T>(Rect rect, GUIContent label, T value, Action<T> action, bool allowSceneObjects) where T : UnityEngine.Object
        {
            ObjectField(rect, sm_LabelOriginWidth, label, value, action, allowSceneObjects);
        }
        public static void ObjectField<T>(Rect rect, float labelWidth, GUIContent label, T value, Action<T> action, bool allowSceneObjects) where T : UnityEngine.Object
        {
            value = ObjectField(rect, labelWidth, label, value, allowSceneObjects);
            action?.Invoke(value);
        }
        public static T ObjectField<T>(Rect rect, T value, bool allowSceneObjects) where T : UnityEngine.Object
        {
            return ObjectField(rect, "", value, allowSceneObjects);
        }
        public static T ObjectField<T>(Rect rect, string label, T value, bool allowSceneObjects) where T : UnityEngine.Object
        {
            return ObjectField(rect, sm_LabelOriginWidth, label, value, allowSceneObjects);
        }
        public static T ObjectField<T>(Rect rect, float labelWidth, string label, T value, bool allowSceneObjects) where T : UnityEngine.Object
        {
            return ObjectField(rect, labelWidth, new GUIContent(label), value, allowSceneObjects);
        }
        public static T ObjectField<T>(Rect rect, GUIContent label, T value, bool allowSceneObjects) where T : UnityEngine.Object
        {
            return ObjectField(rect, sm_LabelOriginWidth, label, value, allowSceneObjects);
        }
        public static T ObjectField<T>(Rect rect, float labelWidth, GUIContent label, T value, bool allowSceneObjects) where T : UnityEngine.Object
        {
            ChangeLabelWidth(labelWidth);

            T result = (T)EditorGUI.ObjectField(rect, label, value, typeof(T), allowSceneObjects);
            UpdateLastRect(rect);

            ResetLabelWidth();

            return result;
        }

        public static void ColorField(Rect rect, Color value, Action<Color> action)
        {
            ColorField(rect, "", value, action);
        }
        public static void ColorField(Rect rect, string label, Color value, Action<Color> action)
        {
            ColorField(rect, sm_LabelOriginWidth, label, value, action);
        }
        public static void ColorField(Rect rect, float labelWidth, string label, Color value, Action<Color> action)
        {
            ColorField(rect, labelWidth, new GUIContent(label), value, action);
        }
        public static void ColorField(Rect rect, GUIContent label, Color value, Action<Color> action)
        {
            ColorField(rect, sm_LabelOriginWidth, label, value, action);
        }
        public static void ColorField(Rect rect, float labelWidth, GUIContent label, Color value, Action<Color> action)
        {
            value = ColorField(rect, labelWidth, label, value);
            action?.Invoke(value);
        }
        public static Color ColorField(Rect rect, Color value)
        {
            return ColorField(rect, "", value);
        }
        public static Color ColorField(Rect rect, string label, Color value)
        {
            return ColorField(rect, sm_LabelOriginWidth, label, value);
        }
        public static Color ColorField(Rect rect, float labelWidth, string label, Color value)
        {
            return ColorField(rect, labelWidth, new GUIContent(label), value);
        }
        public static Color ColorField(Rect rect, GUIContent label, Color value)
        {
            return ColorField(rect, sm_LabelOriginWidth, label, value);
        }
        public static Color ColorField(Rect rect, float labelWidth, GUIContent label, Color value)
        {
            ChangeLabelWidth(labelWidth);

            Color result = EditorGUI.ColorField(rect, label, value);
            UpdateLastRect(rect);

            ResetLabelWidth();

            return result;
        }

        public static void GradientField(Rect rect, Gradient value, Action<Gradient> action)
        {
            GradientField(rect, "", value, action);
        }
        public static void GradientField(Rect rect, string label, Gradient value, Action<Gradient> action)
        {
            GradientField(rect, sm_LabelOriginWidth, label, value, action);
        }
        public static void GradientField(Rect rect, float labelWidth, string label, Gradient value, Action<Gradient> action)
        {
            GradientField(rect, labelWidth, new GUIContent(label), value, action);
        }
        public static void GradientField(Rect rect, GUIContent label, Gradient value, Action<Gradient> action)
        {
            GradientField(rect, sm_LabelOriginWidth, label, value, action);
        }
        public static void GradientField(Rect rect, float labelWidth, GUIContent label, Gradient value, Action<Gradient> action)
        {
            value = GradientField(rect, labelWidth, label, value);
            action?.Invoke(value);
        }
        public static Gradient GradientField(Rect rect, Gradient value)
        {
            return GradientField(rect, "", value);
        }
        public static Gradient GradientField(Rect rect, string label, Gradient value)
        {
            return GradientField(rect, sm_LabelOriginWidth, label, value);
        }
        public static Gradient GradientField(Rect rect, float labelWidth, string label, Gradient value)
        {
            return GradientField(rect, labelWidth, new GUIContent(label), value);
        }
        public static Gradient GradientField(Rect rect, GUIContent label, Gradient value)
        {
            return GradientField(rect, sm_LabelOriginWidth, label, value);
        }
        public static Gradient GradientField(Rect rect, float labelWidth, GUIContent label, Gradient value)
        {
            ChangeLabelWidth(labelWidth);

            Gradient result = EditorGUI.GradientField(rect, label, value);
            UpdateLastRect(rect);

            ResetLabelWidth();

            return result;
        }

        public static void CurveField(Rect rect, AnimationCurve value, Action<AnimationCurve> action)
        {
            CurveField(rect, "", value, action);
        }
        public static void CurveField(Rect rect, string label, AnimationCurve value, Action<AnimationCurve> action)
        {
            CurveField(rect, sm_LabelOriginWidth, label, value, action);
        }
        public static void CurveField(Rect rect, float labelWidth, string label, AnimationCurve value, Action<AnimationCurve> action)
        {
            CurveField(rect, labelWidth, new GUIContent(label), value, action);
        }
        public static void CurveField(Rect rect, GUIContent label, AnimationCurve value, Action<AnimationCurve> action)
        {
            CurveField(rect, sm_LabelOriginWidth, label, value, action);
        }
        public static void CurveField(Rect rect, float labelWidth, GUIContent label, AnimationCurve value, Action<AnimationCurve> action)
        {
            value = CurveField(rect, labelWidth, label, value);
            action?.Invoke(value);
        }
        public static AnimationCurve CurveField(Rect rect, AnimationCurve value)
        {
            return CurveField(rect, "", value);
        }
        public static AnimationCurve CurveField(Rect rect, string label, AnimationCurve value)
        {
            return CurveField(rect, sm_LabelOriginWidth, label, value);
        }
        public static AnimationCurve CurveField(Rect rect, float labelWidth, string label, AnimationCurve value)
        {
            return CurveField(rect, labelWidth, new GUIContent(label), value);
        }
        public static AnimationCurve CurveField(Rect rect, GUIContent label, AnimationCurve value)
        {
            return CurveField(rect, sm_LabelOriginWidth, label, value);
        }
        public static AnimationCurve CurveField(Rect rect, float labelWidth, GUIContent label, AnimationCurve value)
        {
            ChangeLabelWidth(labelWidth);

            AnimationCurve result = EditorGUI.CurveField(rect, label, value);
            UpdateLastRect(rect);

            ResetLabelWidth();

            return result;
        }

        public static void Vector2Field(Rect rect, Vector2 value, Action<Vector2> action)
        {
            Vector2Field(rect, "", value, action);
        }
        public static void Vector2Field(Rect rect, string label, Vector2 value, Action<Vector2> action)
        {
            Vector2Field(rect, sm_LabelOriginWidth, label, value, action);
        }
        public static void Vector2Field(Rect rect, float labelWidth, string label, Vector2 value, Action<Vector2> action)
        {
            Vector2Field(rect, labelWidth, new GUIContent(label), value, action);
        }
        public static void Vector2Field(Rect rect, GUIContent label, Vector2 value, Action<Vector2> action)
        {
            Vector2Field(rect, sm_LabelOriginWidth, label, value, action);
        }
        public static void Vector2Field(Rect rect, float labelWidth, GUIContent label, Vector2 value, Action<Vector2> action)
        {
            value = Vector2Field(rect, labelWidth, label, value);
            action?.Invoke(value);
        }
        public static Vector2 Vector2Field(Rect rect, Vector2 value)
        {
            return Vector2Field(rect, "", value);
        }
        public static Vector2 Vector2Field(Rect rect, string label, Vector2 value)
        {
            return Vector2Field(rect, sm_LabelOriginWidth, label, value);
        }
        public static Vector2 Vector2Field(Rect rect, float labelWidth, string label, Vector2 value)
        {
            return Vector2Field(rect, labelWidth, new GUIContent(label), value);
        }
        public static Vector2 Vector2Field(Rect rect, GUIContent label, Vector2 value)
        {
            return Vector2Field(rect, sm_LabelOriginWidth, label, value);
        }
        public static Vector2 Vector2Field(Rect rect, float labelWidth, GUIContent label, Vector2 value)
        {
            ChangeLabelWidth(labelWidth);

            Vector2 result = EditorGUI.Vector2Field(rect, label, value);
            UpdateLastRect(rect);

            ResetLabelWidth();

            return result;
        }

        public static void Vector3Field(Rect rect, Vector3 value, Action<Vector3> action)
        {
            Vector3Field(rect, "", value, action);
        }
        public static void Vector3Field(Rect rect, string label, Vector3 value, Action<Vector3> action)
        {
            Vector3Field(rect, sm_LabelOriginWidth, label, value, action);
        }
        public static void Vector3Field(Rect rect, float labelWidth, string label, Vector3 value, Action<Vector3> action)
        {
            Vector3Field(rect, labelWidth, new GUIContent(label), value, action);
        }
        public static void Vector3Field(Rect rect, GUIContent label, Vector3 value, Action<Vector3> action)
        {
            Vector3Field(rect, sm_LabelOriginWidth, label, value, action);
        }
        public static void Vector3Field(Rect rect, float labelWidth, GUIContent label, Vector3 value, Action<Vector3> action)
        {
            value = Vector3Field(rect, labelWidth, label, value);
            action?.Invoke(value);
        }
        public static Vector3 Vector3Field(Rect rect, Vector3 value)
        {
            return Vector3Field(rect, "", value);
        }
        public static Vector3 Vector3Field(Rect rect, string label, Vector3 value)
        {
            return Vector3Field(rect, sm_LabelOriginWidth, label, value);
        }
        public static Vector3 Vector3Field(Rect rect, float labelWidth, string label, Vector3 value)
        {
            return Vector3Field(rect, labelWidth, new GUIContent(label), value);
        }
        public static Vector3 Vector3Field(Rect rect, GUIContent label, Vector3 value)
        {
            return Vector3Field(rect, sm_LabelOriginWidth, label, value);
        }
        public static Vector3 Vector3Field(Rect rect, float labelWidth, GUIContent label, Vector3 value)
        {
            ChangeLabelWidth(labelWidth);

            Vector3 result = EditorGUI.Vector3Field(rect, label, value);
            UpdateLastRect(rect);

            ResetLabelWidth();

            return result;
        }

        public static void Vector2IntField(Rect rect, Vector2Int value, Action<Vector2Int> action)
        {
            Vector2IntField(rect, "", value, action);
        }
        public static void Vector2IntField(Rect rect, string label, Vector2Int value, Action<Vector2Int> action)
        {
            Vector2IntField(rect, sm_LabelOriginWidth, label, value, action);
        }
        public static void Vector2IntField(Rect rect, float labelWidth, string label, Vector2Int value, Action<Vector2Int> action)
        {
            Vector2IntField(rect, labelWidth, new GUIContent(label), value, action);
        }
        public static void Vector2IntField(Rect rect, GUIContent label, Vector2Int value, Action<Vector2Int> action)
        {
            Vector2IntField(rect, sm_LabelOriginWidth, label, value, action);
        }
        public static void Vector2IntField(Rect rect, float labelWidth, GUIContent label, Vector2Int value, Action<Vector2Int> action)
        {
            value = Vector2IntField(rect, labelWidth, label, value);
            action?.Invoke(value);
        }
        public static Vector2Int Vector2IntField(Rect rect, Vector2Int value)
        {
            return Vector2IntField(rect, "", value);
        }
        public static Vector2Int Vector2IntField(Rect rect, string label, Vector2Int value)
        {
            return Vector2IntField(rect, sm_LabelOriginWidth, label, value);
        }
        public static Vector2Int Vector2IntField(Rect rect, float labelWidth, string label, Vector2Int value)
        {
            return Vector2IntField(rect, labelWidth, new GUIContent(label), value);
        }
        public static Vector2Int Vector2IntField(Rect rect, GUIContent label, Vector2Int value)
        {
            return Vector2IntField(rect, sm_LabelOriginWidth, label, value);
        }
        public static Vector2Int Vector2IntField(Rect rect, float labelWidth, GUIContent label, Vector2Int value)
        {
            ChangeLabelWidth(labelWidth);

            Vector2Int result = EditorGUI.Vector2IntField(rect, label, value);
            UpdateLastRect(rect);

            ResetLabelWidth();

            return result;
        }

        public static void Vector3IntField(Rect rect, Vector3Int value, Action<Vector3Int> action)
        {
            Vector3IntField(rect, "", value, action);
        }
        public static void Vector3IntField(Rect rect, string label, Vector3Int value, Action<Vector3Int> action)
        {
            Vector3IntField(rect, sm_LabelOriginWidth, label, value, action);
        }
        public static void Vector3IntField(Rect rect, float labelWidth, string label, Vector3Int value, Action<Vector3Int> action)
        {
            Vector3IntField(rect, labelWidth, new GUIContent(label), value, action);
        }
        public static void Vector3IntField(Rect rect, GUIContent label, Vector3Int value, Action<Vector3Int> action)
        {
            Vector3IntField(rect, sm_LabelOriginWidth, label, value, action);
        }
        public static void Vector3IntField(Rect rect, float labelWidth, GUIContent label, Vector3Int value, Action<Vector3Int> action)
        {
            value = Vector3IntField(rect, labelWidth, label, value);
            action?.Invoke(value);
        }
        public static Vector3Int Vector3IntField(Rect rect, Vector3Int value)
        {
            return Vector3IntField(rect, "", value);
        }
        public static Vector3Int Vector3IntField(Rect rect, string label, Vector3Int value)
        {
            return Vector3IntField(rect, sm_LabelOriginWidth, label, value);
        }
        public static Vector3Int Vector3IntField(Rect rect, float labelWidth, string label, Vector3Int value)
        {
            return Vector3IntField(rect, labelWidth, new GUIContent(label), value);
        }
        public static Vector3Int Vector3IntField(Rect rect, GUIContent label, Vector3Int value)
        {
            return Vector3IntField(rect, sm_LabelOriginWidth, label, value);
        }
        public static Vector3Int Vector3IntField(Rect rect, float labelWidth, GUIContent label, Vector3Int value)
        {
            ChangeLabelWidth(labelWidth);

            Vector3Int result = EditorGUI.Vector3IntField(rect, label, value);
            UpdateLastRect(rect);

            ResetLabelWidth();

            return result;
        }

        public static void BoundsField(Rect rect, Bounds value, Action<Bounds> action)
        {
            BoundsField(rect, "", value, action);
        }
        public static void BoundsField(Rect rect, string label, Bounds value, Action<Bounds> action)
        {
            BoundsField(rect, sm_LabelOriginWidth, label, value, action);
        }
        public static void BoundsField(Rect rect, float labelWidth, string label, Bounds value, Action<Bounds> action)
        {
            BoundsField(rect, labelWidth, new GUIContent(label), value, action);
        }
        public static void BoundsField(Rect rect, GUIContent label, Bounds value, Action<Bounds> action)
        {
            BoundsField(rect, sm_LabelOriginWidth, label, value, action);
        }
        public static void BoundsField(Rect rect, float labelWidth, GUIContent label, Bounds value, Action<Bounds> action)
        {
            value = BoundsField(rect, labelWidth, label, value);
            action?.Invoke(value);
        }
        public static Bounds BoundsField(Rect rect, Bounds value)
        {
            return BoundsField(rect, "", value);
        }
        public static Bounds BoundsField(Rect rect, string label, Bounds value)
        {
            return BoundsField(rect, sm_LabelOriginWidth, label, value);
        }
        public static Bounds BoundsField(Rect rect, float labelWidth, string label, Bounds value)
        {
            return BoundsField(rect, labelWidth, new GUIContent(label), value);
        }
        public static Bounds BoundsField(Rect rect, GUIContent label, Bounds value)
        {
            return BoundsField(rect, sm_LabelOriginWidth, label, value);
        }
        public static Bounds BoundsField(Rect rect, float labelWidth, GUIContent label, Bounds value)
        {
            ChangeLabelWidth(labelWidth);

            Bounds result = EditorGUI.BoundsField(rect, label, value);
            UpdateLastRect(rect);

            ResetLabelWidth();

            return result;
        }

        public static void BoundsIntField(Rect rect, BoundsInt value, Action<BoundsInt> action)
        {
            BoundsIntField(rect, "", value, action);
        }
        public static void BoundsIntField(Rect rect, string label, BoundsInt value, Action<BoundsInt> action)
        {
            BoundsIntField(rect, sm_LabelOriginWidth, label, value, action);
        }
        public static void BoundsIntField(Rect rect, float labelWidth, string label, BoundsInt value, Action<BoundsInt> action)
        {
            BoundsIntField(rect, labelWidth, new GUIContent(label), value, action);
        }
        public static void BoundsIntField(Rect rect, GUIContent label, BoundsInt value, Action<BoundsInt> action)
        {
            BoundsIntField(rect, sm_LabelOriginWidth, label, value, action);
        }
        public static void BoundsIntField(Rect rect, float labelWidth, GUIContent label, BoundsInt value, Action<BoundsInt> action)
        {
            value = BoundsIntField(rect, labelWidth, label, value);
            action?.Invoke(value);
        }
        public static BoundsInt BoundsIntField(Rect rect, BoundsInt value)
        {
            return BoundsIntField(rect, "", value);
        }
        public static BoundsInt BoundsIntField(Rect rect, string label, BoundsInt value)
        {
            return BoundsIntField(rect, sm_LabelOriginWidth, label, value);
        }
        public static BoundsInt BoundsIntField(Rect rect, float labelWidth, string label, BoundsInt value)
        {
            return BoundsIntField(rect, labelWidth, new GUIContent(label), value);
        }
        public static BoundsInt BoundsIntField(Rect rect, GUIContent label, BoundsInt value)
        {
            return BoundsIntField(rect, sm_LabelOriginWidth, label, value);
        }
        public static BoundsInt BoundsIntField(Rect rect, float labelWidth, GUIContent label, BoundsInt value)
        {
            ChangeLabelWidth(labelWidth);

            BoundsInt result = EditorGUI.BoundsIntField(rect, label, value);
            UpdateLastRect(rect);

            ResetLabelWidth();

            return result;
        }

        public static void RectField(Rect rect, Rect value, Action<Rect> action)
        {
            RectField(rect, "", value, action);
        }
        public static void RectField(Rect rect, string label, Rect value, Action<Rect> action)
        {
            RectField(rect, sm_LabelOriginWidth, label, value, action);
        }
        public static void RectField(Rect rect, float labelWidth, string label, Rect value, Action<Rect> action)
        {
            RectField(rect, labelWidth, new GUIContent(label), value, action);
        }
        public static void RectField(Rect rect, GUIContent label, Rect value, Action<Rect> action)
        {
            RectField(rect, sm_LabelOriginWidth, label, value, action);
        }
        public static void RectField(Rect rect, float labelWidth, GUIContent label, Rect value, Action<Rect> action)
        {
            value = RectField(rect, labelWidth, label, value);
            action?.Invoke(rect);
        }
        public static Rect RectField(Rect rect, Rect value)
        {
            return RectField(rect, "", value);
        }
        public static Rect RectField(Rect rect, string label, Rect value)
        {
            return RectField(rect, sm_LabelOriginWidth, label, value);
        }
        public static Rect RectField(Rect rect, float labelWidth, string label, Rect value)
        {
            return RectField(rect, labelWidth, new GUIContent(label), value);
        }
        public static Rect RectField(Rect rect, GUIContent label, Rect value)
        {
            return RectField(rect, sm_LabelOriginWidth, label, value);
        }
        public static Rect RectField(Rect rect, float labelWidth, GUIContent label, Rect value)
        {
            ChangeLabelWidth(labelWidth);

            Rect result = EditorGUI.RectField(rect, label, value);
            UpdateLastRect(rect);

            ResetLabelWidth();

            return result;
        }

        public static void RectIntField(Rect rect, RectInt value, Action<RectInt> action)
        {
            RectIntField(rect, "", value, action);
        }
        public static void RectIntField(Rect rect, string label, RectInt value, Action<RectInt> action)
        {
            RectIntField(rect, sm_LabelOriginWidth, label, value, action);
        }
        public static void RectIntField(Rect rect, float labelWidth, string label, RectInt value, Action<RectInt> action)
        {
            RectIntField(rect, labelWidth, new GUIContent(label), value, action);
        }
        public static void RectIntField(Rect rect, GUIContent label, RectInt value, Action<RectInt> action)
        {
            RectIntField(rect, sm_LabelOriginWidth, label, value, action);
        }
        public static void RectIntField(Rect rect, float labelWidth, GUIContent label, RectInt value, Action<RectInt> action)
        {
            value = RectIntField(rect, labelWidth, label, value);
            action?.Invoke(value);
        }
        public static RectInt RectIntField(Rect rect, RectInt value)
        {
            return RectIntField(rect, "", value);
        }
        public static RectInt RectIntField(Rect rect, string label, RectInt value)
        {
            return RectIntField(rect, sm_LabelOriginWidth, label, value);
        }
        public static RectInt RectIntField(Rect rect, float labelWidth, string label, RectInt value)
        {
            return RectIntField(rect, labelWidth, new GUIContent(label), value);
        }
        public static RectInt RectIntField(Rect rect, GUIContent label, RectInt value)
        {
            return RectIntField(rect, sm_LabelOriginWidth, label, value);
        }
        public static RectInt RectIntField(Rect rect, float labelWidth, GUIContent label, RectInt value)
        {
            ChangeLabelWidth(labelWidth);

            RectInt result = EditorGUI.RectIntField(rect, label, value);
            UpdateLastRect(rect);

            ResetLabelWidth();

            return result;
        }

        public static void LayerField(Rect rect, int layer, Action<int> action, GUIStyle style = null)
        {
            LayerField(rect, "", layer, action, style);
        }
        public static void LayerField(Rect rect, string label, int layer, Action<int> action, GUIStyle style = null)
        {
            LayerField(rect, sm_LabelOriginWidth, label, layer, action, style);
        }
        public static void LayerField(Rect rect, float labelWidth, string label, int layer, Action<int> action, GUIStyle style = null)
        {
            LayerField(rect, labelWidth, new GUIContent(label), layer, action, style);
        }
        public static void LayerField(Rect rect, GUIContent label, int layer, Action<int> action, GUIStyle style = null)
        {
            LayerField(rect, sm_LabelOriginWidth, label, layer, action, style);
        }
        public static void LayerField(Rect rect, float labelWidth, GUIContent label, int layer, Action<int> action, GUIStyle style = null)
        {
            layer = LayerField(rect, labelWidth, label, layer, style);
            action?.Invoke(layer);
        }
        public static int LayerField(Rect rect, int layer, GUIStyle style = null)
        {
            return LayerField(rect, "", layer, style);
        }
        public static int LayerField(Rect rect, string label, int layer, GUIStyle style = null)
        {
            return LayerField(rect, sm_LabelOriginWidth, label, layer, style);
        }
        public static int LayerField(Rect rect, float labelWidth, string label, int layer, GUIStyle style = null)
        {
            return LayerField(rect, labelWidth, new GUIContent(label), layer, style);
        }
        public static int LayerField(Rect rect, GUIContent label, int layer, GUIStyle style = null)
        {
            return LayerField(rect, sm_LabelOriginWidth, label, layer, style);
        }
        public static int LayerField(Rect rect, float labelWidth, GUIContent label, int layer, GUIStyle style = null)
        {
            ChangeLabelWidth(labelWidth);

            style = style ?? EditorStyles.popup;
            int result = EditorGUI.LayerField(rect, label, layer, style);
            UpdateLastRect(rect);

            ResetLabelWidth();

            return result;
        }

        public static void MaskField(Rect rect, int mask, string[] displayedOptions, Action<int> action, GUIStyle style = null)
        {
            MaskField(rect, "", mask, displayedOptions, action, style);
        }
        public static void MaskField(Rect rect, string label, int mask, string[] displayedOptions, Action<int> action, GUIStyle style = null)
        {
            MaskField(rect, sm_LabelOriginWidth, label, mask, displayedOptions, action, style);
        }
        public static void MaskField(Rect rect, float labelWidth, string label, int mask, string[] displayedOptions, Action<int> action, GUIStyle style = null)
        {
            MaskField(rect, labelWidth, new GUIContent(label), mask, displayedOptions, action, style);
        }
        public static void MaskField(Rect rect, GUIContent label, int mask, string[] displayedOptions, Action<int> action, GUIStyle style = null)
        {
            MaskField(rect, sm_LabelOriginWidth, label, mask, displayedOptions, action, style);
        }
        public static void MaskField(Rect rect, float labelWidth, GUIContent label, int mask, string[] displayedOptions, Action<int> action, GUIStyle style = null)
        {
            mask = MaskField(rect, labelWidth, label, mask, displayedOptions, style);
            action?.Invoke(mask);
        }
        public static int MaskField(Rect rect, int mask, string[] displayedOptions, GUIStyle style = null)
        {
            return MaskField(rect, "", mask, displayedOptions, style);
        }
        public static int MaskField(Rect rect, string label, int mask, string[] displayedOptions, GUIStyle style = null)
        {
            return MaskField(rect, sm_LabelOriginWidth, label, mask, displayedOptions, style);
        }
        public static int MaskField(Rect rect, float labelWidth, string label, int mask, string[] displayedOptions, GUIStyle style = null)
        {
            return MaskField(rect, labelWidth, new GUIContent(label), mask, displayedOptions, style);
        }
        public static int MaskField(Rect rect, GUIContent label, int mask, string[] displayedOptions, GUIStyle style = null)
        {
            return MaskField(rect, sm_LabelOriginWidth, label, mask, displayedOptions, style);
        }
        public static int MaskField(Rect rect, float labelWidth, GUIContent label, int mask, string[] displayedOptions, GUIStyle style = null)
        {
            ChangeLabelWidth(labelWidth);

            style = style ?? EditorStyles.popup;
            int result = EditorGUI.MaskField(rect, label, mask, displayedOptions, style);
            UpdateLastRect(rect);

            ResetLabelWidth();

            return result;
        }

        public static void EnumFlagsField<T>(Rect rect, T value, Action<T> action) where T : Enum
        {
            EnumFlagsField(rect, "", value, action);
        }
        public static void EnumFlagsField<T>(Rect rect, string label, T value, Action<T> action) where T : Enum
        {
            EnumFlagsField(rect, sm_LabelOriginWidth, label, value, action);
        }
        public static void EnumFlagsField<T>(Rect rect, float labelWidth, string label, T value, Action<T> action) where T : Enum
        {
            EnumFlagsField(rect, labelWidth, new GUIContent(label), value, action);
        }
        public static void EnumFlagsField<T>(Rect rect, GUIContent label, T value, Action<T> action) where T : Enum
        {
            EnumFlagsField(rect, sm_LabelOriginWidth, label, value, action);
        }
        public static void EnumFlagsField<T>(Rect rect, float labelWidth, GUIContent label, T value, Action<T> action) where T : Enum
        {
            value = EnumFlagsField(rect, labelWidth, label, value);
            action?.Invoke(value);
        }
        public static T EnumFlagsField<T>(Rect rect, T value) where T : Enum
        {
            return EnumFlagsField(rect, "", value);
        }
        public static T EnumFlagsField<T>(Rect rect, string label, T value) where T : Enum
        {
            return EnumFlagsField(rect, sm_LabelOriginWidth, label, value);
        }
        public static T EnumFlagsField<T>(Rect rect, float labelWidth, string label, T value) where T : Enum
        {
            return EnumFlagsField(rect, labelWidth, new GUIContent(label), value);
        }
        public static T EnumFlagsField<T>(Rect rect, GUIContent label, T value) where T : Enum
        {
            return EnumFlagsField(rect, sm_LabelOriginWidth, label, value);
        }
        public static T EnumFlagsField<T>(Rect rect, float labelWidth, GUIContent label, T value) where T : Enum
        {
            ChangeLabelWidth(labelWidth);

            T result = (T)EditorGUI.EnumFlagsField(rect, label, value);
            UpdateLastRect(rect);

            ResetLabelWidth();

            return result;
        }

        public static void EnumPopup<T>(Rect rect, T selected, Action<T> action) where T : Enum
        {
            EnumPopup(rect, "", selected, action);
        }
        public static void EnumPopup<T>(Rect rect, string label, T selected, Action<T> action) where T : Enum
        {
            EnumPopup(rect, sm_LabelOriginWidth, label, selected, action);
        }
        public static void EnumPopup<T>(Rect rect, float labelWidth, string label, T selected, Action<T> action) where T : Enum
        {
            EnumPopup(rect, labelWidth, new GUIContent(label), selected, action);
        }
        public static void EnumPopup<T>(Rect rect, GUIContent label, T selected, Action<T> action) where T : Enum
        {
            EnumPopup(rect, sm_LabelOriginWidth, label, selected, action);
        }
        public static void EnumPopup<T>(Rect rect, float labelWidth, GUIContent label, T selected, Action<T> action) where T : Enum
        {
            selected = EnumPopup(rect, labelWidth, label, selected);
            action?.Invoke(selected);
        }
        public static T EnumPopup<T>(Rect rect, T selected) where T : Enum
        {
            return EnumPopup(rect, "", selected);
        }
        public static T EnumPopup<T>(Rect rect, string label, T selected) where T : Enum
        {
            return EnumPopup(rect, sm_LabelOriginWidth, label, selected);
        }
        public static T EnumPopup<T>(Rect rect, float labelWidth, string label, T selected) where T : Enum
        {
            return EnumPopup(rect, labelWidth, new GUIContent(label), selected);
        }
        public static T EnumPopup<T>(Rect rect, GUIContent label, T selected) where T : Enum
        {
            return EnumPopup(rect, sm_LabelOriginWidth, label, selected);
        }
        public static T EnumPopup<T>(Rect rect, float labelWidth, GUIContent label, T selected) where T : Enum
        {
            ChangeLabelWidth(labelWidth);

            T result = (T)EditorGUI.EnumPopup(rect, label, selected);
            UpdateLastRect(rect);

            ResetLabelWidth();

            return result;
        }

        public static void Popup(Rect rect, int selectedIndex, string[] displayedOptions, Action<int> action)
        {
            Popup(rect, "", selectedIndex, displayedOptions, action);
        }
        public static void Popup(Rect rect, string label, int selectedIndex, string[] displayedOptions, Action<int> action)
        {
            Popup(rect, sm_LabelOriginWidth, label, selectedIndex, displayedOptions, action);
        }
        public static void Popup(Rect rect, float labelWidth, string label, int selectedIndex, string[] displayedOptions, Action<int> action)
        {
            Popup(rect, labelWidth, new GUIContent(label), selectedIndex, displayedOptions, action);
        }
        public static void Popup(Rect rect, GUIContent label, int selectedIndex, string[] displayedOptions, Action<int> action)
        {
            Popup(rect, sm_LabelOriginWidth, label, selectedIndex, displayedOptions, action);
        }
        public static void Popup(Rect rect, float labelWidth, GUIContent label, int selectedIndex, string[] displayedOptions, Action<int> action)
        {
            Popup(rect, labelWidth, label, selectedIndex, TempContents(displayedOptions), action);
        }
        public static void Popup(Rect rect, int selectedIndex, GUIContent[] displayedOptions, Action<int> action)
        {
            Popup(rect, "", selectedIndex, displayedOptions, action);
        }
        public static void Popup(Rect rect, string label, int selectedIndex, GUIContent[] displayedOptions, Action<int> action)
        {
            Popup(rect, sm_LabelOriginWidth, label, selectedIndex, displayedOptions, action);
        }
        public static void Popup(Rect rect, float labelWidth, string label, int selectedIndex, GUIContent[] displayedOptions, Action<int> action)
        {
            Popup(rect, labelWidth, new GUIContent(label), selectedIndex, displayedOptions, action);
        }
        public static void Popup(Rect rect, GUIContent label, int selectedIndex, GUIContent[] displayedOptions, Action<int> action)
        {
            Popup(rect, sm_LabelOriginWidth, label, selectedIndex, displayedOptions, action);
        }
        public static void Popup(Rect rect, float labelWidth, GUIContent label, int selectedIndex, GUIContent[] displayedOptions, Action<int> action)
        {
            selectedIndex = Popup(rect, labelWidth, label, selectedIndex, displayedOptions);
            action?.Invoke(selectedIndex);
        }
        public static int Popup(Rect rect, int selectedIndex, string[] displayedOptions)
        {
            return Popup(rect, "", selectedIndex, TempContents(displayedOptions));
        }
        public static int Popup(Rect rect, string label, int selectedIndex, string[] displayedOptions)
        {
            return Popup(rect, sm_LabelOriginWidth, label, selectedIndex, TempContents(displayedOptions));
        }
        public static int Popup(Rect rect, float labelWidth, string label, int selectedIndex, string[] displayedOptions)
        {
            return Popup(rect, labelWidth, new GUIContent(label), selectedIndex, TempContents(displayedOptions));
        }
        public static int Popup(Rect rect, GUIContent label, int selectedIndex, string[] displayedOptions)
        {
            return Popup(rect, sm_LabelOriginWidth, label, selectedIndex, TempContents(displayedOptions));
        }
        public static int Popup(Rect rect, float labelWidth, GUIContent label, int selectedIndex, string[] displayedOptions)
        {
            return Popup(rect, labelWidth, label, selectedIndex, TempContents(displayedOptions));
        }
        public static int Popup(Rect rect, int selectedIndex, GUIContent[] displayedOptions)
        {
            return Popup(rect, "", selectedIndex, displayedOptions);
        }
        public static int Popup(Rect rect, string label, int selectedIndex, GUIContent[] displayedOptions)
        {
            return Popup(rect, sm_LabelOriginWidth, label, selectedIndex, displayedOptions);
        }
        public static int Popup(Rect rect, float labelWidth, string label, int selectedIndex, GUIContent[] displayedOptions)
        {
            return Popup(rect, labelWidth, new GUIContent(label), selectedIndex, displayedOptions);
        }
        public static int Popup(Rect rect, GUIContent label, int selectedIndex, GUIContent[] displayedOptions)
        {
            return Popup(rect, sm_LabelOriginWidth, label, selectedIndex, displayedOptions);
        }
        public static int Popup(Rect rect, float labelWidth, GUIContent label, int selectedIndex, GUIContent[] displayedOptions)
        {
            ChangeLabelWidth(labelWidth);

            int result = EditorGUI.Popup(rect, label, selectedIndex, displayedOptions);
            UpdateLastRect(rect);

            ResetLabelWidth();

            return result;
        }

        public static void Slider(Rect rect, float value, float leftValue, float rightValue, Action<float> action)
        {
            Slider(rect, "", value, leftValue, rightValue, action);
        }
        public static void Slider(Rect rect, string label, float value, float leftValue, float rightValue, Action<float> action)
        {
            Slider(rect, sm_LabelOriginWidth, label, value, leftValue, rightValue, action);
        }
        public static void Slider(Rect rect, float labelWidth, string label, float value, float leftValue, float rightValue, Action<float> action)
        {
            Slider(rect, labelWidth, new GUIContent(label), value, leftValue, rightValue, action);
        }
        public static void Slider(Rect rect, GUIContent label, float value, float leftValue, float rightValue, Action<float> action)
        {
            Slider(rect, sm_LabelOriginWidth, label, value, leftValue, rightValue, action);
        }
        public static void Slider(Rect rect, float labelWidth, GUIContent label, float value, float leftValue, float rightValue, Action<float> action)
        {
            value = Slider(rect, labelWidth, label, value, leftValue, rightValue);
            action?.Invoke(value);
        }
        public static float Slider(Rect rect, float value, float leftValue, float rightValue)
        {
            return Slider(rect, "", value, leftValue, rightValue);
        }
        public static float Slider(Rect rect, string label, float value, float leftValue, float rightValue)
        {
            return Slider(rect, sm_LabelOriginWidth, label, value, leftValue, rightValue);
        }
        public static float Slider(Rect rect, float labelWidth, string label, float value, float leftValue, float rightValue)
        {
            return Slider(rect, labelWidth, new GUIContent(label), value, leftValue, rightValue);
        }
        public static float Slider(Rect rect, GUIContent label, float value, float leftValue, float rightValue)
        {
            return Slider(rect, sm_LabelOriginWidth, label, value, leftValue, rightValue);
        }
        public static float Slider(Rect rect, float labelWidth, GUIContent label, float value, float leftValue, float rightValue)
        {
            ChangeLabelWidth(labelWidth);

            float result = EditorGUI.Slider(rect, label, value, leftValue, rightValue);
            UpdateLastRect(rect);

            ResetLabelWidth();

            return result;
        }

        public static void IntSlider(Rect rect, int value, int leftValue, int rightValue, Action<int> action)
        {
            IntSlider(rect, "", value, leftValue, rightValue, action);
        }
        public static void IntSlider(Rect rect, string label, int value, int leftValue, int rightValue, Action<int> action)
        {
            IntSlider(rect, sm_LabelOriginWidth, label, value, leftValue, rightValue, action);
        }
        public static void IntSlider(Rect rect, float labelWidth, string label, int value, int leftValue, int rightValue, Action<int> action)
        {
            IntSlider(rect, labelWidth, new GUIContent(label), value, leftValue, rightValue, action);
        }
        public static void IntSlider(Rect rect, GUIContent label, int value, int leftValue, int rightValue, Action<int> action)
        {
            IntSlider(rect, sm_LabelOriginWidth, label, value, leftValue, rightValue, action);
        }
        public static void IntSlider(Rect rect, float labelWidth, GUIContent label, int value, int leftValue, int rightValue, Action<int> action)
        {
            value = IntSlider(rect, labelWidth, label, value, leftValue, rightValue);
            action?.Invoke(value);
        }
        public static int IntSlider(Rect rect, int value, int leftValue, int rightValue)
        {
            return IntSlider(rect, "", value, leftValue, rightValue);
        }
        public static int IntSlider(Rect rect, string label, int value, int leftValue, int rightValue)
        {
            return IntSlider(rect, sm_LabelOriginWidth, label, value, leftValue, rightValue);
        }
        public static int IntSlider(Rect rect, float labelWidth, string label, int value, int leftValue, int rightValue)
        {
            return IntSlider(rect, labelWidth, new GUIContent(label), value, leftValue, rightValue);
        }
        public static int IntSlider(Rect rect, GUIContent label, int value, int leftValue, int rightValue)
        {
            return IntSlider(rect, sm_LabelOriginWidth, label, value, leftValue, rightValue);
        }
        public static int IntSlider(Rect rect, float labelWidth, GUIContent label, int value, int leftValue, int rightValue)
        {
            ChangeLabelWidth(labelWidth);

            int result = EditorGUI.IntSlider(rect, label, value, leftValue, rightValue);
            UpdateLastRect(rect);

            ResetLabelWidth();

            return result;
        }

        public static void Foldout(Rect rect, bool foldout, bool toggleOnLabelClick, Action<bool> action, GUIStyle background = null)
        {
            Foldout(rect, "", foldout, toggleOnLabelClick, action, background);
        }
        public static void Foldout(Rect rect, string label, bool foldout, bool toggleOnLabelClick, Action<bool> action, GUIStyle background = null)
        {
            Foldout(rect, sm_LabelOriginWidth, label, foldout, toggleOnLabelClick, action, background);
        }
        public static void Foldout(Rect rect, float labelWidth, string label, bool foldout, bool toggleOnLabelClick, Action<bool> action, GUIStyle background = null)
        {
            Foldout(rect, labelWidth, new GUIContent(label), foldout, toggleOnLabelClick, action, background);
        }
        public static void Foldout(Rect rect, GUIContent label, bool foldout, bool toggleOnLabelClick, Action<bool> action, GUIStyle background = null)
        {
            Foldout(rect, sm_LabelOriginWidth, label, foldout, toggleOnLabelClick, action, background);
        }
        public static void Foldout(Rect rect, float labelWidth, GUIContent label, bool foldout, bool toggleOnLabelClick, Action<bool> action, GUIStyle background = null)
        {
            foldout = Foldout(rect, labelWidth, label, foldout, toggleOnLabelClick, background);
            action?.Invoke(foldout);
        }
        public static bool Foldout(Rect rect, bool foldout, bool toggleOnLabelClick, GUIStyle background = null)
        {
            return Foldout(rect, "", foldout, toggleOnLabelClick, background);
        }
        public static bool Foldout(Rect rect, string label, bool foldout, bool toggleOnLabelClick, GUIStyle background = null)
        {
            return Foldout(rect, sm_LabelOriginWidth, label, foldout, toggleOnLabelClick, background);
        }
        public static bool Foldout(Rect rect, float labelWidth, string label, bool foldout, bool toggleOnLabelClick, GUIStyle background = null)
        {
            return Foldout(rect, labelWidth, new GUIContent(label), foldout, toggleOnLabelClick, background);
        }
        public static bool Foldout(Rect rect, GUIContent label, bool foldout, bool toggleOnLabelClick, GUIStyle background = null)
        {
            return Foldout(rect, sm_LabelOriginWidth, label, foldout, toggleOnLabelClick, background);
        }
        public static bool Foldout(Rect rect, float labelWidth, GUIContent label, bool foldout, bool toggleOnLabelClick, GUIStyle background = null)
        {
            ChangeLabelWidth(labelWidth);

            if (background != null)
                Image(rect, background);

            bool result = EditorGUI.Foldout(rect, foldout, label, toggleOnLabelClick);
            UpdateLastRect(rect);

            ResetLabelWidth();

            return result;
        }

        public static void TextField(Rect rect, string value, Action<string> action)
        {
            TextField(rect, "", value, action);
        }
        public static void TextField(Rect rect, string label, string value, Action<string> action)
        {
            TextField(rect, sm_LabelOriginWidth, label, value, action);
        }
        public static void TextField(Rect rect, float labelWidth, string label, string value, Action<string> action)
        {
            TextField(rect, labelWidth, new GUIContent(label), value, action);
        }
        public static void TextField(Rect rect, GUIContent label, string value, Action<string> action)
        {
            TextField(rect, sm_LabelOriginWidth, label, value, action);
        }
        public static void TextField(Rect rect, float labelWidth, GUIContent label, string value, Action<string> action)
        {
            value = TextField(rect, labelWidth, label, value);
            action?.Invoke(value);
        }
        public static string TextField(Rect rect, string value)
        {
            return TextField(rect, "", value);
        }
        public static string TextField(Rect rect, string label, string value)
        {
            return TextField(rect, sm_LabelOriginWidth, label, value); ;
        }
        public static string TextField(Rect rect, float labelWidth, string label, string value)
        {
            return TextField(rect, labelWidth, new GUIContent(label), value);
        }
        public static string TextField(Rect rect, GUIContent label, string value)
        {
            return TextField(rect, sm_LabelOriginWidth, label, value);
        }
        public static string TextField(Rect rect, float labelWidth, GUIContent label, string value)
        {
            ChangeLabelWidth(labelWidth);

            string result = EditorGUI.TextField(rect, label, value);
            UpdateLastRect(rect);

            ResetLabelWidth();

            return result;
        }

        public static void IntField(Rect rect, int value, Action<int> action)
        {
            IntField(rect, "", value, action);
        }
        public static void IntField(Rect rect, string label, int value, Action<int> action)
        {
            IntField(rect, sm_LabelOriginWidth, label, value, action);
        }
        public static void IntField(Rect rect, float labelWidth, string label, int value, Action<int> action)
        {
            IntField(rect, labelWidth, new GUIContent(label), value, action);
        }
        public static void IntField(Rect rect, GUIContent label, int value, Action<int> action)
        {
            IntField(rect, sm_LabelOriginWidth, label, value, action);
        }
        public static void IntField(Rect rect, float labelWidth, GUIContent label, int value, Action<int> action)
        {
            value = IntField(rect, labelWidth, label, value);
            action?.Invoke(value);
        }
        public static int IntField(Rect rect, int value)
        {
            return IntField(rect, "", value);
        }
        public static int IntField(Rect rect, string label, int value)
        {
            return IntField(rect, sm_LabelOriginWidth, label, value);
        }
        public static int IntField(Rect rect, float labelWidth, string label, int value)
        {
            return IntField(rect, labelWidth, new GUIContent(label), value);
        }
        public static int IntField(Rect rect, GUIContent label, int value)
        {
            return IntField(rect, sm_LabelOriginWidth, label, value);
        }
        public static int IntField(Rect rect, float labelWidth, GUIContent label, int value)
        {
            ChangeLabelWidth(labelWidth);

            int result = EditorGUI.IntField(rect, label, value);
            UpdateLastRect(rect);

            ResetLabelWidth();

            return result;
        }

        public static void FloatField(Rect rect, float value, Action<float> action)
        {
            FloatField(rect, "", value, action);
        }
        public static void FloatField(Rect rect, string label, float value, Action<float> action)
        {
            FloatField(rect, sm_LabelOriginWidth, label, value, action);
        }
        public static void FloatField(Rect rect, float labelWidth, string label, float value, Action<float> action)
        {
            FloatField(rect, labelWidth, new GUIContent(label), value, action);
        }
        public static void FloatField(Rect rect, GUIContent label, float value, Action<float> action)
        {
            FloatField(rect, sm_LabelOriginWidth, label, value, action);
        }
        public static void FloatField(Rect rect, float labelWidth, GUIContent label, float value, Action<float> action)
        {
            value = FloatField(rect, labelWidth, label, value);
            action?.Invoke(value);
        }
        public static float FloatField(Rect rect, float value)
        {
            return FloatField(rect, value);
        }
        public static float FloatField(Rect rect, string label, float value)
        {
            return FloatField(rect, sm_LabelOriginWidth, label, value);
        }
        public static float FloatField(Rect rect, float labelWidth, string label, float value)
        {
            return FloatField(rect, labelWidth, new GUIContent(label), value);
        }
        public static float FloatField(Rect rect, GUIContent label, float value)
        {
            return FloatField(rect, sm_LabelOriginWidth, label, value);
        }
        public static float FloatField(Rect rect, float labelWidth, GUIContent label, float value)
        {
            ChangeLabelWidth(labelWidth);

            float result = EditorGUI.FloatField(rect, label, value);
            UpdateLastRect(rect);

            ResetLabelWidth();

            return result;
        }

        public static void DoubleField(Rect rect, double value, Action<double> action)
        {
            DoubleField(rect, "", value, action);
        }
        public static void DoubleField(Rect rect, string label, double value, Action<double> action)
        {
            DoubleField(rect, sm_LabelOriginWidth, label, value, action);
        }
        public static void DoubleField(Rect rect, float labelWidth, string label, double value, Action<double> action)
        {
            DoubleField(rect, labelWidth, new GUIContent(label), value, action);
        }
        public static void DoubleField(Rect rect, GUIContent label, double value, Action<double> action)
        {
            DoubleField(rect, sm_LabelOriginWidth, label, value, action);
        }
        public static void DoubleField(Rect rect, float labelWidth, GUIContent label, double value, Action<double> action)
        {
            value = DoubleField(rect, labelWidth, label, value);
            action?.Invoke(value);
        }
        public static double DoubleField(Rect rect, double value)
        {
            return DoubleField(rect, "", value);
        }
        public static double DoubleField(Rect rect, string label, double value)
        {
            return DoubleField(rect, sm_LabelOriginWidth, label, value);
        }
        public static double DoubleField(Rect rect, float labelWidth, string label, double value)
        {
            return DoubleField(rect, labelWidth, new GUIContent(label), value);
        }
        public static double DoubleField(Rect rect, GUIContent label, double value)
        {
            return DoubleField(rect, sm_LabelOriginWidth, label, value);
        }
        public static double DoubleField(Rect rect, float labelWidth, GUIContent label, double value)
        {
            ChangeLabelWidth(labelWidth);

            double result = EditorGUI.DoubleField(rect, label, value);
            UpdateLastRect(rect);

            ResetLabelWidth();

            return result;
        }

        public static void LongField(Rect rect, long value, Action<long> action)
        {
            LongField(rect, "", value, action);
        }
        public static void LongField(Rect rect, string label, long value, Action<long> action)
        {
            LongField(rect, sm_LabelOriginWidth, label, value, action);
        }
        public static void LongField(Rect rect, float labelWidth, string label, long value, Action<long> action)
        {
            LongField(rect, labelWidth, new GUIContent(label), value, action);
        }
        public static void LongField(Rect rect, GUIContent label, long value, Action<long> action)
        {
            LongField(rect, sm_LabelOriginWidth, label, value, action);
        }
        public static void LongField(Rect rect, float labelWidth, GUIContent label, long value, Action<long> action)
        {
            value = LongField(rect, labelWidth, label, value);
            action?.Invoke(value);
        }
        public static long LongField(Rect rect, long value)
        {
            return LongField(rect, "", value);
        }
        public static long LongField(Rect rect, string label, long value)
        {
            return LongField(rect, sm_LabelOriginWidth, label, value);
        }
        public static long LongField(Rect rect, float labelWidth, string label, long value)
        {
            return LongField(rect, labelWidth, new GUIContent(label), value);
        }
        public static long LongField(Rect rect, GUIContent label, long value)
        {
            return LongField(rect, sm_LabelOriginWidth, label, value);
        }
        public static long LongField(Rect rect, float labelWidth, GUIContent label, long value)
        {
            ChangeLabelWidth(labelWidth);

            long result = EditorGUI.LongField(rect, label, value);
            UpdateLastRect(rect);

            ResetLabelWidth();

            return result;
        }

        public static void ToggleRight(Rect rect, bool value, Action<bool> action, GUIStyle style = null)
        {
            ToggleRight(rect, sm_LabelOriginWidth, "", value, action, style);
        }
        public static void ToggleRight(Rect rect, string label, bool value, Action<bool> action, GUIStyle style = null)
        {
            ToggleRight(rect, sm_LabelOriginWidth, label, value, action, style);
        }
        public static void ToggleRight(Rect rect, float labelWidth, string label, bool value, Action<bool> action, GUIStyle style = null)
        {
            ToggleRight(rect, labelWidth, new GUIContent(label), value, action, style);
        }
        public static void ToggleRight(Rect rect, GUIContent label, bool value, Action<bool> action, GUIStyle style = null)
        {
            ToggleRight(rect, sm_LabelOriginWidth, label, value, action, style);
        }
        public static void ToggleRight(Rect rect, float labelWidth, GUIContent label, bool value, Action<bool> action, GUIStyle style = null)
        {
            value = ToggleRight(rect, labelWidth, label, value, style);
            action?.Invoke(value);
        }
        public static bool ToggleRight(Rect rect, bool value, GUIStyle style = null)
        {
            return ToggleRight(rect, sm_LabelOriginWidth, "", value, style);
        }
        public static bool ToggleRight(Rect rect, string label, bool value, GUIStyle style = null)
        {
            return ToggleRight(rect, sm_LabelOriginWidth, label, value, style);
        }
        public static bool ToggleRight(Rect rect, float labelWidth, string label, bool value, GUIStyle style = null)
        {
            return ToggleRight(rect, labelWidth, new GUIContent(label), value, style);
        }
        public static bool ToggleRight(Rect rect, GUIContent label, bool value, GUIStyle style = null)
        {
            return ToggleRight(rect, sm_LabelOriginWidth, label, value, style);
        }
        public static bool ToggleRight(Rect rect, float labelWidth, GUIContent label, bool value, GUIStyle style = null)
        {
            ChangeLabelWidth(labelWidth);

            style = style ?? EditorStyles.toggle;
            bool result = EditorGUI.Toggle(rect, label, value, style);
            UpdateLastRect(rect);

            ResetLabelWidth();

            return result;
        }

        public static void ToggleLeft(Rect rect, bool value, Action<bool> action, GUIStyle style = null)
        {
            ToggleLeft(rect, sm_LabelOriginWidth, "", value, action, style);
        }
        public static void ToggleLeft(Rect rect, string label, bool value, Action<bool> action, GUIStyle style = null)
        {
            ToggleLeft(rect, sm_LabelOriginWidth, label, value, action, style);
        }
        public static void ToggleLeft(Rect rect, float labelWidth, string label, bool value, Action<bool> action, GUIStyle style = null)
        {
            ToggleLeft(rect, labelWidth, new GUIContent(label), value, action, style);
        }
        public static void ToggleLeft(Rect rect, GUIContent label, bool value, Action<bool> action, GUIStyle style = null)
        {
            ToggleLeft(rect, sm_LabelOriginWidth, label, value, action, style);
        }
        public static void ToggleLeft(Rect rect, float labelWidth, GUIContent label, bool value, Action<bool> action, GUIStyle style = null)
        {
            value = ToggleLeft(rect, labelWidth, label, value, style);
            action?.Invoke(value);
        }
        public static bool ToggleLeft(Rect rect, bool value, GUIStyle style = null)
        {
            return ToggleLeft(rect, sm_LabelOriginWidth, "", value, style);
        }
        public static bool ToggleLeft(Rect rect, string label, bool value, GUIStyle style = null)
        {
            return ToggleLeft(rect, sm_LabelOriginWidth, label, value, style);
        }
        public static bool ToggleLeft(Rect rect, float labelWidth, string label, bool value, GUIStyle style = null)
        {
            return ToggleLeft(rect, labelWidth, new GUIContent(label), value, style);
        }
        public static bool ToggleLeft(Rect rect, GUIContent label, bool value, GUIStyle style = null)
        {
            return ToggleLeft(rect, sm_LabelOriginWidth, label, value, style);
        }
        public static bool ToggleLeft(Rect rect, float labelWidth, GUIContent label, bool value, GUIStyle style = null)
        {
            ChangeLabelWidth(labelWidth);

            style = style ?? EditorStyles.toggle;
            bool result = EditorGUI.ToggleLeft(rect, label, value, style);
            UpdateLastRect(rect);

            ResetLabelWidth();

            return result;
        }

        public static void Label(Rect rect, string text, GUIStyle style = null)
        {
            Label(rect, new GUIContent(text), style);
        }
        public static void Label(Rect rect, GUIContent text, GUIStyle style = null)
        {
            style = style ?? EditorStyles.label;
            GUI.Label(rect, text, style);
            UpdateLastRect(rect);
        }

        public static void Image(Rect rect, GUIStyle style = null)
        {
            Image(rect, "", style);
        }
        public static void Image(Rect rect, string text, GUIStyle style = null)
        {
            Image(rect, new GUIContent(text), style);
        }
        public static void Image(Rect rect, GUIContent text, GUIStyle style = null)
        {
            style = style ?? GUI.skin.box;

            GUI.Box(rect, text, style);
            UpdateLastRect(rect);
        }
        public static void Image(Rect rect, Texture texture)
        {
            GUI.DrawTexture(rect, texture);
            UpdateLastRect(rect);
        }

        public static void Button(Rect rect, string text, Action action, GUIStyle style = null)
        {
            if (Button(rect, text, style))
                action?.Invoke();
        }
        public static bool Button(Rect rect, string text, GUIStyle style = null)
        {
            return Button(rect, new GUIContent(text), style);
        }
        public static bool Button(Rect rect, GUIContent text, GUIStyle style = null)
        {
            style = style ?? GUI.skin.button;

            bool result = GUI.Button(rect, text, style);
            UpdateLastRect(rect);

            return result;
        }

        public static void Rect(Rect rect, Color color)
        {
            EditorGUI.DrawRect(rect, color);
            UpdateLastRect(rect);
        }

        private static void ChangeLabelWidth(float width)
        {
            EditorGUIUtility.labelWidth = width;
        }
        private static void ResetLabelWidth()
        {
            EditorGUIUtility.labelWidth = sm_LabelOriginWidth;
        }

        private static void UpdateLastRect(Rect rect)
        {
            GUILastRect = rect;
        }

        private static GUIContent[] TempContents(string[] texts)
        {
            GUIContent[] contents = new GUIContent[texts.Length];

            for (int i = 0; i < contents.Length; i++)
                contents[i] = new GUIContent(texts[i]);

            return contents;
        }
    }

    public class EniGUILayoutOption
    {
        public enum TypeOption
        {
            SetPudding,
            SetElementSpacing,
            SetExpandWidth,
            SetExpandHeight,
            SetWidth,
            SetHeight,
            SetClickable,
        }

        public TypeOption Type { get; private set; }
        public object Value { get; private set; }

        public EniGUILayoutOption(TypeOption type, object value)
        {
            Type = type;
            Value = value;
        }

        internal void Reset(TypeOption type, object value)
        {
            Type = type;
            Value = value;
        }
    }

    public static class GUIElementGUID
    {
        public static int GetGUID(Vector2 position, object value, string name)
        {
            string guid = $"{position}_{value}_{name}";
            return guid.GetHashCode();
        }
    }

    internal class GUIGroupPool
    {
        private Queue<GUIGroup> m_Groups = new Queue<GUIGroup>();

        public GUIGroup Get()
        {
            if (m_Groups.TryDequeue(out GUIGroup group) == false)
                group = new GUIGroup();

            return group;
        }

        public void Return(GUIGroup group)
        {
            if (group == null)
                throw new ArgumentNullException("group");

            if (m_Groups.Count >= 1000 || group is GUIScrollView)
                return;

            group.Reset();
            m_Groups.Enqueue(group);
        }
    }

    internal class GUIScrollViewPool
    {
        private Queue<GUIScrollView> m_ScrollViews = new Queue<GUIScrollView>();

        public GUIScrollView Get()
        {
            if (m_ScrollViews.TryDequeue(out GUIScrollView scrollView) == false)
                scrollView = new GUIScrollView();

            return scrollView;
        }

        public void Return(GUIScrollView scrollView)
        {
            if (scrollView == null)
                throw new ArgumentNullException("scrollView");

            if (m_ScrollViews.Count >= 1000)
                return;

            scrollView.Reset();
            m_ScrollViews.Enqueue(scrollView);
        }
    }

    internal class GUIElementPool
    {
        private Queue<GUIElement> m_Elements = new Queue<GUIElement>();

        public GUIElement Get(Rect rect)
        {
            if (m_Elements.TryDequeue(out GUIElement element) == false)
                element = new GUIElement(rect);
            
            element.Rect = rect;

            return element;
        }

        public void Return(GUIElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            if (m_Elements.Count >= 3000 || element is GUIGroup)
                return;

            m_Elements.Enqueue(element);
        }
    }

    internal class GUIOptionPool
    {
        private Queue<EniGUILayoutOption> m_Options = new Queue<EniGUILayoutOption>();

        public EniGUILayoutOption Get(EniGUILayoutOption.TypeOption type, object value)
        {
            if (m_Options.Count == 0)
                m_Options.Enqueue(new EniGUILayoutOption(type, value));

            EniGUILayoutOption option = m_Options.Dequeue();
            option.Reset(type, value);

            return option;
        }

        public void Return(EniGUILayoutOption option)
        {
            if (option == null)
                throw new ArgumentNullException("option");

            if (m_Options.Count >= 20)
                return;

            m_Options.Enqueue(option);
        }
    }

    public delegate void Command();

    public class CommandQueue
    {
        private Queue<Command> m_Commands = new Queue<Command>();

        public void Enqueue(Command command)
        {
            m_Commands.Enqueue(command);
        }

        public void Execute()
        {
            while (m_Commands.Count > 0)
            {
                Command command = m_Commands.Dequeue();
                command?.Invoke();
            }

            m_Commands.Clear();
        }

        public void Clear()
        {
            m_Commands.Clear();
        }
    }

    internal static class GUIDrawCommand
    {
        public static Command Group(GUIGroup group)
        {
            return () => 
            {
                if (group.Style != null)
                    Image(group.Rect, group.Style)?.Invoke();
                else if (group.Color != Color.clear)
                    Rect(group.Rect, group.Color)?.Invoke();
            };
        }

        public static Command BeginScrollView(GUIScrollView scrollView)
        {
            return () => 
            {
                BeginScrollView(scrollView.Rect, scrollView.ScrollPosition, scrollView.View,
                    scrollView.AlwaysShowHorizontal, scrollView.AlwaysShowVertical, scrollView.Action, 
                    scrollView.HorizontalScrollbar, scrollView.VerticalScrollbar)?.Invoke();
            };
        }
        public static Command BeginScrollView(Rect rect, Vector2 scrollPosition, Rect viewRect, Action<Vector2> action)
        {
            return () => { EniGUI.BeginScrollView(rect, scrollPosition, viewRect, action); };
        }
        public static Command BeginScrollView(Rect rect, Vector2 scrollPosition, Rect viewRect, bool alwaysShowHorizontal, bool alwaysShowVertical, Action<Vector2> action)
        {
            return () => { EniGUI.BeginScrollView(rect, scrollPosition, viewRect, alwaysShowHorizontal, alwaysShowVertical, action); };
        }
        public static Command BeginScrollView(Rect rect, Vector2 scrollPosition, Rect viewRect, Action<Vector2> action, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar)
        {
            return () => { EniGUI.BeginScrollView(rect, scrollPosition, viewRect, action, horizontalScrollbar, verticalScrollbar); };
        }
        public static Command BeginScrollView(Rect rect, Vector2 scrollPosition, Rect viewRect, bool alwaysShowHorizontal, bool alwaysShowVertical, Action<Vector2> action, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar)
        {
            return () => { EniGUI.BeginScrollView(rect, scrollPosition, viewRect, alwaysShowHorizontal, alwaysShowVertical, action, horizontalScrollbar, verticalScrollbar); };
        }

        public static Command EndScrollView()
        {
            return () => { EniGUI.EndScrollView(); };
        }

        public static Command PropertyField(Rect rect, SerializedProperty property, string label = "")
        {
            return () => { EniGUI.PropertyField(rect, property, label); };
        }
        public static Command PropertyField(Rect rect, float labelWidth, SerializedProperty property, string label = "")
        {
            return () => { EniGUI.PropertyField(rect, labelWidth, property, label); };
        }
        public static Command PropertyField(Rect rect, SerializedProperty property, GUIContent label = null)
        {
            return () => { EniGUI.PropertyField(rect, property, label); };
        }
        public static Command PropertyField(Rect rect, float labelWidth, SerializedProperty property, GUIContent label = null)
        {
            return () => { EniGUI.PropertyField(rect, labelWidth, property, label); };
        }

        public static Command ObjectField<T>(Rect rect, T value, Action<T> action, bool allowSceneObjects) where T : UnityEngine.Object
        {
            return () => { EniGUI.ObjectField(rect, value, action, allowSceneObjects); };
        }
        public static Command ObjectField<T>(Rect rect, string label, T value, Action<T> action, bool allowSceneObjects) where T : UnityEngine.Object
        {
            return () => { EniGUI.ObjectField(rect, label, value, action, allowSceneObjects); };
        }
        public static Command ObjectField<T>(Rect rect, float labelWidth, string label, T value, Action<T> action, bool allowSceneObjects) where T : UnityEngine.Object
        {
            return () => { EniGUI.ObjectField(rect, labelWidth, label, value, action, allowSceneObjects); };
        }
        public static Command ObjectField<T>(Rect rect, GUIContent label, T value, Action<T> action, bool allowSceneObjects) where T : UnityEngine.Object
        {
            return () => { EniGUI.ObjectField(rect, label, value, action, allowSceneObjects); };
        }
        public static Command ObjectField<T>(Rect rect, float labelWidth, GUIContent label, T value, Action<T> action, bool allowSceneObjects) where T : UnityEngine.Object
        {
            return () => { EniGUI.ObjectField(rect, labelWidth, label, value, action, allowSceneObjects); };
        }

        public static Command ColorField(Rect rect, Color value, Action<Color> action)
        {
            return () => { EniGUI.ColorField(rect, value, action); };
        }
        public static Command ColorField(Rect rect, string label, Color value, Action<Color> action)
        {
            return () => { EniGUI.ColorField(rect, label, value, action); };
        }
        public static Command ColorField(Rect rect, float labelWidth, string label, Color value, Action<Color> action)
        {
            return () => { EniGUI.ColorField(rect, labelWidth, label, value, action); };
        }
        public static Command ColorField(Rect rect, GUIContent label, Color value, Action<Color> action)
        {
            return () => { EniGUI.ColorField(rect, label, value, action); };
        }
        public static Command ColorField(Rect rect, float labelWidth, GUIContent label, Color value, Action<Color> action)
        {
            return () => { EniGUI.ColorField(rect, labelWidth, label, value, action); };
        }

        public static Command GradientField(Rect rect, Gradient value, Action<Gradient> action)
        {
            return () => { EniGUI.GradientField(rect, value, action); };
        }
        public static Command GradientField(Rect rect, string label, Gradient value, Action<Gradient> action)
        {
            return () => { EniGUI.GradientField(rect, label, value, action); };
        }
        public static Command GradientField(Rect rect, float labelWidth, string label, Gradient value, Action<Gradient> action)
        {
            return () => { EniGUI.GradientField(rect, labelWidth, label, value, action); };
        }
        public static Command GradientField(Rect rect, GUIContent label, Gradient value, Action<Gradient> action)
        {
            return () => { EniGUI.GradientField(rect, label, value, action); };
        }
        public static Command GradientField(Rect rect, float labelWidth, GUIContent label, Gradient value, Action<Gradient> action)
        {
            return () => { EniGUI.GradientField(rect, labelWidth, label, value, action); };
        }

        public static Command CurveField(Rect rect, AnimationCurve value, Action<AnimationCurve> action)
        {
            return () => { EniGUI.CurveField(rect, value, action); };
        }
        public static Command CurveField(Rect rect, string label, AnimationCurve value, Action<AnimationCurve> action)
        {
            return () => { EniGUI.CurveField(rect, label, value, action); };
        }
        public static Command CurveField(Rect rect, float labelWidth, string label, AnimationCurve value, Action<AnimationCurve> action)
        {
            return () => { EniGUI.CurveField(rect, labelWidth, label, value, action); };
        }
        public static Command CurveField(Rect rect, GUIContent label, AnimationCurve value, Action<AnimationCurve> action)
        {
            return () => { EniGUI.CurveField(rect, label, value, action); };
        }
        public static Command CurveField(Rect rect, float labelWidth, GUIContent label, AnimationCurve value, Action<AnimationCurve> action)
        {
            return () => { EniGUI.CurveField(rect, labelWidth, label, value, action); };
        }

        public static Command Vector2Field(Rect rect, Vector2 value, Action<Vector2> action)
        {
            return () => { EniGUI.Vector2Field(rect, value, action); };
        }
        public static Command Vector2Field(Rect rect, string label, Vector2 value, Action<Vector2> action)
        {
            return () => { EniGUI.Vector2Field(rect, label, value, action); };
        }
        public static Command Vector2Field(Rect rect, float labelWidth, string label, Vector2 value, Action<Vector2> action)
        {
            return () => { EniGUI.Vector2Field(rect, labelWidth, label, value, action); };
        }
        public static Command Vector2Field(Rect rect, GUIContent label, Vector2 value, Action<Vector2> action)
        {
            return () => { EniGUI.Vector2Field(rect, label, value, action); };
        }
        public static Command Vector2Field(Rect rect, float labelWidth, GUIContent label, Vector2 value, Action<Vector2> action)
        {
            return () => { EniGUI.Vector2Field(rect, labelWidth, label, value, action); };
        }

        public static Command Vector3Field(Rect rect, Vector3 value, Action<Vector3> action)
        {
            return () => { EniGUI.Vector3Field(rect, value, action); };
        }
        public static Command Vector3Field(Rect rect, string label, Vector3 value, Action<Vector3> action)
        {
            return () => { EniGUI.Vector3Field(rect, label, value, action); };
        }
        public static Command Vector3Field(Rect rect, float labelWidth, string label, Vector3 value, Action<Vector3> action)
        {
            return () => { EniGUI.Vector3Field(rect, labelWidth, label, value, action); };
        }
        public static Command Vector3Field(Rect rect, GUIContent label, Vector3 value, Action<Vector3> action)
        {
            return () => { EniGUI.Vector3Field(rect, label, value, action); };
        }
        public static Command Vector3Field(Rect rect, float labelWidth, GUIContent label, Vector3 value, Action<Vector3> action)
        {
            return () => { EniGUI.Vector3Field(rect, labelWidth, label, value, action); };
        }

        public static Command Vector2IntField(Rect rect, Vector2Int value, Action<Vector2Int> action)
        {
            return () => { EniGUI.Vector2IntField(rect, value, action); };
        }
        public static Command Vector2IntField(Rect rect, string label, Vector2Int value, Action<Vector2Int> action)
        {
            return () => { EniGUI.Vector2IntField(rect, label, value, action); };
        }
        public static Command Vector2IntField(Rect rect, float labelWidth, string label, Vector2Int value, Action<Vector2Int> action)
        {
            return () => { EniGUI.Vector2IntField(rect, labelWidth, label, value, action); };
        }
        public static Command Vector2IntField(Rect rect, GUIContent label, Vector2Int value, Action<Vector2Int> action)
        {
            return () => { EniGUI.Vector2IntField(rect, label, value, action); };
        }
        public static Command Vector2IntField(Rect rect, float labelWidth, GUIContent label, Vector2Int value, Action<Vector2Int> action)
        {
            return () => { EniGUI.Vector2IntField(rect, labelWidth, label, value, action); };
        }

        public static Command Vector3IntField(Rect rect, Vector3Int value, Action<Vector3Int> action)
        {
            return () => { EniGUI.Vector3IntField(rect, value, action); };
        }
        public static Command Vector3IntField(Rect rect, string label, Vector3Int value, Action<Vector3Int> action)
        {
            return () => { EniGUI.Vector3IntField(rect, label, value, action); };
        }
        public static Command Vector3IntField(Rect rect, float labelWidth, string label, Vector3Int value, Action<Vector3Int> action)
        {
            return () => { EniGUI.Vector3IntField(rect, labelWidth, label, value, action); };
        }
        public static Command Vector3IntField(Rect rect, GUIContent label, Vector3Int value, Action<Vector3Int> action)
        {
            return () => { EniGUI.Vector3IntField(rect, label, value, action); };
        }
        public static Command Vector3IntField(Rect rect, float labelWidth, GUIContent label, Vector3Int value, Action<Vector3Int> action)
        {
            return () => { EniGUI.Vector3IntField(rect, labelWidth, label, value, action); };
        }

        public static Command BoundsField(Rect rect, Bounds value, Action<Bounds> action)
        {
            return () => { EniGUI.BoundsField(rect, value, action); };
        }
        public static Command BoundsField(Rect rect, string label, Bounds value, Action<Bounds> action)
        {
            return () => { EniGUI.BoundsField(rect, label, value, action); };
        }
        public static Command BoundsField(Rect rect, float labelWidth, string label, Bounds value, Action<Bounds> action)
        {
            return () => { EniGUI.BoundsField(rect, labelWidth, label, value, action); };
        }
        public static Command BoundsField(Rect rect, GUIContent label, Bounds value, Action<Bounds> action)
        {
            return () => { EniGUI.BoundsField(rect, label, value, action); };
        }
        public static Command BoundsField(Rect rect, float labelWidth, GUIContent label, Bounds value, Action<Bounds> action)
        {
            return () => { EniGUI.BoundsField(rect, labelWidth, label, value, action); };
        }

        public static Command BoundsIntField(Rect rect, BoundsInt value, Action<BoundsInt> action)
        {
            return () => { EniGUI.BoundsIntField(rect, value, action); };
        }
        public static Command BoundsIntField(Rect rect, string label, BoundsInt value, Action<BoundsInt> action)
        {
            return () => { EniGUI.BoundsIntField(rect, label, value, action); };
        }
        public static Command BoundsIntField(Rect rect, float labelWidth, string label, BoundsInt value, Action<BoundsInt> action)
        {
            return () => { EniGUI.BoundsIntField(rect, labelWidth, label, value, action); };
        }
        public static Command BoundsIntField(Rect rect, GUIContent label, BoundsInt value, Action<BoundsInt> action)
        {
            return () => { EniGUI.BoundsIntField(rect, label, value, action); };
        }
        public static Command BoundsIntField(Rect rect, float labelWidth, GUIContent label, BoundsInt value, Action<BoundsInt> action)
        {
            return () => { EniGUI.BoundsIntField(rect, labelWidth, label, value, action); };
        }

        public static Command RectField(Rect rect, Rect value, Action<Rect> action)
        {
            return () => { EniGUI.RectField(rect, value, action); };
        }
        public static Command RectField(Rect rect, string label, Rect value, Action<Rect> action)
        {
            return () => { EniGUI.RectField(rect, label, value, action); };
        }
        public static Command RectField(Rect rect, float labelWidth, string label, Rect value, Action<Rect> action)
        {
            return () => { EniGUI.RectField(rect, labelWidth, label, value, action); };
        }
        public static Command RectField(Rect rect, GUIContent label, Rect value, Action<Rect> action)
        {
            return () => { EniGUI.RectField(rect, label, value, action); };
        }
        public static Command RectField(Rect rect, float labelWidth, GUIContent label, Rect value, Action<Rect> action)
        {
            return () => { EniGUI.RectField(rect, labelWidth, label, value, action); };
        }

        public static Command RectIntField(Rect rect, RectInt value, Action<RectInt> action)
        {
            return () => { EniGUI.RectIntField(rect, value, action); };
        }
        public static Command RectIntField(Rect rect, string label, RectInt value, Action<RectInt> action)
        {
            return () => { EniGUI.RectIntField(rect, label, value, action); };
        }
        public static Command RectIntField(Rect rect, float labelWidth, string label, RectInt value, Action<RectInt> action)
        {
            return () => { EniGUI.RectIntField(rect, labelWidth, label, value, action); };
        }
        public static Command RectIntField(Rect rect, GUIContent label, RectInt value, Action<RectInt> action)
        {
            return () => { EniGUI.RectIntField(rect, label, value, action); };
        }
        public static Command RectIntField(Rect rect, float labelWidth, GUIContent label, RectInt value, Action<RectInt> action)
        {
            return () => { EniGUI.RectIntField(rect, labelWidth, label, value, action); };
        }

        public static Command LayerField(Rect rect, int layer, Action<int> action, GUIStyle style = null)
        {
            return () => { EniGUI.LayerField(rect, layer, action, style); };
        }
        public static Command LayerField(Rect rect, string label, int layer, Action<int> action, GUIStyle style = null)
        {
            return () => { EniGUI.LayerField(rect, label, layer, action, style); };
        }
        public static Command LayerField(Rect rect, float labelWidth, string label, int layer, Action<int> action, GUIStyle style = null)
        {
            return () => { EniGUI.LayerField(rect, labelWidth, label, layer, action, style); };
        }
        public static Command LayerField(Rect rect, GUIContent label, int layer, Action<int> action, GUIStyle style = null)
        {
            return () => { EniGUI.LayerField(rect, label, layer, action, style); };
        }
        public static Command LayerField(Rect rect, float labelWidth, GUIContent label, int layer, Action<int> action, GUIStyle style = null)
        {
            return () => { EniGUI.LayerField(rect, labelWidth, label, layer, action, style); };
        }

        public static Command MaskField(Rect rect, int mask, string[] displayedOptions, Action<int> action, GUIStyle style = null)
        {
            return () => { EniGUI.MaskField(rect, mask, displayedOptions, action, style); };
        }
        public static Command MaskField(Rect rect, string label, int mask, string[] displayedOptions, Action<int> action, GUIStyle style = null)
        {
            return () => { EniGUI.MaskField(rect, label, mask, displayedOptions, action, style); };
        }
        public static Command MaskField(Rect rect, float labelWidth, string label, int mask, string[] displayedOptions, Action<int> action, GUIStyle style = null)
        {
            return () => { EniGUI.MaskField(rect, labelWidth, label, mask, displayedOptions, action, style); };
        }
        public static Command MaskField(Rect rect, GUIContent label, int mask, string[] displayedOptions, Action<int> action, GUIStyle style = null)
        {
            return () => { EniGUI.MaskField(rect, label, mask, displayedOptions, action, style); };
        }
        public static Command MaskField(Rect rect, float labelWidth, GUIContent label, int mask, string[] displayedOptions, Action<int> action, GUIStyle style = null)
        {
            return () => { EniGUI.MaskField(rect, labelWidth, label, mask, displayedOptions, action, style); };
        }

        public static Command EnumFlagsField<T>(Rect rect, T value, Action<T> action) where T : Enum
        {
            return () => { EniGUI.EnumFlagsField(rect, value, action); };
        }
        public static Command EnumFlagsField<T>(Rect rect, string label, T value, Action<T> action) where T : Enum
        {
            return () => { EniGUI.EnumFlagsField(rect, label, value, action); };
        }
        public static Command EnumFlagsField<T>(Rect rect, float labelWidth, string label, T value, Action<T> action) where T : Enum
        {
            return () => { EniGUI.EnumFlagsField(rect, labelWidth, label, value, action); };
        }
        public static Command EnumFlagsField<T>(Rect rect, GUIContent label, T value, Action<T> action) where T : Enum
        {
            return () => { EniGUI.EnumFlagsField(rect, label, value, action); };
        }
        public static Command EnumFlagsField<T>(Rect rect, float labelWidth, GUIContent label, T value, Action<T> action) where T : Enum
        {
            return () => { EniGUI.EnumFlagsField(rect, labelWidth, label, value, action); };
        }

        public static Command EnumPopup<T>(Rect rect, T selected, Action<T> action) where T : Enum
        {
            return () => { EniGUI.EnumPopup(rect, selected, action); };
        }
        public static Command EnumPopup<T>(Rect rect, string label, T selected, Action<T> action) where T : Enum
        {
            return () => { EniGUI.EnumPopup(rect, label, selected, action); };
        }
        public static Command EnumPopup<T>(Rect rect, float labelWidth, string label, T selected, Action<T> action) where T : Enum
        {
            return () => { EniGUI.EnumPopup(rect, labelWidth, label, selected, action); };
        }
        public static Command EnumPopup<T>(Rect rect, GUIContent label, T selected, Action<T> action) where T : Enum
        {
            return () => { EniGUI.EnumPopup(rect, label, selected, action); };
        }
        public static Command EnumPopup<T>(Rect rect, float labelWidth, GUIContent label, T selected, Action<T> action) where T : Enum
        {
            return () => { EniGUI.EnumPopup(rect, labelWidth, label, selected, action); };
        }

        public static Command Popup(Rect rect, int selectedIndex, string[] displayedOptions, Action<int> action)
        {
            return () => { EniGUI.Popup(rect, selectedIndex, displayedOptions, action); };
        }
        public static Command Popup(Rect rect, string label, int selectedIndex, string[] displayedOptions, Action<int> action)
        {
            return () => { EniGUI.Popup(rect, label, selectedIndex, displayedOptions, action); };
        }
        public static Command Popup(Rect rect, float labelWidth, string label, int selectedIndex, string[] displayedOptions, Action<int> action)
        {
            return () => { EniGUI.Popup(rect, labelWidth, label, selectedIndex, displayedOptions, action); };
        }
        public static Command Popup(Rect rect, GUIContent label, int selectedIndex, string[] displayedOptions, Action<int> action)
        {
            return () => { EniGUI.Popup(rect, label, selectedIndex, displayedOptions, action); };
        }
        public static Command Popup(Rect rect, float labelWidth, GUIContent label, int selectedIndex, string[] displayedOptions, Action<int> action)
        {
            return () => { EniGUI.Popup(rect, labelWidth, label, selectedIndex, displayedOptions, action); };
        }
        public static Command Popup(Rect rect, int selectedIndex, GUIContent[] displayedOptions, Action<int> action)
        {
            return () => { EniGUI.Popup(rect, selectedIndex, displayedOptions, action); };
        }
        public static Command Popup(Rect rect, string label, int selectedIndex, GUIContent[] displayedOptions, Action<int> action)
        {
            return () => { EniGUI.Popup(rect, label, selectedIndex, displayedOptions, action); };
        }
        public static Command Popup(Rect rect, float labelWidth, string label, int selectedIndex, GUIContent[] displayedOptions, Action<int> action)
        {
            return () => { EniGUI.Popup(rect, labelWidth, label, selectedIndex, displayedOptions, action); };
        }
        public static Command Popup(Rect rect, GUIContent label, int selectedIndex, GUIContent[] displayedOptions, Action<int> action)
        {
            return () => { EniGUI.Popup(rect, label, selectedIndex, displayedOptions, action); };
        }
        public static Command Popup(Rect rect, float labelWidth, GUIContent label, int selectedIndex, GUIContent[] displayedOptions, Action<int> action)
        {
            return () => { EniGUI.Popup(rect, labelWidth, label, selectedIndex, displayedOptions, action); };
        }

        public static Command Slider(Rect rect, float value, float leftValue, float rightValue, Action<float> action)
        {
            return () => { EniGUI.Slider(rect, value, leftValue, rightValue, action); };
        }
        public static Command Slider(Rect rect, string label, float value, float leftValue, float rightValue, Action<float> action)
        {
            return () => { EniGUI.Slider(rect, label, value, leftValue, rightValue, action); };
        }
        public static Command Slider(Rect rect, float labelWidth, string label, float value, float leftValue, float rightValue, Action<float> action)
        {
            return () => { EniGUI.Slider(rect, labelWidth, label, value, leftValue, rightValue, action); };
        }
        public static Command Slider(Rect rect, GUIContent label, float value, float leftValue, float rightValue, Action<float> action)
        {
            return () => { EniGUI.Slider(rect, label, value, leftValue, rightValue, action); };
        }
        public static Command Slider(Rect rect, float labelWidth, GUIContent label, float value, float leftValue, float rightValue, Action<float> action)
        {
            return () => { EniGUI.Slider(rect, labelWidth, label, value, leftValue, rightValue, action); };
        }

        public static Command IntSlider(Rect rect, int value, int leftValue, int rightValue, Action<int> action)
        {
            return () => { EniGUI.IntSlider(rect, value, leftValue, rightValue, action); };
        }
        public static Command IntSlider(Rect rect, string label, int value, int leftValue, int rightValue, Action<int> action)
        {
            return () => { EniGUI.IntSlider(rect, label, value, leftValue, rightValue, action); };
        }
        public static Command IntSlider(Rect rect, float labelWidth, string label, int value, int leftValue, int rightValue, Action<int> action)
        {
            return () => { EniGUI.IntSlider(rect, labelWidth, label, value, leftValue, rightValue, action); };
        }
        public static Command IntSlider(Rect rect, GUIContent label, int value, int leftValue, int rightValue, Action<int> action)
        {
            return () => { EniGUI.IntSlider(rect, label, value, leftValue, rightValue, action); };
        }
        public static Command IntSlider(Rect rect, float labelWidth, GUIContent label, int value, int leftValue, int rightValue, Action<int> action)
        {
            return () => { EniGUI.IntSlider(rect, labelWidth, label, value, leftValue, rightValue, action); };
        }

        public static Command Foldout(Rect rect, bool foldout, bool toggleOnLabelClick, Action<bool> action, GUIStyle background = null)
        {
            return () => { EniGUI.Foldout(rect, foldout, toggleOnLabelClick, action, background); };
        }
        public static Command Foldout(Rect rect, string label, bool foldout, bool toggleOnLabelClick, Action<bool> action, GUIStyle background = null)
        {
            return () => { EniGUI.Foldout(rect, label, foldout, toggleOnLabelClick, action, background); };
        }
        public static Command Foldout(Rect rect, float labelWidth, string label, bool foldout, bool toggleOnLabelClick, Action<bool> action, GUIStyle background = null)
        {
            return () => { EniGUI.Foldout(rect, labelWidth, label, foldout, toggleOnLabelClick, action, background); };
        }
        public static Command Foldout(Rect rect, GUIContent label, bool foldout, bool toggleOnLabelClick, Action<bool> action, GUIStyle background = null)
        {
            return () => { EniGUI.Foldout(rect, label, foldout, toggleOnLabelClick, action, background); };
        }
        public static Command Foldout(Rect rect, float labelWidth, GUIContent label, bool foldout, bool toggleOnLabelClick, Action<bool> action, GUIStyle background = null)
        {
            return () => { EniGUI.Foldout(rect, labelWidth, label, foldout, toggleOnLabelClick, action, background); };
        }

        public static Command TextField(Rect rect, string value, Action<string> action)
        {
            return () => { EniGUI.TextField(rect, value, action); };
        }
        public static Command TextField(Rect rect, string label, string value, Action<string> action)
        {
            return () => { EniGUI.TextField(rect, label, value, action); };
        }
        public static Command TextField(Rect rect, float labelWidth, string label, string value, Action<string> action)
        {
            return () => { EniGUI.TextField(rect, labelWidth, label, value, action); };
        }
        public static Command TextField(Rect rect, GUIContent label, string value, Action<string> action)
        {
            return () => { EniGUI.TextField(rect, label, value, action); };
        }
        public static Command TextField(Rect rect, float labelWidth, GUIContent label, string value, Action<string> action)
        {
            return () => { EniGUI.TextField(rect, labelWidth, label, value, action); };
        }

        public static Command IntField(Rect rect, int value, Action<int> action)
        {
            return () => { EniGUI.IntField(rect, value, action); };
        }
        public static Command IntField(Rect rect, string label, int value, Action<int> action)
        {
            return () => { EniGUI.IntField(rect, label, value, action); };
        }
        public static Command IntField(Rect rect, float labelWidth, string label, int value, Action<int> action)
        {
            return () => { EniGUI.IntField(rect, labelWidth, label, value, action); };
        }
        public static Command IntField(Rect rect, GUIContent label, int value, Action<int> action)
        {
            return () => { EniGUI.IntField(rect, label, value, action); };
        }
        public static Command IntField(Rect rect, float labelWidth, GUIContent label, int value, Action<int> action)
        {
            return () => { EniGUI.IntField(rect, labelWidth, label, value, action); };
        }

        public static Command FloatField(Rect rect, float value, Action<float> action)
        {
            return () => { EniGUI.FloatField(rect, value, action); };
        }
        public static Command FloatField(Rect rect, string label, float value, Action<float> action)
        {
            return () => { EniGUI.FloatField(rect, label, value, action); };
        }
        public static Command FloatField(Rect rect, float labelWidth, string label, float value, Action<float> action)
        {
            return () => { EniGUI.FloatField(rect, labelWidth, label, value, action); };
        }
        public static Command FloatField(Rect rect, GUIContent label, float value, Action<float> action)
        {
            return () => { EniGUI.FloatField(rect, label, value, action); };
        }
        public static Command FloatField(Rect rect, float labelWidth, GUIContent label, float value, Action<float> action)
        {
            return () => { EniGUI.FloatField(rect, labelWidth, label, value, action); };
        }

        public static Command DoubleField(Rect rect, double value, Action<double> action)
        {
            return () => { EniGUI.DoubleField(rect, value, action); };
        }
        public static Command DoubleField(Rect rect, string label, double value, Action<double> action)
        {
            return () => { EniGUI.DoubleField(rect, label, value, action); };
        }
        public static Command DoubleField(Rect rect, float labelWidth, string label, double value, Action<double> action)
        {
            return () => { EniGUI.DoubleField(rect, labelWidth, label, value, action); };
        }
        public static Command DoubleField(Rect rect, GUIContent label, double value, Action<double> action)
        {
            return () => { EniGUI.DoubleField(rect, label, value, action); };
        }
        public static Command DoubleField(Rect rect, float labelWidth, GUIContent label, double value, Action<double> action)
        {
            return () => { EniGUI.DoubleField(rect, labelWidth, label, value, action); };
        }

        public static Command LongField(Rect rect, long value, Action<long> action)
        {
            return () => { EniGUI.LongField(rect, value, action); };
        }
        public static Command LongField(Rect rect, string label, long value, Action<long> action)
        {
            return () => { EniGUI.LongField(rect, label, value, action); };
        }
        public static Command LongField(Rect rect, float labelWidth, string label, long value, Action<long> action)
        {
            return () => { EniGUI.LongField(rect, labelWidth, label, value, action); };
        }
        public static Command LongField(Rect rect, GUIContent label, long value, Action<long> action)
        {
            return () => { EniGUI.LongField(rect, label, value, action); };
        }
        public static Command LongField(Rect rect, float labelWidth, GUIContent label, long value, Action<long> action)
        {
            return () => { EniGUI.LongField(rect, labelWidth, label, value, action); };
        }

        public static Command ToggleRight(Rect rect, bool value, Action<bool> action, GUIStyle style = null)
        {
            return () => { EniGUI.ToggleRight(rect, value, action, style); };
        }
        public static Command ToggleRight(Rect rect, string label, bool value, Action<bool> action, GUIStyle style = null)
        {
            return () => { EniGUI.ToggleRight(rect, label, value, action, style); };
        }
        public static Command ToggleRight(Rect rect, float labelWidth, string label, bool value, Action<bool> action, GUIStyle style = null)
        {
            return () => { EniGUI.ToggleRight(rect, labelWidth, label, value, action, style); };
        }
        public static Command ToggleRight(Rect rect, GUIContent label, bool value, Action<bool> action, GUIStyle style = null)
        {
            return () => { EniGUI.ToggleRight(rect, label, value, action, style); };
        }
        public static Command ToggleRight(Rect rect, float labelWidth, GUIContent label, bool value, Action<bool> action, GUIStyle style = null)
        {
            return () => { EniGUI.ToggleRight(rect, labelWidth, label, value, action, style); };
        }

        public static Command ToggleLeft(Rect rect, bool value, Action<bool> action, GUIStyle style = null)
        {
            return () => { EniGUI.ToggleLeft(rect, value, action, style); };
        }
        public static Command ToggleLeft(Rect rect, string label, bool value, Action<bool> action, GUIStyle style = null)
        {
            return () => { EniGUI.ToggleLeft(rect, label, value, action, style); };
        }
        public static Command ToggleLeft(Rect rect, float labelWidth, string label, bool value, Action<bool> action, GUIStyle style = null)
        {
            return () => { EniGUI.ToggleLeft(rect, labelWidth, label, value, action, style); };
        }
        public static Command ToggleLeft(Rect rect, GUIContent label, bool value, Action<bool> action, GUIStyle style = null)
        {
            return () => { EniGUI.ToggleLeft(rect, label, value, action, style); };
        }
        public static Command ToggleLeft(Rect rect, float labelWidth, GUIContent label, bool value, Action<bool> action, GUIStyle style = null)
        {
            return () => { EniGUI.ToggleLeft(rect, labelWidth, label, value, action, style); };
        }

        public static Command Label(Rect rect, string text, GUIStyle style = null)
        {
            return () => { EniGUI.Label(rect, text, style); };
        }
        public static Command Label(Rect rect, GUIContent text, GUIStyle style = null)
        {
            return () => { EniGUI.Label(rect, text, style); };
        }

        public static Command Image(Rect rect, GUIStyle style = null)
        {
            return () => { EniGUI.Image(rect, style); };
        }
        public static Command Image(Rect rect, string text, GUIStyle style = null)
        {
            return () => { EniGUI.Image(rect, text, style); };
        }
        public static Command Image(Rect rect, GUIContent text, GUIStyle style = null)
        {
            return () => { EniGUI.Image(rect, text, style); };
        }
        public static Command Image(Rect rect, Texture texture)
        {
            return () => { EniGUI.Image(rect, texture); };
        }

        public static Command Button(Rect rect, string text, Action action, GUIStyle style = null)
        {
            return () => { EniGUI.Button(rect, text, action, style); };
        }

        public static Command Rect(Rect rect, Color color)
        {
            return () => { EniGUI.Rect(rect, color); };
        }
    }

    public static class EniGUILayout
    {
        private static GUIGroupPool sm_GroupPool = new GUIGroupPool();
        private static GUIScrollViewPool sm_ScrollViewPool = new GUIScrollViewPool();
        private static GUIElementPool sm_ElementPool = new GUIElementPool();
        private static GUIOptionPool sm_OptionPool = new GUIOptionPool();

        private static CommandQueue sm_CommandQueue = new CommandQueue();

        private static readonly float sm_FieldHeight = 18;
        private static readonly float sm_DabbleFieldHeight = 38;
        public static GUIGroup GUIActiveGroup { get; private set; }
        public static GUIGroup GUILastGroup { get; private set; }

        public static Rect GUILastRect { get; private set; }

        public static void BeginHorizontal(params EniGUILayoutOption[] options)
        {
            BeginHorizontal(false, null, Color.clear, options);
        }
        public static void BeginHorizontal(Color color, params EniGUILayoutOption[] options)
        {
            BeginHorizontal(false, null, color, options);
        }
        public static void BeginHorizontal(GUIStyle style, params EniGUILayoutOption[] options)
        {
            BeginHorizontal(false, style, Color.clear, options);
        }
        public static void BeginHorizontal(bool isClipped, params EniGUILayoutOption[] options)
        {
            BeginHorizontal(isClipped, null, Color.clear, options);
        }
        public static void BeginHorizontal(bool isClipped, Color color, params EniGUILayoutOption[] options)
        {
            BeginHorizontal(isClipped, null, color, options);
        }
        public static void BeginHorizontal(bool isClipped, GUIStyle style, params EniGUILayoutOption[] options)
        {
            BeginHorizontal(isClipped, style, Color.clear, options);
        }
        public static void BeginHorizontal(bool isClipped, GUIStyle style, Color color, params EniGUILayoutOption[] options)
        {
            GUIGroup group = GetGroup(GroupSortType.Horizontal, isClipped, style, color, options);
            BeginGroup(group);
        }

        public static void BeginHorizontal(Rect rect, params EniGUILayoutOption[] options)
        {
            BeginHorizontal(rect, false, null, Color.clear, options);
        }
        public static void BeginHorizontal(Rect rect, Color color, params EniGUILayoutOption[] options)
        {
            BeginHorizontal(rect, false, null, color, options);
        }
        public static void BeginHorizontal(Rect rect, GUIStyle style, params EniGUILayoutOption[] options)
        {
            BeginHorizontal(rect, false, style, Color.clear, options);
        }
        public static void BeginHorizontal(Rect rect, bool isClipped, params EniGUILayoutOption[] options)
        {
            BeginHorizontal(rect, isClipped, null, Color.clear, options);
        }
        public static void BeginHorizontal(Rect rect, bool isClipped, Color color, params EniGUILayoutOption[] options)
        {
            BeginHorizontal(rect, isClipped, null, color, options);
        }
        public static void BeginHorizontal(Rect rect, bool isClipped, GUIStyle style, params EniGUILayoutOption[] options)
        {
            BeginHorizontal(rect, isClipped, style, Color.clear, options);
        }
        public static void BeginHorizontal(Rect rect, bool isClipped, GUIStyle style, Color color, params EniGUILayoutOption[] options)
        {
            GUIGroup group = GetGroup(GroupSortType.Horizontal, rect, isClipped, style, color, options);
            BeginGroup(group);
        }

        public static void EndHorizontal()
        {
            EndGroup();
        }

        public static void BeginVertical(params EniGUILayoutOption[] options)
        {
            BeginVertical(false, null, Color.clear, options);
        }
        public static void BeginVertical(Color color, params EniGUILayoutOption[] options)
        {
            BeginVertical(false, null, color, options);
        }
        public static void BeginVertical(GUIStyle style, params EniGUILayoutOption[] options)
        {
            BeginVertical(false, style, Color.clear, options);
        }
        public static void BeginVertical(bool isClipped, Color color, params EniGUILayoutOption[] options)
        {
            BeginVertical(isClipped, null, color, options);
        }
        public static void BeginVertical(bool isClipped, GUIStyle style, params EniGUILayoutOption[] options)
        {
            BeginVertical(isClipped, style, Color.clear, options);
        }
        public static void BeginVertical(bool isClipped, GUIStyle style, Color color, params EniGUILayoutOption[] options)
        {
            GUIGroup group = GetGroup(GroupSortType.Vertical, isClipped, style, color, options);
            BeginGroup(group);
        }

        public static void BeginVertical(Rect rect, params EniGUILayoutOption[] options)
        {
            BeginVertical(rect, false, null, Color.clear, options);
        }
        public static void BeginVertical(Rect rect, Color color, params EniGUILayoutOption[] options)
        {
            BeginVertical(rect, false, null, color, options);
        }
        public static void BeginVertical(Rect rect, GUIStyle style, params EniGUILayoutOption[] options)
        {
            BeginVertical(rect, false, style, Color.clear, options);
        }
        public static void BeginVertical(Rect rect, bool isClipped, params EniGUILayoutOption[] options)
        {
            BeginVertical(rect, isClipped, null, Color.clear, options);
        }
        public static void BeginVertical(Rect rect, bool isClipped, Color color, params EniGUILayoutOption[] options)
        {
            BeginVertical(rect, isClipped, null, color, options);
        }
        public static void BeginVertical(Rect rect, bool isClipped, GUIStyle style, params EniGUILayoutOption[] options)
        {
            BeginVertical(rect, isClipped, style, Color.clear, options);
        }
        public static void BeginVertical(Rect rect, bool isClipped, GUIStyle style, Color color, params EniGUILayoutOption[] options)
        {
            GUIGroup group = GetGroup(GroupSortType.Vertical, rect, isClipped, style, color, options);
            BeginGroup(group);
        }

        public static void EndVertical()
        {
            EndGroup();
        }

        public static void BeginVerticalScrollView(Vector2 size, Vector2 scrollPosition, 
            Action<Vector2> action, params EniGUILayoutOption[] options)
        {
            Vector2 viewSize = Vector2.right * (size.x - 22);
            BeginVerticalScrollView(scrollPosition, size, viewSize, false, false, action,
                GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, options);
        }
        public static void BeginVerticalScrollView(Vector2 size, Vector2 scrollPosition,
           Vector2 viewSize, Action<Vector2> action, params EniGUILayoutOption[] options)
        {
            BeginVerticalScrollView(scrollPosition, size, viewSize, false, false, action,
                GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, options);
        }
        public static void BeginVerticalScrollView(Vector2 size, Vector2 scrollPosition,
            bool alwaysShowHorizontal, bool alwaysShowVertical, Action<Vector2> action, params EniGUILayoutOption[] options)
        {
            BeginVerticalScrollView(scrollPosition, size, size, alwaysShowHorizontal, alwaysShowVertical, action,
                GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, options);
        }
        public static void BeginVerticalScrollView(Vector2 size, Vector2 scrollPosition,
            Vector2 viewSize, bool alwaysShowHorizontal, bool alwaysShowVertical, Action<Vector2> action,
            params EniGUILayoutOption[] options)
        {
            BeginVerticalScrollView(scrollPosition, size, viewSize, alwaysShowHorizontal, alwaysShowVertical, action,
                GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, options);
        }
        public static void BeginVerticalScrollView(Vector2 size, Vector2 scrollPosition,
            Action<Vector2> action, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar,
            params EniGUILayoutOption[] options)
        {
            BeginVerticalScrollView(scrollPosition, size, size, false, false, action,
                horizontalScrollbar, verticalScrollbar, options);
        }
        public static void BeginVerticalScrollView(Vector2 size, Vector2 scrollPosition,
            Vector2 viewSize, bool alwaysShowHorizontal, bool alwaysShowVertical, Action<Vector2> action,
            GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, params EniGUILayoutOption[] options)
        {
            BeginScrollView(GroupSortType.Vertical, size, scrollPosition, viewSize,
                alwaysShowHorizontal, alwaysShowVertical, action, horizontalScrollbar, verticalScrollbar, options);
        }

        public static void BeginVerticalScrollView(Rect rect, Vector2 scrollPosition,
            Action<Vector2> action, params EniGUILayoutOption[] options)
        {
            BeginVerticalScrollView(rect, scrollPosition, rect, false, false, action,
                GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, options);
        }
        public static void BeginVerticalScrollView(Rect rect, Vector2 scrollPosition,
            Rect viewRect, bool alwaysShowHorizontal, bool alwaysShowVertical, Action<Vector2> action,
            params EniGUILayoutOption[] options)
        {
            BeginVerticalScrollView(rect, scrollPosition, viewRect, alwaysShowHorizontal, alwaysShowVertical, action,
                GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, options);
        }
        public static void BeginVerticalScrollView(Rect rect, Vector2 scrollPosition,
            Rect viewRect, Action<Vector2> action, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar,
            params EniGUILayoutOption[] options)
        {
            BeginVerticalScrollView(rect, scrollPosition, viewRect, false, false, action,
                horizontalScrollbar, verticalScrollbar, options);
        }
        public static void BeginVerticalScrollView(Rect rect, Vector2 scrollPosition,
            Rect viewRect, bool alwaysShowHorizontal, bool alwaysShowVertical, Action<Vector2> action,
            GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, params EniGUILayoutOption[] options)
        {
            BeginScrollView(GroupSortType.Vertical, rect, scrollPosition, viewRect,
                alwaysShowHorizontal, alwaysShowVertical, action, horizontalScrollbar, verticalScrollbar, options);
        }

        public static void EndVerticalScrollView()
        {
            EndScrollView();
        }

        public static void BeginScrollView(GroupSortType sortType, Vector2 size, Vector2 scrollPosition,
            bool alwaysShowHorizontal, bool alwaysShowVertical, Action<Vector2> action, params EniGUILayoutOption[] options)
        {
            BeginScrollView(sortType, scrollPosition, size, size, alwaysShowHorizontal, alwaysShowVertical, action,
                GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, options);
        }
        public static void BeginScrollView(GroupSortType sortType, Vector2 size, Vector2 scrollPosition,
            Vector2 viewSize, bool alwaysShowHorizontal, bool alwaysShowVertical, Action<Vector2> action,
            params EniGUILayoutOption[] options)
        {
            BeginScrollView(sortType, scrollPosition, size, viewSize, alwaysShowHorizontal, alwaysShowVertical, action,
                GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, options);
        }
        public static void BeginScrollView(GroupSortType sortType, Vector2 size, Vector2 scrollPosition,
            Action<Vector2> action, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar,
            params EniGUILayoutOption[] options)
        {
            BeginScrollView(sortType, scrollPosition, size, size, false, false, action,
                horizontalScrollbar, verticalScrollbar, options);
        }
        public static void BeginScrollView(GroupSortType sortType, Vector2 size, Vector2 scrollPosition,
            Vector2 viewSize, bool alwaysShowHorizontal, bool alwaysShowVertical, Action<Vector2> action,
            GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, params EniGUILayoutOption[] options)
        {
            GUIScrollView scrollView = GetScrollView(sortType, scrollPosition, size, viewSize,
                alwaysShowHorizontal, alwaysShowVertical, action, horizontalScrollbar, verticalScrollbar, options);
            BeginScrollView(scrollView);
        }

        public static void BeginScrollView(GroupSortType sortType, Rect rect, Vector2 scrollPosition,
            Action<Vector2> action, params EniGUILayoutOption[] options)
        {
            BeginScrollView(sortType, rect, scrollPosition, rect, false, false, action,
                GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, options);
        }
        public static void BeginScrollView(GroupSortType sortType, Rect rect, Vector2 scrollPosition,
            Rect viewRect, bool alwaysShowHorizontal, bool alwaysShowVertical, Action<Vector2> action, 
            params EniGUILayoutOption[] options)
        {
            BeginScrollView(sortType, rect, scrollPosition, viewRect, alwaysShowHorizontal, alwaysShowVertical, action,
                GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, options);
        }
        public static void BeginScrollView(GroupSortType sortType, Rect rect, Vector2 scrollPosition,
            Rect viewRect, Action<Vector2> action, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, 
            params EniGUILayoutOption[] options)
        {
            BeginScrollView(sortType, rect, scrollPosition, viewRect, false, false, action, 
                horizontalScrollbar, verticalScrollbar, options);
        }
        public static void BeginScrollView(GroupSortType sortType, Rect rect, Vector2 scrollPosition, 
            Rect viewRect, bool alwaysShowHorizontal, bool alwaysShowVertical, Action<Vector2> action, 
            GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, params EniGUILayoutOption[] options)
        {
            GUIScrollView scrollView = GetScrollView(sortType, rect, scrollPosition, viewRect,
                alwaysShowHorizontal, alwaysShowVertical, action, horizontalScrollbar, verticalScrollbar, options);
            BeginScrollView(scrollView);
        }

        public static void EndScrollView()
        {
            if (GUIActiveGroup.Parent is GUIScrollView scrollView == false)
                throw new Exception();

            Space(8);

            EnqueueCommand(EniGUIClip.EndClip());
            EnqueueCommand(GUIDrawCommand.BeginScrollView(scrollView));
            EnqueueCommand(GUIDrawCommand.EndScrollView());

            EndGroup();
            EndGroup();
        }

        public static void PropertyField(float width, SerializedProperty property, string label = "")
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.PropertyField(rect, property, label));
        }
        public static void PropertyField(float width, float labelWidth, SerializedProperty property, string label = "")
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.PropertyField(rect, labelWidth, property, label));
        }
        public static void PropertyField(float width, SerializedProperty property, GUIContent label = null)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.PropertyField(rect, property, label));
        }
        public static void PropertyField(float width, float labelWidth, SerializedProperty property, GUIContent label = null)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.PropertyField(rect, labelWidth, property, label));
        }

        public static void ObjectField<T>(float width, T value, Action<T> action, bool allowSceneObjects) where T : UnityEngine.Object
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.ObjectField(rect, value, action, allowSceneObjects));
        }
        public static void ObjectField<T>(float width, string label, T value, Action<T> action, bool allowSceneObjects) where T : UnityEngine.Object
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.ObjectField(rect, label, value, action, allowSceneObjects));
        }
        public static void ObjectField<T>(float width, float labelWidth, string label, T value, Action<T> action, bool allowSceneObjects) where T : UnityEngine.Object
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.ObjectField(rect, labelWidth, label, value, action, allowSceneObjects));
        }
        public static void ObjectField<T>(float width, GUIContent label, T value, Action<T> action, bool allowSceneObjects) where T : UnityEngine.Object
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.ObjectField(rect, label, value, action, allowSceneObjects));
        }
        public static void ObjectField<T>(float width, float labelWidth, GUIContent label, T value, Action<T> action, bool allowSceneObjects) where T : UnityEngine.Object
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.ObjectField(rect, labelWidth, label, value, action, allowSceneObjects));
        }

        public static void ColorField(float width, Color value, Action<Color> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.ColorField(rect, value, action));
        }
        public static void ColorField(float width, string label, Color value, Action<Color> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.ColorField(rect, label, value, action));
        }
        public static void ColorField(float width, float labelWidth, string label, Color value, Action<Color> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.ColorField(rect, labelWidth, label, value, action));
        }
        public static void ColorField(float width, GUIContent label, Color value, Action<Color> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.ColorField(rect, label, value, action));
        }
        public static void ColorField(float width, float labelWidth, GUIContent label, Color value, Action<Color> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.ColorField(rect, labelWidth, label, value, action));
        }

        public static void GradientField(float width, Gradient value, Action<Gradient> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.GradientField(rect, value, action));
        }
        public static void GradientField(float width, string label, Gradient value, Action<Gradient> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.GradientField(rect, label, value, action));
        }
        public static void GradientField(float width, float labelWidth, string label, Gradient value, Action<Gradient> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.GradientField(rect, labelWidth, label, value, action));
        }
        public static void GradientField(float width, GUIContent label, Gradient value, Action<Gradient> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.GradientField(rect, label, value, action));
        }
        public static void GradientField(float width, float labelWidth, GUIContent label, Gradient value, Action<Gradient> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.GradientField(rect, labelWidth, label, value, action));
        }

        public static void CurveField(float width, AnimationCurve value, Action<AnimationCurve> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.CurveField(rect, value, action));
        }
        public static void CurveField(float width, string label, AnimationCurve value, Action<AnimationCurve> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.CurveField(rect, label, value, action));
        }
        public static void CurveField(float width, float labelWidth, string label, AnimationCurve value, Action<AnimationCurve> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.CurveField(rect, labelWidth, label, value, action));
        }
        public static void CurveField(float width, GUIContent label, AnimationCurve value, Action<AnimationCurve> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.CurveField(rect, label, value, action));
        }
        public static void CurveField(float width, float labelWidth, GUIContent label, AnimationCurve value, Action<AnimationCurve> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.CurveField(rect, labelWidth, label, value, action));
        }

        public static void Vector2Field(float width, Vector2 value, Action<Vector2> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.Vector2Field(rect, value, action));
        }
        public static void Vector2Field(float width, string label, Vector2 value, Action<Vector2> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.Vector2Field(rect, label, value, action));
        }
        public static void Vector2Field(float width, float labelWidth, string label, Vector2 value, Action<Vector2> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.Vector2Field(rect, labelWidth, label, value, action));
        }
        public static void Vector2Field(float width, GUIContent label, Vector2 value, Action<Vector2> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.Vector2Field(rect, label, value, action));
        }
        public static void Vector2Field(float width, float labelWidth, GUIContent label, Vector2 value, Action<Vector2> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.Vector2Field(rect, labelWidth, label, value, action));
        }

        public static void Vector3Field(float width, Vector3 value, Action<Vector3> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.Vector3Field(rect, value, action));
        }
        public static void Vector3Field(float width, string label, Vector3 value, Action<Vector3> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.Vector3Field(rect, label, value, action));
        }
        public static void Vector3Field(float width, float labelWidth, string label, Vector3 value, Action<Vector3> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.Vector3Field(rect, labelWidth, label, value, action));
        }
        public static void Vector3Field(float width, GUIContent label, Vector3 value, Action<Vector3> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.Vector3Field(rect, label, value, action));
        }
        public static void Vector3Field(float width, float labelWidth, GUIContent label, Vector3 value, Action<Vector3> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.Vector3Field(rect, labelWidth, label, value, action));
        }

        public static void Vector2IntField(float width, Vector2Int value, Action<Vector2Int> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.Vector2IntField(rect, value, action));
        }
        public static void Vector2IntField(float width, string label, Vector2Int value, Action<Vector2Int> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.Vector2IntField(rect, label, value, action));
        }
        public static void Vector2IntField(float width, float labelWidth, string label, Vector2Int value, Action<Vector2Int> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.Vector2IntField(rect, labelWidth, label, value, action));
        }
        public static void Vector2IntField(float width, GUIContent label, Vector2Int value, Action<Vector2Int> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.Vector2IntField(rect, label, value, action));
        }
        public static void Vector2IntField(float width, float labelWidth, GUIContent label, Vector2Int value, Action<Vector2Int> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.Vector2IntField(rect, labelWidth, label, value, action));
        }

        public static void Vector3IntField(float width, Vector3Int value, Action<Vector3Int> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.Vector3IntField(rect, value, action));
        }
        public static void Vector3IntField(float width, string label, Vector3Int value, Action<Vector3Int> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.Vector3IntField(rect, label, value, action));
        }
        public static void Vector3IntField(float width, float labelWidth, string label, Vector3Int value, Action<Vector3Int> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.Vector3IntField(rect, labelWidth, label, value, action));
        }
        public static void Vector3IntField(float width, GUIContent label, Vector3Int value, Action<Vector3Int> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.Vector3IntField(rect, label, value, action));
        }
        public static void Vector3IntField(float width, float labelWidth, GUIContent label, Vector3Int value, Action<Vector3Int> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.Vector3IntField(rect, labelWidth, label, value, action));
        }

        public static void BoundsField(float width, Bounds value, Action<Bounds> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.BoundsField(rect, value, action));
        }
        public static void BoundsField(float width, string label, Bounds value, Action<Bounds> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.BoundsField(rect, label, value, action));
        }
        public static void BoundsField(float width, float labelWidth, string label, Bounds value, Action<Bounds> action)
        {
            Rect rect = GetNext(width, sm_DabbleFieldHeight);
            DrawElement(rect, GUIDrawCommand.BoundsField(rect, labelWidth, label, value, action));
        }
        public static void BoundsField(float width, GUIContent label, Bounds value, Action<Bounds> action)
        {
            Rect rect = GetNext(width, sm_DabbleFieldHeight);
            DrawElement(rect, GUIDrawCommand.BoundsField(rect, label, value, action));
        }
        public static void BoundsField(float width, float labelWidth, GUIContent label, Bounds value, Action<Bounds> action)
        {
            Rect rect = GetNext(width, sm_DabbleFieldHeight);
            DrawElement(rect, GUIDrawCommand.BoundsField(rect, labelWidth, label, value, action));
        }

        public static void BoundsIntField(float width, BoundsInt value, Action<BoundsInt> action)
        {
            Rect rect = GetNext(width, sm_DabbleFieldHeight);
            DrawElement(rect, GUIDrawCommand.BoundsIntField(rect, value, action));
        }
        public static void BoundsIntField(float width, string label, BoundsInt value, Action<BoundsInt> action)
        {
            Rect rect = GetNext(width, sm_DabbleFieldHeight);
            DrawElement(rect, GUIDrawCommand.BoundsIntField(rect, label, value, action));
        }
        public static void BoundsIntField(float width, float labelWidth, string label, BoundsInt value, Action<BoundsInt> action)
        {
            Rect rect = GetNext(width, sm_DabbleFieldHeight);
            DrawElement(rect, GUIDrawCommand.BoundsIntField(rect, labelWidth, label, value, action));
        }
        public static void BoundsIntField(float width, GUIContent label, BoundsInt value, Action<BoundsInt> action)
        {
            Rect rect = GetNext(width, sm_DabbleFieldHeight);
            DrawElement(rect, GUIDrawCommand.BoundsIntField(rect, label, value, action));
        }
        public static void BoundsIntField(float width, float labelWidth, GUIContent label, BoundsInt value, Action<BoundsInt> action)
        {
            Rect rect = GetNext(width, sm_DabbleFieldHeight);
            DrawElement(rect, GUIDrawCommand.BoundsIntField(rect, labelWidth, label, value, action));
        }

        public static void RectField(float width, Rect value, Action<Rect> action)
        {
            Rect rect = GetNext(width, sm_DabbleFieldHeight);
            DrawElement(rect, GUIDrawCommand.RectField(rect, value, action));
        }
        public static void RectField(float width, string label, Rect value, Action<Rect> action)
        {
            Rect rect = GetNext(width, sm_DabbleFieldHeight);
            DrawElement(rect, GUIDrawCommand.RectField(rect, label, value, action));
        }
        public static void RectField(float width, float labelWidth, string label, Rect value, Action<Rect> action)
        {
            Rect rect = GetNext(width, sm_DabbleFieldHeight);
            DrawElement(rect, GUIDrawCommand.RectField(rect, labelWidth, label, value, action));
        }
        public static void RectField(float width, GUIContent label, Rect value, Action<Rect> action)
        {
            Rect rect = GetNext(width, sm_DabbleFieldHeight);
            DrawElement(rect, GUIDrawCommand.RectField(rect, label, value, action));
        }
        public static void RectField(float width, float labelWidth, GUIContent label, Rect value, Action<Rect> action)
        {
            Rect rect = GetNext(width, sm_DabbleFieldHeight);
            DrawElement(rect, GUIDrawCommand.RectField(rect, labelWidth, label, value, action));
        }

        public static void RectIntField(float width, RectInt value, Action<RectInt> action)
        {
            Rect rect = GetNext(width, sm_DabbleFieldHeight);
            DrawElement(rect, GUIDrawCommand.RectIntField(rect, value, action));
        }
        public static void RectIntField(float width, string label, RectInt value, Action<RectInt> action)
        {
            Rect rect = GetNext(width, sm_DabbleFieldHeight);
            DrawElement(rect, GUIDrawCommand.RectIntField(rect, label, value, action));
        }
        public static void RectIntField(float width, float labelWidth, string label, RectInt value, Action<RectInt> action)
        {
            Rect rect = GetNext(width, sm_DabbleFieldHeight);
            DrawElement(rect, GUIDrawCommand.RectIntField(rect, labelWidth, label, value, action));
        }
        public static void RectIntField(float width, GUIContent label, RectInt value, Action<RectInt> action)
        {
            Rect rect = GetNext(width, sm_DabbleFieldHeight);
            DrawElement(rect, GUIDrawCommand.RectIntField(rect, label, value, action));
        }
        public static void RectIntField(float width, float labelWidth, GUIContent label, RectInt value, Action<RectInt> action)
        {
            Rect rect = GetNext(width, sm_DabbleFieldHeight);
            DrawElement(rect, GUIDrawCommand.RectIntField(rect, labelWidth, label, value, action));
        }

        public static void LayerField(float width, int layer, Action<int> action, GUIStyle style = null)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.LayerField(rect, layer, action, style));
        }
        public static void LayerField(float width, string label, int layer, Action<int> action, GUIStyle style = null)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.LayerField(rect, label, layer, action, style));
        }
        public static void LayerField(float width, float labelWidth, string label, int layer, Action<int> action, GUIStyle style = null)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.LayerField(rect, labelWidth, label, layer, action, style));
        }
        public static void LayerField(float width, GUIContent label, int layer, Action<int> action, GUIStyle style = null)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.LayerField(rect, label, layer, action, style));
        }
        public static void LayerField(float width, float labelWidth, GUIContent label, int layer, Action<int> action, GUIStyle style = null)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.LayerField(rect, labelWidth, label, layer, action, style));
        }

        public static void MaskField(float width, int mask, string[] displayedOptions, Action<int> action, GUIStyle style = null)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.MaskField(rect, mask, displayedOptions, action, style));
        }
        public static void MaskField(float width, string label, int mask, string[] displayedOptions, Action<int> action, GUIStyle style = null)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.MaskField(rect, label, mask, displayedOptions, action, style));
        }
        public static void MaskField(float width, float labelWidth, string label, int mask, string[] displayedOptions, Action<int> action, GUIStyle style = null)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.MaskField(rect, labelWidth, label, mask, displayedOptions, action, style));
        }
        public static void MaskField(float width, GUIContent label, int mask, string[] displayedOptions, Action<int> action, GUIStyle style = null)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.MaskField(rect, label, mask, displayedOptions, action, style));
        }
        public static void MaskField(float width, float labelWidth, GUIContent label, int mask, string[] displayedOptions, Action<int> action, GUIStyle style = null)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.MaskField(rect, labelWidth, label, mask, displayedOptions, action, style));
        }

        public static void EnumFlagsField<T>(float width, T value, Action<T> action) where T : Enum
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.EnumFlagsField(rect, value, action));
        }
        public static void EnumFlagsField<T>(float width, string label, T value, Action<T> action) where T : Enum
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.EnumFlagsField(rect, label, value, action));
        }
        public static void EnumFlagsField<T>(float width, float labelWidth, string label, T value, Action<T> action) where T : Enum
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.EnumFlagsField(rect, labelWidth, label, value, action));
        }
        public static void EnumFlagsField<T>(float width, GUIContent label, T value, Action<T> action) where T : Enum
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.EnumFlagsField(rect, label, value, action));
        }
        public static void EnumFlagsField<T>(float width, float labelWidth, GUIContent label, T value, Action<T> action) where T : Enum
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.EnumFlagsField(rect, labelWidth, label, value, action));
        }

        public static void EnumPopup<T>(float width, T selected, Action<T> action) where T : Enum
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.EnumPopup(rect, selected, action));
        }
        public static void EnumPopup<T>(float width, string label, T selected, Action<T> action) where T : Enum
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.EnumPopup(rect, label, selected, action));
        }
        public static void EnumPopup<T>(float width, float labelWidth, string label, T selected, Action<T> action) where T : Enum
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.EnumPopup(rect, labelWidth, label, selected, action));
        }
        public static void EnumPopup<T>(float width, GUIContent label, T selected, Action<T> action) where T : Enum
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.EnumPopup(rect, label, selected, action));
        }
        public static void EnumPopup<T>(float width, float labelWidth, GUIContent label, T selected, Action<T> action) where T : Enum
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.EnumPopup(rect, labelWidth, label, selected, action));
        }

        public static void Popup(float width, int selectedIndex, string[] displayedOptions, Action<int> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.Popup(rect, selectedIndex, displayedOptions, action));
        }
        public static void Popup(float width, string label, int selectedIndex, string[] displayedOptions, Action<int> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.Popup(rect, label, selectedIndex, displayedOptions, action));
        }
        public static void Popup(float width, float labelWidth, string label, int selectedIndex, string[] displayedOptions, Action<int> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.Popup(rect, labelWidth, label, selectedIndex, displayedOptions, action));
        }
        public static void Popup(float width, GUIContent label, int selectedIndex, string[] displayedOptions, Action<int> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.Popup(rect, label, selectedIndex, displayedOptions, action));
        }
        public static void Popup(float width, float labelWidth, GUIContent label, int selectedIndex, string[] displayedOptions, Action<int> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.Popup(rect, label, selectedIndex, displayedOptions, action));
        }
        public static void Popup(float width, int selectedIndex, GUIContent[] displayedOptions, Action<int> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.Popup(rect, selectedIndex, displayedOptions, action));
        }
        public static void Popup(float width, string label, int selectedIndex, GUIContent[] displayedOptions, Action<int> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.Popup(rect, label, selectedIndex, displayedOptions, action));
        }
        public static void Popup(float width, float labelWidth, string label, int selectedIndex, GUIContent[] displayedOptions, Action<int> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.Popup(rect, labelWidth, label, selectedIndex, displayedOptions, action));
        }
        public static void Popup(float width, GUIContent label, int selectedIndex, GUIContent[] displayedOptions, Action<int> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.Popup(rect, label, selectedIndex, displayedOptions, action));
        }
        public static void Popup(float width, float labelWidth, GUIContent label, int selectedIndex, GUIContent[] displayedOptions, Action<int> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.Popup(rect, labelWidth, label, selectedIndex, displayedOptions, action));
        }

        public static void Slider(float width, float value, float leftValue, float rightValue, Action<float> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.Slider(rect, value, leftValue, rightValue, action));
        }
        public static void Slider(float width, string label, float value, float leftValue, float rightValue, Action<float> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.Slider(rect, value, leftValue, rightValue, action));
        }
        public static void Slider(float width, float labelWidth, string label, float value, float leftValue, float rightValue, Action<float> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.Slider(rect, labelWidth, label, value, leftValue, rightValue, action));
        }
        public static void Slider(float width, GUIContent label, float value, float leftValue, float rightValue, Action<float> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.Slider(rect, label, value, leftValue, rightValue, action));
        }
        public static void Slider(float width, float labelWidth, GUIContent label, float value, float leftValue, float rightValue, Action<float> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.Slider(rect, labelWidth, label, value, leftValue, rightValue, action));
        }

        public static void IntSlider(float width, int value, int leftValue, int rightValue, Action<int> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.IntSlider(rect, value, leftValue, rightValue, action));
        }
        public static void IntSlider(float width, string label, int value, int leftValue, int rightValue, Action<int> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.IntSlider(rect, label, value, leftValue, rightValue, action));
        }
        public static void IntSlider(float width, float labelWidth, string label, int value, int leftValue, int rightValue, Action<int> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.IntSlider(rect, labelWidth, label, value, leftValue, rightValue, action));
        }
        public static void IntSlider(float width, GUIContent label, int value, int leftValue, int rightValue, Action<int> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.IntSlider(rect, label, value, leftValue, rightValue, action));
        }
        public static void IntSlider(float width, float labelWidth, GUIContent label, int value, int leftValue, int rightValue, Action<int> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.IntSlider(rect, labelWidth, label, value, leftValue, rightValue, action));
        }

        public static void Foldout(float width, bool foldout, bool toggleOnLabelClick, Action<bool> action, GUIStyle background = null)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.Foldout(rect, foldout, toggleOnLabelClick, action, background));
        }
        public static void Foldout(float width, string label, bool foldout, bool toggleOnLabelClick, Action<bool> action, GUIStyle background = null)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.Foldout(rect, label, foldout, toggleOnLabelClick, action, background));
        }
        public static void Foldout(float width, float labelWidth, string label, bool foldout, bool toggleOnLabelClick, Action<bool> action, GUIStyle background = null)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.Foldout(rect, labelWidth, label, foldout, toggleOnLabelClick, action, background));
        }
        public static void Foldout(float width, GUIContent label, bool foldout, bool toggleOnLabelClick, Action<bool> action, GUIStyle background = null)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.Foldout(rect, label, foldout, toggleOnLabelClick, action, background));
        }
        public static void Foldout(float width, float labelWidth, GUIContent label, bool foldout, bool toggleOnLabelClick, Action<bool> action, GUIStyle background = null)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.Foldout(rect, labelWidth, label, foldout, toggleOnLabelClick, action, background));
        }

        public static void TextField(float width, string value, Action<string> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.TextField(rect, value, action));
        }
        public static void TextField(float width, string label, string value, Action<string> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.TextField(rect, label, value, action));
        }
        public static void TextField(float width, float labelWidth, string label, string value, Action<string> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.TextField(rect, labelWidth, label, value, action));
        }
        public static void TextField(float width, GUIContent label, string value, Action<string> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.TextField(rect, label, value, action));
        }
        public static void TextField(float width, float labelWidth, GUIContent label, string value, Action<string> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.TextField(rect, labelWidth, label, value, action));
        }

        public static void IntField(float width, int value, Action<int> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.IntField(rect, value, action));
        }
        public static void IntField(float width, string label, int value, Action<int> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.IntField(rect, label, value, action));
        }
        public static void IntField(float width, float labelWidth, string label, int value, Action<int> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.IntField(rect, labelWidth, label, value, action));
        }
        public static void IntField(float width, GUIContent label, int value, Action<int> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.IntField(rect, label, value, action));
        }
        public static void IntField(float width, float labelWidth, GUIContent label, int value, Action<int> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.IntField(rect, labelWidth, label, value, action));
        }

        public static void FloatField(float width, float value, Action<float> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.FloatField(rect, value, action));
        }
        public static void FloatField(float width, string label, float value, Action<float> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.FloatField(rect, label, value, action));
        }
        public static void FloatField(float width, float labelWidth, string label, float value, Action<float> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.FloatField(rect, labelWidth, label, value, action));
        }
        public static void FloatField(float width, GUIContent label, float value, Action<float> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.FloatField(rect, label, value, action));
        }
        public static void FloatField(float width, float labelWidth, GUIContent label, float value, Action<float> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.FloatField(rect, labelWidth, label, value, action));
        }

        public static void DoubleField(float width, double value, Action<double> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.DoubleField(rect, value, action));
        }
        public static void DoubleField(float width, string label, double value, Action<double> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.DoubleField(rect, label, value, action));
        }
        public static void DoubleField(float width, float labelWidth, string label, double value, Action<double> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.DoubleField(rect, labelWidth, label, value, action));
        }
        public static void DoubleField(float width, GUIContent label, double value, Action<double> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.DoubleField(rect, label, value, action));
        }
        public static void DoubleField(float width, float labelWidth, GUIContent label, double value, Action<double> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.DoubleField(rect, labelWidth, label, value, action));
        }

        public static void LongField(float width, long value, Action<long> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.LongField(rect, value, action));
        }
        public static void LongField(float width, string label, long value, Action<long> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.LongField(rect, label, value, action));
        }
        public static void LongField(float width, float labelWidth, string label, long value, Action<long> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.LongField(rect, labelWidth, label, value, action));
        }
        public static void LongField(float width, GUIContent label, long value, Action<long> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.LongField(rect, label, value, action));
        }
        public static void LongField(float width, float labelWidth, GUIContent label, long value, Action<long> action)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.LongField(rect, labelWidth, label, value, action));
        }

        public static void ToggleRight(float width, bool value, Action<bool> action, GUIStyle style = null)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.ToggleRight(rect, value, action, style));
        }
        public static void ToggleRight(float width, string label, bool value, Action<bool> action, GUIStyle style = null)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.ToggleRight(rect, label, value, action, style));
        }
        public static void ToggleRight(float width, float labelWidth, string label, bool value, Action<bool> action, GUIStyle style = null)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.ToggleRight(rect, labelWidth, label, value, action, style));
        }
        public static void ToggleRight(float width, GUIContent label, bool value, Action<bool> action, GUIStyle style = null)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.ToggleRight(rect, label, value, action, style));
        }
        public static void ToggleRight(float width, float labelWidth, GUIContent label, bool value, Action<bool> action, GUIStyle style = null)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.ToggleRight(rect, labelWidth, label, value, action, style));
        }

        public static void ToggleLeft(float width, bool value, Action<bool> action, GUIStyle style = null)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.ToggleLeft(rect, value, action, style));
        }
        public static void ToggleLeft(float width, string label, bool value, Action<bool> action, GUIStyle style = null)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.ToggleLeft(rect, label, value, action, style));
        }
        public static void ToggleLeft(float width, float labelWidth, string label, bool value, Action<bool> action, GUIStyle style = null)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.ToggleLeft(rect, labelWidth, label, value, action, style));
        }
        public static void ToggleLeft(float width, GUIContent label, bool value, Action<bool> action, GUIStyle style = null)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.ToggleLeft(rect, label, value, action, style));
        }
        public static void ToggleLeft(float width, float labelWidth, GUIContent label, bool value, Action<bool> action, GUIStyle style = null)
        {
            Rect rect = GetNext(width, sm_FieldHeight);
            DrawElement(rect, GUIDrawCommand.ToggleLeft(rect, labelWidth, label, value, action, style));
        }

        public static void Label(string text, GUIStyle style = null)
        {
            Rect rect = GetNext(text, style);
            DrawElement(rect, GUIDrawCommand.Label(rect, text, style));
        }
        public static void Label(GUIContent text, GUIStyle style = null)
        {
            Rect rect = GetNext(text, style);
            DrawElement(rect, GUIDrawCommand.Label(rect, text, style));
        }

        public static void Image(Vector2 size, GUIStyle style = null)
        {
            Rect rect = GetNext(size);
            DrawElement(rect, GUIDrawCommand.Image(rect, style));
        }
        public static void Image(Vector2 size, string text, GUIStyle style = null)
        {
            Rect rect = GetNext(size);
            DrawElement(rect, GUIDrawCommand.Image(rect, text, style));
        }
        public static void Image(Vector2 size, GUIContent text, GUIStyle style = null)
        {
            Rect rect = GetNext(size);
            DrawElement(rect, GUIDrawCommand.Image(rect, text, style));
        }
        public static void Image(Vector2 size, Texture texture)
        {
            Rect rect = GetNext(size);
            DrawElement(rect, GUIDrawCommand.Image(rect, texture));
        }

        public static void Button(Vector2 size, string text, Action action, GUIStyle style = null)
        {
            Rect rect = GetNext(size);
            DrawElement(rect, GUIDrawCommand.Button(rect, text, action, style));
        }
        public static void Button(string text, Action action, GUIStyle style = null)
        {
            Vector2 size = CalculateSize(text) + Vector2.right * 6;
            Rect rect = GetNext(size);
            DrawElement(rect, GUIDrawCommand.Button(rect, text, action, style));
        }

        public static void Rect(Vector2 size, Color color)
        {
            Rect rect = GetNext(size);
            DrawElement(rect, GUIDrawCommand.Rect(rect, color));
        }

        public static void Space(Vector2 size)
        {
            Rect rect = GetNext(size);
            DrawElement(rect, null);
        }
        public static void Space(float space)
        {
            Vector2 direction = GUIActiveGroup.SortType == GroupSortType.Horizontal ? Vector2.right : Vector2.up;
            Rect rect = GetNext(direction * space);
            DrawElement(rect, null);
        }

        private static void DrawElement(Rect rect, Command command)
        {
            GUIElement element = GetElement(rect);
            GUIActiveGroup?.AddElement(element);
            UpdateLastRect(rect);
            EnqueueCommand(command);
        }

        public static EniGUILayoutOption Padding(float pudding)
        {
            return GetOption(EniGUILayoutOption.TypeOption.SetPudding, pudding);
        }
        public static EniGUILayoutOption ElementSpacing(float spacing)
        {
            return GetOption(EniGUILayoutOption.TypeOption.SetElementSpacing, spacing);
        }
        public static EniGUILayoutOption ExpandWidth(bool isExpand)
        {
            return GetOption(EniGUILayoutOption.TypeOption.SetExpandWidth, isExpand);
        }
        public static EniGUILayoutOption ExpandHeight(bool isExpand)
        {
            return GetOption(EniGUILayoutOption.TypeOption.SetExpandHeight, isExpand);
        }
        public static EniGUILayoutOption Width(float width)
        {
            return GetOption(EniGUILayoutOption.TypeOption.SetWidth, width);
        }
        public static EniGUILayoutOption Height(float height)
        {
            return GetOption(EniGUILayoutOption.TypeOption.SetHeight, height);
        }

        public static void FillActiveGroup(GUIStyle style)
        {
            if (GUIActiveGroup == null)
                throw new Exception();

            GUIActiveGroup.Style = style;
        }
        public static void FillActiveGroup(Color color)
        {
            if (GUIActiveGroup == null)
                throw new Exception();

            GUIActiveGroup.Color = color;
        }
        public static void FillLastGroup(GUIStyle style)
        {
            if (GUILastGroup == null)
                throw new Exception();

            GUILastGroup.Style = style;
        }
        public static void FillLastGroup(Color color)
        {
            if (GUILastGroup == null)
                throw new Exception();

            GUILastGroup.Color = color;
        }

        private static void BeginGroup(GUIGroup group)
        {
            if (GUIActiveGroup != null)
                GUIActiveGroup.AddElement(group);

            SetActiveGroup(group);
            EnqueueCommand(GUIDrawCommand.Group(group));
            UpdateLastRect(group.Rect);
        }
        private static void EndGroup()
        {
            if (GUIActiveGroup == null)
                throw new Exception();

            ReturnGroup(GUIActiveGroup);

            GUIActiveGroup.RecalculateSize();
            SetActiveGroup(GUIActiveGroup.Parent);

            if (GUIActiveGroup == null)
                sm_CommandQueue.Execute();
        }

        private static void BeginScrollView(GUIScrollView scrollView)
        {
            EniGUILayoutOption[] options = { ExpandHeight(true), ExpandWidth(true) };
            scrollView.Container.ApplyOptions(options);
            ReturnOptions(options);

            BeginGroup(scrollView);
            BeginGroup(scrollView.Container);

            float height = scrollView.Rect.height - 14;
            float width = scrollView.Rect.width - 14;

            Rect scrollViewRect = new Rect(scrollView.Rect);
            scrollViewRect.size = new Vector2(width, height);

            EnqueueCommand(EniGUIClip.BeginClip(scrollViewRect));

            scrollView.UpdatePosition();
        }

        private static GUIGroup GetGroup(GroupSortType sortType, bool isClipped, GUIStyle style, Color color, params EniGUILayoutOption[] options)
        {
            Rect rect = new Rect(GetNext().position, Vector2.zero);
            return GetGroup(sortType, rect, isClipped, style, color, options);
        }
        private static GUIGroup GetGroup(GroupSortType sortType, Rect rect, bool isClipped, GUIStyle style, Color color, params EniGUILayoutOption[] options)
        {
            GUIGroup group = GetGroup();

            group.Rect = rect;
            group.SortType = sortType;
            group.IsClipped = isClipped;
            group.Style = style;
            group.Color = color;

            group.ApplyOptions(options);
            ReturnOptions(options);

            return group;
        }
        private static GUIGroup GetGroup()
        {
            return sm_GroupPool.Get();
        }
        private static void ReturnGroup(GUIGroup group)
        {
            Command command = () =>
            {
                group.ForEach((element) => { ReturnElement(element); });

                if (group is GUIScrollView scrollView)
                    sm_ScrollViewPool.Return(scrollView);
                else
                    sm_GroupPool.Return(group);
            };

            EnqueueCommand(command);
        }

        private static GUIScrollView GetScrollView(GroupSortType sortType, Vector2 size, Vector2 scrollPosition,
            bool alwaysShowHorizontal, bool alwaysShowVertical, Action<Vector2> action,
            GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, params EniGUILayoutOption[] options)
        {
            Rect rect = new Rect(GetNext().position, size);
            Rect viewRect = new Rect(0, 0, 0, 0);

            return GetScrollView(sortType, rect, scrollPosition,
                alwaysShowHorizontal, alwaysShowVertical, action, horizontalScrollbar,
                verticalScrollbar, options);
        }
        private static GUIScrollView GetScrollView(GroupSortType sortType, Vector2 size, Vector2 scrollPosition, 
            Vector2 viewSize, bool alwaysShowHorizontal, bool alwaysShowVertical, Action<Vector2> action,
            GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, params EniGUILayoutOption[] options)
        {
            Rect rect = new Rect(GetNext().position, size);
            Rect viewRect = new Rect(Vector2.zero, viewSize);

            return GetScrollView(sortType, rect, scrollPosition, viewRect, 
                alwaysShowHorizontal, alwaysShowVertical, action, horizontalScrollbar, 
                verticalScrollbar, options);
        }
        private static GUIScrollView GetScrollView(GroupSortType sortType, Rect rect, Vector2 scrollPosition,
            bool alwaysShowHorizontal, bool alwaysShowVertical, Action<Vector2> action,
            GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, params EniGUILayoutOption[] options)
        {
            Rect viewRect = new Rect(rect);
            viewRect.position = Vector2.down * 18;

            return GetScrollView(sortType, rect, scrollPosition, viewRect, 
                alwaysShowHorizontal, alwaysShowVertical, action, horizontalScrollbar, 
                verticalScrollbar, options);
        }
        private static GUIScrollView GetScrollView(GroupSortType sortType, Rect rect, Vector2 scrollPosition,
            Rect viewRect, bool alwaysShowHorizontal, bool alwaysShowVertical, Action<Vector2> action,
            GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, params EniGUILayoutOption[] options)
        {
            GUIScrollView scrollView = GetScrollView();

            scrollView.Rect = rect;
            scrollView.SortType = sortType;
            scrollView.Container.Rect = viewRect;
            scrollView.Container.SortType = sortType;
            scrollView.ScrollPosition = scrollPosition;
            scrollView.AlwaysShowHorizontal = alwaysShowHorizontal;
            scrollView.AlwaysShowVertical = alwaysShowVertical;
            scrollView.Action = action;
            scrollView.HorizontalScrollbar = horizontalScrollbar;
            scrollView.VerticalScrollbar = verticalScrollbar;

            scrollView.Container.ApplyOptions(options);
            ReturnOptions(options);

            return scrollView;
        }

        private static GUIScrollView GetScrollView()
        {
            GUIScrollView scrollView = sm_ScrollViewPool.Get();
            scrollView.Container = GetGroup();
            return scrollView;
        }

        private static GUIElement GetElement(Rect rect)
        {
            //return new GUIElement(rect);
            return sm_ElementPool.Get(rect);
        }
        private static void ReturnElement(GUIElement element)
        {
            sm_ElementPool.Return(element);
        }

        private static EniGUILayoutOption GetOption(EniGUILayoutOption.TypeOption type, object value)
        {
            return sm_OptionPool.Get(type, value);
        }
        private static void ReturnOptions(EniGUILayoutOption[] options)
        {
            foreach (EniGUILayoutOption option in options)
                ReturnOption(option);
        }
        private static void ReturnOption(EniGUILayoutOption option)
        {
            sm_OptionPool.Return(option);
        }

        private static void SetActiveGroup(GUIGroup group)
        {
            GUILastGroup = GUIActiveGroup;
            GUIActiveGroup = group;
        }

        private static void EnqueueCommand(Command command)
        {
            sm_CommandQueue.Enqueue(command);
        }

        private static void UpdateLastRect(Rect rect)
        {
            GUILastRect = rect;
        }

        private static Rect GetNext(GUIContent text, GUIStyle style = null)
        {
            return GetNext(CalculateSize(text, style));
        }
        private static Rect GetNext(string text, GUIStyle style = null)
        {
            return GetNext(CalculateSize(text, style));
        }
        private static Rect GetNext(float width, float height)
        {
            return GetNext(new Vector2(width, height));
        }
        private static Rect GetNext(Vector2 size)
        {
            return new Rect(GetNext().position, size);
        }
        private static Rect GetNext()
        {
            if (GUIActiveGroup == null)
                return UnityEngine.Rect.zero;

            return GUIActiveGroup.GetNext();
        }

        public static Vector2 CalculateSize(string text, GUIStyle style = null)
        {
            style = style ?? new GUIStyle(GUI.skin.label);
            return style.CalcSize(new GUIContent(text)) + Vector2.one * 2;
        }
        public static Vector2 CalculateSize(GUIContent text, GUIStyle style = null)
        {
            style = style ?? new GUIStyle(GUI.skin.label);
            return style.CalcSize(text) + Vector2.one * 2;
        }
        public static Vector2 CalculateSize(float width)
        {
            return new Vector2(width, 18);
        }
    }

    public static class EniGUIClip
    {
        private static List<Vector2> sm_ClappedAreas = new List<Vector2>();

        public static Command BeginClip(Rect rect)
        {
            sm_ClappedAreas.Add(rect.position);
            return () => { GUI.BeginClip(rect); };
        }

        public static Command EndClip()
        {
            sm_ClappedAreas.Remove(sm_ClappedAreas.Last());
            return () => { GUI.EndClip(); };
        }

        public static Rect UnClip(Rect rect)
        {
            Rect result = new Rect(UnClip(rect.position), rect.size);
            return result;
        }

        public static Vector2 UnClip(Vector2 position)
        {
            Vector2 result = Vector2.zero;

            foreach (Vector2 clappedArea in sm_ClappedAreas)
                result += clappedArea;

            result += position;
            return result;
        }
    }

    public static class GUIControlUtility
    {
        public static bool MouseLeftClickDown(Rect rect)
        {
            return MouseClickDown(rect, 0);
        }

        public static bool MouseRightClickDown(Rect rect)
        {
            return MouseClickDown(rect, 1);
        }

        private static bool MouseClickDown(Rect rect, int button)
        {
            return rect.Contains(EditorInput.GetMousePosition()) && EditorInput.GetMouseButtonDown(button);
        }

        public static void Repaint()
        {
            EditorWindow.focusedWindow?.Repaint();
        }
    }

    public class EniWindow : EditorWindow
    {
        public bool IsInitialized { get; private set; }
        public Vector2 Size { get; private set; }

        private void OnEnable()
        {
            IsInitialized = false;
        }

        private void OnGUI()
        {
            Initialize();

            if (Resize())
                return;

            EditorInput.UpdateInput();
            Draw();
        }

        private void OnDisable()
        {
            OnClose();
        }

        private void Initialize()
        {
            if (IsInitialized)
                return;

            OnOpen();
            IsInitialized = true;
        }
        private void Draw()
        {
            EniGUILayout.BeginVertical(EniGUILayout.Padding(0), EniGUILayout.ElementSpacing(0),
                EniGUILayout.ExpandWidth(true), EniGUILayout.ExpandHeight(true));
            {
                OnDraw();
            }
            EniGUILayout.EndVertical();
        }
        private bool Resize()
        {
            if (position.size == Size)
                return false;

            Size = position.size;

            OnResize();
            Repaint();

            return true;
        }

        protected virtual void OnOpen() { }
        protected virtual void OnDraw() { }
        protected virtual void OnClose() { }
        protected virtual void OnResize() { }
    }

    public class ReorderableList<T>
    {
        private List<T> m_List;

        private Vector2 m_SelectedElementSize;
        private Rect m_InteractableRect;

        private readonly Vector2 m_MovedElementSize = new Vector2(16, 16);

        public int SelectedElementIndex { get; private set; }
        public int LastSelectedElementIndex { get; private set; }
        public int TargetElementIndex { get; private set; }

        public T SelectedElement
        {
            get
            {
                return LastSelectedElementIndex == -1 ? default : m_List[LastSelectedElementIndex];
            }
        }

        public event Action<int> OnDrawElementByIndex;
        public event Action<T> OnDrawElementByT;

        public event Action OnDrawBackground;
        public event Action OnDrawHoverBackground;
        public event Action OnDrawSelectedBackground;

        public void AttachList(List<T> list)
        {
            if (list == null)
                throw new Exception();

            m_List = list;
        }

        public void Draw(float width)
        {
            if (m_List == null)
                return;

            EniGUILayout.BeginVertical(EniGUILayout.Width(width),
                EniGUILayout.Padding(0), EniGUILayout.ElementSpacing(0),
                EniGUILayout.ExpandWidth(true), EniGUILayout.ExpandHeight(true));
            {
                for (int i = 0; i < m_List.Count; i++)
                {
                    DrawElementByReorderable(i);
                    DrawElementBackground(i);
                    Control(i);
                }
            }
            EniGUILayout.EndVertical();

            if (SelectedElementIndex > -1)
                DrawMovedElement();
        }

        private void DrawElementByReorderable(int index)
        {
            if (TargetElementIndex == index && SelectedElementIndex > -1)
                DrawVoidElement();
            else
                DrawElement(index);
        }

        private void DrawElement(int index)
        {
            float width = EniGUILayout.GUIActiveGroup.Rect.width;

            EniGUILayout.BeginHorizontal(EniGUILayout.Width(width), EniGUILayout.Padding(0),
                EniGUILayout.ElementSpacing(0), EniGUILayout.ExpandHeight(true),
                EniGUILayout.ExpandWidth(true));
            {
                EniGUILayout.Image(m_MovedElementSize, EnigmaticStyles.elementMoveIcon);
                m_InteractableRect = EniGUILayout.GUILastRect;

                OnDrawElementByIndex?.Invoke(index);
                OnDrawElementByT?.Invoke(m_List[index]);
            }
            EniGUILayout.EndHorizontal();
        }

        private void DrawVoidElement()
        {
            EniGUILayout.BeginHorizontal(EniGUILayout.Width(m_SelectedElementSize.x),
                EniGUILayout.Height(m_SelectedElementSize.y));
            EniGUILayout.EndHorizontal();
        }

        private void DrawMovedElement()
        {
            Rect rect = EniGUILayout.GUILastGroup.Rect;

            float x = rect.x;
            float y = EditorInput.GetMousePosition().y - 8;

            float maxYPosition = rect.y + rect.height - 18;

            y = Mathf.Clamp(y, rect.y, maxYPosition);

            Rect rect2 = new Rect(x, y, 0, 0);

            EniGUILayout.BeginHorizontal(rect2, EniGUILayout.Padding(0),
                EniGUILayout.ExpandWidth(true), EniGUILayout.ExpandHeight(true));
            {
                DrawElement(SelectedElementIndex);
            }
            EniGUILayout.EndHorizontal();

            DrawElementBackground(SelectedElementIndex);
        }

        private void DrawElementBackground(int index)
        {
            if (SelectedElementIndex == -1 && TargetElementIndex == -1 && LastSelectedElementIndex == -1)
                DrawBackground();
            else if (SelectedElementIndex == index || LastSelectedElementIndex == index)
                DrawSelectedBackground();
            else if (TargetElementIndex == index)
                DrawHoverBackground();
        }

        private void DrawBackground()
        {
            OnDrawBackground?.Invoke();
        }
        private void DrawHoverBackground()
        {
            if (OnDrawHoverBackground == null)
                EniGUILayout.FillLastGroup(EnigmaticStyles.HeverColor);
            else
                OnDrawHoverBackground?.Invoke();
        }
        private void DrawSelectedBackground()
        {
            if (OnDrawSelectedBackground == null)
                EniGUILayout.FillLastGroup(EnigmaticStyles.AltDarkThemeBlueElementSelected);
            else
                OnDrawSelectedBackground?.Invoke();
        }

        private void Control(int index)
        {
            Rect rect = EniGUILayout.GUILastGroup.Rect;
            Vector2 mousePosition = EditorInput.GetMousePosition();

            if (GUIControlUtility.MouseLeftClickDown(m_InteractableRect)
                && SelectedElementIndex == -1)
            {
                SelectedElementIndex = index;
                LastSelectedElementIndex = index;
                m_SelectedElementSize = rect.size;

                GUI.FocusControl(null);
            }

            if (rect.Contains(mousePosition))
            {
                TargetElementIndex = index;
                GUIControlUtility.Repaint();
            }

            if (SelectedElementIndex > -1 && TargetElementIndex > -1)
            {
                if (SelectedElementIndex != TargetElementIndex)
                    MoveElements();

                LastSelectedElementIndex = SelectedElementIndex;
            }

            if (EditorInput.GetMouseButtonUp(0))
            {
                SelectedElementIndex = -1;
                TargetElementIndex = -1;
            }
        }

        private void MoveElements()
        {
            T tempList = m_List[SelectedElementIndex];

            m_List[SelectedElementIndex] = m_List[TargetElementIndex];
            m_List[TargetElementIndex] = tempList;

            SelectedElementIndex = TargetElementIndex;
            TargetElementIndex = -1;

            GUIControlUtility.Repaint();
        }
    }

    public class HierarchicalReorderableList<T> where T : TreeNode<T>
    {
        private List<T> m_List;

        private List<T> m_SelectedNode = new List<T>();
        private T m_HoverNode;
        private Rect m_HoverRect;

        private T m_ReorderParent;
        private int m_ReorderIndex;
        private Rect m_ReorderRect;
        private bool IsReordering;

        private bool m_IsDragging;
        private bool m_IsClickOnSelectedNode;
        private T m_TrySelectElement;

        private CommandQueue m_CommandQueue = new CommandQueue();

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
                else if (m_IsDragging == true && value == false)
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

        public event Action<T> OnSelectNode;

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
            if (m_List == null || m_List.Count == 0)
                return;

            ResetClick();
            DrawNodes(width, elementSpacing);

            DrawReorderLine();
            HandleDrag();

            SelectLastHoverElement();

            m_CommandQueue.Execute();

            if(IsDragging)
                EditorGUIUtility.AddCursorRect(new Rect(0, 0, Screen.width, Screen.height), MouseCursor.MoveArrow);

            EnigmaticGUIUtility.Repaint();
        }

        private void DrawNodes(float width, float elementSpacing = 0)
        {
            EniGUILayout.BeginVertical(EniGUILayout.Width(width), EniGUILayout.Padding(0),
                EniGUILayout.ElementSpacing(elementSpacing), EniGUILayout.ExpandHeight(true));
            {
                foreach (T node in m_List)
                {
                    DrawNode(node, width);
                    Control(node);
                    DrawNodeChild(node, width);
                }
            }
            EniGUILayout.EndVertical();
        }

        private void DrawNode(T node, float width)
        {
            EniGUILayout.BeginHorizontal(EniGUILayout.Width(width),
                EniGUILayout.Padding(0), EniGUILayout.ExpandHeight(true));
            {
                OnDrawNode?.Invoke(node);
            }
            EniGUILayout.EndHorizontal();

            DrawNodeBackground(node);
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

        private void DrawNodeBackground(T node)
        {
            if (m_SelectedNode.Contains(node))
                DrawSelectedBackground(node);
            else if (m_HoverNode == node)
                DrawHoverBackground(node);
            else
                DrawBackground(node);
        }

        private void DrawBackground(T node)
        {
            OnDrawBackground?.Invoke(node);
        }
        private void DrawHoverBackground(T node)
        {
            if (OnDrawHoverNodeBackground == null)
                EniGUILayout.FillLastGroup(EnigmaticStyles.DarkedBackgroundColor);
            else
                OnDrawHoverNodeBackground.Invoke(node);
        }
        private void DrawSelectedBackground(T node)
        {
            if (OnDrawSelectedNodeBackground == null)
                EniGUILayout.FillLastGroup(EnigmaticStyles.DarkThemeBlueElementSelected);
            else
                OnDrawSelectedNodeBackground.Invoke(node);
        }

        private void Control(T node)
        {
            Rect rect = EniGUILayout.GUILastGroup.Rect;
            Rect rectUnclipped = EniGUIClip.UnClip(rect);

            HandleSelection(node, rectUnclipped);

            if(IsDragging)
                HandleReorder(node, rect);

            if (rectUnclipped.Contains(EditorInput.GetMousePosition()))
            {
                m_HoverNode = node;
                m_HoverRect = rect;
            }
        }

        private void Reparent()
        {
            if (m_HoverNode == null)
                return;

            if (m_SelectedNode.Count > 0
                && EditorInput.GetButtonPress(KeyCode.LeftControl) == false)
            {
                foreach (T tNode in m_SelectedNode)
                {
                    m_CommandQueue.Enqueue(() =>
                    {
                        if (m_HoverNode.OnValidAddChild(tNode))
                        {
                            if (OnReparent == null || OnReparent.Invoke((m_HoverNode, tNode)))
                            {
                                m_List.Remove(tNode);
                                tNode.Parent?.RemoveChild(tNode);
                                m_HoverNode.AddChild(tNode);
                            }
                        }
                    });
                }
            }
        }

        private bool Reorder()
        {
            if (IsReordering == false)
                return false;

            foreach (T node in m_SelectedNode)
            {
                bool reorder = OnReorder == null ? false : OnReorder.Invoke((m_ReorderParent, node));

                if (node == null || reorder || m_ReorderParent == node)
                    continue;

                m_CommandQueue.Enqueue(() =>
                {
                    m_List.Remove(node);
                    node.Parent?.RemoveChild(node);
                });

                if (m_ReorderParent == null)
                {
                    m_CommandQueue.Enqueue(() =>
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
                    m_CommandQueue.Enqueue(() =>
                    {
                        if (m_ReorderIndex < m_ReorderParent.ChildCount)
                            m_ReorderParent.Insert(m_ReorderIndex, node);
                        else
                            m_ReorderParent.AddChild(node);

                        m_ReorderIndex++;
                    });
                }
            }

            GUIControlUtility.Repaint();
            return true;
        }

        private void HandleSelection(T node, Rect rect)
        {
            //rect = EnigmaticGUIClip.UnClip(rect);

            if (GUIControlUtility.MouseLeftClickDown(rect))
            {
                if (m_SelectedNode.Contains(node))
                {
                    m_IsClickOnSelectedNode = true;
                    m_TrySelectElement = node;
                }

                if (EditorInput.GetButtonPress(KeyCode.LeftControl))
                {
                    if (m_SelectedNode.Contains(node))
                    {
                        m_SelectedNode.Remove(node);
                        m_TrySelectElement = null;
                    }
                    else
                    {
                        AdditionalSelectElement(node);
                    }
                }
                else if (m_IsClickOnSelectedNode == false)
                {
                    SelectElement(node);
                    m_IsClickOnSelectedNode = true;
                }

                EditorInput.UpdateLastMousePosition();
            }
        }

        private void HandleReorder(T node, Rect rect)
        {
            Rect elementRect = new Rect(rect.position - (Vector2.up * 1.5f), new Vector2(rect.width, 3));
            Rect elementRectUnclipped = EniGUIClip.UnClip(elementRect);

            if (elementRectUnclipped.Contains(EditorInput.GetMousePosition()))
            {
                m_ReorderParent = node.Parent;
                m_ReorderIndex = m_ReorderParent == null ? m_List.IndexOf(node) : m_ReorderParent.IndexOf(node);
                m_ReorderRect = elementRect;
                IsReordering = true;
            }
        }

        private void SelectElement(T node)
        {
            m_SelectedNode.Clear();
            m_SelectedNode.Add(node);

            OnSelectNode?.Invoke(node);
        }

        private void AdditionalSelectElement(T node)
        {
            m_SelectedNode.Add(node);
            OnSelectNode?.Invoke(node);
        }

        private void ResetClick()
        {
            if(EditorInput.GetMouseButtonPress(0) == false)
                m_IsClickOnSelectedNode = false;
        }

        private void DrawReorderLine()
        {
            if (IsDragging && EniGUIClip.UnClip(m_ReorderRect).Contains(EditorInput.GetMousePosition()))
            {
                EniGUILayout.BeginHorizontal(m_ReorderRect, EnigmaticStyles.AltDarkThemeBlueElementSelected);
                EniGUILayout.EndHorizontal();
            }
        }

        private void SelectLastHoverElement()
        {
            if (m_HoverNode != null && m_HoverNode == m_TrySelectElement 
                && EditorInput.GetMouseButtonUp(0) && IsDragging == false)
            {
                SelectElement(m_TrySelectElement);
            }
        }

        private void HandleDrag()
        {
            if (EniGUILayout.GUILastGroup.Rect.Contains(EditorInput.GetMousePosition()) == false)
                return;

            float delta = Vector2.Distance(EditorInput.GetMousePosition(), EditorInput.GetLastMousePosition());

            IsDragging = (m_IsClickOnSelectedNode || EditorInput.GetMouseButtonPress(0))
                && delta > 3 && m_SelectedNode.Count > 0;

            IsReordering = false;
        }
    }

    public static class DrawingUtilities
    {
        public static void BeginToolBar(float width)
        {
            EniGUILayout.BeginHorizontal(EditorStyles.toolbar, EniGUILayout.Padding(0),
                EniGUILayout.ElementSpacing(0), EniGUILayout.Width(width), EniGUILayout.Height(21));
        }
        public static void EndToolBar()
        {
            EniGUILayout.EndHorizontal();
        }

        public static void BeginColum(float width, float height, float elementSpacing = 0, params EniGUILayoutOption[] options)
        {
            EniGUILayout.BeginVertical(EnigmaticStyles.columnBackground, EniGUILayout.Width(width),
                EniGUILayout.Height(height), EniGUILayout.Padding(0), EniGUILayout.ElementSpacing(elementSpacing));

            EniGUILayout.GUIActiveGroup.ApplyOptions(options);
        }
        public static void EndColum()
        {
            EniGUILayout.EndVertical();
        }
    }

    public class EnigmaticGUITestWindow : EniWindow
    {
        Rect rect => new Rect(0, 0, position.width, 18);
        private ReorderableList<int> reorderableList = new ReorderableList<int>();
        private HierarchicalReorderableList<TestNode> hierarchicalReorderableList = new HierarchicalReorderableList<TestNode>();
        List<int> list = new List<int>(10);
        List<TestNode> list2 = new List<TestNode>(10);
        Vector2 scrollPositon = Vector2.zero;

        [MenuItem("Tools/EnigmaticGUITestWindow")]
        private static void Open()
        {
            EnigmaticGUITestWindow window = GetWindow<EnigmaticGUITestWindow>();
            window.Show();
        }

        private void OnGUITest()
        {
            //---------------------------------------------------------------------------------------

            //EditorGUI.Vector2Field(); +
            //EditorGUI.Vector3Field(); +
            //EditorGUI.Vector2IntField(); +
            //EditorGUI.Vector3IntField(); +

            //EditorGUI.BoundsField(); +
            //EditorGUI.BoundsIntField(); +

            //EditorGUI.ColorField(); +
            //EditorGUI.GradientField(); + 
            //EditorGUI.CurveField(); +

            //EditorGUI.ProgressBar();

            //EditorGUI.LinkButton();

            //EditorGUI.EnumPopup(); + 
            //EditorGUI.Popup(); +

            //EditorGUI.LayerField(); +
            //EditorGUI.MaskField(); +
            //EditorGUI.EnumFlagsField(); +

            //EditorGUI.PropertyField(); +
            //EditorGUI.ObjectField(); +

            //EditorGUI.RectField(); + 
            //EditorGUI.RectIntField(); + 

            //EditorGUI.MinMaxSlider(); -
            //EditorGUI.IntSlider(); +
            //EditorGUI.Slider(); +

            //EditorGUI.Foldout();

            //---------------------------------------------------------------------------------------

            //EniGUI.ObjectField<ScriptableObject>(rect, null, false);
            //EniGUI.ObjectField<ScriptableObject>(rect, "TestField", null, false);
            //EniGUI.ObjectField<ScriptableObject>(rect, 10, "TestField", null, false);

            //EniGUI.BoundsIntField(rect, new BoundsInt());
        }

        protected override void OnOpen()
        {
            base.OnOpen();

            //reorderableList = new ReorderableList<int>();

            //list = new List<int>(10);

            //for (int i = 0; i < 10; i++)
            //    list.Add(UnityEngine.Random.Range(0, 1024));

            //reorderableList.AttachList(list);

            //reorderableList.OnDrawElementByIndex += (index) =>
            //{
            //    EniGUILayout.IntField(rect.width - 20, list[index], (value) => { list[index] = value; });
            //};

            hierarchicalReorderableList.AttachList(list2);
            hierarchicalReorderableList.OnDrawNode += (node) => 
            {
                EniGUILayout.Space(node.DepthLevel * 16);
                EniGUILayout.Label($"{node.Value} {node.DepthLevel}");
            };
        }

        protected override void OnDraw()
        {
            base.OnDraw();
            //reorderableList.Draw(rect.width);

            //EniGUI.Image(new Rect(0, 0, 100, 100), "");

            //EniGUILayout.Image(new Vector2(100, 100));

            //EniGUILayout.BeginHorizontal("box", EniGUILayout.Width(100), EniGUILayout.Height(100));
            //EniGUILayout.EndHorizontal();

            //DrawingUtilities.BeginToolBar(rect.width);
            //DrawingUtilities.EndToolBar();

            //EniGUILayout.BeginVertical(EniGUILayout.ExpandWidth(true), EniGUILayout.ExpandHeight(true));
            //{
            //    EniGUILayout.RectField(rect.width, new Rect(), (x) => { });
            //    EniGUILayout.FloatField(rect.width, 12, (x) => { });
            //    EniGUILayout.TextField(rect.width, "Hello", (x) => { });
            //    EniGUILayout.Vector3Field(rect.width, Vector3.zero, (x) => { });
            //    EniGUILayout.Button(new Vector2(rect.width, 18), "TestButton", () => { list2.Add(new TestNode("TreeNode")); });
            //}
            //EniGUILayout.EndVertical();

            //DrawingUtilities.BeginColum(rect.width, rect.height - 21);
            //DrawingUtilities.EndColum();

            EniGUILayout.Button(new Vector2(rect.width, 18), "AddNode", () => { list2.Add(new TestNode("TreeNode")); });

            //EniGUILayout.BeginScrollView(GroupSortType.Vertical, Size / 2, scrollPositon,
            //    Size / 2, false, false, (x) => { scrollPositon = x; }) ;
            //{
            //    foreach (TestNode node in list2)
            //        EniGUILayout.Label(node.Value);
            //}
            //EniGUILayout.EndScrollView();

            EniGUILayout.BeginVerticalScrollView(Size / 2, scrollPositon, (x) => { scrollPositon = x; });
            {
                float width = EniGUILayout.GUIActiveGroup.Rect.width;
                hierarchicalReorderableList.Draw(width);
            }
            EniGUILayout.EndVerticalScrollView();
        }
    }

    public class TestNode : TreeNode<TestNode>
    {
        public string Value;

        public TestNode(string value)
        {
            Value = value;
        }
    }
}