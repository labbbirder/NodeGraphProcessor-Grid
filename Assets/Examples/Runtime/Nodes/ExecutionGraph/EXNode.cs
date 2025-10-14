using GraphProcessor;

namespace BBBirder.Graphs
{
    [CompatibleWithGraph(typeof(ExecutionGraph))]
    public abstract class EXNode : BaseNode
    {
        ExecutionGraphTypeCache.NodeInfo info;

        internal bool HasCustomEnter => info.hasCustomEnter;
        internal bool HasCustomMoveNext => info.hasCustomMoveNext;
        public ExecutionGraph Graph => graph as ExecutionGraph;

        public EXNode()
        {
            info = ExecutionGraphTypeCache.GetNodeInfo(GetType());
        }

        /// <summary>
        /// Called when the node is added into executing list
        /// </summary>
        public virtual void Enter() { }

        /// <summary>
        /// Called when the node is being executing
        /// </summary>
        /// <returns></returns>
        public virtual bool MoveNext() { return false; }

        public void EnqueueExecutionPort(string portName)
        {
            if (!outputPorts.TryGetPorts(portName, out var ports))
            {
                throw new($"field {portName} not found in {this}");
            }

            if (ports.Count > 0)
            {
                Graph.PushExecutingPort(ports[0]);
            }
        }

        public virtual void EnqueueExecutionNode(EXNode node)
        {
            Graph.PushExecutingNode(node);
        }

        public virtual void EnqueueExecutionPort(NodePort outputPort)
        {
            Graph.PushExecutingPort(outputPort);
        }

    }
}
