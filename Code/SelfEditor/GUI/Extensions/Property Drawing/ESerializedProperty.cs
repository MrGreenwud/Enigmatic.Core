#if UNITY_EDITOR

using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using System.Linq;
using ENIX;

namespace Enigmatic.Core.Editor
{
    public class ESerializedProperty
    {
        public bool IsExpanded;

        public object Value;
        private List<ESerializedProperty> m_ChildPropertes = new List<ESerializedProperty>();

        public object Instance { get; private set; }
        public FieldInfo FieldInfo { get; private set; }

        public string Name { get; private set; }
        public Type Type { get; private set; }
        public bool IsClass { get; private set; }
        public bool IsStruct { get; private set; }
        public bool IsGenericType { get; private set; }
        public bool IsList { get; private set; }
        public bool IsArray { get; private set; }
        public bool IsEnum { get; private set; }
        public bool IsArrayElement { get; private set; }

        public ESerializedProperty[] ChildPropertes => m_ChildPropertes.ToArray();

        public ESerializedProperty(FieldInfo fieldInfo, object instance)
        {
            FieldInfo = fieldInfo;
            Instance = instance;

            if (instance != null)
                Value = FieldInfo.GetValue(Instance);

            Type = FieldInfo.FieldType;
            Name = FieldInfo.Name;

            InitType();

            IsArrayElement = false;
            InitChildren();
        }

        public ESerializedProperty(string name, object instance, Type type, object value)
        {
            Instance = instance;

            Type = type;
            Name = name;

            InitType();

            IsArrayElement = true;
            Value = value;

            InitChildren();
        }

        public void AddChildProperty()
        {
            if (m_ChildPropertes.Count == 0)
                m_ChildPropertes.Add(CreateChildProperty());
            else
                m_ChildPropertes.Add(m_ChildPropertes.Last().Clone());
        }

        public void RemoveChildProperty()
        {
            m_ChildPropertes.Remove(m_ChildPropertes.Last());
        }

        private void InitType()
        {
            IsClass = Type.IsClass && Type != typeof(string) && IsGenericType == false && IsArray == false;
            IsStruct = Type.IsStruct();
            IsGenericType = Type.IsGenericType;
            IsList = Type.IsGenericType && Type.GetGenericTypeDefinition() == typeof(List<>);
            IsArray = Type.IsArray;
            IsEnum = Type.IsEnum;
        }

        public void InitChildren()
        {
            m_ChildPropertes.Clear();

            if (IsArray)
            {
                if (Value == null)
                    return;

                Array array = (Array)Value;
                Type elementType = Type.GetElementType();

                if (array == null)
                    return;

                for (int i = 0; i < array.Length; i++)
                {
                    ESerializedProperty property = new ESerializedProperty($"Element {i}", array, elementType, array.GetValue(i));
                    m_ChildPropertes.Add(property);
                }
            }
            else if (IsList)
            {
                if (Value == null)
                    return;

                IList list = Value as IList;
                Type elementType = Type.GetGenericArguments()[0];

                int iteration = 0;
                foreach (object element in list)
                {
                    ESerializedProperty property = new ESerializedProperty($"Element {iteration}", list, elementType, element);
                    m_ChildPropertes.Add(property);
                    iteration++;
                }
            }
            else if (IsClass || IsStruct)
            {
                FieldInfo[] fieldInfos = Type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                foreach (FieldInfo field in fieldInfos)
                {
                    SerializeField serializeField = field.GetCustomAttribute<SerializeField>();
                    ESerializedProperty property = new ESerializedProperty(field, Value);
                    m_ChildPropertes.Add(property);
                }
            }
        }

        public void ApplyValue()
        {
            if (IsArray == true)
            {
                List<ESerializedProperty> tempChildProperty = m_ChildPropertes.Clone();

                Type typeElement = Type.GetElementType();

                foreach (ESerializedProperty childProperty in m_ChildPropertes)
                {
                    if (childProperty.Type != typeElement)
                    {
                        tempChildProperty.Remove(childProperty);
                        continue;
                    }

                    childProperty.ApplyValue();
                }

                Array newArray = Array.CreateInstance(typeElement, tempChildProperty.Count);

                for (int i = 0; i < newArray.Length; i++)
                    newArray.SetValue(tempChildProperty[i].Value, i);

                Value = newArray;
            }
            else if (IsList == true)
            {
                IList list = (IList)Value;
                Type typeElement = Type.GetGenericArguments()[0];

                foreach (ESerializedProperty childProperty in m_ChildPropertes)
                {
                    if (childProperty.Type != typeElement)
                        continue;

                    childProperty.ApplyValue();
                    list.Add(childProperty.Value);
                }

                Value = list;
            }
            else if (IsClass == true || IsStruct == true)
            {
                foreach (ESerializedProperty childProperty in m_ChildPropertes)
                    childProperty.ApplyValue();
            }

            if (IsArrayElement == false && Instance != null)
                FieldInfo.SetValue(Instance, Value);
        }

        public ESerializedProperty Clone()
        {
            if (IsArrayElement == false)
                throw new InvalidOperationException();

            object value = null;

            if (IsClass == true)
                value = Activator.CreateInstance(Type);
            else
                value = Value;

            ESerializedProperty property = new ESerializedProperty($"Element {int.Parse(Name.Split(" ")[1]) + 1}", Instance, Type, value);
            return property;
        }

        private ESerializedProperty CreateChildProperty()
        {
            if (IsArray == false && IsList == false)
                throw new InvalidOperationException();

            object value = null;
            Type elementType = null;

            if (IsArray)
                elementType = Type.GetElementType();
            else
                elementType = Type.GetGenericArguments()[0];

            if (elementType == typeof(string))
                value = string.Empty;
            else
                value = Activator.CreateInstance(elementType);

            ESerializedProperty property = new ESerializedProperty("Element 0", Instance, elementType, value);
            return property;
        }
    }
}

#endif