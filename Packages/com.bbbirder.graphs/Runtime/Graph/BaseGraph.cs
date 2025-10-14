using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BBBirder;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace GraphProcessor
{
    public class GraphChanges
    {
        public SerializableEdge removedEdge;
        public SerializableEdge addedEdge;
        public BaseNode removedNode;
        public BaseNode addedNode;
        public BaseNode nodeChanged;
        public BaseNode nodeFocusOutInEditor;
        public Group addedGroups;
        public Group removedGroups;
        public BaseStackNode addedStackNode;
        public BaseStackNode removedStackNode;
        public StickyNote addedStickyNotes;
        public StickyNote removedStickyNotes;
    }

    [System.Serializable]
    public abstract class BaseGraph : ISerializationCallbackReceiver
    {
        /// <summary>
        /// The unity object that contains this graph. It can be a Component or ScriptableObject.
        /// </summary>
        [NonSerialized] internal UnityEngine.Object unityObject;

        /// <summary>
        /// List of all the nodes in the graph.
        /// </summary>
        /// <typeparam name="BaseNode"></typeparam>
        /// <returns></returns>
        [SerializeReference]
        public List<BaseNode> nodes = new();

        [SerializeReference] protected List<BaseNode> entryNodes = new();

        /// <summary>
        /// Dictionary to access node per GUID, faster than a search in a list
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="BaseNode"></typeparam>
        /// <returns></returns>
        [System.NonSerialized]
        public Dictionary<string, BaseNode> nodesPerGUID = new();

        /// <summary>
        /// Json list of edges
        /// </summary>
        /// <typeparam name="SerializableEdge"></typeparam>
        /// <returns></returns>
        [SerializeField]
        public List<SerializableEdge> edges = new();
        /// <summary>
        /// Dictionary of edges per GUID, faster than a search in a list
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="SerializableEdge"></typeparam>
        /// <returns></returns>
        [System.NonSerialized]
        public Dictionary<string, SerializableEdge> edgesPerGUID = new();

        /// <summary>
        /// All groups in the graph
        /// </summary>
        /// <typeparam name="Group"></typeparam>
        /// <returns></returns>
        [SerializeField, FormerlySerializedAs("commentBlocks")]
        public List<Group> groups = new();

        /// <summary>
        /// All Stack Nodes in the graph
        /// </summary>
        /// <typeparam name="stackNodes"></typeparam>
        /// <returns></returns>
        [SerializeField, SerializeReference] // Polymorphic serialization
        public List<BaseStackNode> stackNodes = new();

        /// <summary>
        /// All exposed parameters in the graph
        /// </summary>
        /// <typeparam name="ExposedParameter"></typeparam>
        /// <returns></returns>
        [SerializeField, SerializeReference]
        public List<ExposedParameter> exposedParameters = new();

        // [SerializeField, FormerlySerializedAs("exposedParameters")] // We keep this for upgrade
        // List<ExposedParameter> serializedParameterList = new List<ExposedParameter>();

        [SerializeField]
        public List<StickyNote> stickyNotes = new();

        [NonSerialized]
        Scene linkedScene;

        // Trick to keep the node inspector alive during the editor session
        [SerializeField]
        internal UnityEngine.Object nodeInspectorReference;

        [SerializeField]
        protected BaseNode EntryNode => entryNodes.FirstOrDefault();

        //graph visual properties
        public Vector3 position = Vector3.zero;
        public Vector3 scale = Vector3.one;

        /// <summary>
        /// Triggered when something is changed in the list of exposed parameters
        /// </summary>
        public event Action onExposedParameterListChanged;
        public event Action<ExposedParameter> onExposedParameterModified;
        public event Action<ExposedParameter> onExposedParameterValueChanged;

        /// <summary>
        /// Triggered when the graph is linked to an active scene.
        /// </summary>
        public event Action<Scene> onSceneLinked;

        /// <summary>
        /// Triggered when the graph is enabled
        /// </summary>
        public event Action onEnabled;

        /// <summary>
        /// Triggered when the graph is changed
        /// </summary>
        public event Action<GraphChanges> onGraphChanges;

        #region Initialization

        [System.NonSerialized]
        bool _isInitialized = false;


        Dictionary<BaseNode, (int hi, int vi)> nodeIndices = new();
        Dictionary<BaseNode, HashSet<BaseNode>> dataFlowDirections = new();
        Dictionary<BaseNode, HashSet<BaseNode>> dataFlowReversedDirections = new();
        Dictionary<BaseNode, bool> dataFlowDeterministics = new();
        Dictionary<BaseNode, List<BaseNode>> dataflowDependenciesMatrix = new();

        public bool IsInitialized => _isInitialized;

        public UnityEngine.Object GetContainingObject() => unityObject;

        protected internal virtual void Initialize()
        {
            if (IsInitialized)
                Deinitialize();

            // MigrateGraphIfNeeded();
            InitializeGraphElements();

            // destroy broken elements
            edges.RemoveAll(e => e.inputNode == null
                || e.outputNode == null
                || string.IsNullOrEmpty(e.outputFieldName)
                || string.IsNullOrEmpty(e.inputFieldName)
            );
            nodes.RemoveAll(n => n == null);

            _isInitialized = true;
            onEnabled?.Invoke();
        }

        internal void ClearNodeSortCache()
        {
            nodeIndices.Clear();
        }

        private void EnsureNodeSorted()
        {
            nodes.RemoveAll(n => n is null);

            if (nodes.Count > 0 && nodeIndices.Count == 0)
            {
                using var _ = CollectionPool.Get<List<BaseNode>>(out var tempNodes);
                tempNodes.AddRange(nodes);

                tempNodes.Sort((l, r) => Math.Sign(l.position.y - r.position.y));

                for (int i = 0; i < tempNodes.Count; i++)
                {
                    var n = tempNodes[i];
                    nodeIndices[n] = (i, 0);
                }

                tempNodes.Sort((l, r) => Math.Sign(l.position.x - r.position.x));

                for (int i = 0; i < tempNodes.Count; i++)
                {
                    var n = tempNodes[i];
                    var rec = nodeIndices[n];
                    nodeIndices[n] = (rec.hi, i);
                }
            }
        }

        /// <summary>
        /// see <see href="EdgeFullSorting.md"/>
        /// </summary>
        internal void SortEdges()
        {
            EnsureNodeSorted();

            edges.RemoveAll(e => e.inputNode == null
                || e.outputNode == null
                || string.IsNullOrEmpty(e.outputFieldName)
                || string.IsNullOrEmpty(e.inputFieldName)
            );

            foreach (var edge in edges)
            {
                edge.OnBeforeSerialize();
            }

            edges.Sort((l, r) =>
            {
                // 1. src ori

                var ol = l.outputPort.portData.vertical ? 1 : 0;
                var or = r.outputPort.portData.vertical ? 1 : 0;
                if (ol != or) return ol - or;

                // 2. src node pos

                var nl = l.outputNode;
                var nr = r.outputNode;
                if (nr != nl)
                {
                    bool isV = ol == 1;
                    if (isV)
                    {
                        return nodeIndices[nl].vi - nodeIndices[nr].vi;
                    }
                    else
                    {
                        return nodeIndices[nl].hi - nodeIndices[nr].hi;
                    }
                }

                // 3. src port index

                var pl = l.outputPort;
                var pr = r.outputPort;
                if (pl != pr)
                {
                    return pl.owner.outputPorts.IndexOf(pl)
                        - pr.owner.outputPorts.IndexOf(pr);
                }

                // 4. dst node pos

                nl = l.inputNode;
                nr = r.inputNode;
                if (nr != nl)
                {
                    bool isV = ol == 1;
                    if (isV)
                    {
                        return nodeIndices[nl].vi - nodeIndices[nr].vi;
                    }
                    else
                    {
                        return nodeIndices[nl].hi - nodeIndices[nr].hi;
                    }
                }

                // 5. dst ori

                ol = l.inputPort.portData.vertical ? 1 : 0;
                or = r.inputPort.portData.vertical ? 1 : 0;
                if (ol != or) return ol - or;

                // 6. dst port index

                pl = l.inputPort;
                pr = r.inputPort;
                if (pl != pr)
                {
                    return pl.owner.inputPorts.IndexOf(pl)
                        - pr.owner.inputPorts.IndexOf(pr);
                }

                // 7. edge guid

                return l.GUID.CompareTo(r.GUID);
            });

            foreach (var n in nodes)
            {
                foreach (var p in n.inputPorts)
                {
                    p.GetEdges().Sort((l, r) => edges.IndexOf(l) - edges.IndexOf(r));
                }

                foreach (var p in n.outputPorts)
                {
                    p.GetEdges().Sort((l, r) => edges.IndexOf(l) - edges.IndexOf(r));
                }
            }
        }

        protected internal virtual void Deinitialize()
        {
            if (!_isInitialized) return;

            _isInitialized = false;
            foreach (var node in nodes)
                node?.DisableInternal();
        }

        void InitializeGraphElements()
        {
            // Sanitize the element lists (it's possible that nodes are null if their full class name have changed)
            // If you rename / change the assembly of a node or parameter, please use the MovedFrom() attribute to avoid breaking the graph.
            nodes.RemoveAll(n => n == null);
            exposedParameters.RemoveAll(e => e == null);

            foreach (var node in nodes)
            {
                nodesPerGUID[node.GUID] = node;
                node.Initialize(this);
            }

            using var _ = CollectionPool.Get<List<SerializableEdge>>(out var brokenEdges);

            foreach (var edge in edges)
            {
                edge.Deserialize(this);
                edgesPerGUID[edge.GUID] = edge;

                // Sanity check for the edge:
                if (edge.inputPort == null || edge.outputPort == null)
                {
                    brokenEdges.Add(edge);
                    continue;
                }

                // Add the edge to the non-serialized port data
                edge.inputPort.owner.OnEdgeConnected(edge);
                edge.outputPort.owner.OnEdgeConnected(edge);
            }

            foreach (var edge in brokenEdges)
            {
                Disconnect(edge.GUID);
            }
        }

        #endregion // end of Initialization

        #region Execution
        public bool AutoStep { get; private set; }
        public virtual bool IsRunning { get; }

        internal event Action onExecutionStateChanged;

        protected internal void NotifyExecutionStateChanged()
        {
            onExecutionStateChanged?.Invoke();
        }

        public virtual void Run()
        {
            AutoStep = true;
            if (IsRunning) return;

            MoveNext();
            FrameStep();
        }

        public void FrameStep()
        {
            const int MAX_ITERATION_COUNT = 200;
            var iter = 0;
            while (IsRunning && MoveNext() != NodeStatus.Running)
            {
                if (iter++ > MAX_ITERATION_COUNT)
                {
                    Debug.LogError($"execution iteration exceeds limits {MAX_ITERATION_COUNT}.");
                    Stop();
                    break;
                }
            }

            if (!IsRunning)
            {
                AutoStep = false;
            }

            NotifyExecutionStateChanged();
        }

        public abstract NodeStatus MoveNext();

        public virtual void Stop()
        {
        }

        internal protected virtual NodeStatus GetNodeStatus(BaseNode node) => NodeStatus.Normal;

        #endregion // end of Execution

        internal virtual bool CanConnectEdge(NodePort outport, NodePort inport)
        {
            var outNode = outport.owner;
            if (outport.fieldInfo.FieldType == typeof(ExecutionLink)) return true;

            // Default Rule1: data transfer cannot be circular

            using var _1 = CollectionPool.Get<HashSet<BaseNode>>(out var visitedNodes);
            using var _2 = CollectionPool.Get<Queue<BaseNode>>(out var waveFront);
            waveFront.Enqueue(inport.owner);
            visitedNodes.Add(inport.owner);
            while (waveFront.TryDequeue(out var node))
            {
                foreach (var port in node.outputPorts)
                {
                    foreach (var e in port.GetEdges())
                    {
                        if (e.outputPort.fieldInfo.FieldType == typeof(ExecutionLink))
                            continue;

                        var n = e.inputNode;
                        if (ReferenceEquals(n, outNode))
                            return false;

                        if (visitedNodes.Contains(n))
                            continue;

                        visitedNodes.Add(n);
                        waveFront.Enqueue(n);
                    }
                }
            }

            return true;
        }

        internal void ClearPortsTransferCache()
        {
            foreach (var node in nodes)
            {
                foreach (var port in node.inputPorts)
                {
                    port.ClearTransferCache();
                }
            }
        }

        internal void ClearDataFlowDirectionsCache()
        {
            dataFlowDirections.Clear();
            dataFlowReversedDirections.Clear();
            dataFlowDeterministics.Clear();
            dataflowDependenciesMatrix.Clear();
        }


        public HashSet<BaseNode> GetDataFlowDirections(BaseNode node)
        {
            if (!dataFlowDirections.TryGetValue(node, out var directions))
            {
                directions = new();
                foreach (var p in node.outputPorts)
                {
                    if (p.fieldInfo.FieldType == typeof(ExecutionLink))
                        continue;

                    foreach (var e in p.GetEdges())
                    {
                        directions.Add(e.inputNode);
                    }
                }

                dataFlowDirections[node] = directions;
            }


            return directions;
        }

        public HashSet<BaseNode> GetDataFlowReversedDirections(BaseNode node)
        {
            if (!dataFlowReversedDirections.TryGetValue(node, out var directions))
            {
                directions = new();
                foreach (var p in node.inputPorts)
                {
                    if (p.fieldInfo.FieldType == typeof(ExecutionLink))
                        continue;

                    foreach (var e in p.GetEdges())
                    {
                        directions.Add(e.outputNode);
                    }
                }

                dataFlowReversedDirections[node] = directions;
            }

            return directions;
        }


        bool IsFlowDeterministic(BaseNode targetNode)
        {
            if (!targetNode.IsDataFlowDeterministic) return false;

            if (!dataFlowDeterministics.TryGetValue(targetNode, out var deterministic))
            {
                deterministic = true;
                foreach (var p in targetNode.inputPorts)
                {
                    if (p.fieldInfo.FieldType == typeof(ExecutionLink))
                        continue;
                    foreach (var e in p.GetEdges())
                    {
                        var pn = e.outputNode;
                        if (!pn.IsDataFlowDeterministic)
                        {
                            deterministic = false;
                            goto break_all;
                        }
                    }
                }

            break_all:
                dataFlowDeterministics[targetNode] = deterministic;
            }

            return deterministic;
        }

        public void PullDataRecursively(NodePort inputPort)
        {
            foreach (var e in inputPort.GetEdges())
            {
                var n = e.outputNode;
                PullDataRecursively(n);
            }

            inputPort.PullData();
        }

        public void PullDataRecursively(BaseNode targetNode)
        {
            bool first;

            // Compute dependencies

            if (first = !dataflowDependenciesMatrix.TryGetValue(targetNode, out var dependencies))
            {
                dependencies = new();

                using var _1 = CollectionPool.Get<Dictionary<BaseNode, int>>(out var indegrees);
                using var _2 = CollectionPool.Get<Queue<BaseNode>>(out var waveFront);
                using var _3 = CollectionPool.Get<Queue<BaseNode>>(out var echoFront);

                waveFront.Enqueue(targetNode);

                while (waveFront.TryDequeue(out var node))
                {
                    if (node.PullDataManually)
                    {
                        echoFront.Enqueue(node);
                        continue;
                    }

                    var argumentNodes = GetDataFlowReversedDirections(node);
                    var cnt = argumentNodes.Count;
                    indegrees[node] = cnt;
                    if (cnt == 0)
                    {
                        echoFront.Enqueue(node);
                    }

                    foreach (var n in argumentNodes)
                    {
                        if (indegrees.ContainsKey(n))
                            continue;

                        waveFront.Enqueue(n);
                    }
                }

                while (echoFront.TryDequeue(out var node))
                {
                    dependencies.Add(node);
                    indegrees.Remove(node);
                    foreach (var n in GetDataFlowDirections(node))
                    {
                        if (indegrees.TryGetValue(n, out var degree))
                        {
                            if (degree == 1)
                            {
                                indegrees.Remove(n);
                                echoFront.Enqueue(n);
                            }
                            else
                            {
                                indegrees[n] = degree - 1;
                            }
                        }
                    }
                }

                if (indegrees.Count > 0)
                {
                    throw new("Circular dependency detected!");
                }

                dataflowDependenciesMatrix[targetNode] = dependencies;
            }

            // flow up

            if (first)
            {
                // prune deterministic leaves
                using var _ = CollectionPool.Get<HashSet<BaseNode>>(out var deterministicLeaves);
                foreach (var n in dependencies)
                {
                    if (IsFlowDeterministic(n))
                    {
                        deterministicLeaves.Add(n);
                    }

                    n.PullDatas();
                }

                foreach (var n in deterministicLeaves)
                {
                    dependencies.Remove(n);
                }
            }
            else
            {
                foreach (var n in dependencies) n.PullDatas();
            }
        }

        /// <summary>
        /// Do some graph elements correction jobs here.
        /// </summary>
        internal protected virtual void BeforeSaveToDisk()
        {
            entryNodes.Clear();
            foreach (var n in nodes)
            {
                if (n is IEntryNode)
                {
                    entryNodes.Add(n);
                }
            }
        }

        public virtual void OnAssetDeleted() { }

        /// <summary>
        /// Adds a node to the graph
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public BaseNode AddNode(BaseNode node)
        {
            nodesPerGUID[node.GUID] = node;

            nodes.Add(node);
            node.Initialize(this);

            NotifyGraphChanges(new GraphChanges { addedNode = node });

            return node;
        }

        /// <summary>
        /// Removes a node from the graph
        /// </summary>
        /// <param name="node"></param>
        public void RemoveNode(BaseNode node)
        {
            node.DisableInternal();
            node.DestroyInternal();

            nodesPerGUID.Remove(node.GUID);

            nodes.Remove(node);

            NotifyGraphChanges(new GraphChanges { removedNode = node });
        }

        /// <summary>
        /// Connect two ports with an edge
        /// </summary>
        /// <param name="inputPort">input port</param>
        /// <param name="outputPort">output port</param>
        /// <param name="DisconnectInputs">is the edge allowed to disconnect another edge</param>
        /// <returns>the connecting edge</returns>
        public SerializableEdge Connect(NodePort inputPort, NodePort outputPort, bool autoDisconnectInputs = true)
        {
            var edge = SerializableEdge.CreateNewEdge(this, inputPort, outputPort);

            //If the input port does not support multi-connection, we remove them
            if (autoDisconnectInputs && !inputPort.portData.acceptMultipleEdges)
            {
                foreach (var e in inputPort.GetEdges().ToList())
                {
                    // TODO: do not disconnect them if the connected port is the same than the old connected
                    Disconnect(e);
                }
            }
            // same for the output port:
            if (autoDisconnectInputs && !outputPort.portData.acceptMultipleEdges)
            {
                foreach (var e in outputPort.GetEdges().ToList())
                {
                    // TODO: do not disconnect them if the connected port is the same than the old connected
                    Disconnect(e);
                }
            }

            edges.Add(edge);

            // Add the edge to the list of connected edges in the nodes
            inputPort.owner.OnEdgeConnected(edge);
            outputPort.owner.OnEdgeConnected(edge);

            NotifyGraphChanges(new GraphChanges { addedEdge = edge });

            return edge;
        }

        /// <summary>
        /// Disconnect two ports
        /// </summary>
        /// <param name="inputNode">input node</param>
        /// <param name="inputFieldName">input field name</param>
        /// <param name="outputNode">output node</param>
        /// <param name="outputFieldName">output field name</param>
        public void Disconnect(BaseNode inputNode, string inputFieldName, BaseNode outputNode, string outputFieldName)
        {
            edges.RemoveAll(r =>
            {
                bool remove = r.inputNode == inputNode
                && r.outputNode == outputNode
                && r.outputFieldName == outputFieldName
                && r.inputFieldName == inputFieldName;

                if (remove)
                {
                    r.inputNode?.OnEdgeDisconnected(r);
                    r.outputNode?.OnEdgeDisconnected(r);
                    NotifyGraphChanges(new GraphChanges { removedEdge = r });
                }

                return remove;
            });
        }

        /// <summary>
        /// Disconnect an edge
        /// </summary>
        /// <param name="edge"></param>
        public void Disconnect(SerializableEdge edge) => Disconnect(edge.GUID);

        /// <summary>
        /// Disconnect an edge
        /// </summary>
        /// <param name="edgeGUID"></param>
        public void Disconnect(string edgeGUID)
        {
            List<(BaseNode, SerializableEdge)> disconnectEvents = new List<(BaseNode, SerializableEdge)>();

            edges.RemoveAll(r =>
            {
                if (r.GUID == edgeGUID)
                {
                    disconnectEvents.Add((r.inputNode, r));
                    disconnectEvents.Add((r.outputNode, r));
                    NotifyGraphChanges(new GraphChanges { removedEdge = r });
                }

                return r.GUID == edgeGUID;
            });

            // Delay the edge disconnect event to avoid recursion
            foreach (var (node, edge) in disconnectEvents)
                node?.OnEdgeDisconnected(edge);
        }

        /// <summary>
        /// Add a group
        /// </summary>
        /// <param name="block"></param>
        public void AddGroup(Group block)
        {
            groups.Add(block);
            NotifyGraphChanges(new GraphChanges { addedGroups = block });
        }

        /// <summary>
        /// Removes a group
        /// </summary>
        /// <param name="block"></param>
        public void RemoveGroup(Group block)
        {
            groups.Remove(block);
            NotifyGraphChanges(new GraphChanges { removedGroups = block });
        }

        /// <summary>
        /// Add a StackNode
        /// </summary>
        /// <param name="stackNode"></param>
        public void AddStackNode(BaseStackNode stackNode)
        {
            stackNodes.Add(stackNode);
            NotifyGraphChanges(new GraphChanges { addedStackNode = stackNode });
        }

        /// <summary>
        /// Remove a StackNode
        /// </summary>
        /// <param name="stackNode"></param>
        public void RemoveStackNode(BaseStackNode stackNode)
        {
            stackNodes.Remove(stackNode);
            NotifyGraphChanges(new GraphChanges { removedStackNode = stackNode });
        }

        /// <summary>
        /// Add a sticky note
        /// </summary>
        /// <param name="note"></param>
        public void AddStickyNote(StickyNote note)
        {
            stickyNotes.Add(note);
            NotifyGraphChanges(new GraphChanges { addedStickyNotes = note });
        }

        /// <summary>
        /// Removes a sticky note
        /// </summary>
        /// <param name="note"></param>
        public void RemoveStickyNote(StickyNote note)
        {
            stickyNotes.Remove(note);
            NotifyGraphChanges(new GraphChanges { removedStickyNotes = note });
        }

        /// <summary>
        /// Invoke the onGraphChanges event, can be used as trigger to execute the graph when the content of a node is changed
        /// </summary>
        /// <param name="node"></param>
        public void NotifyNodeContentChanged(BaseNode node) => NotifyGraphChanges(new GraphChanges { nodeChanged = node });

        public void NotifyNodeFocusOutInEditor(BaseNode node) => NotifyGraphChanges(new GraphChanges { nodeFocusOutInEditor = node });

        private void NotifyGraphChanges(GraphChanges changes)
        {
            onGraphChanges?.Invoke(changes);
            OnGraphChanges(changes);
        }

        protected virtual void OnGraphChanges(GraphChanges changes)
        {
            if (changes.addedNode is IEntryNode)
            {
                entryNodes.Add(changes.addedNode);
            }

            if (changes.removedNode is IEntryNode)
            {
                entryNodes.Remove(changes.removedNode);
            }
        }

        public void OnBeforeSerialize()
        {
            // Cleanup broken elements
            stackNodes.RemoveAll(s => s == null);
            nodes.RemoveAll(n => n == null);
        }

        // We can deserialize data here because it's called in a unity context
        // so we can load objects references
        public void Deserialize()
        {
            // Disable nodes correctly before removing them:
            if (nodes != null)
            {
                foreach (var node in nodes)
                    node.DisableInternal();
            }

            // MigrateGraphIfNeeded();

            InitializeGraphElements();
        }

        public void OnAfterDeserialize() { }

        internal protected virtual void PostprocessNewNodePort(bool input, NodePort port) { }

        /// <summary>
        /// Add an exposed parameter
        /// </summary>
        /// <param name="name">parameter name</param>
        /// <param name="type">parameter type (must be a subclass of ExposedParameter)</param>
        /// <param name="value">default value</param>
        /// <returns>The unique id of the parameter</returns>
        public string AddExposedParameter(string name, Type type, object value = null)
        {

            if (!type.IsSubclassOf(typeof(ExposedParameter)))
            {
                Debug.LogError($"Can't add parameter of type {type}, the type doesn't inherit from ExposedParameter.");
            }

            var param = Activator.CreateInstance(type) as ExposedParameter;

            // patch value with correct type:
            if (param.GetValueType().IsValueType)
                value = Activator.CreateInstance(param.GetValueType());

            param.Initialize(name, value);
            exposedParameters.Add(param);

            onExposedParameterListChanged?.Invoke();

            return param.guid;
        }

        /// <summary>
        /// Add an already allocated / initialized parameter to the graph
        /// </summary>
        /// <param name="parameter">The parameter to add</param>
        /// <returns>The unique id of the parameter</returns>
        public string AddExposedParameter(ExposedParameter parameter)
        {
            string guid = Guid.NewGuid().ToString(); // Generated once and unique per parameter

            parameter.guid = guid;
            exposedParameters.Add(parameter);

            onExposedParameterListChanged?.Invoke();

            return guid;
        }

        /// <summary>
        /// Remove an exposed parameter
        /// </summary>
        /// <param name="ep">the parameter to remove</param>
        public void RemoveExposedParameter(ExposedParameter ep)
        {
            exposedParameters.Remove(ep);

            onExposedParameterListChanged?.Invoke();
        }

        /// <summary>
        /// Remove an exposed parameter
        /// </summary>
        /// <param name="guid">GUID of the parameter</param>
        public void RemoveExposedParameter(string guid)
        {
            if (exposedParameters.RemoveAll(e => e.guid == guid) != 0)
                onExposedParameterListChanged?.Invoke();
        }

        internal void NotifyExposedParameterListChanged()
            => onExposedParameterListChanged?.Invoke();

        /// <summary>
        /// Update an exposed parameter value
        /// </summary>
        /// <param name="guid">GUID of the parameter</param>
        /// <param name="value">new value</param>
        public void UpdateExposedParameter(string guid, object value)
        {
            var param = exposedParameters.Find(e => e.guid == guid);
            if (param == null)
                return;

            if (value != null && !param.GetValueType().IsAssignableFrom(value.GetType()))
                throw new Exception("Type mismatch when updating parameter " + param.name + ": from " + param.GetValueType() + " to " + value.GetType().AssemblyQualifiedName);

            param.value = value;
            onExposedParameterModified?.Invoke(param);
        }

        /// <summary>
        /// Update the exposed parameter name
        /// </summary>
        /// <param name="parameter">The parameter</param>
        /// <param name="name">new name</param>
        public void UpdateExposedParameterName(ExposedParameter parameter, string name)
        {
            parameter.name = name;
            onExposedParameterModified?.Invoke(parameter);
        }

        /// <summary>
        /// Update parameter visibility
        /// </summary>
        /// <param name="parameter">The parameter</param>
        /// <param name="isHidden">is Hidden</param>
        public void NotifyExposedParameterChanged(ExposedParameter parameter)
        {
            onExposedParameterModified?.Invoke(parameter);
        }

        public void NotifyExposedParameterValueChanged(ExposedParameter parameter)
        {
            onExposedParameterValueChanged?.Invoke(parameter);
        }

        /// <summary>
        /// Get the exposed parameter from name
        /// </summary>
        /// <param name="name">name</param>
        /// <returns>the parameter or null</returns>
        public ExposedParameter GetExposedParameter(string name)
        {
            return exposedParameters.FirstOrDefault(e => e.name == name);
        }

        /// <summary>
        /// Get exposed parameter from GUID
        /// </summary>
        /// <param name="guid">GUID of the parameter</param>
        /// <returns>The parameter</returns>
        public ExposedParameter GetExposedParameterFromGUID(string guid)
        {
            return exposedParameters.FirstOrDefault(e => e?.guid == guid);
        }

        /// <summary>
        /// Set parameter value from name. (Warning: the parameter name can be changed by the user)
        /// </summary>
        /// <param name="name">name of the parameter</param>
        /// <param name="value">new value</param>
        /// <returns>true if the value have been assigned</returns>
        public bool SetParameterValue(string name, object value)
        {
            var e = exposedParameters.FirstOrDefault(p => p.name == name);

            if (e == null)
                return false;

            e.value = value;

            return true;
        }

        /// <summary>
        /// Get the parameter value
        /// </summary>
        /// <param name="name">parameter name</param>
        /// <returns>value</returns>
        public object GetParameterValue(string name) => exposedParameters.FirstOrDefault(p => p.name == name)?.value;

        /// <summary>
        /// Get the parameter value template
        /// </summary>
        /// <param name="name">parameter name</param>
        /// <typeparam name="T">type of the parameter</typeparam>
        /// <returns>value</returns>
        public T GetParameterValue<T>(string name) => (T)GetParameterValue(name);

        /// <summary>
        /// Link the current graph to the scene in parameter, allowing the graph to pick and serialize objects from the scene.
        /// </summary>
        /// <param name="scene">Target scene to link</param>
        public void LinkToScene(Scene scene)
        {
            linkedScene = scene;
            onSceneLinked?.Invoke(scene);
        }

        /// <summary>
        /// Return true when the graph is linked to a scene, false otherwise.
        /// </summary>
        public bool IsLinkedToScene() => linkedScene.IsValid();

        /// <summary>
        /// Get the linked scene. If there is no linked scene, it returns an invalid scene
        /// </summary>
        public Scene GetLinkedScene() => linkedScene;

        /// <summary>
        /// Tell if two types can be connected in the context of a graph
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public static bool TypesAreConnectable(Type t1, Type t2)
        {
            if (t1 == null || t2 == null)
                return false;

            // User defined type convertions
            if (RuntimeConverter.CanConvert(t1, t2) || RuntimeConverter.CanConvert(t2, t1))
            {
                return true;
            }

            return false;
        }
    }
}
