#if UNITY_EDITOR

using System;
using System.Collections.Generic;

using Unity.Profiling;
using UnityEditor;
using UnityEngine;

namespace Enigmatic.Core.Editor
{
    public static class EnigmaticGUILayout
    {
        private static ProfilerMarker DrawGroup = new ProfilerMarker(ProfilerCategory.Render, "DrawGroup");

        private static GUIGroup sm_GUIActiveGroup;
        private static GUIGroup sm_LastGUIGroup;
        private static Rect sm_LastGUIRect;

        private static GUIGroupPool sm_GroupPool = new GUIGroupPool();
        private static GUIElementPool sm_ElementsPool = new GUIElementPool();

        private static Queue<GUIGroup> sm_UsedGroup = new Queue<GUIGroup>();
        private static Queue<GUIElement> sm_UsedElement = new Queue<GUIElement>();

        private static bool sm_IsClick;

        public static void Reset()
        {
            sm_IsClick = false;
        }

        public static GUIGroup GetActiveGroup() => sm_GUIActiveGroup;

        public static GUIGroup GetLastGroup() => sm_LastGUIGroup;

        public static void BeginHorizontal(params EnigmaticGUILayoutOption[] options)
        {
            Rect rect = new Rect(Vector2.zero, Vector2.zero);

            if (sm_GUIActiveGroup != null)
                rect = new Rect(sm_GUIActiveGroup.GetNext().position, Vector2.zero);

            BeginHorizontal(rect, options);
        }

        public static void BeginHorizontal(GUIStyle style, params EnigmaticGUILayoutOption[] options)
        {
            BeginHorizontal(options);
            FillActiveGroupImage(style);
        }

        public static void BeginHorizontal(Rect rect, GUIStyle style, int border = 0, params EnigmaticGUILayoutOption[] options)
        {
            BeginHorizontal(rect, options);
            FillActiveGroupImage(style);
        }

        public static void BeginHorizontal(GUIStyle style, int border = 0, params EnigmaticGUILayoutOption[] options)
        {
            BeginHorizontal(options);
            FillActiveGroupImage(style);
        }

        public static void BeginHorizontal(Rect rect, params EnigmaticGUILayoutOption[] options)
        {
            GUIGroup group = sm_GroupPool.GetGUIGroup(rect, GroupSortType.Horizontal);
            group.ApplyOptions(options);

            if (sm_GUIActiveGroup != null)
                sm_GUIActiveGroup.AddElement(group);

            ChangeActiveGroup(group);
            EnqueueCaller(EnigmaticGUILayoutPostDrawerCaller.Group(sm_GUIActiveGroup));
        }

        public static void BeginHorizontal(Rect rect, bool isAddedGroup, params EnigmaticGUILayoutOption[] options)
        {
            GUIGroup Group = sm_GroupPool.GetGUIGroup(rect, GroupSortType.Horizontal);
            Group.ApplyOptions(options);

            if (sm_GUIActiveGroup != null && isAddedGroup)
                sm_GUIActiveGroup.AddElement(Group);

            ChangeActiveGroup(Group);
            EnqueueCaller(EnigmaticGUILayoutPostDrawerCaller.Group(sm_GUIActiveGroup));
        }

        public static bool EndHorizontal()
        {
            return EndGroup();
        }

        public static void BeginVertical(GUIStyle style, params EnigmaticGUILayoutOption[] options)
        {
            BeginVertical(options);
            FillActiveGroupImage(style);
        }

        public static void BeginVertical(Rect rect, GUIStyle style, int border = 0, params EnigmaticGUILayoutOption[] options)
        {
            BeginVertical(rect, options);
            FillActiveGroupImage(style);
        }

        public static void BeginVertical(GUIStyle style, int border = 0, params EnigmaticGUILayoutOption[] options)
        {
            BeginVertical(options);
            FillActiveGroupImage(style);
        }

        public static void BeginVertical(params EnigmaticGUILayoutOption[] options)
        {
            Vector2 position = Vector2.zero;

            if (sm_GUIActiveGroup != null)
                position = sm_GUIActiveGroup.GetNext().position;

            Rect rect = new Rect(position, Vector2.zero);
            BeginVertical(rect, options);
        }

