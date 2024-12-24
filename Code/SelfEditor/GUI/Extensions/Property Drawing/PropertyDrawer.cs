#if UNITY_EDITOR

using System.Reflection;
using UnityEngine;

namespace Enigmatic.Core.Editor
{
    public class PropertyDrawer
    {
        public virtual void Draw(object instance, FieldInfo fieldInfo, Vector2 position, float widthFieldArea) { }
        public virtual void Draw(ESerializedProperty property, Vector2 position, float widthFieldArea) { }
    }
}

#endif