using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace GraphProcessor
{
    internal static class RuntimeTypeCache
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
                            return i + fieldInfos.Length;
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

        public static Dictionary<Type, NodeInformation> s_node2nodeInformation = new();
        const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public static NodeInformation GetNodeInformation(Type nodeType)
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
                    bool isMultiple = false;
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
                    isMultiple = (inputAttribute != null) ? inputAttribute.allowMultiple : outputAttribute.allowMultiple;
                    isHide = (inputAttribute != null) ? inputAttribute.hide : outputAttribute.hide;
                    input = inputAttribute != null;
                    tooltip = tooltipAttribute?.tooltip;

                    if (!string.IsNullOrEmpty(inputAttribute?.name))
                        name = inputAttribute.name;
                    if (!string.IsNullOrEmpty(outputAttribute?.name))
                        name = outputAttribute.name;

                    // By default we set the behavior to null, if the field have a custom behavior, it will be set in the loop just below
                    ioFields[field.Name] = new NodeFieldInformation(field, name, input, isMultiple, tooltip, vertical != null)
                    {
                        hide = isHide
                    };
                }

                s_node2nodeInformation[nodeType] = information = new()
                {
                    hasCustomEnter = IsNodeMethodOverrided(nodeType, nameof(BaseNode.Enter)),
                    hasCustomAfterPullDatas = IsNodeMethodOverrided(nodeType, nameof(BaseNode.AfterPullDatas)),
                    hasCustomMoveNext = IsNodeMethodOverrided(nodeType, nameof(BaseNode.MoveNext)),
                    needsInspector = needsInspector,
                    ioFields = ioFields,
                };
            }

            return information;

            static bool IsNodeMethodOverrided(Type nodeType, string name)
            {
                return nodeType.GetMethod(name, InstanceFlags).DeclaringType != typeof(BaseNode);
            }
        }

        internal class NodeInformation
        {
            public bool needsInspector;
            public bool hasCustomEnter;
            public bool hasCustomMoveNext;
            public bool hasCustomAfterPullDatas;
            public Dictionary<string, NodeFieldInformation> ioFields;
        }

        internal class NodeFieldInformation
        {
            public string name;
            public string fieldName;
            public FieldInfo info;
            public bool input;
            public bool isMultiple;
            public bool hide;
            public string tooltip;
            // public CustomPortBehaviorDelegate behavior;
            public bool vertical;

            public NodeFieldInformation(FieldInfo info, string name, bool input, bool isMultiple, string tooltip, bool vertical)
            {
                this.input = input;
                this.isMultiple = isMultiple;
                this.info = info;
                this.name = name;
                this.fieldName = info.Name;
                // this.behavior = behavior;
                this.tooltip = tooltip;
                this.vertical = vertical;
            }
        }
    }
}