        public static void BeginVertical(Rect rect, params EnigmaticGUILayoutOption[] options)
        {
            GUIGroup Group = sm_GroupPool.GetGUIGroup(rect, GroupSortType.Vertical);
            Group.ApplyOptions(options);

            if (sm_GUIActiveGroup != null)
                sm_GUIActiveGroup.AddElement(Group);

            ChangeActiveGroup(Group);
            EnqueueCaller(EnigmaticGUILayoutPostDrawerCaller.Group(sm_GUIActiveGroup));
        }

        public static bool EndVertical()
        {
            return EndGroup();
        }

        public static void BeginScrollView(Rect rect, Rect viewRect, Vector2 scrollPosition, bool alwaysShowHorizontal,
            bool alwaysShowVertical, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, GUIStyle background,
            params EnigmaticGUILayoutOption[] options)
        {
            ScrollView scrollView = new ScrollView(rect, viewRect, scrollPosition,
                alwaysShowHorizontal, alwaysShowVertical, horizontalScrollbar, verticalScrollbar,
                background, GroupSortType.Vertical);

            scrollView.ApplyOptions(ExpandHeight(true), ExpandWidth(true));
            scrollView.ApplyOptions(options);
            scrollView.UpdatePosition();
            scrollView.isCliped = true;

            if (sm_GUIActiveGroup != null)
                sm_GUIActiveGroup.AddElement(scrollView);

            ChangeActiveGroup(scrollView);

            EnigmaticGUIClip.BeginClip(scrollView.ViewRect);
        }

        public static void BeginScrollView(Rect rect, Rect viewRect, Vector2 scrollPosition,
            bool alwaysShowHorizontal = true, bool alwaysShowVertical = true, params EnigmaticGUILayoutOption[] options)
        {
            BeginScrollView(rect, viewRect, scrollPosition, alwaysShowHorizontal, alwaysShowVertical,
                GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, GUI.skin.scrollView, options);
        }

        public static void BeginHorizontalScrollView(Rect rect, Rect viewRect, Vector2 scrollPosition,
            params EnigmaticGUILayoutOption[] options)
        {
            BeginScrollView(rect, viewRect, scrollPosition, true, false,
                GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, GUI.skin.scrollView, options);
        }

        public static void BeginVerticalScrollView(Rect rect, Rect viewRect, Vector2 scrollPosition,
            params EnigmaticGUILayoutOption[] options)
        {
            BeginScrollView(rect, viewRect, scrollPosition, false, true,
                GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, GUI.skin.scrollView, options);
        }

        public static Vector2 EndScrollView(Action repaint)
        {
            if (sm_GUIActiveGroup is ScrollView scrollView == false)
                throw new Exception();

            scrollView.RecalculateSize();

            Vector2 result = EnigmaticGUI.ScrollView(scrollView.Rect, scrollView.ViewRect, scrollView.ScrollPosition,
                scrollView.AlwaysShowHorizontal, scrollView.AlwaysShowVertical, scrollView.HorizontalScrollbar,
                scrollView.VerticalScrollbar, scrollView.Background);

            if (result != scrollView.ScrollPosition)
                repaint?.Invoke();

            EnigmaticGUIClip.EndClip();

            EndGroup();
            return result;
        }

        public static bool EndGroup()
        {
            if (sm_GUIActiveGroup == null)
                Debug.LogError("Ending group");

            DrawGroup.Begin();
            sm_GUIActiveGroup.RecalculateSize();

            bool onClick = false;

            if (sm_GUIActiveGroup.IsClickable && sm_IsClick == false)
            {
                onClick = EnigmaticGUIUtility.OnClick(sm_GUIActiveGroup.Rect, 0);

                if (onClick)
                    sm_IsClick = true;
            }

            if (sm_GUIActiveGroup.ParentGroup != null)
                ChangeActiveGroup(sm_GUIActiveGroup.ParentGroup);
            else
                ChangeActiveGroup(null);

            if (sm_GUIActiveGroup == null)
                GUIPostDrawQueue.CallAll();

            if (onClick == true)
                EnigmaticGUIUtility.Repaint();

            while (sm_UsedGroup.Count > 0)
            {
                GUIGroup group = sm_UsedGroup.Dequeue();
                sm_GroupPool.RemoveGroup(group);
            }

            while (sm_UsedElement.Count > 0)
            {
                GUIElement element = sm_UsedGroup.Dequeue();
                sm_ElementsPool.RemoveElement(element);
            }

            DrawGroup.End();

            return onClick;
        }

