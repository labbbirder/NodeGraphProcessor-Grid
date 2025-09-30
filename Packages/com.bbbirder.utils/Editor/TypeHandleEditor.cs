using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace BBBirder
{
    [CustomPropertyDrawer(typeof(TypeHandle))]
    public class TypeHandleEditor : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var propAQN = property.FindPropertyRelative(nameof(TypeHandle.AQN));
            var attrBaseType = this.fieldInfo.GetCustomAttribute<TypeHandleFilterAttribute>(false);
            if (attrBaseType == null)
            {
                return new Label("error: no TypeHandleFilterAttribute provided.");
            }

            var curType = Type.GetType(propAQN.stringValue, false);
            var subTypes = TypeCache.GetTypesDerivedFrom(attrBaseType.BaseType);
            var index = subTypes.IndexOf(curType);
            var dropdown = new DropdownField(
                subTypes.Select(t => t?.AssemblyQualifiedName).Prepend(null).ToList(),
                index + 1,
                selectedName => string.IsNullOrEmpty(selectedName) ? "<null>" : Type.GetType(selectedName).FullName,
                selectedName => string.IsNullOrEmpty(selectedName) ? "<null>" : Type.GetType(selectedName).FullName);
            dropdown.BindProperty(propAQN);
            dropdown.label = property.name;
            return dropdown;
        }
    }
}
