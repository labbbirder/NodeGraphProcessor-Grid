using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphProcessor
{
	public class BlackboardFieldView : BlackboardField
	{
		protected BaseGraphView graphView;

		public ExposedParameter parameter { get; private set; }

		public BlackboardFieldView(BaseGraphView graphView, ExposedParameter param) : base(null, param.name, param.shortType)
		{
			this.graphView = graphView;
			parameter = param;
			this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
			this.Q("icon").AddToClassList("parameter-" + param.shortType);
			this.Q("icon").visible = true;

			; (this.Q("textField") as TextField).RegisterValueChangedCallback((e) =>
			{
				param.name = e.newValue;
				text = e.newValue;
				graphView.graph.UpdateExposedParameterName(param, e.newValue);
			});
		}

		void BuildContextualMenu(ContextualMenuPopulateEvent evt)
		{
			evt.menu.AppendAction("Rename", (a) => OpenTextEditor(), DropdownMenuAction.AlwaysEnabled);
			evt.menu.AppendAction("Delete", (a) => graphView.graph.RemoveExposedParameter(parameter), DropdownMenuAction.AlwaysEnabled);

			evt.StopPropagation();
		}
	}
}