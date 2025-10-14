using System;
using GraphProcessor;

namespace BBBirder.Graphs
{
    [Serializable]
    public class BehaviorTree : BaseGraph
    {
        // public override bool MoveNext()
        // {
        //     return base.MoveNext();
        // }
        public override NodeStatus MoveNext()
        {
            throw new NotImplementedException();
        }

        protected override NodeStatus GetNodeStatus(BaseNode node)
        {
            return (node as BTNode).Status;
        }

        public override void Stop()
        {
            foreach (var node in nodes)
            {
                ; (node as BTNode)?.Reset();
            }
        }

        protected override void PostprocessNewNodePort(bool input, NodePort port)
        {
            port.portData.acceptMultipleEdges = port.owner is not IEntryNode && !input;
        }
    }
}
