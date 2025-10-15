// using System;
// using System.Collections.Generic;
// using System.Reflection;
// using Game;
// using Newtonsoft.Json.Linq;
// using UnityEditor;
// using UnityEditor.UIElements;
// using UnityEngine.UIElements;

// namespace BBBirder.Instructions
// {
//     public class FieldBridge
//     {
//         enum DirtyColor
//         {
//             White,
//             Gray,
//             Black,
//         }
//         DirtyColor _dirtyColor;
//         object _value;
//         FieldBridge root;
//         FieldBridge parent;
//         bool isValueType;
//         public Type FieldType => fieldInfo.FieldType;
//         public FieldInfo fieldInfo;
//         List<FieldBridge> children;

//         public FieldBridge GetChild(string name)
//         {

//         }

//         public void SetValue(object value)
//         {
//             if (value != _value)
//             {
//                 _dirty = true;
//                 _value = value;
//             }
//         }

//         public T? GetValue<T>(object value)
//         {
//             return _value is T t ? t : default;
//         }

//         public void ApplyModifications()
//         {
//             if (FieldType.IsValueType)
//             {
//                 var refParent = parent;
//                 while (refParent != null && refParent.FieldType.IsValueType)
//                 {
//                     refParent = refParent.parent;
//                 }

//                 if (refParent == null)
//                 {
//                     refParent = root;
//                 }

//                 refParent.ApplyModifications();
//             }
//             else
//             {
//                 foreach (var child in children)
//                 {
//                     child.ApplyModifications();
//                 }
//             }
//             _dirty = false;
//         }

//         private void MakeDirtyUpwards()
//         {
//             _dirtyColor = DirtyColor.Black;

//             var p = parent;
//             while (p != null)
//             {
//                 if (p._dirtyColor != DirtyColor.White) break;
//                 p._dirtyColor = DirtyColor.Gray;
//                 p = p.parent;
//             }
//         }

//         private void RefreshAndCleanDirty()
//         {
//             if (_dirtyColor == DirtyColor.White)
//             {
//                 return;
//             }

//             if (_dirtyColor == DirtyColor.Gray)
//             {
//                 if (isValueType)
//                 {

//                 }
//             }


//             foreach (var child in children)
//             {
//                 child.RefreshAndCleanDirty();
//                 // child.fieldInfo.SetValue(_value, child._value);
//             }

//             if (isValueType)
//             {

//             }

//             _dirtyColor = DirtyColor.White;
//         }

//         private void UpdateValue()
//         {
//             foreach (var child in children)
//             {
//                 child.UpdateValue();
//                 child.fieldInfo.SetValue(_value, child._value);
//             }
//         }

//         public static FieldBridge Create(object value)
//         {
//             var bridge = new FieldBridge();
//             bridge.fieldType = value.GetType();
//         }
//     }
//     [CustomPropertyDrawer(typeof(Wrapper))]
//     public class WrapperDrawer : PropertyDrawer
//     {
//         public override VisualElement CreatePropertyGUI(SerializedProperty property)
//         {
//             var p = property.GetValue();
//             var jobj = JObject.FromObject(p);
//             var jfie = jobj["sd"];
//             jfie.Value<object>();
//             var sp = new SerializedProperty(); SerializedObject
//             var wr = __makeref(p);
//         }

//         public static VisualElement CreateField(string label, JToken field)
//         {
//             var value = field.Value<object>();
//             switch (Type.GetTypeCode(value?.GetType()))
//             {
//                 case TypeCode.Int32:
//                 case TypeCode.UInt32:
//                     {
//                         var ele = new IntegerField(label);
//                         ele.value = Convert.ToInt32(field.GetValueDirect(inst));
//                         ele.RegisterValueChangedCallback(e =>
//                         {
//                             if (e.newValue != Convert.ToInt32(field.GetValueDirect(inst)))
//                             {
//                                 field.SetValueDirect(inst);
//                             }
//                         })
//                         return ele;
//                     }
//             }
//         }
//     }
// }
