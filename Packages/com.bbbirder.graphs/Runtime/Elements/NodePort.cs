// #define DEBUG_LAMBDA

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using BBBirder;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Profiling;

namespace GraphProcessor
{
    /// <summary>
    /// Class that describe port attributes for it's creation
    /// </summary>
    public class PortData : IEquatable<PortData>
    {
        /// <summary>
        /// Unique identifier for the port
        /// </summary>
        public string identifier;

        /// <summary>
        /// The index of partial port. -1 means solo port.
        /// </summary>
        public int partialIndex = -1;

        /// <summary>
        /// Display name on the node
        /// </summary>
        public string displayName;
        /// <summary>
        /// The type that will be used for coloring with the type stylesheet
        /// </summary>
        public Type displayType;
        /// <summary>
        /// If the port accept multiple connection
        /// </summary>
        public bool acceptMultipleEdges;
        /// <summary>
        /// Port size, will also affect the size of the connected edge
        /// </summary>
        public int sizeInPixel;
        /// <summary>
        /// Tooltip of the port
        /// </summary>
        public string tooltip;
        /// <summary>
        /// Is the port vertical
        /// </summary>
        public bool vertical;

        public bool Equals(PortData other)
        {
            return identifier == other.identifier
                && displayName == other.displayName
                && displayType == other.displayType
                && acceptMultipleEdges == other.acceptMultipleEdges
                && sizeInPixel == other.sizeInPixel
                && tooltip == other.tooltip
                && vertical == other.vertical;
        }

        public void CopyFrom(PortData other)
        {
            identifier = other.identifier;
            displayName = other.displayName;
            displayType = other.displayType;
            acceptMultipleEdges = other.acceptMultipleEdges;
            sizeInPixel = other.sizeInPixel;
            tooltip = other.tooltip;
            vertical = other.vertical;
        }
    }

    /// <summary>
    /// Runtime class that stores all info about one port that is needed for the processing
    /// </summary>
    public class NodePort
    {
        /// <summary>
        /// The actual name of the property behind the port (must be exact, it is used for Reflection)
        /// </summary>
        public string fieldName;
        /// <summary>
        /// The node on which the port is
        /// </summary>
        public BaseNode owner;
        /// <summary>
        /// The fieldInfo from the fieldName
        /// </summary>
        public FieldInfo fieldInfo;
        /// <summary>
        /// Data of the port
        /// </summary>
        public PortData portData;
        List<SerializableEdge> edges = new List<SerializableEdge>();

        // Comparison<SerializableEdge> horizontalEdgeSorter = HorizontalEdgeSorter;
        // Comparison<SerializableEdge> verticalEdgeSorter;
        // public void SortEdges()
        // { horizontalEdgeSorter ??= HorizontalEdgeSorter;
        //     if (portData.vertical)
        //     {
        //         edges.Sort(Comparison)
        //     }
        //     else
        //     {

        //     }
        // }

        // private int HorizontalEdgeSorter(SerializableEdge lhs, SerializableEdge rhs)
        // {
        //     if(lhs.ou)
        // }

        static Dictionary<(FieldInfo, Type), Delegate> s_typedGetters = new();
        private static Delegate GetNodeFieldGetter(FieldInfo fieldInfo, Type valueType)
        {
            const BindingFlags Flags = BindingFlags.Static | BindingFlags.NonPublic;
            var key = (fieldInfo, valueType);
            if (!s_typedGetters.TryGetValue(key, out var @delegate))
            {
                var methodInfo = fieldInfo.DeclaringType.GetMethod("get_" + fieldInfo.Name, Flags);
                var funcType = typeof(Func<,>).MakeGenericType(typeof(BaseNode), valueType);
                s_typedGetters[key] = @delegate = methodInfo.MakeGenericMethod(valueType).CreateDelegate(funcType);
            }

            return @delegate;
        }

        private static Func<BaseNode, T> GetNodeFieldGetter<T>(FieldInfo fieldInfo)
        {
            return GetNodeFieldGetter(fieldInfo, typeof(T)) as Func<BaseNode, T>;
        }

        static Dictionary<(FieldInfo, Type), Delegate> s_typedSetters = new();
        private static Delegate GetNodeFieldSetter(FieldInfo fieldInfo, Type valueType)
        {
            const BindingFlags Flags = BindingFlags.Static | BindingFlags.NonPublic;
            var key = (fieldInfo, valueType);
            if (!s_typedSetters.TryGetValue(key, out var @delegate))
            {
                var methodInfo = fieldInfo.DeclaringType.GetMethod("set_" + fieldInfo.Name, Flags);
                var funcType = typeof(Action<,>).MakeGenericType(typeof(BaseNode), valueType);
                s_typedSetters[key] = @delegate = methodInfo.CreateDelegate(funcType);
            }

            return @delegate;
        }