        public static void BeginDisabledGroup(bool condition)
        {
            EnqueueCaller((x) => EditorGUI.BeginDisabledGroup((bool)x[0]), condition);
            EditorGUI.BeginDisabledGroup(condition);
        }

        public static void EndDisabledGroup()
        {
            EnqueueCaller((x) => EditorGUI.EndDisabledGroup());
            EditorGUI.EndDisabledGroup();
        }

        public static bool ButtonCentric(string text, Vector2 offset, GUIStyle style = null)
        {
            Vector2 size = CalculateSize(text) + Vector2.right * 6;
            bool onClick = false;

            onClick = ButtonCentric(text, size, offset, style);

            return onClick;
        }

        public static bool ButtonCentric(string text, Vector2 size, Vector2 offset, GUIStyle style = null)
        {
            bool onClick = false;

            BeginHorizontal(Padding(0), ElementSpacing(0), ExpandHeight(true), ExpandWidth(true));
            {
                Space(offset.x);

                BeginVertical(Padding(0), ElementSpacing(0), ExpandHeight(true), ExpandWidth(true));
                {
                    Space(offset.y);

                    onClick = Button(text, size, style);
                }
                EndVertical();
            }
            EndHorizontal();

            return onClick;
        }

        public static bool Button(string text, GUIStyle style = null)
        {
            Vector2 size = CalculateSize(text) + Vector2.right * 6;
            return Button(text, size, style);
        }

        public static bool Button(string text, Vector2 size, GUIStyle style = null)
        {
            Vector2 position = sm_GUIActiveGroup.GetNext().position;
            Rect rect = new Rect(position, size);
            GUIElement element = sm_ElementsPool.GetGUIElement(rect);
            sm_GUIActiveGroup.AddElement(element);

            UpdateLastGUIRect(rect);

            EnqueueCaller(EnigmaticGUILayoutPostDrawerCaller.Button(element, text, style));

            return sm_IsClick == false && EnigmaticGUIUtility.OnClick(rect, 0) ? sm_IsClick = true : false;
        }

        public static void Label(string name, GUIStyle style = null)
        {
            Vector2 position = sm_GUIActiveGroup.GetNext().position;
            Vector2 size = CalculateSize(name, style);
            Rect rect = new Rect(position, size);
            GUIElement element = sm_ElementsPool.GetGUIElement(rect);
            sm_GUIActiveGroup.AddElement(element);

            UpdateLastGUIRect(rect);

            EnqueueCaller(EnigmaticGUILayoutPostDrawerCaller.Label(name, element, style));
        }

        public static void Label(string name, float width, GUIStyle style = null)
        {
            Vector2 position = sm_GUIActiveGroup.GetNext().position;
            Vector2 size = CalculateSize(name, style);
            size.x = width;

            Rect rect = new Rect(position, size);
            GUIElement element = sm_ElementsPool.GetGUIElement(rect);
            sm_GUIActiveGroup.AddElement(element);

            UpdateLastGUIRect(rect);

            EnqueueCaller(EnigmaticGUILayoutPostDrawerCaller.Label(name, element, style, width));
        }

        public static void CentricLabel(string name, Vector2 offset, GUIStyle style = null)
        {
            BeginVertical(ElementSpacing(0), Padding(0));
            {
                Space(offset.x);

                BeginHorizontal(ElementSpacing(0), Padding(0));
                {
                    Space(offset.y);

                    Label(name, style);
                }
                EndHorizontal();
            }
            EndVertical();
        }

