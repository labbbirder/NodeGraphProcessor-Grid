using BBBirder;
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
		Label lblStart, lblEnd;

		public EdgeView() : base()
		{
			styleSheets.Add(ResUtils.Load<StyleSheet>(edgeStylePath));
			RegisterCallback<MouseDownEvent>(OnMouseDown);
			this.Add(lblEnd = new Label(""));
			this.Add(lblStart = new Label(""));
			SetLabelState(false, false);
			SetLabelState(true, false);
		}

		internal void UpdateLabels()
		{
			var edges = serializedEdge.inputPort.GetEdges();
			SetLabelState(true, edges.Count > 1);
			if (edges.Count > 1)
			{
				var i = edges.IndexOf(serializedEdge) + 1;
				lblStart.text = i.ToString();
			}


			edges = serializedEdge.outputPort.GetEdges();
			SetLabelState(false, edges.Count > 1);
			if (edges.Count > 1)
			{
				var i = edges.IndexOf(serializedEdge) + 1;
				lblEnd.text = i.ToString();
			}
		}

		private void SetLabelState(bool start, bool displayState)
		{
			; (start ? lblStart : lblEnd).style.display =
				displayState ? DisplayStyle.Flex : DisplayStyle.None;
		}

		public override bool UpdateEdgeControl()
		{
			var result = base.UpdateEdgeControl();
			var vstart = (output as PortView)?.portData.vertical ?? false;
			var vend = (input as PortView)?.portData.vertical ?? false;
			lblStart.style.left = vstart ? edgeControl.from.x : edgeControl.from.x + 16;
			lblStart.style.top = vstart ? edgeControl.from.y : edgeControl.from.y;
			lblEnd.style.left = vend ? edgeControl.to.x : edgeControl.to.x - 16 - lblEnd.localBound.width;
			lblEnd.style.top = vend ? edgeControl.to.y - 16 : edgeControl.to.y;
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

#warning TODO: fix relay node
				// owner.AddRelayNode(input as PortView, output as PortView, mousePos);
			}
		}
	}
}
