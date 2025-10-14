using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GraphProcessor;

namespace BBBirder.Graphs
{
	[System.Serializable, NodeMenuItem("Start")]
	public partial class EXStartNode : EXNode, IEntryNode
	{
		public override string name => "Start";

		[Output(name: "Executes")]
		public ExecutionLink executes;

		public override bool MoveNext()
		{
			EnqueueExecutionPort(nameof(executes));
			return false;
		}
	}

}
