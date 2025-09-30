using GraphProcessor;
using UnityEngine;

[System.Serializable, NodeMenuItem("Custom/Renamable")]
public partial class RenamableNode : BaseNode
{
	[Output("Out")]
	public float output;

	[Input("In")]
	public float input;

	public override string name => "Renamable";

	public override bool isRenamable => true;
}