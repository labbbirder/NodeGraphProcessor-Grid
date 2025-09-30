using System.Diagnostics;
using UnityEngine;

namespace BBBirder
{
    /// <summary>
    /// Trigger callback when changed from Inspector
    /// </summary>
    /// <remarks>
    /// Valid callback methods:
    /// <![CDATA[
    /// // without parameter
    /// void OnValueChanged() { ... }
    /// // one parameter
    /// void OnNicknameChanged(string newValue) { ... }
    /// // two parameter
    /// void OnLevelChanged(int newValue, int prevValue) { ... }
    /// // static
    /// static void OnValueChanged() { ... }
    /// static void OnValueChanged(string newValue) { ... }
    /// static void OnValueChanged(int newValue, int prevValue) { ... }
    /// ]]>
    /// </remarks>
    [Conditional("UNITY_EDITOR")]
    public class OnChangeAttribute : PropertyAttribute
    {
        public readonly string CallbackName;
        public OnChangeAttribute(string callbackName)
        {
            this.CallbackName = callbackName;
        }
    }
}
