using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine;

[System.Serializable, NodeMenuItem("Custom/MessageNode2")]
public partial class MessageNode2 : BaseNode
{
	[Input(name: "In")]
	public float input;

	[Output(name: "Out")]
	public float output;

	public override string name => "MessageNode2";

}
