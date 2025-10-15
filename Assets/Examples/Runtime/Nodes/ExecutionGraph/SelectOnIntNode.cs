using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine;

namespace BBBirder.Graphs
{
    [System.Serializable, NodeMenuItem("Logic/Select On Int")]
    public partial class SelectOnIntNode : EXNode
    {
        [Input(name: "In")]
        public int input;

        [Input(hide: true)]
        public object selected;

        [Output(name: "Out")]
        public object result;

        [OnChange(nameof(ReloadPorts))]
        [SerializeField, ShowInInspector]
        private int[] options;

        NodePort defaultPort;
        Dictionary<int, NodePort> optionPorts = new();

        protected override bool PullDataManually => true;
        public override string name => "Select On Int";

        protected override void LoadPorts()
        {
            base.LoadPorts();

            optionPorts.Clear();
            if (options != null)
            {
                foreach (var opt in options)
                {
                    optionPorts[opt] = AddPort(true, $"selected", new PortData()
                    {
                        identifier = opt.ToString(),
                        displayName = opt.ToString(),
                    });
                }
            }

            defaultPort = AddPort(true, $"selected", new PortData()
            {
                identifier = "default",
                displayName = "default",
            });
        }

        protected override void AfterPullDatas()
        {
            PullPortData(nameof(input));

            selected = default;
            if (optionPorts.TryGetValue(input, out var port))
            {
                PullPortData(port);
            }
            else
            {
                PullPortData(defaultPort);
            }

            result = selected;
        }

    }
}
