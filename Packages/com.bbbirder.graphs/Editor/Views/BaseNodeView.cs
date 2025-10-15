using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using BBBirder;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using NodeView = UnityEditor.Experimental.GraphView.Node;
using Status = UnityEngine.UIElements.DropdownMenuAction.Status;

namespace GraphProcessor
{
    [NodeCustomEditor(typeof(BaseNode))]
    public class BaseNodeView : NodeView
    {
        const string BaseNodeStylePath = "../res/Styles/BaseNodeView.uss";

        public BaseNode nodeTarget;

        public List<PortView> inputPortViews = new();
        public List<PortView> outputPortViews = new();

        public BaseGraphView GraphView { private set; get; }

        protected Dictionary<string, List<PortView>> portViewsPerFieldName = new();

        public VisualElement controlsContainer;
        protected VisualElement debugContainer;
        protected VisualElement headDot;
        protected VisualElement rightTitleContainer;
        protected VisualElement topPortContainer;
        protected VisualElement bottomPortContainer;
        private VisualElement inputContainerElement;

        VisualElement settings;
        NodeSettingsView settingsContainer;
        Button settingButton;
        TextField titleTextField;

        public event Action<PortView> onPortConnected;
        public event Action<PortView> onPortDisconnected;

        protected virtual bool hasSettings { get; set; }

        public bool initializing = false; //Used for applying SetPosition on locked node at init.

        bool settingsExpanded = false;

        [System.NonSerialized]
        List<IconBadge> badges = new();

        private List<NodeView> selectedNodes = new();
        private float selectedNodesFarLeft;
        private float selectedNodesNearLeft;
        private float selectedNodesFarRight;
        private float selectedNodesNearRight;
        private float selectedNodesFarTop;
        private float selectedNodesNearTop;
        private float selectedNodesFarBottom;
        private float selectedNodesNearBottom;
        private float selectedNodesAvgHorizontal;
        private float selectedNodesAvgVertical;

        #region  Initialization

        public void Initialize(BaseGraphView owner, BaseNode node)
        {
            nodeTarget = node;
            this.GraphView = owner;

            if (!node.deletable)
                capabilities &= ~Capabilities.Deletable;
            // Note that the Renamable capability is useless right now as it haven't been implemented in Graphview
            if (node.isRenamable)
                capabilities |= Capabilities.Renamable;

            // if (node.resizable)
            //     capabilities |= Capabilities.Resizable;

            node.onMessageAdded += AddMessageView;
            node.onMessageRemoved += RemoveMessageView;
            node.onPortsUpdated += OnPortsUpdated;

            styleSheets.Add(ResUtils.Load<StyleSheet>(BaseNodeStylePath));

            if (!string.IsNullOrEmpty(node.layoutStylePath))
                styleSheets.Add(ResUtils.Load<StyleSheet>(node.layoutStylePath));

            InitializeView();
            InitializePorts();

            // If the standard Enable method is still overwritten, we call it
            if (GetType().GetMethod(nameof(Enable), new Type[] { }).DeclaringType != typeof(BaseNodeView))
                ExceptionToLog.Call(() => Enable());
            else
                ExceptionToLog.Call(() => Enable(false));

            InitializeSettings();

            RefreshExpandedState();

            this.RefreshPorts();

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            RegisterCallback<DetachFromPanelEvent>(e => ExceptionToLog.Call(Disable));
            OnGeometryChanged(null);
        }

        void InitializePorts()
        {
            var listener = GraphView.connectorListener;

            foreach (var inputPort in nodeTarget.inputPorts)
            {
                AddPortView(inputPort.fieldInfo, Direction.Input, listener, inputPort.portData);
            }

            foreach (var outputPort in nodeTarget.outputPorts)
            {
                AddPortView(outputPort.fieldInfo, Direction.Output, listener, outputPort.portData);
            }
        }

        void InitializeView()
        {
            controlsContainer = new VisualElement { name = "controls" };
            controlsContainer.AddToClassList("NodeControls");
            mainContainer.Add(controlsContainer);

            rightTitleContainer = new VisualElement { name = "RightTitleContainer" };
            titleContainer.Add(rightTitleContainer);

            headDot = new VisualElement { name = "HeadDot" };
            Add(headDot);
            headDot.BringToFront();
            SetHeadDotDisplayState(false);

            topPortContainer = new VisualElement { name = "TopPortContainer" };
            this.Insert(0, topPortContainer);

            bottomPortContainer = new VisualElement { name = "BottomPortContainer" };
            this.Add(bottomPortContainer);

            if (nodeTarget.showControlsOnHover)
            {
                bool mouseOverControls = false;
                controlsContainer.style.display = DisplayStyle.None;
                RegisterCallback<MouseOverEvent>(e =>
                {
                    controlsContainer.style.display = DisplayStyle.Flex;
                    mouseOverControls = true;
                });
                RegisterCallback<MouseOutEvent>(e =>
                {
                    var rect = GetPosition();
                    var graphMousePosition = GraphView.contentViewContainer.WorldToLocal(e.mousePosition);
                    if (rect.Contains(graphMousePosition) || !nodeTarget.showControlsOnHover)
                        return;
                    mouseOverControls = false;
                    schedule.Execute(_ =>
                    {
                        if (!mouseOverControls)
                            controlsContainer.style.display = DisplayStyle.None;
                    }).ExecuteLater(500);
                });
            }

            Undo.undoRedoPerformed += UpdateFieldValues;

            debugContainer = new VisualElement { name = "debug" };
            if (nodeTarget.debug)
                mainContainer.Add(debugContainer);

            initializing = true;

            UpdateTitle();
            SetPosition(nodeTarget.position);
            SetNodeColor(nodeTarget.color);

            AddInputContainer();

            // Add renaming capability
            if ((capabilities & Capabilities.Renamable) != 0)
                SetupRenamableTitle();
        }

