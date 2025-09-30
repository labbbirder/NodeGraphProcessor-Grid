using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[NodeCustomEditor(typeof(TypeSwitchNode))]
public class TypeSwitchNodeView : BaseNodeView
{
	public override void Enable()
	{
		var node = nodeTarget as TypeSwitchNode;

		var t = new Toggle("Swith type") { value = node.toggleType };
		t.RegisterValueChangedCallback(e =>
		{
			node.toggleType = e.newValue;
			// ForceUpdatePorts();
		});

		controlsContainer.Add(t);
	}
}
