using System;
using System.Diagnostics;
using UnityEngine;
namespace BBBirder
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    public class PolymorphicAttribute : PropertyAttribute
    {
        public readonly bool AllowNull;

        public PolymorphicAttribute(bool allowNull = true)
        {
            AllowNull = allowNull;
        }
    }
}