        private static Action<BaseNode, T> GetNodeFieldSetter<T>(FieldInfo fieldInfo)
        {
            return GetNodeFieldSetter(fieldInfo, typeof(T)) as Action<BaseNode, T>;
        }

        private enum CollectionMetatype
        {
            Element,    // not regarded as a collection
            List,       // implements IList`1, can perform partial pull
            Array,      // is T[], can perform partial pull
            Collection, // implements ICollection`1, can only perform entirely pull
        }

        // Action<NodePort> cachedPullMethod;
        // private Action<NodePort> GetPullMethod()
        // {
        //     var fieldType = fieldInfo.FieldType;
        //     var collectionMetatype = !portData.unpack ? CollectionMetatype.Element
        //         : fieldType.IsArray ? CollectionMetatype.Array
        //         : HasImplement(fieldType, typeof(IList<>)) ? CollectionMetatype.List
        //         : HasImplement(fieldType, typeof(ICollection<>)) ? CollectionMetatype.Collection
        //         : CollectionMetatype.Element;



        //     static bool HasImplement(Type type, Type interfType)
        //     {
        //         foreach (var t in type.GetInterfaces())
        //         {
        //             if (t.IsGenericType)
        //             {
        //                 if (t.GetGenericTypeDefinition() == interfType)
        //                     return true;
        //             }
        //             else
        //             {
        //                 if (t == interfType)
        //                     return true;
        //             }
        //         }

        //         return false;
        //     }
        // }

        private static Action<BaseNode> GenericPullFactory<TCollection, TElement>(NodePort inputPort, int edgeIndex = -1)
        {
            var edges = inputPort.edges;
            var fieldInfo = inputPort.fieldInfo;

            var cnt = edges.Count;
            var getters = edges
                .Select(e => (GetNodeFieldGetter<TElement>(e.outputPort.fieldInfo), e.outputNode))
                .ToArray();
            var setter = GetNodeFieldSetter<TCollection>(fieldInfo);
            if (typeof(IList<TElement>).IsAssignableFrom(typeof(TCollection)))
            {
                return (node) =>
                {
                    var list = fieldInfo.GetValue(node) as IList<TElement>;
                    if (list is null)
                    {
                        list = Activator.CreateInstance(fieldInfo.FieldType) as IList<TElement>;
                        fieldInfo.SetValue(node, list);
                    }

                    if (list.Count != cnt)
                    {
                        list.Clear();
                        for (int i = 0; i < cnt; i++)
                        {
                            list.Add(default);
                        }
                    }

                    if (edgeIndex == -1)
                    {
                        for (int i = 0; i < cnt; i++)
                        {
                            var (getter, n) = getters[i];
                            list[i] = getter(n);
                        }
                    }
                    else
                    {
                        var (getter, n) = getters[edgeIndex];
                        list[edgeIndex] = getter(n);
                    }
                };
            }
            else if (typeof(TCollection).IsArray)
            {
                return (node) =>
                {
                    var arr = fieldInfo.GetValue(node) as TElement[];
                    if (arr is null || arr.Length != cnt)
                    {
                        arr = new TElement[cnt];
                    }

                    if (edgeIndex == -1)
                    {
                        for (int i = 0; i < cnt; i++)
                        {
                            var (getter, n) = getters[i];
                            arr[i] = getter(n);
                        }
                    }
                    else
                    {
                        var (getter, n) = getters[edgeIndex];
                        arr[edgeIndex] = getter(n);
                    }
                };
            }
            else
            {
                throw new NotSupportedException($"type {typeof(TCollection)}");
            }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="owner">owner node</param>
        /// <param name="fieldOwner"></param>
        /// <param name="fieldName">the C# property name</param>
        /// <param name="portData">Data of the port</param>
        public NodePort(BaseNode owner, string fieldName, PortData portData)
        {
            this.fieldName = fieldName;
            this.owner = owner;
            this.portData = portData;

            fieldInfo = RuntimeTypeCache.GetNodeInformation(owner.GetType()).ioFields.GetValueOrDefault(fieldName)?.info;
            if (fieldInfo == null)
            {
                throw new($"no field named {fieldName} in node {owner}");
            }
        }

        /// <summary>
        /// Connect an edge to this port
        /// </summary>
        /// <param name="edge"></param>
        public void Add(SerializableEdge edge)
        {
            if (!edges.Contains(edge))
                edges.Add(edge);
        }

        /// <summary>
        /// Disconnect an Edge from this port
        /// </summary>
        /// <param name="edge"></param>
        public void Remove(SerializableEdge edge)
        {
            if (!edges.Contains(edge))
                return;

            edges.Remove(edge);
        }

        /// <summary>
        /// Get all the edges connected to this port
        /// </summary>
        /// <returns></returns>
        public List<SerializableEdge> GetEdges() => edges;

        /// <summary>
        /// Reset the value of the field to default if possible
        /// </summary>
        public void ResetToDefault()
        {
            // Clear lists, set classes to null and struct to default value.
            if (typeof(IList).IsAssignableFrom(fieldInfo.FieldType))
                (fieldInfo.GetValue(owner) as IList)?.Clear();
            else if (fieldInfo.FieldType.GetTypeInfo().IsClass)
                fieldInfo.SetValue(owner, null);
            else
            {
                try
                {
                    fieldInfo.SetValue(owner, Activator.CreateInstance(fieldInfo.FieldType));
                }
                catch { } // Catch types that don't have any constructors
            }
        }

        /// <summary>
        /// Pull values from the edge (in case of a custom convertion method)
        /// This method can only be called on input ports
        /// </summary>
        public void PullData()
        {
            // Only one input connection is handled by this code, if you want to
            // take multiple inputs, you must create a custom input function see CustomPortsNode.cs
            if (edges.Count > 0)
            {
                // if (!isArray)
                {
                    var edge = edges.First();
                    edge.TransferFunc.Invoke(edge.outputNode, edge.inputNode);
                }
                // else
                // {
                //     // init port array
                //     // fieldInfo.SetValue()
                //     foreach (var edge in edges)
                //     {

                //     }
                // }
                // var data = edge.outputPort.fieldInfo.GetValue(edge.outputNode);
                // // We do an extra convertion step in case the buffer output is not compatible with the input port
                // RuntimeConverter.TryConvert(data, fieldInfo.FieldType, out var convertedValue);
                // fieldInfo.SetValue(fieldOwner, convertedValue);
            }
        }
    }