        public static void Image(Vector2 size, GUIStyle style = null)
        {
            Vector2 position = sm_GUIActiveGroup.GetNext().position;
            Rect rect = new Rect(position, size);
            GUIElement element = sm_ElementsPool.GetGUIElement(rect);
            sm_GUIActiveGroup.AddElement(element);

            UpdateLastGUIRect(rect);

            EnqueueCaller(EnigmaticGUILayoutPostDrawerCaller.Image(element, style));
        }

        public static void Image(Vector2 size, Texture texture)
        {
            Vector2 position = sm_GUIActiveGroup.GetNext().position;
            Rect rect = new Rect(position, size);
            GUIElement element = sm_ElementsPool.GetGUIElement(rect);
            sm_GUIActiveGroup.AddElement(element);

            UpdateLastGUIRect(rect);

            EnqueueCaller(EnigmaticGUILayoutPostDrawerCaller.Image(element, texture));
        }

        public static void ImageCentric(Vector2 size, Vector2 offset, GUIStyle style = null)
        {
            BeginHorizontal(Padding(0), ExpandHeight(true), ExpandWidth(true));
            Space(offset.x);

            BeginVertical(Padding(0), ExpandHeight(true), ExpandWidth(true));
            Space(offset.y);

            Vector2 position = sm_GUIActiveGroup.GetNext().position;
            Rect rect = new Rect(position, size);
            GUIElement element = sm_ElementsPool.GetGUIElement(rect);
            sm_GUIActiveGroup.AddElement(element);

            UpdateLastGUIRect(rect);

            EnqueueCaller(EnigmaticGUILayoutPostDrawerCaller.Image(element, style));

            EndVertical();

            EndHorizontal();
        }

        public static void Rect(Vector2 size, Color color)
        {
            Vector2 position = sm_GUIActiveGroup.GetNext().position;
            Rect rect = new Rect(position, size);
            GUIElement element = sm_ElementsPool.GetGUIElement(rect);
            
            EnqueueCaller(EnigmaticGUILayoutPostDrawerCaller.Rect(element, color));
        }

        public static void Rect(Vector2 size, Vector2 offset, Color color)
        {
            Vector2 position = sm_GUIActiveGroup.GetNext().position;
            position += offset;
            Rect rect = new Rect(position, size);
            GUIElement element = sm_ElementsPool.GetGUIElement(rect);

            EnqueueCaller(EnigmaticGUILayoutPostDrawerCaller.Rect(element, color));
        }

        public static void Rect(Rect rect, Color color)
        {
            GUIElement element = sm_ElementsPool.GetGUIElement(rect);
            EnqueueCaller(EnigmaticGUILayoutPostDrawerCaller.Rect(element, color));
        }

        public static void FillActiveGroupColor(Color color)
        {
            GetActiveGroup().Color = color;
        }

        public static void FillActiveGroupImage(GUIStyle style)
        {
            GetActiveGroup().Style = style;
        }

        public static void FillLastGroupColor(Color color)
        {
            GetLastGroup().Color = color;
        }

        public static void FillLastGroupImage(GUIStyle style)
        {
            GetLastGroup().Style = style;
        }

        public static void Port(Vector2 size, GUIStyle style = null)
        {
            float x = sm_GUIActiveGroup.GetNext().position.x;
            float y = sm_GUIActiveGroup.GetNext().position.y + 9 - size.y / 2;

            Vector2 position = new Vector2(x, y);
            Rect rect = new Rect(position, CalculateSize(size));
            GUIElement element = sm_ElementsPool.GetGUIElement(rect);
            sm_GUIActiveGroup.AddElement(element);

            UpdateLastGUIRect(rect);

            EnqueueCaller(EnigmaticGUILayoutPostDrawerCaller.Image(element, style));
        }

        public static float FloatField(string name, float value, float widthFieldArea = -1)
        {
            if (widthFieldArea == -1)
                widthFieldArea = sm_GUIActiveGroup.GetFreeArea().x;

            Vector2 position = sm_GUIActiveGroup.GetNext().position;
            Vector2 size = CalculateSize(widthFieldArea);
            Rect rect = new Rect(position, size);
            GUIElement element = sm_ElementsPool.GetGUIElement(rect);

            sm_GUIActiveGroup.AddElement(element);

            int guid = GUIgUID.Next(name, rect, value.ToString());
            EnqueueCaller(EnigmaticGUILayoutPostDrawerCaller.FloatField(element, guid, value, name));

            if (GUIValueCasher.TryGetValue(guid, out object result) == false)
                result = value;

            return (float)result;
        }

