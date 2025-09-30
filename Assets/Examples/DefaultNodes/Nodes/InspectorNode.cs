using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine;

[System.Serializable, NodeMenuItem("Custom/InspectorNode")]
public partial class InspectorNode : BaseNode
{
	[Input(name: "In")]
	public float input;

	[Output(name: "Out")]
	public float output;

	[ShowInInspector]
	public bool additionalSettings;
	[ShowInInspector]
	public string additionalParam;

	public override string name => "InspectorNode";

}
