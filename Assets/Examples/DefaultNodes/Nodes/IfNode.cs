using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using NodeGraphProcessor.Examples;
using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable, NodeMenuItem("Conditional/If"), NodeMenuItem("Conditional/Branch")]
public partial class IfNode : BaseNode
{
	[Input(name: "Executed")]
	public ExecutionLink executed;

	[Input(name: "Condition")]
	public bool condition;

	[Output(name: "True")]
	public ExecutionLink @true;

	[Output(name: "False")]
	public ExecutionLink @false;

	[Setting("Compare Function")]
	public CompareFunction compareOperator;

	public override string name => "If";

	public override bool MoveNext()
	{
		if (condition)
		{
			EnqueueExecutionPort(nameof(@true));
		}
		else
		{
			EnqueueExecutionPort(nameof(@false));
		}

		return false;
	}

	// public override IEnumerable<ConditionalNode> CoroutineProcess()
	// {
	// 	string fieldName = condition ? nameof(@true) : nameof(@false);
	// 	var outputs = outputPorts.FirstOrDefault(n => n.fieldName == fieldName)
	// 		.GetEdges().Select(e => e.inputNode as ConditionalNode);
	// 	foreach (var n in outputs)
	// 	{
	// 		yield return n;
	// 	}
	// }
}
