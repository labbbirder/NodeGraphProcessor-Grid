using GraphProcessor;
using UnityEngine;

namespace BBBirder.Graphs
{
	[System.Serializable, NodeMenuItem("Math/Subtract")]
	public partial class SubtractNode : EXNode
	{
		[Input(name: "A")]
		public float inputA;
		[Input(name: "B")]
		public float inputB;

		[Output(name: "Out")]
		public float output;

		public override string name => "Sub";
	}
}