        void SetupRenamableTitle()
        {
            var titleLabel = this.Q("title-label") as Label;

            titleTextField = new TextField { isDelayed = true };
            titleTextField.style.display = DisplayStyle.None;
            titleLabel.parent.Insert(0, titleTextField);

            titleLabel.RegisterCallback<MouseDownEvent>(e =>
            {
                if (e.clickCount == 2 && e.button == (int)MouseButton.LeftMouse)
                    OpenTitleEditor();
            });

            titleTextField.RegisterValueChangedCallback(e => CloseAndSaveTitleEditor(e.newValue));

            titleTextField.RegisterCallback<MouseDownEvent>(e =>
            {
                if (e.clickCount == 2 && e.button == (int)MouseButton.LeftMouse)
                    CloseAndSaveTitleEditor(titleTextField.value);
            });

            titleTextField.RegisterCallback<FocusOutEvent>(e => CloseAndSaveTitleEditor(titleTextField.value));

            void OpenTitleEditor()
            {
                // show title textbox
                titleTextField.style.display = DisplayStyle.Flex;
                titleLabel.style.display = DisplayStyle.None;
                titleTextField.focusable = true;

                titleTextField.SetValueWithoutNotify(title);
                titleTextField.Focus();
                titleTextField.SelectAll();
            }

            void CloseAndSaveTitleEditor(string newTitle)
            {
                GraphView.RegisterCompleteObjectUndo("Renamed node " + newTitle);
                nodeTarget.SetCustomName(newTitle);

                // hide title TextBox
                titleTextField.style.display = DisplayStyle.None;
                titleLabel.style.display = DisplayStyle.Flex;
                titleTextField.focusable = false;

                UpdateTitle();
            }
        }

        void UpdateTitle()
        {
            title = (nodeTarget.GetCustomName() == null) ? nodeTarget.GetType().Name : nodeTarget.GetCustomName();
        }

        void InitializeSettings()
        {
            // Initialize settings button:
            if (hasSettings)
            {
                CreateSettingButton();
                settingsContainer = new NodeSettingsView();
                settingsContainer.visible = false;
                settings = new VisualElement();
                // Add Node type specific settings
                settings.Add(CreateSettingsView());
                settingsContainer.Add(settings);
                Add(settingsContainer);

                var fields = nodeTarget.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                foreach (var field in fields)
                    if (field.GetCustomAttribute(typeof(SettingAttribute)) != null)
                        AddSettingField(field);
            }
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (settingButton != null)
            {
                var settingsButtonLayout = settingButton.ChangeCoordinatesTo(settingsContainer.parent, settingButton.layout);
                settingsContainer.style.top = settingsButtonLayout.yMax - 18f;
                settingsContainer.style.left = settingsButtonLayout.xMin - layout.width + 20f;
            }
        }

        // Workaround for bug in GraphView that makes the node selection border way too big
        VisualElement selectionBorder, nodeBorder;
        internal void EnableSyncSelectionBorderHeight()
        {
            if (selectionBorder == null || nodeBorder == null)
            {
                selectionBorder = this.Q("selection-border");
                nodeBorder = this.Q("node-border");

                bool hasPendingExecution = false;
                nodeBorder.RegisterCallback<GeometryChangedEvent>(e =>
                {
                    const float ProbeHeight = 400;
                    const float ExtendHeight = 8;
                    style.height = ProbeHeight;
                    if (!hasPendingExecution)
                    {
                        schedule.Execute(() =>
                        {
                            hasPendingExecution = false;
                            if (panel == null) return;
                            style.height = nodeBorder.localBound.height + ExtendHeight;
                            selectionBorder.style.height = nodeBorder.localBound.height;
                        }).ExecuteLater(0);
                        hasPendingExecution = true;
                    }
                });
            }
        }

        void CreateSettingButton()
        {
            settingButton = new Button(ToggleSettings) { name = "settings-button" };
            settingButton.Add(new Image { name = "icon", scaleMode = ScaleMode.ScaleToFit });

            titleContainer.Add(settingButton);
        }

