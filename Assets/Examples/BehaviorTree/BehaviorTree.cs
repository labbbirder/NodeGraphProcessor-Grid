using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using NodeGraphProcessor.Examples;
using UnityEngine;

namespace BBBirder.Graphs
{
    [Serializable]
    public class BehaviorTree : BaseGraph
    {
        protected override BaseNode EntryNode => startNodes.FirstOrDefault();
        [SerializeReference] private List<BaseNode> startNodes = new();

        protected override void OnGraphChanges(GraphChanges e)
        {
            if (e.addedNode is IStartNode)
            {
                startNodes.Add(e.addedNode);
            }

            if (e.removedNode is IStartNode)
            {
                startNodes.Remove(e.removedNode);
            }
        }

        protected override void BeforeSaveToDisk()
        {
            startNodes.Clear();
            foreach (var n in nodes)
            {
                if (n is IStartNode)
                {
                    startNodes.Add(n);
                }
            }

            base.BeforeSaveToDisk();
        }

        protected override void PostprocessNewNodePort(bool input, NodePort port)
        {
            port.portData.acceptMultipleEdges = true;
        }

    }
}
