using System.Linq;
using GraphProcessor;

namespace BBBirder.Graphs
{
    [System.Serializable, NodeMenuItem("Start")]
    public partial class BTStartNode : BTNode, IEntryNode
    {
        public override string name => "Start";

        [Output(name: "Executes"), Vertical]
        public ExecutionLink executes;

        protected override NodeStatus Run()
        {
            var n = outputPorts[0].GetEdges().FirstOrDefault()?.inputNode as BTNode;

            if (n == null) return NodeStatus.Success;

            return n.RunInternal();
        }

        public override void Reset()
        {
            base.Reset();
#if UNITY_EDITOR
            // 重置所有结点，方便观察状态
            foreach (var n in graph.nodes)
            {
                if (n is BTStartNode) continue;
                ; (n as BTNode).Reset();
            }
#endif
        }

        public override void Abort()
        {
            var n = outputPorts[0].GetEdges().FirstOrDefault()?.inputNode as BTNode;
            if (n.Status is NodeStatus.Running)
            {
                n.Abort();
            }

            Reset();
        }
    }
}
