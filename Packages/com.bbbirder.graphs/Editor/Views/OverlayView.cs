using System;
using System.Reflection;
using BBBirder;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphProcessor
{
    public abstract class OverlayView : Overlay
    {
        const string pinnedElementStylePath = "../res/Styles/OverlayView.uss";

        protected OverlayElement pinnedElement;
        protected VisualElement root;

        public BaseGraphWindow Window => this.containerWindow as BaseGraphWindow;
        public BaseGraph Graph => Window?.Graph;

        public override void OnCreated()
        {
            Window.RegisterOverlayView(id, this);
        }

        protected void AddStyleSheet(VisualElement target)
        {
            target.styleSheets.Add(ResUtils.Load<StyleSheet>(pinnedElementStylePath));
        }

        protected virtual VisualElement CreateRootView()
        {
            return new();
        }

        internal protected virtual void Update()
        {
        }

        public override sealed VisualElement CreatePanelContent()
        {
            if (root == null)
            {
                root = CreateRootView();
                AddStyleSheet(root);
            }

            return root;
        }

        public virtual OverlayToolbar CreateVerticalToolbarContent()
        {
            var inst = new OverlayToolbar();
            var attrOverly = this.GetType().GetCustomAttribute<OverlayAttribute>();
            var title = attrOverly != null ? attrOverly.displayName : "";
            inst.Add(new Label(title) { name = "OverlayTitle" });
            inst.Add(CreatePanelContent());
            var style = ResUtils.Load<StyleSheet>("../res/Styles/OverlayView.uss");

            if (style != null)
                inst.styleSheets.Add(style);
            return inst;
        }

        public void InitializeGraphView(OverlayElement pinnedElement, BaseGraphView graphView)
        {
            this.pinnedElement = pinnedElement;

            Initialize(graphView);
        }

        protected abstract void Initialize(BaseGraphView graphView);

        ~OverlayView()
        {
            Destroy();
        }

        protected virtual void Destroy() { }
    }
}
