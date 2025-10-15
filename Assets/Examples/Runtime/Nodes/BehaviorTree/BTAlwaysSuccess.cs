using System.Linq;
using GraphProcessor;

namespace BBBirder.Graphs
{
    [System.Serializable, NodeMenuItem("Decorators/" + DisplayName)]
    public class BTAlwaysSuccess : BTDecoratorNode
    {
        const string DisplayName = "Always Success";
        public override string name => DisplayName;

        protected override NodeStatus Transform(NodeStatus input)
        {
            return input switch
            {
                NodeStatus.Success or NodeStatus.Fault => NodeStatus.Success,
                _ => NodeStatus.Normal,
            };
        }
    }
}
