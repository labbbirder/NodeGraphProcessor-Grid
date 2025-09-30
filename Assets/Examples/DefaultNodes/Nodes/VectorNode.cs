using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine;

[System.Serializable, NodeMenuItem("Custom/Vector")]
public partial class VectorNode : BaseNode
{
	[Output(name: "Out")]
	public Vector4 output;

	[Input(name: "In"), SerializeField]
	public Vector4 input;

	public override string name => "Vector";
}
