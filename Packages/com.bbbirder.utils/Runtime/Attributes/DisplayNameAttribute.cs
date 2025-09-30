using System;
using System.Diagnostics;
using UnityEngine;

namespace BBBirder
{
    [Conditional("UNITY_EDITOR")]
    public class DisplayNameAttribute : Attribute
    {
        public readonly string Name;
        public DisplayNameAttribute(string name)
        {
            Name = name;
        }
    }
}
