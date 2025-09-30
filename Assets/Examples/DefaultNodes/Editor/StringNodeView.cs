using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[NodeCustomEditor(typeof(StringNode))]
public class StringNodeView : BaseNodeView
{
	public override void Enable()
	{
		var node = nodeTarget as StringNode;

		var textArea = new TextField(-1, true, false, '*') { value = node.output };
		textArea.Children().First().style.unityTextAlign = TextAnchor.UpperLeft;
		textArea.style.width = 120;
		textArea.style.height = 50;
		textArea.RegisterValueChangedCallback(v =>
		{
			GraphView.RegisterCompleteObjectUndo("Edit string node");
			node.output = v.newValue;
		});
		controlsContainer.Add(textArea);
	}
}
