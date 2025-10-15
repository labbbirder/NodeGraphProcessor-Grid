using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BBBirder;
using GraphProcessor;
using UnityEngine;

// [System.Serializable, NodeMenuItem("Conditional/Switch")]
public partial class SwitchNode : BaseNode
{
    [Input]
    public ExecutionLink executed;

    [Input(name: "In")]
    public float input;

    // [Polymorphic]
    [TypeHandleFilter(typeof(Enum))]
    public TypeHandle enumType;

    [Output(name: "Out", hide: true)]
    public ExecutionLink output;

    public override string name => "Switch";

    protected override void LoadPorts()
    {
        base.LoadPorts();
        Debug.Log("load ports " + enumType.Type);

        if (enumType.Type != null)
        {
            foreach (var e in Enum.GetNames(enumType.Type))
            {
                AddPort(false, $"output", new PortData()
                {
                    identifier = e.ToString(),
                    displayName = e.ToString(),
                    displayType = typeof(ExecutionLink),
                });
            }
        }
    }

}
