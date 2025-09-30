using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine;

[System.Serializable, NodeMenuItem("Custom/OutputNode")]
public partial class OutputNode : BaseNode
{
	[Input(name: "In")]
	public float input;

	public override string name => "OutputNode";

	// public override bool		deletable => false;
}