        public static float FloatField(string name, float value, float textWidth, float widthFieldArea = -1)
        {
            if (widthFieldArea == -1)
                widthFieldArea = sm_GUIActiveGroup.GetFreeArea().x;

            float result = value;

            BeginHorizontal(Width(widthFieldArea), ExpandHeight(true), Padding(0));
            {
                Label(name, textWidth);
                result = FloatField("", value);
            }
            EndHorizontal();

            return result;
        }

        public static int IntField(string name, int value, float widthFieldArea = -1)
        {
            if (widthFieldArea == -1)
                widthFieldArea = sm_GUIActiveGroup.GetFreeArea().x;

            Vector2 position = sm_GUIActiveGroup.GetNext().position;
            Vector2 size = CalculateSize(widthFieldArea);
            Rect rect = new Rect(position, size);
            GUIElement element = sm_ElementsPool.GetGUIElement(rect);

            sm_GUIActiveGroup.AddElement(element);

            int guid = GUIgUID.Next(name, rect, $"{value}");
            EnqueueCaller(EnigmaticGUILayoutPostDrawerCaller.IntField(element, guid, value, name));

            if (GUIValueCasher.TryGetValue(guid, out object result) == false)
                result = value;

            return (int)result;
        }

        public static int IntField(string name, int value, float textWidth, float widthFieldArea = -1)
        {
            if (widthFieldArea == -1)
                widthFieldArea = sm_GUIActiveGroup.GetFreeArea().x;

            int result = value;

            BeginHorizontal(Width(widthFieldArea), ExpandHeight(true), Padding(0));
            {
                Label(name, textWidth);
                result = IntField("", value);
            }
            EndHorizontal();

            return result;
        }

        public static string TextField(string name, string value, float widthFieldArea = -1)
        {
            Vector2 position = GetNextPosition() + Vector2.up;

            if (widthFieldArea == -1)
                widthFieldArea = sm_GUIActiveGroup.GetFreeArea().x;

            Vector2 size = CalculateSize(widthFieldArea);
            Rect rect = new Rect(position, size);
            GUIElement element = sm_ElementsPool.GetGUIElement(rect);
            sm_GUIActiveGroup.AddElement(element);

            int guid = GUIgUID.Next(name, rect, value);

            EnqueueCaller(EnigmaticGUILayoutPostDrawerCaller.TextField(element, guid, value, name));

            if (GUIValueCasher.TryGetValue(guid, out object result) == false)
                result = value;

            return (string)result;
        }

        public static string TextField(string name, string value, float textWidth, float widthFieldArea = -1)
        {
            if (widthFieldArea == -1)
                widthFieldArea = sm_GUIActiveGroup.GetFreeArea().x;

            string result = value;

            BeginHorizontal(Width(widthFieldArea), ExpandHeight(true), Padding(0));
            {
                Label(name, textWidth);
                result = TextField("", value);
            }
            EndHorizontal();

            return (string)result;
        }

        public static void Space(float space)
        {
            Vector2 position = GetNextPosition();

            Rect rect = new Rect();

            if (sm_GUIActiveGroup.SortType == GroupSortType.Horizontal)
                rect = new Rect(position, Vector2.right * space);
            else if (sm_GUIActiveGroup.SortType == GroupSortType.Vertical)
                rect = new Rect(position, Vector2.up * space);

            sm_GUIActiveGroup.AddElement(sm_ElementsPool.GetGUIElement(rect));

            UpdateLastGUIRect(rect);
        }

        public static void SpaceBox(Vector2 size)
        {
            Vector2 position = GetNextPosition();
            Rect rect = new Rect(position, size);
            sm_GUIActiveGroup.AddElement(sm_ElementsPool.GetGUIElement(rect));

            UpdateLastGUIRect(rect);
        }

