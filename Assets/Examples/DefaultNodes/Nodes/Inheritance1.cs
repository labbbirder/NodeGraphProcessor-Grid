using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine;

[System.Serializable, NodeMenuItem("Custom/Inheritance1")]
public partial class Inheritance1 : InheritanceBase
{
	[Input(name: "In 1")]
	public float input1;

	[Output(name: "Out 1")]
	public float output1;

	public float field1;

	public override string name => "Inheritance1";

	protected override void Process()
	{
		output = input * 42;
	}
}
