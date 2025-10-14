using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine;
using static GraphProcessor.RuntimeTypeCache;

namespace BBBirder.Graphs
{
    static class ExecutionGraphTypeCache
    {
        public class NodeInfo
        {
            public bool hasCustomEnter;
            public bool hasCustomMoveNext;
        }

        public static NodeInfo GetNodeInfo(Type nodeType)
        {
            if (!s_exNodeInfos.TryGetValue(nodeType, out var info))
            {
                s_exNodeInfos[nodeType] = info = new()
                {
                    hasCustomEnter = IsMethodOverrided(nodeType, nameof(EXNode.Enter), typeof(BaseNode)),
                    hasCustomMoveNext = IsMethodOverrided(nodeType, nameof(EXNode.MoveNext), typeof(BaseNode)),
                };
            }

            return info;
        }

        private static Dictionary<Type, NodeInfo> s_exNodeInfos = new();
    }

    [Serializable]
    public class ExecutionGraph : BaseGraph
    {
        private Stack<EXNode> executingNodes = new();
        private HashSet<EXNode> hashExecutingNodes = new();
        private Queue<EXNode> pushingNodes = new();

        internal void PushExecutingNode(EXNode node)
        {
            pushingNodes.Enqueue(node);
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

        protected override NodeStatus GetNodeStatus(BaseNode node) => hashExecutingNodes.Contains(node) ? NodeStatus.Running : NodeStatus.Normal;

        public override void Run()
        {
            const int MAX_ITERATION_COUNT = 200;
            var iter = 0;
            while (MoveNext())
            {
                if (iter++ > MAX_ITERATION_COUNT)
                {
                    Debug.LogError($"execution iteration exceeds limits {MAX_ITERATION_COUNT}.");
                    break;
                }
            }
        }

        public override bool MoveNext()
        {
            if (executingNodes.Count == 0)
            {
                var entryNode = EntryNode as EXNode;
                executingNodes.Push(entryNode);
                hashExecutingNodes.Add(entryNode);
                entryNode.Enter();
                NotifyExecutionStateChanged();
                return true;
            }

            var n = executingNodes.Peek();

            if (n.HasCustomMoveNext)
            {
                PullDataRecursively(n);
                try
                {
                    var reenter = n.MoveNext();
                    if (!reenter)
                    {
                        TryPopExecutingNode(out _);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

                DrainPushingNodes();
            }
            else
            {
                TryPopExecutingNode(out _);
            }

            NotifyExecutionStateChanged();
            return executingNodes.Count != 0;
        }

        public override void Stop()
        {
            pushingNodes.Clear();
            executingNodes.Clear();
            hashExecutingNodes.Clear();
            NotifyExecutionStateChanged();
        }

    }
}
