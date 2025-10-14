using GraphProcessor;

namespace BBBirder.Graphs
{
    [CompatibleWithGraph(typeof(BehaviorTree))]
    public abstract class BTNode : BaseNode
    {
        public NodeStatus Status { get; protected internal set; }

        internal protected virtual void Reset()
        {
            Status = NodeStatus.Normal;
        }
    }
}