        void ToggleSettings()
        {
            settingsExpanded = !settingsExpanded;
            if (settingsExpanded)
                OpenSettings();
            else
                CloseSettings();
        }

        public void OpenSettings()
        {
            if (settingsContainer != null)
            {
                GraphView.ClearSelection();
                GraphView.AddToSelection(this);

                settingButton.AddToClassList("clicked");
                settingsContainer.visible = true;
                settingsExpanded = true;
            }
        }

        public void CloseSettings()
        {
            if (settingsContainer != null)
            {
                settingButton.RemoveFromClassList("clicked");
                settingsContainer.visible = false;
                settingsExpanded = false;
            }
        }

        #endregion

        #region API

        public List<PortView> GetPortViewsFromFieldName(string fieldName)
        {
            portViewsPerFieldName.TryGetValue(fieldName, out var ret);
            return ret;
        }

        public PortView GetFirstPortViewFromFieldName(string fieldName)
        {
            return GetPortViewsFromFieldName(fieldName)?.First();
        }

        public PortView GetPortViewFromFieldName(string fieldName, string identifier)
        {
            return GetPortViewsFromFieldName(fieldName)?.FirstOrDefault(pv =>
            {
                return (pv.portData.identifier == identifier) || (string.IsNullOrEmpty(pv.portData.identifier) && string.IsNullOrEmpty(identifier));
            });
        }


        public PortView AddPortView(FieldInfo fieldInfo, Direction direction, BaseEdgeConnectorListener listener, PortData portData)
        {
            PortView p = CreatePortView(direction, fieldInfo, portData, listener);

            if (p.direction == Direction.Input)
            {
                inputPortViews.Add(p);

                if (portData.vertical)
                    topPortContainer.Add(p);
                else
                    inputContainer.Add(p);
            }
            else
            {
                outputPortViews.Add(p);

                if (portData.vertical)
                    bottomPortContainer.Add(p);
                else
                    outputContainer.Add(p);
            }

            p.Initialize(this, portData?.displayName);

            portViewsPerFieldName.TryGetValue(p.fieldName, out var ports);
            if (ports == null)
            {
                ports = new List<PortView>();
                portViewsPerFieldName[p.fieldName] = ports;
            }

            ports.Add(p);

            return p;
        }

        protected virtual PortView CreatePortView(Direction direction, FieldInfo fieldInfo, PortData portData, BaseEdgeConnectorListener listener)
            => PortView.CreatePortView(direction, fieldInfo, portData, listener);

        public void RemovePortView(PortView pv)
        {
            // Remove all connected edges:
            var edgesCopy = pv.GetEdges().ToList();
            foreach (var e in edgesCopy)
                GraphView.Disconnect(e, refreshPorts: false);

            if (pv.direction == Direction.Input)
            {
                if (inputPortViews.Remove(pv))
                    pv.RemoveFromHierarchy();
            }
            else
            {
                if (outputPortViews.Remove(pv))
                    pv.RemoveFromHierarchy();
            }

            portViewsPerFieldName.TryGetValue(pv.fieldName, out var portViews);
            portViews.Remove(pv);
        }

        private void SetValuesForSelectedNodes()
        {
            selectedNodes = new List<Node>();
            foreach (var node in GraphView.nodes)
            {
                if (node.selected) selectedNodes.Add(node);
            }

            if (selectedNodes.Count < 2) return; //	No need for any of the calculations below

            selectedNodesFarLeft = int.MinValue;
            selectedNodesFarRight = int.MinValue;
            selectedNodesFarTop = int.MinValue;
            selectedNodesFarBottom = int.MinValue;

            selectedNodesNearLeft = int.MaxValue;
            selectedNodesNearRight = int.MaxValue;
            selectedNodesNearTop = int.MaxValue;
            selectedNodesNearBottom = int.MaxValue;

            foreach (var selectedNode in selectedNodes)
            {
                var nodeStyle = selectedNode.style;
                var nodeWidth = selectedNode.localBound.size.x;
                var nodeHeight = selectedNode.localBound.size.y;

                if (nodeStyle.left.value.value > selectedNodesFarLeft) selectedNodesFarLeft = nodeStyle.left.value.value;
                if (nodeStyle.left.value.value + nodeWidth > selectedNodesFarRight) selectedNodesFarRight = nodeStyle.left.value.value + nodeWidth;
                if (nodeStyle.top.value.value > selectedNodesFarTop) selectedNodesFarTop = nodeStyle.top.value.value;
                if (nodeStyle.top.value.value + nodeHeight > selectedNodesFarBottom) selectedNodesFarBottom = nodeStyle.top.value.value + nodeHeight;

                if (nodeStyle.left.value.value < selectedNodesNearLeft) selectedNodesNearLeft = nodeStyle.left.value.value;
                if (nodeStyle.left.value.value + nodeWidth < selectedNodesNearRight) selectedNodesNearRight = nodeStyle.left.value.value + nodeWidth;
                if (nodeStyle.top.value.value < selectedNodesNearTop) selectedNodesNearTop = nodeStyle.top.value.value;
                if (nodeStyle.top.value.value + nodeHeight < selectedNodesNearBottom) selectedNodesNearBottom = nodeStyle.top.value.value + nodeHeight;
            }

            selectedNodesAvgHorizontal = (selectedNodesNearLeft + selectedNodesFarRight) / 2f;
            selectedNodesAvgVertical = (selectedNodesNearTop + selectedNodesFarBottom) / 2f;
        }

