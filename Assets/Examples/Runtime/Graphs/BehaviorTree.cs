using System;
using GraphProcessor;

namespace BBBirder.Graphs
{
    [Serializable]
    public class BehaviorTree : BaseGraph
    {
        public override bool MoveNext()
        {
            return base.MoveNext();
        }

        protected override NodeStatus GetNodeStatus(BaseNode node)
        {
            return (node as BTNode).Status;
        }

        protected override void PostprocessNewNodePort(bool input, NodePort port)
        {
            port.portData.acceptMultipleEdges = port.owner is not IEntryNode && !input;
        }
    }
}
