using GraphProcessor;

namespace BBBirder.Graphs
{
    [CompatibleWithGraph(typeof(BehaviorTree))]
    public abstract class BTNode : BaseNode
    {
        public NodeStatus Status { get; protected internal set; }

        protected virtual NodeStatus Run()
        {
            return NodeStatus.Success;
        }

        internal NodeStatus RunInternal()
        {
            if (Status is NodeStatus.Success or NodeStatus.Fault)
            {
                Reset();
            }

            return Status = Run();
        }

        public virtual void Reset()
        {
            Status = NodeStatus.Normal;
        }

        public virtual void Abort()
        {
            Reset();
        }
    }
}
