using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BBBirder;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static GraphProcessor.RuntimeTypeCache;

namespace GraphProcessor
{
	public enum NodeStatus
	{
		Normal,
		Running,
		Success,
		Fault,
	}

	// public delegate IEnumerable<PortData> CustomPortBehaviorDelegate(List<SerializableEdge> edges);
	// public delegate IEnumerable<PortData> CustomPortTypeBehaviorDelegate(string fieldName, string displayName, object value);
	// public delegate void NodeStatusChangedDelegate(NodeStatus statusBefore, NodeStatus statusAfter);

	[Serializable]
	public abstract class BaseNode
	{
		protected static TTo ConvertData<TFrom, TTo>(TFrom data)
		{
			return RuntimeConverter.Convert<TFrom, TTo>(data);
		}

		//id
		public string GUID;

		internal protected virtual bool IsDataFlowDeterministic => false;
		// public event NodeStatusChangedDelegate onStatusChanged;

		[SerializeField]
		internal string nodeCustomName = null; // The name of the node in case it was renamed by a user

		/// <summary>
		/// Name of the node, it will be displayed in the title section
		/// </summary>
		/// <returns></returns>
		public virtual string name => GetType().Name;

		/// <summary>
		/// The accent color of the node
		/// </summary>
		public virtual Color color => Color.clear;

		/// <summary>
		/// Set a custom uss file for the node. We use a Resources.Load to get the stylesheet so be sure to put the correct resources path
		/// https://docs.unity3d.com/ScriptReference/Resources.Load.html
		/// </summary>
		public virtual string layoutStylePath => string.Empty;

		/// <summary>
		/// If the node can be locked or not
		/// </summary>
		public virtual bool unlockable => true;

		/// <summary>
		/// Is the node is locked (if locked it can't be moved)
		/// </summary>
		public virtual bool isLocked => nodeLock;

		/// <summary>Tell wether or not the node can be processed. Do not check anything from inputs because this step happens before inputs are sent to the node</summary>
		public virtual bool canProcess => true;

		/// <summary>Show the node controlContainer only when the mouse is over the node</summary>
		public virtual bool showControlsOnHover => false;

		/// <summary>True if the node can be deleted, false otherwise</summary>
		public virtual bool deletable => true;

		public virtual bool resizable => false;

		/// <summary>
		/// Container of input ports
		/// </summary>
		[NonSerialized]
		public readonly NodeInputPortContainer inputPorts;
		/// <summary>
		/// Container of output ports
		/// </summary>
		[NonSerialized]
		public readonly NodeOutputPortContainer outputPorts;

		//Node view datas
		public Rect position;
		/// <summary>
		/// Is the node expanded
		/// </summary>
		public bool expanded;
		/// <summary>
		/// Is debug visible
		/// </summary>
		public bool debug;
		/// <summary>
		/// Node locked state
		/// </summary>
		public bool nodeLock;

		public delegate void ProcessDelegate();

		/// <summary>
		/// Triggered when the node is processes
		/// </summary>
		// public event ProcessDelegate onProcessed;
		public event Action<string, NodeMessageType> onMessageAdded;
		public event Action<string> onMessageRemoved;
		/// <summary>
		/// Triggered after an edge was connected on the node
		/// </summary>
		public event Action<SerializableEdge> onAfterEdgeConnected;
		/// <summary>
		/// Triggered after an edge was disconnected on the node
		/// </summary>
		public event Action<SerializableEdge> onAfterEdgeDisconnected;

		/// <summary>
		/// Triggered after a single/list of port(s) is updated, the parameter is the field name
		/// </summary>
		public event Action<string> onPortsUpdated;

		[NonSerialized]
		NodeInformation information;

		/// <summary>
		/// Does the node needs to be visible in the inspector (when selected).
		/// </summary>
		public virtual bool needsInspector => information.needsInspector;

		/// <summary>
		/// Can the node be renamed in the UI. By default a node can be renamed by double clicking it's name.
		/// </summary>
		public virtual bool isRenamable => false;

		/// <summary>
		/// Is the node created from a duplicate operation (either ctrl-D or copy/paste).
		/// </summary>
		public bool createdFromDuplication { get; internal set; } = false;

		/// <summary>
		/// True only when the node was created from a duplicate operation and is inside a group that was also duplicated at the same time.
		/// </summary>
		public bool createdWithinGroup { get; internal set; } = false;

