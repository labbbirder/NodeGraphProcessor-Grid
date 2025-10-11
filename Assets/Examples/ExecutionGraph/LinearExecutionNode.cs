using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GraphProcessor;
using UnityEngine;

namespace BBBirder.Graphs
{

	[System.Serializable]
	/// <summary>
	/// This class represent a simple node which takes one event in parameter and pass it to the next node
	/// </summary>
	public abstract partial class LinearExecutionNode : BaseNode
	{
		[Input(name: "Executed")]
		public ExecutionLink executed;

		[Output(name: "Executes")]
		public ExecutionLink executes;


		public override bool MoveNext()
		{
			EnqueueExecutionPort(nameof(executes));
			return false;
		}
		// public override IEnumerable<ConditionalNode> CoroutineProcess()
		// {
		// 	var outputs = outputPorts.FirstOrDefault(n => n.fieldName == nameof(executes))
		// 		.GetEdges().Select(e => e.inputNode as ConditionalNode);
		// 	foreach (var n in outputs)
		// 	{
		// 		n.CoroutineProcess();
		// 		yield return n;
		// 	}
		// }
	}
}
