using GraphProcessor;

namespace BBBirder.Graphs
{
    [System.Serializable, NodeMenuItem(DisplayName, typeof(BehaviorTree))]
    public partial class SelectorNode : BaseNode, IStartNode
    {
        const string DisplayName = "Selector";
        public override string name => DisplayName;

        [Input, Vertical]
        public ExecutionLink executed;

        [Output, Vertical]
        public ExecutionLink executes;

        // public override NodeStatus MoveNext()
        // {
        //     EnqueueExecutionPort(nameof(executes));
        //     return NodeStatus.Success;
        // }
    }
}
