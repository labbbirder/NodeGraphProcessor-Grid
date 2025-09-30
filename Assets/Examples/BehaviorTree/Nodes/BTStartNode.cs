using GraphProcessor;

namespace BBBirder.Graphs
{
    [System.Serializable, NodeMenuItem("Start", typeof(BehaviorTree))]
    public partial class BTStartNode : BaseNode, IStartNode
    {
        public override string name => "Start";

        [Output(name: "Executes"), Vertical]
        public ExecutionLink executes;

        public override bool MoveNext()
        {
            EnqueueExecutionPort(nameof(executes));
            return false;
        }
    }
}
