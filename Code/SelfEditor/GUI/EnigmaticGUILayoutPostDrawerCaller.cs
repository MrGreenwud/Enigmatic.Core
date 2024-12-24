#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace Enigmatic.Core.Editor
{
    public static class EnigmaticGUILayoutPostDrawerCaller
    {
        public static PostActionCaller Lable(string text, GUIElement element, GUIStyle style = null, float width = -1)
        {
            Action<object[]> action = GUIElementPostDrawer.Lable;
            return PostActionCallerPool.GetCaller(action, text, element.Rect.position, width, style);
        }

        public static PostActionCaller Image(GUIElement element, GUIStyle style = null)
        {
            Action<object[]> action = GUIElementPostDrawer.Image;
            return PostActionCallerPool.GetCaller(action, element.Rect, style);
        }

        public static PostActionCaller Image(GUIElement element, Texture texture)
        {
            Action<object[]> action = GUIElementPostDrawer.ImageTexture;
            return PostActionCallerPool.GetCaller(action, element.Rect, texture);
        }

        public static PostActionCaller Rect(GUIElement element, Color color)
        {
            Action<object[]> action = GUIElementPostDrawer.Rect;
            return PostActionCallerPool.GetCaller(action, element.Rect, color);
        }

        public static PostActionCaller Button(GUIElement element, string text = "", GUIStyle style = null)
        {
            Action<object[]> action = GUIElementPostDrawer.Button;
            return PostActionCallerPool.GetCaller(action, element.Rect, text, style);
        }

        public static PostActionCaller FloatField(GUIElement element, int guid, float value, string name = "")
        {
            Action<object[]> action = GUIElementPostDrawer.FloatField;
            return PostActionCallerPool.GetCaller(action, element.Rect, guid, value, name);
        }

        public static PostActionCaller IntField(GUIElement element, int guid, int value, string name = "")
        {
            Action<object[]> action = GUIElementPostDrawer.IntField;
            return PostActionCallerPool.GetCaller(action, element.Rect, guid, value, name);
        }

        public static PostActionCaller TextField(GUIElement element, int guid, string value, string name = "")
        {
            Action<object[]> action = GUIElementPostDrawer.TextField;
            return PostActionCallerPool.GetCaller(action, element.Rect, guid, value, name);
        }

        public static PostActionCaller Grup(GUIGrup grup, int border = 0)
        {
            Action<object[]> action = GUIElementPostDrawer.Grup;
            return PostActionCallerPool.GetCaller(action, grup, border);
        }

        public static PostActionCaller PropertyField(SerializedProperty property, GUIElement element, string fieldName = "")
        {
            Action<object[]> action = GUIElementPostDrawer.PropertyField;
            return PostActionCallerPool.GetCaller(action, property, element.Rect, fieldName);
        }

        public static PostActionCaller ObjectField(GUIElement element, float fieldWidth,
            string fieldName, UnityEngine.Object obj, Type type, int guid)
        {
            Action<object[]> action = GUIElementPostDrawer.ObjectField;
            return PostActionCallerPool.GetCaller(action, element.Rect.position, fieldWidth, fieldName, obj, type, guid);
        }

        public static PostActionCaller ColorField(GUIElement element, float fieldWidth,
            string fieldName, Color value, int guid)
        {
            Action<object[]> action = GUIElementPostDrawer.ColorField;
            return PostActionCallerPool.GetCaller(action, element.Rect.position, fieldWidth, fieldName, value, guid);
        }

        public static PostActionCaller EnumPopup(GUIElement element, float fieldWidth,
            string fieldName, Enum selected, int guid)
        {
            Action<object[]> action = GUIElementPostDrawer.EnumPopup;
            return PostActionCallerPool.GetCaller(action, element.Rect.position, fieldWidth, fieldName, selected, guid);
        }

        public static PostActionCaller PropertyField(ESerializedProperty property, Rect rect)
        {
            Action<object[]> action = GUIElementPostDrawer.EPropertyField;
            return PostActionCallerPool.GetCaller(action, property, rect);
        }

        public static PostActionCaller Foldout(Rect rect, bool isExpanded, string displayName)
        {
            Action<object[]> action = GUIElementPostDrawer.Foldout;
            return PostActionCallerPool.GetCaller(action, rect, isExpanded, displayName);
        }
    }
}

#endif