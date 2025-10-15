using System;
using GraphProcessor;

namespace BBBirder.Graphs
{
    [Serializable]
    public class BehaviorTree : BaseGraph
    {
        public override bool IsRunning => (EntryNode as BTNode)?.Status == NodeStatus.Running;

        public override NodeStatus MoveNext()
        {
            return (EntryNode as BTNode)?.RunInternal() ?? NodeStatus.Normal;
        }

        public void Abort()
        {
            ; (EntryNode as BTNode)?.Abort();
        }

        protected override NodeStatus GetNodeStatus(BaseNode node)
        {
            return (node as BTNode).Status;
        }

        public override void Stop()
        {
            base.Stop();
            foreach (var node in nodes)
            {
                ; (node as BTNode)?.Reset();
            }
        }

        protected override void PostprocessNewNodePort(bool input, NodePort port)
        {
            port.portData.acceptMultipleEdges = port.owner
                is not IEntryNode and not BTDecoratorNode
                && !input;
        }
    }
}