        public static void Property(ESerializedProperty property, float widthFieldArea = -1)
        {
            if (widthFieldArea == -1)
                widthFieldArea = sm_GUIActiveGroup.GetFreeArea().x - 6;

            Vector2 position = GetNextPosition();
            GUIElement element = sm_ElementsPool.GetGUIElement(new Rect(position, CalculateSize(property, widthFieldArea)));
            sm_GUIActiveGroup.AddElement(element);
        } //Old

        public static void PropertyField(SerializedProperty property, float widthFieldArea = -1, string fieldName = "")
        {
            if (widthFieldArea == -1)
                widthFieldArea = sm_GUIActiveGroup.GetFreeArea().x - 6;

            Vector2 position = GetNextPosition();
            Rect rect = new Rect(position, CalculateSize(property, widthFieldArea));
            GUIElement element = sm_ElementsPool.GetGUIElement(rect);
            sm_GUIActiveGroup.AddElement(element);

            UpdateLastGUIRect(rect);

            EnqueueCaller(EnigmaticGUILayoutPostDrawerCaller.PropertyField(property, element, fieldName));
        }

        public static UnityEngine.Object ObjectField(string filedName, UnityEngine.Object obj, Type objectType, float widthFieldArea = -1)
        {
            if (widthFieldArea == -1)
                widthFieldArea = sm_GUIActiveGroup.GetFreeArea().x;

            Vector2 position = GetNextPosition();
            Rect rect = new Rect(position, CalculateSize(new Vector2(widthFieldArea, 18)));
            GUIElement element = sm_ElementsPool.GetGUIElement(rect);
            sm_GUIActiveGroup.AddElement(element);

            UpdateLastGUIRect(rect);

            int guid = GUIgUID.Next(filedName, rect, 0);
            EnqueueCaller(EnigmaticGUILayoutPostDrawerCaller.ObjectField(element, widthFieldArea, filedName, obj, objectType, guid));

            if (GUIValueCasher.TryGetValue(guid, out object result) == false)
                result = obj;

            return (UnityEngine.Object)result;
        }

        public static UnityEngine.Object ObjectField(string filedName, UnityEngine.Object obj, Type objectType, float textWidth, float widthFieldArea = -1)
        {
            if (widthFieldArea == -1)
                widthFieldArea = sm_GUIActiveGroup.GetFreeArea().x;

            UnityEngine.Object result = obj;

            BeginHorizontal(Width(widthFieldArea), ExpandHeight(true), Padding(0));
            {
                Label(filedName, textWidth);
                result = ObjectField("", obj, objectType);
            }
            EndHorizontal();

            return result;
        } //Check

        public static T ObjectField<T>(string filedName, T obj, float widthFieldArea = -1) where T : UnityEngine.Object
        {
            return (T)ObjectField(filedName, obj, typeof(T), widthFieldArea);
        }

        public static T ObjectField<T>(string filedName, T obj, float textWidth, float widthFieldArea = -1) where T : UnityEngine.Object
        {
            if (widthFieldArea == -1)
                widthFieldArea = sm_GUIActiveGroup.GetFreeArea().x;

            T result = obj;

            BeginHorizontal(Width(widthFieldArea), ExpandHeight(true), Padding(0));
            {
                Label(filedName, textWidth);
                result = ObjectField<T>("", obj);
            }
            EndHorizontal();

            return result;
        }

        public static Color ColorField(string fieldName, Color value, float widthFieldArea = -1)
        {
            if (widthFieldArea == -1)
                widthFieldArea = sm_GUIActiveGroup.GetFreeArea().x;

            Vector2 position = GetNextPosition();
            Rect rect = new Rect(position, CalculateSize(new Vector2(widthFieldArea, 18)));
            GUIElement element = sm_ElementsPool.GetGUIElement(rect);
            sm_GUIActiveGroup.AddElement(element);

            UpdateLastGUIRect(rect);

            int guid = GUIgUID.Next(fieldName, rect, value);

            EnqueueCaller(EnigmaticGUILayoutPostDrawerCaller
                .ColorField(element, widthFieldArea, fieldName, value, guid));

            if (GUIValueCasher.TryGetValue(guid, out object result) == false)
                result = value;

            return (Color)result;
        }

