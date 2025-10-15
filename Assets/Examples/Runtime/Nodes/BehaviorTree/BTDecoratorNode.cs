using System.Linq;
using GraphProcessor;

namespace BBBirder.Graphs
{
    public abstract class BTDecoratorNode : BTNode
    {
        public override string name => "Start";

        [Input, Vertical]
        public ExecutionLink executed;

        [Output, Vertical]
        public ExecutionLink executes;

        protected override NodeStatus Run()
        {
            var n = outputPorts[0].GetEdges().FirstOrDefault()?.inputNode as BTNode;

            if (n == null) return NodeStatus.Success;

            var inputStatus = n.RunInternal();
            if (inputStatus is NodeStatus.Running) return NodeStatus.Running;

            return Transform(inputStatus);
        }

        protected abstract NodeStatus Transform(NodeStatus input);

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
