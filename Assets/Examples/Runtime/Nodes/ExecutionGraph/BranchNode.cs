using GraphProcessor;
using UnityEngine;

namespace BBBirder.Graphs
{
	[System.Serializable, NodeMenuItem("Conditional/If")]
	public partial class BranchNode : EXNode
	{
		[Input(name: "Executed")]
		public ExecutionLink executed;

		[Input(name: "Condition")]
		public bool condition;

		[Output(name: "True")]
		public ExecutionLink @true;

		[Output(name: "False")]
		public ExecutionLink @false;

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
	}
}