        public static Rect GetNodeRect(Node node, float left = int.MaxValue, float top = int.MaxValue)
        {
            return new Rect(
                new Vector2(left != int.MaxValue ? left : node.style.left.value.value, top != int.MaxValue ? top : node.style.top.value.value),
                new Vector2(node.style.width.value.value, node.style.height.value.value)
            );
        }

        public void AlignToLeft()
        {
            SetValuesForSelectedNodes();
            if (selectedNodes.Count < 2) return;

            foreach (var selectedNode in selectedNodes)
            {
                selectedNode.SetPosition(GetNodeRect(selectedNode, selectedNodesNearLeft));
            }
        }

        public void AlignToCenter()
        {
            SetValuesForSelectedNodes();
            if (selectedNodes.Count < 2) return;

            foreach (var selectedNode in selectedNodes)
            {
                selectedNode.SetPosition(GetNodeRect(selectedNode, selectedNodesAvgHorizontal - selectedNode.localBound.size.x / 2f));
            }
        }

        public void AlignToRight()
        {
            SetValuesForSelectedNodes();
            if (selectedNodes.Count < 2) return;

            foreach (var selectedNode in selectedNodes)
            {
                selectedNode.SetPosition(GetNodeRect(selectedNode, selectedNodesFarRight - selectedNode.localBound.size.x));
            }
        }

        public void AlignToTop()
        {
            SetValuesForSelectedNodes();
            if (selectedNodes.Count < 2) return;

            foreach (var selectedNode in selectedNodes)
            {
                selectedNode.SetPosition(GetNodeRect(selectedNode, top: selectedNodesNearTop));
            }
        }

        public void AlignToMiddle()
        {
            SetValuesForSelectedNodes();
            if (selectedNodes.Count < 2) return;

            foreach (var selectedNode in selectedNodes)
            {
                selectedNode.SetPosition(GetNodeRect(selectedNode, top: selectedNodesAvgVertical - selectedNode.localBound.size.y / 2f));
            }
        }

        public void AlignToBottom()
        {
            SetValuesForSelectedNodes();
            if (selectedNodes.Count < 2) return;

            foreach (var selectedNode in selectedNodes)
            {
                selectedNode.SetPosition(GetNodeRect(selectedNode, top: selectedNodesFarBottom - selectedNode.localBound.size.y));
            }
        }

        public void OpenNodeViewScript()
        {
            var script = NodeProvider.GetNodeViewScript(GetType());

            if (script != null)
                AssetDatabase.OpenAsset(script.GetInstanceID(), 0, 0);
        }

        public void OpenNodeScript()
        {
            var script = NodeProvider.GetNodeScript(nodeTarget.GetType());

            if (script != null)
                AssetDatabase.OpenAsset(script.GetInstanceID(), 0, 0);
        }

        public void ToggleDebug()
        {
            nodeTarget.debug = !nodeTarget.debug;
            UpdateDebugView();
        }

        public void UpdateDebugView()
        {
            if (nodeTarget.debug)
                mainContainer.Add(debugContainer);
            else
                mainContainer.Remove(debugContainer);
        }

        public void AddMessageView(string message, Texture icon, Color color)
            => AddBadge(new NodeBadgeView(message, icon, color));

        public void AddMessageView(string message, NodeMessageType messageType)
        {
            IconBadge badge = null;
            badge = messageType switch
            {
                NodeMessageType.Warning => new NodeBadgeView(message, EditorGUIUtility.IconContent("Collab.Warning").image, Color.yellow),
                NodeMessageType.Error => IconBadge.CreateError(message),
                NodeMessageType.Info => IconBadge.CreateComment(message),
                _ => new NodeBadgeView(message, null, Color.grey),
            };
            AddBadge(badge);
        }

        void AddBadge(IconBadge badge)
        {
            Add(badge);
            badges.Add(badge);
            badge.AttachTo(topContainer, SpriteAlignment.TopRight);
        }

        void RemoveBadge(Func<IconBadge, bool> callback)
        {
            badges.RemoveAll(b =>
            {
                if (callback(b))
                {
                    b.Detach();
                    b.RemoveFromHierarchy();
                    return true;
                }

                return false;
            });
        }

        public void RemoveMessageViewContains(string message) => RemoveBadge(b => b.badgeText.Contains(message));

