using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine;

[System.Serializable, NodeMenuItem("Custom/InheritanceBase")]
public partial class InheritanceBase : BaseNode
{
	[Input(name: "In Base")]
	public float input;

	[Output(name: "Out Base")]
	public float output;

	public float fieldBase;

	public override string name => "InheritanceBase";

	protected virtual void Process()
	{
		output = input * 42;
	}
}
