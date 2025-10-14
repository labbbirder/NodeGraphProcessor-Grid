using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace GraphProcessor
{
    public static class RuntimeTypeCache
    {
        private static Dictionary<Type, FieldInfo[]> s_node2allInstanceFields = new();

        internal static FieldInfo[] GetNodeInstantceFieldInfos(Type nodeType)
        {
            if (!s_node2allInstanceFields.TryGetValue(nodeType, out var fieldInfos))
            {
                if (!nodeType.IsSubclassOf(typeof(BaseNode)))
                {
                    return null;
                }

                s_node2allInstanceFields[nodeType] = fieldInfos =
                    nodeType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    ;

                bool hasExecutionLink = false;
                foreach (var field in fieldInfos)
                {
                    if (field.FieldType == typeof(ExecutionLink))
                    {
                        hasExecutionLink = true;
                        break;
                    }
                }

                if (hasExecutionLink)
                {
                    var keys = fieldInfos.Select((e, i) =>
                    {
                        if (e.FieldType == typeof(ExecutionLink))
                        {
                            return i - fieldInfos.Length;
                        }
                        else
                        {
                            return i;
                        }
                    }).ToArray();
                    Array.Sort(keys, fieldInfos);
                }
            }

            return fieldInfos;
        }

        static Dictionary<Type, NodeInformation> s_node2nodeInformation = new();
        const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        internal static NodeInformation GetNodeInformation(Type nodeType)
        {

            var fields = GetNodeInstantceFieldInfos(nodeType);
            if (!s_node2nodeInformation.TryGetValue(nodeType, out var information))
            {
                var needsInspector = false;
                var ioFields = new Dictionary<string, NodeFieldInformation>();

                foreach (var field in fields)
                {
                    var inputAttribute = field.GetCustomAttribute<InputAttribute>();
                    var outputAttribute = field.GetCustomAttribute<OutputAttribute>();
                    var tooltipAttribute = field.GetCustomAttribute<TooltipAttribute>();
                    var showInInspector = field.GetCustomAttribute<ShowInInspector>();
                    var vertical = field.GetCustomAttribute<VerticalAttribute>();
                    bool unpack = false;
                    bool isHide = false;
                    bool input = false;
                    string name = field.Name;
                    string tooltip = null;

                    // bool isExecutionLink = field.FieldType == typeof(ExecutionLink);

                    if (showInInspector != null || inputAttribute != null || outputAttribute != null)
                        needsInspector = true;

                    if (inputAttribute == null && outputAttribute == null)
                        continue;

                    //check if field is a collection type
                    unpack = (inputAttribute != null) ? inputAttribute.Unpack : false;
                    isHide = (inputAttribute != null) ? inputAttribute.Hide : outputAttribute.hide;
                    input = inputAttribute != null;
                    tooltip = tooltipAttribute?.tooltip;

                    if (!string.IsNullOrEmpty(inputAttribute?.Name))
                        name = inputAttribute.Name;
                    if (!string.IsNullOrEmpty(outputAttribute?.name))
                        name = outputAttribute.name;

                    // By default we set the behavior to null, if the field have a custom behavior, it will be set in the loop just below
                    ioFields[field.Name] = new NodeFieldInformation(field, name, input, unpack, tooltip, vertical != null)
                    {
                        hide = isHide
                    };
                }

                s_node2nodeInformation[nodeType] = information = new()
                {
                    needsInspector = needsInspector,
                    ioFields = ioFields,
                };
            }

            return information;
        }


        public static bool IsMethodOverrided(Type nodeType, string name, Type baseType)
        {
            return nodeType.GetMethod(name, InstanceFlags).DeclaringType != baseType;
        }

        static Dictionary<Type, CollectionMetatype> s_collectionMetatypes = new();

        internal static CollectionMetatype GetCollectionMetatype(Type type)
        {
            if (!s_collectionMetatypes.TryGetValue(type, out var metatype))
            {
                s_collectionMetatypes[type] = metatype = type.IsArray ? CollectionMetatype.Array
                    : HasImplement(type, typeof(IList<>)) ? CollectionMetatype.List
                    : HasImplement(type, typeof(ICollection<>)) ? CollectionMetatype.Collection
                    : CollectionMetatype.Element;
            }

            return metatype;

            static bool HasImplement(Type type, Type interfType)
            {
                foreach (var t in type.GetInterfaces())
                {
                    if (t.IsGenericType)
                    {
                        if (t.GetGenericTypeDefinition() == interfType)
                            return true;
                    }
                    else
                    {
                        if (t == interfType)
                            return true;
                    }
                }

                return false;
            }
        }

        static Dictionary<Type, Type> s_unpackedElementTypes = new();
        public static Type GetUnpackedElementType(Type type)
        {
            if (!s_unpackedElementTypes.TryGetValue(type, out var eleType))
            {
                s_unpackedElementTypes[type] = eleType = GetCollectionMetatype(type) switch
                {
                    CollectionMetatype.List => GetGenericArgumentOfInterface(type, typeof(IList<>)),
                    CollectionMetatype.Collection => GetGenericArgumentOfInterface(type, typeof(ICollection<>)),
                    CollectionMetatype.Array => type.GetElementType(),
                    _ => null,
                };
            }

            return eleType;

            static Type GetGenericArgumentOfInterface(Type type, Type interfType)
            {
                foreach (var t in type.GetInterfaces())
                {
                    if (t.IsGenericType)
                    {
                        if (t.GetGenericTypeDefinition() == interfType)
                            return t.GenericTypeArguments[0];
                    }
                }

                return null;
            }
        }

        internal class NodeInformation
        {
            public bool needsInspector;
            public Dictionary<string, NodeFieldInformation> ioFields;
        }

        internal class NodeFieldInformation
        {
            public string name;
            public string fieldName;
            public FieldInfo info;
            public bool input;
            public bool unpack;
            public bool hide;
            public string tooltip;
            // public CustomPortBehaviorDelegate behavior;
            public bool vertical;

            public NodeFieldInformation(FieldInfo info, string name, bool input, bool unpack, string tooltip, bool vertical)
            {
                this.input = input;
                this.unpack = unpack;
                this.info = info;
                this.name = name;
                this.fieldName = info.Name;
                // this.behavior = behavior;
                this.tooltip = tooltip;
                this.vertical = vertical;
            }
        }
    }

    internal enum CollectionMetatype
    {
        Element,    // not regarded as a collection
        List,       // implements IList`1, can perform partial pull
        Array,      // is T[], can perform partial pull
        Collection, // implements ICollection`1, can only perform entirely pull
    }

}

