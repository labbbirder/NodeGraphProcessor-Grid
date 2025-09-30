using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine;

[System.Serializable, NodeMenuItem("Operations/Subtract")]
public partial class SubtractNode : BaseNode
{
	[Input(name: "A")]
	public float inputA;
	[Input(name: "B")]
	public float inputB;

	[Output(name: "Out")]
	public float output;

	public override string name => "Sub";
}
