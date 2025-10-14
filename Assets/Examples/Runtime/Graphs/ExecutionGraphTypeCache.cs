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
}
