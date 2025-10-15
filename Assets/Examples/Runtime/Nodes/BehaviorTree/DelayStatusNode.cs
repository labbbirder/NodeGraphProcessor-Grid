using GraphProcessor;
using UnityEditor;
using UnityEngine;

namespace BBBirder.Graphs
{
    [System.Serializable, NodeMenuItem(DisplayName)]
    public partial class DelayStatusNode : BTNode
    {
        const string DisplayName = "Delay Status";

        [Input, Vertical]
        public ExecutionLink executed;

        public NodeStatus result;
        public float delay = 1;
        float expireTime;

        protected override NodeStatus Run()
        {
            if (Status != NodeStatus.Running)
            {
                expireTime = (float)EditorApplication.timeSinceStartup + delay;
            }

            if ((float)EditorApplication.timeSinceStartup < expireTime)
            {
                return NodeStatus.Running;
            }
            else
            {
                return result;
            }
        }

    }
}
