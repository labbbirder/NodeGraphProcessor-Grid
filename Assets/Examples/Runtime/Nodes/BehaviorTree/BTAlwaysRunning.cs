using System.Linq;
using GraphProcessor;

namespace BBBirder.Graphs
{
    [System.Serializable, NodeMenuItem("Decorators/" + DisplayName)]
    public class BTAlwaysRunning : BTDecoratorNode
    {
        const string DisplayName = "Always Running";
        public override string name => DisplayName;

        protected override NodeStatus Transform(NodeStatus input)
        {
            return input switch
            {
                NodeStatus.Normal => NodeStatus.Normal,
                _ => NodeStatus.Running,
            };
        }
    }
}
