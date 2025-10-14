using GraphProcessor;

namespace BBBirder.Graphs
{
    [System.Serializable, NodeMenuItem(DisplayName)]
    public partial class SelectorNode : BTNode
    {
        const string DisplayName = "Selector";
        public override string name => DisplayName;

        [Input, Vertical]
        public ExecutionLink executed;

        [Output, Vertical]
        public ExecutionLink executes;

        private int runningIndex;

        // public override NodeStatus MoveNext()
        // {
        //     var edges = outputPorts[0].GetEdges();
        //     if (runningIndex >= edges.Count || runningIndex < 0)
        //     {
        //         runningIndex = 0;
        //     }


        //     for (; runningIndex < edges.Count; runningIndex++)
        //     {
        //         var e = edges[runningIndex];
        //         NodeStatus status = e.inputNode.MoveNext();

        //         if (status is NodeStatus.Success)
        //         {
        //             return NodeStatus.Success;
        //         }
        //         else if (status is NodeStatus.Fault)
        //         {
        //             continue;
        //         }
        //         else if (status is NodeStatus.Running)
        //         {
        //             return NodeStatus.Running;
        //         }
        //     }
        // }

        // public override NodeStatus MoveNext()
        // {
        //     EnqueueExecutionPort(nameof(executes));
        //     return NodeStatus.Success;
        // }
    }
}