    /// <summary>
    /// Container of ports and the edges connected to these ports
    /// </summary>
    public abstract class NodePortContainer : List<NodePort>
    {
        Dictionary<string, List<NodePort>> lut;
        protected BaseNode node;

        public NodePortContainer(BaseNode node)
        {
            this.node = node;
            lut = new();
        }

        /// <summary>
        /// Remove an edge that is connected to one of the node in the container
        /// </summary>
        /// <param name="edge"></param>
        public void Remove(SerializableEdge edge)
        {
            foreach (var p in this)
            {
                p.Remove(edge);
            }
        }

        /// <summary>
        /// Add an edge that is connected to one of the node in the container
        /// </summary>
        /// <param name="edge"></param>
        public void Add(SerializableEdge edge)
        {
            string portFieldName = (edge.inputNode == node) ? edge.inputFieldName : edge.outputFieldName;
            string portIdentifier = (edge.inputNode == node) ? edge.inputPortIdentifier : edge.outputPortIdentifier;

            // Force empty string to null since portIdentifier is a serialized value
            if (String.IsNullOrEmpty(portIdentifier))
                portIdentifier = null;

            var port = this.FirstOrDefault(p =>
            {
                return p.fieldName == portFieldName && p.portData.identifier == portIdentifier;
            });

            if (port == null)
            {
                Debug.LogError("The edge can't be properly connected because it's ports can't be found");
                return;
            }

            port.Add(edge);
        }

        public new void Add(NodePort port)
        {
            if (!lut.TryGetValue(port.fieldName, out var ports))
            {
                lut[port.fieldName] = ports = new();
            }

            ports.Add(port);

            base.Add(port);
        }

        public new bool Remove(NodePort port)
        {
            if (lut.TryGetValue(port.fieldName, out var ports))
            {
                ports.Remove(port);
            }

            return base.Remove(port);
        }

        public new void Clear()
        {
            lut.Clear();
            base.Clear();
        }

        public bool TryGetPort(string fieldName, out ReadOnlyList<NodePort> ports)
        {
            var found = lut.TryGetValue(fieldName, out var results);
            ports = found ? results.ToReadOnly() : default;
            return found;
        }
    }

    /// <inheritdoc/>
    public class NodeInputPortContainer : NodePortContainer
    {
        public NodeInputPortContainer(BaseNode node) : base(node) { }
    }

    /// <inheritdoc/>
    public class NodeOutputPortContainer : NodePortContainer
    {
        public NodeOutputPortContainer(BaseNode node) : base(node) { }
    }
}
