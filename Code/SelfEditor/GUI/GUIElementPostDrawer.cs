#if UNITY_EDITOR

using System;
using UnityEngine;

namespace Enigmatic.Core.Editor
{
    public static class GUIElementPostDrawer
    {
        /// <summary>
        /// 0 - text, 1 - position, 2 - width, 2 - style
        /// </summary>
        public static void Lable(object[] parameters)
        {
            if (parameters.Length < 4)
                return;

            string name = parameters[0].ToString();
            Vector2 position = (Vector2)parameters[1];
            float width = (float)parameters[2];
            GUIStyle style = parameters[3] as GUIStyle;

            EnigmaticGUI.Label(name, position, style, width);
        }

        /// <summary>
        /// 0 - rect, 1 - style
        /// </summary>
        public static void Image(object[] parameters)
        {
            if (parameters.Length < 2)
                return;

            Rect rect = (Rect)parameters[0];
            GUIStyle style = parameters[1] as GUIStyle;

            EnigmaticGUI.Image(rect, style);
        }

        /// <summary>
        /// 0 - rect, 1 - texture
        /// </summary>
        public static void ImageTexture(object[] parameters)
        {
            if (parameters.Length < 2)
                return;

            Rect rect = (Rect)parameters[0];
            Texture texture = parameters[1] as Texture;

            EnigmaticGUI.Image(rect, texture);
        }

        /// <summary>
        /// 0 - rect, 1 - color
        /// </summary>
        public static void Rect(object[] parameters)
        {
            if (parameters.Length < 2)
                return;

            Rect rect = (Rect)parameters[0];
            Color color = (Color)parameters[1];

            EnigmaticGUI.DrawRect(rect, color);
        }

        /// <summary>
        /// 0 - rect, 1 - text, 2 - style
        /// </summary>
        public static void Button(object[] parameters)
        {
            if (parameters.Length < 3)
                return;

            Rect rect = (Rect)parameters[0];
            string text = (string)parameters[1];
            GUIStyle style = parameters[2] as GUIStyle;

            EnigmaticGUI.Button(text, rect.position, rect.size, style);
        }

        /// <summary>
        /// 0 - rect, 1 - value, 2 - value guid, 3! - name 
        /// </summary>
        public static void FloatField(object[] parameters)
        {
            if (parameters.Length < 4)
                return;

            Rect rect = (Rect)parameters[0];
            int guid = (int)parameters[1];
            float value = (float)parameters[2];
            string name = (string)parameters[3];

            float result = value;

            if (string.IsNullOrEmpty(name) == false)
                result = EnigmaticGUI.FloatField(name, value, rect.position, rect.width);
            else
                result = EnigmaticGUI.FloatField(value, rect.position, rect.width);

            if (result != value)
                GUIValueCasher.CashValue(guid, result);
        }

        /// <summary>
        /// 0 - rect, 1 - value, 2 - value guid, 3! - name 
        /// </summary>
        public static void IntField(object[] parameters)
        {
            if (parameters.Length < 4)
                return;

            Rect rect = (Rect)parameters[0];
            int guid = (int)parameters[1];
            int value = (int)parameters[2];
            string name = (string)parameters[3];

            int result = value;

            if (string.IsNullOrEmpty(name))
                result = EnigmaticGUI.IntField(value, rect.position, rect.width);
            else
                result = EnigmaticGUI.IntField(name, value, rect.position, rect.width);

            if (result != value)
                GUIValueCasher.CashValue(guid, result);
        }

        /// <summary>
        /// 0 - rect, 1 - value, 2 - value guid, 3! - name 
        /// </summary>
        public static void TextField(object[] parameters)
        {
            if (parameters.Length < 4)
                throw new ArgumentException();

            Rect rect = (Rect)parameters[0];
            int guid = (int)parameters[1];

            string value = (string)parameters[2];
            string name = (string)parameters[3];

            string newValue = value;

            if (string.IsNullOrEmpty(name))
                newValue = EnigmaticGUI.TextField(newValue, rect.position, rect.width);
            else
                newValue = EnigmaticGUI.TextField(name, newValue, rect.position, rect.width);

            if (value != newValue)
                GUIValueCasher.CashValue(guid, newValue);
        }

