using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine;

namespace BBBirder.Graphs
{
    [Serializable]
    public class ExecutionGraph : BaseGraph
    {
        private EXNode topExecutingNode;
        private Stack<EXNode> executingNodes = new();
        private HashSet<EXNode> hashExecutingNodes = new();
        private Queue<EXNode> pushingNodes = new();

        public override bool IsRunning => executingNodes.Count != 0;

        protected override NodeStatus GetNodeStatus(BaseNode node) => hashExecutingNodes.Contains(node) ? NodeStatus.Running : NodeStatus.Normal;

        public override NodeStatus MoveNext()
        {
            if (executingNodes.Count == 0)
            {
                var entryNode = EntryNode as EXNode;
                executingNodes.Push(entryNode);
                hashExecutingNodes.Add(entryNode);
                entryNode.Enter();
                return NodeStatus.Success;
            }

            TryPopExecutingNode(out var n);

            NodeStatus status = NodeStatus.Normal;
            if (n.HasCustomMoveNext)
            {
                PullDataRecursively(n);
                try
                {
                    topExecutingNode = n;
                    var isRunning = n.MoveNext();
                    status = isRunning ? NodeStatus.Running : NodeStatus.Success;
                    if (isRunning)
                    {
                        PushExecutingNode(n);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    n.AddMessage(e.Message, NodeMessageType.Error);
                    status = NodeStatus.Fault;
                }
                finally
                {
                    topExecutingNode = null;
                }

                DrainPushingNodes();
            }

            return status;
        }

        public override void Stop()
        {
            topExecutingNode = null;
            pushingNodes.Clear();
            executingNodes.Clear();
            hashExecutingNodes.Clear();
            foreach (var n in nodes)
            {
                n.ClearMessages();
            }

            NotifyExecutionStateChanged();
        }

        internal void PushExecutingNode(EXNode node)
        {
            // top executing node push self should not call Enter() again.
            if (node != null && topExecutingNode == node)
            {
                executingNodes.Push(node);
                hashExecutingNodes.Add(node);
            }
            else
            {
                pushingNodes.Enqueue(node);
            }
        }

        internal void PushExecutingPort(NodePort outputPort)
        {
            var edges = outputPort.GetEdges();
#if DEBUG
            if (outputPort.fieldInfo != null && outputPort.fieldInfo.FieldType != typeof(ExecutionLink))
            {
                throw new($"field {outputPort.fieldName} must be ExecutionLink");
            }
#endif
            if (edges.Count > 0)
            {
                PushExecutingNode(edges[0].inputNode as EXNode);
            }
        }

        private void DrainPushingNodes()
        {
            while (pushingNodes.TryDequeue(out var node))
            {
                if (hashExecutingNodes.Contains(node))
                {
                    while (executingNodes.TryPeek(out var top) && top != node)
                    {
                        executingNodes.Pop();
                    }
                }
                else
                {
                    executingNodes.Push(node);
                    hashExecutingNodes.Add(node);
                    try
                    {
                        if (node.HasCustomEnter)
                        {
                            PullDataRecursively(node);
                            node.Enter();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }

        public bool TryPopExecutingNode(out EXNode node)
        {
            if (executingNodes.Count > 0)
            {
                node = executingNodes.Pop();
                hashExecutingNodes.Remove(node);
                return true;
            }
            else
            {
                node = null;
                return false;
            }
        }

        protected override void PostprocessNewNodePort(bool input, NodePort port)
        {
            var type = port.portData.displayType ?? port.fieldInfo.FieldType;
            if (type == typeof(ExecutionLink))
            {
                port.portData.acceptMultipleEdges = input;
            }
            else
            {
                if (input)
                {
                    port.portData.acceptMultipleEdges = port.portData.unpack;
                    if (port.portData.unpack)
                    {
                        port.portData.displayType = RuntimeTypeCache.GetUnpackedElementType(type);
                    }
                }
                else
                {
                    port.portData.acceptMultipleEdges = true;
                }
            }
        }

    }
}
