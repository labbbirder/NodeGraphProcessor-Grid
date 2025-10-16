using System;
using System.Collections.Generic;
using System.Linq;
using com.bbbirder;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BBBirder.Instructions
{
    [CustomPropertyDrawer(typeof(IEvaluation), true)]
    class EvaluationDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var propInstr = property.FindPropertyRelative(nameof(Evaluation<int>.instruction));
            var propUseInstr = property.FindPropertyRelative(nameof(Evaluation<int>.useInstruction));
            var propConstValue = property.FindPropertyRelative(nameof(Evaluation<int>.constantValue));

            var root = new VisualElement()
            {
                name = "ValueGetterView",
                // pickingMode = PickingMode.Ignore,
            };

            root.styleSheets.Add(ResUtils.Load<StyleSheet>("./Styles/Instructions.uss"));

            var uiConstValue = new PropertyField(propConstValue, property.displayName)
            {
                name = "Constant"
            };
            root.Add(uiConstValue);

            var uiInstr = new PropertyField(propInstr, property.displayName);
            root.Add(uiInstr);

            var headContainer = new VisualElement()
            {
                name = "HeadContainer",
                pickingMode = PickingMode.Ignore,
            };

            root.RegisterCallback<PointerDownEvent>(e =>
            {
                if (e.button == 1)
                {
                    GenericMenu gm = new();
                    gm.AddItem(new("Print"), false, () =>
                    {
                        var evaluation = property.GetValue() as IEvaluation;
                        Debug.Log(evaluation.RunTypeless());
                    });
                    gm.ShowAsContext();
                }
            }, TrickleDown.TrickleDown);
            root.Add(headContainer);

            // popup
            var fieldType = fieldInfo.FieldType;
            if (property.propertyPath.EndsWith(']'))
            {
                if (fieldType.IsArray)
                {
                    fieldType = fieldType.GetElementType();
                }
                else
                {
                    foreach (var interfType in fieldType.GetInterfaces())
                    {
                        if (interfType.IsGenericType && interfType.GetGenericTypeDefinition() == typeof(IList<>))
                        {
                            fieldType = interfType.GenericTypeArguments[0];
                            break;
                        }
                    }
                }
            }

            var subtypes = InstructionsRegistry.GetValidInstructions(fieldType);
            var typeIndex = subtypes.IndexOf(propInstr?.managedReferenceValue?.GetType());
            var popup = new PopupField<Type>(null, subtypes, typeIndex, t => t?.Name, t => t?.Name)
            {
                style = {
                    flexGrow = 1,
                }
            };

            popup.RegisterValueChangedCallback(e =>
            {
                propInstr.serializedObject.Update();
                propInstr.managedReferenceValue = Activator.CreateInstance(e.newValue);
                // Refresh();
                propInstr.serializedObject.ApplyModifiedProperties();
            });

            // toggle button
            if (propUseInstr != null)
            {
                if (propConstValue == null)
                {
                    propUseInstr.boolValue = true;
                    propUseInstr.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }
                else
                {
                    var togUseInstr = new ToggleButton(null, ResUtils.Load<Texture2D>("./Textures/ri--code-view.png"));
                    togUseInstr.BindProperty(propUseInstr);
                    togUseInstr.RegisterValueChangedCallback(e =>
                    {
                        uiConstValue.style.display = !e.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                        uiInstr.style.display = e.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                        popup.style.display = e.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                    });

                    headContainer.Add(togUseInstr);
                }
            }

            headContainer.Add(popup);

            return root;
        }
    }
}
