#if UNITY_EDITOR

namespace Enigmatic.Core.Editor
{
    public class EnigmaticGUILayoutOption
    {
        public enum TypeOption
        {
            SetPudding,
            SetElementSpacing,
            SetExpandWidth,
            SetExpandHeight,
            SetWidth,
            SetHeight,
            SetClickable
        }

        public TypeOption Type { get; private set; }
        public object Value { get; private set; }

        public EnigmaticGUILayoutOption(TypeOption type, object value)
        {
            Type = type;
            Value = value;
        }
    }
}

#endif