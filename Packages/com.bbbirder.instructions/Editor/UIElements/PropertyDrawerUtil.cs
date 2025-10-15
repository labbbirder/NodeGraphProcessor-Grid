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

public static class PropertyDrawerUtility
{
    private static bool IsOdinInstalled()
    {
#if ODIN_INSPECTOR
        return true;
#else
        return false;
#endif
    }

    public static bool IsTypeDrawingByOdin(System.Type unityObjectType)
    {
#if ODIN_INSPECTOR
        if (!InspectorConfig.Instance.EnableOdinInInspector) return false;

        var editorType = InspectorConfig.Instance.DrawingConfig.GetEditorType(unityObjectType);
        Debug.Log($"editor type for {unityObjectType} is {editorType}");

        return editorType == typeof(OdinEditor);
#else
        return false;
#endif
    }

    static Dictionary<Type, List<(Type drawerType, bool forChildren)>> s_propertyDrawers;
    static Dictionary<Type, Type> s_type2drawers = new();

    static void EnsurePropertyDrawersCacheBuilt()
    {
        if (s_propertyDrawers != null) return;

        s_propertyDrawers = new();
        var typeDrawers = TypeCache.GetTypesDerivedFrom(typeof(PropertyDrawer));

        foreach (var drawerType in typeDrawers)
        {
            var attrs = drawerType.GetCustomAttributes<CustomPropertyDrawer>();

            foreach (var attr in attrs)
            {
                var fiType = attr.GetType().GetField("m_Type", BindingFlags.Instance | BindingFlags.NonPublic);
                var fiUseForChildren = attr.GetType().GetField("m_UseForChildren", BindingFlags.Instance | BindingFlags.NonPublic);
                var pType = (Type)fiType.GetValue(attr);
                var pUseForChildren = (bool)fiUseForChildren.GetValue(attr);
                if (!s_propertyDrawers.TryGetValue(pType, out var results))
                {
                    s_propertyDrawers[pType] = results = new();
                }

                results.Add((drawerType, pUseForChildren));
            }
        }
    }

    static Type CalculatePropertyDrawerForType(Type type)
    {
        if (type == null) return null;
        if (s_propertyDrawers.TryGetValue(type, out var result))
        {
            return result[0].drawerType;
        }

        for (var baseType = type.BaseType; baseType != null; baseType = baseType.BaseType)
        {
            if (s_propertyDrawers.TryGetValue(baseType, out var res))
            {
                foreach (var (drawerType, forChildren) in res)
                {
                    if (forChildren)
                    {
                        return drawerType;
                    }
                }
            }
        }

        foreach (var itype in type.GetInterfaces())
        {
            if (s_propertyDrawers.TryGetValue(itype, out var res))
            {
                foreach (var (drawerType, forChildren) in res)
                {
                    if (forChildren)
                    {
                        return drawerType;
                    }
                }
            }
        }

        return null;
    }

    public static Type GetPropertyDrawerForType(Type type)
    {
        EnsurePropertyDrawersCacheBuilt();

        if (!s_type2drawers.TryGetValue(type, out var drawerType))
        {
            s_type2drawers[type] = drawerType = CalculatePropertyDrawerForType(type);
        }

        return drawerType;
    }

    /// <summary>
    /// 创建自定义 PropertyDrawer 元素
    /// </summary>
    private static VisualElement CreateCustomPropertyDrawerElement(SerializedProperty prop)
    {
        Debug.Log($"draw {prop.type} by custom");
        // 使用 PropertyField 会自动应用 CustomPropertyDrawer
        PropertyField propertyField = new PropertyField(prop);

        // 对于有子属性的情况，确保展开
        if (prop.hasChildren && prop.isExpanded)
        {
            propertyField.Bind(prop.serializedObject);
        }

        return propertyField;
    }

    /// <summary>
    /// 创建默认属性元素
    /// </summary>
    private static VisualElement CreateDefaultPropertyElement(SerializedProperty prop)
    {
        VisualElement container = new VisualElement();

        if (prop.hasChildren && prop.propertyType != SerializedPropertyType.String)
        {
            PropertyField propertyField = new PropertyField(prop);
            container.Add(propertyField);
        }
        else
        {
            IMGUIContainer imguiContainer = new IMGUIContainer(() =>
            {
                EditorGUI.BeginChangeCheck();
                SerializedProperty localProp = prop.Copy();

                float height = EditorGUI.GetPropertyHeight(localProp, true);
                Rect position = GUILayoutUtility.GetRect(0, height, GUILayout.ExpandWidth(true));

                EditorGUI.PropertyField(position, localProp, true);

                if (EditorGUI.EndChangeCheck())
                {
                    prop.serializedObject.ApplyModifiedProperties();
                }
            });

            container.Add(imguiContainer);
        }

        return container;
    }

    /// <summary>
    /// 获取序列化属性对应的目标对象
    /// </summary>
    private static object GetTargetObject(SerializedProperty prop)
    {
        object obj = prop.serializedObject.targetObject;
        string path = prop.propertyPath;

        // 处理嵌套路径
        string[] pathParts = path.Split('.');
        object current = obj;

        foreach (string part in pathParts)
        {
            if (part == "Array")
            {
                // 跳过数组标识
                continue;
            }
            else if (part.StartsWith("data["))
            {
                // 处理数组元素
                if (current is System.Collections.IList list)
                {
                    int index = int.Parse(part[5..^1]);
                    if (index >= 0 && index < list.Count)
                    {
                        current = list[index];
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            else
            {
                // 处理普通字段
                System.Type type = current.GetType();
                FieldInfo field = type.GetField(part, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    current = field.GetValue(current);
                }
                else
                {
                    return null;
                }
            }

            if (current is null) return null;
        }

        return current;
    }
}
