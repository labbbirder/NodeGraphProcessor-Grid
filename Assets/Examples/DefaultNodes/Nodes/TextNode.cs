using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine;

[System.Serializable, NodeMenuItem("Primitives/Text")]
public partial class TextNode : BaseNode
{
	[Output(name: "Label"), SerializeField]
	public string output;

	public override string name => "Text";
}
