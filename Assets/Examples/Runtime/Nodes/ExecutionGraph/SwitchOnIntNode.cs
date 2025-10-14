using System;
using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;

namespace BBBirder.Graphs
{
	[System.Serializable, NodeMenuItem("Control/Switch On Int")]
	public partial class SwitchOnIntNode : EXNode
	{
		public override string name => "Switch On Int";
		[Input]
		public ExecutionLink executed;

		[Input(name: "In")]
		public int input;

		[Output(name: "Out", hide: true)]
		public ExecutionLink output;

		[OnChange(nameof(ReloadPorts))]
		[SerializeField, ShowInInspector]
		private int[] options;

		NodePort defaultPort;
		Dictionary<int, NodePort> optionPorts = new();


		protected override void LoadPorts()
		{
			base.LoadPorts();

			optionPorts.Clear();
			if (options != null)
			{
				foreach (var opt in options)
				{
					optionPorts[opt] = AddPort(false, $"output", new PortData()
					{
						identifier = opt.ToString(),
						displayName = opt.ToString(),
						displayType = typeof(ExecutionLink),
					});
				}
			}

			defaultPort = AddPort(false, $"output", new PortData()
			{
				identifier = "default",
				displayName = "default",
				displayType = typeof(ExecutionLink),
			});
		}

		public override bool MoveNext()
		{
			if (optionPorts.TryGetValue(input, out var port))
			{
				EnqueueExecutionPort(port);
			}
			else
			{
				EnqueueExecutionPort(defaultPort);
			}

			return false;
		}
	}
}
