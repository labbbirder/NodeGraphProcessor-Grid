using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine;

[System.Serializable, NodeMenuItem("String")]
public partial class StringNode : BaseNode
{
	[Output(name: "Out"), SerializeField]
	public string output;
	public override bool resizable => true;
	public override string name => "String";
}
