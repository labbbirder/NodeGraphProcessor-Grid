#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BBBirder
{
    public static class EditorExtensions
    {
        public static System.Type GetTargetType(this SerializedObject obj)
        {
            if (obj == null) return null;

            if (obj.isEditingMultipleObjects)
            {
                var c = obj.targetObjects[0];
                return c.GetType();
            }
            else
            {
                return obj.targetObject.GetType();
            }
        }

        private static Type GetTypeFromUnityFullName(string unityFullName)
        {
            var fieldFullName = unityFullName.Split(" ");
            var assemblyName = fieldFullName[0];
            var fullName = fieldFullName[1].Replace('/', '+');

            var type = Type.GetType($"{fullName}, {assemblyName}");

            return type;
        }

        public static Type GetManagedReferenceFieldType(this SerializedProperty prop)
        {
            return GetTypeFromUnityFullName(prop.managedReferenceFieldTypename);
        }

        public static Type GetManagedReferenceValueType(this SerializedProperty prop)
        {
            return GetTypeFromUnityFullName(prop.managedReferenceFullTypename);
        }

        public static FieldInfo GetFieldOfProperty(this SerializedProperty prop)
        {
            if (prop == null) return null;

            var tp = GetTargetType(prop.serializedObject);
            var obj = prop.serializedObject;
            if (tp == null) return null;

            var path = prop.propertyPath.Replace(".Array.data[", "[");
            var elements = path.Split('.');
            FieldInfo field = null;
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element[..element.IndexOf("[")];
                    var index = System.Convert.ToInt32(element[element.IndexOf("[")..].Replace("[", "").Replace("]", ""));

                    field = tp.GetField(elementName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (field == null) return null;

                    if (field.FieldType.IsArray)
                    {
                        tp = field.FieldType.GetElementType();
                    }
                    else if (field.FieldType.IsGenericType)
                    {
                        tp = field.FieldType.GetGenericArguments()[0];
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    field = tp.GetField(element, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (field == null) return null;
                    tp = field.FieldType;
                }
            }

            return field;
        }

        /// <summary>
        /// Gets the object the property represents.
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static object GetValue(this SerializedProperty prop)
        {
            if (prop == null) return null;

            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element[..element.IndexOf("[")];
                    var index = System.Convert.ToInt32(element[element.IndexOf("[")..].Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }

            return obj;
        }

        private static object GetValue_Imp(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();

            while (type != null)
            {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null)
                    return f.GetValue(source);

                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null)
                    return p.GetValue(source, null);

                type = type.BaseType;
            }

            return null;
        }

        private static object GetValue_Imp(object source, string name, int index)
        {
            var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
            if (enumerable == null) return null;
            var enm = enumerable.GetEnumerator();
            //while (index-- >= 0)
            //    enm.MoveNext();
            //return enm.Current;

            for (int i = 0; i <= index; i++)
            {
                if (!enm.MoveNext()) return null;
            }

            return enm.Current;
        }

    }
}
#endif
