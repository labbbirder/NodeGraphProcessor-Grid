using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GraphProcessor;

namespace BBBirder.Graphs
{
	public interface IStartNode { }
	[System.Serializable, NodeMenuItem("Start", typeof(ExecutionGraph))]
	public partial class StartNode : BaseNode, IStartNode
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
