using UnityEditor;
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
        EditorToolbarButton btnRun, btnStep, btnStop;

        public OverlayToolbar CreateHorizontalToolbarContent()
        {
            return CreateToolbar();
        }

        protected internal override void Update()
        {
            if (Graph != null)
            {
                btnRun.SetEnabled(!Graph.IsRunning);
                // btnStop.SetEnabled(Graph.IsRunning);
                if (Graph.AutoStep)
                    Graph.FrameStep();
            }
        }

        public override OverlayToolbar CreateVerticalToolbarContent()
        {
            return CreateToolbar();
        }

        protected virtual OverlayToolbar CreateToolbar()
        {
            var root = new OverlayToolbar();
            root.styleSheets.Add(ResUtils.Load<StyleSheet>("../res/Styles/ToolbarView.uss"));

            var uiLeft = new VisualElement();
            uiLeft.AddToClassList("left-group");
            root.Add(uiLeft);
            var uiMiddle = new VisualElement();
            uiMiddle.AddToClassList("mid-group");
            root.Add(uiMiddle);
            var uiRight = new VisualElement();
            uiRight.AddToClassList("right-group");
            root.Add(uiRight);

            var btnBlackboard = new EditorToolbarToggle(ResUtils.Load<Texture2D>("../res/Icons/codicon--settings.png"))
            {
                tooltip = "Blackboard View",
            };
            btnBlackboard.RegisterValueChangedCallback(e =>
            {
                Window.SetOverlayDisplayState(BlackboardView.Id, e.newValue);
            });
            btnBlackboard.SetValueWithoutNotify(Window.GetOverlayDisplayState(BlackboardView.Id));
            uiLeft.Add(btnBlackboard);



            var btnInspector = new EditorToolbarToggle(ResUtils.Load<Texture2D>("../res/Icons/ri--info-card-line.png"))
            {
                tooltip = "Inspector View"
            };
            btnInspector.RegisterValueChangedCallback(e =>
            {
                // Window.SetOverlayDisplayState(BlackboardView.Id, e.newValue);
            });
            // btnInspector.SetValueWithoutNotify(Window.GetOverlayDisplayState());
            uiLeft.Add(btnInspector);

            uiLeft.Add(new ToolbarSpacer());


            var btnFit = new EditorToolbarButton(ResUtils.Load<Texture2D>("../res/Icons/material-symbols--fit-screen.png"), () =>
            {
                Window.graphView.FitViewport();
            })
            {
                tooltip = "Fit Viewport"
            };
            uiLeft.Add(btnFit);

            var btnSnapGrid = new EditorToolbarToggle(ResUtils.Load<Texture2D>("../res/Icons/dinkie-icons--grid.png"))
            {
                tooltip = "Snap Grid"
            };
            btnSnapGrid.RegisterValueChangedCallback(e =>
            {
            });
            uiLeft.Add(btnSnapGrid);

            // root.Add(new ToolbarSpacer());

            btnRun = new EditorToolbarButton(ResUtils.Load<Texture2D>("../res/Icons/mdi--play.png"), () =>
            {
                // Topology may be changed
                Window.Graph.ClearDataFlowDirectionsCache();
                Window.Graph.ClearPortsTransferCache();

                Window.Graph.Run();
            })
            {
                tooltip = "Run",
            };
            uiMiddle.Add(btnRun);

            btnStep = new EditorToolbarButton(ResUtils.Load<Texture2D>("../res/Icons/codicon--debug-step-over.png"), () =>
            {
                Window.Graph.MoveNext();
                Window.Graph.NotifyExecutionStateChanged();
            })
            {
                tooltip = "Step",
            };
            uiMiddle.Add(btnStep);

            btnStop = new EditorToolbarButton(ResUtils.Load<Texture2D>("../res/Icons/material-symbols--stop.png"), () =>
            {
                Window.Graph.Stop();
                Window.Graph.NotifyExecutionStateChanged();
            })
            {
                tooltip = "Stop"
            };
            uiMiddle.Add(btnStop);


            var btnFocus = new EditorToolbarButton(ResUtils.Load<Texture2D>("../res/Icons/ph--cube-focus.png"), () =>
            {
                if (Window.graphOwner)
                {
                    EditorGUIUtility.PingObject(Window.graphOwner);
                    // Selection.activeObject = Window.graphOwner;
                }
            })
            {
                tooltip = "Select Asset"
            };
            uiRight.Add(btnFocus);

            var btnExportTemplate = new EditorToolbarButton(ResUtils.Load<Texture2D>("../res/Icons/mage--box-3d-upload.png"), () =>
            {
                if (Window.Graph is null) return;

                var path = EditorUtility.SaveFilePanelInProject("Save Graph Template", $"New {Window.Graph.GetType().Name}", "asset", "message");
                if (!string.IsNullOrEmpty(path))
                {
                    Window.SaveCurrentGraphAsTemplate(path);
                }
            })
            {
                tooltip = "Export Template"
            };
            uiRight.Add(btnExportTemplate);


            return root;
        }

        protected override void Initialize(BaseGraphView graphView)
        {
        }
    }
}
