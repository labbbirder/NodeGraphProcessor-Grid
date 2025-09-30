using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphProcessor
{
	public class BlackboardFieldPropertyView : VisualElement
	{
		protected BaseGraphView baseGraphView;

		public ExposedParameter parameter { get; private set; }

		public Toggle hideInInspector { get; private set; }

		public BlackboardFieldPropertyView(BaseGraphView graphView, ExposedParameter param)
		{
			baseGraphView = graphView;
			parameter = param;

			var field = graphView.exposedParameterFactory.GetParameterSettingsField(param, (newValue) =>
			{
				param.settings = newValue as ExposedParameter.Settings;
			});

			Add(field);
		}
	}
}