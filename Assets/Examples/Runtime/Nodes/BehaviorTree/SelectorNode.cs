using GraphProcessor;

namespace BBBirder.Graphs
{
    [System.Serializable, NodeMenuItem("Composites/" + DisplayName)]
    public partial class SelectorNode : BTNode
    {
        const string DisplayName = "Selector";
        public override string name => DisplayName;

        [Input, Vertical]
        public ExecutionLink executed;

        [Output, Vertical]
        public ExecutionLink executes;

        private int runningIndex;

        protected override NodeStatus Run()
        {
            var edges = outputPorts[0].GetEdges();

            if (edges.Count == 0) return NodeStatus.Success;

            if (runningIndex >= edges.Count || runningIndex < 0)
            {
                runningIndex = 0;
            }

            for (; runningIndex < edges.Count; runningIndex++)
            {
                var e = edges[runningIndex];
                NodeStatus status = (e.inputNode as BTNode).RunInternal();

                if (status is NodeStatus.Success)
                {
                    return NodeStatus.Success;
                }
                else if (status is NodeStatus.Fault)
                {
                    continue;
                }
                else if (status is NodeStatus.Running)
                {
                    return NodeStatus.Running;
                }
            }

            return NodeStatus.Fault;
        }

        public override void Reset()
        {
            runningIndex = 0;
            base.Reset();
        }

        public override void Abort()
        {
            var edges = outputPorts[0].GetEdges();
            if (runningIndex < edges.Count && runningIndex >= 0)
            {
                var e = edges[runningIndex];
                var n = e.inputNode as BTNode;
                if (n.Status is NodeStatus.Running)
                {
                    n.Abort();
                }
            }

            Reset();
        }
    }
}
