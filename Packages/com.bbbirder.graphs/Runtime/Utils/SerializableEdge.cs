using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace GraphProcessor
{
	[System.Serializable]
	public class SerializableEdge : ISerializationCallbackReceiver
	{
		public string GUID;

		[NonSerialized] BaseGraph owner;

		[SerializeField]
		string inputNodeGUID;
		[SerializeField]
		string outputNodeGUID;

		[System.NonSerialized]
		public BaseNode inputNode;

		[System.NonSerialized]
		public NodePort inputPort;
		[System.NonSerialized]
		public NodePort outputPort;

		private Action<BaseNode, BaseNode> transferFunc;

		[System.NonSerialized]
		public BaseNode outputNode;

		public string inputFieldName;
		public string outputFieldName;

		// Use to store the id of the field that generate multiple ports
		public string inputPortIdentifier;
		public string outputPortIdentifier;

		public Action<BaseNode, BaseNode> TransferFunc => transferFunc ??= GetTransferFunc();

		public SerializableEdge() { }

		public static Dictionary<(FieldInfo, FieldInfo), Action<BaseNode, BaseNode>> s_dataTransfers = new(new FieldPairComparer());

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


		static MethodInfo s_miTransferHelper;
		private static Action<BaseNode, BaseNode> TransferHelper<TTo>(FieldInfo ffrom, FieldInfo fto)
		{
			const BindingFlags Flags = BindingFlags.Static | BindingFlags.NonPublic;
			var mifrom = ffrom.DeclaringType.GetMethod("get_" + ffrom.Name, Flags);
			var mito = fto.DeclaringType.GetMethod("set_" + fto.Name, Flags);

			// var attrInput = fto.GetCustomAttribute<InputAttribute>();
			// if (attrInput.unpack)
			// {
			// 	if ()
			// }

			var getter = mifrom.MakeGenericMethod(typeof(TTo)).CreateDelegate(typeof(Func<BaseNode, TTo>)) as Func<BaseNode, TTo>;
			var setter = mito.CreateDelegate(typeof(Action<BaseNode, TTo>)) as Action<BaseNode, TTo>;
			return (nfrom, nto) =>
			{
				var value = getter(nfrom);
				// Debug.Log($"set {value} from {nfrom}::{ffrom.Name} to {nto}::{fto.Name}");
				setter(nto, value);
			};
		}

		// private static Action<BaseNode, List<SerializableEdge>> PackHelper<TFrom, TTo>(FieldInfo fto)
		// {

		// }

		// static void IsTypeArrayLike(Type type)
		// {
		// 	if (type.IsArray)
		// }

		private Action<BaseNode, BaseNode> GetTransferFunc()
		{
			var key = (outputPort.fieldInfo, inputPort.fieldInfo);
			if (!s_dataTransfers.TryGetValue(key, out var transFunc))
			{
				s_miTransferHelper ??= typeof(SerializableEdge).GetMethod(nameof(TransferHelper), BindingFlags.Static | BindingFlags.NonPublic);
				s_dataTransfers[key] = transFunc = s_miTransferHelper
					.MakeGenericMethod(inputPort.fieldInfo.FieldType)
					.Invoke(null, new object[] { outputPort.fieldInfo, inputPort.fieldInfo }) as Action<BaseNode, BaseNode>;
			}

			return transFunc;
		}

		public static SerializableEdge CreateNewEdge(BaseGraph graph, NodePort inputPort, NodePort outputPort)
		{
			SerializableEdge edge = new SerializableEdge();

			edge.owner = graph;
			edge.GUID = System.Guid.NewGuid().ToString();
			edge.inputNode = inputPort.owner;
			edge.inputFieldName = inputPort.fieldName;
			edge.outputNode = outputPort.owner;
			edge.outputFieldName = outputPort.fieldName;
			edge.inputPort = inputPort;
			edge.outputPort = outputPort;
			edge.inputPortIdentifier = inputPort.portData.identifier;
			edge.outputPortIdentifier = outputPort.portData.identifier;
			return edge;
		}

		public void OnBeforeSerialize()
		{
			if (outputNode == null || inputNode == null)
				return;

			outputNodeGUID = outputNode.GUID;
			inputNodeGUID = inputNode.GUID;
		}

		public void OnAfterDeserialize() { }

		//here our owner have been deserialized
		public void Deserialize(BaseGraph owner)
		{
			this.owner = owner;

			if (!owner.nodesPerGUID.ContainsKey(outputNodeGUID) || !owner.nodesPerGUID.ContainsKey(inputNodeGUID))
				return;

			outputNode = owner.nodesPerGUID[outputNodeGUID];
			inputNode = owner.nodesPerGUID[inputNodeGUID];
			inputPort = inputNode.GetPort(inputFieldName, inputPortIdentifier);
			outputPort = outputNode.GetPort(outputFieldName, outputPortIdentifier);
		}

		public override string ToString() => $"{outputNode.name}:{outputPort.fieldName} -> {inputNode.name}:{inputPort.fieldName}";
	}
}
