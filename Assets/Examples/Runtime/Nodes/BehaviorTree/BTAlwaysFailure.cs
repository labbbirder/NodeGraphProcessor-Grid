using System.Linq;
using GraphProcessor;

namespace BBBirder.Graphs
{
    [System.Serializable, NodeMenuItem("Decorators/" + DisplayName)]
    public class BTAlwaysFailure : BTDecoratorNode
    {
        const string DisplayName = "Always Failure";
        public override string name => DisplayName;

        protected override NodeStatus Transform(NodeStatus input)
        {
            return input switch
            {
                NodeStatus.Success or NodeStatus.Fault => NodeStatus.Fault,
                _ => NodeStatus.Normal,
            };
        }
    }
}
