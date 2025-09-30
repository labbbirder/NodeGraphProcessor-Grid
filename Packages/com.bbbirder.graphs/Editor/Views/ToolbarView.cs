using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphProcessor
{
    public class ToolbarView : OverlayView, ICreateVerticalToolbar, ICreateHorizontalToolbar
    {
        protected const string Id = "GraphProcessor-Toolbar";

        public OverlayToolbar CreateHorizontalToolbarContent()
        {
            return CreateToolbar();
        }

        public override OverlayToolbar CreateVerticalToolbarContent()
        {
            return CreateToolbar();
        }

        protected virtual OverlayToolbar CreateToolbar()
        {
            var root = new OverlayToolbar();
            var btnBlackboard = new EditorToolbarToggle(ResUtils.Load<Texture2D>("../res/Icons/codicon--settings.png"))
            {
                tooltip = "Blackboard View",
            };
            btnBlackboard.RegisterValueChangedCallback(e =>
            {
                Window.SetOverlayDisplayState(BlackboardView.Id, e.newValue);
            });
            btnBlackboard.SetValueWithoutNotify(Window.GetOverlayDisplayState(BlackboardView.Id));
            root.Add(btnBlackboard);



            var btnInspector = new EditorToolbarToggle(ResUtils.Load<Texture2D>("../res/Icons/ri--info-card-line.png"))
            {
                tooltip = "Inspector View"
            };
            btnInspector.RegisterValueChangedCallback(e =>
            {
                // Window.SetOverlayDisplayState(BlackboardView.Id, e.newValue);
            });
            // btnInspector.SetValueWithoutNotify(Window.GetOverlayDisplayState());
            root.Add(btnInspector);

            root.Add(new ToolbarSpacer());

            var btnRun = new EditorToolbarButton(ResUtils.Load<Texture2D>("../res/Icons/mdi--play.png"), () =>
            {
                Window.Graph.Run();
            })
            {
                tooltip = "Run",
            };
            root.Add(btnRun);

            var btnStep = new EditorToolbarButton(ResUtils.Load<Texture2D>("../res/Icons/codicon--debug-step-over.png"), () =>
            {
                Window.Graph.MoveNext();
            })
            {
                tooltip = "Step",
            };
            root.Add(btnStep);

            var btnStop = new EditorToolbarButton(ResUtils.Load<Texture2D>("../res/Icons/material-symbols--stop.png"), () =>
            {
                Window.Graph.Stop();
            })
            {
                tooltip = "Stop"
            };
            root.Add(btnStop);

            return root;
        }

        protected override void Initialize(BaseGraphView graphView)
        {
        }
    }
}