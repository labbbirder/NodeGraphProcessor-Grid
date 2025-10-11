using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BBBirder.Graphs;
using GraphProcessor;
using UnityEngine;

[System.Serializable, NodeMenuItem("Custom/Add2")]
public partial class Add2 : BaseNode
{
    [Input] public float a;
    [Input] public float b;

    [Output] public float sum;

    public override void Enter()
    {
        sum = a + b;
    }
}

[System.Serializable, NodeMenuItem("Custom/MultiAdd")]
public partial class MultiAddNode : BaseNode
{
    [Input(unpack: true)]
    public float[] inputs;

    [Output, ShowAsDrawer]
    public float output;

    public override string name => "Add";

    // protected override void Process()
    // {
    //     output = 0;

    //     if (inputs == null)
    //         return;

    //     foreach (float input in inputs)
    //         output += input;
    // }

    protected override void AfterPullDatas()
    {
        output = 0;
        foreach (var n in inputs)
        {
            output += n;
        }
    }

    // [CustomPortBehavior(nameof(inputs))]
    // IEnumerable<PortData> GetPortsForInputs(List<SerializableEdge> edges)
    // {
    //     yield return new PortData { displayName = "In ", displayType = typeof(float), acceptMultipleEdges = true };
    // }

    // [CustomPortInput(nameof(inputs), typeof(float), allowCast = true)]
    // public void GetInputs(List<SerializableEdge> edges)
    // {
    //     inputs = edges.Select(e => RuntimeConverter.Convert<float>(e.passThroughBuffer));
    // }

    // mark to inputs attribute
    public void SetInputValue(float value, int index)
    {

    }
}