		internal Dictionary<string, NodeFieldInformation> ioFields => information.ioFields;

		// [NonSerialized]
		// internal Dictionary<Type, CustomPortTypeBehaviorDelegate> customPortTypeBehaviorMap = new();

		[NonSerialized]
		List<string> messages = new();

		[NonSerialized]
		protected internal BaseGraph graph;

		internal bool HasCustomEnter => information.hasCustomEnter;
		internal bool HasCustomMoveNext => information.hasCustomMoveNext;
		internal bool HasCustomAfterPullDatas => information.hasCustomAfterPullDatas;
		internal protected virtual bool PullDataManually => false;


		public NodeStatus GetStatus(BaseGraph graph)
		{
			return graph.GetNodeStatus(this);
		}

		internal void PullDatas()
		{
			if (!PullDataManually)
			{
				foreach (var p in inputPorts)
				{
					p.PullData();
				}
			}

			AfterPullDatas();
		}

		protected void PullPortData(NodePort inputPort)
		{
			graph.PullDataRecursively(inputPort);
		}

		protected void PullPortData(string fieldName)
		{
			if (inputPorts.TryGetPorts(fieldName, out var ports))
			{
				foreach (var p in ports)
				{
					PullPortData(p);
				}
			}
		}

		protected void PullAllData()
		{
			graph.PullDataRecursively(this);
		}

		protected internal virtual void AfterPullDatas() { }

		/// <summary>
		/// Creates a node of type T at a certain position
		/// </summary>
		/// <param name="position">position in the graph in pixels</param>
		/// <typeparam name="T">type of the node</typeparam>
		/// <returns>the node instance</returns>
		public static T CreateFromType<T>(Vector2 position) where T : BaseNode
		{
			return CreateFromType(typeof(T), position) as T;
		}

		/// <summary>
		/// Creates a node of type nodeType at a certain position
		/// </summary>
		/// <param name="position">position in the graph in pixels</param>
		/// <typeparam name="nodeType">type of the node</typeparam>
		/// <returns>the node instance</returns>
		public static BaseNode CreateFromType(Type nodeType, Vector2 position)
		{
			if (!nodeType.IsSubclassOf(typeof(BaseNode)))
				return null;

			var node = Activator.CreateInstance(nodeType) as BaseNode;

			node.position = new Rect(position, new Vector2(100, 100));

			ExceptionToLog.Call(() => node.OnNodeCreated());

			return node;
		}

		#region Initialization

		// called by the BaseGraph when the node is added to the graph
		public void Initialize(BaseGraph graph)
		{
			this.graph = graph;

			try
			{
				Enable(); // empty virtual method
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}

			LoadPorts();
		}

		public void ReloadPorts()
		{
			using var _ = CollectionPool.Get<HashSet<NodePort>>(out var ports);
			ports.UnionWith(inputPorts);
			ports.UnionWith(outputPorts);

			inputPorts.Clear();
			outputPorts.Clear();

			LoadPorts();

			foreach (var p in ports)
			{
				foreach (var e in p.GetEdges())
				{
					e.Deserialize(graph);

					e.inputPort.owner.OnEdgeConnected(e);
					e.outputPort.owner.OnEdgeConnected(e);
				}
			}

			onPortsUpdated?.Invoke("");
		}

		/// <summary>
		/// Use this function to initialize anything related to ports generation in your node
		/// This will allow the node creation menu to correctly recognize ports that can be connected between nodes
		/// </summary>
		protected internal virtual void LoadPorts()
		{
			foreach (var (name, nodeField) in ioFields)
			{
				if (nodeField.hide) continue;
				AddPort(nodeField.input, nodeField.fieldName, new PortData
				{
					// acceptMultipleEdges = nodeField.isMultiple,
					displayName = nodeField.name,
					tooltip = nodeField.tooltip,
					unpack = nodeField.unpack,
					vertical = nodeField.vertical
				});
			}
		}

		protected BaseNode()
		{
			inputPorts = new NodeInputPortContainer(this);
			outputPorts = new NodeOutputPortContainer(this);

			this.information = RuntimeTypeCache.GetNodeInformation(GetType());
		}

		internal void DisableInternal()
		{
			// port containers are initialized in the OnEnable
			inputPorts.Clear();
			outputPorts.Clear();

			try
			{
				Disable();
			}
			catch (Exception e)
			{
				Debug.LogException(e, graph?.unityObject);
			}
		}

