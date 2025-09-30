using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine;

public abstract partial class AbstractNode : BaseNode
{
	[Input(name: "In")]
	public float input;

	[Output(name: "Out")]
	public float output;

	public override string name => "AbstractNode";

	// protected override void Process()
	// {
	// 	output = input * 42;
	// }
}

[System.Serializable, NodeMenuItem("Custom/Abstract Child1")]
public class AbstractNodeChild1 : AbstractNode { }
[System.Serializable, NodeMenuItem("Custom/Abstract Child2")]
public class AbstractNodeChild2 : AbstractNode { }
