using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine;

[System.Serializable, NodeMenuItem("Custom/TypeSwitchNode")]
public partial class TypeSwitchNode : BaseNode
{
	[Input]
	public string input;

	[SerializeField]
	public bool toggleType;

	public override string name => "TypeSwitchNode";

	// [CustomPortBehavior(nameof(input))]
	// IEnumerable<PortData> GetInputPort(List<SerializableEdge> edges)
	// {
	// 	yield return new PortData
	// 	{
	// 		identifier = "input",
	// 		displayName = "In",
	// 		displayType = (toggleType) ? typeof(float) : typeof(string)
	// 	};
	// }
}
