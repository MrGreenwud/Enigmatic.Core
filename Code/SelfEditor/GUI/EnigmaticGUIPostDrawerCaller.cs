#if UNITY_EDITOR

using System;

using UnityEditor;
using UnityEngine;

namespace Enigmatic.Core.Editor
{
    internal static class EnigmaticGUIPostDrawerCaller
    {
        public static DeferredActionCaller Label(string text, Vector2 position, GUIStyle style = null, float width = -1)
        {
            Action<object[]> action = GUIElementPostDrawer.Label;
            return PostActionCallerPool.GetCaller(action, text, position, width, style);
        }

        public static DeferredActionCaller Image(Rect rect, GUIStyle style = null)
        {
            Action<object[]> action = GUIElementPostDrawer.Image;
            return PostActionCallerPool.GetCaller(action, rect, style);
        }
        
        public static DeferredActionCaller Image(Rect rect, Texture texture)
        {
            Action<object[]> action = GUIElementPostDrawer.ImageTexture;
            return PostActionCallerPool.GetCaller(action, rect, texture);
        }

        public static DeferredActionCaller Rect(Rect rect, Color color)
        {
            Action<object[]> action = GUIElementPostDrawer.Rect;
            return PostActionCallerPool.GetCaller(action, rect, color);
        }

        public static DeferredActionCaller Button(Rect rect, string text = "", GUIStyle style = null)
        {
            Action<object[]> action = GUIElementPostDrawer.Button;
            return PostActionCallerPool.GetCaller(action, rect, text, style);
        }

        public static DeferredActionCaller FloatField(Rect rect, int guid, float value, string name = "")
        {
            Action<object[]> action = GUIElementPostDrawer.FloatField;
            return PostActionCallerPool.GetCaller(action, rect, guid, value, name);
        }

        public static DeferredActionCaller IntField(Rect rect, int guid, int value, string name = "")
        {
            Action<object[]> action = GUIElementPostDrawer.IntField;
            return PostActionCallerPool.GetCaller(action, rect, guid, value, name);
        }

        public static DeferredActionCaller TextField(Rect rect, int guid, string value, string name = "")
        {
            Action<object[]> action = GUIElementPostDrawer.TextField;
            return PostActionCallerPool.GetCaller(action, rect, guid, value, name);
        }

        public static DeferredActionCaller PropertyField(SerializedProperty property, Rect rect, string fieldName = "")
        {
            Action<object[]> action = GUIElementPostDrawer.PropertyField;
            return PostActionCallerPool.GetCaller(action, property, rect, fieldName);
        }

        public static DeferredActionCaller ObjectField(Vector2 position, float fieldWidth,
            string fieldName, UnityEngine.Object obj, Type type, int guid)
        {
            Action<object[]> action = GUIElementPostDrawer.ObjectField;
            return PostActionCallerPool.GetCaller(action, position, fieldWidth, fieldName, obj, type, guid);
        }

        public static DeferredActionCaller ColorField(Vector2 position, float fieldWidth,
            string fieldName, Color value, int guid)
        {
            Action<object[]> action = GUIElementPostDrawer.ColorField;
            return PostActionCallerPool.GetCaller(action, position, fieldWidth, fieldName, value, guid);
        }

        public static DeferredActionCaller EnumPopup(Vector2 position, float fieldWidth,
            string fieldName, Enum selected, int guid)
        {
            Action<object[]> action = GUIElementPostDrawer.EnumPopup;
            return PostActionCallerPool.GetCaller(action, position, fieldWidth, fieldName, selected, guid);
        }

        public static DeferredActionCaller PropertyField(ESerializedProperty property, Rect rect)
        {
            Action<object[]> action = GUIElementPostDrawer.EPropertyField;
            return PostActionCallerPool.GetCaller(action, property, rect);
        } //Check

        public static DeferredActionCaller Foldout(Rect rect, bool isExpanded, string displayName)
        {
            Action<object[]> action = GUIElementPostDrawer.Foldout;
            return PostActionCallerPool.GetCaller(action, rect, isExpanded, displayName);
        } //Check
    }
}

#endif