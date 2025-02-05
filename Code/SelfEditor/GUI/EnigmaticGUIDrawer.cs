#if UNITY_EDITOR

using System;

using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

namespace Enigmatic.Core.Editor
{
    public static class EnigmaticGUIDrawer
    {
        private static bool sm_IsClick;

        public static void Label(string text, Vector2 position, GUIStyle style = null, float width = -1)
        {
            DeferredActionCaller caller = EnigmaticGUIPostDrawerCaller.Label(text, position, style, width);
            GUIPostDrawQueue.Enqueue(caller);
        }

        public static void Image(Rect rect, GUIStyle style = null)
        {
            DeferredActionCaller caller = EnigmaticGUIPostDrawerCaller.Image(rect, style);
            GUIPostDrawQueue.Enqueue(caller);
        }

        public static void Image(Rect rect, Texture texture)
        {
            DeferredActionCaller caller = EnigmaticGUIPostDrawerCaller.Image(rect, texture);
            GUIPostDrawQueue.Enqueue(caller);
        }

        public static void Rect(Rect rect, Color color)
        {
            DeferredActionCaller caller = EnigmaticGUIPostDrawerCaller.Rect(rect, color);
            GUIPostDrawQueue.Enqueue(caller);
        }

        public static bool Button(Rect rect, string text = "", GUIStyle style = null)
        {
            DeferredActionCaller caller = EnigmaticGUIPostDrawerCaller.Button(rect, text, style);
            GUIPostDrawQueue.Enqueue(caller);

            return sm_IsClick == false && EnigmaticGUIUtility.OnClick(rect, 0) ? sm_IsClick = true : false;
        }

        public static float FloatField(Rect rect, float value, string name = "")
        {
            int guid = GUIgUID.Next(name, rect, value.ToString());

            DeferredActionCaller caller = EnigmaticGUIPostDrawerCaller.FloatField(rect, guid, value, name);
            GUIPostDrawQueue.Enqueue(caller);

            return GUIValueCasher.TryGetValue(guid, out object result) ? (float)result : value;
        }

        public static int  IntField(Rect rect, int value, string name = "")
        {
            int guid = GUIgUID.Next(name, rect, value.ToString());

            DeferredActionCaller caller = EnigmaticGUIPostDrawerCaller.IntField(rect, guid, value, name);
            GUIPostDrawQueue.Enqueue(caller);

            return GUIValueCasher.TryGetValue(guid, out object result) ? (int)result : value;
        }

        public static string TextField(Rect rect, string value, string name = "")
        {
            int guid = GUIgUID.Next(name, rect, value);

            DeferredActionCaller caller = EnigmaticGUIPostDrawerCaller.TextField(rect, guid, value, name);
            GUIPostDrawQueue.Enqueue(caller);

            return GUIValueCasher.TryGetValue(guid, out object result) ? (string)result : value;
        }

        public static void PropertyField(SerializedProperty property, Rect rect, string fieldName = "")
        {
            DeferredActionCaller caller = EnigmaticGUIPostDrawerCaller.PropertyField(property, rect, fieldName);
            GUIPostDrawQueue.Enqueue(caller);
        }

        public static UnityEngine.Object ObjectField(Vector2 position, float fieldWidth, string fieldName, UnityEngine.Object obj, Type type)
        {
            int guid = GUIgUID.Next(fieldName, new Rect(position, new Vector2(fieldWidth, 20)), 0);

            DeferredActionCaller caller = EnigmaticGUIPostDrawerCaller.ObjectField(position, fieldWidth, fieldName, obj, type, guid);
            GUIPostDrawQueue.Enqueue(caller);

            return GUIValueCasher.TryGetValue(guid, out object result) ? (UnityEngine.Object)result : obj;
        }

        public static Color ColorField(Vector2 position, float fieldWidth, string fieldName, Color value)
        {
            int guid = GUIgUID.Next(fieldName, new Rect(position, new Vector2(fieldWidth, 20)), value);

            DeferredActionCaller caller = EnigmaticGUIPostDrawerCaller.ColorField(position, fieldWidth, fieldName, value, guid);
            GUIPostDrawQueue.Enqueue(caller);

            return GUIValueCasher.TryGetValue(guid, out object result) ? (Color)result : value;
        }

        public static Enum EnumPopup(Vector2 position, float fieldWidth, string fieldName, Enum selected)
        {
            int guid = GUIgUID.Next(fieldName, new Rect(position, new Vector2(fieldWidth, 20)), selected);

            DeferredActionCaller caller = EnigmaticGUIPostDrawerCaller.EnumPopup(position, fieldWidth, fieldName, selected, guid);
            GUIPostDrawQueue.Enqueue(caller);

            return GUIValueCasher.TryGetValue(guid, out object result) ? (Enum)result : selected;
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