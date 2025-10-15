using System.Linq;
using GraphProcessor;

namespace BBBirder.Graphs
{
    [System.Serializable, NodeMenuItem("Decorators/" + DisplayName)]
    public class BTInvert : BTDecoratorNode
    {
        const string DisplayName = "Invert";
        public override string name => DisplayName;

        protected override NodeStatus Transform(NodeStatus input)
        {
            return input switch
            {
                NodeStatus.Success => NodeStatus.Fault,
                NodeStatus.Fault => NodeStatus.Success,
                _ => NodeStatus.Normal,
            };
        }
    }
}
