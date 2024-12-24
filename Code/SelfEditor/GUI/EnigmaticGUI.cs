#if UNITY_EDITOR

using System;

using UnityEditor;
using UnityEngine;
using Unity.Profiling;

namespace Enigmatic.Core.Editor
{
    public static class EnigmaticGUI
    {
        private static Rect sm_LastGUIRect;

        public static Rect ZoomedArea { get; private set; }

        public static bool Button(string text, Vector2 position, params GUILayoutOption[] gUILayoutOptions)
        {
            bool result;

            GUILayout.BeginArea(new Rect(position.x, position.y, 100, 100));
            result = GUILayout.Button(text, gUILayoutOptions);
            GUILayout.EndArea();

            return result;
        } // Check

        public static void Image(Vector2 size, string text, GUIStyle style)
        {
            GUILayout.BeginHorizontal();
            {
                float offset = size.y / 2;

                GUILayout.Space(offset);

                GUILayout.BeginVertical();
                {
                    GUILayout.Space(offset);

                    GUILayout.Box(text, EnigmaticStyles.Port,
                        GUILayout.Width(size.x), GUILayout.Height(size.y));
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndHorizontal();
        } //Delete

        public static void sLabel(string text, GUIStyle style = null, float s = 0)
        {
            if (style == null)
                style = new GUIStyle(GUI.skin.label);

            float width = EnigmaticGUILayout.CalculateSize(text).x;
            GUILayout.Label(text, style, GUILayout.MinWidth(width));
        }  //Delete

        public static void PropertyField(UnityEditor.SerializedProperty property, float fieldWidth)
        {
            if (property.isArray)
            {
                EditorGUILayout.PropertyField(property, new GUIContent(property.displayName), GUILayout.MinWidth(30));
                return;
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginHorizontal(GUILayout.Width(fieldWidth / 2), GUILayout.MaxWidth(fieldWidth / 2));
                {
                    sLabel(property.displayName);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(GUILayout.Width(fieldWidth / 2), GUILayout.MaxWidth(fieldWidth / 2), GUILayout.MinWidth(30));
                {
                    EditorGUILayout.PropertyField(property, GUIContent.none, GUILayout.MinWidth(60));
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndHorizontal();
        } //Delete

        public static void PropertyField(Vector2 position, SerializedProperty property, float fieldWidth, string fieldName = "")
        {
            float height = EditorGUI.GetPropertyHeight(property, true);
            Rect fieldRect = new Rect(position, new Vector2(fieldWidth, height));

            if (string.IsNullOrEmpty(fieldName))
                fieldName = property.displayName;

            if (property.isArray)
            {
                EditorGUI.PropertyField(fieldRect, property, new GUIContent(fieldName));
                return;
            }

            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = fieldWidth / 2;

            EditorGUI.PropertyField(fieldRect, property, new GUIContent(fieldName));

            UpdateLastGUIRect(new Rect(position, new Vector2(fieldWidth, height)));

            EditorGUIUtility.labelWidth = labelWidth;
        }

        public static UnityEngine.Object ObjectField(Vector2 position, float fieldWidth,
            string fieldName, UnityEngine.Object obj, Type objectType)
        {
            float height = 18;
            Rect fieldRect = new Rect(position, new Vector2(fieldWidth, height));

            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = fieldWidth / 2;
            UnityEngine.Object result = obj;

            try { result = EditorGUI.ObjectField(fieldRect, new GUIContent(fieldName), obj, objectType, false); }
            catch (Exception e) { string a = e.ToString(); }

            UpdateLastGUIRect(fieldRect);

            EditorGUIUtility.labelWidth = labelWidth;

            return result;
        }

        public static Color ColorField(Vector2 position, float fieldWidth,
            string fieldName, Color value)
        {
            float height = 18;
            Rect fieldRect = new Rect(position, new Vector2(fieldWidth, height));

            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = fieldWidth / 2;
            Color result = value;

            try { result = EditorGUI.ColorField(fieldRect, new GUIContent(fieldName), value); }
            catch (Exception e) { string a = e.ToString(); }

            UpdateLastGUIRect(fieldRect);

            EditorGUIUtility.labelWidth = labelWidth;

            return result;
        }

        public static Enum EnumPopup(Vector2 position, float fieldWidth, string fieldName, Enum selected)
        {
            float height = 18;
            Rect fieldRect = new Rect(position, new Vector2(fieldWidth, height));

            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = fieldWidth / 2;
            Enum result = selected;

            try { result = EditorGUI.EnumPopup(fieldRect, new GUIContent(fieldName), selected); }
            catch (Exception e) { string a = e.ToString(); }

            UpdateLastGUIRect(fieldRect);

            EditorGUIUtility.labelWidth = labelWidth;

            return result;
        }

        public static bool Foldout(Vector2 position, bool isExpanded, string displayName, float width,
            GUIStyle background = null, GUIStyle lable = null, GUIStyle foldout = null)
        {
            bool result = false;

            Rect rectBackground = new Rect(position, new Vector2(width, 18));
            Rect rectFoldout = new Rect(position + Vector2.right * 6, Vector2.one * 16);

            Image(rectBackground, background);
            Image(rectFoldout, foldout);
            //Label(displayName, )

            //result = EditorGUI.Foldout(rect, isExpanded, displayName);
            //UpdateLastGUIRect(rect);

            return result;
        }

        public static bool Button(string text, Vector2 position, GUIStyle style = null)
        {
            if (style == null)
                style = GUI.skin.button;

            Vector2 size = EnigmaticGUILayout.CalculateSize(text, style) + Vector2.one * 6;
            return Button(text, position, size, style);
        }

        public static bool Button(string text, Vector2 position, Vector2 size, GUIStyle style = null)
        {
            if (style == null)
                style = GUI.skin.button;

            GUIStyle styleToUse = style;
            Rect rect = new Rect(position, size);

            if (EnigmaticGUIUtility.IsHover(new Rect(position, size)))
            {
                GUIStyle hoveredStyle = new GUIStyle(style);
                hoveredStyle.normal.background = style.hover.background;

                GUI.Box(rect, "", EnigmaticStyles.whiteBox);
            }

            GUI.Box(rect, text, styleToUse);
            EditorGUIUtility.GetControlID((int)(position.x + position.y), FocusType.Passive);

            //EnigmaticGUIUtility.Repaint();

            return EnigmaticGUIUtility.OnClick(rect, 0);
        }

        public static bool Button(string text, float x, float y, params GUILayoutOption[] gUILayoutOptions)
        {
            return Button(text, new Vector2(x, y), gUILayoutOptions);
        } // Delete

        public static void BeginZoomedElement()
        {
            GUI.BeginGroup(ZoomedArea);
        }

        public static void EndZoomedElement()
        {
            GUI.EndGroup();
        }

        public static float DrawFloatField(string fieldName, float value, float space = 3)
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(fieldName);
            GUILayout.Space(space);
            float result = EditorGUILayout.FloatField(value);

            EditorGUILayout.EndHorizontal();

            return result;
        }

        public static void Label(string name, Vector2 position, GUIStyle style = null, float width = -1)
        {
            if (style == null)
                style = new GUIStyle(EditorStyles.label);

            Vector2 lableSize = style.CalcSize(new GUIContent(name)) + Vector2.one * 2;

            if(width > 0)
                lableSize.x = width;

            GUI.Label(new Rect(position, lableSize), name, style);
            UpdateLastGUIRect(new Rect(position, lableSize));
        }

        public static void Image(Rect rect, GUIStyle style = null)
        {
            if (style == null)
                style = new GUIStyle(GUI.skin.box);

            GUI.Box(rect, "", style);
            UpdateLastGUIRect(rect);
        }

        public static void Image(Rect rect, Texture texture)
        {
            GUI.DrawTexture(rect, texture);
            UpdateLastGUIRect(rect);
        }

        public static void DrawRect(Rect rect, Color color)
        {
            EditorGUI.DrawRect(rect, color);
            UpdateLastGUIRect(rect);
        }

        public static float FloatField(string name, float value, Vector2 position, float widthFieldArea)
        {
            Vector2 lableSize = new Vector2(widthFieldArea / 2, 18f);
            GUI.Label(new Rect(position, lableSize), name);

            Vector2 fieldPosition = position + Vector2.right * (widthFieldArea / 2);
            Vector2 fieldSize = new Vector2(widthFieldArea / 2, 18f);
            float result = EditorGUI.FloatField(new Rect(fieldPosition, fieldSize), value);
            UpdateLastGUIRect(new Rect(position, new Vector2(widthFieldArea, 18f)));

            EditorGUIUtility.GetControlID((int)(position.x + position.y), FocusType.Passive);

            return result;
        }

        public static float FloatField(float value, Vector2 position, float widthFieldArea)
        {
            Vector2 fieldSize = new Vector2(widthFieldArea, 18f);
            float result = EditorGUI.FloatField(new Rect(position, fieldSize), value);
            UpdateLastGUIRect(new Rect(position, new Vector2(widthFieldArea, 18f)));

            EditorGUIUtility.GetControlID((int)(position.x + position.y), FocusType.Passive);

            return result;
        }

        public static int IntField(string name, int value, Vector2 position, float widthFieldArea)
        {
            Vector2 lableSize = new Vector2(widthFieldArea / 2, 18f);
            GUI.Label(new Rect(position, lableSize), name);

            Vector2 fieldPosition = position + Vector2.right * (widthFieldArea / 2);
            Vector2 fieldSize = new Vector2(widthFieldArea / 2, 18f);
            int result = EditorGUI.IntField(new Rect(fieldPosition, fieldSize), value);
            UpdateLastGUIRect(new Rect(position, new Vector2(widthFieldArea, 18f)));

            EditorGUIUtility.GetControlID((int)(position.x + position.y), FocusType.Passive);

            return result;
        }

        public static int IntField(int value, Vector2 position, float widthFieldArea)
        {
            Vector2 fieldSize = new Vector2(widthFieldArea, 18f);
            int result = EditorGUI.IntField(new Rect(position, fieldSize), value);
            UpdateLastGUIRect(new Rect(position, new Vector2(widthFieldArea, 18f)));

            EditorGUIUtility.GetControlID((int)(position.x + position.y), FocusType.Passive);

            return result;
        }

        public static string TextField(string name, string value, Vector2 position, float widthFieldArea)
        {
            Vector2 lableSize = new Vector2(widthFieldArea / 2, 18f);
            GUI.Label(new Rect(position, lableSize), name);

            Vector2 fieldPosition = position + Vector2.right * (widthFieldArea / 2);
            Vector2 fieldSize = new Vector2(widthFieldArea / 2, 18f);
            string result = EditorGUI.TextField(new Rect(fieldPosition, fieldSize), value);
            UpdateLastGUIRect(new Rect(position, new Vector2(widthFieldArea, 18f)));

            EditorGUIUtility.GetControlID((int)(position.x + position.y), FocusType.Passive);

            return result;
        }

        public static string TextField(string value, Vector2 position, float widthFieldArea)
        {
            Vector2 fieldSize = new Vector2(widthFieldArea, 18f);
            string result = EditorGUI.TextField(new Rect(position, fieldSize), value);
            UpdateLastGUIRect(new Rect(position, new Vector2(widthFieldArea, 18f)));

            EditorGUIUtility.GetControlID((int)(position.x + position.y), FocusType.Passive);

            return result;
        }

        public static string EnumField(string name, int value, Type enumType, Vector2 position, float widthFieldArea)
        {
            if (enumType.IsEnum == false)
                return string.Empty;

            string[] values = Enum.GetNames(enumType);
            int result = value;

            Vector2 lableSize = new Vector2(widthFieldArea / 2, 18f);
            GUI.Label(new Rect(position, lableSize), name);

            Vector2 fieldPosition = position + Vector2.right * (widthFieldArea / 2);
            Vector2 fieldSize = new Vector2(widthFieldArea / 2, 18f);
            result = EditorGUI.Popup(new Rect(fieldPosition, fieldSize), result, values);
            UpdateLastGUIRect(new Rect(position, new Vector2(widthFieldArea, 18f)));

            return values[result];
        } //Make varialtin without name arg. 

        public static bool Foldout(string name, bool foldDown, Vector2 position, float widthFieldArea)
        {
            float newWidthFieldArea = widthFieldArea - (18f + 6f);
            bool resultFoldDown = foldDown;

            if (resultFoldDown == false)
                Image(new Rect(position, Vector2.one * 18f), EnigmaticStyles.foldoutButtonClose);
            else
                Image(new Rect(position, Vector2.one * 18f), EnigmaticStyles.foldoutButtonOpen);

            if (EnigmaticGUIUtility.OnClick(new Rect(position, new Vector2(widthFieldArea, 18)), 0))
                resultFoldDown = !resultFoldDown;

            Vector2 lablePostion = position + Vector2.right * (18f);
            Label(name, lablePostion);

            return resultFoldDown;
        }

        public static bool FoldoutArray(string name, bool foldDown, Vector2 position, float widthFieldArea, int count, out int outCount)
        {
            float newWidthFieldArea = widthFieldArea - 18f;
            bool resultFoldDown = foldDown;

            if (resultFoldDown == false)
                Image(new Rect(position, Vector2.one * 18f), EnigmaticStyles.foldoutButtonClose);
            else
                Image(new Rect(position, Vector2.one * 18f), EnigmaticStyles.foldoutButtonOpen);

            Vector2 lablePostion = position + Vector2.right * 18f;
            Label(name, lablePostion);

            Vector2 fieldPosition = lablePostion + Vector2.right * (newWidthFieldArea / 2);
            Vector2 fieldSize = new Vector2(newWidthFieldArea / 4, 18f);
            outCount = EditorGUI.IntField(new Rect(fieldPosition, fieldSize), count);

            if (GUI.Button(new Rect(fieldPosition + Vector2.right * (fieldSize.x + 3),
                new Vector2(16, 14)), "", EnigmaticStyles.addButton))
            {
                outCount += 1;
            }

            if (GUI.Button(new Rect(fieldPosition + Vector2.right * (fieldSize.x + 24),
                new Vector2(16, 14)), "", EnigmaticStyles.substractButton))
            {
                outCount -= 1;
            }

            outCount = Math.Clamp(outCount, 0, 1000);
            UpdateLastGUIRect(new Rect(position, new Vector2(widthFieldArea, 18f)));

            return resultFoldDown;
        }

        public static void PropertyField(ESerializedProperty property, Vector2 position, float widthFieldArea)
        {
            if (PropertyDrawerRegister.GetDrawer(property.Type) != null)
            {
                PropertyDrawer drawer = PropertyDrawerRegister.GetDrawer(property.Type);
                drawer.Draw(property, position, widthFieldArea);
            }
            else if (property.IsArray == true || property.IsList == true)
            {
                property.IsExpanded = FoldoutArray(property.Name, property.IsExpanded, position,
                    widthFieldArea, property.ChildPropertes.Length, out int count);

                if (count != property.ChildPropertes.Length)
                {
                    if (count > property.ChildPropertes.Length)
                    {
                        int delta = count - property.ChildPropertes.Length;

                        for (int i = 0; i < delta; i++)
                            property.AddChildProperty();
                    }
                    else
                    {
                        int delta = property.ChildPropertes.Length - count;

                        for (int i = 0; i < delta; i++)
                            property.RemoveChildProperty();
                    }
                }

                if (property.IsExpanded)
                {
                    for (int i = 0; i < property.ChildPropertes.Length; i++)
                    {
                        Vector2 newPosition = new Vector2(position.x + 18f, GetLastGUIRect().position.y + 24);
                        PropertyField(property.ChildPropertes[i], newPosition, widthFieldArea - 18f);
                    }
                }

                float width = widthFieldArea;
                float height = (GetLastGUIRect().position - position).y + 16;
                Vector2 size = new Vector2(width, height);

                UpdateLastGUIRect(new Rect(position, size));
            }
            else if (property.IsClass == true)
            {
                property.IsExpanded = Foldout(property.Name, property.IsExpanded, position, widthFieldArea);

                if (property.IsExpanded)
                {
                    for (int i = 0; i < property.ChildPropertes.Length; i++)
                    {
                        Vector2 newPosition = new Vector2(position.x + 18f, GetLastGUIRect().position.y + 24);
                        PropertyField(property.ChildPropertes[i], newPosition, widthFieldArea);
                    }
                }
            }
            else
            {
                object value = property.Value;

                if (property.Type == typeof(string))
                    value = TextField(property.Name, (string)property.Value, position, widthFieldArea);
                else if (property.Type == typeof(int))
                    value = IntField(property.Name, (int)property.Value, position, widthFieldArea);
                else if (property.Type == typeof(float))
                    value = FloatField(property.Name, (float)property.Value, position, widthFieldArea);

                property.Value = value;
            }

            property.ApplyValue();
        }

        public static void DrawProperties(ESerializedProperty[] propertes, float widthFieldArea)
        {
            Vector2 lastRectPosition = GetLastGUIRect().position;

            foreach (ESerializedProperty property in propertes)
            {
                Vector2 position = new Vector2(lastRectPosition.x, GetLastGUIRect().position.y + 24);
                PropertyField(property, position, widthFieldArea);
                property.ApplyValue();
            }
        }

        public static void DrawProperty(SerializedProperty property, bool drawChildren)
        {
            string lastPropertyPath = string.Empty;

            foreach (SerializedProperty p in property)
            {
                if (p.isArray == true && p.propertyType == SerializedPropertyType.Generic)
                {
                    EditorGUILayout.BeginHorizontal();
                    p.isExpanded = EditorGUILayout.Foldout(p.isExpanded, p.displayName);
                    EditorGUILayout.EndHorizontal();

                    if (p.isExpanded)
                    {
                        EditorGUI.indentLevel++;
                        DrawProperty(p, drawChildren);
                        EditorGUI.indentLevel--;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(lastPropertyPath) == false
                        && p.propertyPath.Contains(lastPropertyPath))
                        continue;

                    lastPropertyPath = p.propertyPath;
                    EditorGUILayout.PropertyField(p, drawChildren);
                }
            }
        }

        public static void DrawProperty(SerializedProperty property, Rect rect, bool drawChildren)
        {
            string lastPropertyPath = string.Empty;

            foreach (SerializedProperty p in property)
            {
                if (p.isArray == true && p.propertyType == SerializedPropertyType.Generic)
                {
                    EditorGUILayout.BeginHorizontal();
                    p.isExpanded = EditorGUI.Foldout(rect, p.isExpanded, p.displayName);
                    EditorGUILayout.EndHorizontal();

                    if (p.isExpanded)
                    {
                        EditorGUI.indentLevel++;
                        DrawProperty(p, new Rect(rect.x, rect.y + 24, rect.width, rect.height), drawChildren);
                        EditorGUI.indentLevel--;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(lastPropertyPath) == false
                        && p.propertyPath.Contains(lastPropertyPath))
                        continue;

                    lastPropertyPath = p.propertyPath;
                    EditorGUI.PropertyField(rect, p, drawChildren);
                }
            }
        }

        public static Vector2 ScrollView(Rect rect, Rect viewRect, Vector2 scrollPosition, bool alwaysShowHorizontal,
            bool alwaysShowVertical, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, GUIStyle background)
        {
            //GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, GUI.skin.scrollView

            EditorInput.UpdateInput();

            Vector2 result = scrollPosition;

            if (viewRect.Contains(EditorInput.GetMousePosition()))
            {
                if (EditorInput.GetButtonPress(KeyCode.LeftShift) && alwaysShowHorizontal)
                    result.x -= EditorInput.GetMouseScrollWheel() * 10;
                else if (alwaysShowVertical)
                    result.y -= EditorInput.GetMouseScrollWheel() * 10;

                //float clampWidth = Mathf.Clamp(rect.width / viewRect.width, 1, float.MaxValue);
                //float clampHeight = Mathf.Clamp(rect.height / viewRect.height, 1, float.MaxValue);

                float x = Mathf.Clamp(result.x, -rect.width, 0);
                float y = Mathf.Clamp(result.y, -(rect.height - viewRect.height), 0);

                result = new Vector2(x, y);
                //Debug.Log();
            }

            return result;
        }

        public static float VerticalScrollbar(Rect rect, float value, float size, float topValue, float bottomValue)
        {
            float result = GUI.VerticalScrollbar(rect, value, size, topValue, bottomValue);
            UpdateLastGUIRect(rect);

            return result;
        }

        public static float HorizontalScrollbar(Rect rect, float value, float size, float topValue, float bottomValue)
        {
            float result = GUI.HorizontalScrollbar(rect, value, size, topValue, bottomValue);
            UpdateLastGUIRect(rect);

            return result;
        }

        public static Rect GetFixedBox(Rect rect, int border)
        {
            RectOffset offset = new RectOffset(border, border, border, border);
            return GetFixedBox(rect, new RectOffset(border, border, border, border));
        }

        public static ProfilerMarker CalcualteFixedBox = new ProfilerMarker(ProfilerCategory.Render, "CalcualteFixedBox");

        public static Rect GetFixedBox(Rect rect, GUIStyle style)
        {
            CalcualteFixedBox.Begin();

            int left = style.border.left - style.padding.left;
            int right = style.border.right - style.padding.right;
            int top = style.border.top - style.padding.top;
            int bottom = style.border.bottom - style.padding.bottom;

            RectOffset offset = new RectOffset(left, right, top, bottom);

            CalcualteFixedBox.End();

            return GetFixedBox(rect, offset);
        }

        public static Rect GetFixedBox(Rect rect, RectOffset offset)
        {
            CalcualteFixedBox.Begin();

            Vector2 position = new Vector2(rect.x - offset.left, rect.y - offset.top);

            Vector2 size = new Vector2(rect.width + (offset.right + offset.left),
                rect.height + (offset.bottom + offset.top));

            CalcualteFixedBox.End();

            return new Rect(position, size);
        }

        public static Rect GetLastGUIRect() => sm_LastGUIRect;

        public static void UpdateLastGUIRect(Rect rect) => sm_LastGUIRect = rect;
    }
}

#endif