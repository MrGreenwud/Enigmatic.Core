#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace Enigmatic.Core.Editor
{
    internal static class EnigmaticGUILayoutPostDrawerCaller
    {
        public static DeferredActionCaller Label(string text, GUIElement element, GUIStyle style = null, float width = -1)
        {
            Action<object[]> action = GUIElementPostDrawer.Label;
            return PostActionCallerPool.GetCaller(action, text, element.Rect.position, width, style);
        }

        public static DeferredActionCaller Image(GUIElement element, GUIStyle style = null)
        {
            Action<object[]> action = GUIElementPostDrawer.Image;
            return PostActionCallerPool.GetCaller(action, element.Rect, style);
        }

        public static DeferredActionCaller Image(GUIElement element, Texture texture)
        {
            Action<object[]> action = GUIElementPostDrawer.ImageTexture;
            return PostActionCallerPool.GetCaller(action, element.Rect, texture);
        }

        public static DeferredActionCaller Rect(GUIElement element, Color color)
        {
            Action<object[]> action = GUIElementPostDrawer.Rect;
            return PostActionCallerPool.GetCaller(action, element.Rect, color);
        }

        public static DeferredActionCaller Button(GUIElement element, string text = "", GUIStyle style = null)
        {
            Action<object[]> action = GUIElementPostDrawer.Button;
            return PostActionCallerPool.GetCaller(action, element.Rect, text, style);
        }

        public static DeferredActionCaller FloatField(GUIElement element, int guid, float value, string name = "")
        {
            Action<object[]> action = GUIElementPostDrawer.FloatField;
            return PostActionCallerPool.GetCaller(action, element.Rect, guid, value, name);
        }

        public static DeferredActionCaller IntField(GUIElement element, int guid, int value, string name = "")
        {
            Action<object[]> action = GUIElementPostDrawer.IntField;
            return PostActionCallerPool.GetCaller(action, element.Rect, guid, value, name);
        }

        public static DeferredActionCaller TextField(GUIElement element, int guid, string value, string name = "")
        {
            Action<object[]> action = GUIElementPostDrawer.TextField;
            return PostActionCallerPool.GetCaller(action, element.Rect, guid, value, name);
        }

        public static DeferredActionCaller Group(GUIGroup group, int border = 0)
        {
            Action<object[]> action = GUIElementPostDrawer.Group;
            return PostActionCallerPool.GetCaller(action, group, border);
        }

        public static DeferredActionCaller PropertyField(SerializedProperty property, GUIElement element, string fieldName = "")
        {
            Action<object[]> action = GUIElementPostDrawer.PropertyField;
            return PostActionCallerPool.GetCaller(action, property, element.Rect, fieldName);
        }

        public static DeferredActionCaller ObjectField(GUIElement element, float fieldWidth,
            string fieldName, UnityEngine.Object obj, Type type, int guid)
        {
            Action<object[]> action = GUIElementPostDrawer.ObjectField;
            return PostActionCallerPool.GetCaller(action, element.Rect.position, fieldWidth, fieldName, obj, type, guid);
        }

        public static DeferredActionCaller ColorField(GUIElement element, float fieldWidth,
            string fieldName, Color value, int guid)
        {
            Action<object[]> action = GUIElementPostDrawer.ColorField;
            return PostActionCallerPool.GetCaller(action, element.Rect.position, fieldWidth, fieldName, value, guid);
        }

        public static DeferredActionCaller EnumPopup(GUIElement element, float fieldWidth,
            string fieldName, Enum selected, int guid)
        {
            Action<object[]> action = GUIElementPostDrawer.EnumPopup;
            return PostActionCallerPool.GetCaller(action, element.Rect.position, fieldWidth, fieldName, selected, guid);
        }

        public static DeferredActionCaller PropertyField(ESerializedProperty property, Rect rect)
        {
            Action<object[]> action = GUIElementPostDrawer.EPropertyField;
            return PostActionCallerPool.GetCaller(action, property, rect);
        }

        public static DeferredActionCaller Foldout(Rect rect, bool isExpanded, string displayName)
        {
            Action<object[]> action = GUIElementPostDrawer.Foldout;
            return PostActionCallerPool.GetCaller(action, rect, isExpanded, displayName);
        }
    }
}

#endif