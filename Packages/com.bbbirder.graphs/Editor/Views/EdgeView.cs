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
			this.Add(lblEnd = new Label("hi"));
			this.Add(lblStart = new Label("hi"));
			SetLabelState(false, false);
			SetLabelState(true, false);
		}

		internal void UpdateIndex(bool start, int i)
		{
			if (i == -1)
			{
				SetLabelState(start, false);
				; (start ? lblStart : lblEnd).text = "";
			}
			else
			{
				SetLabelState(start, true);
				; (start ? lblStart : lblEnd).text = i.ToString();
			}
		}

		private void SetLabelState(bool start, bool displayState)
		{
			; (start ? lblStart : lblEnd).style.display =
				displayState ? DisplayStyle.Flex : DisplayStyle.None;
		}

		internal void UpdateEdgeControlBase()
		{
			base.UpdateEdgeControl();
		}

		public override bool UpdateEdgeControl()
		{
			var result = base.UpdateEdgeControl();
			lblStart.style.left = edgeControl.from.x + 16;
			lblStart.style.top = edgeControl.from.y;
			lblEnd.style.left = edgeControl.to.x - 16 - lblEnd.localBound.width;
			lblEnd.style.top = edgeControl.to.y;
			; (input as PortView)?.UpdatePortSort();
			; (output as PortView)?.UpdatePortSort();
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
