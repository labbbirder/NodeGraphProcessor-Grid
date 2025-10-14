using GraphProcessor;

namespace BBBirder.Graphs
{
    [System.Serializable, NodeMenuItem("Start")]
    public partial class BTStartNode : BTNode, IEntryNode
    {
        public override string name => "Start";

        [Output(name: "Executes"), Vertical]
        public ExecutionLink executes;

    }
}
