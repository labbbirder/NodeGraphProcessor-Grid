using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
#endif

namespace BBBirder
{
    [CustomPropertyDrawer(typeof(PolymorphicAttribute), false)]
    public class PolymorphicDrawer : PropertyDrawer
    {
        static Editor s_cachedEditor;
        private bool IsDrawnByOdin(SerializedProperty property)
        {
#if ODIN_INSPECTOR
            Editor.CreateCachedEditor(property.serializedObject.targetObjects, null, ref s_cachedEditor);
            if (s_cachedEditor != null && s_cachedEditor.GetType() == typeof(OdinEditor))
            {
                return true;
            }
#endif
            return false;
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var fieldFullName = property.managedReferenceFieldTypename.Split(" ");
            var assemblyName = fieldFullName[0];
            var fullName = fieldFullName[1].Replace('/', '+');

            var type = Type.GetType($"{fullName}, {assemblyName}");

            var subtypesBuilder = TypeCache.GetTypesDerivedFrom(type)
                .Prepend(type)
                .Where(t => !t.IsInterface)
                .Where(t => !t.IsAbstract)
                .Select(t => t.AssemblyQualifiedName)
                // .ToList()
                ;
            var allowNull = (attribute as PolymorphicAttribute)?.AllowNull ?? false;
            if (allowNull)
            {
                subtypesBuilder = subtypesBuilder.Prepend(null);
            }

            var subtypes = subtypesBuilder.ToList();
            var index = subtypes.IndexOf(property.managedReferenceValue?.GetType()?.AssemblyQualifiedName);

            var ui = new VisualElement();

            var contents = new PropertyField(property);
            ui.Add(contents);

            var dropdown = new DropdownField(subtypes, index, GetDisplayName, GetDisplayName);
            dropdown.RegisterValueChangedCallback(e =>
            {
                var newValue = string.IsNullOrEmpty(e.newValue) ? null : Activator.CreateInstance(Type.GetType(e.newValue));
                property.managedReferenceValue = newValue;
                property.serializedObject.ApplyModifiedProperties();
            });
            dropdown.style.position = Position.Absolute;
            dropdown.style.left = 120;
            dropdown.style.right = 0;
            ui.Add(dropdown);

            return ui;
        }

        public static string GetDisplayName(string aqn)
        {
            var type = string.IsNullOrEmpty(aqn) ? null : Type.GetType(aqn, false);
            var attr = type?.GetCustomAttribute<DisplayNameAttribute>();
            if (attr != null)
            {
                return attr.Name;
            }
            else
            {
                return type?.FullName ?? "<null>";
            }
        }

    }
}
