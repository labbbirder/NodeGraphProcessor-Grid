using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine;

[System.Serializable, NodeMenuItem("Custom/List")]
public partial class ListNode : BaseNode
{
	[Output(name: "Out")]
	public Vector4 output;

	[Input(name: "In"), SerializeField]
	public Vector4 input;

	public List<GameObject> objs = new List<GameObject>();

	public override string name => "List";
}
