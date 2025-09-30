using UnityEngine;

namespace BBBirder
{
    /// <summary>
    /// Trigger callback when changed from Inspector
    /// </summary>
    public class ReadOnlyAttribute : PropertyAttribute
    {
        public ReadOnlyAttribute()
        {
        }
    }
}
