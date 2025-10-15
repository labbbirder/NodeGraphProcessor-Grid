using GraphProcessor;

namespace BBBirder.Graphs
{
    [System.Serializable, NodeMenuItem("Composites/" + DisplayName)]
    public partial class ParallelNode : BTNode
    {
        public enum SuccessCondition
        {
            AnySuccess,
            SuccessReachThreshold,
            AllSuccess,
        }

        const string DisplayName = "Parallel";
        public override string name => DisplayName;

        [Input, Vertical]
        public ExecutionLink executed;

        [Output, Vertical]
        public ExecutionLink executes;

        public SuccessCondition successCondition;

        [VisibleIf(nameof(successCondition), SuccessCondition.SuccessReachThreshold)]
        public int threshold = 3;

        protected override NodeStatus Run()
        {
            var edges = outputPorts[0].GetEdges();

            if (edges.Count == 0) return NodeStatus.Success;

            int nRunning = 0;
            int nSuccess = 0;
            if (Status is NodeStatus.Normal)
            {
                foreach (var e in edges)
                {
                    NodeStatus status = (e.inputNode as BTNode).RunInternal();
                    if (status is NodeStatus.Success)
                    {
                        nSuccess++;
                    }
                    else if (status is NodeStatus.Running)
                    {
                        nRunning++;
                    }
                }
            }
            else if (Status is NodeStatus.Running)
            {
                foreach (var e in edges)
                {
                    var n = e.inputNode as BTNode;
                    if (n.Status is NodeStatus.Running)
                    {
                        n.RunInternal();
                    }

                    if (n.Status is NodeStatus.Success)
                    {
                        nSuccess++;
                    }
                    else if (n.Status is NodeStatus.Running)
                    {
                        nRunning++;
                    }
                }
            }

            var resultStatus = successCondition switch
            {
                SuccessCondition.AnySuccess =>
                    nSuccess > 0 ? NodeStatus.Success : NodeStatus.Running,
                SuccessCondition.SuccessReachThreshold =>
                    nSuccess >= threshold ? NodeStatus.Success
                    : nRunning > 0 ? NodeStatus.Running
                    : NodeStatus.Fault,
                SuccessCondition.AllSuccess =>
                    nSuccess >= edges.Count ? NodeStatus.Success : NodeStatus.Running,
                _ => NodeStatus.Success,
            };

            if (resultStatus != NodeStatus.Running)
            {
                Abort();
            }

            return resultStatus;
        }

        public override void Abort()
        {
            var edges = outputPorts[0].GetEdges();
            foreach (var e in edges)
            {
                var n = e.inputNode as BTNode;
                if (n.Status is NodeStatus.Running)
                {
                    n.Abort();
                }
            }

            Reset();
        }
    }
}
