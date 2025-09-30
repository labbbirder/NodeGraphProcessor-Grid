using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphProcessor
{
	public class EdgeView : Edge
	{
		const string edgeStylePath = "../res/Styles/EdgeView.uss";

		public bool isConnected = false;

		public SerializableEdge serializedEdge { get { return userData as SerializableEdge; } }

		protected BaseGraphView owner => ((input ?? output) as PortView).owner.GraphView;
		Label label;

		public EdgeView() : base()
		{
			styleSheets.Add(ResUtils.Load<StyleSheet>(edgeStylePath));
			RegisterCallback<MouseDownEvent>(OnMouseDown);
			this.Add(label = new Label("hi"));
			// SetLabelState(false);
		}

		private void SetLabelState(bool displayState)
		{
			label.style.display = displayState ? DisplayStyle.Flex : DisplayStyle.None;
		}

		public override bool UpdateEdgeControl()
		{
			var result = base.UpdateEdgeControl();
			label.style.left = edgeControl.from.x + 16;
			label.style.top = edgeControl.from.y;
			return result;
		}

		public override void OnPortChanged(bool isInput)
		{
			base.OnPortChanged(isInput);
			UpdateEdgeSize();
		}

		public void UpdateEdgeSize()
		{
			if (input == null && output == null)
				return;

			PortData inputPortData = (input as PortView)?.portData;
			PortData outputPortData = (output as PortView)?.portData;

			for (int i = 1; i < 20; i++)
				RemoveFromClassList($"edge_{i}");
			int maxPortSize = Mathf.Max(inputPortData?.sizeInPixel ?? 0, outputPortData?.sizeInPixel ?? 0);
			if (maxPortSize > 0)
				AddToClassList($"edge_{Mathf.Max(1, maxPortSize - 6)}");
		}

		protected override void OnCustomStyleResolved(ICustomStyle styles)
		{
			base.OnCustomStyleResolved(styles);

			UpdateEdgeControl();
		}

		void OnMouseDown(MouseDownEvent e)
		{
			if (e.clickCount == 2)
			{
				// Empirical offset:
				var position = e.mousePosition;
				position += new Vector2(-10f, -28);
				Vector2 mousePos = owner.ChangeCoordinatesTo(owner.contentViewContainer, position);

				owner.AddRelayNode(input as PortView, output as PortView, mousePos);
			}
		}
	}
}
