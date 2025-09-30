using System;
using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;

[System.Serializable, NodeMenuItem("Primitives/Float")]
public partial class FloatNode : BaseNode
{
    [Output("Out"), ShowAsDrawer]
    public float value;

    public override string name => "Float";

    protected override bool IsDataFlowDeterministic => true;
}