        public void RemoveMessageView(string message) => RemoveBadge(b => b.badgeText == message);

        public void SetHighlightState(NodeStatus status)
        {
            this.EnableInClassList("Highlight-Running", status == NodeStatus.Running);
            this.EnableInClassList("Highlight-Success", status == NodeStatus.Success);
            this.EnableInClassList("Highlight-Fault", status == NodeStatus.Fault);
        }

        public void SetHeadDotDisplayState(bool state)
        {
            if (headDot != null)
            {
                headDot.style.display = state ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        #endregion

        #region Callbacks & Overrides

        public virtual void Enable(bool fromInspector = false) => DrawDefaultInspector(fromInspector);
        public virtual void Enable() => DrawDefaultInspector(false);

        public virtual void Disable() { }

        Dictionary<string, List<(object value, VisualElement target)>> visibleConditions = new();
        Dictionary<string, VisualElement> hideElementIfConnected = new();
        Dictionary<FieldInfo, List<VisualElement>> fieldControlsMap = new();

        protected void AddInputContainer()
        {
            inputContainerElement = new VisualElement { name = "input-container" };
            mainContainer.parent.Add(inputContainerElement);
            inputContainerElement.SendToBack();
            inputContainerElement.pickingMode = PickingMode.Ignore;
        }

        protected virtual void DrawDefaultInspector(bool fromInspector = false)
        {
            // const bool fromInspector = true;

            // RuntimeTypeCache.GetNodeInstantceFieldInfos(nodeTarget.GetType());
            var fields = RuntimeTypeCache.GetNodeInstantceFieldInfos(nodeTarget.GetType())
                // Filter fields from the BaseNode type since we are only interested in user-defined fields
                // (better than BindingFlags.DeclaredOnly because we keep any inherited user-defined fields)
                .Where(f => f.DeclaringType != typeof(BaseNode));

            foreach (var field in fields)
            {
                //skip if the field is a node setting
                if (field.GetCustomAttribute(typeof(SettingAttribute)) != null)
                {
                    hasSettings = true;
                    continue;
                }

                //skip if the field is not serializable
                bool serializeField = field.GetCustomAttribute(typeof(SerializeField)) != null;
                if ((!field.IsPublic && !serializeField) || field.IsNotSerialized)
                {
                    AddEmptyField(field, fromInspector);
                    continue;
                }

                //skip if the field is an input/output and not marked as SerializedField
                bool hasInputAttribute = field.GetCustomAttribute(typeof(InputAttribute)) != null;
                bool hasInputOrOutputAttribute = hasInputAttribute || field.GetCustomAttribute(typeof(OutputAttribute)) != null;
                bool showAsDrawer = !fromInspector && field.GetCustomAttribute(typeof(ShowAsDrawer)) != null;
                if (!serializeField && hasInputOrOutputAttribute && !showAsDrawer && !fromInspector)
                {
                    AddEmptyField(field, fromInspector);
                    continue;
                }

                //skip if marked with NonSerialized or HideInInspector
                if (field.GetCustomAttribute(typeof(System.NonSerializedAttribute)) != null || field.GetCustomAttribute(typeof(HideInInspector)) != null)
                {
                    AddEmptyField(field, fromInspector);
                    continue;
                }

                // Hide the field if we want to display in in the inspector
                var showInInspector = field.GetCustomAttribute<ShowInInspector>();
                if (showInInspector != null && !showInInspector.showInNode && !fromInspector)
                {
                    AddEmptyField(field, fromInspector);
                    continue;
                }

                var showInputDrawer = field.GetCustomAttribute(typeof(InputAttribute)) != null && field.GetCustomAttribute(typeof(SerializeField)) != null;
                showInputDrawer |= field.GetCustomAttribute(typeof(InputAttribute)) != null && field.GetCustomAttribute(typeof(ShowAsDrawer)) != null;
                showInputDrawer &= !fromInspector; // We can't show a drawer in the inspector
                showInputDrawer &= !typeof(IList).IsAssignableFrom(field.FieldType);

                string displayName = ObjectNames.NicifyVariableName(field.Name);

                var inspectorNameAttribute = field.GetCustomAttribute<InspectorNameAttribute>();
                if (inspectorNameAttribute != null)
                    displayName = inspectorNameAttribute.displayName;

                var elem = AddControlField(field, displayName, showInputDrawer);
                if (hasInputAttribute)
                {
                    hideElementIfConnected[field.Name] = elem;

                    // Hide the field right away if there is already a connection:
                    if (portViewsPerFieldName.TryGetValue(field.Name, out var pvs))
                        if (pvs.Any(pv => pv.GetEdges().Count > 0))
                            elem.style.display = DisplayStyle.None;
                }
            }
        }

        protected virtual void SetNodeColor(Color color)
        {
            titleContainer.style.borderBottomColor = new StyleColor(color);
            titleContainer.style.borderBottomWidth = new StyleFloat(color.a > 0 ? 5f : 0f);
        }

        private void AddEmptyField(FieldInfo field, bool fromInspector)
        {
            if (field.GetCustomAttribute(typeof(InputAttribute)) == null || fromInspector)
                return;

            if (field.GetCustomAttribute<VerticalAttribute>() != null)
                return;

            var box = new VisualElement { name = field.Name };
            box.AddToClassList("port-input-element");
            box.AddToClassList("empty");
            inputContainerElement.Add(box);
        }

        void UpdateFieldVisibility(string fieldName, object newValue)
        {
            if (newValue == null)
                return;
            if (visibleConditions.TryGetValue(fieldName, out var list))
            {
                foreach (var (value, target) in list)
                {
                    if (newValue.Equals(value))
                        target.style.display = DisplayStyle.Flex;
                    else
                        target.style.display = DisplayStyle.None;
                }
            }
        }

        void UpdateOtherFieldValueSpecific<T>(FieldInfo field, object newValue)
        {
            foreach (var inputField in fieldControlsMap[field])
            {
                if (inputField is INotifyValueChanged<T> notify)
                    notify.SetValueWithoutNotify((T)newValue);
            }
        }

        static MethodInfo specificUpdateOtherFieldValue = typeof(BaseNodeView).GetMethod(nameof(UpdateOtherFieldValueSpecific), BindingFlags.NonPublic | BindingFlags.Instance);
        void UpdateOtherFieldValue(FieldInfo info, object newValue)
        {
            // Warning: Keep in sync with FieldFactory CreateField
            var fieldType = info.FieldType.IsSubclassOf(typeof(UnityEngine.Object)) ? typeof(UnityEngine.Object) : info.FieldType;
            var genericUpdate = specificUpdateOtherFieldValue.MakeGenericMethod(fieldType);

            genericUpdate.Invoke(this, new object[] { info, newValue });
        }

        object GetInputFieldValueSpecific<T>(FieldInfo field)
        {
            if (fieldControlsMap.TryGetValue(field, out var list))
            {
                foreach (var inputField in list)
                {
                    if (inputField is INotifyValueChanged<T> notify)
                        return notify.value;
                }
            }

            return null;
        }

        static MethodInfo specificGetValue = typeof(BaseNodeView).GetMethod(nameof(GetInputFieldValueSpecific), BindingFlags.NonPublic | BindingFlags.Instance);
        object GetInputFieldValue(FieldInfo info)
        {
            // Warning: Keep in sync with FieldFactory CreateField
            var fieldType = info.FieldType.IsSubclassOf(typeof(UnityEngine.Object)) ? typeof(UnityEngine.Object) : info.FieldType;
            var genericUpdate = specificGetValue.MakeGenericMethod(fieldType);

            return genericUpdate.Invoke(this, new object[] { info });
        }

        protected VisualElement AddControlField(string fieldName, string label = null, bool showInputDrawer = false, Action valueChangedCallback = null)
            => AddControlField(nodeTarget.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance), label, showInputDrawer, valueChangedCallback);

        Regex s_ReplaceNodeIndexPropertyPath = new(@"(^nodes.Array.data\[)(\d+)(\])");
        internal void SyncSerializedPropertyPathes()
        {
            int nodeIndex = GraphView.graph.nodes.IndexOf(nodeTarget);

            // If the node is not found, then it means that it has been deleted from serialized data.
            if (nodeIndex == -1)
                return;

            var nodeIndexString = nodeIndex.ToString();
            foreach (var propertyField in this.Query<PropertyField>().ToList())
            {
                propertyField.Unbind();
                // The property path look like this: nodes.Array.data[x].fieldName
                // And we want to update the value of x with the new node index:
                if (string.IsNullOrEmpty(propertyField.bindingPath)) continue;
                propertyField.bindingPath = s_ReplaceNodeIndexPropertyPath.Replace(propertyField.bindingPath, m => m.Groups[1].Value + nodeIndexString + m.Groups[3].Value);
                propertyField.Bind(GraphView.serializedGraph.serializedObject);
            }
        }

        protected SerializedProperty FindSerializedProperty(string fieldName)
        {
            int i = GraphView.graph.nodes.FindIndex(n => n == nodeTarget);
            return GraphView.serializedGraph.FindPropertyRelative("nodes").GetArrayElementAtIndex(i).FindPropertyRelative(fieldName);
        }

        protected VisualElement AddControlField(FieldInfo field, string label = null, bool showInputDrawer = false, Action valueChangedCallback = null)
        {
            if (field == null)
                return null;

            var property = FindSerializedProperty(field.Name);
            var element = new PropertyField(property, showInputDrawer ? "" : label);
            element.Bind(GraphView.serializedGraph.serializedObject);

#if UNITY_2020_3 // In Unity 2020.3 the empty label on property field doesn't hide it, so we do it manually
			if ((showInputDrawer || String.IsNullOrEmpty(label)) && element != null)
				element.AddToClassList("DrawerField_2020_3");
#endif

            if (property != null)
            {
                var propSibling = property.Copy();
                propSibling.Next(false);
                bool hasArray = false;
                do
                {
                    hasArray = property.isArray;
                    if (hasArray) break;

                    foreach (var p in property)
                    {
                        if (p == propSibling) break;
                        if ((p as SerializedProperty).isArray)
                        {
                            hasArray = true;
                            break;
                        }
                    }

                } while (false);

                if (hasArray)
                    EnableSyncSelectionBorderHeight();
            }

            element.RegisterValueChangeCallback(e =>
            {
                UpdateFieldVisibility(field.Name, field.GetValue(nodeTarget));
                valueChangedCallback?.Invoke();
                NotifyNodeContentChanged();
            });

            element.RegisterCallback<FocusOutEvent>(e =>
            {
                NotifyNodeFocusOutInEditor();
            });
            // element.TrackPropertyValue(FindSerializedProperty(field.Name), p =>
            // {
            //     Debug.Log($"prop change {p}");
            // });
            // Disallow picking scene objects when the graph is not linked to a scene
            if (element != null && !GraphView.graph.IsLinkedToScene())
            {
                var objectField = element.Q<ObjectField>();
                if (objectField != null)
                    objectField.allowSceneObjects = false;
            }

            if (!fieldControlsMap.TryGetValue(field, out var inputFieldList))
                inputFieldList = fieldControlsMap[field] = new List<VisualElement>();
            inputFieldList.Add(element);

            if (element != null)
            {
                if (showInputDrawer)
                {
                    var box = new VisualElement { name = field.Name };
                    box.AddToClassList("port-input-element");
                    box.Add(element);
                    inputContainerElement.Add(box);
                }
                else
                {
                    controlsContainer.Add(element);
                }

                element.name = field.Name;
            }
            else
            {
                // Make sure we create an empty placeholder if FieldFactory can not provide a drawer
                if (showInputDrawer) AddEmptyField(field, false);
            }

            if (field.GetCustomAttribute(typeof(VisibleIf)) is VisibleIf visibleCondition)
            {
                // Check if target field exists:
                var conditionField = nodeTarget.GetType().GetField(visibleCondition.fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (conditionField == null)
                    Debug.LogError($"[VisibleIf] Field {visibleCondition.fieldName} does not exists in node {nodeTarget.GetType()}");
                else
                {
                    visibleConditions.TryGetValue(visibleCondition.fieldName, out var list);
                    list ??= visibleConditions[visibleCondition.fieldName] = new List<(object value, VisualElement target)>();
                    list.Add((visibleCondition.value, element));
                    UpdateFieldVisibility(visibleCondition.fieldName, conditionField.GetValue(nodeTarget));
                }
            }

            return element;
        }

        void UpdateFieldValues()
        {
            foreach (var kp in fieldControlsMap)
                UpdateOtherFieldValue(kp.Key, kp.Key.GetValue(nodeTarget));
        }

        protected void AddSettingField(FieldInfo field)
        {
            if (field == null)
                return;

            var label = field.GetCustomAttribute<SettingAttribute>().name;

            var element = new PropertyField(FindSerializedProperty(field.Name));
            element.Bind(GraphView.serializedGraph.serializedObject);

            if (element != null)
            {
                settingsContainer.Add(element);
                element.name = field.Name;
            }
        }

        internal void OnPortConnected(PortView port)
        {
            if (port.direction == Direction.Input && inputContainerElement?.Q(port.fieldName) != null)
                inputContainerElement.Q(port.fieldName).AddToClassList("empty");

            if (hideElementIfConnected.TryGetValue(port.fieldName, out var elem))
                elem.style.display = DisplayStyle.None;

            onPortConnected?.Invoke(port);
        }

        internal void OnPortDisconnected(PortView port)
        {
            if (port.direction == Direction.Input && inputContainerElement?.Q(port.fieldName) != null)
            {
                inputContainerElement.Q(port.fieldName).RemoveFromClassList("empty");

                if (nodeTarget.ioFields.TryGetValue(port.fieldName, out var fieldInfo))
                {
                    var valueBeforeConnection = GetInputFieldValue(fieldInfo.info);

                    if (valueBeforeConnection != null)
                    {
                        fieldInfo.info.SetValue(nodeTarget, valueBeforeConnection);
                    }
                }
            }

            if (hideElementIfConnected.TryGetValue(port.fieldName, out var elem))
                elem.style.display = DisplayStyle.Flex;

            onPortDisconnected?.Invoke(port);
        }

        // TODO: a function to force to reload the custom behavior ports (if we want to do a button to add ports for example)

        public void OnRemovedInternal()
        {
            nodeTarget.graph.ClearNodeSortCache();
            GraphView.DelayToResortEdges("On Node Remove");
            OnRemoved();
        }

        public void OnCreatedInternal()
        {
            nodeTarget.graph.ClearNodeSortCache();
            GraphView.DelayToResortEdges("On Node Add");
            OnCreated();
        }

        protected virtual void OnRemoved() { }
        protected virtual void OnCreated() { }

        public override void SetPosition(Rect newPos)
        {
            if (initializing || !nodeTarget.isLocked)
            {
                base.SetPosition(newPos);

                if (!initializing)
                    GraphView.RegisterCompleteObjectUndo("Moved graph node");

                nodeTarget.position = newPos;
                nodeTarget.graph.ClearNodeSortCache();
                GraphView.DelayToResortEdges("On Node SetPosition");

                initializing = false;
            }
        }

        public override bool expanded
        {
            get { return base.expanded; }
            set
            {
                base.expanded = value;
                nodeTarget.expanded = value;
            }
        }

        public void ChangeLockStatus()
        {
            nodeTarget.nodeLock ^= true;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            SetValuesForSelectedNodes();
            if (selectedNodes.Count > 1)
            {
                BuildAlignMenu(evt);
            }

            evt.menu.AppendAction("Open Node Script", (e) => OpenNodeScript(), OpenNodeScriptStatus);
            evt.menu.AppendAction("Open Node View Script", (e) => OpenNodeViewScript(), OpenNodeViewScriptStatus);
            evt.menu.AppendAction("Debug", (e) => ToggleDebug(), DebugStatus);
            if (nodeTarget.unlockable)
                evt.menu.AppendAction((nodeTarget.isLocked ? "Unlock" : "Lock"), (e) => ChangeLockStatus(), LockStatus);
        }

        protected void BuildAlignMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Align/To Left", (e) => AlignToLeft());
            evt.menu.AppendAction("Align/To Center", (e) => AlignToCenter());
            evt.menu.AppendAction("Align/To Right", (e) => AlignToRight());
            evt.menu.AppendSeparator("Align/");
            evt.menu.AppendAction("Align/To Top", (e) => AlignToTop());
            evt.menu.AppendAction("Align/To Middle", (e) => AlignToMiddle());
            evt.menu.AppendAction("Align/To Bottom", (e) => AlignToBottom());
            evt.menu.AppendSeparator();
        }

        Status LockStatus(DropdownMenuAction action)
        {
            return Status.Normal;
        }

        Status DebugStatus(DropdownMenuAction action)
        {
            if (nodeTarget.debug)
                return Status.Checked;
            return Status.Normal;
        }

        Status OpenNodeScriptStatus(DropdownMenuAction action)
        {
            if (NodeProvider.GetNodeScript(nodeTarget.GetType()) != null)
                return Status.Normal;
            return Status.Disabled;
        }

        Status OpenNodeViewScriptStatus(DropdownMenuAction action)
        {
            if (NodeProvider.GetNodeViewScript(GetType()) != null)
                return Status.Normal;
            return Status.Disabled;
        }

        void SyncPortCounts(List<NodePort> ports, ref List<PortView> portViews)
        {
            var listener = GraphView.connectorListener;
            var portViewsCopy = portViews.ToList();

            // Add missing port views

            var resultPortViews = new List<PortView>();
            resultPortViews.AddRange(Enumerable.Repeat(default(PortView), ports.Count));

            for (int i = 0; i < ports.Count; i++)
            {
                var p = ports[i];
                var pv = portViewsCopy.FirstOrDefault(pv => p.portData.identifier == pv.portData.identifier);
                if (pv == null)
                {
                    Direction portDirection = nodeTarget.IsFieldInput(p.fieldName) ? Direction.Input : Direction.Output;
                    pv = AddPortView(p.fieldInfo, portDirection, listener, p.portData);
                }
                else
                {
                    portViewsCopy.Remove(pv);
                }

                resultPortViews[i] = pv;
                pv.UpdatePortView(p.portData);
                pv.BringToFront();
            }

            // Remove remaining port views

            foreach (var pv in portViewsCopy)
            {
                RemovePortView(pv);
            }

            portViews = resultPortViews;
        }

        public virtual new bool RefreshPorts()
        {
            // If a port behavior was attached to one port, then
            // the port count might have been updated by the node
            // so we have to refresh the list of port views.
            SyncPortCounts(nodeTarget.inputPorts, ref inputPortViews);
            SyncPortCounts(nodeTarget.outputPorts, ref outputPortViews);

            return base.RefreshPorts();
        }

        void OnPortsUpdated()
        {
            RefreshPorts();
        }

        protected virtual VisualElement CreateSettingsView() => new Label("Settings") { name = "header" };

        /// <summary>
        /// Send an event to the graph telling that the content of this node have changed
        /// </summary>
        public void NotifyNodeContentChanged() => GraphView.graph.NotifyNodeContentChanged(nodeTarget);

        public void NotifyNodeFocusOutInEditor() => GraphView.graph.NotifyNodeFocusOutInEditor(nodeTarget);

        #endregion
    }
}
