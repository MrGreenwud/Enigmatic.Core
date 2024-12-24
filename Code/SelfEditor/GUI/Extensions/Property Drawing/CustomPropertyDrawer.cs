#if UNITY_EDITOR

using System;

namespace Enigmatic.Core.Editor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CustomPropertyDrawer : Attribute
    {
        public readonly Type PropertyType;

        public CustomPropertyDrawer(Type propertyType)
        {
            PropertyType = propertyType;
        }
    }
}

#endif