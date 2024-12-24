#if UNITY_EDITOR

using System;
using System.Reflection;
using System.Collections.Generic;

namespace Enigmatic.Core.Editor
{
    public static class PropertyDrawerRegister
    {
        private static Dictionary<Type, PropertyDrawer> s_PropertyDrawers = new Dictionary<Type, PropertyDrawer>();

        private static void Init()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            foreach (Type type in assembly.GetTypes())
            {
                if (typeof(PropertyDrawer).IsAssignableFrom(type))
                    RegisterPropertyDrawer(type);
            }
        }

        private static void RegisterPropertyDrawer(Type drawerType)
        {
            CustomPropertyDrawer customPropertyDrawer = (CustomPropertyDrawer)Attribute.GetCustomAttribute
                (drawerType, typeof(CustomPropertyDrawer));

            if (customPropertyDrawer == null)
                return;

            PropertyDrawer propertyDrawer = Activator.CreateInstance(drawerType) as PropertyDrawer;
            s_PropertyDrawers.Add(customPropertyDrawer.PropertyType, propertyDrawer);
        }

        public static PropertyDrawer GetDrawer(Type type)
        {
            if (s_PropertyDrawers.ContainsKey(type) == false)
                return null;

            return s_PropertyDrawers[type];
        }
    }
}

#endif