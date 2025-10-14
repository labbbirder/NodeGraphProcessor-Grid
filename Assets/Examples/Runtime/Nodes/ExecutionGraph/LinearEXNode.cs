using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GraphProcessor;
using UnityEngine;

namespace BBBirder.Graphs
{

	[System.Serializable]
	/// <summary>
	/// This class represent a simple node which takes one event in parameter and pass it to the next node
	/// </summary>
	public abstract partial class LinearEXNode : EXNode
	{
		[Input(name: "Executed")]
		public ExecutionLink executed;

		[Output(name: "Executes")]
		public ExecutionLink executes;


		public override bool MoveNext()
		{
			EnqueueExecutionPort(nameof(executes));
			return false;
		}
	}
}