        public static Color ColorField(string filedName, Color value, float textWidth, float widthFieldArea = -1)
        {
            if (widthFieldArea == -1)
                widthFieldArea = sm_GUIActiveGroup.GetFreeArea().x;

            Color result = value;

            BeginHorizontal(Width(widthFieldArea), ExpandHeight(true), Padding(0));
            {
                Label(filedName, textWidth);
                result = ColorField("", value);
            }
            EndHorizontal();

            return result;
        } //Check

        public static Enum EnumPopup(string filedName, Enum selected, float widthFieldArea = -1)
        {
            if (widthFieldArea == -1)
                widthFieldArea = sm_GUIActiveGroup.GetFreeArea().x;

            Vector2 position = GetNextPosition();
            Rect rect = new Rect(position, CalculateSize(new Vector2(widthFieldArea, 18)));
            GUIElement element = sm_ElementsPool.GetGUIElement(rect);
            sm_GUIActiveGroup.AddElement(element);

            UpdateLastGUIRect(rect);

            int guid = GUIgUID.Next(filedName, rect, selected);
            EnqueueCaller(EnigmaticGUILayoutPostDrawerCaller.EnumPopup(element, widthFieldArea, filedName, selected, guid));

            if (GUIValueCasher.TryGetValue(guid, out object result) == false)
                result = selected;

            return (Enum)result;
        }

        public static Enum EnumPopup(string filedName, Enum selected, float textWidth, float widthFieldArea = -1)
        {
            if (widthFieldArea == -1)
                widthFieldArea = sm_GUIActiveGroup.GetFreeArea().x;

            Enum result = selected;

            BeginHorizontal(Width(widthFieldArea), ExpandHeight(true), Padding(0));
            {
                Label(filedName, textWidth);
                result = EnumPopup("", selected);
            }
            EndHorizontal();

            return result;
        }

        public static void PropertyField(ESerializedProperty property, float widthFieldArea = -1)
        {
            if (widthFieldArea == -1)
                widthFieldArea = sm_GUIActiveGroup.GetFreeArea().x - 6;

            Vector2 position = GetNextPosition();
            Rect rect = new Rect(position, CalculateSize(property, widthFieldArea));
            GUIElement element = sm_ElementsPool.GetGUIElement(rect);
            sm_GUIActiveGroup.AddElement(element);

            UpdateLastGUIRect(rect);

            EnqueueCaller(EnigmaticGUILayoutPostDrawerCaller.PropertyField(property, rect));
        } //Old

        public static bool BeginFoldout(ref bool isExpanded, string displayName, Action repaintAction,
            GUIStyle foldoutBackground, float width = -1)
        {
            BeginVertical(Width(width), Padding(0), ElementSpacing(0), ExpandHeight(true));

            if (foldoutBackground == null)
                BeginHorizontal(Width(width), Clickable(true), ExpandHeight(true), Padding(0));
            else
                BeginHorizontal(foldoutBackground, 7, Width(width), Clickable(true), ExpandHeight(true), Padding(0));
            {
                if (isExpanded)
                    Port(Vector2.one * 20, EnigmaticStyles.foldoutButtonOpen);
                else
                    Port(Vector2.one * 20, EnigmaticStyles.foldoutButtonClose);

                Label(displayName);
            }
            if (EndHorizontal())
            {
                isExpanded = !isExpanded;
                repaintAction?.Invoke();
            }

            if (isExpanded == false)
            {
                EndVertical();
                return isExpanded;
            }

            BeginVertical(ExpandHeight(true), Width(width));

            return isExpanded;
        }

        public static bool BeginFoldout(ref bool isExpanded, string displayName, Action repaintAction, float width = -1)
        {
            return BeginFoldout(ref isExpanded, displayName, repaintAction, EnigmaticStyles.foldoutBackground, width);
        }

        public static void EndFoldout(bool isExpanded)
        {
            if (isExpanded == false)
                return;

            EndVertical();
            EndVertical();
        }

