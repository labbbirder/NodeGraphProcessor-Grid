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

        // /// <summary>
        // /// The index of partial port. -1 means solo port.
        // /// </summary>
        // public int partialIndex = -1;
        public bool sort;

        public bool unpack;

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
                && unpack == other.unpack
                && sort == other.sort
                // && partialIndex == other.partialIndex
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
            unpack = other.unpack;
            sort = other.sort;
            // partialIndex = other.partialIndex;
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


        Action<NodePort, int> packTransfer;
        Action<SerializableEdge> soloTransfer;

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

        public void ClearTransferCache()
        {
            soloTransfer = null;
            packTransfer = null;
        }

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

        private Action<SerializableEdge> CreateSoloTransfer()
        {
            var edge = edges[0];
            var key = (fieldInfo, edge.outputPort.fieldInfo);
            if (!s_dataTransfers.TryGetValue(key, out var transFunc))
            {
                s_miSoloTransferHelper ??= GetType().GetMethod(nameof(SoloTransferHelper), BindingFlags.Static | BindingFlags.NonPublic);
                s_dataTransfers[key] = transFunc = s_miSoloTransferHelper
                    .MakeGenericMethod(fieldInfo.FieldType)
                    .Invoke(null, new object[] { edge }) as Action<SerializableEdge>;
            }

            return transFunc;
        }

        private Action<NodePort, int> CreatePackTransfer()
        {
            s_miPackTransferHelper ??= GetType().GetMethod(nameof(PackTransferHelper), BindingFlags.Static | BindingFlags.NonPublic);
            var targetType = fieldInfo.FieldType;
            var collectionMetatype = RuntimeTypeCache.GetCollectionMetatype(targetType);
            if (collectionMetatype is CollectionMetatype.Element)
            {
                throw new($"Type {targetType} is not unpackable.");
            }

            var eleType = RuntimeTypeCache.GetUnpackedElementType(targetType);
            return s_miPackTransferHelper
                .MakeGenericMethod(targetType, eleType)
                .Invoke(null, new object[] { this, collectionMetatype }) as Action<NodePort, int>;
        }

        static MethodInfo s_miSoloTransferHelper;
        static Dictionary<(FieldInfo, FieldInfo), Action<SerializableEdge>> s_dataTransfers = new(new FieldPairComparer());
        private static Action<SerializableEdge> SoloTransferHelper<TTo>(SerializableEdge edge)
        {
            var getter = GetNodeFieldGetter<TTo>(edge.outputPort.fieldInfo);
            var setter = GetNodeFieldSetter<TTo>(edge.inputPort.fieldInfo);
            return (e) =>
            {
                // Assert: e == edge
                var value = getter(e.outputNode);
                setter(e.inputNode, value);
            };
        }

        static MethodInfo s_miPackTransferHelper;
        private static Action<NodePort, int> PackTransferHelper<TCollection, TElement>(NodePort inputPort, CollectionMetatype collectionMetatype) where TCollection : class
        {
            var edges = inputPort.edges;
            var fieldInfo = inputPort.fieldInfo;

            // var getters = edges
            //     .Select(e => (GetNodeFieldGetter<TElement>(e.outputPort.fieldInfo), e.outputNode))
            //     .ToArray();

#warning TODO: expose getter as well.
            var setter = GetNodeFieldSetter<TCollection>(fieldInfo);
            if (collectionMetatype is CollectionMetatype.List)
            {
                return (port, edgeIndex) =>
                {
                    var cnt = edges.Count;
                    var node = port.owner;

                    var list = fieldInfo.GetValue(node) as IList<TElement>;
                    if (list is null)
                    {
                        list = Activator.CreateInstance(fieldInfo.FieldType) as IList<TElement>;
                        setter(node, list as TCollection);
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
                            var edge = edges[i];
                            list[i] = PullFromEdge(edge);
                        }
                    }
                    else
                    {
                        var edge = edges[edgeIndex];
                        list[edgeIndex] = PullFromEdge(edge);
                    }
                };
            }
            else if (collectionMetatype is CollectionMetatype.Array)
            {
                return (port, edgeIndex) =>
                {
                    var cnt = edges.Count;
                    var node = port.owner;

                    var arr = fieldInfo.GetValue(node) as TElement[];
                    if (arr is null || arr.Length != cnt)
                    {
                        arr = new TElement[cnt];
                        setter(node, arr as TCollection);
                    }

                    if (edgeIndex == -1)
                    {
                        for (int i = 0; i < cnt; i++)
                        {
                            var edge = edges[i];
                            arr[i] = PullFromEdge(edge);
                        }
                    }
                    else
                    {
                        var edge = edges[edgeIndex];
                        arr[edgeIndex] = PullFromEdge(edge);
                    }
                };
            }
            else if (collectionMetatype is CollectionMetatype.Collection)
            {
                return (port, edgeIndex) =>
                {
                    var cnt = edges.Count;
                    var node = port.owner;

                    var container = fieldInfo.GetValue(node) as ICollection<TElement>;

                    if (container is null)
                    {
                        container = Activator.CreateInstance(fieldInfo.FieldType) as ICollection<TElement>;
                        setter(node, container as TCollection);
                    }

                    container.Clear();

                    foreach (var edge in edges)
                    {
                        container.Add(PullFromEdge(edge));
                    }
                };
            }
            else
            {
                throw new NotSupportedException($"type {typeof(TCollection)}");
                var edge = edges[0];
                var getter = GetNodeFieldGetter<TCollection>(edge.outputPort.fieldInfo);
                return (port, edgeIndex) =>
                {
                    var node = port.owner;
                    var nfrom = port.edges[0].outputNode;

                    setter(node, getter(nfrom));
                };
            }

            static TElement PullFromEdge(SerializableEdge edge)
            {
                var getter = GetNodeFieldGetter<TElement>(edge.outputPort.fieldInfo);
                return getter.Invoke(edge.outputNode);
            }
        }

        private class FieldPairComparer : IEqualityComparer<(FieldInfo, FieldInfo)>
        {
            private static bool IsMemberEquals(MemberInfo lhs, MemberInfo rhs)
            {
                if (ReferenceEquals(lhs, rhs)) return true;

                if (lhs is null || rhs is null) return false;

                if (lhs.MetadataToken != rhs.MetadataToken) return false;

                if (lhs.Module.MetadataToken != rhs.Module.MetadataToken) return false;

                return true;
            }

            public bool Equals((FieldInfo, FieldInfo) lhs, (FieldInfo, FieldInfo) rhs)
            {
                return IsMemberEquals(lhs.Item1, rhs.Item1)
                    && IsMemberEquals(lhs.Item2, rhs.Item2)
                    ;
            }

            public int GetHashCode((FieldInfo, FieldInfo) obj)
            {
                var hash = 17;
                hash = hash * 23 + obj.Item1.MetadataToken;
                hash = hash * 23 + obj.Item2.MetadataToken;
                return hash;
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
        internal void PullData()
        {
            if (edges.Count > 0)
            {
                if (portData.unpack)
                {
                    packTransfer ??= CreatePackTransfer();
                    packTransfer.Invoke(this, -1);
                }
                else
                {
                    soloTransfer ??= CreateSoloTransfer();
                    soloTransfer.Invoke(edges[0]);
                }
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

        public bool TryGetPorts(string fieldName, out ReadOnlyList<NodePort> ports)
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
