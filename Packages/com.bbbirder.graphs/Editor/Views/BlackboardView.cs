using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Overlays;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphProcessor
{
    public class BlackboardView : OverlayView, ICreateVerticalToolbar
    {
        internal protected const string Id = "GraphProcessor-Blackboard";
        const string exposedParameterViewStyle = "../res/Styles/ExposedParameterView.uss";

        private ScrollView content;
        protected BaseGraphView graphView;

        protected override Layout supportedLayouts => Layout.Panel | Layout.VerticalToolbar;

        protected override VisualElement CreateRootView()
        {
            var tpl = ResUtils.Load<VisualTreeAsset>("../res/Templates/BlackboardView.uxml");

            var root = tpl.CloneTree();
            content = root.Q<ScrollView>();
            root.Q<Button>("btnNew").clicked += OnAddClicked;
            root.RegisterCallback<DetachFromPanelEvent>(OnViewClosed);

            var style = ResUtils.Load<StyleSheet>(exposedParameterViewStyle);
            if (style != null)
                root.styleSheets.Add(style);

            return root;
        }

        protected virtual void OnAddClicked()
        {
            if (graphView == null) return;

            var txtName = root.Q<TextField>("txtName");
            var paramName = txtName.value;
            if (graphView.graph.exposedParameters.Any(e => e.name == paramName))
            {
                EditorUtility.DisplayDialog("error", $"There is already a parameter named `{paramName}`", "ok");
                return;
            }
            var parameterType = new GenericMenu();

            foreach (var paramType in GetExposedParameterTypes())
                parameterType.AddItem(new GUIContent(GetNiceNameFromType(paramType)), false, () =>
                {
                    if (string.IsNullOrWhiteSpace(paramName))
                    {
                        paramName = GetUniqueExposedPropertyName($"New {GetNiceNameFromType(paramType)}");
                    }

                    graphView.graph.AddExposedParameter(paramName, paramType);
                    txtName.value = "";
                });

            parameterType.ShowAsContext();
        }

        protected string GetNiceNameFromType(Type type)
        {
            string name = type.Name;

            // Remove parameter in the name of the type if it exists
            name = name.Replace("Parameter", "");

            return ObjectNames.NicifyVariableName(name);
        }

        protected string GetUniqueExposedPropertyName(string name)
        {
            // Generate unique name
            string uniqueName = name;
            int i = 0;
            while (graphView.graph.exposedParameters.Any(e => e.name == name))
                name = uniqueName + " " + i++;
            return name;
        }

        protected virtual IEnumerable<Type> GetExposedParameterTypes()
        {
            foreach (var type in TypeCache.GetTypesDerivedFrom<ExposedParameter>())
            {
                if (type.IsGenericType)
                    continue;

                yield return type;
            }
        }

        protected virtual void UpdateParameterList()
        {
            content.Clear();

            foreach (var param in graphView.graph.exposedParameters)
            {
                var row = new BlackboardRow(new BlackboardFieldView(graphView, param), new BlackboardFieldPropertyView(graphView, param));
                row.expanded = param.settings.expanded;
                row.RegisterCallback<GeometryChangedEvent>(e =>
                {
                    param.settings.expanded = row.expanded;
                });

                content.Add(row);
            }
        }

        protected override void Initialize(BaseGraphView graphView)
        {
            this.graphView = graphView;

            graphView.onExposedParameterListChanged += UpdateParameterList;
            graphView.initialized += UpdateParameterList;
            Undo.undoRedoPerformed += UpdateParameterList;


            UpdateParameterList();

        }

        void OnViewClosed(DetachFromPanelEvent evt)
            => Undo.undoRedoPerformed -= UpdateParameterList;
    }
}
