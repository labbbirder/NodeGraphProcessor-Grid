using BBBirder;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphProcessor
{
    class NodeSettingsView : VisualElement
    {
        VisualElement m_ContentContainer;

        public NodeSettingsView()
        {
            pickingMode = PickingMode.Ignore;
            styleSheets.Add(ResUtils.Load<StyleSheet>("../res/Styles/NodeSettings.uss"));
            var uxml = ResUtils.Load<VisualTreeAsset>("../res/Templates/NodeSettings.uxml");
            uxml.CloneTree(this);

            // Get the element we want to use as content container
            m_ContentContainer = this.Q("contentContainer");
            RegisterCallback<MouseDownEvent>(OnMouseDown);
            RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        void OnMouseUp(MouseUpEvent evt)
        {
            evt.StopPropagation();
        }

        void OnMouseDown(MouseDownEvent evt)
        {
            evt.StopPropagation();
        }

        public override VisualElement contentContainer
        {
            get { return m_ContentContainer; }
        }
    }
}
