using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BBBirder.Instructions
{
    [CustomPropertyDrawer(typeof(Procedure), true)]
    class ProcedureDrawer : PropertyDrawer
    {
        static Evaluation? s_evaluationInPaste;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var propInstructions = property.FindPropertyRelative(nameof(Procedure.instructions));

            var root = new VisualElement()
            {
                name = "ProcedureView",
                pickingMode = PickingMode.Ignore,
            };

            root.styleSheets.Add(ResUtils.Load<StyleSheet>("./Styles/Instructions.uss"));

            if (!string.IsNullOrEmpty(property.displayName))
            {
                var title = new Label(property.displayName) { name = "Title" };
                root.Add(title);
            }

            var lstView = new ListView()
            {
                name = "Procedure-InstructionList",
                showAddRemoveFooter = false,
                showBoundCollectionSize = false,
                reorderable = false,
                showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
            };

            var unityObjectType = property.serializedObject.targetObject.GetType();
            var isDrawingByOdin = PropertyDrawerUtility.IsTypeDrawingByOdin(unityObjectType);

            lstView.reorderable = true;
            lstView.makeItem += () =>
            {
                var veItem = new VisualElement() { name = "ProcedureItem", focusable = true };
                veItem.Add(new PropertyField());
                if (isDrawingByOdin)
                {
                    veItem.style.paddingLeft = 8;
                }
                veItem.RegisterCallback<KeyDownEvent>(e =>
                {
                    if (e.target != veItem)
                        return;

                    propInstructions.serializedObject.Update();
                    var index = lstView.selectedIndex;

                    if (e.ctrlKey && e.keyCode == KeyCode.X && index >= 0)
                    {
                        var prop = propInstructions.GetArrayElementAtIndex(index);
                        s_evaluationInPaste = (Evaluation)prop.boxedValue;
                        propInstructions.DeleteArrayElementAtIndex(index);
                        e.StopPropagation();
                    }

                    if (e.ctrlKey && e.keyCode == KeyCode.C && index >= 0)
                    {
                        var prop = propInstructions.GetArrayElementAtIndex(index);
                        s_evaluationInPaste = (Evaluation)prop.boxedValue;
                        e.StopPropagation();
                    }

                    if (e.ctrlKey && e.keyCode == KeyCode.V && s_evaluationInPaste != null)
                    {
                        var i = index == -1 ? propInstructions.arraySize : index;
                        propInstructions.InsertArrayElementAtIndex(i);
                        var prop = propInstructions.GetArrayElementAtIndex(i);
                        prop.boxedValue = s_evaluationInPaste.Value.Clone();
                        e.StopPropagation();
                    }

                    if (e.keyCode == KeyCode.Delete && index >= 0)
                    {
                        propInstructions.DeleteArrayElementAtIndex(index);
                        e.StopPropagation();
                    }

                    if (e.altKey && e.keyCode == KeyCode.UpArrow && index >= 1)
                    {
                        var prop = propInstructions.GetArrayElementAtIndex(index);
                        propInstructions.MoveArrayElement(index, index - 1);
                        e.StopPropagation();
                    }

                    if (e.altKey && e.keyCode == KeyCode.DownArrow && index >= 0 && index < propInstructions.arraySize - 1)
                    {
                        var prop = propInstructions.GetArrayElementAtIndex(index);
                        propInstructions.MoveArrayElement(index, index + 1);
                        e.StopPropagation();
                    }

                    propInstructions.serializedObject.ApplyModifiedProperties();
                });

                return veItem;
            };

            lstView.bindItem += (ve, i) =>
            {
                var uiProp = ve.Q<PropertyField>();
                var prop = propInstructions.GetArrayElementAtIndex(i);
                uiProp.BindProperty(prop);
            };

            lstView.BindProperty(propInstructions);
            root.Add(lstView);

            var footer = new VisualElement() { name = "Footer" };
            root.Add(footer);

            var btnAdd = new Button()
            {
                name = "AddButton",
                text = "New Instruction...",
            };
            btnAdd.clicked += () =>
            {
                InstructionSelector.ShowInstructionBrowsable(btnAdd, type =>
                {
                    property.serializedObject.Update();
                    var index = propInstructions.arraySize;
                    propInstructions.InsertArrayElementAtIndex(index);
                    var newProp = propInstructions.GetArrayElementAtIndex(index);
                    newProp.boxedValue = new Evaluation()
                    {
                        instruction = Activator.CreateInstance(type) as IInstruction
                    };
                    property.serializedObject.ApplyModifiedProperties();
                });
            };

            var btnPlay = new Button()
            {
                name = "PlayButton",
                text = "",
            };
            btnPlay.Add(new Image() { });
            btnPlay.clicked += () =>
            {
                if (property.boxedValue is Procedure procedure)
                {
                    procedure.RunAsync().Forget();
                }
            };

            footer.Add(btnAdd);
            footer.Add(btnPlay);

            return root;
        }
    }
}