		internal void DestroyInternal()
		{
			try
			{
				Destroy();
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}

		/// <summary>
		/// Called only when the node is created, not when instantiated
		/// </summary>
		public virtual void OnNodeCreated() => GUID = Guid.NewGuid().ToString();

		#endregion

		#region Events and Processing

		public void OnEdgeConnected(SerializableEdge edge)
		{
			bool input = edge.inputNode == this;
			NodePortContainer portCollection = (input) ? (NodePortContainer)inputPorts : outputPorts;

			portCollection.Add(edge);

			onAfterEdgeConnected?.Invoke(edge);
		}

		protected virtual bool CanResetPort(NodePort port) => true;

		public void OnEdgeDisconnected(SerializableEdge edge)
		{
			if (edge == null)
				return;

			bool input = edge.inputNode == this;
			NodePortContainer portCollection = (input) ? (NodePortContainer)inputPorts : outputPorts;

			portCollection.Remove(edge);

			// Reset default values of input port:
			bool haveConnectedEdges = edge.inputNode.inputPorts.Where(p => p.fieldName == edge.inputFieldName).Any(p => p.GetEdges().Count != 0);
			if (edge.inputNode == this && !haveConnectedEdges && CanResetPort(edge.inputPort))
				edge.inputPort?.ResetToDefault();

			onAfterEdgeDisconnected?.Invoke(edge);
		}

		// public void InvokeOnProcessed() => onProcessed?.Invoke();

		/// <summary>
		/// Called when the node is enabled
		/// </summary>
		protected virtual void Enable() { }
		/// <summary>
		/// Called when the node is disabled
		/// </summary>
		protected virtual void Disable() { }
		/// <summary>
		/// Called when the node is removed
		/// </summary>
		protected virtual void Destroy() { }

		/// <summary>
		/// Called when the node is about to executed
		/// </summary>
		public virtual void Enter() { }

		/// <summary>
		/// Called when the node is being executing
		/// </summary>
		/// <returns></returns>
		public virtual bool MoveNext() { return false; }

		#endregion

		#region API and utils

		public void EnqueueExecutionPort(string portName)
		{
			if (!outputPorts.TryGetPorts(portName, out var ports))
			{
				throw new($"field {portName} not found in {this}");
			}

			if (ports.Count > 0)
			{
				graph.PushExecutingPort(ports[0]);
			}
		}

		public virtual void EnqueueExecutionPort(NodePort outputPort)
		{
			graph.PushExecutingPort(outputPort);
		}

		/// <summary>
		/// Add a port
		/// </summary>
		/// <param name="input">is input port</param>
		/// <param name="fieldName">C# field name</param>
		/// <param name="portData">Data of the port</param>
		public NodePort AddPort(bool input, string fieldName, PortData portData)
		{
			// Fixup port data info if needed:
			if (portData.displayType == null)
				portData.displayType = ioFields[fieldName].info.FieldType;

			var port = new NodePort(this, fieldName, portData);
			if (input)
				inputPorts.Add(port);
			else
				outputPorts.Add(port);

			graph.PostprocessNewNodePort(input, port);
			return port;
		}

		/// <summary>
		/// Remove a port
		/// </summary>
		/// <param name="input">is input port</param>
		/// <param name="port">the port to delete</param>
		public void RemovePort(bool input, NodePort port)
		{
			if (input)
				inputPorts.Remove(port);
			else
				outputPorts.Remove(port);
		}

		/// <summary>
		/// Remove port(s) from field name
		/// </summary>
		/// <param name="input">is input</param>
		/// <param name="fieldName">C# field name</param>
		public void RemovePort(bool input, string fieldName)
		{
			if (input)
				inputPorts.RemoveAll(p => p.fieldName == fieldName);
			else
				outputPorts.RemoveAll(p => p.fieldName == fieldName);
		}

		/// <summary>
		/// Get all the nodes connected to the input ports of this node
		/// </summary>
		/// <returns>an enumerable of node</returns>
		public IEnumerable<BaseNode> GetInputNodes()
		{
			foreach (var port in inputPorts)
				foreach (var edge in port.GetEdges())
					yield return edge.outputNode;
		}

		/// <summary>
		/// Get all the nodes connected to the output ports of this node
		/// </summary>
		/// <returns>an enumerable of node</returns>
		public IEnumerable<BaseNode> GetOutputNodes()
		{
			foreach (var port in outputPorts)
				foreach (var edge in port.GetEdges())
					yield return edge.inputNode;
		}

		/// <summary>
		/// Return a node matching the condition in the dependencies of the node
		/// </summary>
		/// <param name="condition">Condition to choose the node</param>
		/// <returns>Matched node or null</returns>
		public BaseNode FindInDependencies(Func<BaseNode, bool> condition)
		{
			Stack<BaseNode> dependencies = new Stack<BaseNode>();

			dependencies.Push(this);

			int depth = 0;
			while (dependencies.Count > 0)
			{
				var node = dependencies.Pop();

				// Guard for infinite loop (faster than a HashSet based solution)
				depth++;
				if (depth > 2000)
					break;

				if (condition(node))
					return node;

				foreach (var dep in node.GetInputNodes())
					dependencies.Push(dep);
			}

			return null;
		}

		/// <summary>
		/// Get the port from field name and identifier
		/// </summary>
		/// <param name="fieldName">C# field name</param>
		/// <param name="identifier">Unique port identifier</param>
		/// <returns></returns>
		public NodePort GetPort(string fieldName, string identifier)
		{
			bool isKeyNull = string.IsNullOrEmpty(identifier);
			foreach (var p in inputPorts)
			{
				if (p.fieldName != fieldName) continue;

				var bothNull = isKeyNull && string.IsNullOrEmpty(p.portData.identifier);
				if (bothNull || identifier == p.portData.identifier)
				{
					return p;
				}
			}

			foreach (var p in outputPorts)
			{
				if (p.fieldName != fieldName) continue;

				var bothNull = isKeyNull && string.IsNullOrEmpty(p.portData.identifier);
				if (bothNull || identifier == p.portData.identifier)
				{
					return p;
				}
			}

			return default;
		}

		/// <summary>
		/// Return all the ports of the node
		/// </summary>
		/// <returns></returns>
		public IEnumerable<NodePort> GetAllPorts()
		{
			foreach (var port in inputPorts)
				yield return port;
			foreach (var port in outputPorts)
				yield return port;
		}

		/// <summary>
		/// Return all the connected edges of the node
		/// </summary>
		/// <returns></returns>
		public IEnumerable<SerializableEdge> GetAllEdges()
		{
			foreach (var port in GetAllPorts())
				foreach (var edge in port.GetEdges())
					yield return edge;
		}

		/// <summary>
		/// Is the port an input
		/// </summary>
		/// <param name="fieldName"></param>
		/// <returns></returns>
		public bool IsFieldInput(string fieldName) => ioFields[fieldName].input;

		/// <summary>
		/// Add a message on the node
		/// </summary>
		/// <param name="message"></param>
		/// <param name="messageType"></param>
		public void AddMessage(string message, NodeMessageType messageType)
		{
			if (messages.Contains(message))
				return;

			onMessageAdded?.Invoke(message, messageType);
			messages.Add(message);
		}

		/// <summary>
		/// Remove a message on the node
		/// </summary>
		/// <param name="message"></param>
		public void RemoveMessage(string message)
		{
			onMessageRemoved?.Invoke(message);
			messages.Remove(message);
		}

		/// <summary>
		/// Remove a message that contains
		/// </summary>
		/// <param name="subMessage"></param>
		public void RemoveMessageContains(string subMessage)
		{
			string toRemove = messages.Find(m => m.Contains(subMessage));
			messages.Remove(toRemove);
			onMessageRemoved?.Invoke(toRemove);
		}

		/// <summary>
		/// Remove all messages on the node
		/// </summary>
		public void ClearMessages()
		{
			foreach (var message in messages)
				onMessageRemoved?.Invoke(message);
			messages.Clear();
		}

		/// <summary>
		/// Set the custom name of the node. This is intended to be used by renamable nodes.
		/// This custom name will be serialized inside the node.
		/// </summary>
		/// <param name="customNodeName">New name of the node.</param>
		public void SetCustomName(string customName) => nodeCustomName = customName;

		/// <summary>
		/// Get the name of the node. If the node have a custom name (set using the UI by double clicking on the node title) then it will return this name first, otherwise it returns the value of the name field.
		/// </summary>
		/// <returns>The name of the node as written in the title</returns>
		public string GetCustomName() => string.IsNullOrEmpty(nodeCustomName) ? name : nodeCustomName;

		#endregion


	}
}