        /// <summary>
        /// 0 - GUIGrup, 1 - border
        /// </summary>
        public static void Grup(object[] parameters)
        {
            if (parameters.Length < 2)
                return;

            GUIGrup grup = parameters[0] as GUIGrup;
            GUIStyle style = grup.Style;
            int border = (int)parameters[1];

            if (style != null)
                EnigmaticGUI.Image(EnigmaticGUI.GetFixedBox(grup.Rect, border), style);
        }

        /// <summary>
        /// 0 - SerializedProperty, 1 - Rect
        /// </summary>
        public static void PropertyField(object[] parameters)
        {
            if (parameters.Length < 3)
                return;

            UnityEditor.SerializedProperty property = (UnityEditor.SerializedProperty)parameters[0];
            Rect rect = (Rect)parameters[1];
            string fieldName = (string)parameters[2];

            EnigmaticGUI.PropertyField(rect.position, property, rect.width, fieldName);
        }

        /// <summary>
        /// 0 - position, 1 - fieldWidth,  2 - fieldName, 3- obj, 4 - objectType, 5 - guid
        /// </summary>
        public static void ObjectField(object[] parameters)
        {
            if (parameters.Length < 6)
                throw new ArgumentException();

            Vector2 position = (Vector2)parameters[0];
            float fieldWidth = (float)parameters[1];
            string fieldName = (string)parameters[2];
            UnityEngine.Object obj = (UnityEngine.Object)parameters[3];
            Type objectType = (Type)parameters[4];
            int guid = (int)parameters[5];

            UnityEngine.Object value = EnigmaticGUI.ObjectField(position, fieldWidth, fieldName, obj, objectType);

            if (obj != value)
                GUIValueCasher.CashValue(guid, value);
        }

        /// <summary>
        /// 0 - position, 1 - fieldWidth,  2 - fieldName, 3 - value, 4 - guid
        /// </summary>
        public static void ColorField(object[] parameters)
        {
            if (parameters.Length < 5)
                throw new ArgumentException();

            Vector2 position = (Vector2)parameters[0];
            float fieldWidth = (float)parameters[1];
            string fieldName = (string)parameters[2];
            Color value = (Color)parameters[3];
            int guid = (int)parameters[4];

            Color result = EnigmaticGUI.ColorField(position, fieldWidth, fieldName, value);

            if (result != value)
                GUIValueCasher.CashValue(guid, result);
        }

        /// <summary>
        /// 0 - position, 1 - fieldWidth,  2 - fieldName, 3 - selected, 4 - guid
        /// </summary>
        public static void EnumPopup(object[] parameters)
        {
            if (parameters.Length < 5)
                throw new ArgumentException();

            Vector2 position = (Vector2)parameters[0];
            float fieldWidth = (float)parameters[1];
            string fieldName = (string)parameters[2];
            Enum selected = (Enum)parameters[3];
            int guid = (int)parameters[4];

            Enum result = EnigmaticGUI.EnumPopup(position, fieldWidth, fieldName, selected);

            if (result != selected)
                GUIValueCasher.CashValue(guid, result);
        }

        /// <summary>
        /// 0 - SerializedProperty, 1 - Rect
        /// </summary>
        public static void EPropertyField(object[] parameters)
        {
            if (parameters.Length < 2)
                return;

            ESerializedProperty property = (ESerializedProperty)parameters[0];
            Rect rect = (Rect)parameters[1];

            EnigmaticGUI.PropertyField(property, rect.position, rect.width);
        }

        /// <summary>
        /// 0 - Rect, 1 - IsExpanded, 2 - displayName
        /// </summary>
        public static void Foldout(object[] parameters)
        {
            if (parameters.Length < 3)
                return;

            Rect rect = (Rect)parameters[0];
            bool isExpanded = (bool)parameters[1];
            string displayName = (string)parameters[2];

            EnigmaticGUI.Foldout(rect.position, isExpanded, displayName, rect.width);
        }
    }
}

#endif