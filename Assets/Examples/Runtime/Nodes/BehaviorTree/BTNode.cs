using GraphProcessor;

namespace BBBirder.Graphs
{
    [CompatibleWithGraph(typeof(BehaviorTree))]
    public abstract class BTNode : BaseNode
    {
        public NodeStatus Status { get; protected internal set; }
    }
}
