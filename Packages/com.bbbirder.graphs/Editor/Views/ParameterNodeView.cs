using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[NodeCustomEditor(typeof(ParameterNode))]
public class ParameterNodeView : BaseNodeView
{
    ParameterNode parameterNode;

    public override void Enable(bool fromInspector = false)
    {
        parameterNode = nodeTarget as ParameterNode;

        EnumField accessorSelector = new EnumField(parameterNode.accessor);
        accessorSelector.SetValueWithoutNotify(parameterNode.accessor);
        accessorSelector.RegisterValueChangedCallback(evt =>
        {
            parameterNode.accessor = (ParameterAccessor)evt.newValue;
            UpdatePort();
            controlsContainer.MarkDirtyRepaint();
            // ForceUpdatePorts();
        });

        UpdatePort();
        controlsContainer.Add(accessorSelector);

        //    Find and remove expand/collapse button
        titleContainer.Remove(titleContainer.Q("title-button-container"));
        //    Remove Port from the #content
        topContainer.parent.Remove(topContainer);
        //    Add Port to the #title
        titleContainer.Add(topContainer);

        parameterNode.onParameterChanged += UpdateView;
        UpdateView();
    }

    void UpdateView()
    {
        title = parameterNode.parameter?.name;
    }

    void UpdatePort()
    {
        if (parameterNode.accessor == ParameterAccessor.Set)
        {
            titleContainer.AddToClassList("input");
        }
        else
        {
            titleContainer.RemoveFromClassList("input");
        }
    }
}
