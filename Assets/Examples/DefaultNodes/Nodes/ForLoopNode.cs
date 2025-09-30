using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using NodeGraphProcessor.Examples;
using UnityEngine;

[System.Serializable, NodeMenuItem("Conditional/ForLoop")]
public partial class ForLoopNode : BaseNode
{
    [Input(name: "Executed", allowMultiple: true)]
    public ExecutionLink executed;

    [Output(name: "Loop Body")]
    public ExecutionLink loopBody;

    [Output(name: "Completed")]
    public ExecutionLink loopCompleted;


    [Output]
    public int index;

    [Input, ShowAsDrawer]
    public int start = 0;

    [Input, ShowAsDrawer]
    public int end = 10;

    public override string name => "ForLoop";

    public override void Enter()
    {
        index = start - 1;
    }

    public override bool MoveNext()
    {
        if (index++ >= end)
        {
            EnqueueExecutionPort(nameof(loopCompleted));
            return false;
        }
        else
        {
            EnqueueExecutionPort(nameof(loopBody));
            return true;
        }
    }


    // public override IEnumerable<ConditionalNode> CoroutineProcess()
    // {
    //     for (index = start; index < end; index++)
    //     {
    //         foreach (var n in GetExecutedNodesLoopBody())
    //         {
    //             yield return n;
    //         }
    //     }

    //     foreach (var n in GetExecutedNodesLoopCompleted())
    //     {
    //         yield return n;
    //     }
    // }

    // public IEnumerable<ConditionalNode> GetExecutedNodesLoopBody()
    // {
    //     // Return all the nodes connected to the executes port
    //     return outputPorts.FirstOrDefault(n => n.fieldName == nameof(loopBody))
    //         .GetEdges().Select(e => e.inputNode as ConditionalNode);
    // }

    // public IEnumerable<ConditionalNode> GetExecutedNodesLoopCompleted()
    // {
    //     // Return all the nodes connected to the executes port
    //     return outputPorts.FirstOrDefault(n => n.fieldName == nameof(loopCompleted))
    //         .GetEdges().Select(e => e.inputNode as ConditionalNode);
    // }
}
