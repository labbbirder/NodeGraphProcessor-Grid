using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BBBirder
{
    [CustomPropertyDrawer(typeof(OnChangeAttribute), true)]
    public class OnChangeDrawer : PropertyDrawer
    {
        PropertyField root;
        SerializedProperty property;
        OnChangeAttribute Attribute => attribute as OnChangeAttribute;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            this.property = property;
            this.root = new PropertyField(property);

            root.RegisterCallback<SerializedPropertyChangeEvent>(OnPropertyValueChanged);

            root.userData = property.boxedValue;
            return root;
        }

        static string GetParentPath(string unityPropertyPath)
        {

            if (unityPropertyPath.EndsWith(']'))
            {
                var i = unityPropertyPath.LastIndexOf(".Array.data[");
                return unityPropertyPath[..i];
            }
            else
            {
                var i = unityPropertyPath.LastIndexOf(".");
                if (i == -1) return "";

                return unityPropertyPath[..i];
            }
        }

        private object GetPropertyOwner()
        {
            var parentPropertyPath = GetParentPath(property.propertyPath);

            if (parentPropertyPath == "")
            {
                return property.serializedObject.targetObject;
            }
            else
            {
                while (property.serializedObject.FindProperty(parentPropertyPath).isArray)
                {
                    parentPropertyPath = GetParentPath(parentPropertyPath);
                    if (parentPropertyPath == "")
                    {
                        return property.serializedObject.targetObject;
                    }
                }

                return property.serializedObject.FindProperty(parentPropertyPath).boxedValue;
            }
        }

        void OnPropertyValueChanged(SerializedPropertyChangeEvent e)
        {
            EditorApplication.delayCall -= InvokeCallback;
            EditorApplication.delayCall += InvokeCallback;
        }

        void InvokeCallback()
        {
            if (root.panel == null) return;
            if (string.IsNullOrEmpty(root.bindingPath)) return;

            var target = GetPropertyOwner();
            var newValue = property.boxedValue;
            var prevValue = root.userData;
            root.userData = newValue;

            var method = target.GetType().GetMethod(Attribute.CallbackName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            if (method == null)
            {
                Debug.LogWarning($"method `{Attribute.CallbackName}` not found");
                return;
            }

            var parameters = method.GetParameters();
            var args = new List<object>();
            if (parameters.Length >= 1)
            {
                args.Add(newValue);
            }

            if (parameters.Length >= 2)
            {
                args.Add(prevValue);
            }


            if (parameters.Length >= 3)
            {
                Debug.LogWarning($"method {method.Name} has more than 2 parameters");
                return;
            }

            if (method.IsStatic)
            {
                method.Invoke(null, args.ToArray());
            }
            else
            {
                method.Invoke(target, args.ToArray());
            }
        }
    }
}
