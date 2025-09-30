using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BBBirder
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {


        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return new PropertyField(property)
            {
                style =
                {
                    opacity = 0.3f
                }
            };
        }
    }
}
