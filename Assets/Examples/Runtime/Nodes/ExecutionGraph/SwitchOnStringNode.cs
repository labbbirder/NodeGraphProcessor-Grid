using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine;

namespace BBBirder.Graphs
{
	[System.Serializable, NodeMenuItem("Control/Switch On String")]
	public partial class SwitchOnStringNode : EXNode
	{
		public override string name => "Switch On String";
		[Input]
		public ExecutionLink executed;

		[Input(name: "In")]
		public string input;

		[Output(name: "Out", hide: true)]
		public ExecutionLink output;

		[OnChange(nameof(ReloadPorts))]
		[SerializeField, ShowInInspector]
		private string[] options;

		NodePort defaultPort;
		Dictionary<string, NodePort> optionPorts = new();

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
						identifier = opt,
						displayName = $"\"{opt}\"",
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
