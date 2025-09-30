using GraphProcessor;

namespace BBBirder.Graphs
{
    [System.Serializable, NodeMenuItem(DisplayName, typeof(BehaviorTree))]
    public partial class SelectorNode : BaseNode, IStartNode
    {
        const string DisplayName = "Selector";
        public override string name => DisplayName;

        [Input(name: "Executed"), Vertical]
        public ExecutionLink executed;

        // public override bool MoveNext()
        // {
        //     EnqueueExecutionPort(nameof(executes));
        //     return false;
        // }
    }
}
