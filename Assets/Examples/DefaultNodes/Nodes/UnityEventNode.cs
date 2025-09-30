using GraphProcessor;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable, NodeMenuItem("Custom/Unity Event Node")]
public partial class UnityEventNode : BaseNode
{
	[Input(name: "In")]
	public float input;

	[Output(name: "Out")]
	public float output;

	public UnityEvent evt;

	public override string name => "Unity Event Node";
}
