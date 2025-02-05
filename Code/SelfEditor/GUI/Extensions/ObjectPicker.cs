using System;

using UnityEngine;
using UnityEditor;

using Random = UnityEngine.Random;

namespace Enigmatic.Core.Editor
{
    public class ObjectPicker<T> where T : UnityEngine.Object
    {
        private int m_ControlID;

        public Action<T> OnPick;

        public ObjectPicker()
        {
            m_ControlID = Random.Range(int.MinValue, int.MaxValue);
        }

        public void Show(T selectedObject, bool allowSceneObjects, string filter)
        {
            EditorGUIUtility.ShowObjectPicker<T>(selectedObject,
                allowSceneObjects, filter, m_ControlID);
        }

        public void Update()
        {
            if (EditorGUIUtility.GetObjectPickerControlID() != m_ControlID)
                return;

            if (Event.current.type == EventType.ExecuteCommand
                && Event.current.commandName == "ObjectSelectorUpdated")
            {
                T result = EditorGUIUtility.GetObjectPickerObject() as T;
                OnPick?.Invoke(result);
            }
        }
    }
}