        public static void EnqueueCaller(Action<object[]> action, params object[] parameters)
        {
            GUIPostDrawQueue.Enqueue(action, parameters);
        }

        public static void EnqueueCaller(DeferredActionCaller caller)
        {
            GUIPostDrawQueue.Enqueue(caller);
        }

        public static Vector2 GetNextPosition()
        {
            if (sm_GUIActiveGroup == null)
                return Vector2.zero;

            return sm_GUIActiveGroup.GetNext().position;
        }

        public static EnigmaticGUILayoutOption Padding(float pudding)
        {
            return new EnigmaticGUILayoutOption(EnigmaticGUILayoutOption.TypeOption.SetPudding, pudding);
        }

        public static EnigmaticGUILayoutOption ElementSpacing(float spacing)
        {
            return new EnigmaticGUILayoutOption(EnigmaticGUILayoutOption.TypeOption.SetElementSpacing, spacing);
        }

        public static EnigmaticGUILayoutOption ExpandWidth(bool isExpand)
        {
            return new EnigmaticGUILayoutOption(EnigmaticGUILayoutOption.TypeOption.SetExpandWidth, isExpand);
        }

        public static EnigmaticGUILayoutOption ExpandHeight(bool isExpand)
        {
            return new EnigmaticGUILayoutOption(EnigmaticGUILayoutOption.TypeOption.SetExpandHeight, isExpand);
        }

        public static EnigmaticGUILayoutOption Width(float width)
        {
            return new EnigmaticGUILayoutOption(EnigmaticGUILayoutOption.TypeOption.SetWidth, width);
        }

        public static EnigmaticGUILayoutOption Height(float height)
        {
            return new EnigmaticGUILayoutOption(EnigmaticGUILayoutOption.TypeOption.SetHeight, height);
        }

        public static EnigmaticGUILayoutOption Clickable(bool isClicable)
        {
            return new EnigmaticGUILayoutOption(EnigmaticGUILayoutOption.TypeOption.SetClickable, isClicable);
        }

        public static Vector2 CalculateSize(float widthFieldArea)
        {
            return new Vector2(widthFieldArea, 18);
        }

        public static Vector2 CalculateSize(string name, GUIStyle style = null)
        {
            if (style == null)
                style = new GUIStyle(GUI.skin.label);

            return style.CalcSize(new GUIContent(name)) + Vector2.one * 2;
        }

        public static Vector2 CalculateSize(Vector2 size)
        {
            return size;
        }

        public static Vector2 CalculateSize(ESerializedProperty property, float widthFieldArea)
        {
            Vector2 result = Vector2.zero;

            result += CalculateSize(widthFieldArea);

            if (property.IsArray == true || property.IsList == true)
            {
                if (property.IsExpanded)
                {
                    for (int i = 0; i < property.ChildPropertes.Length; i++)
                    {
                        result.y += CalculateSize(property.ChildPropertes[i], widthFieldArea).y;

                        if (i < property.ChildPropertes.Length)
                            result.y += 6;
                    }
                }
            }

            return result;
        } //Old

        public static Vector2 CalculateSize(UnityEditor.SerializedProperty property, float widthFieldArea)
        {
            if (sm_GUIActiveGroup != null)
            {
                if (widthFieldArea == -1)
                    widthFieldArea = sm_GUIActiveGroup.GetFreeArea().x - 6;
            }

            return new Vector2(widthFieldArea, EditorGUI.GetPropertyHeight(property, true));
        }

        public static float GetElementSize(Vector2 size)
        {
            if (sm_GUIActiveGroup.SortType == GroupSortType.Horizontal)
                return size.x;
            else if (sm_GUIActiveGroup.SortType == GroupSortType.Vertical)
                return size.y;

            return -1;
        }

        public static void UpdateLastGUIRect(Rect rect) => sm_LastGUIRect = rect;

        public static Rect GetLastGUIRect() => EnigmaticGUIClip.UnClip(sm_LastGUIRect);

        private static void ChangeActiveGroup(GUIGroup Group)
        {
            sm_LastGUIGroup = sm_GUIActiveGroup;
            sm_GUIActiveGroup = Group;
        }
    }
}

#endif